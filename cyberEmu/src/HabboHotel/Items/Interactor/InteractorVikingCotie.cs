using Cyber.HabboHotel.GameClients;
using Cyber.HabboHotel.Rooms;
using System;
using System.Timers;

namespace Cyber.HabboHotel.Items.Interactor
{
	internal class InteractorVikingCotie : IFurniInteractor
	{
        private RoomItem mItem;

		public void OnPlace(GameClient Session, RoomItem Item)
		{
		}
		public void OnRemove(GameClient Session, RoomItem Item)
		{
		}

		public void OnTrigger(GameClient Session, RoomItem Item, int Request, bool HasRights)
		{
			RoomUser User = Item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
			if (User == null)
			{
				return;
			}
			
			if (User.CurrentEffect == 172 || User.CurrentEffect == 5 || User.CurrentEffect == 173)
			{
				if (Item.ExtraData != "5")
				{
					if (!Item.VikingCotieBurning)
					{
                        Item.ExtraData = "1";
                        Item.UpdateState();

						Item.VikingCotieBurning = true;
                        GameClient clientByUsername = CyberEnvironment.GetGame().GetClientManager().GetClientByUsername(Item.GetRoom().Owner);
                       
                        if (clientByUsername != null )
                        {
                            if (clientByUsername.GetHabbo().Username != Item.GetRoom().Owner)
                            {
                                clientByUsername.SendNotif("" + User.GetUsername() + " ha empezado a quemar una cabaña vikingo de tu Sala!");
                            }
                        }

                        this.mItem = Item;

                        Timer Timer = new Timer(5000);
                        Timer.Elapsed += OnElapse;
                        Timer.Enabled = true;
						return;
					}
				}
				else
				{
					Session.SendNotif("¡Lo sentimos! Esta cabaña Vikingo ya ha sido quemada y no hay marcha atrás!");
				}
			}
		}

        private void OnElapse(object sender, ElapsedEventArgs e)
        {
            switch (mItem.ExtraData)
            {
                case "1":
                    mItem.ExtraData = "2";
                    mItem.UpdateState();
                    return;

                case "2":
                    mItem.ExtraData = "3";
                    mItem.UpdateState();
                    return;

                case "3":
                    mItem.ExtraData = "4";
                    mItem.UpdateState();
                    return;

                case "4":
                    ((Timer)sender).Stop();
                    mItem.ExtraData = "5";
                    mItem.UpdateState();
                    return;
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
