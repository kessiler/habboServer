using Cyber.HabboHotel.GameClients;
using Cyber.HabboHotel.Rooms;
using System;
namespace Cyber.HabboHotel.Items.Interactor
{
	internal class InteractorGroupForumTerminal : IFurniInteractor
	{
		public void OnPlace(GameClient Session, RoomItem Item)
		{
		}
		public void OnRemove(GameClient Session, RoomItem Item)
		{
		}
		public void OnTrigger(GameClient Session, RoomItem Item, int Request, bool HasRights)
		{
			uint.Parse(Item.ExtraData);
		}
		public void OnUserWalk(GameClient Session, RoomItem Item, RoomUser User)
		{
		}
		public void OnWiredTrigger(RoomItem Item)
		{
		}
	}
}
