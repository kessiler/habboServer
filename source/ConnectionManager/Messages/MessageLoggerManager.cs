using ConnectionManager.LoggingSystem;
using Database_Manager.Database;
using Database_Manager.Database.Session_Details.Interfaces;
using System;
using System.Collections;
using System.Text;
namespace ConnectionManager.Messages
{
	internal class MessageLoggerManager
	{
		private static Queue loggedMessages;
		private static bool enabled;
		private static DateTime timeSinceLastPacket;
		internal static bool Enabled
		{
			get
			{
				return MessageLoggerManager.enabled;
			}
			set
			{
				MessageLoggerManager.enabled = value;
				if (MessageLoggerManager.enabled)
				{
					MessageLoggerManager.loggedMessages = new Queue();
				}
			}
		}
		internal static void AddMessage(byte[] data, int connectionID, LogState state)
		{
			if (!MessageLoggerManager.enabled)
			{
				return;
			}
			string data2;
			switch (state)
			{
			case LogState.ConnectionOpen:
				data2 = "CONCLOSE";
				break;
			case LogState.ConnectionClose:
				data2 = "CONOPEN";
				break;
			default:
				data2 = Encoding.Default.GetString(data);
				break;
			}
			lock (MessageLoggerManager.loggedMessages.SyncRoot)
			{
				Message message = new Message(connectionID, MessageLoggerManager.GenerateTimestamp(), data2);
				MessageLoggerManager.loggedMessages.Enqueue(message);
			}
		}
		private static int GenerateTimestamp()
		{
			DateTime now = DateTime.Now;
			TimeSpan timeSpan = now - MessageLoggerManager.timeSinceLastPacket;
			MessageLoggerManager.timeSinceLastPacket = now;
			return checked((int)timeSpan.TotalMilliseconds);
		}
		internal static void Save()
		{
			if (!MessageLoggerManager.enabled)
			{
				return;
			}
			lock (MessageLoggerManager.loggedMessages.SyncRoot)
			{
				int arg_28_0 = MessageLoggerManager.loggedMessages.Count;
				if (MessageLoggerManager.loggedMessages.Count > 0)
				{
					DatabaseManager databaseManager = new DatabaseManager(1u, 1u);
					using (IQueryAdapter queryreactor = databaseManager.getQueryreactor())
					{
						while (MessageLoggerManager.loggedMessages.Count > 0)
						{
							Message message = (Message)MessageLoggerManager.loggedMessages.Dequeue();
							queryreactor.setQuery("INSERT INTO system_packetlog (connectionid, timestamp, data) VALUES @connectionid @timestamp, @data");
							queryreactor.addParameter("connectionid", message.ConnectionID);
							queryreactor.addParameter("timestamp", message.GetTimestamp);
							queryreactor.addParameter("data", message.GetData);
							queryreactor.runQuery();
						}
					}
				}
			}
		}
	}
}
