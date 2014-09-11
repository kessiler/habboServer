using Cyber.Core;
using Cyber.HabboHotel.Items;
using Cyber.Messages;
using Cyber.Messages.Headers;
using System;
using System.Collections.Generic;
namespace Cyber.HabboHotel.Rooms
{
	internal class Trade
	{
		private TradeUser[] Users;
		private int TradeStage;
		private uint RoomId;
		private uint oneId;
		private uint twoId;
		internal bool AllUsersAccepted
		{
			get
			{
				checked
				{
					for (int i = 0; i < this.Users.Length; i++)
					{
						if (this.Users[i] != null && !this.Users[i].HasAccepted)
						{
							return false;
						}
					}
					return true;
				}
			}
		}
		internal Trade(uint UserOneId, uint UserTwoId, uint RoomId)
		{
			this.oneId = UserOneId;
			this.twoId = UserTwoId;
			this.Users = new TradeUser[2];
			this.Users[0] = new TradeUser(UserOneId, RoomId);
			this.Users[1] = new TradeUser(UserTwoId, RoomId);
			this.TradeStage = 1;
			this.RoomId = RoomId;
			TradeUser[] users = this.Users;
			for (int i = 0; i < users.Length; i++)
			{
				TradeUser tradeUser = users[i];
				if (!tradeUser.GetRoomUser().Statusses.ContainsKey("trd"))
				{
					tradeUser.GetRoomUser().AddStatus("trd", "");
					tradeUser.GetRoomUser().UpdateNeeded = true;
				}
			}
			ServerMessage serverMessage = new ServerMessage(Outgoing.TradeStartMessageComposer);
			serverMessage.AppendUInt(UserOneId);
			serverMessage.AppendInt32(1);
			serverMessage.AppendUInt(UserTwoId);
			serverMessage.AppendInt32(1);
			this.SendMessageToUsers(serverMessage);
		}
		internal bool ContainsUser(uint Id)
		{
			checked
			{
				for (int i = 0; i < this.Users.Length; i++)
				{
					if (this.Users[i] != null && this.Users[i].UserId == Id)
					{
						return true;
					}
				}
				return false;
			}
		}
		internal TradeUser GetTradeUser(uint Id)
		{
			checked
			{
				for (int i = 0; i < this.Users.Length; i++)
				{
					if (this.Users[i] != null && this.Users[i].UserId == Id)
					{
						return this.Users[i];
					}
				}
				return null;
			}
		}
		internal void OfferItem(uint UserId, UserItem Item)
		{
			TradeUser tradeUser = this.GetTradeUser(UserId);
			if (tradeUser == null || Item == null || !Item.GetBaseItem().AllowTrade || tradeUser.HasAccepted || this.TradeStage != 1)
			{
				return;
			}
			this.ClearAccepted();
			if (!tradeUser.OfferedItems.Contains(Item))
			{
				tradeUser.OfferedItems.Add(Item);
			}
			this.UpdateTradeWindow();
		}
		internal void TakeBackItem(uint UserId, UserItem Item)
		{
			TradeUser tradeUser = this.GetTradeUser(UserId);
			if (tradeUser == null || Item == null || tradeUser.HasAccepted || this.TradeStage != 1)
			{
				return;
			}
			this.ClearAccepted();
			tradeUser.OfferedItems.Remove(Item);
			this.UpdateTradeWindow();
		}
		internal void Accept(uint UserId)
		{
			TradeUser tradeUser = this.GetTradeUser(UserId);
			if (tradeUser == null || this.TradeStage != 1)
			{
				return;
			}
			tradeUser.HasAccepted = true;
			ServerMessage serverMessage = new ServerMessage(Outgoing.TradeAcceptMessageComposer);
			serverMessage.AppendUInt(UserId);
			serverMessage.AppendInt32(1);
			this.SendMessageToUsers(serverMessage);
			checked
			{
				if (this.AllUsersAccepted)
				{
					this.SendMessageToUsers(new ServerMessage(Outgoing.TradeConfirmationMessageComposer));
					this.TradeStage++;
					this.ClearAccepted();
				}
			}
		}
		internal void Unaccept(uint UserId)
		{
			TradeUser tradeUser = this.GetTradeUser(UserId);
			if (tradeUser == null || this.TradeStage != 1 || this.AllUsersAccepted)
			{
				return;
			}
			tradeUser.HasAccepted = false;
			ServerMessage serverMessage = new ServerMessage(Outgoing.TradeAcceptMessageComposer);
			serverMessage.AppendUInt(UserId);
			serverMessage.AppendInt32(0);
			this.SendMessageToUsers(serverMessage);
		}
		internal void CompleteTrade(uint UserId)
		{
			TradeUser tradeUser = this.GetTradeUser(UserId);
			if (tradeUser == null || this.TradeStage != 2)
			{
				return;
			}
			tradeUser.HasAccepted = true;
			ServerMessage serverMessage = new ServerMessage(Outgoing.TradeAcceptMessageComposer);
			serverMessage.AppendUInt(UserId);
			serverMessage.AppendInt32(1);
			this.SendMessageToUsers(serverMessage);
			if (this.AllUsersAccepted)
			{
				this.TradeStage = 999;
				this.Finnito();
			}
		}
		private void Finnito()
		{
			try
			{
				this.DeliverItems();
				this.CloseTradeClean();
			}
			catch (Exception ex)
			{
				Logging.LogThreadException(ex.ToString(), "Trade task");
			}
		}
		internal void ClearAccepted()
		{
			TradeUser[] users = this.Users;
			for (int i = 0; i < users.Length; i++)
			{
				TradeUser tradeUser = users[i];
				tradeUser.HasAccepted = false;
			}
		}
		internal void UpdateTradeWindow()
		{
			ServerMessage serverMessage = new ServerMessage(Outgoing.TradeUpdateMessageComposer);
			checked
			{
				for (int i = 0; i < this.Users.Length; i++)
				{
					TradeUser tradeUser = this.Users[i];
					if (tradeUser != null)
					{
						serverMessage.AppendUInt(tradeUser.UserId);
						serverMessage.AppendInt32(tradeUser.OfferedItems.Count);
						foreach (UserItem current in tradeUser.OfferedItems)
						{
							serverMessage.AppendUInt(current.Id);
							serverMessage.AppendString(current.GetBaseItem().Type.ToString().ToLower());
							serverMessage.AppendUInt(current.Id);
							serverMessage.AppendInt32(current.GetBaseItem().SpriteId);
							serverMessage.AppendInt32(0);
							serverMessage.AppendBoolean(true);
							serverMessage.AppendInt32(0);
							serverMessage.AppendString("");
							serverMessage.AppendInt32(0);
							serverMessage.AppendInt32(0);
							serverMessage.AppendInt32(0);
							if (current.GetBaseItem().Type == 's')
							{
								serverMessage.AppendInt32(0);
							}
						}
					}
				}
				this.SendMessageToUsers(serverMessage);
			}
		}
		internal void DeliverItems()
		{
			List<UserItem> offeredItems = this.GetTradeUser(this.oneId).OfferedItems;
			List<UserItem> offeredItems2 = this.GetTradeUser(this.twoId).OfferedItems;
			foreach (UserItem current in offeredItems)
			{
				if (this.GetTradeUser(this.oneId).GetClient().GetHabbo().GetInventoryComponent().GetItem(current.Id) == null)
				{
					this.GetTradeUser(this.oneId).GetClient().SendNotif("El tradeo ha fallado.");
					this.GetTradeUser(this.twoId).GetClient().SendNotif("El tradeo ha fallado.");
					return;
				}
			}
			foreach (UserItem current2 in offeredItems2)
			{
				if (this.GetTradeUser(this.twoId).GetClient().GetHabbo().GetInventoryComponent().GetItem(current2.Id) == null)
				{
					this.GetTradeUser(this.oneId).GetClient().SendNotif("El tradeo ha fallado.");
					this.GetTradeUser(this.twoId).GetClient().SendNotif("El tradeo ha fallado.");
					return;
				}
			}
			this.GetTradeUser(this.twoId).GetClient().GetHabbo().GetInventoryComponent().RunDBUpdate();
			this.GetTradeUser(this.oneId).GetClient().GetHabbo().GetInventoryComponent().RunDBUpdate();
			foreach (UserItem current3 in offeredItems)
			{
				this.GetTradeUser(this.oneId).GetClient().GetHabbo().GetInventoryComponent().RemoveItem(current3.Id, false);
				this.GetTradeUser(this.twoId).GetClient().GetHabbo().GetInventoryComponent().AddNewItem(current3.Id, current3.BaseItem, current3.ExtraData, current3.GroupId, false, false, 0, 0, current3.SongCode);
				this.GetTradeUser(this.oneId).GetClient().GetHabbo().GetInventoryComponent().RunDBUpdate();
				this.GetTradeUser(this.twoId).GetClient().GetHabbo().GetInventoryComponent().RunDBUpdate();
			}
			foreach (UserItem current4 in offeredItems2)
			{
				this.GetTradeUser(this.twoId).GetClient().GetHabbo().GetInventoryComponent().RemoveItem(current4.Id, false);
				this.GetTradeUser(this.oneId).GetClient().GetHabbo().GetInventoryComponent().AddNewItem(current4.Id, current4.BaseItem, current4.ExtraData, current4.GroupId, false, false, 0, 0, current4.SongCode);
				this.GetTradeUser(this.twoId).GetClient().GetHabbo().GetInventoryComponent().RunDBUpdate();
				this.GetTradeUser(this.oneId).GetClient().GetHabbo().GetInventoryComponent().RunDBUpdate();
			}
			ServerMessage serverMessage = new ServerMessage(Outgoing.NewInventoryObjectMessageComposer);
			serverMessage.AppendInt32(1);
			int i = 1;
			foreach (UserItem current5 in offeredItems)
			{
				if (current5.GetBaseItem().Type.ToString().ToLower() != "s")
				{
					i = 2;
				}
			}
			serverMessage.AppendInt32(i);
			serverMessage.AppendInt32(offeredItems.Count);
			foreach (UserItem current6 in offeredItems)
			{
				serverMessage.AppendUInt(current6.Id);
			}
			this.GetTradeUser(this.twoId).GetClient().SendMessage(serverMessage);
			ServerMessage serverMessage2 = new ServerMessage(Outgoing.NewInventoryObjectMessageComposer);
			serverMessage2.AppendInt32(1);
			i = 1;
			foreach (UserItem current7 in offeredItems2)
			{
				if (current7.GetBaseItem().Type.ToString().ToLower() != "s")
				{
					i = 2;
				}
			}
			serverMessage2.AppendInt32(i);
			serverMessage2.AppendInt32(offeredItems2.Count);
			foreach (UserItem current8 in offeredItems2)
			{
				serverMessage2.AppendUInt(current8.Id);
			}
			this.GetTradeUser(this.oneId).GetClient().SendMessage(serverMessage2);
			this.GetTradeUser(this.oneId).GetClient().GetHabbo().GetInventoryComponent().UpdateItems(false);
			this.GetTradeUser(this.twoId).GetClient().GetHabbo().GetInventoryComponent().UpdateItems(false);
		}
		internal void CloseTradeClean()
		{
			checked
			{
				for (int i = 0; i < this.Users.Length; i++)
				{
					TradeUser tradeUser = this.Users[i];
					if (tradeUser != null && tradeUser.GetRoomUser() != null)
					{
						tradeUser.GetRoomUser().RemoveStatus("trd");
						tradeUser.GetRoomUser().UpdateNeeded = true;
					}
				}
				this.SendMessageToUsers(new ServerMessage(Outgoing.TradeCompletedMessageComposer));
				this.GetRoom().ActiveTrades.Remove(this);
			}
		}
		internal void CloseTrade(uint UserId)
		{
			checked
			{
				for (int i = 0; i < this.Users.Length; i++)
				{
					TradeUser tradeUser = this.Users[i];
					if (tradeUser != null && tradeUser.GetRoomUser() != null)
					{
						tradeUser.GetRoomUser().RemoveStatus("trd");
						tradeUser.GetRoomUser().UpdateNeeded = true;
					}
				}
				ServerMessage serverMessage = new ServerMessage(Outgoing.TradeCloseMessageComposer);
				serverMessage.AppendUInt(UserId);
				serverMessage.AppendInt32(0);
				this.SendMessageToUsers(serverMessage);
			}
		}
		internal void SendMessageToUsers(ServerMessage Message)
		{
			if (this.Users == null)
			{
				return;
			}
			checked
			{
				for (int i = 0; i < this.Users.Length; i++)
				{
					TradeUser tradeUser = this.Users[i];
					if (tradeUser != null && tradeUser != null && tradeUser.GetClient() != null)
					{
						tradeUser.GetClient().SendMessage(Message);
					}
				}
			}
		}
		private Room GetRoom()
		{
			return CyberEnvironment.GetGame().GetRoomManager().GetRoom(this.RoomId);
		}
	}
}
