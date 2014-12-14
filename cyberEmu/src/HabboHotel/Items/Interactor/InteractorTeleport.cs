using Cyber.HabboHotel.GameClients;
using Cyber.HabboHotel.Rooms;
using System;
namespace Cyber.HabboHotel.Items.Interactor
{
	internal class InteractorTeleport : IFurniInteractor
	{
		public void OnPlace(GameClient Session, RoomItem Item)
		{
			Item.ExtraData = "0";
			if (Item.InteractingUser != 0u)
			{
				RoomUser roomUserByHabbo = Item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(Item.InteractingUser);
				if (roomUserByHabbo != null)
				{
					roomUserByHabbo.ClearMovement(true);
					roomUserByHabbo.AllowOverride = false;
					roomUserByHabbo.CanWalk = true;
				}
				Item.InteractingUser = 0u;
			}
			if (Item.InteractingUser2 != 0u)
			{
				RoomUser roomUserByHabbo2 = Item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(Item.InteractingUser2);
				if (roomUserByHabbo2 != null)
				{
					roomUserByHabbo2.ClearMovement(true);
					roomUserByHabbo2.AllowOverride = false;
					roomUserByHabbo2.CanWalk = true;
				}
				Item.InteractingUser2 = 0u;
			}
		}
		public void OnRemove(GameClient Session, RoomItem Item)
		{
			Item.ExtraData = "0";
			if (Item.InteractingUser != 0u)
			{
				RoomUser roomUserByHabbo = Item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(Item.InteractingUser);
				if (roomUserByHabbo != null)
				{
					roomUserByHabbo.UnlockWalking();
				}
				Item.InteractingUser = 0u;
			}
			if (Item.InteractingUser2 != 0u)
			{
				RoomUser roomUserByHabbo2 = Item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(Item.InteractingUser2);
				if (roomUserByHabbo2 != null)
				{
					roomUserByHabbo2.UnlockWalking();
				}
				Item.InteractingUser2 = 0u;
			}
		}
		public void OnTrigger(GameClient Session, RoomItem Item, int Request, bool HasRights)
		{
			if (Item == null || Item.GetRoom() == null || Session == null || Session.GetHabbo() == null)
			{
				return;
			}
			RoomUser roomUserByHabbo = Item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
			if (roomUserByHabbo == null)
			{
				return;
			}
			if (!(roomUserByHabbo.Coordinate == Item.Coordinate) && !(roomUserByHabbo.Coordinate == Item.SquareInFront))
			{
				roomUserByHabbo.MoveTo(Item.SquareInFront);
				return;
			}
			if (Item.InteractingUser != 0)
			{
				return;
			}
			Item.InteractingUser = roomUserByHabbo.GetClient().GetHabbo().Id;
		}
		public void OnUserWalk(GameClient Session, RoomItem Item, RoomUser User)
		{
		}
		public void OnWiredTrigger(RoomItem Item)
		{
		}
	}
}
