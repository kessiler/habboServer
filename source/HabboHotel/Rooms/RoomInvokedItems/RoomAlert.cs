using System;
namespace Cyber.HabboHotel.Rooms.RoomInvokedItems
{
	internal class RoomAlert
	{
		internal string message;
		internal int minrank;
		public RoomAlert(string message, int minrank)
		{
			this.message = message;
			this.minrank = minrank;
		}
	}
}
