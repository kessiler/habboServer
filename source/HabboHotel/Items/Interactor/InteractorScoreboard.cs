using Cyber.HabboHotel.GameClients;
using Cyber.HabboHotel.Rooms;
using System;
namespace Cyber.HabboHotel.Items.Interactor
{
	internal class InteractorScoreboard : IFurniInteractor
	{
		public void OnPlace(GameClient Session, RoomItem Item)
		{
		}
		public void OnRemove(GameClient Session, RoomItem Item)
		{
		}
		public void OnTrigger(GameClient Session, RoomItem Item, int Request, bool HasRights)
		{
			if (!HasRights)
			{
				return;
			}
			int num = 0;
			int.TryParse(Item.ExtraData, out num);
			checked
			{
				if (Request == 1)
				{
					if (Item.pendingReset && num > 0)
					{
						num = 0;
						Item.pendingReset = false;
					}
					else
					{
						num += 60;
						Item.UpdateNeeded = false;
					}
				}
				else
				{
					if (Request == 2)
					{
						Item.UpdateNeeded = !Item.UpdateNeeded;
						Item.pendingReset = true;
					}
				}
				Item.ExtraData = num.ToString();
				Item.UpdateState();
			}
		}
		public void OnUserWalk(GameClient Session, RoomItem Item, RoomUser User)
		{
		}
		public void OnWiredTrigger(RoomItem Item)
		{
			int num = 0;
			int.TryParse(Item.ExtraData, out num);
			checked
			{
				num += 60;
				Item.UpdateNeeded = false;
				Item.ExtraData = num.ToString();
				Item.UpdateState();
			}
		}
	}
}
