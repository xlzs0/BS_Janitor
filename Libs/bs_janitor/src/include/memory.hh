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

#ifndef BS_JANITOR_MEMORY_HH
#define BS_JANITOR_MEMORY_HH

#include <bs_janitor/bs_janitor.h>
#include <cstddef>
#include <mutex>
#include <stdlib.h>
#include <unordered_map>

namespace bs_janitor {
    std::mutex& get_mem_mutex();
    std::unordered_map<void*, size_t>& get_mem_allocs();

    template <typename T = void*>
    inline T mem_alloc(size_t size) {
        const auto ptr = malloc(size);
        if (!ptr)
            return nullptr;

        std::lock_guard<std::mutex> lock(get_mem_mutex());
        get_mem_allocs()[ptr] = size;
        return static_cast<T>(ptr);
    }

    template <typename T = void*>
    inline T mem_realloc(void* ptr, size_t new_size) {
        if (!ptr)
            return mem_alloc(new_size);

        if (new_size == 0) {
            mem_free(ptr);
            return nullptr;
        }

        std::lock_guard<std::mutex> lock(get_mem_mutex());
        auto& allocs = get_mem_allocs();
        auto it = allocs.find(ptr);
        if (it == allocs.end())
            return nullptr;

        const auto new_ptr = realloc(ptr, new_size);
        if (!new_ptr)
            return nullptr;

        if (new_ptr != ptr) {
            allocs.erase(it);
            allocs[new_ptr] = new_size;
        } else
            it->second = new_size;

        return static_cast<T>(new_ptr);
    }
} // namespace bs_janitor

#endif