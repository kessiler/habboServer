using Cyber.Core;
using Cyber.HabboHotel.GameClients;
using Cyber.HabboHotel.Rooms;
using Cyber.Messages;
using Cyber.Messages.Headers;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
namespace Cyber.Net
{
	internal class MusConnection
	{
		private Socket socket;
		private byte[] buffer = new byte[1024];
		internal MusConnection(Socket _socket)
		{
			this.socket = _socket;
			try
			{
				this.socket.BeginReceive(this.buffer, 0, this.buffer.Length, SocketFlags.None, new AsyncCallback(this.OnEvent_RecieveData), this.socket);
			}
			catch
			{
				this.tryClose();
			}
		}
		internal void tryClose()
		{
			try
			{
				this.socket.Shutdown(SocketShutdown.Both);
				this.socket.Close();
				this.socket.Dispose();
			}
			catch
			{
			}
			this.socket = null;
			this.buffer = null;
		}
		internal void OnEvent_RecieveData(IAsyncResult iAr)
		{
			try
			{
				int count = 0;
				try
				{
					count = this.socket.EndReceive(iAr);
				}
				catch
				{
					this.tryClose();
					return;
				}
				string @string = Encoding.Default.GetString(this.buffer, 0, count);
				if (@string.Length > 0)
				{
					this.processCommand(@string);
				}
			}
			catch (Exception value)
			{
				Console.WriteLine(value);
			}
			this.tryClose();
		}
		internal void processCommand(string data)
		{
			string text = data.Split(new char[]
			{
				Convert.ToChar(1)
			})[0];
			string text2 = data.Split(new char[]
			{
				Convert.ToChar(1)
			})[1];
			string[] array = text2.Split(new char[]
			{
				Convert.ToChar(5)
			});
			string a;
			if ((a = text.ToLower()) != null)
			{
				if (!(a == "updatemotto"))
				{
					GameClient clientByUserID;
					if (!(a == "updaterooms"))
					{
						if (!(a == "addtoinventory"))
						{
							if (!(a == "updatecredits"))
							{
								if (!(a == "updatesubscription"))
								{
									goto IL_38B;
								}
								uint userID = Convert.ToUInt32(array[0]);
								clientByUserID = CyberEnvironment.GetGame().GetClientManager().GetClientByUserID(userID);
								if (clientByUserID != null && clientByUserID.GetHabbo() != null)
								{
									clientByUserID.GetHabbo().GetSubscriptionManager().ReloadSubscription();
									clientByUserID.GetHabbo().SerializeClub();
									clientByUserID.SendMessage(new ServerMessage(Outgoing.PublishShopMessageComposer));
									goto IL_3A3;
								}
								goto IL_3A3;
							}
							else
							{
								uint userID2 = Convert.ToUInt32(array[0]);
								int credits = Convert.ToInt32(array[1]);
								clientByUserID = CyberEnvironment.GetGame().GetClientManager().GetClientByUserID(userID2);
								if (clientByUserID != null && clientByUserID.GetHabbo() != null)
								{
									clientByUserID.GetHabbo().Credits = credits;
									clientByUserID.GetHabbo().UpdateCreditsBalance();
									goto IL_3A3;
								}
								goto IL_3A3;
							}
						}
					}
					else
					{
						uint num = Convert.ToUInt32(array[0]);
						string arg_20F_0 = array[1];
						using (Dictionary<uint, Room>.ValueCollection.Enumerator enumerator = CyberEnvironment.GetGame().GetRoomManager().loadedRooms.Values.GetEnumerator())
						{
							while (enumerator.MoveNext())
							{
								Room current = enumerator.Current;
								if ((long)current.OwnerId == (long)((ulong)num))
								{
									CyberEnvironment.GetGame().GetRoomManager().UnloadRoom(current);
									current.RequestReload();
								}
							}
							goto IL_3A3;
						}
					}
					uint userID3 = Convert.ToUInt32(array[0]);
					int id = Convert.ToInt32(array[1]);
					clientByUserID = CyberEnvironment.GetGame().GetClientManager().GetClientByUserID(userID3);
					if (clientByUserID != null && clientByUserID.GetHabbo() != null && clientByUserID.GetHabbo().GetInventoryComponent() != null)
					{
						clientByUserID.GetHabbo().GetInventoryComponent().UpdateItems(true);
						clientByUserID.GetHabbo().GetInventoryComponent().SendNewItems((uint)id);
					}
				}
				else
				{
					GameClient clientByUserID = CyberEnvironment.GetGame().GetClientManager().GetClientByUserID(Convert.ToUInt32(array[0]));
					clientByUserID.GetHabbo().Motto = MusConnection.MergeParams(array, 1);
					ServerMessage serverMessage = new ServerMessage(Outgoing.UpdateUserDataMessageComposer);
					serverMessage.AppendInt32(-1);
					serverMessage.AppendString(clientByUserID.GetHabbo().Look);
					serverMessage.AppendString(clientByUserID.GetHabbo().Gender.ToLower());
					serverMessage.AppendString(clientByUserID.GetHabbo().Motto);
					serverMessage.AppendInt32(clientByUserID.GetHabbo().AchievementPoints);
					clientByUserID.SendMessage(serverMessage);
					if (clientByUserID.GetHabbo().CurrentRoom != null)
					{
						RoomUser roomUserByHabbo = clientByUserID.GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(clientByUserID.GetHabbo().Username);
						ServerMessage serverMessage2 = new ServerMessage(Outgoing.UpdateUserDataMessageComposer);
						serverMessage2.AppendInt32(roomUserByHabbo.VirtualId);
						serverMessage2.AppendString(clientByUserID.GetHabbo().Look);
						serverMessage2.AppendString(clientByUserID.GetHabbo().Gender.ToLower());
						serverMessage2.AppendString(clientByUserID.GetHabbo().Motto);
						serverMessage2.AppendInt32(clientByUserID.GetHabbo().AchievementPoints);
						clientByUserID.GetHabbo().CurrentRoom.SendMessage(serverMessage2);
					}
				}
				IL_3A3:
				Logging.WriteLine("[MUS SOCKET] Comando MUS procesado correctamente: '" + text + "'", ConsoleColor.Green);
				return;
			}
			IL_38B:
			Logging.WriteLine("[MUS SOCKET] Paquete MUS no reconocido: " + text + "//" + data, ConsoleColor.DarkRed);
		}
		public static string MergeParams(string[] Params, int Start)
		{
			StringBuilder stringBuilder = new StringBuilder();
			checked
			{
				for (int i = 0; i < Params.Length; i++)
				{
					if (i >= Start)
					{
						if (i > Start)
						{
							stringBuilder.Append(" ");
						}
						stringBuilder.Append(Params[i]);
					}
				}
				return stringBuilder.ToString();
			}
		}
	}
}
