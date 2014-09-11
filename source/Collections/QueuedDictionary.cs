using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
namespace Cyber.Collections
{
	internal class QueuedDictionary<T, V>
	{
		private Dictionary<T, V> collection;
		private Queue addQueue;
		private Queue updateQueue;
		private Queue removeQueue;
		private Queue onCycleEventQueue;
		private EventHandler onAdd;
		private EventHandler onUpdate;
		private EventHandler onRemove;
		private EventHandler onCycleDone;
		internal Dictionary<T, V>.ValueCollection Values
		{
			get
			{
				return this.collection.Values;
			}
		}
		internal Dictionary<T, V>.KeyCollection Keys
		{
			get
			{
				return this.collection.Keys;
			}
		}
		internal Dictionary<T, V> Inner
		{
			get
			{
				return this.collection;
			}
			set
			{
				this.collection = value;
			}
		}
		public QueuedDictionary()
		{
			this.collection = new Dictionary<T, V>();
			this.addQueue = new Queue();
			this.updateQueue = new Queue();
			this.removeQueue = new Queue();
			this.onCycleEventQueue = new Queue();
		}
		public QueuedDictionary(EventHandler onAddItem, EventHandler onUpdate, EventHandler onRemove, EventHandler onCycleDone)
		{
			this.collection = new Dictionary<T, V>();
			this.addQueue = new Queue();
			this.updateQueue = new Queue();
			this.removeQueue = new Queue();
			this.onAdd = onAddItem;
			this.onUpdate = onUpdate;
			this.onRemove = onRemove;
			this.onCycleDone = onCycleDone;
			this.onCycleEventQueue = new Queue();
		}
		internal void OnCycle()
		{
			this.WorkRemoveQueue();
			this.WorkAddQueue();
			this.WorkUpdateQueue();
			this.WorkOnEventDoneQueue();
			if (this.onCycleDone != null)
			{
				this.onCycleDone(null, new EventArgs());
			}
		}
		private void WorkOnEventDoneQueue()
		{
			if (this.onCycleEventQueue.Count > 0)
			{
				lock (this.onCycleEventQueue.SyncRoot)
				{
					while (this.onCycleEventQueue.Count > 0)
					{
						onCycleDoneDelegate onCycleDoneDelegate = (onCycleDoneDelegate)this.onCycleEventQueue.Dequeue();
						onCycleDoneDelegate();
					}
				}
			}
		}
		private void WorkAddQueue()
		{
			if (this.addQueue.Count > 0)
			{
				lock (this.addQueue.SyncRoot)
				{
					while (this.addQueue.Count > 0)
					{
						KeyValuePair<T, V> keyValuePair = (KeyValuePair<T, V>)this.addQueue.Dequeue();
						if (this.collection.ContainsKey(keyValuePair.Key))
						{
							this.collection[keyValuePair.Key] = keyValuePair.Value;
						}
						else
						{
							this.collection.Add(keyValuePair.Key, keyValuePair.Value);
						}
						if (this.onAdd != null)
						{
							this.onAdd(keyValuePair, null);
						}
					}
				}
			}
		}
		private void WorkUpdateQueue()
		{
			if (this.updateQueue.Count > 0)
			{
				lock (this.updateQueue.SyncRoot)
				{
					while (this.updateQueue.Count > 0)
					{
						KeyValuePair<T, V> keyValuePair = (KeyValuePair<T, V>)this.addQueue.Dequeue();
						if (this.collection.ContainsKey(keyValuePair.Key))
						{
							this.collection[keyValuePair.Key] = keyValuePair.Value;
						}
						else
						{
							this.collection.Add(keyValuePair.Key, keyValuePair.Value);
						}
						if (this.onUpdate != null)
						{
							this.onUpdate(keyValuePair, null);
						}
					}
				}
			}
		}
		private void WorkRemoveQueue()
		{
			if (this.removeQueue.Count > 0)
			{
				lock (this.removeQueue.SyncRoot)
				{
					List<T> list = new List<T>();
					while (this.removeQueue.Count > 0)
					{
						T t = (T)((object)this.removeQueue.Dequeue());
						if (this.collection.ContainsKey(t))
						{
							V value = this.collection[t];
							this.collection.Remove(t);
							KeyValuePair<T, V> keyValuePair = new KeyValuePair<T, V>(t, value);
							if (this.onRemove != null)
							{
								this.onRemove(keyValuePair, null);
							}
						}
						else
						{
							list.Add(t);
						}
					}
					if (list.Count > 0)
					{
						foreach (T current in list)
						{
							this.removeQueue.Enqueue(current);
						}
					}
				}
			}
		}
		private void WorkEventQueue()
		{
			if (this.onCycleEventQueue.Count > 0)
			{
				lock (this.onCycleEventQueue.SyncRoot)
				{
					while (this.onCycleEventQueue.Count > 0)
					{
						onCycleDoneDelegate onCycleDoneDelegate = (onCycleDoneDelegate)this.onCycleEventQueue.Dequeue();
						onCycleDoneDelegate();
					}
				}
			}
		}
		private void onAddItem(object sender, EventArgs args)
		{
		}
		private void onUpdateItem(object sender, EventArgs args)
		{
		}
		private void onRemoveItem(object sender, EventArgs args)
		{
		}
		private void onCycleIsDone(object sender, EventArgs args)
		{
		}
		internal void Add(T key, V value)
		{
			KeyValuePair<T, V> keyValuePair = new KeyValuePair<T, V>(key, value);
			lock (this.addQueue.SyncRoot)
			{
				this.addQueue.Enqueue(keyValuePair);
			}
		}
		internal void Update(T key, V value)
		{
			KeyValuePair<T, V> keyValuePair = new KeyValuePair<T, V>(key, value);
			lock (this.updateQueue.SyncRoot)
			{
				this.updateQueue.Enqueue(keyValuePair);
			}
		}
		internal void Remove(T key)
		{
			lock (this.removeQueue.SyncRoot)
			{
				this.removeQueue.Enqueue(key);
			}
		}
		internal V GetValue(T key)
		{
			if (this.collection.ContainsKey(key))
			{
				return this.collection[key];
			}
			return default(V);
		}
		internal bool ContainsKey(T key)
		{
			return this.collection.ContainsKey(key);
		}
		internal void Clear()
		{
			this.collection.Clear();
		}
		internal void QueueDelegate(onCycleDoneDelegate function)
		{
			lock (this.onCycleEventQueue.SyncRoot)
			{
				this.onCycleEventQueue.Enqueue(function);
			}
		}
		internal List<KeyValuePair<T, V>> ToList()
		{
			return this.collection.ToList<KeyValuePair<T, V>>();
		}
		internal void Destroy()
		{
			if (this.collection != null)
			{
				this.collection.Clear();
			}
			if (this.addQueue != null)
			{
				this.addQueue.Clear();
			}
			if (this.updateQueue != null)
			{
				this.updateQueue.Clear();
			}
			if (this.removeQueue != null)
			{
				this.removeQueue.Clear();
			}
			if (this.onCycleEventQueue != null)
			{
				this.onCycleEventQueue.Clear();
			}
			this.collection = null;
			this.addQueue = null;
			this.updateQueue = null;
			this.removeQueue = null;
			this.onCycleEventQueue = null;
			this.onAdd = null;
			this.onUpdate = null;
			this.onRemove = null;
			this.onCycleDone = null;
		}
	}
}
