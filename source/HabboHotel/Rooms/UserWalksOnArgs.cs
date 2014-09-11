using System;
namespace Cyber.HabboHotel.Rooms
{
	public class UserWalksOnArgs : EventArgs
	{
		internal readonly RoomUser user;
		public UserWalksOnArgs(RoomUser user)
		{
			this.user = user;
		}
	}
}
