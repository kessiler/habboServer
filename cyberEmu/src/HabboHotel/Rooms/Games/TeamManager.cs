using Cyber.HabboHotel.Items;
using System;
using System.Collections.Generic;
using System.Drawing;
namespace Cyber.HabboHotel.Rooms.Games
{
	public class TeamManager
	{
		public string Game;
		public List<RoomUser> BlueTeam;
		public List<RoomUser> RedTeam;
		public List<RoomUser> YellowTeam;
		public List<RoomUser> GreenTeam;
		public static TeamManager createTeamforGame(string Game)
		{
			return new TeamManager
			{
				Game = Game,
				BlueTeam = new List<RoomUser>(),
				RedTeam = new List<RoomUser>(),
				GreenTeam = new List<RoomUser>(),
				YellowTeam = new List<RoomUser>()
			};
		}
		public bool CanEnterOnTeam(Team t)
		{
			if (t.Equals(Team.blue))
			{
				return this.BlueTeam.Count < 5;
			}
			if (t.Equals(Team.red))
			{
				return this.RedTeam.Count < 5;
			}
			if (t.Equals(Team.yellow))
			{
				return this.YellowTeam.Count < 5;
			}
			return t.Equals(Team.green) && this.GreenTeam.Count < 5;
		}
		public void AddUser(RoomUser user)
		{
			if (user.team.Equals(Team.blue))
			{
				this.BlueTeam.Add(user);
			}
			else
			{
				if (user.team.Equals(Team.red))
				{
					this.RedTeam.Add(user);
				}
				else
				{
					if (user.team.Equals(Team.yellow))
					{
						this.YellowTeam.Add(user);
					}
					else
					{
						if (user.team.Equals(Team.green))
						{
							this.GreenTeam.Add(user);
						}
					}
				}
			}
			string a;
			if ((a = this.Game.ToLower()) != null)
			{
				if (!(a == "banzai"))
				{
					if (!(a == "freeze"))
					{
						return;
					}
				}
				else
				{
					Room currentRoom = user.GetClient().GetHabbo().CurrentRoom;
					using (Dictionary<uint, RoomItem>.ValueCollection.Enumerator enumerator = currentRoom.GetRoomItemHandler().mFloorItems.Values.GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							RoomItem current = enumerator.Current;
							if (current.GetBaseItem().InteractionType.Equals(InteractionType.banzaigateblue))
							{
								current.ExtraData = this.BlueTeam.Count.ToString();
								current.UpdateState();
								if (this.BlueTeam.Count == 5)
								{
									foreach (RoomUser current2 in currentRoom.GetGameMap().GetRoomUsers(new Point(current.GetX, current.GetY)))
									{
										current2.SqState = 0;
									}
									currentRoom.GetGameMap().GameMap[current.GetX, current.GetY] = 0;
								}
							}
							else
							{
								if (current.GetBaseItem().InteractionType.Equals(InteractionType.banzaigatered))
								{
									current.ExtraData = this.RedTeam.Count.ToString();
									current.UpdateState();
									if (this.RedTeam.Count == 5)
									{
										foreach (RoomUser current3 in currentRoom.GetGameMap().GetRoomUsers(new Point(current.GetX, current.GetY)))
										{
											current3.SqState = 0;
										}
										currentRoom.GetGameMap().GameMap[current.GetX, current.GetY] = 0;
									}
								}
								else
								{
									if (current.GetBaseItem().InteractionType.Equals(InteractionType.banzaigategreen))
									{
										current.ExtraData = this.GreenTeam.Count.ToString();
										current.UpdateState();
										if (this.GreenTeam.Count == 5)
										{
											foreach (RoomUser current4 in currentRoom.GetGameMap().GetRoomUsers(new Point(current.GetX, current.GetY)))
											{
												current4.SqState = 0;
											}
											currentRoom.GetGameMap().GameMap[current.GetX, current.GetY] = 0;
										}
									}
									else
									{
										if (current.GetBaseItem().InteractionType.Equals(InteractionType.banzaigateyellow))
										{
											current.ExtraData = this.YellowTeam.Count.ToString();
											current.UpdateState();
											if (this.YellowTeam.Count == 5)
											{
												foreach (RoomUser current5 in currentRoom.GetGameMap().GetRoomUsers(new Point(current.GetX, current.GetY)))
												{
													current5.SqState = 0;
												}
												currentRoom.GetGameMap().GameMap[current.GetX, current.GetY] = 0;
											}
										}
									}
								}
							}
						}
						return;
					}
				}
				Room currentRoom2 = user.GetClient().GetHabbo().CurrentRoom;
				foreach (RoomItem current6 in currentRoom2.GetRoomItemHandler().mFloorItems.Values)
				{
					if (current6.GetBaseItem().InteractionType.Equals(InteractionType.freezebluegate))
					{
						current6.ExtraData = this.BlueTeam.Count.ToString();
						current6.UpdateState();
					}
					else
					{
						if (current6.GetBaseItem().InteractionType.Equals(InteractionType.freezeredgate))
						{
							current6.ExtraData = this.RedTeam.Count.ToString();
							current6.UpdateState();
						}
						else
						{
							if (current6.GetBaseItem().InteractionType.Equals(InteractionType.freezegreengate))
							{
								current6.ExtraData = this.GreenTeam.Count.ToString();
								current6.UpdateState();
							}
							else
							{
								if (current6.GetBaseItem().InteractionType.Equals(InteractionType.freezeyellowgate))
								{
									current6.ExtraData = this.YellowTeam.Count.ToString();
									current6.UpdateState();
								}
							}
						}
					}
				}
			}
		}
		public void OnUserLeave(RoomUser user)
		{
			if (user.team.Equals(Team.blue))
			{
				this.BlueTeam.Remove(user);
			}
			else
			{
				if (user.team.Equals(Team.red))
				{
					this.RedTeam.Remove(user);
				}
				else
				{
					if (user.team.Equals(Team.yellow))
					{
						this.YellowTeam.Remove(user);
					}
					else
					{
						if (user.team.Equals(Team.green))
						{
							this.GreenTeam.Remove(user);
						}
					}
				}
			}
			string a;
			if ((a = this.Game.ToLower()) != null)
			{
				if (!(a == "banzai"))
				{
					if (!(a == "freeze"))
					{
						return;
					}
				}
				else
				{
					Room currentRoom = user.GetClient().GetHabbo().CurrentRoom;
					using (Dictionary<uint, RoomItem>.ValueCollection.Enumerator enumerator = currentRoom.GetRoomItemHandler().mFloorItems.Values.GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							RoomItem current = enumerator.Current;
							if (current.GetBaseItem().InteractionType.Equals(InteractionType.banzaigateblue))
							{
								current.ExtraData = this.BlueTeam.Count.ToString();
								current.UpdateState();
								if (currentRoom.GetGameMap().GameMap[current.GetX, current.GetY] == 0)
								{
									foreach (RoomUser current2 in currentRoom.GetGameMap().GetRoomUsers(new Point(current.GetX, current.GetY)))
									{
										current2.SqState = 1;
									}
									currentRoom.GetGameMap().GameMap[current.GetX, current.GetY] = 1;
								}
							}
							else
							{
								if (current.GetBaseItem().InteractionType.Equals(InteractionType.banzaigatered))
								{
									current.ExtraData = this.RedTeam.Count.ToString();
									current.UpdateState();
									if (currentRoom.GetGameMap().GameMap[current.GetX, current.GetY] == 0)
									{
										foreach (RoomUser current3 in currentRoom.GetGameMap().GetRoomUsers(new Point(current.GetX, current.GetY)))
										{
											current3.SqState = 1;
										}
										currentRoom.GetGameMap().GameMap[current.GetX, current.GetY] = 1;
									}
								}
								else
								{
									if (current.GetBaseItem().InteractionType.Equals(InteractionType.banzaigategreen))
									{
										current.ExtraData = this.GreenTeam.Count.ToString();
										current.UpdateState();
										if (currentRoom.GetGameMap().GameMap[current.GetX, current.GetY] == 0)
										{
											foreach (RoomUser current4 in currentRoom.GetGameMap().GetRoomUsers(new Point(current.GetX, current.GetY)))
											{
												current4.SqState = 1;
											}
											currentRoom.GetGameMap().GameMap[current.GetX, current.GetY] = 1;
										}
									}
									else
									{
										if (current.GetBaseItem().InteractionType.Equals(InteractionType.banzaigateyellow))
										{
											current.ExtraData = this.YellowTeam.Count.ToString();
											current.UpdateState();
											if (currentRoom.GetGameMap().GameMap[current.GetX, current.GetY] == 0)
											{
												foreach (RoomUser current5 in currentRoom.GetGameMap().GetRoomUsers(new Point(current.GetX, current.GetY)))
												{
													current5.SqState = 1;
												}
												currentRoom.GetGameMap().GameMap[current.GetX, current.GetY] = 1;
											}
										}
									}
								}
							}
						}
						return;
					}
				}
				Room currentRoom2 = user.GetClient().GetHabbo().CurrentRoom;
				foreach (RoomItem current6 in currentRoom2.GetRoomItemHandler().mFloorItems.Values)
				{
					if (current6.GetBaseItem().InteractionType.Equals(InteractionType.freezebluegate))
					{
						current6.ExtraData = this.BlueTeam.Count.ToString();
						current6.UpdateState();
					}
					else
					{
						if (current6.GetBaseItem().InteractionType.Equals(InteractionType.freezeredgate))
						{
							current6.ExtraData = this.RedTeam.Count.ToString();
							current6.UpdateState();
						}
						else
						{
							if (current6.GetBaseItem().InteractionType.Equals(InteractionType.freezegreengate))
							{
								current6.ExtraData = this.GreenTeam.Count.ToString();
								current6.UpdateState();
							}
							else
							{
								if (current6.GetBaseItem().InteractionType.Equals(InteractionType.freezeyellowgate))
								{
									current6.ExtraData = this.YellowTeam.Count.ToString();
									current6.UpdateState();
								}
							}
						}
					}
				}
			}
		}
	}
}
