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

using HarmonyLib;
using System.Collections.Generic;
using System;
using System.Buffers;
using UnityEngine;

namespace BS_Janitor.HarmonyPatches
{
    [HarmonyPatch(typeof(LevelFilter), nameof(LevelFilter.FilterLevelByText))]
    internal class LevelFilterPatch
    {
        static int GetTotalLength(BeatmapLevel level)
        {
            var length = 0;
            length += (level.songName?.Length ?? 0) + 1;
            length += (level.songSubName?.Length ?? 0) + 1;
            length += (level.songAuthorName?.Length ?? 0) + 1;

            foreach (var mapper in level.allMappers)
            {
                length += (mapper?.Length ?? 0) + 1;
            }

            return length;
        }

        static void CopyLower(string source, char[] buffer, ref int position)
        {
            if (string.IsNullOrEmpty(source))
            {
                return;
            }

            for (var i = 0; i < source.Length; i++)
            {
                var c = source[i];
                if ('A' <= c && c <= 'Z')
                {
                    c = (char)(c | 0x20u);
                }

                buffer[position + i] = c;
            }

            buffer[position + source.Length] = ' ';
            position += source.Length + 1;
        }

        static bool Prefix(List<BeatmapLevel> levels, string[] searchTerms, ref List<BeatmapLevel> __result)
        {
            if (!Config.Instance.Enabled || !Config.Instance.FasterSearch)
            {
                return true;
            }

            for (var i = 0; i < searchTerms.Length; i++)
            {
                searchTerms[i] = searchTerms[i].ToLower();
            }

            List<BeatmapLevel> filteredLevels = new(levels.Count);
            var buffer = ArrayPool<char>.Shared.Rent(256);
            try
            {
                foreach (BeatmapLevel level in levels)
                {
                    var totalLength = GetTotalLength(level);
                    if (buffer.Length < totalLength)
                    {
                        ArrayPool<char>.Shared.Return(buffer);
                        buffer = ArrayPool<char>.Shared.Rent((int)Mathf.Floor((totalLength + 31) / 32) * 32);
                    }

                    var pos = 0;
                    CopyLower(level.songName, buffer, ref pos);
                    CopyLower(level.songSubName, buffer, ref pos);
                    CopyLower(level.songAuthorName, buffer, ref pos);

                    foreach (var mapper in level.allMappers)
                        CopyLower(mapper, buffer, ref pos);

                    var searchSpan = buffer.AsSpan(0, pos);
                    bool match = true;
                    foreach (var term in searchTerms)
                    {
                        if (searchSpan.IndexOf(term.AsSpan()) < 0)
                        {
                            match = false;
                            break;
                        }
                    }

                    if (match)
                    {
                        filteredLevels.Add(level);
                    }
                }
            }
            finally
            {
                ArrayPool<char>.Shared.Return(buffer);
            }

            __result = filteredLevels;
            return false;
        }
    }
}
