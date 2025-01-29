/*
 *  Copyright (C) 2025 xlzs0
 *
 *  This file is part of BS_Janitor.
 *
 *  BS_Janitor is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published
 *  by the Free Software Foundation, either version 3 of the License,
 *  or (at your option) any later version.
 *
 *  BS_Janitor is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with BS_Janitor.  If not, see <https://www.gnu.org/licenses/>.
 */

#include "include/memory.hh"

#define STB_IMAGE_IMPLEMENTATION
#define STB_IMAGE_RESIZE2_IMPLEMENTATION

#define STBI_MALLOC bs_janitor::mem_alloc
#define STBI_REALLOC bs_janitor::mem_realloc
#define STBI_FREE bs_janitor::mem_free
#define STBIR_MALLOC(size, ...) bs_janitor::mem_alloc(size)
#define STBIR_FREE(ptr, ...) bs_janitor::mem_free(ptr)

#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Weverything"
#include "include/stb/stb_image.h"
#include "include/stb/stb_image_resize2.h"
#pragma clang diagnostic pop

#include <bs_janitor/bs_janitor.h>

namespace bs_janitor {
    void convert_to_rgba(uint8_t** data, uint64_t width, uint64_t height, uint64_t& channels) {
        constexpr auto target_channels = 4u;
        if (!data || !*data || channels < 1 || channels > 3)
            return;

        const auto num_pixels = width * height;
        auto output = mem_alloc<uint8_t*>(num_pixels * target_channels);
        if (!output)
            return;

        auto* __restrict in = *data;
        auto* __restrict out = output;

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
                const auto color = in[0];
                const auto alpha = in[1];
                in += 2;
                out[0] = color;
                out[1] = color;
                out[2] = color;
                out[3] = alpha;
                out += 4;
            }
            break;
        case 3:
            for (auto i = 0ull; i < num_pixels; ++i) {
                out[0] = *in++;
                out[1] = *in++;
                out[2] = *in++;
                out[3] = 0xFF;
                out += 4;
            }
            break;
        }

        mem_free(*data);

        *data = output;
        channels = target_channels;
    }

    uint8_t* load_from_file(FILE* fp, uint64_t& out_width, uint64_t& out_height, uint64_t& out_channels) {
        int32_t x{};
        int32_t y{};
        int32_t channels{};

        stbi_set_flip_vertically_on_load(1);
        auto data = stbi_load_from_file(fp, &x, &y, &channels, 0);

        out_width = static_cast<uint64_t>(x);
        out_height = static_cast<uint64_t>(y);
        out_channels = static_cast<uint64_t>(channels);

        return data;
    }

    BS_JANITOR_EXPORT uint8_t* BS_JANITOR_CC load_image(
#ifdef _WIN32
        const wchar_t* path,
#else
        const char* path,
#endif
        uint64_t* channels,
        uint64_t* width,
        uint64_t* height,
        uint64_t max_size) {
        if (!channels || !width || !height)
            return nullptr;

        FILE* fp = nullptr;
#ifdef _WIN32
        _wfopen_s(&fp, path, L"rb");
#else
        fp = fopen(path, "rb");
#endif
        if (!fp)
            return nullptr;

        uint64_t x{};
        uint64_t y{};
        uint64_t channels_in_file{};

        auto data = load_from_file(fp, x, y, channels_in_file);
        fclose(fp);

        if (!data)
            return nullptr;

        if (max_size > 0 && (x > max_size || y > max_size)) {
            auto output =
                stbir_resize_uint8_srgb(data, x, y, 0, nullptr, max_size, max_size, 0, static_cast<stbir_pixel_layout>(channels_in_file));

            if (output) {
                mem_free(data);
                data = output;
                x = max_size;
                y = max_size;
            }
        }

        if (channels_in_file < 4)
            convert_to_rgba(&data, x, y, channels_in_file);

        *channels = channels_in_file;
        *width = x;
        *height = y;

        return data;
    }
} // namespace bs_janitor
