using Database_Manager.Database.Session_Details.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
namespace Cyber.HabboHotel.Users.Messenger
{
	internal class OfflineMessage
	{
		internal uint FromId;
		internal string Message;
		internal double Timestamp;
		internal OfflineMessage(uint Id, string Msg, double ts)
		{
			this.FromId = Id;
			this.Message = Msg;
			this.Timestamp = ts;
		}
		internal static void InitOfflineMessages(IQueryAdapter dbClient)
		{
			dbClient.setQuery("SELECT * FROM messenger_offline_messages");
			DataTable table = dbClient.getTable();
			foreach (DataRow dataRow in table.Rows)
			{
				uint key = (uint)dataRow[1];
				uint id = (uint)dataRow[2];
				string msg = dataRow[3].ToString();
				double ts = (double)dataRow[4];
				if (!CyberEnvironment.OfflineMessages.ContainsKey(key))
				{
					CyberEnvironment.OfflineMessages.Add(key, new List<OfflineMessage>());
				}
				CyberEnvironment.OfflineMessages[key].Add(new OfflineMessage(id, msg, ts));
			}
		}
		internal static void SaveMessage(IQueryAdapter dbClient, uint ToId, uint FromId, string Message)
		{
			dbClient.setQuery("INSERT INTO messenger_offline_messages (to_id, from_id, message, timestamp) VALUES (@tid, @fid, @msg, UNIX_TIMESTAMP())");
			dbClient.addParameter("tid", ToId);
			dbClient.addParameter("fid", FromId);
			dbClient.addParameter("msg", Message);
			dbClient.runQuery();
		}
		internal static void RemoveAllMessages(IQueryAdapter dbClient, uint ToId)
		{
			dbClient.runFastQuery("DELETE FROM messenger_offline_messages WHERE to_id=" + ToId);
		}
	}
}
