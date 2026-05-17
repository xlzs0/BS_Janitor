#define STB_IMAGE_IMPLEMENTATION
#define STB_IMAGE_RESIZE2_IMPLEMENTATION

#define STBI_ONLY_PNG
#define STBI_ONLY_JPEG
#define STBIDEF [[gnu::flatten]] extern

#include <bs_janitor/bs_janitor.hh>
#include <bs_janitor/mono.hh>
#include <cstdio>
#include <memory>
#include <optional>

#define ASSUME_ALIGNED(x) std::assume_aligned<32>(x)

#define STBI_MALLOC(sz) ASSUME_ALIGNED(_aligned_malloc((sz), 32))
#define STBI_REALLOC(p, newsz) ASSUME_ALIGNED(_aligned_realloc((p), (newsz), 32))
#define STBI_FREE _aligned_free

#define STBIR_MALLOC(size, user_data) STBI_MALLOC(size)
#define STBIR_FREE(ptr, user_data) STBI_FREE(ptr)

#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Weverything"
#include "stb/stb_image.h"
#include "stb/stb_image_resize2.h"
#pragma clang diagnostic pop

using img_data_t = std::unique_ptr<std::uint8_t, decltype(&STBI_FREE)>;

static std::optional<img_data_t> expand_to_rgba(const img_data_t& data, std::int32_t width, std::int32_t height, std::int32_t& channels) {
    constexpr auto target_channels = 4u;
    if (!data || channels < 1 || channels > 2)
        return std::nullopt;

    const auto num_pixels = static_cast<std::size_t>(width) * static_cast<std::size_t>(height);
    auto output = static_cast<std::uint8_t*>(STBI_MALLOC(num_pixels * target_channels));
    if (!output)
        return std::nullopt;

    const std::uint8_t* __restrict in = ASSUME_ALIGNED(data.get());
    std::uint8_t* __restrict out = ASSUME_ALIGNED(output);

    switch (channels) {
    case 1:
        for (auto i = 0ull; i < num_pixels; i++) {
            const auto color = *in++;
            out[0] = color;
            out[1] = color;
            out[2] = color;
            out[3] = 0xFF;
            out += 4;
        }
        break;
    case 2:
        for (auto i = 0ull; i < num_pixels; i++) {
            const auto color = *in++;
            const auto alpha = *in++;
            out[0] = color;
            out[1] = color;
            out[2] = color;
            out[3] = alpha;
            out += 4;
        }
        break;
    }

    channels = target_channels;

    return img_data_t{ output, STBI_FREE };
}

static std::optional<img_data_t>
resize_image(const img_data_t& data, std::int32_t& width, std::int32_t& height, std::int32_t channels, std::int32_t max_size) {
    if (max_size <= 0 || (width <= max_size && height <= max_size))
        return std::nullopt;

    auto* output = static_cast<std::uint8_t*>(stbir_resize(data.get(),
                                                           width,
                                                           height,
                                                           0,
                                                           nullptr,
                                                           static_cast<std::int32_t>(max_size),
                                                           static_cast<std::int32_t>(max_size),
                                                           0,
                                                           static_cast<stbir_pixel_layout>(channels),
                                                           STBIR_TYPE_UINT8_SRGB,
                                                           STBIR_EDGE_CLAMP,
                                                           STBIR_FILTER_BOX));
    if (!output)
        return std::nullopt;

    width = max_size;
    height = max_size;

    return img_data_t{ output, STBI_FREE };
}

std::uint8_t* bs_janitor::load_image(
    MonoString* ptr, std::int32_t* out_width, std::int32_t* out_height, std::int32_t* out_channels, std::int32_t max_size) {
    const auto path = mono_string_chars(ptr);
    if (!path || !out_channels || !out_width || !out_height)
        return nullptr;

    std::FILE* fp{};
    _wfopen_s(&fp, path, L"rb");

    if (!fp)
        return nullptr;

    std::int32_t width{}, height{}, channels{};
    stbi_set_flip_vertically_on_load(1);
    img_data_t data{ stbi_load_from_file(fp, &width, &height, &channels, 0), STBI_FREE };
    std::fclose(fp);

    if (!data)
        return nullptr;

    if (auto resized = resize_image(data, width, height, channels, max_size))
        data = std::move(*resized);

    if (auto rgba = expand_to_rgba(data, width, height, channels))
        data = std::move(*rgba);

    *out_channels = channels;
    *out_width = width;
    *out_height = height;

    return data.release();
}

void bs_janitor::mem_free(void* ptr) {
    if (ptr)
        STBI_FREE(ptr);
}
