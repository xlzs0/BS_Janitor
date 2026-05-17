#ifndef BS_JANITOR_EXPORTS_H
#define BS_JANITOR_EXPORTS_H

#include <cstdint>

#ifndef BS_JANITOR_CC
#if defined(_MSC_VER)
#define BS_JANITOR_CC __cdecl
#elif defined(__clang__) || defined(__GNUC__)
#define BS_JANITOR_CC __attribute__((cdecl))
#else
#define BS_JANITOR_CC
#endif
#endif

#ifndef BS_JANITOR_EXPORT
#define BS_JANITOR_EXPORT extern "C" __declspec(dllexport)
#endif

namespace bs_janitor {
    struct output {
        uint64_t cuttable_notes;
        uint64_t cuttable_objects;
        uint64_t obstacles;
        uint64_t bombs;
    };

    BS_JANITOR_EXPORT bool BS_JANITOR_CC parse_basic_data(const char* json, output* output);
    BS_JANITOR_EXPORT bool BS_JANITOR_CC parse_basic_data_from_file(const wchar_t* path, output* output);

    BS_JANITOR_EXPORT uint8_t* BS_JANITOR_CC
    load_image(const wchar_t* path, uint64_t* channels, uint64_t* width, uint64_t* height, uint64_t max_size);

    BS_JANITOR_EXPORT void BS_JANITOR_CC mem_free(void* ptr);
} // namespace bs_janitor

#endif
