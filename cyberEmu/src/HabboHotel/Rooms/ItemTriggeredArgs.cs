using Cyber.HabboHotel.Items;
using System;
namespace Cyber.HabboHotel.Rooms
{
	public class ItemTriggeredArgs : EventArgs
	{
		internal readonly RoomUser TriggeringUser;
		internal readonly RoomItem TriggeringItem;
		public ItemTriggeredArgs(RoomUser user, RoomItem item)
		{
			this.TriggeringUser = user;
			this.TriggeringItem = item;
		}
	}
}
