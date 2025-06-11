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
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;

namespace BS_Janitor.HarmonyPatches;

[HarmonyPatch(typeof(LevelFilter), nameof(LevelFilter.FilterLevelByText))]
internal class LevelFilterPatch
{
    private static void CopyLower(string source, char[] buffer, ref int position)
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

            buffer[position++] = c;
        }
    }

    internal static bool Prefix(List<BeatmapLevel> levels, string[] searchTerms, ref List<BeatmapLevel> __result)
    {
        for (var i = 0; i < searchTerms.Length; i++)
        {
            searchTerms[i] = searchTerms[i].ToLower();
        }
        
        var buffer = ArrayPool<char>.Shared.Rent(levels.Max(level => level.songName.Length + level.songSubName.Length + level.songAuthorName.Length + level.allMappers.Sum(mapper => mapper.Length) + 1));
        try
        {
            __result = levels.Where(level =>
            {
                var pos = 0;
                CopyLower(level.songName, buffer, ref pos);
                CopyLower(level.songSubName, buffer, ref pos);
                CopyLower(level.songAuthorName, buffer, ref pos);

                foreach (var mapper in level.allMappers)
                    CopyLower(mapper, buffer, ref pos);

                var searchSpan = buffer.AsSpan(0, pos);
                foreach (var term in searchTerms)
                {
                    if (searchSpan.IndexOf(term.AsSpan()) < 0)
                    {
                        return false;
                    }
                }

                return true;
            }).ToList();
        }
        finally
        {
            ArrayPool<char>.Shared.Return(buffer);
        }

        return false;
    }
}
