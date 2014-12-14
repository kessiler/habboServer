using Database_Manager.Database.Session_Details.Interfaces;
using Cyber.HabboHotel.GameClients;
using Cyber.HabboHotel.Users.UserDataManagement;
using Cyber.Messages;
using Cyber.Messages.Headers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace Cyber.HabboHotel.Users.Badges
{
	internal class BadgeComponent
	{
		private HybridDictionary Badges;
		private uint UserId;
		internal int Count
		{
			get
			{
				return this.Badges.Count;
			}
		}
		internal int EquippedCount
		{
			get
			{
				int num = 0;
				checked
				{
					foreach (Badge badge in this.Badges.Values)
					{
						if (badge.Slot > 0)
						{
							num++;
						}
					}
					return num;
				}
			}
		}
		internal HybridDictionary BadgeList
		{
			get
			{
				return this.Badges;
			}
		}
		internal BadgeComponent(uint userId, UserData data)
		{
			this.Badges = new HybridDictionary();
			foreach (Badge current in data.badges)
			{
                if (!this.Badges.Contains(current.Code))
				{
					this.Badges.Add(current.Code, current);
				}
			}
			this.UserId = userId;
		}
		internal Badge GetBadge(string Badge)
		{
            if (this.Badges.Contains(Badge))
			{
				return (Badge)this.Badges[Badge];
			}
			return null;
		}
		internal bool HasBadge(string Badge)
		{
            return this.Badges.Contains(Badge);
		}
		internal void GiveBadge(string Badge, bool InDatabase, GameClient Session, bool WiredReward = false)
		{
			if (WiredReward)
			{
				Session.SendMessage(this.SerializeBadgeReward(!this.HasBadge(Badge)));
			}
			if (this.HasBadge(Badge))
			{
				return;
			}
			if (InDatabase)
			{
				using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
				{
					queryreactor.setQuery(string.Concat(new object[]
					{
						"INSERT INTO user_badges (user_id,badge_id,badge_slot) VALUES (",
						this.UserId,
						",@badge,",
						0,
						")"
					}));
					queryreactor.addParameter("badge", Badge);
					queryreactor.runQuery();
				}
			}
			this.Badges.Add(Badge, new Badge(Badge, 0));
			Session.SendMessage(this.SerializeBadge(Badge));
            Session.SendMessage(this.Update(Badge));
		}
		internal ServerMessage SerializeBadge(string Badge)
		{
			ServerMessage serverMessage = new ServerMessage();
			serverMessage.Init(Outgoing.ReceiveBadgeMessageComposer);
			serverMessage.AppendInt32(1);
			serverMessage.AppendString(Badge);
			return serverMessage;
		}
		internal ServerMessage SerializeBadgeReward(bool Success)
		{
			ServerMessage serverMessage = new ServerMessage();
			serverMessage.Init(Outgoing.WiredRewardAlertMessageComposer);
			serverMessage.AppendInt32(Success ? 7 : 1);
			return serverMessage;
		}
		internal void ResetSlots()
		{
			foreach (Badge badge in this.Badges.Values)
			{
				badge.Slot = 0;
			}
		}
		internal void RemoveBadge(string Badge, GameClient Session)
		{
			if (!this.HasBadge(Badge))
			{
				return;
			}
			using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
			{
				queryreactor.setQuery("DELETE FROM user_badges WHERE badge_id = @badge AND user_id = " + this.UserId + " LIMIT 1");
				queryreactor.addParameter("badge", Badge);
				queryreactor.runQuery();
			}
			this.Badges.Remove(this.GetBadge(Badge));
			Session.SendMessage(this.Serialize());
		}
		internal ServerMessage Update(string badgeId)
		{
			ServerMessage serverMessage = new ServerMessage(Outgoing.NewInventoryObjectMessageComposer);
			serverMessage.AppendInt32(1);
			serverMessage.AppendInt32(4);
			serverMessage.AppendInt32(1);
			serverMessage.AppendString(badgeId);
			return serverMessage;
		}
		internal ServerMessage Serialize()
		{
			List<Badge> list = new List<Badge>();
			ServerMessage serverMessage = new ServerMessage(Outgoing.LoadBadgesWidgetMessageComposer);
			serverMessage.AppendInt32(this.Count);
			foreach (Badge badge in this.Badges.Values)
			{
				serverMessage.AppendInt32(1);
				serverMessage.AppendString(badge.Code);
				if (badge.Slot > 0)
				{
					list.Add(badge);
				}
			}
			serverMessage.AppendInt32(list.Count);
			foreach (Badge current in list)
			{
				serverMessage.AppendInt32(current.Slot);
				serverMessage.AppendString(current.Code);
			}
			return serverMessage;
		}
	}
}
