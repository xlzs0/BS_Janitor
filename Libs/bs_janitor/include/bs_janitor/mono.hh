#pragma once

using MonoString = struct _MonoString;
#ifdef _WIN32
using mono_unichar2 = wchar_t;
#else
using mono_unichar2 = std::uint16_t;
#endif

using mt_mono_string_to_utf8 = char* (*)(MonoString*);
using mt_mono_string_chars = mono_unichar2* (*)(MonoString*);
using mt_mono_free = void (*)(void*);
using mt_mono_add_internal_call = void (*)(const char*, const void*);

extern mt_mono_string_to_utf8 mono_string_to_utf8;
extern mt_mono_string_chars mono_string_chars;
extern mt_mono_free mono_free;
extern mt_mono_add_internal_call mono_add_internal_call;
