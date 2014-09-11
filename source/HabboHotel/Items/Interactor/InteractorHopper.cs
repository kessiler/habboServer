using Database_Manager.Database.Session_Details.Interfaces;
using Cyber.HabboHotel.GameClients;
using Cyber.HabboHotel.Rooms;
using System;
namespace Cyber.HabboHotel.Items.Interactor
{
	internal class InteractorHopper : IFurniInteractor
	{
		public void OnPlace(GameClient Session, RoomItem Item)
		{
			checked
			{
				Item.GetRoom().GetRoomItemHandler().HopperCount++;
				using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
				{
					queryreactor.setQuery("INSERT INTO items_hopper (hopper_id, room_id) VALUES (@hopperid, @roomid);");
					queryreactor.addParameter("hopperid", Item.Id);
					queryreactor.addParameter("roomid", Item.RoomId);
					queryreactor.runQuery();
				}
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
			}
		}
		public void OnRemove(GameClient Session, RoomItem Item)
		{
			checked
			{
				Item.GetRoom().GetRoomItemHandler().HopperCount--;
				using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
				{
					queryreactor.setQuery("DELETE FROM items_hopper WHERE item_id=@hid OR room_id=" + Item.GetRoom().RoomId + " LIMIT 1");
					queryreactor.addParameter("hid", Item.Id);
					queryreactor.runQuery();
				}
				if (Item.InteractingUser != 0u)
				{
					RoomUser roomUserByHabbo = Item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(Item.InteractingUser);
					if (roomUserByHabbo != null)
					{
						roomUserByHabbo.UnlockWalking();
					}
					Item.InteractingUser = 0u;
				}
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
				if (roomUserByHabbo.CanWalk)
				{
					roomUserByHabbo.MoveTo(Item.SquareInFront);
				}
				return;
			}
			if (Item.InteractingUser != 0u)
			{
				return;
			}
			roomUserByHabbo.TeleDelay = 2;
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
