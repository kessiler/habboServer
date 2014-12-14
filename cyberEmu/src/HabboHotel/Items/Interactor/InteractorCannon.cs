using Cyber.HabboHotel.GameClients;
using Cyber.HabboHotel.Rooms;
using System;
using System.Threading.Tasks;
using System.Drawing;
using System.Linq;
using System.Collections.Generic;
using Cyber.Messages;
using Cyber.Messages.Headers;
using System.Timers;

namespace Cyber.HabboHotel.Items.Interactor
{
    class InteractorCannon : IFurniInteractor
    {
        private RoomItem mItem;
        private HashSet<Point> mCoords;

        public void OnPlace(GameClient Session, RoomItem Item)
        {
            Item.ExtraData = "0";
        }

        public void OnRemove(GameClient Session, RoomItem Item)
        {
            Item.ExtraData = "0";
        }

        public void OnTrigger(GameClient Session, RoomItem Item, int Request, bool HasRights)
        {
            // nada
        }

        private void ExplodeAndKick(Object Source, ElapsedEventArgs e)
        {
            Timer Timer = (Timer)Source;
            Timer.Stop();

                ServerMessage serverMessage = new ServerMessage(Outgoing.SuperNotificationMessageComposer);
                serverMessage.AppendString("room.kick.cannonball");
                serverMessage.AppendInt32(2);
                serverMessage.AppendString("link");
                serverMessage.AppendString("event:");
                serverMessage.AppendString("linkTitle");
                serverMessage.AppendString("ok");

                Room Room = mItem.GetRoom();

                HashSet<RoomUser> toRemove = new HashSet<RoomUser>();

                foreach (Point coord in mCoords)
                {
                    foreach (RoomUser User in Room.GetGameMap().GetRoomUsers(coord))
                    {
                        if (User == null || User.IsBot || User.IsPet || User.GetUsername() == Room.Owner)
                        {
                            continue;
                        }

                        
                        User.GetClient().GetHabbo().GetAvatarEffectsInventoryComponent().ActivateCustomEffect(4, false);
                        toRemove.Add(User);
                    }
                }


                foreach (RoomUser user in toRemove)
                {
                    Room.GetRoomUserManager().RemoveUserFromRoom(user.GetClient(), true, false);
                    user.GetClient().SendMessage(serverMessage);
                }
            
            mItem.OnCannonActing = false;
        }
      

        public void OnUserWalk(GameClient Session, RoomItem Item, RoomUser User)
        {
        }

        public void OnWiredTrigger(RoomItem Item)
        {
            if (Item.OnCannonActing)
            {
                return;
            }

            Item.OnCannonActing = true;
            HashSet<Point> coords = new HashSet<Point>();

            int itemX = Item.GetX;
            int itemY = Item.GetY;

            switch (Item.Rot)
            {
                case 0: // TESTEADO OK
                    int startingcoordX = itemX - 1;

                    for (int i = startingcoordX; i > 0; i--)
                    {
                        coords.Add(new Point(i, itemY));
                    }
                    break;

                case 4: // TESTEADO OK
                    int startingcoordX2 = itemX + 2;
                    int mapsizeX = Item.GetRoom().GetGameMap().Model.MapSizeX;

                    for (int i = startingcoordX2; i < mapsizeX; i++)
                    {
                        coords.Add(new Point(i, itemY));
                    }
                    break;

                case 2: // TESTEADO OK
                    int startingcoordY = itemY - 1;

                    for (int i = startingcoordY; i > 0; i--)
                    {
                        coords.Add(new Point(itemX, i));
                    }
                    break;

                case 6: // OK!
                    int startingcoordY2 = itemY + 2;
                    int mapsizeY = Item.GetRoom().GetGameMap().Model.MapSizeY;

                    for (int i = startingcoordY2; i < mapsizeY; i++)
                    {
                        coords.Add(new Point(itemX, i));
                    }
                    break;
            }

            Item.ExtraData = (Item.ExtraData == "0") ? "1" : "0";
            Item.UpdateState();

            mItem = Item;
            mCoords = coords;

            Timer explodeTimer = new Timer(1350);
            explodeTimer.Elapsed += ExplodeAndKick;
            explodeTimer.Enabled = true;
        }
    }
}
