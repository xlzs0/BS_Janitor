#pragma once

#ifndef BS_JANITOR_EXPORT
#define BS_JANITOR_EXPORT extern "C" __declspec(dllexport)
#endif

#include "mono.hh"
#include <cstdint>

namespace bs_janitor {
    enum class init_error : std::int32_t { SUCCESS = 0, MONO_MODULE_NOT_FOUND, MONO_FUNC_NOT_FOUND };

    struct output {
        std::int32_t cuttable_notes;
        std::int32_t cuttable_objects;
        std::int32_t obstacles;
        std::int32_t bombs;
    };

    bool parse_basic_data(MonoString* ptr, output* output);
    bool parse_basic_data_from_file(MonoString* ptr, output* output);
    std::uint8_t*
    load_image(MonoString* ptr, std::int32_t* out_width, std::int32_t* out_height, std::int32_t* out_channels, std::int32_t max_size);
    void mem_free(void* ptr);
} // namespace bs_janitor

BS_JANITOR_EXPORT bs_janitor::init_error init();
