using Database_Manager.Database.Session_Details.Interfaces;
using Cyber.HabboHotel.GameClients;
using Cyber.HabboHotel.Rooms;
using Cyber.Messages;
using Cyber.Messages.Headers;
using System;
using System.Collections.Generic;
using System.Data;
namespace Cyber.HabboHotel.Items.Interactor
{
	internal class InteractorWired : IFurniInteractor
	{
		public void OnPlace(GameClient Session, RoomItem Item)
		{
		}
		public void OnRemove(GameClient Session, RoomItem Item)
		{
			Room room = Item.GetRoom();
			room.GetWiredHandler().RemoveWired(Item);
		}
		public void OnTrigger(GameClient Session, RoomItem Item, int Request, bool HasRights)
		{
			if (Session == null || Item == null)
			{
				return;
			}
			if (!HasRights)
			{
				return;
			}
			string ExtraInfo = "";
			bool flag = false;
			int i = 0;
			List<RoomItem> list = new List<RoomItem>();
			string ExtraString = "";
			string ExtraString2 = "";
			using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
			{
				queryreactor.setQuery("SELECT * FROM wired_items WHERE id=@id LIMIT 1");
				queryreactor.addParameter("id", Item.Id);
				DataRow row = queryreactor.getRow();
				if (row != null)
				{
					ExtraInfo = row["string"].ToString();
					i = (int)row["delay"] / 500;
					flag = (row["bool"].ToString() == "1");
					ExtraString = row["extra_string"].ToString();
					ExtraString2 = row["extra_string_2"].ToString();
					string[] array = row["items"].ToString().Split(new char[]
					{
						';'
					});
					for (int j = 0; j < array.Length; j++)
					{
						string s = array[j];
						uint pId = 0u;
						if (uint.TryParse(s, out pId))
						{
							RoomItem item = Item.GetRoom().GetRoomItemHandler().GetItem(pId);
							if (item != null && !list.Contains(item))
							{
								list.Add(item);
							}
						}
					}
				}
			}
			switch (Item.GetBaseItem().InteractionType)
			{
			case InteractionType.triggertimer:
			{
				ServerMessage serverMessage = new ServerMessage(Outgoing.WiredTriggerMessageComposer);
				serverMessage.AppendBoolean(false);
				serverMessage.AppendInt32(5);
				serverMessage.AppendInt32(list.Count);
				foreach (RoomItem current in list)
				{
					serverMessage.AppendUInt(current.Id);
				}
				serverMessage.AppendInt32(Item.GetBaseItem().SpriteId);
				serverMessage.AppendUInt(Item.Id);
				serverMessage.AppendString(ExtraInfo);
				serverMessage.AppendInt32(1);
				serverMessage.AppendInt32(1);
				serverMessage.AppendInt32(1);
				serverMessage.AppendInt32(3);
				serverMessage.AppendInt32(0);
				serverMessage.AppendInt32(0);
				Session.SendMessage(serverMessage);
				return;
			}
			case InteractionType.triggerroomenter:
			{
				ServerMessage serverMessage2 = new ServerMessage(Outgoing.WiredTriggerMessageComposer);
				serverMessage2.AppendBoolean(false);
				serverMessage2.AppendInt32(0);
				serverMessage2.AppendInt32(list.Count);
				foreach (RoomItem current2 in list)
				{
					serverMessage2.AppendUInt(current2.Id);
				}
				serverMessage2.AppendInt32(Item.GetBaseItem().SpriteId);
				serverMessage2.AppendUInt(Item.Id);
				serverMessage2.AppendString(ExtraInfo);
				serverMessage2.AppendInt32(0);
				serverMessage2.AppendInt32(0);
				serverMessage2.AppendInt32(7);
				serverMessage2.AppendInt32(0);
				serverMessage2.AppendInt32(0);
				serverMessage2.AppendInt32(0);
				Session.SendMessage(serverMessage2);
				return;
			}
			case InteractionType.triggergameend:
			{
				ServerMessage serverMessage3 = new ServerMessage(Outgoing.WiredTriggerMessageComposer);
				serverMessage3.AppendBoolean(false);
				serverMessage3.AppendInt32(0);
				serverMessage3.AppendInt32(list.Count);
				foreach (RoomItem current3 in list)
				{
					serverMessage3.AppendUInt(current3.Id);
				}
				serverMessage3.AppendInt32(Item.GetBaseItem().SpriteId);
				serverMessage3.AppendUInt(Item.Id);
				serverMessage3.AppendString(ExtraInfo);
				serverMessage3.AppendInt32(0);
				serverMessage3.AppendInt32(0);
				serverMessage3.AppendInt32(8);
				serverMessage3.AppendInt32(0);
				serverMessage3.AppendInt32(0);
				serverMessage3.AppendInt32(0);
				Session.SendMessage(serverMessage3);
				return;
			}
			case InteractionType.triggergamestart:
			{
				ServerMessage serverMessage4 = new ServerMessage(Outgoing.WiredTriggerMessageComposer);
				serverMessage4.AppendBoolean(false);
				serverMessage4.AppendInt32(0);
				serverMessage4.AppendInt32(list.Count);
				foreach (RoomItem current4 in list)
				{
					serverMessage4.AppendUInt(current4.Id);
				}
				serverMessage4.AppendInt32(Item.GetBaseItem().SpriteId);
				serverMessage4.AppendUInt(Item.Id);
				serverMessage4.AppendString(ExtraInfo);
				serverMessage4.AppendInt32(0);
				serverMessage4.AppendInt32(0);
				serverMessage4.AppendInt32(8);
				serverMessage4.AppendInt32(0);
				serverMessage4.AppendInt32(0);
				serverMessage4.AppendInt32(0);
				Session.SendMessage(serverMessage4);
				return;
			}
                case InteractionType.triggerlongrepeater:
            {
                ServerMessage serverMessage = new ServerMessage(Outgoing.WiredTriggerMessageComposer);
                serverMessage.AppendBoolean(false);
                serverMessage.AppendInt32(5);
                serverMessage.AppendInt32(0);
                serverMessage.AppendInt32(Item.GetBaseItem().SpriteId);
                serverMessage.AppendUInt(Item.Id);
                serverMessage.AppendString("");
                serverMessage.AppendInt32(1);
                serverMessage.AppendInt32(i / 10);//fix
                serverMessage.AppendInt32(0);
                serverMessage.AppendInt32(12);
                serverMessage.AppendInt32(0);
                Session.SendMessage(serverMessage);
                return;
            }

			case InteractionType.triggerrepeater:
			{
				ServerMessage serverMessage5 = new ServerMessage(Outgoing.WiredTriggerMessageComposer);
				serverMessage5.AppendBoolean(false);
				serverMessage5.AppendInt32(5);
				serverMessage5.AppendInt32(list.Count);
				foreach (RoomItem current5 in list)
				{
					serverMessage5.AppendUInt(current5.Id);
				}
				serverMessage5.AppendInt32(Item.GetBaseItem().SpriteId);
				serverMessage5.AppendUInt(Item.Id);
				serverMessage5.AppendString(ExtraInfo);
				serverMessage5.AppendInt32(1);
				serverMessage5.AppendInt32(i);
				serverMessage5.AppendInt32(0);
				serverMessage5.AppendInt32(6);
				serverMessage5.AppendInt32(0);
				serverMessage5.AppendInt32(0);
				Session.SendMessage(serverMessage5);
				return;
			}
			case InteractionType.triggeronusersay:
			{
				ServerMessage serverMessage6 = new ServerMessage(Outgoing.WiredTriggerMessageComposer);
				serverMessage6.AppendBoolean(false);
				serverMessage6.AppendInt32(0);
				serverMessage6.AppendInt32(list.Count);
				foreach (RoomItem current6 in list)
				{
					serverMessage6.AppendUInt(current6.Id);
				}
				serverMessage6.AppendInt32(Item.GetBaseItem().SpriteId);
				serverMessage6.AppendUInt(Item.Id);
				serverMessage6.AppendString(ExtraInfo);
				serverMessage6.AppendInt32(0);
				serverMessage6.AppendInt32(0);
				serverMessage6.AppendInt32(0);
				serverMessage6.AppendInt32(0);
				serverMessage6.AppendInt32(0);
				serverMessage6.AppendInt32(0);
				Session.SendMessage(serverMessage6);
				return;
			}

			case InteractionType.triggerscoreachieved:
			{
				ServerMessage serverMessage7 = new ServerMessage(Outgoing.WiredTriggerMessageComposer);
				serverMessage7.AppendBoolean(false);
				serverMessage7.AppendInt32(5);
                serverMessage7.AppendInt32(0);
				serverMessage7.AppendInt32(Item.GetBaseItem().SpriteId);
				serverMessage7.AppendUInt(Item.Id);
				serverMessage7.AppendString("");
				serverMessage7.AppendInt32(1);
				serverMessage7.AppendInt32((String.IsNullOrWhiteSpace(ExtraInfo)) ? 100 : int.Parse(ExtraInfo));
				serverMessage7.AppendInt32(0);
				serverMessage7.AppendInt32(10);
				serverMessage7.AppendInt32(0);
				serverMessage7.AppendInt32(0);
				Session.SendMessage(serverMessage7);
				return;
			}
			case InteractionType.triggerstatechanged:
			{
                ServerMessage serverMessage8 = new ServerMessage(Outgoing.WiredTriggerMessageComposer);
				serverMessage8.AppendBoolean(false);
				serverMessage8.AppendInt32(5);
				serverMessage8.AppendInt32(list.Count);
				foreach (RoomItem current8 in list)
				{
					serverMessage8.AppendUInt(current8.Id);
				}
				serverMessage8.AppendInt32(Item.GetBaseItem().SpriteId);
				serverMessage8.AppendUInt(Item.Id);
				serverMessage8.AppendString(ExtraInfo);
				serverMessage8.AppendInt32(0);
				serverMessage8.AppendInt32(0);
				serverMessage8.AppendInt32(1);
				serverMessage8.AppendInt32(i);
				serverMessage8.AppendInt32(0);
				serverMessage8.AppendInt32(0);
				Session.SendMessage(serverMessage8);
				return;
			}
			case InteractionType.triggerwalkonfurni:
			{
                ServerMessage serverMessage9 = new ServerMessage(Outgoing.WiredTriggerMessageComposer);
				serverMessage9.AppendBoolean(false);
				serverMessage9.AppendInt32(5);
				serverMessage9.AppendInt32(list.Count);
				foreach (RoomItem current9 in list)
				{
					serverMessage9.AppendUInt(current9.Id);
				}
				serverMessage9.AppendInt32(Item.GetBaseItem().SpriteId);
				serverMessage9.AppendUInt(Item.Id);
				serverMessage9.AppendString(ExtraInfo);
				serverMessage9.AppendInt32(0);
				serverMessage9.AppendInt32(0);
				serverMessage9.AppendInt32(1);
				serverMessage9.AppendInt32(0);
				serverMessage9.AppendInt32(0);
				serverMessage9.AppendInt32(0);
				Session.SendMessage(serverMessage9);
				return;
			}
                case InteractionType.actionmuteuser:
            {
                ServerMessage serverMessage18 = new ServerMessage(Outgoing.WiredEffectMessageComposer);
                serverMessage18.AppendBoolean(false);
                serverMessage18.AppendInt32(5);
                serverMessage18.AppendInt32(0);
                serverMessage18.AppendInt32(Item.GetBaseItem().SpriteId);
                serverMessage18.AppendUInt(Item.Id);
                serverMessage18.AppendString(ExtraInfo);
                serverMessage18.AppendInt32(1);
                serverMessage18.AppendInt32(i);
                serverMessage18.AppendInt32(0);
                serverMessage18.AppendInt32(20);
                serverMessage18.AppendInt32(0);
                serverMessage18.AppendInt32(0);
                Session.SendMessage(serverMessage18);
                return;
            }
			case InteractionType.triggerwalkofffurni:
			{
                ServerMessage serverMessage10 = new ServerMessage(Outgoing.WiredTriggerMessageComposer);
				serverMessage10.AppendBoolean(false);
				serverMessage10.AppendInt32(5);
				serverMessage10.AppendInt32(list.Count);
				foreach (RoomItem current10 in list)
				{
					serverMessage10.AppendUInt(current10.Id);
				}
				serverMessage10.AppendInt32(Item.GetBaseItem().SpriteId);
				serverMessage10.AppendUInt(Item.Id);
				serverMessage10.AppendString(ExtraInfo);
				serverMessage10.AppendInt32(0);
                serverMessage10.AppendInt32(0);
				serverMessage10.AppendInt32(1);
				serverMessage10.AppendInt32(0);
				serverMessage10.AppendInt32(0);
				serverMessage10.AppendInt32(0);
				serverMessage10.AppendInt32(0);
				Session.SendMessage(serverMessage10);
				return;
			}

			case InteractionType.actiongivescore:
			{
                // Por hacer.
				ServerMessage serverMessage11 = new ServerMessage(Outgoing.WiredEffectMessageComposer);
				serverMessage11.AppendBoolean(false);
				serverMessage11.AppendInt32(5);
                serverMessage11.AppendInt32(0);
				serverMessage11.AppendInt32(Item.GetBaseItem().SpriteId);
				serverMessage11.AppendUInt(Item.Id);
				serverMessage11.AppendString("");
				serverMessage11.AppendInt32(2);
                if (String.IsNullOrWhiteSpace(ExtraInfo))
                {
                    serverMessage11.AppendInt32(10); // Puntos a dar
                    serverMessage11.AppendInt32(1); // Numero de veces por equipo
                }
                else
                {
                    string[] Integers = ExtraInfo.Split(',');
                    serverMessage11.AppendInt32(int.Parse(Integers[0])); // Puntos a dar
                    serverMessage11.AppendInt32(int.Parse(Integers[1])); // Numero de veces por equipo
                }
				serverMessage11.AppendInt32(0);
				serverMessage11.AppendInt32(6);
				serverMessage11.AppendInt32(0);
				serverMessage11.AppendInt32(0);
				serverMessage11.AppendInt32(0);
				Session.SendMessage(serverMessage11);
				return;
			}

                case InteractionType.conditiongroupmember:
                case InteractionType.conditionnotgroupmember:
            {
                ServerMessage Message = new ServerMessage(Outgoing.WiredConditionMessageComposer);
                Message.AppendBoolean(false);
                Message.AppendInt32(5);
                Message.AppendInt32(0);
                Message.AppendInt32(Item.GetBaseItem().SpriteId);
                Message.AppendUInt(Item.Id);
                Message.AppendString("");
                Message.AppendInt32(0);
                Message.AppendInt32(0);
                Message.AppendInt32(10);
                Session.SendMessage(Message);
                return;
            }

            case InteractionType.conditionitemsmatches:
                case InteractionType.conditionitemsdontmatch:
            {
                ServerMessage serverMessage21 = new ServerMessage(Outgoing.WiredConditionMessageComposer);
                serverMessage21.AppendBoolean(false);
                serverMessage21.AppendInt32(5);
                serverMessage21.AppendInt32(list.Count);
                foreach (RoomItem current20 in list)
                {
                    serverMessage21.AppendUInt(current20.Id);
                }
                serverMessage21.AppendInt32(Item.GetBaseItem().SpriteId);
                serverMessage21.AppendUInt(Item.Id);
                serverMessage21.AppendString(ExtraString2);
                serverMessage21.AppendInt32(3);

                if (String.IsNullOrWhiteSpace(ExtraInfo))
                {
                    serverMessage21.AppendInt32(0);
                    serverMessage21.AppendInt32(0);
                    serverMessage21.AppendInt32(0);
                }
                else
                {
                    string[] boolz = ExtraInfo.Split(',');

                    foreach (string Stringy in boolz)
                    {
                        if (Stringy.ToLower() == "true")
                        {
                            serverMessage21.AppendInt32(1);
                        }
                        else
                        {
                            serverMessage21.AppendInt32(0);
                        }
                    }
                }
                serverMessage21.AppendInt32(0);
                serverMessage21.AppendInt32(0);
                Session.SendMessage(serverMessage21);
                return;
            }


			case InteractionType.actionposreset:
			{
				ServerMessage serverMessage12 = new ServerMessage(Outgoing.WiredEffectMessageComposer);
				serverMessage12.AppendBoolean(false);
				serverMessage12.AppendInt32(5);
				serverMessage12.AppendInt32(list.Count);
				foreach (RoomItem current12 in list)
				{
					serverMessage12.AppendUInt(current12.Id);
				}
				serverMessage12.AppendInt32(Item.GetBaseItem().SpriteId);
				serverMessage12.AppendUInt(Item.Id);
				serverMessage12.AppendString(ExtraString2);
				serverMessage12.AppendInt32(3);

                if (String.IsNullOrWhiteSpace(ExtraInfo))
                {
                    serverMessage12.AppendInt32(0);
                    serverMessage12.AppendInt32(0);
                    serverMessage12.AppendInt32(0);
                }
                else
                {
                    string[] boolz = ExtraInfo.Split(',');

                    foreach (string Stringy in boolz)
                    {
                        if (Stringy.ToLower() == "true")
                        {
                            serverMessage12.AppendInt32(1);
                        }
                        else
                        {
                            serverMessage12.AppendInt32(0);
                        }
                    }
                }
				serverMessage12.AppendInt32(0);
				serverMessage12.AppendInt32(3);
				serverMessage12.AppendInt32(i); // Delay
				serverMessage12.AppendInt32(0);
				Session.SendMessage(serverMessage12);
				return;
			}
			case InteractionType.actionmoverotate:
			{
				ServerMessage serverMessage13 = new ServerMessage(Outgoing.WiredEffectMessageComposer);
				serverMessage13.AppendBoolean(false);
				serverMessage13.AppendInt32(5);
				serverMessage13.AppendInt32(list.Count);
				foreach (RoomItem current13 in list)
				{
					serverMessage13.AppendUInt(current13.Id);
				}
				serverMessage13.AppendInt32(Item.GetBaseItem().SpriteId);
				serverMessage13.AppendUInt(Item.Id);
				serverMessage13.AppendString(ExtraInfo);
				serverMessage13.AppendInt32(2);
				serverMessage13.AppendInt32(int.Parse(ExtraInfo.Split(new char[]
				{
					';'
				})[1]));
                serverMessage13.AppendInt32(int.Parse(ExtraInfo.Split(new char[]
				{
					';'
				})[0]));
				serverMessage13.AppendInt32(0);
				serverMessage13.AppendInt32(4);
				serverMessage13.AppendInt32(i);
				serverMessage13.AppendInt32(0);
				serverMessage13.AppendInt32(0);
				Session.SendMessage(serverMessage13);
				return;
			}
			case InteractionType.actionresettimer:
			{
				ServerMessage serverMessage14 = new ServerMessage(Outgoing.WiredEffectMessageComposer);
				serverMessage14.AppendBoolean(false);
				serverMessage14.AppendInt32(5);
				serverMessage14.AppendInt32(list.Count);
				foreach (RoomItem current14 in list)
				{
					serverMessage14.AppendUInt(current14.Id);
				}
				serverMessage14.AppendInt32(Item.GetBaseItem().SpriteId);
				serverMessage14.AppendUInt(Item.Id);
				serverMessage14.AppendString(ExtraInfo);
				serverMessage14.AppendInt32(0);
				serverMessage14.AppendInt32(0);
				serverMessage14.AppendInt32(0);
				serverMessage14.AppendInt32(0);
				serverMessage14.AppendInt32(0);
				serverMessage14.AppendInt32(0);
				Session.SendMessage(serverMessage14);
				return;
			}
			case InteractionType.actionshowmessage:
			case InteractionType.actionkickuser:
			{
				ServerMessage serverMessage15 = new ServerMessage(Outgoing.WiredEffectMessageComposer);
				serverMessage15.AppendBoolean(false);
				serverMessage15.AppendInt32(0);
				serverMessage15.AppendInt32(list.Count);
				foreach (RoomItem current15 in list)
				{
					serverMessage15.AppendUInt(current15.Id);
				}
				serverMessage15.AppendInt32(Item.GetBaseItem().SpriteId);
				serverMessage15.AppendUInt(Item.Id);
				serverMessage15.AppendString(ExtraInfo);
				serverMessage15.AppendInt32(0);
				serverMessage15.AppendInt32(0);
				serverMessage15.AppendInt32(7);
				serverMessage15.AppendInt32(0);
				serverMessage15.AppendInt32(0);
				serverMessage15.AppendInt32(0);
				Session.SendMessage(serverMessage15);
				return;
			}
			case InteractionType.actionteleportto:
			{
				ServerMessage serverMessage16 = new ServerMessage(Outgoing.WiredEffectMessageComposer);
				serverMessage16.AppendBoolean(false);
				serverMessage16.AppendInt32(5);
				serverMessage16.AppendInt32(list.Count);
				foreach (RoomItem current16 in list)
				{
					serverMessage16.AppendUInt(current16.Id);
				}
				serverMessage16.AppendInt32(Item.GetBaseItem().SpriteId);
				serverMessage16.AppendUInt(Item.Id);
				serverMessage16.AppendString(ExtraInfo);
				serverMessage16.AppendInt32(0);
				serverMessage16.AppendInt32(8);
				serverMessage16.AppendInt32(0);
				serverMessage16.AppendInt32(i);
				serverMessage16.AppendInt32(0);
				serverMessage16.AppendByte(2);
				Session.SendMessage(serverMessage16);
				return;
			}
			case InteractionType.actiontogglestate:
			{
				ServerMessage serverMessage17 = new ServerMessage(Outgoing.WiredEffectMessageComposer);
				serverMessage17.AppendBoolean(false);
				serverMessage17.AppendInt32(5);
				serverMessage17.AppendInt32(list.Count);
				foreach (RoomItem current17 in list)
				{
					serverMessage17.AppendUInt(current17.Id);
				}
				serverMessage17.AppendInt32(Item.GetBaseItem().SpriteId);
				serverMessage17.AppendUInt(Item.Id);
				serverMessage17.AppendString(ExtraInfo);
				serverMessage17.AppendInt32(0);
				serverMessage17.AppendInt32(8);
				serverMessage17.AppendInt32(0);
				serverMessage17.AppendInt32(i);
				serverMessage17.AppendInt32(0);
				serverMessage17.AppendInt32(0);
				Session.SendMessage(serverMessage17);
				return;
			}
			case InteractionType.actiongivereward:
			{
				ServerMessage serverMessage18 = new ServerMessage(Outgoing.WiredEffectMessageComposer);
				serverMessage18.AppendBoolean(false);
				serverMessage18.AppendInt32(5);
				serverMessage18.AppendInt32(0);
				serverMessage18.AppendInt32(Item.GetBaseItem().SpriteId);
				serverMessage18.AppendUInt(Item.Id);
				serverMessage18.AppendString(ExtraInfo);
				serverMessage18.AppendInt32(3);
				serverMessage18.AppendInt32((ExtraString == "") ? 0 : int.Parse(ExtraString));
				serverMessage18.AppendInt32(flag ? 1 : 0);
				serverMessage18.AppendInt32((ExtraString2 == "") ? 0 : int.Parse(ExtraString2));
				serverMessage18.AppendInt32(0);
				serverMessage18.AppendInt32(17);
				serverMessage18.AppendInt32(0);
				serverMessage18.AppendInt32(0);
				Session.SendMessage(serverMessage18);
				return;
			}

            case InteractionType.conditionhowmanyusersinroom:
                case InteractionType.conditionnegativehowmanyusers:
            {
                ServerMessage serverMessage19 = new ServerMessage(Outgoing.WiredConditionMessageComposer);
                serverMessage19.AppendBoolean(false);
                serverMessage19.AppendInt32(5);
                serverMessage19.AppendInt32(0);
                serverMessage19.AppendInt32(Item.GetBaseItem().SpriteId);
                serverMessage19.AppendUInt(Item.Id);
                serverMessage19.AppendString("");
                serverMessage19.AppendInt32(2);
                if (String.IsNullOrWhiteSpace(ExtraInfo))
                {
                    serverMessage19.AppendInt32(1);
                    serverMessage19.AppendInt32(50);
                }
                else
                {
                    foreach (string Integers in ExtraInfo.Split(','))
                    {
                        serverMessage19.AppendInt32(int.Parse(Integers));
                    }
                }
                serverMessage19.AppendBoolean(false);
                serverMessage19.AppendInt32(0);
                serverMessage19.AppendInt32(1290);
                Session.SendMessage(serverMessage19);
                return;
            }

			case InteractionType.conditionfurnishaveusers:
            case InteractionType.conditionstatepos:
            case InteractionType.conditiontriggeronfurni:
            case InteractionType.conditionfurnihasfurni:
            case InteractionType.conditionfurnitypematches:
                case InteractionType.conditionfurnihasnotfurni:
                case InteractionType.conditionfurnishavenotusers:
                case InteractionType.conditionfurnitypedontmatch:
                case InteractionType.conditiontriggerernotonfurni:
			{
				ServerMessage serverMessage19 = new ServerMessage(Outgoing.WiredConditionMessageComposer);
				serverMessage19.AppendBoolean(false);
				serverMessage19.AppendInt32(5);
				serverMessage19.AppendInt32(list.Count);
				foreach (RoomItem current18 in list)
				{
					serverMessage19.AppendUInt(current18.Id);
				}
				serverMessage19.AppendInt32(Item.GetBaseItem().SpriteId);
				serverMessage19.AppendUInt(Item.Id);
				serverMessage19.AppendInt32(0);
				serverMessage19.AppendInt32(0);
				serverMessage19.AppendInt32(0);
				serverMessage19.AppendBoolean(false);
				serverMessage19.AppendBoolean(true);
				Session.SendMessage(serverMessage19);
				return;
			}
			case InteractionType.conditiontimelessthan:
			case InteractionType.conditiontimemorethan:
			{
				ServerMessage serverMessage21 = new ServerMessage(Outgoing.WiredConditionMessageComposer);
				serverMessage21.AppendBoolean(false);
				serverMessage21.AppendInt32(5);
				serverMessage21.AppendInt32(list.Count);
				foreach (RoomItem current20 in list)
				{
					serverMessage21.AppendUInt(current20.Id);
				}
				serverMessage21.AppendInt32(Item.GetBaseItem().SpriteId);
				serverMessage21.AppendUInt(Item.Id);
				serverMessage21.AppendInt32(0);
				serverMessage21.AppendInt32(0);
				serverMessage21.AppendInt32(0);
				serverMessage21.AppendInt32(0);
				serverMessage21.AppendInt32(0);
				serverMessage21.AppendInt32(0);
				serverMessage21.AppendInt32(0);
				Session.SendMessage(serverMessage21);
				return;
			}

            case InteractionType.conditionuserwearingeffect:
                case InteractionType.conditionusernotwearingeffect:
            {
                int effect = 0;
                int.TryParse(ExtraInfo, out effect);
                ServerMessage serverMessage21 = new ServerMessage(Outgoing.WiredConditionMessageComposer);
                serverMessage21.AppendBoolean(false);
                serverMessage21.AppendInt32(5);
                serverMessage21.AppendInt32(0);
                serverMessage21.AppendInt32(Item.GetBaseItem().SpriteId);
                serverMessage21.AppendUInt(Item.Id);
                serverMessage21.AppendString("");
                serverMessage21.AppendInt32(1);
                serverMessage21.AppendInt32(effect);
                serverMessage21.AppendInt32(0);
                serverMessage21.AppendInt32(12);
                Session.SendMessage(serverMessage21);
                return;
            }

                case InteractionType.conditionuserwearingbadge:
                case InteractionType.conditionusernotwearingbadge:
            {
                ServerMessage serverMessage21 = new ServerMessage(Outgoing.WiredConditionMessageComposer);
                serverMessage21.AppendBoolean(false);
                serverMessage21.AppendInt32(5);
                serverMessage21.AppendInt32(0);
                serverMessage21.AppendInt32(Item.GetBaseItem().SpriteId);
                serverMessage21.AppendUInt(Item.Id);
                serverMessage21.AppendString(ExtraInfo);
                serverMessage21.AppendInt32(0);
                serverMessage21.AppendInt32(0);
                serverMessage21.AppendInt32(11);
                Session.SendMessage(serverMessage21);
                return;
            }

                case InteractionType.conditiondaterangeactive:
            {
                int date1 = 0;
                int date2 = 0;

                try
                {
                    string[] strArray = ExtraInfo.Split(',');
                    date1 = int.Parse(strArray[0]);
                    date2 = int.Parse(strArray[1]);
                }
                catch { }

                ServerMessage serverMessage21 = new ServerMessage(Outgoing.WiredConditionMessageComposer);
                serverMessage21.AppendBoolean(false);
                serverMessage21.AppendInt32(5);
                serverMessage21.AppendInt32(0);
                serverMessage21.AppendInt32(Item.GetBaseItem().SpriteId);
                serverMessage21.AppendUInt(Item.Id);
                serverMessage21.AppendString(ExtraInfo);
                serverMessage21.AppendInt32(2);
                serverMessage21.AppendInt32(date1);
                serverMessage21.AppendInt32(date2);
                serverMessage21.AppendInt32(0);
                serverMessage21.AppendInt32(24);
                Session.SendMessage(serverMessage21);
                return;
            }
			
           
			
			case InteractionType.arrowplate:
			case InteractionType.pressurepad:
			case InteractionType.ringplate:
			case InteractionType.colortile:
			case InteractionType.colorwheel:
			case InteractionType.floorswitch1:
			case InteractionType.floorswitch2:
			case InteractionType.firegate:
			case InteractionType.glassfoor:
				break;
			case InteractionType.specialrandom:
			{
				ServerMessage serverMessage24 = new ServerMessage(Outgoing.WiredEffectMessageComposer);
				serverMessage24.AppendBoolean(false);
				serverMessage24.AppendInt32(5);
				serverMessage24.AppendInt32(list.Count);
				foreach (RoomItem current23 in list)
				{
					serverMessage24.AppendUInt(current23.Id);
				}
				serverMessage24.AppendInt32(Item.GetBaseItem().SpriteId);
				serverMessage24.AppendUInt(Item.Id);
				serverMessage24.AppendString(ExtraInfo);
				serverMessage24.AppendInt32(0);
				serverMessage24.AppendInt32(8);
				serverMessage24.AppendInt32(0);
				serverMessage24.AppendInt32(0);
				serverMessage24.AppendInt32(0);
				serverMessage24.AppendInt32(0);
				Session.SendMessage(serverMessage24);
				return;
			}
			case InteractionType.specialunseen:
			{
				ServerMessage serverMessage25 = new ServerMessage(Outgoing.WiredEffectMessageComposer);
				serverMessage25.AppendBoolean(false);
				serverMessage25.AppendInt32(5);
				serverMessage25.AppendInt32(list.Count);
				foreach (RoomItem current24 in list)
				{
					serverMessage25.AppendUInt(current24.Id);
				}
				serverMessage25.AppendInt32(Item.GetBaseItem().SpriteId);
				serverMessage25.AppendUInt(Item.Id);
				serverMessage25.AppendString(ExtraInfo);
				serverMessage25.AppendInt32(0);
				serverMessage25.AppendInt32(8);
				serverMessage25.AppendInt32(0);
				serverMessage25.AppendInt32(0);
				serverMessage25.AppendInt32(0);
				serverMessage25.AppendInt32(0);
				Session.SendMessage(serverMessage25);
				return;
			}
			default:
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
