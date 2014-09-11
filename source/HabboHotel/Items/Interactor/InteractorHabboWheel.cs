using Cyber.HabboHotel.GameClients;
using Cyber.HabboHotel.Rooms;
using System;
namespace Cyber.HabboHotel.Items.Interactor
{
	internal class InteractorHabboWheel : IFurniInteractor
	{
		public void OnPlace(GameClient Session, RoomItem Item)
		{
			Item.ExtraData = "-1";
			Item.ReqUpdate(10, true);
		}
		public void OnRemove(GameClient Session, RoomItem Item)
		{
			Item.ExtraData = "-1";
		}
		public void OnTrigger(GameClient Session, RoomItem Item, int Request, bool HasRights)
		{
			if (!HasRights)
			{
				return;
			}
			if (Item.ExtraData != "-1")
			{
				Item.ExtraData = "-1";
				Item.UpdateState();
				Item.ReqUpdate(10, true);
			}
		}
		public void OnUserWalk(GameClient Session, RoomItem Item, RoomUser User)
		{
		}
		public void OnWiredTrigger(RoomItem Item)
		{
			if (Item.ExtraData != "-1")
			{
				Item.ExtraData = "-1";
				Item.UpdateState();
				Item.ReqUpdate(10, true);
			}
		}
	}
}
