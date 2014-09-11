using Database_Manager.Database.Session_Details.Interfaces;
using Cyber.HabboHotel.GameClients;
using Cyber.HabboHotel.Rooms;
using Cyber.Messages;
using Cyber.Messages.Headers;
using System;
using System.Collections.Generic;
namespace Cyber.HabboHotel.Items.Interactor
{
	internal class InteractorMannequin : IFurniInteractor
	{
		public void OnPlace(GameClient Session, RoomItem Item)
		{
		}
		public void OnRemove(GameClient Session, RoomItem Item)
		{
		}
		public void OnTrigger(GameClient Session, RoomItem Item, int Request, bool HasRights)
		{
			if (Item.ExtraData.Contains(Convert.ToChar(5).ToString()))
			{
				string[] array = Item.ExtraData.Split(new char[]
				{
					Convert.ToChar(5)
				});
				Session.GetHabbo().Gender = array[0].ToUpper();
				Dictionary<string, string> dictionary = new Dictionary<string, string>();
				dictionary.Clear();
				string[] array2 = array[1].Split(new char[]
				{
					'.'
				});
				for (int i = 0; i < array2.Length; i++)
				{
					string text = array2[i];
					string[] array3 = Session.GetHabbo().Look.Split(new char[]
					{
						'.'
					});
					for (int j = 0; j < array3.Length; j++)
					{
						string text2 = array3[j];
						if (text2.Split(new char[]
						{
							'-'
						})[0] == text.Split(new char[]
						{
							'-'
						})[0])
						{
							if (dictionary.ContainsKey(text2.Split(new char[]
							{
								'-'
							})[0]) && !dictionary.ContainsValue(text))
							{
								dictionary.Remove(text2.Split(new char[]
								{
									'-'
								})[0]);
								dictionary.Add(text2.Split(new char[]
								{
									'-'
								})[0], text);
							}
							else
							{
								if (!dictionary.ContainsKey(text2.Split(new char[]
								{
									'-'
								})[0]) && !dictionary.ContainsValue(text))
								{
									dictionary.Add(text2.Split(new char[]
									{
										'-'
									})[0], text);
								}
							}
						}
						else
						{
							if (!dictionary.ContainsKey(text2.Split(new char[]
							{
								'-'
							})[0]))
							{
								dictionary.Add(text2.Split(new char[]
								{
									'-'
								})[0], text2);
							}
						}
					}
				}
				string text3 = "";
				foreach (string current in dictionary.Values)
				{
					text3 = text3 + current + ".";
				}
				Session.GetHabbo().Look = text3.TrimEnd(new char[]
				{
					'.'
				});
				using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
				{
					queryreactor.setQuery("UPDATE users SET look = @look, gender = @gender WHERE id = " + Session.GetHabbo().Id);
					queryreactor.addParameter("look", Session.GetHabbo().Look);
					queryreactor.addParameter("gender", Session.GetHabbo().Gender);
					queryreactor.runQuery();
				}
				Session.GetMessageHandler().GetResponse().Init(Outgoing.UpdateUserDataMessageComposer);
				Session.GetMessageHandler().GetResponse().AppendInt32(-1);
				Session.GetMessageHandler().GetResponse().AppendString(Session.GetHabbo().Look);
				Session.GetMessageHandler().GetResponse().AppendString(Session.GetHabbo().Gender.ToLower());
				Session.GetMessageHandler().GetResponse().AppendString(Session.GetHabbo().Motto);
				Session.GetMessageHandler().GetResponse().AppendInt32(Session.GetHabbo().AchievementPoints);
				Session.GetMessageHandler().SendResponse();
				RoomUser roomUserByHabbo = Item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
				ServerMessage serverMessage = new ServerMessage(Outgoing.UpdateUserDataMessageComposer);
				serverMessage.AppendInt32(roomUserByHabbo.VirtualId);
				serverMessage.AppendString(Session.GetHabbo().Look);
				serverMessage.AppendString(Session.GetHabbo().Gender.ToLower());
				serverMessage.AppendString(Session.GetHabbo().Motto);
				serverMessage.AppendInt32(Session.GetHabbo().AchievementPoints);
				Session.GetHabbo().CurrentRoom.SendMessage(serverMessage);
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
