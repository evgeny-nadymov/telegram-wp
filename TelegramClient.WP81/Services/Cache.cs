// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Collections.Generic;

namespace TelegramClient.Services
{
    internal class LinkedDictionary<TKey, TValue>
    {
        private readonly LinkedList<TKey> _queue = new LinkedList<TKey>();

        private readonly Dictionary<TKey, KeyValuePair<LinkedListNode<TKey>, TValue>> _cache = new Dictionary<TKey, KeyValuePair<LinkedListNode<TKey>, TValue>>();

        public LinkedDictionary()
        {

        }

        public void AddFirst(TKey key, TValue value)
        {
            try
            {
                var node = _queue.AddFirst(key);
                _cache[key] = new KeyValuePair<LinkedListNode<TKey>, TValue>(node, value);
            }
            catch (Exception ex)
            {
                
            }
        }

        public void RemoveLast()
        {
            var last = _queue.Last;

            _queue.RemoveLast();
            _cache.Remove(last.Value);
        }

        public KeyValuePair<TKey, TValue> First
        {
            get
            {
                var node = _queue.First;

                KeyValuePair<LinkedListNode<TKey>, TValue> result;
                if (node != null && _cache.TryGetValue(node.Value, out result))
                {
                    return new KeyValuePair<TKey, TValue>(node.Value, result.Value);
                }

                return new KeyValuePair<TKey, TValue>();
            }
        }

        public KeyValuePair<TKey, TValue> Last
        {
            get
            {
                var node = _queue.Last;

                KeyValuePair<LinkedListNode<TKey>, TValue> result;
                if (node != null && _cache.TryGetValue(node.Value, out result))
                {
                    return new KeyValuePair<TKey, TValue>(node.Value, result.Value);
                }

                return new KeyValuePair<TKey, TValue>();
            }
        }

        public int Count
        {
            get { return _queue.Count; }
        }

        public void Add(TKey key, TValue value)
        {
            KeyValuePair<LinkedListNode<TKey>, TValue> tuple;
            if (!_cache.TryGetValue(key, out tuple))
            {
                var node = _queue.AddLast(key);                                             // O(1)
                _cache[key] = new KeyValuePair<LinkedListNode<TKey>, TValue>(node, value);  // O(1)
            }
        }

        public bool Remove(TKey key)
        {
            KeyValuePair<LinkedListNode<TKey>, TValue> tuple;
            if (_cache.TryGetValue(key, out tuple))
            {
                _cache.Remove(key);                 // O(1)
                _queue.Remove(tuple.Key);           // O(1)

                return true;
            }

            return false;
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            KeyValuePair<LinkedListNode<TKey>, TValue> tuple;
            if (_cache.TryGetValue(key, out tuple))
            {
                value = tuple.Value;

                return true;
            }

            value = default(TValue);

            return false;
        }

        public TValue this[TKey key]
        {
            get
            {
                TValue value;
                if (TryGetValue(key, out value))
                {
                    return value;
                }

                throw new KeyNotFoundException();
            }
            set
            {
                Add(key, value);
            }
        }
    }

    public class LRUCache<TKey, TValue>
    {
        private readonly LinkedList<TKey> _queue = new LinkedList<TKey>();

        private readonly Dictionary<TKey, KeyValuePair<LinkedListNode<TKey>, TValue>> _cache;

        private readonly int _capacity;

        private readonly object _syncRoot = new object();

        public LRUCache(int capacity)
        {
            _capacity = capacity;
            _cache = new Dictionary<TKey, KeyValuePair<LinkedListNode<TKey>, TValue>>(capacity);
        }

        public void Add(TKey key, TValue value)
        {
            lock (_syncRoot)
            {
                KeyValuePair<LinkedListNode<TKey>, TValue> tuple;
                if (_cache.TryGetValue(key, out tuple))
                {
                    _queue.Remove(tuple.Key);           // O(1)
                    var node = _queue.AddLast(key);     // O(1)
                    _cache[key] = new KeyValuePair<LinkedListNode<TKey>, TValue>(node, value);
                }
                else
                {
                    if (_cache.Count == _capacity)
                    {
                        var first = _queue.First;

                        _queue.RemoveFirst();           // O(1)
                        _cache.Remove(first.Value);     // O(1)
                    }

                    var node = _queue.AddLast(key);
                    _cache[key] = new KeyValuePair<LinkedListNode<TKey>, TValue>(node, value);
                }
            }
        }

        public void Remove(TKey key)
        {
            lock (_syncRoot)
            {
                KeyValuePair<LinkedListNode<TKey>, TValue> tuple;
                if (_cache.TryGetValue(key, out tuple))
                {
                    _cache.Remove(key);                 // O(1)
                    _queue.Remove(tuple.Key);           // O(1)
                }
            }
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            lock (_syncRoot)
            {
                KeyValuePair<LinkedListNode<TKey>, TValue> tuple;
                if (_cache.TryGetValue(key, out tuple))
                {
                    value = tuple.Value;

                    _queue.Remove(tuple.Key);           // O(1)
                    _queue.AddLast(tuple.Key);          // O(1)

                    return true;
                }

                value = default(TValue);

                return false;
            }
        }

        public TValue this[TKey key]
        {
            get
            {
                TValue value; 
                if (TryGetValue(key, out value))
                {
                    return value;
                }

                throw new KeyNotFoundException();
            }
            set
            {
                Add(key, value);
            }
        }
    }

    /// https://pdfs.semanticscholar.org/d62d/e5f995164fff50f5ce61c0113f6bc9f04225.pdf
    /// A1 - _inQueue, _outQueue
    /// Am - _cache
    public class TwoQueueCache<TKey, TValue>
    {
        private readonly LRUCache<TKey, TValue> _cache;

        private readonly LinkedDictionary<TKey, TValue> _inQueue = new LinkedDictionary<TKey, TValue>();

        private readonly LinkedDictionary<TKey, TValue> _outQueue = new LinkedDictionary<TKey, TValue>();
        
        private readonly int _cacheCapacity;

        private readonly int _inQueueCapacity;

        private readonly int _outQueueCapacity;

        private readonly object _syncRoot = new object();

        public TwoQueueCache(int capacity)
        {
            _inQueueCapacity = (int)(0.25 * capacity);
            _outQueueCapacity = (int)(0.5 * capacity);
            _cacheCapacity = capacity - _inQueueCapacity - _outQueueCapacity;

            _cache = new LRUCache<TKey, TValue>(_cacheCapacity);
        }

        public void Add(TKey key, TValue value)
        {
            lock (_syncRoot)
            {
                // make room
                if (_inQueue.Count > _inQueueCapacity)
                {
                    if (_outQueue.Count > _outQueueCapacity)
                    {
                        _outQueue.RemoveLast();                         // O(1)
                    }

                    var last = _inQueue.Last;
                    _inQueue.RemoveLast();                              // O(1)
                    _outQueue.AddFirst(last.Key, last.Value);           // O(1)
                }

                // add
                _inQueue.AddFirst(key, value);                          // O(1)
            }
        }

        public bool Remove(TKey key)
        {
            lock (_syncRoot)
            {
                _inQueue.Remove(key);               // O(1)
                _outQueue.Remove(key);              // O(1)
                _cache.Remove(key);                 // O(1)
            }

            return true;
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            lock (_syncRoot)
            {
                if (_inQueue.TryGetValue(key, out value))
                {
                    return true;
                }

                if (_outQueue.TryGetValue(key, out value))
                {
                    _outQueue.Remove(key);              // O(1)
                    _cache.Add(key, value);             // O(1)

                    return true;
                }

                if (_cache.TryGetValue(key, out value))
                {
                    return true;
                }

                value = default(TValue);

                return false;
            }
        }

        public TValue this[TKey key]
        {
            get
            {
                TValue value;
                if (TryGetValue(key, out value))
                {
                    return value;
                }

                throw new KeyNotFoundException();
            }
            set
            {
                Add(key, value);
            }
        }
    }
}
