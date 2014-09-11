using Cyber.Messages;
using System;
namespace Cyber.HabboHotel.Users.Messenger
{
	internal struct SearchResult
	{
		internal uint userID;
		internal string username;
		internal string motto;
		internal string look;
		internal string last_online;
		public SearchResult(uint userID, string username, string motto, string look, string last_online)
		{
			this.userID = userID;
			this.username = username;
			this.motto = motto;
			this.look = look;
			this.last_online = last_online;
		}
		internal void Searialize(ServerMessage reply)
		{
			reply.AppendUInt(this.userID);
			reply.AppendString(this.username);
			reply.AppendString(this.motto);
			bool b = CyberEnvironment.GetGame().GetClientManager().GetClient(this.userID) != null;
			reply.AppendBoolean(b);
			reply.AppendBoolean(false);
			reply.AppendString(string.Empty);
			reply.AppendInt32(0);
			reply.AppendString(this.look);
			reply.AppendString(this.last_online);
		}
	}
}
