using Cyber.HabboHotel.GameClients;
using Cyber.HabboHotel.Rooms;
using System;
namespace Cyber.HabboHotel.Items.Interactor
{
	internal class InteractorAlert : IFurniInteractor
	{
		public void OnPlace(GameClient Session, RoomItem Item)
		{
			Item.ExtraData = "0";
			Item.UpdateNeeded = true;
		}
		public void OnRemove(GameClient Session, RoomItem Item)
		{
			Item.ExtraData = "0";
		}
		public void OnTrigger(GameClient Session, RoomItem Item, int Request, bool HasRights)
		{
			if (!HasRights)
			{
				return;
			}
			if (Item.ExtraData == "0")
			{
				Item.ExtraData = "1";
				Item.UpdateState(false, true);
				Item.ReqUpdate(4, true);
			}
		}
		public void OnUserWalk(GameClient Session, RoomItem Item, RoomUser User)
		{
		}
		public void OnWiredTrigger(RoomItem Item)
		{
			if (Item.ExtraData == "0")
			{
				Item.ExtraData = "1";
				Item.UpdateState(false, true);
				Item.ReqUpdate(4, true);
			}
		}
	}
}
