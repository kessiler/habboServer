using System;
namespace Cyber.HabboHotel.Achievements
{
	internal class UserTalent
	{
		internal int TalentId;
		internal int State;
		public UserTalent(int TalentId, int State)
		{
			this.TalentId = TalentId;
			this.State = State;
		}
	}
}
