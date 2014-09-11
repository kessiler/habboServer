using System;
namespace Cyber.HabboHotel.Rooms.RoomInvokedItems
{
	internal struct RoomKick
	{
		internal string allert;
		internal int minrank;
		public RoomKick(string allert, int minrank)
		{
			this.allert = allert;
			this.minrank = minrank;
		}
	}
}
