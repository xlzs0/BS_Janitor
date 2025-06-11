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

using System;
using System.Collections.Generic;

namespace BS_Janitor.Utils;

internal readonly struct LRUCache<TKey, TValue>(uint size)
    where TKey : IEquatable<TKey>
{
    private readonly Dictionary<TKey, (TValue Value, LinkedListNode<TKey> lruNode)> cache = [];
    private readonly LinkedList<TKey> lru = [];

    public void Add(TKey key, TValue? value)
    {
        lock (cache)
        {
            if (value == null)
            {
                return;
            }

            if (cache.TryGetValue(key, out var cachedValue))
            {
                lru.Remove(cachedValue.lruNode);
                cache.Remove(key);
            }

            cache.Add(key, (value, lru.AddFirst(key)));

            while (lru.Count > size)
            {
                cache.Remove(lru.Last.Value);
                lru.RemoveLast();
            }
        }
    }

    public bool TryGet(TKey key, out TValue? value)
    {
        lock (cache)
        {
            if (!cache.TryGetValue(key, out var cachedValue))
            {
                value = default;
                return false;
            }

            if (cachedValue.lruNode.Previous != null)
            {
                lru.Remove(cachedValue.lruNode);
                lru.AddFirst(cachedValue.lruNode);
            }

            value = cachedValue.Value;
            return true;
        }
    }
}
