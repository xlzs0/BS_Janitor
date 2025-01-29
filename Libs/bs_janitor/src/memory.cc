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

namespace bs_janitor {
    std::mutex mutex{};
    std::unordered_map<void*, size_t> allocs{};

    std::mutex& get_mem_mutex() {
        return mutex;
    }

    std::unordered_map<void*, size_t>& get_mem_allocs() {
        return allocs;
    }

    BS_JANITOR_EXPORT void BS_JANITOR_CC mem_free(void* ptr) {
        if (!ptr)
            return;

        std::lock_guard<std::mutex> lock(get_mem_mutex());
        auto& allocs = get_mem_allocs();
        auto it = allocs.find(ptr);
        if (it == allocs.end())
            return;

        free(ptr);
        allocs.erase(it);
    }

    BS_JANITOR_EXPORT void BS_JANITOR_CC mem_free_everything() {
        std::lock_guard<std::mutex> lock(get_mem_mutex());
        auto& allocs = get_mem_allocs();
        for (auto& [ptr, info] : allocs)
            free(ptr);

        allocs.clear();
    }
} // namespace bs_janitor