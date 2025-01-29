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

#ifndef BS_JANITOR_PARSER_HH
#define BS_JANITOR_PARSER_HH

namespace bs_janitor {
    namespace constants {
        inline constexpr const char* default_file_extension = ".dat";
        inline constexpr const char* info_file_name = "Info.dat";

        namespace v2 {
            inline constexpr const char* time = "_time";
            inline constexpr const char* line_index = "_lineIndex";
            inline constexpr const char* line_layer = "_lineLayer";
            inline constexpr const char* wall_line_layer = "_type";
            inline constexpr const char* type = "_type";
            inline constexpr const char* direction = "_cutDirection";
            inline constexpr const char* duration = "_duration";
            inline constexpr const char* width = "_width";

            inline constexpr const char* notes = "_notes";
            inline constexpr const char* walls = "_obstacles";

            inline constexpr const char* beatmap_sets = "_difficultyBeatmapSets";
            inline constexpr const char* difficulty_beatmaps = "_difficultyBeatmaps";
            inline constexpr const char* characteristic_name = "_beatmapCharacteristicName";
            inline constexpr const char* difficulty_name = "_difficulty";
            inline constexpr const char* beatmap_file_name = "_beatmapFilename";
            inline constexpr const char* bpm = "_beatsPerMinute";

            inline constexpr const char* version = "_version";

            namespace noodle {
                inline constexpr const char* custom_data = "_customData";
                inline constexpr const char* fake_note = "_fake";
            } // namespace noodle
        } // namespace v2

        namespace v3 {
            inline constexpr const char* time = "b";
            inline constexpr const char* line_index = "x";
            inline constexpr const char* line_layer = "y";
            inline constexpr const char* type = "c";
            inline constexpr const char* direction = "d";
            inline constexpr const char* duration = "d";
            inline constexpr const char* width = "w";
            inline constexpr const char* height = "h";
            inline constexpr const char* angle = "a";
            inline constexpr const char* cuttable_slices = "sc";

            inline constexpr const char* notes = "colorNotes";
            inline constexpr const char* bombs = "bombNotes";
            inline constexpr const char* walls = "obstacles";
            inline constexpr const char* burst_sliders = "burstSliders";

            inline constexpr const char* version = "version";
        } // namespace v3

        namespace v4 {
            inline constexpr const char* time = "b";
            inline constexpr const char* line_index = "x";
            inline constexpr const char* line_layer = "y";
            inline constexpr const char* type = "c";
            inline constexpr const char* direction = "d";
            inline constexpr const char* duration = "d";
            inline constexpr const char* width = "w";
            inline constexpr const char* height = "h";
            inline constexpr const char* angle = "a";

            inline constexpr const char* data_index = "i";

            inline constexpr const char* notes = "colorNotes";
            inline constexpr const char* bombs = "bombNotes";
            inline constexpr const char* walls = "obstacles";

            inline constexpr const char* notes_data = "colorNotesData";
            inline constexpr const char* bombs_data = "bombNotesData";
            inline constexpr const char* walls_data = "obstaclesData";

            inline constexpr const char* difficulty_beatmaps = "difficultyBeatmaps";
            inline constexpr const char* characteristic_name = "characteristic";
            inline constexpr const char* difficulty_name = "difficulty";
            inline constexpr const char* beatmap_file_name = "beatmapDataFilename";
            inline constexpr const char* audio = "audio";
            inline constexpr const char* bpm = "bpm";
        } // namespace v4
    } // namespace constants

    enum class beatmap_version { v2, v3, v4 };
} // namespace bs_janitor

#endif
