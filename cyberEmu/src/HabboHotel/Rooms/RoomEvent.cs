using System;
namespace Cyber.HabboHotel.Rooms
{
	internal class RoomEvent
	{
		internal string Name;
		internal string Description;
		internal int Time;
		internal uint RoomId;
		internal bool HasExpired
		{
			get
			{
				return CyberEnvironment.GetUnixTimestamp() > this.Time;
			}
		}
		internal RoomEvent(uint RoomId, string Name, string Description, int Time = 0)
		{
			this.RoomId = RoomId;
			this.Name = Name;
			this.Description = Description;
			this.Time = ((Time == 0) ? checked(CyberEnvironment.GetUnixTimestamp() + 7200) : Time);
		}
	}
}
