using ConnectionManager;
using Database_Manager.Database.Session_Details.Interfaces;
using Cyber.Core;
using Cyber.HabboHotel.Misc;
using Cyber.HabboHotel.Users.Messenger;
using Cyber.Messages;
using Cyber.Messages.Headers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Specialized;

namespace Cyber.HabboHotel.GameClients
{
	internal class GameClientManager
	{
		internal HybridDictionary clients;
		private Queue clientsAddQueue;
		private Queue clientsToRemove;
		private Queue badgeQueue;
		private Queue broadcastQueue;
		private HybridDictionary usernameRegister;
		private HybridDictionary userIDRegister;
		private HybridDictionary usernameIdRegister;
		private HybridDictionary idUsernameRegister;
		private Queue timedOutConnections;

		internal int ClientCount
		{
			get
			{
				return this.clients.Count;
			}
		}
		internal GameClient GetClientByUserID(uint userID)
		{
            if (this.userIDRegister.Contains(userID))
			{
				return (GameClient)this.userIDRegister[userID];
			}
			return null;
		}
		internal GameClient GetClientByUsername(string username)
		{
            if (this.usernameRegister.Contains(username.ToLower()))
			{
				return (GameClient)this.usernameRegister[username.ToLower()];
			}
			return null;
		}
		internal GameClient GetClient(uint clientID)
		{
            if (this.clients.Contains(clientID))
			{
				return (GameClient)this.clients[clientID];
			}
			return null;
		}
		internal string GetNameById(uint Id)
		{
			GameClient clientByUserID = this.GetClientByUserID(Id);
			if (clientByUserID != null)
			{
				return clientByUserID.GetHabbo().Username;
			}
			string @string;
			using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
			{
				queryreactor.setQuery("SELECT username FROM users WHERE id = " + Id);
				@string = queryreactor.getString();
			}
			return @string;
		}
		internal IEnumerable<GameClient> GetClientsById(Dictionary<uint, MessengerBuddy>.KeyCollection users)
		{
			foreach (uint current in users)
			{
				GameClient clientByUserID = this.GetClientByUserID(current);
				if (clientByUserID != null)
				{
					yield return clientByUserID;
				}
			}
			yield break;
		}
		internal void SendSuperNotif(string Title, string Notice, string Picture, GameClient Client, string Link, string LinkTitle, bool Broadcast, bool Event)
		{
			ServerMessage serverMessage = new ServerMessage(Outgoing.SuperNotificationMessageComposer);
			serverMessage.AppendString(Picture);
			serverMessage.AppendInt32(4);
			serverMessage.AppendString("title");
			serverMessage.AppendString(Title);
			serverMessage.AppendString("message");
			if (Broadcast)
			{
				if (Event)
				{
					serverMessage.AppendString("<b>¡Nuevo evento abierto ahora en la Sala de " + Client.GetHabbo().CurrentRoom.Owner + "!</b> ¡Corre! No querrás ser el último en llegar.\r\n<b>Más detalles:</b>\r\n" + Notice);
				}
				else
				{
					serverMessage.AppendString(string.Concat(new string[]
					{
						"<b>Mensaje del Equipo Staff:</b>\r\n",
						Notice,
						"\r\n- <i>",
						Client.GetHabbo().Username,
						"</i>"
					}));
				}
			}
			else
			{
				serverMessage.AppendString(Notice);
			}
			if (Link != "")
			{
				serverMessage.AppendString("linkUrl");
				serverMessage.AppendString(Link);
                serverMessage.AppendString("linkTitle");
                serverMessage.AppendString(LinkTitle);
			}
            else
            {
                serverMessage.AppendString("linkUrl");
                serverMessage.AppendString("event:");
                serverMessage.AppendString("linkTitle");
                serverMessage.AppendString("ok");
            }
           
			if (Broadcast)
			{
				this.QueueBroadcaseMessage(serverMessage);
				return;
			}
			Client.SendMessage(serverMessage);
		}
		internal GameClientManager()
		{
			this.clients = new HybridDictionary();
			this.clientsAddQueue = new Queue();
			this.clientsToRemove = new Queue();
			this.badgeQueue = new Queue();
			this.broadcastQueue = new Queue();
			this.timedOutConnections = new Queue();
			this.usernameRegister = new HybridDictionary();
			this.userIDRegister = new HybridDictionary();
			this.usernameIdRegister = new HybridDictionary();
			this.idUsernameRegister = new HybridDictionary();
			Thread thread = new Thread(new ThreadStart(this.HandleTimeouts));
			thread.Start();
		}
        internal void OnCycle()
        {
            try
            {
                this.RemoveClients();
                this.AddClients();
                this.GiveBadges();
                this.BroadcastPackets();
                CyberEnvironment.GetGame().ClientManagerCycle_ended = true;
            }
            catch (Exception ex)
            {
                Logging.LogThreadException(ex.ToString(), "GameClientManager.OnCycle Exception --> Not inclusive");
            }
        }

		private void CheckCycleUpdates()
		{
			/*try
			{
				DateTime now = DateTime.Now;
				if (this.cyclePixelsEnabled && (DateTime.Now - this.cyclePixelsLastUpdate).TotalMilliseconds >= (double)this.cyclePixelsTime)
				{
					this.cyclePixelsLastUpdate = DateTime.Now;
					try
					{
						foreach (GameClient current in this.clients.Values)
						{
							if (current.GetHabbo() != null && current != null)
							{
								PixelManager.GivePixels(current, this.cyclePixelsAmount);
							}
						}
					}
					catch (Exception ex)
					{
						Logging.LogThreadException(ex.ToString(), "GCMExt.cyclePixelsEnabled task");
					}
				}
				if (this.cycleCreditsEnabled && (DateTime.Now - this.cycleCreditsLastUpdate).TotalMilliseconds >= (double)this.cycleCreditsTime)
				{
					this.cycleCreditsLastUpdate = DateTime.Now;
					try
					{
						foreach (GameClient current2 in this.clients.Values)
						{
							if (current2.GetHabbo() != null && current2 != null)
							{
								CreditManager.GiveCredits(current2, this.cycleCreditsAmount);
							}
						}
					}
					catch (Exception ex2)
					{
						Logging.LogThreadException(ex2.ToString(), "GCMExt.cycleCreditsEnabled task");
					}
				}
				//this.CheckEffects();
			}
			catch (Exception ex3)
			{
				Logging.LogThreadException(ex3.ToString(), "GameClientManager.CheckCycleUpdates Exception --> Not inclusive");
			}*/
		}
		private void TestClientConnections()
		{
			/*checked
			{
				if ((DateTime.Now - GameClientManager.pingLastExecution).TotalMilliseconds >= (double)this.pingInterval)
				{
					try
					{
						ServerMessage serverMessage = new ServerMessage(Outgoing.PingMessageComposer);
						List<GameClient> list = new List<GameClient>();
						foreach (GameClient current in this.clients.Values)
						{
							TimeSpan timeSpan = DateTime.Now - GameClientManager.pingLastExecution.AddMilliseconds((double)this.pingInterval);
							if (unchecked((DateTime.Now - current.TimePingedReceived).TotalMilliseconds - timeSpan.TotalMilliseconds) < (double)(this.pingInterval + 10000))
							{
								list.Add(current);
							}
							else
							{
								lock (this.timedOutConnections.SyncRoot)
								{
									this.timedOutConnections.Enqueue(current);
								}
							}
						}
						DateTime now = DateTime.Now;
						byte[] bytes = serverMessage.GetBytes();
						foreach (GameClient current2 in list)
						{
							try
							{
								if (current2.GetConnection() != null)
								{
									current2.GetConnection().SendUnsafeData(bytes);
								}
								else
								{
									lock (this.timedOutConnections.SyncRoot)
									{
										this.timedOutConnections.Enqueue(current2);
									}
								}
							}
							catch
							{
								lock (this.timedOutConnections.SyncRoot)
								{
									this.timedOutConnections.Enqueue(current2);
								}
							}
						}
						TimeSpan timeSpan2 = DateTime.Now - now;
						if (timeSpan2.TotalSeconds > 3.0)
						{
							Console.WriteLine("Spent seconds on testing: " + (int)timeSpan2.TotalSeconds);
						}
						if (timeSpan2.TotalSeconds > 3.0)
						{
							Console.WriteLine("Spent seconds on disconnecting: " + (int)timeSpan2.TotalSeconds);
						}
						list.Clear();
						list = null;
					}
					catch (Exception ex)
					{
						Logging.LogThreadException(ex.ToString(), "Connection checker task");
					}
					GameClientManager.pingLastExecution = DateTime.Now;
				}
			}*/
		}
		private void HandleTimeouts()
		{
			while (true)
			{
				try
				{
					while (this.timedOutConnections != null && this.timedOutConnections.Count > 0)
					{
						GameClient gameClient = null;
						lock (this.timedOutConnections.SyncRoot)
						{
							if (this.timedOutConnections.Count > 0)
							{
								gameClient = (GameClient)this.timedOutConnections.Dequeue();
							}
						}
						if (gameClient != null)
						{
							gameClient.Disconnect();
						}
					}
				}
				catch (Exception ex)
				{
					Logging.LogThreadException(ex.ToString(), "HandleTimeoutsVoid");
				}
				Thread.Sleep(5000);
			}
		}
		private void AddClients()
		{
			DateTime now = DateTime.Now;
			if (this.clientsAddQueue.Count > 0)
			{
				lock (this.clientsAddQueue.SyncRoot)
				{
					while (this.clientsAddQueue.Count > 0)
					{
						GameClient gameClient = (GameClient)this.clientsAddQueue.Dequeue();
						this.clients.Add(gameClient.ConnectionID, gameClient);
						gameClient.StartConnection();
					}
				}
			}
			TimeSpan timeSpan = DateTime.Now - now;
			if (timeSpan.TotalSeconds > 3.0)
			{
				Console.WriteLine("GameClientManager.AddClients spent: " + timeSpan.TotalSeconds + " seconds in working.");
			}
		}
		private void RemoveClients()
		{
			try
			{
				DateTime now = DateTime.Now;
				if (this.clientsToRemove.Count > 0)
				{
					lock (this.clientsToRemove.SyncRoot)
					{
						while (this.clientsToRemove.Count > 0)
						{
							uint key = (uint)this.clientsToRemove.Dequeue();
							this.clients.Remove(key);
						}
					}
				}
				TimeSpan timeSpan = DateTime.Now - now;
				if (timeSpan.TotalSeconds > 3.0)
				{
					Console.WriteLine("GameClientManager.RemoveClients spent: " + timeSpan.TotalSeconds + " seconds in working.");
				}
			}
			catch (Exception ex)
			{
				Logging.LogThreadException(ex.ToString(), "GameClientManager.RemoveClients Exception --> Not inclusive");
			}
		}
		
		private void GiveBadges()
		{
			try
			{
				DateTime now = DateTime.Now;
				if (this.badgeQueue.Count > 0)
				{
					lock (this.badgeQueue.SyncRoot)
					{
						while (this.badgeQueue.Count > 0)
						{
							string badge = (string)this.badgeQueue.Dequeue();
							foreach (GameClient current in this.clients.Values)
							{
								if (current.GetHabbo() != null)
								{
									try
									{
										current.GetHabbo().GetBadgeComponent().GiveBadge(badge, true, current, false);
										current.SendNotif("¡Has recibido una placa! Mira en tu inventario.");
									}
									catch
									{
									}
								}
							}
						}
					}
				}
				TimeSpan timeSpan = DateTime.Now - now;
				if (timeSpan.TotalSeconds > 3.0)
				{
					Console.WriteLine("GameClientManager.GiveBadges spent: " + timeSpan.TotalSeconds + " seconds in working.");
				}
			}
			catch (Exception ex)
			{
				Logging.LogThreadException(ex.ToString(), "GameClientManager.GiveBadges Exception --> Not inclusive");
			}
		}

		private void BroadcastPackets()
		{
			try
			{
				DateTime now = DateTime.Now;
				if (this.broadcastQueue.Count > 0)
				{
					lock (this.broadcastQueue.SyncRoot)
					{
						while (this.broadcastQueue.Count > 0)
						{
							ServerMessage serverMessage = (ServerMessage)this.broadcastQueue.Dequeue();
							byte[] bytes = serverMessage.GetBytes();
							foreach (GameClient current in this.clients.Values)
							{
								try
								{
									current.GetConnection().SendData(bytes);
								}
								catch
								{
								}
							}
						}
					}
				}
				TimeSpan timeSpan = DateTime.Now - now;
				if (timeSpan.TotalSeconds > 3.0)
				{
					Console.WriteLine("GameClientManager.BroadcastPackets spent: " + timeSpan.TotalSeconds + " seconds in working.");
				}
			}
			catch (Exception ex)
			{
				Logging.LogThreadException(ex.ToString(), "GameClientManager.BroadcastPackets Exception --> Not inclusive");
			}
		}

        internal void StaffAlert(ServerMessage Message, uint Exclude = 0u)
        {
            var gameClients = this.clients.Values.OfType<GameClient>().Where(x => x.GetHabbo() != null && x.GetHabbo().Rank >= CyberEnvironment.StaffAlertMinRank && x.GetHabbo().Id != Exclude);
            foreach (GameClient current in gameClients)
            {
                current.SendMessage(Message);
            }
        }

		internal void ModAlert(ServerMessage Message)
		{
			byte[] bytes = Message.GetBytes();
			foreach (GameClient current in this.clients.Values)
			{
				if (current != null && current.GetHabbo() != null)
				{
					if (current.GetHabbo().Rank != 4u && current.GetHabbo().Rank != 5u)
					{
						if (current.GetHabbo().Rank != 6u)
						{
							continue;
						}
					}
					try
					{
						current.GetConnection().SendData(bytes);
					}
					catch
					{
					}
				}
			}
		}
		internal void CreateAndStartClient(uint clientID, ConnectionInformation connection)
		{
			GameClient obj = new GameClient(clientID, connection);
            if (this.clients.Contains(clientID))
			{
				this.clients.Remove(clientID);
			}
			lock (this.clientsAddQueue.SyncRoot)
			{
				this.clientsAddQueue.Enqueue(obj);
			}
		}
		internal void DisposeConnection(uint clientID)
		{
			GameClient client = this.GetClient(clientID);
			if (client != null)
			{
				client.Stop();
			}
			lock (this.clientsToRemove.SyncRoot)
			{
				this.clientsToRemove.Enqueue(clientID);
			}
		}
		internal void QueueBroadcaseMessage(ServerMessage message)
		{
			lock (this.broadcastQueue.SyncRoot)
			{
				this.broadcastQueue.Enqueue(message);
			}
		}
		private void BroadcastMessage(ServerMessage message)
		{
			lock (this.broadcastQueue.SyncRoot)
			{
				this.broadcastQueue.Enqueue(message);
			}
		}
		internal void QueueBadgeUpdate(string badge)
		{
			lock (this.badgeQueue.SyncRoot)
			{
				this.badgeQueue.Enqueue(badge);
			}
		}
		internal void LogClonesOut(uint UserID)
		{
			GameClient clientByUserID = this.GetClientByUserID(UserID);
			if (clientByUserID != null)
			{
				clientByUserID.Disconnect();
			}
		}
		internal void RegisterClient(GameClient client, uint userID, string username)
		{
            if (this.usernameRegister.Contains(username.ToLower()))
			{
				this.usernameRegister[username.ToLower()] = client;
			}
			else
			{
				this.usernameRegister.Add(username.ToLower(), client);
			}
            if (this.userIDRegister.Contains(userID))
			{
				this.userIDRegister[userID] = client;
			}
			else
			{
				this.userIDRegister.Add(userID, client);
			}
            if (!this.usernameIdRegister.Contains(username))
			{
				this.usernameIdRegister.Add(username, userID);
			}
            if (!this.idUsernameRegister.Contains(userID))
			{
				this.idUsernameRegister.Add(userID, username);
			}
			using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
			{
				queryreactor.setQuery("UPDATE users SET online='1' WHERE id=" + userID + " LIMIT 1");
			}
		}
		internal void UnregisterClient(uint userid, string username)
		{
			this.userIDRegister.Remove(userid);
			this.usernameRegister.Remove(username.ToLower());
			using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
			{
				queryreactor.setQuery("UPDATE users SET online='0' WHERE id=" + userid + " LIMIT 1");
			}
		}
		internal void CloseAll()
		{
			StringBuilder stringBuilder = new StringBuilder();
			bool flag = false;
			int num = 0;
			checked
			{
				foreach (GameClient current in this.clients.Values)
				{
					if (current.GetHabbo() != null)
					{
						num++;
					}
				}
				if (num < 1)
				{
					num = 1;
				}
				int num2 = 0;
				int count = this.clients.Count;
				foreach (GameClient current2 in this.clients.Values)
				{
					num2++;
					if (current2.GetHabbo() != null)
					{
						try
						{
							current2.GetHabbo().GetInventoryComponent().RunDBUpdate();
							current2.GetHabbo().RunDBUpdate(CyberEnvironment.GetDatabaseManager().getQueryReactor());
							stringBuilder.Append(current2.GetHabbo().GetQueryString);
							flag = true;
							Console.Clear();
							Console.WriteLine();
							Console.ForegroundColor = ConsoleColor.Gray;
							Console.WriteLine("<<- APAGANDO ->> GUARDADO DE INVENTARIOS: " + string.Format("{0:0.##}", unchecked((double)num2 / (double)num * 100.0)) + "%");
						}
						catch
						{
						}
					}
				}
				if (flag)
				{
					try
					{
						if (stringBuilder.Length > 0)
						{
							using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
							{
								queryreactor.runFastQuery(stringBuilder.ToString());
							}
						}
					}
					catch (Exception pException)
					{
						Logging.HandleException(pException, "GameClientManager.CloseAll()");
					}
				}
				try
				{
					int num3 = 0;
					foreach (GameClient current3 in this.clients.Values)
					{
						num3++;
						if (current3.GetConnection() != null)
						{
							try
							{
								current3.GetConnection().Dispose();
							}
							catch
							{
							}
							Console.Clear();
							Console.WriteLine();
							Console.ForegroundColor = ConsoleColor.Gray;
							Console.WriteLine("<<- SHUTTING DOWN ->> CLOSING CONNECTIONS: " + string.Format("{0:0.##}", unchecked((double)num3 / (double)count * 100.0)) + "%");
						}
					}
				}
				catch (Exception ex)
				{
					Logging.LogCriticalException(ex.ToString());
				}
				this.clients.Clear();
				Console.WriteLine("Connections were closed!");
			}
		}

        internal void UpdateClient(string OldName, string NewName)
        {
            if (usernameRegister.Contains(OldName.ToLower()))
            {
                GameClient old = (GameClient)usernameRegister[OldName.ToLower()];
                usernameRegister.Remove(OldName.ToLower());
                usernameRegister.Add(NewName.ToLower(), old);
            }
        }
    }
}
