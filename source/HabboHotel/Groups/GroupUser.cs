using System;
namespace Cyber.HabboHotel.Groups
{
	internal class GroupUser
	{
		internal uint Id;
		internal int Rank;
		internal uint GroupId;
		internal GroupUser(uint Id, uint GroupId, int Rank)
		{
			this.Id = Id;
			this.GroupId = GroupId;
			this.Rank = Rank;
		}
	}
}
