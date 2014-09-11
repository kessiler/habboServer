using Cyber.Collections;
using Cyber.HabboHotel.Items;
using System;
using System.Collections.Generic;
using System.Drawing;
using Cyber.HabboHotel.Rooms.Wired;
namespace Cyber.HabboHotel.Rooms.Games
{
	internal class GameManager
	{
		internal int[] TeamPoints;
		private QueuedDictionary<uint, RoomItem> redTeamItems;
		private QueuedDictionary<uint, RoomItem> blueTeamItems;
		private QueuedDictionary<uint, RoomItem> greenTeamItems;
		private QueuedDictionary<uint, RoomItem> yellowTeamItems;
		private Room room;
		internal event TeamScoreChangedDelegate OnScoreChanged;
		internal event RoomEventDelegate OnGameStart;
		internal event RoomEventDelegate OnGameEnd;
		internal int[] Points
		{
			get
			{
				return this.TeamPoints;
			}
			set
			{
				this.TeamPoints = value;
			}
		}
		internal void OnCycle()
		{
			this.redTeamItems.OnCycle();
			this.blueTeamItems.OnCycle();
			this.greenTeamItems.OnCycle();
			this.yellowTeamItems.OnCycle();
		}
		internal QueuedDictionary<uint, RoomItem> GetItems(Team team)
		{
			switch (team)
			{
			case Team.red:
				return this.redTeamItems;
			case Team.green:
				return this.greenTeamItems;
			case Team.blue:
				return this.blueTeamItems;
			case Team.yellow:
				return this.yellowTeamItems;
			default:
				return new QueuedDictionary<uint, RoomItem>();
			}
		}
		public GameManager(Room room)
		{
			this.TeamPoints = new int[5];
			this.redTeamItems = new QueuedDictionary<uint, RoomItem>();
			this.blueTeamItems = new QueuedDictionary<uint, RoomItem>();
			this.greenTeamItems = new QueuedDictionary<uint, RoomItem>();
			this.yellowTeamItems = new QueuedDictionary<uint, RoomItem>();
			this.room = room;
		}
		internal Team getWinningTeam()
		{
			int result = 1;
			int num = 0;
			checked
			{
				for (int i = 1; i < 5; i++)
				{
					if (this.TeamPoints[i] > num)
					{
						num = this.TeamPoints[i];
						result = i;
					}
				}
				return (Team)result;
			}
		}
		internal void AddPointToTeam(Team team, RoomUser user)
		{
			this.AddPointToTeam(team, 1, user);
		}

		internal void AddPointToTeam(Team team, int points, RoomUser user)
		{
			int num = checked(this.TeamPoints[(int)team] += points);
			if (num < 0)
			{
				num = 0;
			}
			this.TeamPoints[(int)team] = num;
			if (this.OnScoreChanged != null)
			{
				this.OnScoreChanged(null, new TeamScoreChangedArgs(num, team, user));
			}
			foreach (RoomItem current in this.GetFurniItems(team).Values)
			{
				if (!GameManager.isSoccerGoal(current.GetBaseItem().InteractionType))
				{
					current.ExtraData = this.TeamPoints[(int)team].ToString();
					current.UpdateState();
				}
			}

            room.GetWiredHandler().ExecuteWired(WiredItemType.TriggerScoreAchieved, new object[]
							{
								user
							});
		}
		internal void Reset()
		{
			checked
			{
				this.AddPointToTeam(Team.blue, this.GetScoreForTeam(Team.blue) * -1, null);
				this.AddPointToTeam(Team.green, this.GetScoreForTeam(Team.green) * -1, null);
				this.AddPointToTeam(Team.red, this.GetScoreForTeam(Team.red) * -1, null);
				this.AddPointToTeam(Team.yellow, this.GetScoreForTeam(Team.yellow) * -1, null);
			}
		}
		private int GetScoreForTeam(Team team)
		{
			return this.TeamPoints[(int)team];
		}
		private QueuedDictionary<uint, RoomItem> GetFurniItems(Team team)
		{
			switch (team)
			{
			case Team.red:
				return this.redTeamItems;
			case Team.green:
				return this.greenTeamItems;
			case Team.blue:
				return this.blueTeamItems;
			case Team.yellow:
				return this.yellowTeamItems;
			default:
				return new QueuedDictionary<uint, RoomItem>();
			}
		}
		private static bool isSoccerGoal(InteractionType type)
		{
			return type == InteractionType.footballgoalblue || type == InteractionType.footballgoalgreen || type == InteractionType.footballgoalred || type == InteractionType.footballgoalyellow;
		}
		internal void AddFurnitureToTeam(RoomItem item, Team team)
		{
			switch (team)
			{
			case Team.red:
				this.redTeamItems.Add(item.Id, item);
				return;
			case Team.green:
				this.greenTeamItems.Add(item.Id, item);
				return;
			case Team.blue:
				this.blueTeamItems.Add(item.Id, item);
				return;
			case Team.yellow:
				this.yellowTeamItems.Add(item.Id, item);
				return;
			default:
				return;
			}
		}
		internal void RemoveFurnitureFromTeam(RoomItem item, Team team)
		{
			switch (team)
			{
			case Team.red:
				this.redTeamItems.Remove(item.Id);
				return;
			case Team.green:
				this.greenTeamItems.Remove(item.Id);
				return;
			case Team.blue:
				this.blueTeamItems.Remove(item.Id);
				return;
			case Team.yellow:
				this.yellowTeamItems.Remove(item.Id);
				return;
			default:
				return;
			}
		}
		internal RoomItem GetFirstScoreBoard(Team team)
		{
			switch (team)
			{
			case Team.red:
				goto IL_BF;
			case Team.green:
				break;
			case Team.blue:
				using (Dictionary<uint, RoomItem>.ValueCollection.Enumerator enumerator = this.blueTeamItems.Values.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						RoomItem current = enumerator.Current;
						if (current.GetBaseItem().InteractionType == InteractionType.freezebluecounter)
						{
							RoomItem result = current;
							return result;
						}
					}
					goto IL_151;
				}
			case Team.yellow:
				goto IL_108;
			default:
				goto IL_151;
			}
			using (Dictionary<uint, RoomItem>.ValueCollection.Enumerator enumerator2 = this.greenTeamItems.Values.GetEnumerator())
			{
				while (enumerator2.MoveNext())
				{
					RoomItem current2 = enumerator2.Current;
					if (current2.GetBaseItem().InteractionType == InteractionType.freezegreencounter)
					{
						RoomItem result = current2;
						return result;
					}
				}
				goto IL_151;
			}
			IL_BF:
			using (Dictionary<uint, RoomItem>.ValueCollection.Enumerator enumerator3 = this.redTeamItems.Values.GetEnumerator())
			{
				while (enumerator3.MoveNext())
				{
					RoomItem current3 = enumerator3.Current;
					if (current3.GetBaseItem().InteractionType == InteractionType.freezeredcounter)
					{
						RoomItem result = current3;
						return result;
					}
				}
				goto IL_151;
			}
			IL_108:
			foreach (RoomItem current4 in this.yellowTeamItems.Values)
			{
				if (current4.GetBaseItem().InteractionType == InteractionType.freezeyellowcounter)
				{
					RoomItem result = current4;
					return result;
				}
			}
			IL_151:
			return null;
		}
		internal void UnlockGates()
		{
			foreach (RoomItem current in this.redTeamItems.Values)
			{
				this.UnlockGate(current);
			}
			foreach (RoomItem current2 in this.greenTeamItems.Values)
			{
				this.UnlockGate(current2);
			}
			foreach (RoomItem current3 in this.blueTeamItems.Values)
			{
				this.UnlockGate(current3);
			}
			foreach (RoomItem current4 in this.yellowTeamItems.Values)
			{
				this.UnlockGate(current4);
			}
		}
		private void LockGate(RoomItem item)
		{
			InteractionType interactionType = item.GetBaseItem().InteractionType;
			if (interactionType == InteractionType.freezebluegate || interactionType == InteractionType.freezegreengate || interactionType == InteractionType.freezeredgate || interactionType == InteractionType.freezeyellowgate || interactionType == InteractionType.banzaigateblue || interactionType == InteractionType.banzaigatered || interactionType == InteractionType.banzaigategreen || interactionType == InteractionType.banzaigateyellow)
			{
				foreach (RoomUser current in this.room.GetGameMap().GetRoomUsers(new Point(item.GetX, item.GetY)))
				{
					current.SqState = 0;
				}
				this.room.GetGameMap().GameMap[item.GetX, item.GetY] = 0;
			}
		}
		private void UnlockGate(RoomItem item)
		{
			InteractionType interactionType = item.GetBaseItem().InteractionType;
			if (interactionType == InteractionType.freezebluegate || interactionType == InteractionType.freezegreengate || interactionType == InteractionType.freezeredgate || interactionType == InteractionType.freezeyellowgate || interactionType == InteractionType.banzaigateblue || interactionType == InteractionType.banzaigatered || interactionType == InteractionType.banzaigategreen || interactionType == InteractionType.banzaigateyellow)
			{
				foreach (RoomUser current in this.room.GetGameMap().GetRoomUsers(new Point(item.GetX, item.GetY)))
				{
					current.SqState = 1;
				}
				this.room.GetGameMap().GameMap[item.GetX, item.GetY] = 1;
			}
		}
		internal void LockGates()
		{
			foreach (RoomItem current in this.redTeamItems.Values)
			{
				this.LockGate(current);
			}
			foreach (RoomItem current2 in this.greenTeamItems.Values)
			{
				this.LockGate(current2);
			}
			foreach (RoomItem current3 in this.blueTeamItems.Values)
			{
				this.LockGate(current3);
			}
			foreach (RoomItem current4 in this.yellowTeamItems.Values)
			{
				this.LockGate(current4);
			}
		}
		internal void StopGame()
		{
			if (this.OnGameEnd != null)
			{
				this.OnGameEnd(null, null);
			}
		}
		internal void StartGame()
		{
			if (this.OnGameStart != null)
			{
				this.OnGameStart(null, null);
			}
            GetRoom().GetWiredHandler().ResetExtraString(Wired.WiredItemType.EffectGiveScore);
		}
		internal Room GetRoom()
		{
			return this.room;
		}
		internal void Destroy()
		{
			Array.Clear(this.TeamPoints, 0, this.TeamPoints.Length);
			this.redTeamItems.Destroy();
			this.blueTeamItems.Destroy();
			this.greenTeamItems.Destroy();
			this.yellowTeamItems.Destroy();
			this.TeamPoints = null;
			this.OnScoreChanged = null;
			this.OnGameStart = null;
			this.OnGameEnd = null;
			this.redTeamItems = null;
			this.blueTeamItems = null;
			this.greenTeamItems = null;
			this.yellowTeamItems = null;
			this.room = null;
		}
	}
}
