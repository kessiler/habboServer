using System;
namespace Cyber.HabboHotel.Users.Badges
{
	internal class Badge
	{
		internal string Code;
		internal int Slot;
		internal Badge(string Code, int Slot)
		{
			this.Code = Code;
			this.Slot = Slot;
		}
	}
}
