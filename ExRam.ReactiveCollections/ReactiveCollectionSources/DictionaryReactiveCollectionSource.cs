﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.Specialized;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace ExRam.ReactiveCollections
{
    public class DictionaryReactiveCollectionSource<TKey, TValue> :
        ReactiveCollectionSource<DictionaryChangedNotification<TKey, TValue>, KeyValuePair<TKey, TValue>>,
        IDictionary<TKey, TValue>, 
        IDictionary
    {
        public DictionaryReactiveCollectionSource()
        {
            this.Subject.OnNext(new DictionaryChangedNotification<TKey, TValue>(ImmutableDictionary<TKey, TValue>.Empty, NotifyCollectionChangedAction.Reset, ImmutableList<KeyValuePair<TKey, TValue>>.Empty, ImmutableList<KeyValuePair<TKey, TValue>>.Empty));
        }

        #region IImmutableDictionary<TKey, TValue> implementation
        public void Add(TKey key, TValue value)
        {
            var newList = this.Current.Add(key, value);
            this.Subject.OnNext(new DictionaryChangedNotification<TKey, TValue>(newList, NotifyCollectionChangedAction.Add, ImmutableList<KeyValuePair<TKey, TValue>>.Empty, ImmutableList.Create(new KeyValuePair<TKey, TValue>(key, value))));
        }

        public void AddRange(IEnumerable<TValue> values, Func<TValue, TKey> keySelector)
        {
            this.AddRange(values.Select(x => new KeyValuePair<TKey, TValue>(keySelector(x), x)));
        }

        public void AddRange(IEnumerable<KeyValuePair<TKey, TValue>> pairs)
        {
            var immutablePairs = ImmutableList<KeyValuePair<TKey, TValue>>.Empty.AddRange(pairs);
            var newList = this.Current.AddRange(immutablePairs);
            this.Subject.OnNext(new DictionaryChangedNotification<TKey, TValue>(newList, NotifyCollectionChangedAction.Add, ImmutableList<KeyValuePair<TKey, TValue>>.Empty, immutablePairs));
        }

        public void Clear()
        {
            this.Subject.OnNext(new DictionaryChangedNotification<TKey, TValue>(ImmutableDictionary<TKey, TValue>.Empty, NotifyCollectionChangedAction.Reset, ImmutableList<KeyValuePair<TKey, TValue>>.Empty, ImmutableList<KeyValuePair<TKey, TValue>>.Empty));
        }

        public bool Contains(KeyValuePair<TKey, TValue> pair)
        {
            return this.Current.Contains(pair);
        }

        public void Remove(TKey key)
        {
            var oldList = this.Current;
            var newList = oldList.Remove(key);

            if (oldList != newList)
            {
                var oldValue = oldList[key];
                this.Subject.OnNext(new DictionaryChangedNotification<TKey, TValue>(newList, NotifyCollectionChangedAction.Remove, ImmutableList.Create(new KeyValuePair<TKey, TValue>(key, oldValue)), ImmutableList<KeyValuePair<TKey, TValue>>.Empty));
            }
        }

        public void RemoveRange(IEnumerable<TKey> keys)
        {
            Contract.Requires(keys != null);

            //TODO: Optimize!
            foreach (var key in keys)
            {
                this.Remove(key);
            }
        }

        public void SetItem(TKey key, TValue value)
        {
            var oldList = this.Current;
            var newList = oldList.SetItem(key, value);

            if (oldList != newList)
            {
                var oldValue = oldList[key];
                this.Subject.OnNext(new DictionaryChangedNotification<TKey, TValue>(newList, NotifyCollectionChangedAction.Replace, ImmutableList.Create(new KeyValuePair<TKey, TValue>(key, oldValue)), ImmutableList.Create(new KeyValuePair<TKey, TValue>(key, value))));
            }
        }

        public void SetItems(IEnumerable<KeyValuePair<TKey, TValue>> items)
        {
            Contract.Requires(items != null);

            //TODO: Optimize!
            foreach (var item in items)
            {
                this.SetItem(item.Key, item.Value);
            }
        }

        public bool ContainsKey(TKey key)
        {
            return this.Current.ContainsKey(key);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return this.Current.TryGetValue(key, out value);
        }

        public TValue this[TKey key]
        {
            get
            {
                return this.Current[key];
            }

            set
            {
                this.SetItem(key, value);
            }
        }

        public int Count
        {
            get
            {
                return this.Current.Count;
            }
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return this.Current.GetEnumerator();
        }
        #endregion

        #region IDictionary<TKey, TValue> implementation
        void IDictionary<TKey, TValue>.Add(TKey key, TValue value)
        {
            this.Add(key, value);
        }

        ICollection<TKey> IDictionary<TKey, TValue>.Keys
        {
            get
            {
                return this.Current.Keys.ToList();
            }
        }

        bool IDictionary<TKey, TValue>.Remove(TKey key)
        {
            var oldList = this.Current;
            this.Remove(key);

            return (this.Current != oldList);
        }

        ICollection<TValue> IDictionary<TKey, TValue>.Values
        {
            get
            {
                return this.Current.Values.ToList();
            }
        }
        #endregion

        #region ICollection<KeyValuePair<TKey, TValue>> implementation
        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item)
        {
            this.Add(item.Key, item.Value);
        }

        void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            ((ICollection<KeyValuePair<TKey, TValue>>)this.Current).CopyTo(array, arrayIndex);
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
        {
            return ((ICollection<KeyValuePair<TKey, TValue>>)this.Current).Remove(item);
        }

        void ICollection<KeyValuePair<TKey, TValue>>.Clear()
        {
            this.Clear();
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly
        {
            get
            {
                return false;
            }
        }
        #endregion

        #region IDictionary implementation
        void IDictionary.Add(object key, object value)
        {
            this.Add((TKey)key, (TValue)value);
        }

        bool IDictionary.Contains(object key)
        {
            return this.ContainsKey((TKey)key);
        }

        IDictionaryEnumerator IDictionary.GetEnumerator()
        {
            return ((IDictionary)this.Current).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        bool IDictionary.IsFixedSize
        {
            get
            {
                return false;
            }
        }

        bool IDictionary.IsReadOnly
        {
            get
            {
                return false;
            }
        }

        ICollection IDictionary.Keys
        {
            get
            {
                return this.Current.Keys.ToList();
            }
        }

        void IDictionary.Remove(object key)
        {
            this.Remove((TKey)key);
        }

        ICollection IDictionary.Values
        {
            get
            {
                return this.Current.Values.ToList();
            }
        }

        object IDictionary.this[object key]
        {
            get
            {
                return this[(TKey)key];
            }

            set
            {
                this[(TKey)key] = (TValue)value;
            }
        }

        void ICollection.CopyTo(Array array, int index)
        {
            ((IDictionary)this).CopyTo(array, index);
        }
        #endregion

        #region ICollection implementation
        bool ICollection.IsSynchronized
        {
            get
            {
                return false;
            }
        }

        object ICollection.SyncRoot
        {
            get
            {
                return this;
            }
        }
        #endregion

        public IEnumerable<TValue> Values
        {
            get
            {
                return this.Current.Values;
            }
        }

        private ImmutableDictionary<TKey, TValue> Current
        {
            get
            {
                return this.Subject.Value.Current;
            }
        }
    }
}