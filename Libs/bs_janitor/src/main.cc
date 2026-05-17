/* TODO: replace raw mono with bscpp submodule */

#include <Windows.h>
#include <bs_janitor/bs_janitor.hh>
#include <bs_janitor/mono.hh>

mt_mono_string_to_utf8 mono_string_to_utf8;
mt_mono_string_chars mono_string_chars;
mt_mono_free mono_free;
mt_mono_add_internal_call mono_add_internal_call;

[[nodiscard]] static bool get_funcs(HMODULE mono_module) {
#define GET_MONO_FUNC(fn)                                                    \
    if (!(fn = reinterpret_cast<mt_##fn>(GetProcAddress(mono_module, #fn)))) \
        return false;

#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Wcast-function-type-mismatch"
#pragma clang diagnostic ignored "-Wcast-function-type-strict"
    GET_MONO_FUNC(mono_string_to_utf8)
    GET_MONO_FUNC(mono_string_chars)
    GET_MONO_FUNC(mono_free)
    GET_MONO_FUNC(mono_add_internal_call)
#pragma clang diagnostic pop

    return true;
#undef GET_MONO_FUNC
}

BS_JANITOR_EXPORT bs_janitor::init_error init() {
    const auto mono_module = GetModuleHandleA("mono-2.0-bdwgc.dll");
    if (!mono_module)
        return bs_janitor::init_error::MONO_MODULE_NOT_FOUND;

    if (!get_funcs(mono_module))
        return bs_janitor::init_error::MONO_FUNC_NOT_FOUND;

    mono_add_internal_call("BS_Janitor.Utils.BasicBeatmapDataParser::ParseBasicData_Injected",
                           reinterpret_cast<void*>(bs_janitor::parse_basic_data));
    mono_add_internal_call("BS_Janitor.Utils.BasicBeatmapDataParser::ParseBasicDataFromFile_Injected",
                           reinterpret_cast<void*>(bs_janitor::parse_basic_data_from_file));
    mono_add_internal_call("BS_Janitor.Utils.ImageLoader::LoadImage_Injected", reinterpret_cast<void*>(bs_janitor::load_image));
    mono_add_internal_call("BS_Janitor.Utils.ImageLoader::Free_Injected", reinterpret_cast<void*>(bs_janitor::mem_free));

    return bs_janitor::init_error::SUCCESS;
}
