#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Weverything"
#include <glaze/glaze.hpp>
#pragma clang diagnostic pop

#include <bs_janitor/bs_janitor.h>

using namespace bs_janitor;

enum class beatmap_version { v2 = '2', v3 };

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

static std::optional<std::string_view> load_file_to_view(const wchar_t* path, std::string& buf, size_t& capacity) noexcept {
    std::FILE* fp = nullptr;
    _wfopen_s(&fp, path, L"rb");

    if (!fp)
        return std::nullopt;

    size_t len{};

    if (std::fseek(fp, 0, SEEK_END) < 0 || (len = std::ftell(fp), len == static_cast<size_t>(-1))) {
        std::fclose(fp);
        return std::nullopt;
    }

    if (capacity < len) {
        buf.resize(static_cast<size_t>(static_cast<float>(len + 15) / 16) * 16);
        capacity = buf.size();
    }

    std::rewind(fp);

    const auto bytes_read = std::fread(buf.data(), 1, len, fp);
    if (std::fclose(fp) != 0 || bytes_read != len)
        return std::nullopt;

    return std::string_view(buf.begin(), buf.begin() + len);
}

namespace v2 {
    struct custom_data {
        bool _fake;
    };

    struct note {
        uint8_t _type;
        custom_data _customData;
    };

    struct obstacle {};

    struct beatmap {
        std::vector<note> _notes;
        std::vector<obstacle> _obstacles;
    };

    static std::optional<bs_janitor::output> parse(std::string_view& json) {
        bs_janitor::output output{};

        beatmap beatmap{};
        if (glz::read<opts>(beatmap, json))
            return std::nullopt;

        output.cuttable_notes = beatmap._notes.size();

        for (const auto& note : beatmap._notes) {
            if (note._customData._fake) [[unlikely]] {
                output.cuttable_notes--;
                continue;
            }

            output.bombs += note._type == 3;
        }

        output.cuttable_notes -= output.bombs;
        output.cuttable_objects = output.cuttable_notes;
        output.obstacles = beatmap._obstacles.size();

        return output;
    }
} // namespace v2

namespace v3 {
    struct note {};
    struct bomb {};
    struct obstacle {};

    struct burst_slider {
        uint64_t sc;
    };

    struct beatmap {
        std::vector<note> colorNotes;
        std::vector<bomb> bombNotes;
        std::vector<obstacle> obstacles;
        std::vector<burst_slider> burstSliders;
    };

    static std::optional<bs_janitor::output> parse(std::string_view& json) {
        bs_janitor::output output{};

        beatmap beatmap{};
        if (glz::read<opts>(beatmap, json))
            return std::nullopt;

        output.cuttable_notes = beatmap.colorNotes.size();
        output.cuttable_objects = output.cuttable_notes;
        output.bombs = beatmap.bombNotes.size();
        output.obstacles = beatmap.obstacles.size();

        auto total = 0;
        for (const auto& slider : beatmap.burstSliders)
            total += slider.sc;

        output.cuttable_objects += total;

        return output;
    }
} // namespace v3

namespace static_objects {
    static std::string buffer{};
    static size_t buffer_capacity{};
} // namespace static_objects

static bool parse_basic_data_impl(std::string_view json, bs_janitor::output* output) {
    if (!output)
        return false;

    try {
        const auto version = detect_version(json);
        if (!version.has_value())
            return false;

        auto out = version.value() == beatmap_version::v2 ? v2::parse(json) : v3::parse(json);
        if (!out.has_value())
            return false;

        *output = out.value();

        return true;
    } catch (...) {
        return false;
    }
}

BS_JANITOR_EXPORT bool BS_JANITOR_CC parse_basic_data(const char* json, bs_janitor::output* output) {
    return parse_basic_data_impl(json, output);
}

BS_JANITOR_EXPORT bool BS_JANITOR_CC parse_basic_data_from_file(const wchar_t* path, bs_janitor::output* output) {
    const auto json = load_file_to_view(path, static_objects::buffer, static_objects::buffer_capacity);
    if (!json.has_value())
        return false;

    return parse_basic_data_impl(json.value(), output);
}
