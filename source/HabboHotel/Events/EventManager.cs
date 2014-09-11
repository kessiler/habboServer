using Cyber.HabboHotel.Rooms;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
namespace Cyber.HabboHotel.Events
{
	internal class EventManager
	{
		private Dictionary<RoomData, int> events;
		private IOrderedEnumerable<KeyValuePair<RoomData, int>> orderedEventRooms;
		private Queue addQueue;
		private Queue removeQueue;
		private Queue updateQueue;
		private Dictionary<int, EventCategory> eventCategories;
		internal KeyValuePair<RoomData, int>[] GetRooms()
		{
			return this.orderedEventRooms.ToArray<KeyValuePair<RoomData, int>>();
		}
		public EventManager()
		{
			this.eventCategories = new Dictionary<int, EventCategory>();
			this.events = new Dictionary<RoomData, int>();
			this.orderedEventRooms = 
				from t in this.events
				orderby t.Value descending
				select t;
			this.addQueue = new Queue();
			this.removeQueue = new Queue();
			this.updateQueue = new Queue();
			checked
			{
				for (int i = 0; i < 30; i++)
				{
					this.eventCategories.Add(i, new EventCategory(i));
				}
			}
		}
		internal void onCycle()
		{
			this.workRemoveQueue();
			this.workAddQueue();
			this.workUpdate();
			this.SortCollection();
			foreach (EventCategory current in this.eventCategories.Values)
			{
				current.onCycle();
			}
		}
		private void SortCollection()
		{
			this.orderedEventRooms = 
				from t in this.events.Take(40)
				orderby t.Value descending
				select t;
		}
		private void workAddQueue()
		{
			if (this.addQueue.Count > 0)
			{
				lock (this.addQueue.SyncRoot)
				{
					while (this.addQueue.Count > 0)
					{
						RoomData roomData = (RoomData)this.addQueue.Dequeue();
						if (!this.events.ContainsKey(roomData))
						{
							this.events.Add(roomData, roomData.UsersNow);
						}
					}
				}
			}
		}
		private void workRemoveQueue()
		{
			if (this.removeQueue.Count > 0)
			{
				lock (this.removeQueue.SyncRoot)
				{
					while (this.removeQueue.Count > 0)
					{
						RoomData key = (RoomData)this.removeQueue.Dequeue();
						this.events.Remove(key);
					}
				}
			}
		}
		private void workUpdate()
		{
			if (this.removeQueue.Count > 0)
			{
				lock (this.removeQueue.SyncRoot)
				{
					while (this.removeQueue.Count > 0)
					{
						RoomData roomData = (RoomData)this.updateQueue.Dequeue();
						if (this.events.ContainsKey(roomData))
						{
							this.events[roomData] = roomData.UsersNow;
						}
					}
				}
			}
		}
		internal void QueueAddEvent(RoomData data, int roomEventCategory)
		{
			lock (this.addQueue.SyncRoot)
			{
				this.addQueue.Enqueue(data);
			}
			this.eventCategories[roomEventCategory].QueueAddEvent(data);
		}
		internal void QueueRemoveEvent(RoomData data, int roomEventCategory)
		{
			lock (this.removeQueue.SyncRoot)
			{
				this.removeQueue.Enqueue(data);
			}
			this.eventCategories[roomEventCategory].QueueRemoveEvent(data);
		}
		internal void QueueUpdateEvent(RoomData data, int roomEventCategory)
		{
			lock (this.updateQueue.SyncRoot)
			{
				this.updateQueue.Enqueue(data);
			}
			this.eventCategories[roomEventCategory].QueueUpdateEvent(data);
		}
	}
}
