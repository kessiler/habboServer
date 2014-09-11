using Cyber.Core;
using Cyber.HabboHotel.GameClients;
using Cyber.HabboHotel.Items;
using Cyber.Messages;
using Cyber.Messages.Headers;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Cyber.HabboHotel.Rooms.Wired
{
	public static class WiredSaver
	{
        public static void SaveWired(GameClient Session, RoomItem Item, ClientMessage Request)
        {
            if (!Item.IsWired)
            {
                return;
            }
            Room room = Item.GetRoom();
            WiredHandler wiredHandler = room.GetWiredHandler();
            checked
            {
                switch (Item.GetBaseItem().InteractionType)
                {
                    case InteractionType.triggerroomenter:
                        {
                            Request.PopWiredInt32();
                            string otherString = Request.PopFixedString();
                            WiredItem wired = wiredHandler.GetWired(Item);
                            wired.OtherString = otherString;
                            wiredHandler.ReloadWired(wired);
                            break;
                        }

                    case InteractionType.triggerlongrepeater:
                        {
                            Request.PopWiredInt32();
                            int delay = Request.PopWiredInt32() * 5000;
                            WiredItem wired2 = wiredHandler.GetWired(Item);
                            wired2.Delay = delay;
                            wiredHandler.ReloadWired(wired2);
                            break;
                        }

                    case InteractionType.triggerrepeater:
                        {
                            Request.PopWiredInt32();
                            int delay = Request.PopWiredInt32() * 500;
                            WiredItem wired2 = wiredHandler.GetWired(Item);
                            wired2.Delay = delay;
                            wiredHandler.ReloadWired(wired2);
                            break;
                        }
                    case InteractionType.triggeronusersay:
                        {
                            Request.PopWiredInt32();
                            int num = Request.PopWiredInt32();
                            string otherString2 = Request.PopFixedString();
                            WiredItem wired3 = wiredHandler.GetWired(Item);
                            wired3.OtherString = otherString2;
                            wired3.OtherBool = (num == 1);
                            wiredHandler.ReloadWired(wired3);
                            break;
                        }
                    case InteractionType.triggerstatechanged:
                        {
                            Request.PopWiredInt32();
                            Request.PopFixedString();
                            List<RoomItem> furniItems = WiredSaver.GetFurniItems(Request, room);
                            int num2 = Request.PopWiredInt32();
                            WiredItem wired4 = wiredHandler.GetWired(Item);
                            wired4.Delay = num2 * 500;
                            wired4.Items = furniItems;
                            wiredHandler.ReloadWired(wired4);
                            break;
                        }
                    case InteractionType.triggerwalkonfurni:
                        {
                            Request.PopWiredInt32();
                            Request.PopFixedString();
                            List<RoomItem> furniItems2 = WiredSaver.GetFurniItems(Request, room);
                            int num3 = Request.PopWiredInt32();
                            WiredItem wired5 = wiredHandler.GetWired(Item);
                            wired5.Delay = num3 * 500;
                            wired5.Items = furniItems2;
                            wiredHandler.ReloadWired(wired5);
                            break;
                        }

                    case InteractionType.triggerwalkofffurni:
                        {
                            Request.PopWiredInt32();
                            Request.PopFixedString();
                            List<RoomItem> furniItems3 = WiredSaver.GetFurniItems(Request, room);
                            int num4 = Request.PopWiredInt32();
                            WiredItem wired6 = wiredHandler.GetWired(Item);
                            wired6.Delay = num4 * 500;
                            wired6.Items = furniItems3;
                            wiredHandler.ReloadWired(wired6);
                            break;
                        }
                    case InteractionType.actionmoverotate:
                        {
                            Request.PopWiredInt32();
                            int num5 = Request.PopWiredInt32();
                            int num6 = Request.PopWiredInt32();
                            Request.PopFixedString();
                            List<RoomItem> furniItems4 = WiredSaver.GetFurniItems(Request, room);
                            int num7 = Request.PopWiredInt32();
                            WiredItem wired7 = wiredHandler.GetWired(Item);
                            wired7.Items = furniItems4;
                            wired7.Delay = num7 * 500;
                            wired7.OtherString = num6 + ";" + num5;
                            wiredHandler.ReloadWired(wired7);
                            break;
                        }
                    case InteractionType.actionshowmessage:
                    case InteractionType.actionkickuser:
                        {
                            Request.PopWiredInt32();
                            string otherString3 = Request.PopFixedString();
                            WiredItem wired8 = wiredHandler.GetWired(Item);
                            wired8.OtherString = otherString3;
                            wiredHandler.ReloadWired(wired8);
                            break;
                        }
                    case InteractionType.actionteleportto:
                        {
                            Request.PopWiredInt32();
                            Request.PopFixedString();
                            List<RoomItem> furniItems5 = WiredSaver.GetFurniItems(Request, room);
                            int num8 = Request.PopWiredInt32();
                            WiredItem wired9 = wiredHandler.GetWired(Item);
                            wired9.Items = furniItems5;
                            wired9.Delay = num8 * 500;
                            wiredHandler.ReloadWired(wired9);
                            break;
                        }
                    case InteractionType.actiontogglestate:
                        {
                            Request.PopWiredInt32();
                            Request.PopFixedString();
                            List<RoomItem> furniItems6 = WiredSaver.GetFurniItems(Request, room);
                            int num9 = Request.PopWiredInt32();
                            WiredItem wired10 = wiredHandler.GetWired(Item);
                            wired10.Items = furniItems6;
                            wired10.Delay = num9 * 500;
                            wiredHandler.ReloadWired(wired10);
                            break;
                        }
                    case InteractionType.actiongivereward:
                        {
                            Request.PopWiredInt32();
                            int num10 = Request.PopWiredInt32();
                            bool otherBool = Request.PopWiredInt32() == 1;
                            int num11 = Request.PopWiredInt32();
                            Request.PopWiredInt32();
                            string otherString4 = Request.PopFixedString();
                            List<RoomItem> furniItems7 = WiredSaver.GetFurniItems(Request, room);
                            WiredItem wired11 = wiredHandler.GetWired(Item);
                            wired11.Items = furniItems7;
                            wired11.Delay = 0;
                            wired11.OtherBool = otherBool;
                            wired11.OtherString = otherString4;
                            wired11.OtherExtraString = num10.ToString();
                            wired11.OtherExtraString2 = num11.ToString();
                            wiredHandler.ReloadWired(wired11);
                            break;
                        }
                    case InteractionType.actionmuteuser:
                        {
                            Request.PopWiredInt32();
                            int minutes = Request.PopWiredInt32() * 500;
                            string Message = Request.PopFixedString();
                            List<RoomItem> furniItems7 = WiredSaver.GetFurniItems(Request, room);
                            WiredItem wired11 = wiredHandler.GetWired(Item);
                            wired11.Items = furniItems7;
                            wired11.Delay = minutes;
                            wired11.OtherBool = false;
                            wired11.OtherString = Message;
                            wiredHandler.ReloadWired(wired11);
                            break;
                        }
                    case InteractionType.triggerscoreachieved:
                        {
                            Request.PopWiredInt32();
                            int pointsRequired = Request.PopWiredInt32();

                            WiredItem wired11 = wiredHandler.GetWired(Item);
                            wired11.Delay = 0;
                            wired11.OtherString = pointsRequired.ToString();
                            wiredHandler.ReloadWired(wired11);
                            break;
                        }

                    case InteractionType.conditionitemsmatches:
                    case InteractionType.conditionitemsdontmatch:
                    case InteractionType.actionposreset:
                   
                        {
                            // Coded by Finn for Cyber Emulator
                            Request.PopWiredInt32();
                            bool ActualExtraData = Request.PopWiredInt32() == 1;
                            bool ActualRot = Request.PopWiredInt32() == 1;
                            bool ActualPosition = Request.PopWiredInt32() == 1;

                            string Booleans = (ActualExtraData.ToString() + "," + ActualRot.ToString() + "," + ActualPosition.ToString()).ToLower();

                            Request.PopFixedString();
                            List<RoomItem> Items = WiredSaver.GetFurniItems(Request, room);

                            int Delay = Request.PopWiredInt32() * 500;
                            WiredItem Wiry = wiredHandler.GetWired(Item);

                            string DataToSave = "";
                            string ExtraStringForWI = "";

                            foreach (RoomItem AItem in Items)
                            {
                                DataToSave += (AItem.Id + "|" + AItem.ExtraData + "|" + AItem.Rot + "|" + AItem.GetX + "," + AItem.GetY + "," + AItem.GetZ.ToString());
                                ExtraStringForWI += (AItem.Id + "," + ((ActualExtraData) ? AItem.ExtraData : "N") + "," + ((ActualRot) ? AItem.Rot.ToString() : "N") + "," + ((ActualPosition) ? AItem.GetX.ToString() : "N") + "," + ((ActualPosition) ? AItem.GetY.ToString() : "N"));

                                if (AItem != Items.Last())
                                {
                                    DataToSave += "/";
                                    ExtraStringForWI += ";";
                                }
                            }

                            Wiry.Items = Items;
                            Wiry.Delay = Delay;
                            Wiry.OtherBool = true;
                            Wiry.OtherString = Booleans;
                            Wiry.OtherExtraString = DataToSave;
                            Wiry.OtherExtraString2 = ExtraStringForWI;
                            wiredHandler.ReloadWired(Wiry);
                            break;
                        }

                    case InteractionType.conditiongroupmember:
                    case InteractionType.conditionnotgroupmember:
                        {
                            // Nothing is needed.
                            break;
                        }

                    case InteractionType.conditionhowmanyusersinroom:
                    case InteractionType.conditionnegativehowmanyusers:
                        {
                            Request.PopWiredInt32();
                            int Minimum = Request.PopWiredInt32();
                            int Maximum = Request.PopWiredInt32();

                            string EI = Minimum + "," + Maximum;
                            WiredItem wired12 = wiredHandler.GetWired(Item);
                            wired12.Items = new List<RoomItem>();
                            wired12.OtherString = EI;
                            wiredHandler.ReloadWired(wired12);
                            break;
                        }

                    case InteractionType.conditionusernotwearingeffect:
                    case InteractionType.conditionuserwearingeffect:
                        {
                            Request.PopWiredInt32();
                            int Effect = Request.PopWiredInt32();
                            WiredItem wired12 = wiredHandler.GetWired(Item);
                            wired12.Items = new List<RoomItem>();
                            wired12.OtherString = Effect.ToString();
                            wiredHandler.ReloadWired(wired12);
                            break;
                        }

                    case InteractionType.conditionuserwearingbadge:
                    case InteractionType.conditionusernotwearingbadge:
                        {
                            Request.PopWiredInt32();
                            string badge = Request.PopFixedString();
                            WiredItem wired12 = wiredHandler.GetWired(Item);
                            wired12.Items = new List<RoomItem>();
                            wired12.OtherString = badge;
                            wiredHandler.ReloadWired(wired12);
                            break;
                        }

                    case InteractionType.conditiondaterangeactive:
                        {
                            Request.PopWiredInt32();
                            int startDate = Request.PopWiredInt32();
                            int endDate = Request.PopWiredInt32();//timestamps

                            WiredItem wired12 = wiredHandler.GetWired(Item);
                            wired12.Items = new List<RoomItem>();
                            wired12.OtherString = startDate + "," + endDate;
                            
                            if (startDate == 0)
                            {
                                wired12.OtherString = "";
                                Session.SendNotif(@"Para poder guardar la fecha debes introducirlo así: <b>YYYY/MM/dd hh:mm</b> o bien <b>dd/MM/YYYY hh:mm</b><br />No especifiques fecha de término si no hay. La hora y los minutos son opcionales. Anímate a arreglarlo.<br /><br /><br />Con cariño,<br /><b>Finn</b>");
                            }

                            wiredHandler.ReloadWired(wired12);
                            break;
                        }

                    case InteractionType.conditionfurnishaveusers:
                    case InteractionType.conditiontriggeronfurni:
                    case InteractionType.conditionfurnihasfurni:
                    case InteractionType.conditionfurnitypematches:
                    case InteractionType.conditionfurnishavenotusers:
                    case InteractionType.conditiontriggerernotonfurni:
                    case InteractionType.conditionfurnihasnotfurni:
                    case InteractionType.conditionfurnitypedontmatch:
                        {
                            Request.PopWiredInt32();
                            Request.PopFixedString();
                            List<RoomItem> furniItems8 = WiredSaver.GetFurniItems(Request, room);
                            WiredItem wired12 = wiredHandler.GetWired(Item);
                            wired12.Items = furniItems8;
                            wiredHandler.ReloadWired(wired12);
                            break;
                        }

                    case InteractionType.actiongivescore:
                        {
                            Request.PopWiredInt32();
                            int ScoreToGive = Request.PopWiredInt32();
                            int MaxTimesPerGame = Request.PopWiredInt32();

                            string NewExtraInfo = (ScoreToGive + "," + MaxTimesPerGame);

                            List<RoomItem> furniItems8 = WiredSaver.GetFurniItems(Request, room);
                            WiredItem wired12 = wiredHandler.GetWired(Item);
                            wired12.Items = furniItems8;
                            wired12.OtherString = NewExtraInfo;
                            wiredHandler.ReloadWired(wired12);
                            break;
                        }


                }
                
                Session.SendMessage(new ServerMessage(Outgoing.SaveWiredMessageComposer));
            }
        }

		private static List<RoomItem> GetFurniItems(ClientMessage Request, Room Room)
		{
			List<RoomItem> list = new List<RoomItem>();
			int num = Request.PopWiredInt32();
			checked
			{
				for (int i = 0; i < num; i++)
				{
					RoomItem item = Room.GetRoomItemHandler().GetItem(Request.PopWiredUInt());
					if (item != null)
					{
						list.Add(item);
					}
				}
				return list;
			}
		}
	}
}
