using Cyber.HabboHotel.GameClients;
using Cyber.HabboHotel.Quests;
using Cyber.HabboHotel.Rooms;
using Cyber.HabboHotel.Rooms.Wired;
using System;
namespace Cyber.HabboHotel.Items.Interactor
{
	internal class InteractorGenericSwitch : IFurniInteractor
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
				if (Session == null || !HasRights || num <= 0 || Item.GetBaseItem().InteractionType == InteractionType.pinata)
				{
					return;
				}
				CyberEnvironment.GetGame().GetQuestManager().ProgressUserQuest(Session, QuestType.FURNI_SWITCH, 0u);
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
				Item.ExtraData = num3.ToString();
				Item.UpdateState();
				Item.GetRoom().GetWiredHandler().ExecuteWired(WiredItemType.TriggerToggleFurni, new object[]
				{
					Item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id),
					Item
				});
				if (Item.GetBaseItem().StackMultipler)
				{
					Room room = Item.GetRoom();
					foreach (RoomUser current in room.GetRoomUserManager().UserList.Values)
					{
						if (current.Statusses.ContainsKey("sit"))
						{
							room.GetRoomUserManager().UpdateUserStatus(current, true);
						}
					}
				}
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
				if (num == 0)
				{
					return;
				}
				int num2 = 0;
				if (!int.TryParse(Item.ExtraData, out num2))
				{
					return;
				}
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
				Item.ExtraData = num3.ToString();
				Item.UpdateState();
				if (Item.GetBaseItem().StackMultipler)
				{
					Room room = Item.GetRoom();
					foreach (RoomUser current in room.GetRoomUserManager().UserList.Values)
					{
						if (current.Statusses.ContainsKey("sit"))
						{
							room.GetRoomUserManager().UpdateUserStatus(current, true);
						}
					}
				}
			}
		}
	}
}
