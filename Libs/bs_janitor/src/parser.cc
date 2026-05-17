#include <bs_janitor/bs_janitor.hh>
#include <bs_janitor/mono.hh>
#include <glaze/glaze.hpp>

enum class beatmap_version { V2 = '2', V3 };

constexpr auto opts = glz::opts{ .error_on_unknown_keys = false, .error_on_missing_keys = false };

[[nodiscard]] static std::optional<beatmap_version> detect_version(const std::string_view& json) {
    constexpr auto version_str = "version\"";
    constexpr auto version_len = sizeof("version\"");

    const auto key_idx = json.find(version_str);
    if (key_idx == std::string_view::npos)
        return std::nullopt;

    const auto val_idx = json.find("\"", key_idx + version_len);
    if (val_idx == std::string_view::npos)
        return std::nullopt;

    const auto c = json[val_idx + 1];
    if (c == '2' || c == '3')
        return static_cast<beatmap_version>(c);

    return std::nullopt;
}

[[nodiscard]] static std::optional<std::string_view>
load_file_to_view(const wchar_t* path, std::string& buf, std::size_t& capacity) noexcept {
    std::FILE* fp{};
    _wfopen_s(&fp, path, L"rb");

    if (!fp)
        return std::nullopt;

    if (std::fseek(fp, 0, SEEK_END) < 0) {
        std::fclose(fp);
        return std::nullopt;
    }

    const auto len = static_cast<std::size_t>(std::ftell(fp));
    if (len == static_cast<std::size_t>(-1)) {
        std::fclose(fp);
        return std::nullopt;
    }

    if (capacity < len) {
        buf.resize((len + 15) & ~15ull);
        capacity = buf.size();
    }

    std::rewind(fp);

    const auto bytes_read = std::fread(buf.data(), 1, len, fp);
    if (std::fclose(fp) != 0 || bytes_read != len)
        return std::nullopt;

    return std::string_view(buf.begin(), buf.begin() + static_cast<std::ptrdiff_t>(len));
}

struct array_counter {
    std::int32_t count{};
    static constexpr auto glaze_reflect = false;
};

struct note_accumulator {
    std::int32_t note_count{};
    std::int32_t bomb_count{};
    static constexpr auto glaze_reflect = false;
};

struct burst_slider_accumulator {
    std::int32_t count{};
    static constexpr auto glaze_reflect = false;
};

namespace glz {
    template <auto Opts>
    static void parse_array_with_callback(auto&& ctx, auto&& it, auto&& end, auto&& callback) {
        if (match<'['>(ctx, it))
            return;

        skip_ws<Opts>(ctx, it, end);

        if (*it == ']') {
            ++it;
            return;
        }

        while (true) {
            callback(ctx, it, end);

            if (bool(ctx.error)) [[unlikely]]
                return;

            skip_ws<Opts>(ctx, it, end);

            if (*it == ',') {
                ++it;
                skip_ws<Opts>(ctx, it, end);
            } else
                break;
        }

        match<']'>(ctx, it);
    }

    template <>
    struct from<JSON, array_counter> {
        template <auto Opts>
        static void op(array_counter& value, is_context auto&& ctx, auto&& it, auto&& end) {
            std::int32_t n{};

            parse_array_with_callback<Opts>(ctx, it, end, [&](auto&& ctx, auto&& it, auto&& end) {
                skip_value<JSON>::op<Opts>(ctx, it, end);
                n++;
            });

            value.count = n;
        }
    };

    template <>
    struct from<JSON, note_accumulator> {
        template <auto Opts>
        static void op(note_accumulator& value, is_context auto&& ctx, auto&& it, auto&& end) {
            struct note {
                std::uint8_t _type;
                struct custom_data {
                    bool _fake;
                } _customData;
            } tmp;

            parse_array_with_callback<Opts>(ctx, it, end, [&](auto&& ctx, auto&& it, auto&& end) {
                tmp = {};

                parse<JSON>::op<Opts>(tmp, ctx, it, end);

                if (tmp._customData._fake)
                    return;

                if (tmp._type == 3)
                    value.bomb_count++;
                else
                    value.note_count++;
            });
        }
    };

    template <>
    struct from<JSON, burst_slider_accumulator> {
        template <auto Opts>
        static void op(burst_slider_accumulator& value, is_context auto&& ctx, auto&& it, auto&& end) {
            struct burst_slider {
                std::uint64_t sc;
            } tmp;

            parse_array_with_callback<Opts>(ctx, it, end, [&](auto&& ctx, auto&& it, auto&& end) {
                tmp = {};

                parse<JSON>::op<Opts>(tmp, ctx, it, end);

                value.count += std::max(tmp.sc, 1ull) - 1;
            });
        }
    };
} // namespace glz

namespace v2 {
    struct beatmap {
        note_accumulator _notes;
        array_counter _obstacles;
    };

    static std::optional<bs_janitor::output> parse(std::string_view& json) {
        bs_janitor::output output{};

        beatmap beatmap{};
        if (glz::read<opts>(beatmap, json))
            return std::nullopt;

        output.cuttable_notes = beatmap._notes.note_count;
        output.cuttable_objects = output.cuttable_notes;
        output.bombs = beatmap._notes.bomb_count;
        output.obstacles = beatmap._obstacles.count;

        return output;
    }
} // namespace v2

namespace v3 {
    struct beatmap {
        array_counter colorNotes;
        array_counter bombNotes;
        array_counter obstacles;
        burst_slider_accumulator burstSliders;
    };

    static std::optional<bs_janitor::output> parse(std::string_view& json) {
        bs_janitor::output output{};

        beatmap beatmap{};
        if (glz::read<opts>(beatmap, json))
            return std::nullopt;

        output.cuttable_notes = beatmap.colorNotes.count;
        output.cuttable_objects = output.cuttable_notes + beatmap.burstSliders.count;
        output.bombs = beatmap.bombNotes.count;
        output.obstacles = beatmap.obstacles.count;

        return output;
    }
} // namespace v3

namespace static_objects {
    static thread_local std::string buffer{};
    static thread_local std::size_t buffer_capacity{};
} // namespace static_objects

static bool parse_basic_data_impl(std::string_view json, bs_janitor::output* output) {
    if (!output)
        return false;

    const auto version = detect_version(json);
    if (!version.has_value())
        return false;

    const auto out = version.value() == beatmap_version::V2 ? v2::parse(json) : v3::parse(json);
    if (!out.has_value())
        return false;

    *output = out.value();

    return true;
}

bool bs_janitor::parse_basic_data(MonoString* ptr, bs_janitor::output* output) {
    const std::unique_ptr<char, decltype(mono_free)> utf8_str{ mono_string_to_utf8(ptr), mono_free };
    if (!utf8_str)
        return false;

    return parse_basic_data_impl(utf8_str.get(), output);
}

bool bs_janitor::parse_basic_data_from_file(MonoString* ptr, bs_janitor::output* output) {
    const auto wide_path = mono_string_chars(ptr);
    if (!wide_path)
        return false;

    const auto json = load_file_to_view(wide_path, static_objects::buffer, static_objects::buffer_capacity);
    if (!json.has_value())
        return false;

    return parse_basic_data_impl(json.value(), output);
}
