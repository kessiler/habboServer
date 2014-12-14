using System;
namespace Cyber.HabboHotel.Rooms
{
	internal struct InvokedChatMessage
	{
		internal RoomUser user;
		internal string message;
		internal bool shout;
		internal int ColourType;
		internal int count;
		public InvokedChatMessage(RoomUser user, string message, bool shout, int colour, int count)
		{
			this.user = user;
			this.message = message;
			this.shout = shout;
			this.ColourType = colour;
			this.count = count;
		}
		internal void Dispose()
		{
			this.user = null;
			this.message = null;
		}
	}
}
