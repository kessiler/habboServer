using Database_Manager.Database.Session_Details.Interfaces;
using Cyber.Core;
using Cyber.HabboHotel.GameClients;
using Cyber.Messages;
using Cyber.Messages.Headers;
using System;
using System.Collections.Generic;
using System.Data;
namespace Cyber.HabboHotel.Rooms
{
	internal class RoomEvents
	{
		private Dictionary<uint, RoomEvent> Events;
		internal RoomEvents()
		{
			this.Events = new Dictionary<uint, RoomEvent>();
			using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
			{
				queryreactor.setQuery("SELECT * FROM room_events WHERE `expire` > UNIX_TIMESTAMP()");
				DataTable table = queryreactor.getTable();
				foreach (DataRow dataRow in table.Rows)
				{
					this.Events.Add((uint)dataRow[0], new RoomEvent((uint)dataRow[0], dataRow[1].ToString(), dataRow[2].ToString(), (int)dataRow[3]));
				}
			}
		}
		internal void AddNewEvent(uint RoomId, string EventName, string EventDesc, GameClient Session, int Time = 7200)
		{
			checked
			{
				if (this.Events.ContainsKey(RoomId))
				{
					RoomEvent roomEvent = this.Events[RoomId];
					roomEvent.Name = EventName;
					roomEvent.Description = EventDesc;
					if (roomEvent.HasExpired)
					{
						roomEvent.Time = CyberEnvironment.GetUnixTimestamp() + Time;
					}
					else
					{
						roomEvent.Time += Time;
					}
					using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
					{
						queryreactor.setQuery(string.Concat(new object[]
						{
							"REPLACE INTO room_events VALUES (",
							RoomId,
							", @name, @desc, ",
							roomEvent.Time,
							")"
						}));
						queryreactor.addParameter("name", EventName);
						queryreactor.addParameter("desc", EventDesc);
						queryreactor.runQuery();
						goto IL_17C;
					}
				}
				using (IQueryAdapter queryreactor2 = CyberEnvironment.GetDatabaseManager().getQueryReactor())
				{
					queryreactor2.setQuery(string.Concat(new object[]
					{
						"REPLACE INTO room_events VALUES (",
						RoomId,
						", @name, @desc, ",
						CyberEnvironment.GetUnixTimestamp() + 7200,
						")"
					}));
					queryreactor2.addParameter("name", EventName);
					queryreactor2.addParameter("desc", EventDesc);
					queryreactor2.runQuery();
				}
				this.Events.Add(RoomId, new RoomEvent(RoomId, EventName, EventDesc, 0));
				IL_17C:
				CyberEnvironment.GetGame().GetRoomManager().GenerateRoomData(RoomId).Event = this.Events[RoomId];
				Room room = CyberEnvironment.GetGame().GetRoomManager().GetRoom(RoomId);
				if (room != null)
				{
					room.Event = this.Events[RoomId];
				}
				if (Session.GetHabbo().CurrentRoomId == RoomId)
				{
					this.SerializeEventInfo(RoomId);
				}
			}
		}
		internal void RemoveEvent(uint RoomId)
		{
			this.Events.Remove(RoomId);
			this.SerializeEventInfo(RoomId);
		}
		internal Dictionary<uint, RoomEvent> GetEvents()
		{
			return this.Events;
		}
		internal RoomEvent GetEvent(uint RoomId)
		{
			if (this.Events.ContainsKey(RoomId))
			{
				return this.Events[RoomId];
			}
			return null;
		}
		internal bool RoomHasEvents(uint RoomId)
		{
			return this.Events.ContainsKey(RoomId);
		}
		internal void SerializeEventInfo(uint RoomId)
		{
			Room room = CyberEnvironment.GetGame().GetRoomManager().GetRoom(RoomId);
			if (room == null)
			{
				return;
			}
			RoomEvent @event = this.GetEvent(RoomId);
			if (@event != null && !@event.HasExpired)
			{
				if (!this.RoomHasEvents(RoomId))
				{
					return;
				}
				ServerMessage serverMessage = new ServerMessage();
				serverMessage.Init(Outgoing.RoomEventMessageComposer);
				serverMessage.AppendUInt(RoomId);
				serverMessage.AppendInt32(room.OwnerId);
				serverMessage.AppendString(room.Owner);
				serverMessage.AppendInt32(1);
				serverMessage.AppendInt32(1);
				serverMessage.AppendString(@event.Name);
				serverMessage.AppendString(@event.Description);
				serverMessage.AppendInt32(0);
				serverMessage.AppendInt32(checked((int)Math.Floor((double)(@event.Time - CyberEnvironment.GetUnixTimestamp()) / 60.0)));
				room.SendMessage(serverMessage);
			}
		}
		internal void UpdateEvent(RoomEvent Event)
		{
			using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
			{
				queryreactor.setQuery(string.Concat(new object[]
				{
					"REPLACE INTO room_events VALUES (",
					Event.RoomId,
					", @name, @desc, ",
					Event.Time,
					")"
				}));
				queryreactor.addParameter("name", Event.Name);
				queryreactor.addParameter("desc", Event.Description);
				queryreactor.runQuery();
			}
			this.SerializeEventInfo(Event.RoomId);
		}
	}
}
