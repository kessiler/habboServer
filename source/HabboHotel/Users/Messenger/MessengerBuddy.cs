using Cyber.HabboHotel.GameClients;
using Cyber.HabboHotel.Rooms;
using Cyber.HabboHotel.Users.Relationships;
using Cyber.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Cyber.HabboHotel.Users.Messenger
{
	internal class MessengerBuddy
	{
		private readonly uint UserId;
		internal string mUsername;
		private readonly string mLook;
		private readonly string mMotto;
		private readonly int mLastOnline;
		private readonly bool mAppearOffline;
		private readonly bool mHideInroom;
		internal GameClient client;
		private Room currentRoom;
		internal uint Id
		{
			get
			{
				return this.UserId;
			}
		}
		internal bool IsOnline
		{
			get
			{
				return this.client != null && this.client.GetHabbo() != null && this.client.GetHabbo().GetMessenger() != null && !this.client.GetHabbo().GetMessenger().AppearOffline;
			}
		}
		private GameClient Client
		{
			get
			{
				return this.client;
			}
			set
			{
				this.client = value;
			}
		}
		internal bool InRoom
		{
			get
			{
				return this.currentRoom != null;
			}
		}
		internal Room CurrentRoom
		{
			get
			{
				return this.currentRoom;
			}
			set
			{
				this.currentRoom = value;
			}
		}
		internal MessengerBuddy(uint UserId, string pUsername, string pLook, string pMotto, int pLastOnline, bool pAppearOffline, bool pHideInroom)
		{
			this.UserId = UserId;
			this.mUsername = pUsername;
			this.mLook = pLook;
			this.mMotto = pMotto;
			this.mLastOnline = pLastOnline;
			this.mAppearOffline = pAppearOffline;
			this.mHideInroom = pHideInroom;
		}
		internal void UpdateUser()
		{
			this.client = CyberEnvironment.GetGame().GetClientManager().GetClient(this.UserId);
			this.UpdateUser(this.client);
		}
		internal void UpdateUser(GameClient client)
		{
			this.client = client;
			if (client != null && client.GetHabbo() != null)
			{
				this.currentRoom = client.GetHabbo().CurrentRoom;
			}
		}
		internal void Serialize(ServerMessage Message, GameClient Session)
		{
			Relationship value = Session.GetHabbo().Relationships.FirstOrDefault((KeyValuePair<int, Relationship> x) => x.Value.UserId == Convert.ToInt32(this.UserId)).Value;
			int i = (value == null) ? 0 : value.Type;
			Message.AppendUInt(this.UserId);
			Message.AppendString(this.mUsername);
			Message.AppendInt32(1);
			if (!this.mAppearOffline || Session.GetHabbo().Rank >= 4u)
			{
				Message.AppendBoolean(this.IsOnline);
			}
			else
			{
				Message.AppendBoolean(false);
			}
			if (!this.mHideInroom || Session.GetHabbo().Rank >= 4u)
			{
				Message.AppendBoolean(this.InRoom);
			}
			else
			{
				Message.AppendBoolean(false);
			}
			Message.AppendString(this.IsOnline ? this.mLook : "");
			Message.AppendInt32(0);
			Message.AppendString(this.mMotto);
			Message.AppendString(string.Empty);
			Message.AppendString(string.Empty);
			Message.AppendBoolean(true);
			Message.AppendBoolean(false);
			Message.AppendBoolean(false);
			Message.AppendShort(i);
		}
	}
}
