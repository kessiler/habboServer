using System;
namespace Cyber.HabboHotel.Rooms
{
	public class UserSaysArgs : EventArgs
	{
		internal readonly RoomUser user;
		internal readonly string message;
		public UserSaysArgs(RoomUser user, string message)
		{
			this.user = user;
			this.message = message;
		}
	}
}
