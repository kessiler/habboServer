using Cyber.HabboHotel.GameClients;
using Cyber.HabboHotel.PathFinding;
using Cyber.HabboHotel.Rooms;
using System;
namespace Cyber.HabboHotel.Items.Interactor
{
	internal class InteractorVendor : IFurniInteractor
	{
		public void OnPlace(GameClient Session, RoomItem Item)
		{
			Item.ExtraData = "0";
			Item.UpdateNeeded = true;
			if (Item.InteractingUser > 0u)
			{
				RoomUser roomUserByHabbo = Item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(Item.InteractingUser);
				if (roomUserByHabbo != null)
				{
					roomUserByHabbo.CanWalk = true;
				}
			}
		}
		public void OnRemove(GameClient Session, RoomItem Item)
		{
			Item.ExtraData = "0";
			if (Item.InteractingUser > 0u)
			{
				RoomUser roomUserByHabbo = Item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(Item.InteractingUser);
				if (roomUserByHabbo != null)
				{
					roomUserByHabbo.CanWalk = true;
				}
			}
		}
		public void OnTrigger(GameClient Session, RoomItem Item, int Request, bool HasRights)
		{
			if (Item.ExtraData != "1" && Item.GetBaseItem().VendingIds.Count >= 1 && Item.InteractingUser == 0u && Session != null)
			{
				RoomUser roomUserByHabbo = Item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
				if (roomUserByHabbo == null)
				{
					return;
				}
				if (!Gamemap.TilesTouching(roomUserByHabbo.X, roomUserByHabbo.Y, Item.GetX, Item.GetY))
				{
					roomUserByHabbo.MoveTo(Item.SquareInFront);
					return;
				}
				Item.InteractingUser = Session.GetHabbo().Id;
				roomUserByHabbo.CanWalk = false;
				roomUserByHabbo.ClearMovement(true);
                roomUserByHabbo.SetRot(PathFinder.CalculateRotation(roomUserByHabbo.X, roomUserByHabbo.Y, Item.GetX, Item.GetY));
				Item.ReqUpdate(2, true);
				Item.ExtraData = "1";
				Item.UpdateState(false, true);
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
