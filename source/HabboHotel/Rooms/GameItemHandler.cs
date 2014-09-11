using Cyber.Collections;
using Cyber.HabboHotel.Items;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Cyber.HabboHotel.Rooms
{
	internal class GameItemHandler
	{
		private QueuedDictionary<uint, RoomItem> banzaiTeleports;
        private QueuedDictionary<uint, RoomItem> banzaiPyramids;
		private Room room;
		private Random rnd;
		public GameItemHandler(Room room)
		{
			this.room = room;
			this.rnd = new Random();
			this.banzaiPyramids = new QueuedDictionary<uint, RoomItem>();
			this.banzaiTeleports = new QueuedDictionary<uint, RoomItem>();
		}
		internal void OnCycle()
		{
			this.CyclePyramids();
			this.CycleRandomTeleports();
		}
        private void CyclePyramids()
        {
            this.banzaiPyramids.OnCycle();
            Random random = new Random();

            for (uint i = 0; i < this.banzaiPyramids.Inner.Count; i++)
            {
                RoomItem current = this.banzaiPyramids.Inner[i];
                if (current == null)
                {
                    continue;
                }

                if (current.interactionCountHelper == 0 && current.ExtraData == "1")
                {
                    this.room.GetGameMap().RemoveFromMap(current, false);
                    current.interactionCountHelper = 1;
                }
                if (string.IsNullOrEmpty(current.ExtraData))
                {
                    current.ExtraData = "0";
                }
                int num = random.Next(0, 30);
                if (num == 15)
                {
                    if (current.ExtraData == "0")
                    {
                        current.ExtraData = "1";
                        current.UpdateState();
                        this.room.GetGameMap().RemoveFromMap(current, false);
                    }
                    else
                    {
                        if (this.room.GetGameMap().itemCanBePlacedHere(current.GetX, current.GetY))
                        {
                            current.ExtraData = "0";
                            current.UpdateState();
                            this.room.GetGameMap().AddItemToMap(current, false);
                        }
                    }
                }
            }
               
        }

		private void CycleRandomTeleports()
		{
			this.banzaiTeleports.OnCycle();
		}

		internal void AddPyramid(RoomItem item, uint itemID)
		{
			if (this.banzaiPyramids.ContainsKey(itemID))
			{
				this.banzaiPyramids.Inner[itemID] = item;
				return;
			}
			this.banzaiPyramids.Add(itemID, item);
		}
		internal void RemovePyramid(uint itemID)
		{
			this.banzaiPyramids.Remove(itemID);
		}
		internal void AddTeleport(RoomItem item, uint itemID)
		{
			if (this.banzaiTeleports.ContainsKey(itemID))
			{
				this.banzaiTeleports.Inner[itemID] = item;
				return;
			}
			this.banzaiTeleports.Add(itemID, item);
		}
		internal void RemoveTeleport(uint itemID)
		{
			this.banzaiTeleports.Remove(itemID);
		}
		internal void onTeleportRoomUserEnter(RoomUser User, RoomItem Item)
		{
			IEnumerable<RoomItem> enumerable = 
				from p in this.banzaiTeleports.Inner.Values
				where p.Id != Item.Id
				select p;
			int num = enumerable.Count<RoomItem>();
			int num2 = this.rnd.Next(0, num);
			int num3 = 0;
			if (num == 0)
			{
				return;
			}
			checked
			{
				foreach (RoomItem current in enumerable)
				{
					if (current != null)
					{
						if (num3 == num2)
						{
							current.ExtraData = "1";
							current.UpdateNeeded = true;
							this.room.GetGameMap().TeleportToItem(User, current);
							Item.ExtraData = "1";
							Item.UpdateNeeded = true;
							current.UpdateState();
							Item.UpdateState();
						}
						num3++;
					}
				}
			}
		}
		internal void Destroy()
		{
			if (this.banzaiTeleports != null)
			{
				this.banzaiTeleports.Destroy();
			}
			if (this.banzaiPyramids != null)
			{
				this.banzaiPyramids.Clear();
			}
			this.banzaiPyramids = null;
			this.banzaiTeleports = null;
			this.room = null;
			this.rnd = null;
		}
	}
}
