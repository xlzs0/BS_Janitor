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
