using System;
namespace Cyber.HabboHotel.Users.Relationships
{
	internal class Relationship
	{
		public int Id;
		public int UserId;
		public int Type;
		public Relationship(int Id, int User, int Type)
		{
			this.Id = Id;
			this.UserId = User;
			this.Type = Type;
		}
	}
}
