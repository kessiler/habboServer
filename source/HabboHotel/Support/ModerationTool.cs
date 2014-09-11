using Database_Manager.Database.Session_Details.Interfaces;
using Cyber.Collections;
using Cyber.Core;
using Cyber.HabboHotel.GameClients;
using Cyber.HabboHotel.Rooms;
using Cyber.HabboHotel.Users;
using Cyber.Messages;
using Cyber.Messages.Headers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Linq;
namespace Cyber.HabboHotel.Support
{
	public class ModerationTool
	{
		internal List<SupportTicket> Tickets;
		internal Dictionary<uint, ModerationTemplate> ModerationTemplates;
		internal List<string> UserMessagePresets;
		internal List<string> RoomMessagePresets;
		internal StringDictionary SupportTicketHints;
		internal ModerationTool()
		{
			this.Tickets = new List<SupportTicket>();
			this.UserMessagePresets = new List<string>();
			this.RoomMessagePresets = new List<string>();
			this.SupportTicketHints = new StringDictionary();
			this.ModerationTemplates = new Dictionary<uint, ModerationTemplate>();
		}
		internal ServerMessage SerializeTool()
		{
			ServerMessage serverMessage = new ServerMessage(Outgoing.LoadModerationToolMessageComposer);
			serverMessage.AppendInt32(this.Tickets.Count);
			foreach (SupportTicket current in this.Tickets)
			{
				current.Serialize(serverMessage);
			}
			serverMessage.AppendInt32(this.UserMessagePresets.Count);
			foreach (string current2 in this.UserMessagePresets)
			{
				serverMessage.AppendString(current2);
			}
			IEnumerable<ModerationTemplate> enumerable = 
				from x in this.ModerationTemplates.Values
				where x.Category == -1
				select x;
			serverMessage.AppendInt32(enumerable.Count<ModerationTemplate>());
			using (IEnumerator<ModerationTemplate> enumerator3 = enumerable.GetEnumerator())
			{
				while (enumerator3.MoveNext())
				{
					ModerationTemplate Template = enumerator3.Current;
					IEnumerable<ModerationTemplate> enumerable2 = 
						from x in this.ModerationTemplates.Values
						where (long)x.Category == (long)((ulong)Template.Id)
						select x;
					serverMessage.AppendString(Template.Caption);
					serverMessage.AppendBoolean(true);
					serverMessage.AppendInt32(enumerable2.Count<ModerationTemplate>());
					foreach (ModerationTemplate current3 in enumerable2)
					{
						serverMessage.AppendString(current3.Caption);
						serverMessage.AppendString(current3.BanMessage);
						serverMessage.AppendInt32((int)current3.BanHours);
						serverMessage.AppendInt32(CyberEnvironment.BoolToInteger(current3.AvatarBan));
						serverMessage.AppendInt32(CyberEnvironment.BoolToInteger(current3.Mute));
						serverMessage.AppendInt32(CyberEnvironment.BoolToInteger(current3.TradeLock));
						serverMessage.AppendString(current3.WarningMessage);
					}
				}
			}
			serverMessage.AppendBoolean(true);
			serverMessage.AppendBoolean(true);
			serverMessage.AppendBoolean(true);
			serverMessage.AppendBoolean(true);
			serverMessage.AppendBoolean(true);
			serverMessage.AppendBoolean(true);
			serverMessage.AppendBoolean(true);
			serverMessage.AppendInt32(this.RoomMessagePresets.Count);
			foreach (string current4 in this.RoomMessagePresets)
			{
				serverMessage.AppendString(current4);
			}
			return serverMessage;
		}
		internal void LoadMessagePresets(IQueryAdapter dbClient)
		{
			this.UserMessagePresets.Clear();
			this.RoomMessagePresets.Clear();
			this.SupportTicketHints.Clear();
			this.ModerationTemplates.Clear();
			dbClient.setQuery("SELECT type,message FROM moderation_presets WHERE enabled = 2");
			DataTable table = dbClient.getTable();
			dbClient.setQuery("SELECT word,hint FROM moderation_tickethints");
			DataTable table2 = dbClient.getTable();
			dbClient.setQuery("SELECT * FROM moderation_templates");
			DataTable table3 = dbClient.getTable();
			if (table == null || table2 == null)
			{
				return;
			}
			foreach (DataRow dataRow in table.Rows)
			{
				string item = (string)dataRow["message"];
				string a;
				if ((a = dataRow["type"].ToString().ToLower()) != null)
				{
					if (!(a == "message"))
					{
						if (a == "roommessage")
						{
							this.RoomMessagePresets.Add(item);
						}
					}
					else
					{
						this.UserMessagePresets.Add(item);
					}
				}
			}
			foreach (DataRow dataRow2 in table2.Rows)
			{
				this.SupportTicketHints.Add((string)dataRow2[0], (string)dataRow2[1]);
			}
			foreach (DataRow dataRow3 in table3.Rows)
			{
				this.ModerationTemplates.Add(uint.Parse(dataRow3["id"].ToString()), new ModerationTemplate(uint.Parse(dataRow3["id"].ToString()), short.Parse(dataRow3["category"].ToString()), dataRow3["caption"].ToString(), dataRow3["warning_message"].ToString(), dataRow3["ban_message"].ToString(), short.Parse(dataRow3["ban_hours"].ToString()), dataRow3["avatar_ban"].ToString() == "1", dataRow3["mute"].ToString() == "1", dataRow3["trade_lock"].ToString() == "1"));
			}
		}
		internal void LoadPendingTickets(IQueryAdapter dbClient)
		{
			dbClient.runFastQuery("TRUNCATE TABLE moderation_tickets");
		}

        internal void SendNewTicket(GameClient Session, int Category, uint ReportedUser, string Message, int type, List<string> Messages)
		{
            UInt32 Id = 0;

				if (Session.GetHabbo().CurrentRoomId <= 0)
				{
					using (IQueryAdapter DBClient = CyberEnvironment.GetDatabaseManager().getQueryReactor())
					{
						DBClient.setQuery(string.Concat(new object[]
						{
							"INSERT INTO moderation_tickets (score,type,status,sender_id,reported_id,moderator_id,message,room_id,room_name,timestamp) VALUES (1,'",
							Category,
							"','open','",
							Session.GetHabbo().Id,
							"','",
							ReportedUser,
							"','0',@message,'0','','",
							CyberEnvironment.GetUnixTimestamp(),
							"')"
						}));
						DBClient.addParameter("message", Message);
						Id = (uint)DBClient.insertQuery();
						DBClient.runFastQuery("UPDATE user_info SET cfhs = cfhs + 1 WHERE user_id = " + Session.GetHabbo().Id);
					}

					SupportTicket Ticket = new SupportTicket(Id, 1, type, Session.GetHabbo().Id, ReportedUser, Message, 0u, "", (double)CyberEnvironment.GetUnixTimestamp(), Messages);
                    this.Tickets.Add(Ticket);
                    ModerationTool.SendTicketToModerators(Ticket);
					return;
				}

				RoomData Data = CyberEnvironment.GetGame().GetRoomManager().GenerateNullableRoomData(Session.GetHabbo().CurrentRoomId);
				using (IQueryAdapter DBClient = CyberEnvironment.GetDatabaseManager().getQueryReactor())
				{
					DBClient.setQuery(string.Concat(new object[]
					{
						"INSERT INTO moderation_tickets (score,type,status,sender_id,reported_id,moderator_id,message,room_id,room_name,timestamp) VALUES (1,'",
						Category,
						"','open','",
						Session.GetHabbo().Id,
						"','",
						ReportedUser,
						"','0',@message,'",
						Data.Id,
						"',@name,'",
						CyberEnvironment.GetUnixTimestamp(),
						"')"
					}));
					DBClient.addParameter("message", Message);
					DBClient.addParameter("name", Data.Name);
					Id = (uint)DBClient.insertQuery();
					DBClient.runFastQuery("UPDATE user_info SET cfhs = cfhs + 1 WHERE user_id = " + Session.GetHabbo().Id);
				}
				SupportTicket Ticket2 = new SupportTicket(Id, 1, type, Session.GetHabbo().Id, ReportedUser, Message, 0u, "", (double)CyberEnvironment.GetUnixTimestamp(), Messages);
                this.Tickets.Add(Ticket2);
                ModerationTool.SendTicketToModerators(Ticket2);
		}

		internal void SerializeOpenTickets(ref QueuedServerMessage serverMessages, uint userID)
		{
			ServerMessage message = new ServerMessage(Outgoing.ModerationToolIssueMessageComposer);
			foreach (SupportTicket current in this.Tickets)
			{
				if (current.Status == TicketStatus.OPEN || (current.Status == TicketStatus.PICKED && current.ModeratorId == userID) || (current.Status == TicketStatus.PICKED && current.ModeratorId == 0u))
				{
					message = current.Serialize(message);
					serverMessages.appendResponse(message);
				}
			}
		}
		internal SupportTicket GetTicket(uint TicketId)
		{
			foreach (SupportTicket current in this.Tickets)
			{
				if (current.TicketId == TicketId)
				{
					return current;
				}
			}
			return null;
		}
		internal void PickTicket(GameClient Session, uint TicketId)
		{
			SupportTicket ticket = this.GetTicket(TicketId);
			if (ticket == null || ticket.Status != TicketStatus.OPEN)
			{
				return;
			}
			ticket.Pick(Session.GetHabbo().Id, true);
			ModerationTool.SendTicketToModerators(ticket);
		}
		internal void ReleaseTicket(GameClient Session, uint TicketId)
		{
			SupportTicket ticket = this.GetTicket(TicketId);
			if (ticket == null || ticket.Status != TicketStatus.PICKED || ticket.ModeratorId != Session.GetHabbo().Id)
			{
				return;
			}
			ticket.Release(true);
			ModerationTool.SendTicketToModerators(ticket);
		}
		internal void CloseTicket(GameClient Session, uint TicketId, int Result)
		{
			SupportTicket ticket = this.GetTicket(TicketId);
			if (ticket == null || ticket.Status != TicketStatus.PICKED || ticket.ModeratorId != Session.GetHabbo().Id)
			{
				return;
			}
			GameClient clientByUserID = CyberEnvironment.GetGame().GetClientManager().GetClientByUserID(ticket.SenderId);
			int i = 0;
			TicketStatus newStatus;
			switch (Result)
			{
			case 1:
				i = 1;
				newStatus = TicketStatus.INVALID;
				goto IL_9E;
			case 2:
				i = 2;
				newStatus = TicketStatus.ABUSIVE;
				using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
				{
					queryreactor.runFastQuery("UPDATE user_info SET cfhs_abusive = cfhs_abusive + 1 WHERE user_id = " + ticket.SenderId);
					goto IL_9E;
				}
			}
			i = 0;
			newStatus = TicketStatus.RESOLVED;
			IL_9E:
            if (clientByUserID != null && (ticket.Type != 3 && ticket.Type != 4))
			{
				clientByUserID.GetMessageHandler().GetResponse().Init(Outgoing.ModerationToolUpdateIssueMessageComposer);
				clientByUserID.GetMessageHandler().GetResponse().AppendInt32(1);
				clientByUserID.GetMessageHandler().GetResponse().AppendUInt(ticket.TicketId);
				clientByUserID.GetMessageHandler().GetResponse().AppendUInt(ticket.ModeratorId);
				clientByUserID.GetMessageHandler().GetResponse().AppendString((CyberEnvironment.getHabboForId(ticket.ModeratorId) != null) ? CyberEnvironment.getHabboForId(ticket.ModeratorId).Username : "Undefined");
				clientByUserID.GetMessageHandler().GetResponse().AppendBoolean(false);
				clientByUserID.GetMessageHandler().GetResponse().AppendInt32(0);
				clientByUserID.GetMessageHandler().GetResponse().Init(Outgoing.ModerationTicketResponseMessageComposer);
				clientByUserID.GetMessageHandler().GetResponse().AppendInt32(i);
				clientByUserID.GetMessageHandler().SendResponse();
			}
			using (IQueryAdapter queryreactor2 = CyberEnvironment.GetDatabaseManager().getQueryReactor())
			{
				queryreactor2.runFastQuery("UPDATE user_stats SET tickets_answered = tickets_answered+1 WHERE id=" + Session.GetHabbo().Id + " LIMIT 1");
			}
			ticket.Close(newStatus, true);
			ModerationTool.SendTicketToModerators(ticket);
		}
		internal bool UsersHasPendingTicket(uint Id)
		{
			foreach (SupportTicket current in this.Tickets)
			{
				if (current.SenderId == Id && current.Status == TicketStatus.OPEN)
				{
					return true;
				}
			}
			return false;
		}
		internal void DeletePendingTicketForUser(uint Id)
		{
			foreach (SupportTicket current in this.Tickets)
			{
				if (current.SenderId == Id)
				{
					current.Delete(true);
					ModerationTool.SendTicketToModerators(current);
					break;
				}
			}
		}
		internal static void SendTicketUpdateToModerators(SupportTicket Ticket)
		{
		}
		internal static void SendTicketToModerators(SupportTicket Ticket)
		{
			ServerMessage message = new ServerMessage(Outgoing.ModerationToolIssueMessageComposer);
			message = Ticket.Serialize(message);
            CyberEnvironment.GetGame().GetClientManager().StaffAlert(message);
		}
		internal void LogStaffEntry(string modName, string target, string type, string description)
		{
			using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
			{
				queryreactor.setQuery("INSERT INTO staff_logs (staffuser,target,action_type,description) VALUES (@username,@target,@type,@desc)");
				queryreactor.addParameter("username", modName);
				queryreactor.addParameter("target", target);
				queryreactor.addParameter("type", type);
				queryreactor.addParameter("desc", description);
				queryreactor.runQuery();
			}
		}
		internal static void PerformRoomAction(GameClient ModSession, uint RoomId, bool KickUsers, bool LockRoom, bool InappropriateRoom, ServerMessage Message)
		{
			Room room = CyberEnvironment.GetGame().GetRoomManager().GetRoom(RoomId);
			if (room == null)
			{
				return;
			}
			if (LockRoom)
			{
				room.State = 1;
				using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
				{
					queryreactor.runFastQuery("UPDATE rooms SET state = 'locked' WHERE id = " + room.RoomId);
				}
			}
			if (InappropriateRoom)
			{
                room.Name = "Inappropiate to Hotel Management.";
				room.Description = "Your room description is not allowed.";
				room.ClearTags();
                room.RoomData.SerializeRoomData(Message, false, ModSession, true);
			}
			if (KickUsers)
			{
                room.onRoomKick();
			}
		}
		internal static ServerMessage SerializeRoomTool(RoomData Data)
		{
			Room room = CyberEnvironment.GetGame().GetRoomManager().GetRoom(Data.Id);
			ServerMessage serverMessage = new ServerMessage(Outgoing.ModerationRoomToolMessageComposer);
			serverMessage.AppendUInt(Data.Id);
			serverMessage.AppendInt32(Data.UsersNow);
			if (room != null)
			{
				serverMessage.AppendBoolean(room.GetRoomUserManager().GetRoomUserByHabbo(Data.Owner) != null);
			}
			else
			{
				serverMessage.AppendBoolean(false);
			}
			serverMessage.AppendInt32(room.OwnerId);
			serverMessage.AppendString(Data.Owner);
			serverMessage.AppendBoolean(room != null);
			serverMessage.AppendString(Data.Name);
			serverMessage.AppendString(Data.Description);
			serverMessage.AppendInt32(Data.TagCount);
			foreach (string current in Data.Tags)
			{
				serverMessage.AppendString(current);
			}
			serverMessage.AppendBoolean(false);
			return serverMessage;
		}
		internal static void KickUser(GameClient ModSession, uint UserId, string Message, bool Soft)
		{
			GameClient clientByUserID = CyberEnvironment.GetGame().GetClientManager().GetClientByUserID(UserId);
			if (clientByUserID == null || clientByUserID.GetHabbo().CurrentRoomId < 1u || clientByUserID.GetHabbo().Id == ModSession.GetHabbo().Id)
			{
				return;
			}
			if (clientByUserID.GetHabbo().Rank >= ModSession.GetHabbo().Rank)
			{
				ModSession.SendNotif("You are not allowed to kick him/her.");
				return;
			}
			Room room = CyberEnvironment.GetGame().GetRoomManager().GetRoom(clientByUserID.GetHabbo().CurrentRoomId);
			if (room == null)
			{
				return;
			}
			room.GetRoomUserManager().RemoveUserFromRoom(clientByUserID, true, false);
			clientByUserID.CurrentRoomUserID = -1;
			if (!Soft)
			{
				clientByUserID.SendNotif(Message);
				using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
				{
					queryreactor.runFastQuery("UPDATE user_info SET cautions = cautions + 1 WHERE user_id = " + UserId);
				}
			}
		}
		internal static void AlertUser(GameClient ModSession, uint UserId, string Message, bool Caution)
		{
			GameClient clientByUserID = CyberEnvironment.GetGame().GetClientManager().GetClientByUserID(UserId);
			if (clientByUserID == null)
			{
				return;
			}
			clientByUserID.SendNotif(Message);
		}
		internal static void LockTrade(GameClient ModSession, uint UserId, string Message, int Length)
		{
			GameClient clientByUserID = CyberEnvironment.GetGame().GetClientManager().GetClientByUserID(UserId);
			if (clientByUserID == null)
			{
				return;
			}
			int num = Length;
			checked
			{
				if (!clientByUserID.GetHabbo().CheckTrading())
				{
					num += CyberEnvironment.GetUnixTimestamp() - clientByUserID.GetHabbo().TradeLockExpire;
				}
				clientByUserID.GetHabbo().TradeLocked = true;
				clientByUserID.GetHabbo().TradeLockExpire = CyberEnvironment.GetUnixTimestamp() + num;
				clientByUserID.SendNotif(Message);
				using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
				{
					queryreactor.runFastQuery("UPDATE users SET trade_lock = '1', trade_lock_expire = '" + clientByUserID.GetHabbo().TradeLockExpire + "'");
				}
			}
		}
		internal static void BanUser(GameClient ModSession, uint UserId, int Length, string Message)
		{
			GameClient clientByUserID = CyberEnvironment.GetGame().GetClientManager().GetClientByUserID(UserId);
			if (clientByUserID == null || clientByUserID.GetHabbo().Id == ModSession.GetHabbo().Id)
			{
				return;
			}
			if (clientByUserID.GetHabbo().Rank >= ModSession.GetHabbo().Rank)
			{
				ModSession.SendNotif("No tienes los permisos para banear");
				return;
			}
			double lengthSeconds = (double)Length;
			CyberEnvironment.GetGame().GetBanManager().BanUser(clientByUserID, ModSession.GetHabbo().Username, lengthSeconds, Message, false, false);
		}
		internal static ServerMessage SerializeUserInfo(uint UserId)
		{
			checked
			{
				ServerMessage result;
				using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
				{
					queryreactor.setQuery("SELECT id, username, online, mail, ip_last, look , rank , trade_lock , trade_lock_expire FROM users WHERE id = " + UserId);
					DataRow row = queryreactor.getRow();
					queryreactor.setQuery("SELECT reg_timestamp, login_timestamp, cfhs, cfhs_abusive, cautions, bans FROM user_info WHERE user_id = " + UserId);
					DataRow row2 = queryreactor.getRow();
					if (row == null)
					{
						throw new NullReferenceException("User not found in database.");
					}
					ServerMessage serverMessage = new ServerMessage(Outgoing.ModerationToolUserToolMessageComposer);
					serverMessage.AppendUInt(Convert.ToUInt32(row["id"]));
					serverMessage.AppendString((string)row["username"]);
					serverMessage.AppendString((string)row["look"]);
					if (row2 != null)
					{
						serverMessage.AppendInt32((int)Math.Ceiling(unchecked((double)CyberEnvironment.GetUnixTimestamp() - (double)row2["reg_timestamp"]) / 60.0));
						serverMessage.AppendInt32((int)Math.Ceiling(unchecked((double)CyberEnvironment.GetUnixTimestamp() - (double)row2["login_timestamp"]) / 60.0));
					}
					else
					{
						serverMessage.AppendInt32(0);
						serverMessage.AppendInt32(0);
					}
					serverMessage.AppendBoolean(CyberEnvironment.GetGame().GetClientManager().GetClientByUserID(Convert.ToUInt32(row["id"])) != null);
					if (row2 != null)
					{
						serverMessage.AppendInt32((int)row2["cfhs"]);
						serverMessage.AppendInt32((int)row2["cfhs_abusive"]);
						serverMessage.AppendInt32((int)row2["cautions"]);
						serverMessage.AppendInt32((int)row2["bans"]);
					}
					else
					{
						serverMessage.AppendInt32(0);
						serverMessage.AppendInt32(0);
						serverMessage.AppendInt32(0);
						serverMessage.AppendInt32(0);
					}
					serverMessage.AppendInt32(0);
					serverMessage.AppendString((row["trade_lock"].ToString() == "1") ? CyberEnvironment.UnixToDateTime((double)int.Parse(row["trade_lock_expire"].ToString())).ToLongDateString() : "Not trade-locked");
					serverMessage.AppendString(((uint)row["rank"] < 6u) ? ((string)row["ip_last"]) : "127.0.0.1");
					serverMessage.AppendUInt(Convert.ToUInt32(row["id"]));
					serverMessage.AppendInt32(0);
					serverMessage.AppendString("E-Mail:         " + row["mail"].ToString());
					serverMessage.AppendString("Rank ID:        " + (uint)row["rank"]);
					result = serverMessage;
				}
				return result;
			}
		}
		internal static ServerMessage SerializeRoomVisits(uint UserId)
		{
			ServerMessage result;
			using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
			{
				queryreactor.setQuery("SELECT room_id,hour,minute FROM user_roomvisits WHERE user_id = " + UserId + " ORDER BY entry_timestamp DESC LIMIT 50");
				DataTable table = queryreactor.getTable();
				ServerMessage serverMessage = new ServerMessage(Outgoing.ModerationToolRoomVisitsMessageComposer);
				serverMessage.AppendUInt(UserId);
				serverMessage.AppendString(CyberEnvironment.GetGame().GetClientManager().GetNameById(UserId));
				if (table != null)
				{
					serverMessage.AppendInt32(table.Rows.Count);
					IEnumerator enumerator = table.Rows.GetEnumerator();
					try
					{
						while (enumerator.MoveNext())
						{
							DataRow dataRow = (DataRow)enumerator.Current;
							RoomData roomData = CyberEnvironment.GetGame().GetRoomManager().GenerateNullableRoomData(Convert.ToUInt32(dataRow["room_id"]));
							serverMessage.AppendBoolean(false);
							serverMessage.AppendUInt(roomData.Id);
							serverMessage.AppendString(roomData.Name);
							serverMessage.AppendInt32((int)dataRow["hour"]);
							serverMessage.AppendInt32((int)dataRow["minute"]);
						}
						goto IL_120;
					}
					finally
					{
						IDisposable disposable = enumerator as IDisposable;
						if (disposable != null)
						{
							disposable.Dispose();
						}
					}
				}
				serverMessage.AppendInt32(0);
				IL_120:
				result = serverMessage;
			}
			return result;
		}

        internal static ServerMessage SerializeUserChatlog(uint UserId)
        {
            ServerMessage result;
            using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
            {
                queryreactor.setQuery("SELECT DISTINCT room_id FROM chatlogs WHERE user_id = " + UserId + " ORDER BY timestamp DESC LIMIT 4");
                DataTable table = queryreactor.getTable();
                ServerMessage serverMessage = new ServerMessage(Outgoing.ModerationToolUserChatlogMessageComposer);
                serverMessage.AppendUInt(UserId);
                serverMessage.AppendString(CyberEnvironment.GetGame().GetClientManager().GetNameById(UserId));
                if (table != null)
                {
                    serverMessage.AppendInt32(table.Rows.Count);
                    IEnumerator enumerator = table.Rows.GetEnumerator();
                    try
                    {
                        while (enumerator.MoveNext())
                        {
                            DataRow dataRow = (DataRow)enumerator.Current;
                            queryreactor.setQuery(string.Concat(new object[]
							{
								"SELECT user_id,timestamp,message FROM chatlogs WHERE room_id = ",
								(uint)dataRow["room_id"],
								" AND user_id = ",
								UserId,
								" ORDER BY timestamp DESC LIMIT 30"
							}));
                            DataTable table2 = queryreactor.getTable();
                            RoomData roomData = CyberEnvironment.GetGame().GetRoomManager().GenerateRoomData((uint)dataRow["room_id"]);
                            if (table2 != null)
                            {
                                serverMessage.AppendByte(1);
                                serverMessage.AppendShort(2);
                                serverMessage.AppendString("roomName");
                                serverMessage.AppendByte(2);
                                if (roomData == null)
                                {
                                    serverMessage.AppendString("This room was deleted");
                                }
                                else
                                {
                                    serverMessage.AppendString(roomData.Name);
                                }
                                serverMessage.AppendString("roomId");
                                serverMessage.AppendByte(1);
                                serverMessage.AppendUInt((uint)dataRow["room_id"]);
                                serverMessage.AppendShort(table2.Rows.Count);
                                IEnumerator enumerator2 = table2.Rows.GetEnumerator();
                                try
                                {
                                    while (enumerator2.MoveNext())
                                    {
                                        DataRow dataRow2 = (DataRow)enumerator2.Current;
                                        Habbo habboForId = CyberEnvironment.getHabboForId((uint)dataRow2["user_id"]);
                                        CyberEnvironment.UnixToDateTime((double)dataRow2["timestamp"]);
                                        if (habboForId == null)
                                        {
                                            result = null;
                                            return result;
                                        }
                                        serverMessage.AppendInt32(checked((int)unchecked((double)CyberEnvironment.GetUnixTimestamp() - (double)dataRow2["timestamp"])));
                                        serverMessage.AppendUInt(habboForId.Id);
                                        serverMessage.AppendString(habboForId.Username);
                                        serverMessage.AppendString(dataRow2["message"].ToString());
                                        serverMessage.AppendBoolean(false);
                                    }
                                    continue;
                                }
                                finally
                                {
                                    IDisposable disposable = enumerator2 as IDisposable;
                                    if (disposable != null)
                                    {
                                        disposable.Dispose();
                                    }
                                }
                            }
                            serverMessage.AppendByte(1);
                            serverMessage.AppendShort(0);
                            serverMessage.AppendShort(0);
                        }
                        goto IL_29B;
                    }
                    finally
                    {
                        IDisposable disposable2 = enumerator as IDisposable;
                        if (disposable2 != null)
                        {
                            disposable2.Dispose();
                        }
                    }
                }
                serverMessage.AppendInt32(0);
            IL_29B:
                result = serverMessage;
            }
            return result;
        }
    
		internal static ServerMessage SerializeTicketChatlog(SupportTicket Ticket, RoomData RoomData, double Timestamp)
		{
			ServerMessage result;
			using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
			{
				queryreactor.setQuery(string.Concat(new object[]
				{
					"SELECT user_id,timestamp,message FROM chatlogs WHERE room_id = ",
					RoomData.Id,
					" AND (timestamp >= ",
					Timestamp - 300.0,
					" AND timestamp <= ",
					Timestamp,
					") OR (timestamp >= ",
					Timestamp - 300.0,
					" AND timestamp = 0) ORDER BY timestamp DESC LIMIT 150"
				}));
				DataTable table = queryreactor.getTable();
				ServerMessage serverMessage = new ServerMessage(Outgoing.ModerationToolIssueChatlogMessageComposer);
				serverMessage.AppendUInt(Ticket.TicketId);
				serverMessage.AppendUInt(Ticket.SenderId);
				serverMessage.AppendUInt(Ticket.ReportedId);
				serverMessage.AppendUInt(RoomData.Id);
				serverMessage.AppendBoolean(false);
				serverMessage.AppendUInt(RoomData.Id);
				serverMessage.AppendString(RoomData.Name);
				if (table != null)
				{
					serverMessage.AppendInt32(table.Rows.Count);
					IEnumerator enumerator = table.Rows.GetEnumerator();
					try
					{
						while (enumerator.MoveNext())
						{
							DataRow dataRow = (DataRow)enumerator.Current;
							Habbo habboForId = CyberEnvironment.getHabboForId(Convert.ToUInt32(dataRow["user_id"]));
							serverMessage.AppendInt32(CyberEnvironment.UnixToDateTime(Convert.ToDouble(dataRow["timestamp"])).Minute);
							serverMessage.AppendUInt(habboForId.Id);
							serverMessage.AppendString(habboForId.Username);
							serverMessage.AppendString((string)dataRow["message"]);
						}
						goto IL_1B8;
					}
					finally
					{
						IDisposable disposable = enumerator as IDisposable;
						if (disposable != null)
						{
							disposable.Dispose();
						}
					}
				}
				serverMessage.AppendInt32(0);
				IL_1B8:
				result = serverMessage;
			}
			return result;
		}

		internal static ServerMessage SerializeRoomChatlog(uint roomID)
		{
            // NEW CHATLOGS [March 2014] Coded by Finn
            // Please don't remove credits, this took me some time to do... :(
            // Credits to Itachi for the structure's "context" enigma :D

			ServerMessage Message = new ServerMessage();
            RoomData Room = CyberEnvironment.GetGame().GetRoomManager().GenerateRoomData(roomID);
			if (Room == null)
			{
                throw new NullReferenceException("No room found.");
			}

			Message.Init(Outgoing.ModerationToolRoomChatlogMessageComposer);
			Message.AppendByte(1);
			Message.AppendShort(2);
			Message.AppendString("roomName");
			Message.AppendByte(2);
			Message.AppendString(Room.Name);
			Message.AppendString("roomId");
			Message.AppendByte(1);
			Message.AppendUInt(Room.Id);


            var TempChatlogs = Room.RoomChat.Reverse().Take(60);
            Message.AppendShort(TempChatlogs.Count());
            foreach (Chatlog current in TempChatlogs)
			{
				Habbo Habbo = CyberEnvironment.getHabboForId(current.UserId);
				DateTime Date = CyberEnvironment.UnixToDateTime(current.Timestamp);
                if (Habbo == null)
                {
                    Message.AppendInt32((DateTime.Now - Date).Seconds);
                    Message.AppendUInt(current.UserId);
                    Message.AppendString("*User not found*");
                    Message.AppendString(current.Message);
                    Message.AppendBoolean(true);
                }
                else
                {
                    Message.AppendInt32((DateTime.Now - Date).Seconds);
                    Message.AppendUInt(Habbo.Id);
                    Message.AppendString(Habbo.Username);
                    Message.AppendString(current.Message);
                    Message.AppendBoolean(false); // Text is bold
                }
			}
            TempChatlogs = null;

            return Message;
		}
	}
}
