using System;
using System.Collections;
using System.Collections.Generic;
namespace Cyber.Collections
{
	public class SafeDictionary<T, V> : IDictionary<T, V>, ICollection<KeyValuePair<T, V>>, IEnumerable<KeyValuePair<T, V>>, IEnumerable, IDisposable
	{
		private readonly Dictionary<T, V> _inner;
		private readonly object lockObject;
		internal Dictionary<T, V> getInner
		{
			get
			{
				return this._inner;
			}
		}
		public int Count
		{
			get
			{
				return this._inner.Count;
			}
		}
		public bool IsReadOnly
		{
			get
			{
				return false;
			}
		}
		public ICollection<T> Keys
		{
			get
			{
				ICollection<T> result;
				lock (this.lockObject)
				{
					result = new List<T>(this._inner.Keys);
				}
				return result;
			}
		}
		public ICollection<V> Values
		{
			get
			{
				ICollection<V> result;
				lock (this.lockObject)
				{
					result = new List<V>(this._inner.Values);
				}
				return result;
			}
		}
		public V this[T Item]
		{
			get
			{
				return this._inner[Item];
			}
			set
			{
				this._inner[Item] = value;
			}
		}
		public SafeDictionary()
		{
			this._inner = new Dictionary<T, V>();
			this.lockObject = new object();
		}
		public SafeDictionary(Dictionary<T, V> inner)
		{
			this._inner = inner;
			this.lockObject = new object();
		}
		internal Dictionary<T, V> getNonThreadSafeCollection()
		{
			return this._inner;
		}
		~SafeDictionary()
		{
			this.Dispose(false);
		}
		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}
		private void Dispose(bool disposing)
		{
		}
		public void Clear()
		{
			lock (this.lockObject)
			{
				this._inner.Clear();
			}
		}
		public void Add(T Item, V Value)
		{
			lock (this.lockObject)
			{
				this._inner.Add(Item, Value);
			}
		}
		public void Remove(T Item)
		{
			lock (this.lockObject)
			{
				this._inner.Remove(Item);
			}
		}
		public bool ContainsKey(T Item)
		{
			return this._inner.ContainsKey(Item);
		}
		public V GetValue(T Item)
		{
			if (this._inner.ContainsKey(Item))
			{
				return this._inner[Item];
			}
			return default(V);
		}
		public IEnumerator<KeyValuePair<T, V>> GetEnumerator()
		{
			throw new NotImplementedException();
		}
		bool IDictionary<T, V>.Remove(T key)
		{
			throw new NotImplementedException();
		}
		public bool TryGetValue(T key, out V value)
		{
			return this._inner.TryGetValue(key, out value);
		}
		public void Add(KeyValuePair<T, V> item)
		{
			throw new NotImplementedException();
		}
		public bool Contains(KeyValuePair<T, V> item)
		{
			throw new NotImplementedException();
		}
		public void CopyTo(KeyValuePair<T, V>[] array, int arrayIndex)
		{
			throw new NotImplementedException();
		}
		public bool Remove(KeyValuePair<T, V> item)
		{
			throw new NotImplementedException();
		}
		IEnumerator IEnumerable.GetEnumerator()
		{
			throw new NotImplementedException();
		}
	}
}
