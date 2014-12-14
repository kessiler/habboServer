using Cyber.HabboHotel.Items;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;

namespace Cyber.HabboHotel.Rooms
{
	internal class CoordItemSearch
	{
		private HybridDictionary items;
		public CoordItemSearch(HybridDictionary itemArray)
		{
			this.items = itemArray;
		}
		internal List<RoomItem> GetRoomItemForSquare(int pX, int pY, double minZ)
		{
			List<RoomItem> list = new List<RoomItem>();
			Point point = new Point(pX, pY);
            if (this.items.Contains(point))
			{
				List<RoomItem> list2 = (List<RoomItem>)this.items[point];
				foreach (RoomItem current in list2)
				{
					if (current.GetZ > minZ && current.GetX == pX && current.GetY == pY)
					{
						list.Add(current);
					}
				}
			}
			return list;
		}
		internal List<RoomItem> GetRoomItemForSquare(int pX, int pY)
		{
			Point point = new Point(pX, pY);
			List<RoomItem> list = new List<RoomItem>();
            if (this.items.Contains(point))
			{
				List<RoomItem> list2 = (List<RoomItem>)this.items[point];
				foreach (RoomItem current in list2)
				{
					if (current.Coordinate.X == point.X && current.Coordinate.Y == point.Y)
					{
						list.Add(current);
					}
				}
			}
			return list;
		}
		internal List<RoomItem> GetAllRoomItemForSquare(int pX, int pY)
		{
			Point point = new Point(pX, pY);
			List<RoomItem> list = new List<RoomItem>();
            if (this.items.Contains(point))
			{
				List<RoomItem> list2 = (List<RoomItem>)this.items[point];
				foreach (RoomItem current in list2)
				{
					if (!list.Contains(current))
					{
						list.Add(current);
					}
				}
			}
			return list;
		}
	}
}
