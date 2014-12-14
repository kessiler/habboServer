using Cyber.HabboHotel.GameClients;
using Cyber.HabboHotel.Rooms;
using System;
namespace Cyber.HabboHotel.Items.Interactor
{
	internal interface IFurniInteractor
	{
		void OnPlace(GameClient Session, RoomItem Item);
		void OnRemove(GameClient Session, RoomItem Item);
		void OnTrigger(GameClient Session, RoomItem Item, int Request, bool HasRights);
		void OnUserWalk(GameClient Session, RoomItem Item, RoomUser User);
		void OnWiredTrigger(RoomItem Item);
	}
}
