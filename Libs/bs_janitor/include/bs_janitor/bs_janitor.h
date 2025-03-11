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

#ifndef BS_JANITOR_EXPORTS_H
#define BS_JANITOR_EXPORTS_H

#include <cstdint>

#ifndef BS_JANITOR_CC
#if defined(_WIN32) || defined(_MSC_VER)
#define BS_JANITOR_CC __cdecl
#elif defined(__clang__) || defined(__GNUC__)
#define BS_JANITOR_CC __attribute__((cdecl))
#else
#define BS_JANITOR_CC
#endif
#endif

#ifndef BS_JANITOR_EXPORT
#ifdef _WIN32
#define BS_JANITOR_EXPORT extern "C" __declspec(dllexport)
#else
#define BS_JANITOR_EXPORT
#endif
#endif

#if !defined(__cplusplus) && !defined(bool)
#include <stdbool.h>
#endif

#ifdef __cplusplus
namespace bs_janitor {
#endif
    typedef struct output {
        uint64_t cuttable_notes;
        uint64_t cuttable_objects;
        uint64_t obstacles;
        uint64_t bombs;
    } output;

    BS_JANITOR_EXPORT bool BS_JANITOR_CC parse_basic_data(const char* json, output* output);

    BS_JANITOR_EXPORT uint8_t* BS_JANITOR_CC load_image(
#ifdef _WIN32
        const wchar_t* path,
#else
    const char* path,
#endif
        uint64_t* channels,
        uint64_t* width,
        uint64_t* height,
        uint64_t max_size);

    BS_JANITOR_EXPORT void BS_JANITOR_CC mem_free(void* ptr);
#ifdef __cplusplus
} // namespace bs_janitor
#endif

#endif
