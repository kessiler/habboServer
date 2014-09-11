using Database_Manager.Database.Session_Details.Interfaces;
using Cyber.HabboHotel.Rooms;
using Cyber.Messages;
using Cyber.Messages.Headers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
namespace Cyber.HabboHotel.Items
{
	internal class PinataHandler
	{
		internal Dictionary<uint, PinataItem> Pinatas;
		private DataTable Table;
		internal void Initialize(IQueryAdapter dbClient)
		{
			dbClient.setQuery("SELECT * FROM pinatas_items");
			this.Pinatas = new Dictionary<uint, PinataItem>();
			this.Table = dbClient.getTable();
			foreach (DataRow dataRow in this.Table.Rows)
			{
				PinataItem value = new PinataItem(dataRow);
				this.Pinatas.Add(uint.Parse(dataRow["item_baseid"].ToString()), value);
			}
		}
		internal void DeliverRandomPinataItem(RoomUser User, Room Room, RoomItem Item)
		{
			if (Room == null || Item == null || Item.GetBaseItem().InteractionType != InteractionType.pinata || !this.Pinatas.ContainsKey(Item.GetBaseItem().ItemId))
			{
				return;
			}
			PinataItem pinataItem;
			this.Pinatas.TryGetValue(Item.GetBaseItem().ItemId, out pinataItem);
			if (pinataItem == null || pinataItem.Rewards.Count < 1)
			{
				return;
			}
			int getX = Item.GetX;
			int getY = Item.GetY;
			double getZ = Item.GetZ;
			Thread.Sleep(900);
			Item.refreshItem();
			Item.BaseItem = pinataItem.Rewards[new Random().Next(checked(pinataItem.Rewards.Count - 1))];
			Item.ExtraData = "";
			Room.GetRoomItemHandler().RemoveFurniture(User.GetClient(), Item.Id, false);
			using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
			{
				queryreactor.runFastQuery(string.Concat(new object[]
				{
					"UPDATE items SET base_item = '",
					Item.BaseItem,
					"', extra_data = '' WHERE id = ",
					Item.Id
				}));
				queryreactor.runQuery();
			}
			Room.GetRoomItemHandler().SetFloorItem(Item, getX, getY, getZ, 0, false);
			ServerMessage serverMessage = new ServerMessage(Outgoing.AddFloorItemMessageComposer);
			Item.Serialize(serverMessage);
			serverMessage.AppendString(Room.Owner);
			Room.SendMessage(serverMessage);
			Room.GetRoomItemHandler().SetFloorItem(User.GetClient(), Item, Item.GetX, Item.GetY, 0, true, false, true);
		}
	}
}
