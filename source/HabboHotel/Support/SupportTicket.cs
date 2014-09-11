using Database_Manager.Database.Session_Details.Interfaces;
using Cyber.Core;
using Cyber.Messages;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace Cyber.HabboHotel.Support
{
	internal class SupportTicket
	{
		private uint Id;
		internal int Score;
		internal int Type;
		internal TicketStatus Status;
		internal uint SenderId;
		internal uint ReportedId;
		internal uint ModeratorId;
		internal string Message;
		internal uint RoomId;
		internal string RoomName;
		internal double Timestamp;
        internal List<string> ReportedChats;
		private string SenderName;
		private string ReportedName;
		private string ModName;
		internal int TabId
		{
			get
			{
				if (this.Status == TicketStatus.OPEN)
				{
					return 1;
				}
				if (this.Status == TicketStatus.PICKED)
				{
					return 2;
				}
				if (this.Status == TicketStatus.ABUSIVE || this.Status == TicketStatus.INVALID || this.Status == TicketStatus.RESOLVED)
				{
					return 0;
				}
				if (this.Status == TicketStatus.DELETED)
				{
					return 0;
				}
				return 0;
			}
		}
		internal uint TicketId
		{
			get
			{
				return this.Id;
			}
		}

		internal SupportTicket(uint Id, int Score, int Type, uint SenderId, uint ReportedId, string Message, uint RoomId, string RoomName, double Timestamp, List<string> ReportedChats)
		{
			this.Id = Id;
			this.Score = Score;
			this.Type = Type;
			this.Status = TicketStatus.OPEN;
			this.SenderId = SenderId;
			this.ReportedId = ReportedId;
			this.ModeratorId = 0u;
			this.Message = Message;
			this.RoomId = RoomId;
			this.RoomName = RoomName;
			this.Timestamp = Timestamp;
			this.SenderName = CyberEnvironment.GetGame().GetClientManager().GetNameById(SenderId);
			this.ReportedName = CyberEnvironment.GetGame().GetClientManager().GetNameById(ReportedId);
			this.ModName = CyberEnvironment.GetGame().GetClientManager().GetNameById(this.ModeratorId);
            this.ReportedChats = ReportedChats;
		}
		internal void Pick(uint pModeratorId, bool UpdateInDb)
		{
			this.Status = TicketStatus.PICKED;
			this.ModeratorId = pModeratorId;
			this.ModName = CyberEnvironment.getHabboForId(pModeratorId).Username;
			if (UpdateInDb)
			{
				using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
				{
					queryreactor.runFastQuery(string.Concat(new object[]
					{
						"UPDATE moderation_tickets SET status = 'picked', moderator_id = ",
						pModeratorId,
						", timestamp = '",
						CyberEnvironment.GetUnixTimestamp(),
						"' WHERE id = ",
						this.Id
					}));
				}
			}
		}
		internal void Close(TicketStatus NewStatus, bool UpdateInDb)
		{
			this.Status = NewStatus;
			if (UpdateInDb)
			{
				string text;
				switch (NewStatus)
				{
				case TicketStatus.ABUSIVE:
					text = "abusive";
					goto IL_41;
				case TicketStatus.INVALID:
					text = "invalid";
					goto IL_41;
				}
				text = "resolved";
				IL_41:
				using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
				{
					queryreactor.runFastQuery(string.Concat(new object[]
					{
						"UPDATE moderation_tickets SET status = '",
						text,
						"' WHERE id = ",
						this.Id
					}));
				}
			}
		}
		internal void Release(bool UpdateInDb)
		{
			this.Status = TicketStatus.OPEN;
			if (UpdateInDb)
			{
				using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
				{
					queryreactor.runFastQuery("UPDATE moderation_tickets SET status = 'open' WHERE id = " + this.Id);
				}
			}
		}
		internal void Delete(bool UpdateInDb)
		{
			this.Status = TicketStatus.DELETED;
			if (UpdateInDb)
			{
				using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
				{
					queryreactor.runFastQuery("UPDATE moderation_tickets SET status = 'deleted' WHERE id = " + this.Id);
				}
			}
		}

		internal ServerMessage Serialize(ServerMessage Message)
		{
			Message.AppendUInt(this.Id);
			Message.AppendInt32(this.TabId);
			Message.AppendInt32(this.Type);
			Message.AppendInt32(114); // Category
			checked
			{
				Message.AppendInt32((CyberEnvironment.GetUnixTimestamp() - (int)this.Timestamp) * 1000);
				Message.AppendInt32(this.Score);
				Message.AppendInt32(0);
				Message.AppendUInt(this.SenderId);
				Message.AppendString(this.SenderName);
				Message.AppendUInt(this.ReportedId);
				Message.AppendString(this.ReportedName);
				Message.AppendUInt((this.Status == TicketStatus.PICKED) ? this.ModeratorId : 0u);
				Message.AppendString(this.ModName);
				Message.AppendString(this.Message);
				Message.AppendInt32(0);

                Message.AppendInt32(this.ReportedChats.Count);
                
                    foreach (string str in ReportedChats)
                    {
                        Message.AppendString(str);
                        Message.AppendInt32(-1);
                        Message.AppendInt32(-1);
                    }
				return Message;
			}
		}
	}
}
