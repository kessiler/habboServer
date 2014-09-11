using Database_Manager.Database.Session_Details.Interfaces;
using Cyber.HabboHotel.GameClients;
using Cyber.HabboHotel.Rooms;
using Cyber.HabboHotel.Users;
using System;
namespace Cyber.HabboHotel.Items.Interactor
{
	internal class InteractorOneWayGate : IFurniInteractor
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
					roomUserByHabbo.UnlockWalking();
				}
				Item.InteractingUser = 0u;
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
					roomUserByHabbo.ClearMovement(true);
					roomUserByHabbo.UnlockWalking();
				}
				Item.InteractingUser = 0u;
			}
		}
		public void OnTrigger(GameClient Session, RoomItem Item, int Request, bool HasRights)
		{
			if (Session == null)
			{
				return;
			}
			RoomUser roomUserByHabbo = Item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
			if (Item.InteractingUser2 != roomUserByHabbo.UserID)
			{
				Item.InteractingUser2 = roomUserByHabbo.UserID;
			}
			if (roomUserByHabbo == null)
			{
				return;
			}
			if (roomUserByHabbo.Coordinate != Item.SquareInFront && roomUserByHabbo.CanWalk)
			{
				roomUserByHabbo.MoveTo(Item.SquareInFront);
				return;
			}
			if (!Item.GetRoom().GetGameMap().ValidTile(Item.SquareBehind.X, Item.SquareBehind.Y) || !Item.GetRoom().GetGameMap().CanWalk(Item.SquareBehind.X, Item.SquareBehind.Y, false, 0u) || !Item.GetRoom().GetGameMap().SquareIsOpen(Item.SquareBehind.X, Item.SquareBehind.Y, false))
			{
				return;
			}
			checked
			{
				if (roomUserByHabbo.LastInteraction - CyberEnvironment.GetUnixTimestamp() < 0 && roomUserByHabbo.InteractingGate && roomUserByHabbo.GateId == Item.Id)
				{
					roomUserByHabbo.InteractingGate = false;
					roomUserByHabbo.GateId = 0u;
				}
				{
					if (!Item.GetRoom().GetGameMap().CanWalk(Item.SquareBehind.X, Item.SquareBehind.Y, roomUserByHabbo.AllowOverride, 0u))
					{
						return;
					}
					if (Item.InteractingUser == 0u)
					{
						roomUserByHabbo.InteractingGate = true;
						roomUserByHabbo.GateId = Item.Id;
						Item.InteractingUser = roomUserByHabbo.HabboId;
						roomUserByHabbo.CanWalk = false;
						if (roomUserByHabbo.IsWalking && (roomUserByHabbo.GoalX != Item.SquareInFront.X || roomUserByHabbo.GoalY != Item.SquareInFront.Y))
						{
							roomUserByHabbo.ClearMovement(true);
						}

						roomUserByHabbo.InteractingGate = false;
						roomUserByHabbo.GateId = 0u;
						return;
					}
					return;
				}
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
