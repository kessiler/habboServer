using Cyber.HabboHotel.GameClients;
using Cyber.HabboHotel.Rooms;
using Cyber.HabboHotel.Rooms.Wired;
using System;
namespace Cyber.HabboHotel.Items.Interactor
{
	internal class InteractorGate : IFurniInteractor
	{
		public void OnPlace(GameClient Session, RoomItem Item)
		{
		}
		public void OnRemove(GameClient Session, RoomItem Item)
		{
		}
		public void OnTrigger(GameClient Session, RoomItem Item, int Request, bool HasRights)
		{
			checked
			{
				int num = Item.GetBaseItem().Modes - 1;
				if (!HasRights)
				{
					return;
				}
				if (num <= 0)
				{
					Item.UpdateState(false, true);
				}
				int num2 = 0;
				int.TryParse(Item.ExtraData, out num2);
				int num3;
				if (num2 <= 0)
				{
					num3 = 1;
				}
				else
				{
					if (num2 >= num)
					{
						num3 = 0;
					}
					else
					{
						num3 = num2 + 1;
					}
				}
				if (num3 == 0 && !Item.GetRoom().GetGameMap().itemCanBePlacedHere(Item.GetX, Item.GetY))
				{
					return;
				}
				Item.ExtraData = num3.ToString();
				Item.UpdateState();
				Item.GetRoom().GetGameMap().updateMapForItem(Item);
				Item.GetRoom().GetWiredHandler().ExecuteWired(WiredItemType.TriggerToggleFurni, new object[]
				{
					Item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id),
					Item
				});
			}
		}
		public void OnUserWalk(GameClient Session, RoomItem Item, RoomUser User)
		{
		}
		public void OnWiredTrigger(RoomItem Item)
		{
			checked
			{
				int num = Item.GetBaseItem().Modes - 1;
				if (num <= 0)
				{
					Item.UpdateState(false, true);
				}
				int num2 = 0;
				int.TryParse(Item.ExtraData, out num2);
				int num3;
				if (num2 <= 0)
				{
					num3 = 1;
				}
				else
				{
					if (num2 >= num)
					{
						num3 = 0;
					}
					else
					{
						num3 = num2 + 1;
					}
				}
				if (num3 == 0 && !Item.GetRoom().GetGameMap().itemCanBePlacedHere(Item.GetX, Item.GetY))
				{
					return;
				}
				Item.ExtraData = num3.ToString();
				Item.UpdateState();
				Item.GetRoom().GetGameMap().updateMapForItem(Item);
			}
		}
	}
}
