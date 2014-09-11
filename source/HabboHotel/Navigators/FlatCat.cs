using System;
namespace Cyber.HabboHotel.Navigators
{
	internal class FlatCat
	{
		internal int Id;
		internal string Caption;
		internal int MinRank;
        internal int UsersNow;

		internal FlatCat(int Id, string Caption, int MinRank)
		{
			this.Id = Id;
			this.Caption = Caption;
			this.MinRank = MinRank;
            this.UsersNow = 0;
		}

        internal void removeUsers(int count)
        {
            this.UsersNow = UsersNow - count;
            if (UsersNow < 0)
            {
                UsersNow = 0;
            }
        }

        internal void addUser()
        {
            this.UsersNow++;
        }
	}
}
