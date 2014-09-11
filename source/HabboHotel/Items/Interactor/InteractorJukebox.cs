using Cyber.HabboHotel.GameClients;
using Cyber.HabboHotel.Rooms;
using System;
namespace Cyber.HabboHotel.Items.Interactor
{
	internal class InteractorJukebox : IFurniInteractor
	{
		public void OnPlace(GameClient Session, RoomItem Item)
		{
			Item.ExtraData = "0";
			Item.UpdateState();
		}
		public void OnRemove(GameClient Session, RoomItem Item)
		{
		}
		public void OnTrigger(GameClient Session, RoomItem Item, int Request, bool HasRights)
		{
            if (!HasRights)
            { return; }
            if (Item.ExtraData == "1")
            {
                Item.GetRoom().GetRoomMusicController().Stop();
                Item.ExtraData = "0";
            }
            else
            {
                Item.GetRoom().GetRoomMusicController().Start();
                Item.ExtraData = "1";
            }
            Item.UpdateState();
		}
		public void OnUserWalk(GameClient Session, RoomItem Item, RoomUser User)
		{
		}
		public void OnWiredTrigger(RoomItem Item)
		{
			if (Item.ExtraData == "1")
			{
				Item.GetRoom().GetRoomMusicController().Stop();
				Item.ExtraData = "0";
			}
			else
			{
				Item.GetRoom().GetRoomMusicController().Start();
				Item.ExtraData = "1";
			}
			Item.UpdateState();
		}
	}
}
