using Database_Manager.Database.Session_Details.Interfaces;
using System;
namespace Cyber.HabboHotel.Rooms
{
	internal class Chatlog
	{
		internal uint UserId;
		internal string Message;
		internal double Timestamp;
		internal bool IsSaved;
		internal Chatlog(uint User, string Msg, double Time, bool FromDatabase = false)
		{
			this.UserId = User;
			this.Message = Msg;
			this.Timestamp = Time;
			this.IsSaved = FromDatabase;
		}
		internal void Save(uint RoomId)
		{
			using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
			{
				queryreactor.setQuery("REPLACE INTO chatlogs (user_id, room_id, timestamp, message) VALUES (@user, @room, @time, @message)");
				queryreactor.addParameter("user", this.UserId);
				queryreactor.addParameter("room", RoomId);
				queryreactor.addParameter("time", this.Timestamp);
				queryreactor.addParameter("message", this.Message);
				queryreactor.runQuery();
			}
		}
	}
}
