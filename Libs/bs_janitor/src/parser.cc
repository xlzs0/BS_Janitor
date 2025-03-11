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

#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Weverything"
#include <simdjson.h>
#pragma clang diagnostic pop

#include "include/parser.hh"
#include <bs_janitor/bs_janitor.h>

namespace bs_janitor {
    [[nodiscard]] static std::optional<beatmap_version> detect_version(simdjson::dom::element& beatmap) {
        if (auto version_element = beatmap[constants::v3::version]; !version_element.error() && version_element.is_string()) {
            auto version_string = version_element.get_string().value();
            if (version_string.starts_with('4'))
                return beatmap_version::v4;

            if (version_string.starts_with('3'))
                return beatmap_version::v3;
        }

        if (!beatmap[constants::v2::version].error())
            return beatmap_version::v2;

        if (!beatmap[constants::v4::notes_data].error())
            return beatmap_version::v4;

        if (!beatmap[constants::v3::notes].error())
            return beatmap_version::v3;

        if (!beatmap[constants::v2::notes].error())
            return beatmap_version::v2;

        return std::nullopt;
    }

    namespace static_objects {
        static std::mutex mutex{};
        static simdjson::dom::parser parser{};
    } // namespace static_objects

    BS_JANITOR_EXPORT bool BS_JANITOR_CC parse_basic_data(const char* json, bs_janitor::output* output) {
        if (!output)
            return false;

        try {
            std::lock_guard<std::mutex> lock(static_objects::mutex);

            simdjson::dom::element element = static_objects::parser.parse(json, std::strlen(json));

            auto version = detect_version(element);
            if (!version.has_value())
                return false;

            uint64_t bombs{};
            uint64_t cuttable_notes{};
            uint64_t cuttable_objects{};
            uint64_t obstacles{};

            switch (version.value()) {
            case beatmap_version::v2: {
                if (auto notes = element[constants::v2::notes]; !notes.error()) {
                    auto notes_array = notes.get_array().take_value();
                    cuttable_notes = notes_array.size();

                    for (auto item : notes_array) {
                        if (auto custom_data = item[constants::v2::noodle::custom_data]; !custom_data.error()) [[unlikely]] {
                            if (bool fake{}; !custom_data[constants::v2::noodle::fake_note].get(fake) && fake) [[unlikely]] {
                                cuttable_notes--;
                                continue;
                            }
                        }

                        int64_t type{};
                        std::ignore = item[constants::v2::type].get(type);
                        bombs += type == 3;
                    }

                    cuttable_notes -= bombs;
                    cuttable_objects = cuttable_notes;
                } else
                    return false;

                if (auto walls = element[constants::v2::walls]; !walls.error()) {
                    auto walls_array = walls.get_array().take_value();
                    obstacles = walls_array.size();
                } else
                    return false;
            } break;
            case beatmap_version::v3: {
                if (auto bombs_ = element[constants::v3::bombs]; !bombs_.error()) {
                    auto bombs_array = bombs_.get_array().take_value();
                    bombs = bombs_array.size();
                } else
                    return false;

                if (auto notes = element[constants::v3::notes]; !notes.error()) {
                    auto notes_array = notes.get_array().take_value();
                    cuttable_notes = notes_array.size();
                    cuttable_objects = cuttable_notes;
                } else
                    return false;

                if (auto burst_sliders = element[constants::v3::burst_sliders]; !burst_sliders.error()) {
                    auto burst_sliders_array = burst_sliders.get_array().take_value();

                    for (auto item : burst_sliders_array) {
                        uint64_t cuttable_slices{};
                        std::ignore = item[constants::v3::cuttable_slices].get(cuttable_slices);
                        cuttable_objects += cuttable_slices;
                    }
                } else
                    return false;

                if (auto walls = element[constants::v3::walls]; !walls.error()) {
                    auto walls_array = walls.get_array().take_value();
                    obstacles = walls_array.size();
                } else
                    return false;
            } break;
            default:
                return false;
            }

            output->bombs = bombs;
            output->cuttable_notes = cuttable_notes;
            output->cuttable_objects = cuttable_objects;
            output->obstacles = obstacles;

            return true;
        } catch (...) {
            return false;
        }
    }
} // namespace bs_janitor
