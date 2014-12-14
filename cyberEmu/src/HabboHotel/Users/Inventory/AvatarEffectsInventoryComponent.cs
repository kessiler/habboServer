using Database_Manager.Database.Session_Details.Interfaces;
using Cyber.HabboHotel.GameClients;
using Cyber.HabboHotel.Rooms;
using Cyber.HabboHotel.Users.UserDataManagement;
using Cyber.Messages;
using Cyber.Messages.Headers;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Cyber.HabboHotel.Users.Inventory
{
	internal class AvatarEffectsInventoryComponent
	{
		private List<AvatarEffect> Effects;
		private uint UserId;
		internal int CurrentEffect;
		private GameClient Session;
		internal AvatarEffectsInventoryComponent(uint UserId, GameClient Client, UserData Data)
		{
			this.UserId = UserId;
			this.Session = Client;
			this.Effects = new List<AvatarEffect>();
			foreach (AvatarEffect current in Data.effects)
			{
				if (!current.HasExpired)
				{
					this.Effects.Add(current);
				}
				else
				{
					using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
					{
						queryreactor.runFastQuery(string.Concat(new object[]
						{
							"DELETE FROM user_effects WHERE user_id = ",
							UserId,
							" AND effect_id = ",
							current.EffectId,
							"; "
						}));
					}
				}
			}
		}
		internal ServerMessage GetPacket()
		{
			ServerMessage serverMessage = new ServerMessage(Outgoing.EffectsInventoryMessageComposer);
			serverMessage.AppendInt32(this.Effects.Count);
			foreach (AvatarEffect current in this.Effects)
			{
				serverMessage.AppendInt32(current.EffectId);
				serverMessage.AppendInt32(0);
				serverMessage.AppendInt32(current.TotalDuration);
				serverMessage.AppendInt32(0);
				serverMessage.AppendInt32(current.TimeLeft);
			}
			return serverMessage;
		}
		internal void AddNewEffect(int EffectId, int Duration)
		{
			using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
			{
				queryreactor.runFastQuery(string.Concat(new object[]
				{
					"INSERT INTO user_effects (user_id,effect_id,total_duration,is_activated,activated_stamp) VALUES (",
					this.UserId,
					",",
					EffectId,
					",",
					Duration,
					",'0',0)"
				}));
			}
			this.Effects.Add(new AvatarEffect(EffectId, Duration, false, 0.0));
			this.GetClient().GetMessageHandler().GetResponse().Init(Outgoing.AddEffectToInventoryMessageComposer);
			this.GetClient().GetMessageHandler().GetResponse().AppendInt32(EffectId);
			this.GetClient().GetMessageHandler().GetResponse().AppendInt32(0);
			this.GetClient().GetMessageHandler().GetResponse().AppendInt32(Duration);
			this.GetClient().GetMessageHandler().SendResponse();
		}
		internal bool HasEffect(int EffectId)
		{
			return EffectId < 1 || (
				from x in this.Effects
				where x.EffectId == EffectId
				select x).Count<AvatarEffect>() > 0;
		}
		internal void ActivateEffect(int EffectId)
		{
			if (!this.Session.GetHabbo().InRoom)
			{
                return;
			}
			else
			{
				if (!this.HasEffect(EffectId))
				{
					return;
				}
			}
			if (EffectId < 1)
			{
				this.ActivateCustomEffect(EffectId);
				return;
			}
			AvatarEffect avatarEffect = (
				from X in this.Effects
				where X.EffectId == EffectId
				select X).Last<AvatarEffect>();
			avatarEffect.Activate();
			using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
			{
				queryreactor.runFastQuery(string.Concat(new object[]
				{
					"UPDATE user_effects SET is_activated = '1', activated_stamp = ",
					CyberEnvironment.GetUnixTimestamp(),
					" WHERE user_id = ",
					this.UserId,
					" AND effect_id = ",
					EffectId
				}));
			}
			this.EnableInRoom(EffectId);
		}
		internal void ActivateCustomEffect(int EffectId, bool setAsCurrentEffect = true)
		{
			this.EnableInRoom(EffectId, setAsCurrentEffect);
		}
		private void EnableInRoom(int EffectId, bool setAsCurrentEffect = true)
		{
			Room userRoom = this.GetUserRoom();
			if (userRoom == null)
			{
				return;
			}
			RoomUser roomUserByHabbo = userRoom.GetRoomUserManager().GetRoomUserByHabbo(this.GetClient().GetHabbo().Id);
			if (roomUserByHabbo == null)
			{
				return;
			}
            if (setAsCurrentEffect)
            {
                this.CurrentEffect = EffectId;
            }
			ServerMessage serverMessage = new ServerMessage(Outgoing.ApplyEffectMessageComposer);
			serverMessage.AppendInt32(roomUserByHabbo.VirtualId);
			serverMessage.AppendInt32(EffectId);
			serverMessage.AppendInt32(0);
			userRoom.SendMessage(serverMessage);
		}
		internal void OnRoomExit()
		{
			this.CurrentEffect = 0;
		}
		internal void CheckExpired()
		{
			if (this.Effects.Count <= 0)
			{
				return;
			}
			List<AvatarEffect> list = new List<AvatarEffect>();
			foreach (AvatarEffect current in this.Effects)
			{
				if (current.HasExpired)
				{
					list.Add(current);
				}
			}
			foreach (AvatarEffect current2 in list)
			{
				this.StopEffect(current2.EffectId);
			}
			list.Clear();
		}
		internal void StopEffect(int EffectId)
		{
			AvatarEffect avatarEffect = (
				from X in this.Effects
				where X.EffectId == EffectId
				select X).Last<AvatarEffect>();
			if (avatarEffect == null || !avatarEffect.HasExpired)
			{
				return;
			}
			using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
			{
				queryreactor.runFastQuery(string.Concat(new object[]
				{
					"DELETE FROM user_effects WHERE user_id = ",
					this.UserId,
					" AND effect_id = ",
					EffectId,
					" AND is_activated = 1"
				}));
			}
			this.Effects.Remove(avatarEffect);
			this.GetClient().GetMessageHandler().GetResponse().Init(Outgoing.StopAvatarEffectMessageComposer);
			this.GetClient().GetMessageHandler().GetResponse().AppendInt32(EffectId);
			this.GetClient().GetMessageHandler().SendResponse();
			if (this.CurrentEffect >= 0)
			{
				this.ActivateCustomEffect(-1);
			}
		}
		internal void Dispose()
		{
			this.Effects.Clear();
			this.Effects = null;
			this.Session = null;
		}
		internal GameClient GetClient()
		{
			return this.Session;
		}
		private Room GetUserRoom()
		{
			return this.Session.GetHabbo().CurrentRoom;
		}
	}
}
