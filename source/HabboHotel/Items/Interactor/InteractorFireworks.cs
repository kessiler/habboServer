using Cyber.HabboHotel.GameClients;
using Cyber.HabboHotel.Rooms;
using System;
namespace Cyber.HabboHotel.Items.Interactor
{
	internal class InteractorFireworks : IFurniInteractor
	{
		public void OnPlace(GameClient Session, RoomItem Item)
		{
			Item.ExtraData = "1";
		}
		public void OnRemove(GameClient Session, RoomItem Item)
		{
			Item.ExtraData = "1";
		}
		public void OnTrigger(GameClient Session, RoomItem Item, int Request, bool HasRights)
		{
			if (Item.ExtraData == "" || Item.ExtraData == "0")
			{
				Item.ExtraData = "1";
				Item.UpdateState();
				return;
			}
			if (Item.ExtraData == "1")
			{
				Item.ExtraData = "2";
			}
		}
		public void OnUserWalk(GameClient Session, RoomItem Item, RoomUser User)
		{
		}
		public void OnWiredTrigger(RoomItem Item)
		{
		}
	}
}
