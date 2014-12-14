using Cyber.Collections;
using Cyber.HabboHotel.GameClients;
using Cyber.HabboHotel.Items;
using Cyber.HabboHotel.PathFinding;
using Cyber.HabboHotel.Pathfinding;
using Cyber.Messages;
using Cyber.Messages.Headers;
using Cyber.Util;
using System;
using System.Collections.Generic;
using System.Drawing;
namespace Cyber.HabboHotel.Rooms.Games
{
	internal class Soccer
	{
		private RoomItem[] gates;
		private Room room;
		private QueuedDictionary<uint, RoomItem> balls;
		public Soccer(Room room)
		{
			this.room = room;
			this.gates = new RoomItem[4];
			this.balls = new QueuedDictionary<uint, RoomItem>();
		}
		internal void AddBall(RoomItem item)
		{
			this.balls.Add(item.Id, item);
		}
		internal void Destroy()
		{
			Array.Clear(this.gates, 0, this.gates.Length);
			this.gates = null;
			this.room = null;
			this.balls.Destroy();
			this.balls = null;
		}
		private bool GameItemOverlaps(RoomItem gameItem)
		{
			Point coordinate = gameItem.Coordinate;
			foreach (RoomItem current in this.GetFootballItemsForAllTeams())
			{
				foreach (ThreeDCoord current2 in current.GetAffectedTiles.Values)
				{
					if (current2.X == coordinate.X && current2.Y == coordinate.Y)
					{
						return true;
					}
				}
			}
			return false;
		}
		private List<RoomItem> GetFootballItemsForAllTeams()
		{
			List<RoomItem> list = new List<RoomItem>();
			foreach (RoomItem current in this.room.GetGameManager().GetItems(Team.red).Values)
			{
				list.Add(current);
			}
			foreach (RoomItem current2 in this.room.GetGameManager().GetItems(Team.green).Values)
			{
				list.Add(current2);
			}
			foreach (RoomItem current3 in this.room.GetGameManager().GetItems(Team.blue).Values)
			{
				list.Add(current3);
			}
			foreach (RoomItem current4 in this.room.GetGameManager().GetItems(Team.yellow).Values)
			{
				list.Add(current4);
			}
			return list;
		}
		internal int GetThreadTime(int i)
		{
			int result;
			if (i == 1)
			{
				result = 75;
			}
			else
			{
				if (i == 2)
				{
					result = 100;
				}
				else
				{
					if (i == 3)
					{
						result = 125;
					}
					else
					{
						if (i == 4)
						{
							result = 150;
						}
						else
						{
							if (i != 5)
							{
								result = ((i != 6) ? 200 : 350);
							}
							else
							{
								result = 200;
							}
						}
					}
				}
			}
			return result;
		}
		private bool HandleFootballGameItems(Point ballItemCoord, RoomUser user)
		{
			foreach (RoomItem current in this.room.GetGameManager().GetItems(Team.red).Values)
			{
				foreach (ThreeDCoord current2 in current.GetAffectedTiles.Values)
				{
					if (current2.X == ballItemCoord.X && current2.Y == ballItemCoord.Y)
					{
						this.room.GetGameManager().AddPointToTeam(Team.red, user);
						ServerMessage serverMessage = new ServerMessage(Outgoing.RoomUserActionMessageComposer);
						serverMessage.AppendInt32(user.VirtualId);
						serverMessage.AppendInt32(1);
						user.GetClient().GetHabbo().CurrentRoom.SendMessage(serverMessage);
						bool flag = true;
						bool result = flag;
						return result;
					}
				}
			}
			foreach (RoomItem current3 in this.room.GetGameManager().GetItems(Team.green).Values)
			{
				foreach (ThreeDCoord current4 in current3.GetAffectedTiles.Values)
				{
					if (current4.X == ballItemCoord.X && current4.Y == ballItemCoord.Y)
					{
						this.room.GetGameManager().AddPointToTeam(Team.green, user);
                        ServerMessage serverMessage = new ServerMessage(Outgoing.RoomUserActionMessageComposer);
						serverMessage.AppendInt32(user.VirtualId);
						serverMessage.AppendInt32(1);
						user.GetClient().GetHabbo().CurrentRoom.SendMessage(serverMessage);
						bool flag = true;
						bool result = flag;
						return result;
					}
				}
			}
			foreach (RoomItem current5 in this.room.GetGameManager().GetItems(Team.blue).Values)
			{
				foreach (ThreeDCoord current6 in current5.GetAffectedTiles.Values)
				{
					if (current6.X == ballItemCoord.X && current6.Y == ballItemCoord.Y)
					{
						this.room.GetGameManager().AddPointToTeam(Team.blue, user);
                        ServerMessage serverMessage = new ServerMessage(Outgoing.RoomUserActionMessageComposer);
						serverMessage.AppendInt32(user.VirtualId);
						serverMessage.AppendInt32(1);
						user.GetClient().GetHabbo().CurrentRoom.SendMessage(serverMessage);
						bool flag = true;
						bool result = flag;
						return result;
					}
				}
			}
			foreach (RoomItem current7 in this.room.GetGameManager().GetItems(Team.yellow).Values)
			{
				foreach (ThreeDCoord current8 in current7.GetAffectedTiles.Values)
				{
					if (current8.X == ballItemCoord.X && current8.Y == ballItemCoord.Y)
					{
						this.room.GetGameManager().AddPointToTeam(Team.yellow, user);
                        ServerMessage serverMessage = new ServerMessage(Outgoing.RoomUserActionMessageComposer);
						serverMessage.AppendInt32(user.VirtualId);
						serverMessage.AppendInt32(1);
						user.GetClient().GetHabbo().CurrentRoom.SendMessage(serverMessage);
						bool flag = true;
						bool result = flag;
						return result;
					}
				}
			}
			return false;
		}

		internal void OnCycle()
		{
			this.balls.OnCycle();
		}

		internal void onGateRemove(RoomItem item)
		{
			switch (item.GetBaseItem().InteractionType)
			{
			case InteractionType.footballgoalgreen:
			case InteractionType.footballcountergreen:
				this.room.GetGameManager().RemoveFurnitureFromTeam(item, Team.green);
				return;
			case InteractionType.footballgoalyellow:
			case InteractionType.footballcounteryellow:
				this.room.GetGameManager().RemoveFurnitureFromTeam(item, Team.yellow);
				return;
			case InteractionType.footballgoalblue:
			case InteractionType.footballcounterblue:
				this.room.GetGameManager().RemoveFurnitureFromTeam(item, Team.blue);
				return;
			case InteractionType.footballgoalred:
			case InteractionType.footballcounterred:
				this.room.GetGameManager().RemoveFurnitureFromTeam(item, Team.red);
				return;
			default:
				return;
			}
		}

        internal void OnUserWalk(RoomUser User)
        {
            if (User == null)
                return;
            foreach (RoomItem item in balls.Values)
            {
                int NewX = 0;
                int NewY = 0;
                int differenceX = User.X - item.GetX;
                int differenceY = User.Y - item.GetY;

                if (differenceX == 0 && differenceY == 0)
                {
                    //DEVOLVER HACIA ATRAS.

                    if (User.RotBody == 4)
                    {
                        NewX = User.X;
                        NewY = User.Y + 2;

                    }
                    else if (User.RotBody == 6)
                    {
                        NewX = User.X - 2;
                        NewY = User.Y;

                    }
                    else if (User.RotBody == 0)
                    {
                        NewX = User.X;
                        NewY = User.Y - 2;

                    }
                    else if (User.RotBody == 2)
                    {
                        NewX = User.X + 2;
                        NewY = User.Y;

                    }//DIAGONALES
                    else if (User.RotBody == 1)
                    {
                        NewX = User.X + 2;
                        NewY = User.Y - 2;

                    }
                    else if (User.RotBody == 7)
                    {
                        NewX = User.X - 2;
                        NewY = User.Y - 2;

                    }
                    else if (User.RotBody == 3)
                    {
                        NewX = User.X + 2;
                        NewY = User.Y + 2;

                    }
                    else if (User.RotBody == 5)
                    {
                        NewX = User.X - 2;
                        NewY = User.Y + 2;

                    }
                    if (!this.room.GetRoomItemHandler().CheckPosItem(User.GetClient(), item, NewX, NewY, item.Rot, false, false))
                    {


                        if (User.RotBody == 0)
                        {
                            NewX = User.X;
                            NewY = User.Y + 1;
                        }
                        else if (User.RotBody == 2)
                        {
                            NewX = User.X - 1;
                            NewY = User.Y;
                        }
                        else if (User.RotBody == 4)
                        {
                            NewX = User.X;
                            NewY = User.Y - 1;
                        }
                        else if (User.RotBody == 6)
                        {
                            NewX = User.X + 1;
                            NewY = User.Y;
                        }
                        else if (User.RotBody == 5)
                        {
                            NewX = User.X + 1;
                            NewY = User.Y - 1;
                        }
                        else if (User.RotBody == 3)
                        {
                            NewX = User.X - 1;
                            NewY = User.Y - 1;
                        }
                        else if (User.RotBody == 7)
                        {
                            NewX = User.X + 1;
                            NewY = User.Y + 1;
                        }
                        else if (User.RotBody == 1)
                        {
                            NewX = User.X - 1;
                            NewY = User.Y + 1;
                        }
                    }

                }
                else if (differenceX <= 1 && differenceX >= -1 && differenceY <= 1 && differenceY >= -1 && veryficball(User, item.Coordinate.X, 0, item.Coordinate.Y, 0))
                {
                    NewX = differenceX * -1;
                    NewY = differenceY * -1;

                    NewX = NewX + item.GetX;
                    NewY = NewY + item.GetY;

                }

                if (item.interactingBallUser == User.UserID && item.GetRoom().GetGameMap().ValidTile(NewX, NewY))
                {
                    item.interactingBallUser = 0;
                    MoveBall(item, User.GetClient(), User.Coordinate, item.Coordinate, 6, User);
                }
                else if (item.GetRoom().GetGameMap().ValidTile(NewX, NewY))
                {
                    MoveBall(item, User.GetClient(), NewX, NewY, User);
                }

            }
        }

        internal void MoveBall(RoomItem item, GameClient client, Point user, Point ball, int length, RoomUser useroom)
        { }


        internal void MoveBall(RoomItem item, GameClient mover, int newX, int newY, RoomUser user)
        {
            if (item == null || mover == null)
                return;

            if (!room.GetGameMap().itemCanBePlacedHere(newX, newY))
                return;

            Point oldRoomCoord = item.Coordinate;
            // bool itemIsOnGameItem = GameItemOverlaps(item);

            Double NewZ = room.GetGameMap().Model.SqFloorHeight[newX, newY];


            ServerMessage mMessage = new ServerMessage();
            mMessage.Init(Outgoing.ItemAnimationMessageComposer); // Cf
            mMessage.AppendInt32(item.Coordinate.X);
            mMessage.AppendInt32(item.Coordinate.Y);
            mMessage.AppendInt32(newX);
            mMessage.AppendInt32(newY);
            mMessage.AppendInt32(1);
            mMessage.AppendUInt(item.Id);
            mMessage.AppendString(TextHandling.GetString(item.GetZ));
            mMessage.AppendString(TextHandling.GetString(NewZ));
            mMessage.AppendUInt(item.Id);
            room.SendMessage(mMessage);
            item.ExtraData = "11";
            item.UpdateNeeded = true;
            if (oldRoomCoord.X == newX && oldRoomCoord.Y == newY)
                return;

          
            if (!room.GetRoomItemHandler().SetFloorItem(mover, item, newX, newY, item.Rot, false, false, false, false))
            {
                room.GetRoomItemHandler().SetFloorItem(item, newX, newY, NewZ);
            }
            HandleFootballGameItems(item.Coordinate, user);
        }


        private bool veryficball(RoomUser user, int actualx, int nexx, int actualy, int nexy)
        {
            return PathFinder.CalculateRotation(user.X, user.Y, actualx, actualy) == user.RotBody;
        }

		internal void RegisterGate(RoomItem item)
		{
			if (this.gates[0] == null)
			{
				item.team = Team.blue;
				this.gates[0] = item;
				return;
			}
			if (this.gates[1] == null)
			{
				item.team = Team.red;
				this.gates[1] = item;
				return;
			}
			if (this.gates[2] == null)
			{
				item.team = Team.green;
				this.gates[2] = item;
				return;
			}
			if (this.gates[3] == null)
			{
				item.team = Team.yellow;
				this.gates[3] = item;
			}
		}
		internal void RemoveBall(uint itemID)
		{
			this.balls.Remove(itemID);
		}
		internal void UnRegisterGate(RoomItem item)
		{
			switch (item.team)
			{
			case Team.red:
				this.gates[1] = null;
				return;
			case Team.green:
				this.gates[2] = null;
				return;
			case Team.blue:
				this.gates[0] = null;
				return;
			case Team.yellow:
				this.gates[3] = null;
				return;
			default:
				return;
			}
		}
	}
}
