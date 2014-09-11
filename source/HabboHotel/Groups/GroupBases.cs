using System;
namespace Cyber.HabboHotel.Groups
{
	internal class GroupBases
	{
		internal int Id;
		internal string Value1;
		internal string Value2;
		internal GroupBases(int Id, string Value1, string Value2)
		{
			this.Id = Id;
			this.Value1 = Value1;
			this.Value2 = Value2;
		}
	}
}
