using Cyber.HabboHotel.GameClients;
using Cyber.HabboHotel.Rooms;
using Cyber.HabboHotel.Rooms.Games;
using System;
namespace Cyber.HabboHotel.Items.Interactor
{
	internal class InteractorScoreCounter : IFurniInteractor
	{
		public void OnPlace(GameClient Session, RoomItem Item)
		{
			if (Item.team == Team.none)
			{
				return;
			}
			Item.ExtraData = Item.GetRoom().GetGameManager().Points[(int)Item.team].ToString();
			Item.UpdateState(false, true);
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
					num++;
				}
				else
				{
					if (Request == 2)
					{
						num--;
					}
					else
					{
						if (Request == 3)
						{
							num = 0;
						}
					}
				}
				Item.ExtraData = num.ToString();
				Item.UpdateState(false, true);
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
				num++;
				Item.ExtraData = num.ToString();
				Item.UpdateState(false, true);
			}
		}
	}
}
