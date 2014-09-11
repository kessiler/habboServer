using Cyber.HabboHotel.Rooms.Games;
using System;
namespace Cyber.HabboHotel.Rooms
{
	public class TeamScoreChangedArgs : EventArgs
	{
		internal readonly int Points;
		internal readonly Team Team;
		internal readonly RoomUser user;
		public TeamScoreChangedArgs(int points, Team team, RoomUser user)
		{
			this.Points = points;
			this.Team = team;
			this.user = user;
		}
	}
}
