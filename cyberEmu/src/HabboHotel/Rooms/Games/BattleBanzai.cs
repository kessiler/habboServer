using enclosuretest;
using Cyber.Collections;
using Cyber.HabboHotel.GameClients;
using Cyber.HabboHotel.Items;
using Cyber.HabboHotel.Rooms.Wired;
using Cyber.Messages;
using Cyber.Messages.Headers;
using Cyber.Util;
using System;
using System.Collections;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Drawing;
namespace Cyber.HabboHotel.Rooms.Games
{
    internal class BattleBanzai
    {
        private Room room;
        internal HybridDictionary banzaiTiles;
        private bool banzaiStarted;
        private QueuedDictionary<uint, RoomItem> pucks;
        private byte[,] floorMap;
        private GameField field;
        private double timestarted;

        internal bool isBanzaiActive
        {
            get
            {
                return this.banzaiStarted;
            }
        }

        public BattleBanzai(Room room)
        {
            this.room = room;
            this.banzaiTiles = new HybridDictionary();
            this.banzaiStarted = false;
            this.pucks = new QueuedDictionary<uint, RoomItem>();
            this.timestarted = 0.0;
        }

        internal void AddTile(RoomItem item, uint itemID)
        {
            if (this.banzaiTiles.Contains(itemID))
                return;
            this.banzaiTiles.Add(itemID, item);
        }

        internal void RemoveTile(uint itemID)
        {
            this.banzaiTiles.Remove(itemID);
        }

        internal void OnCycle()
        {
            this.pucks.OnCycle();
        }

        internal void AddPuck(RoomItem item)
        {
            if (this.pucks.ContainsKey(item.Id))
                return;
            this.pucks.Add(item.Id, item);
        }

        internal void RemovePuck(uint itemID)
        {
            this.pucks.Remove(itemID);
        }

        internal void OnUserWalk(RoomUser User)
        {
            if (User == null)
                return;
            foreach (RoomItem roomItem in this.pucks.Values)
            {
                int num1 = checked(User.X - roomItem.GetX);
                int num2 = checked(User.Y - roomItem.GetY);
                if (num1 <= 1 && num1 >= -1 && num2 <= 1 && num2 >= -1)
                {
                    int num3 = checked(num1 * -1);
                    int num4 = checked(num2 * -1);
                    int num5 = checked(num3 + roomItem.GetX);
                    int num6 = checked(num4 + roomItem.GetY);
                    if ((int)roomItem.interactingBallUser == (int)User.UserID && this.room.GetGameMap().ValidTile(num5, num6))
                    {
                        roomItem.interactingBallUser = 0U;
                        this.MovePuck(roomItem, User.GetClient(), User.Coordinate, roomItem.Coordinate, 6, User.team);
                    }
                    else if (this.room.GetGameMap().ValidTile(num5, num6))
                        this.MovePuck(roomItem, User.GetClient(), num5, num6, User.team);
                }
            }
            if (!this.banzaiStarted)
                return;
            this.HandleBanzaiTiles(User.Coordinate, User.team, User);
        }

        internal void BanzaiStart()
        {
            if (this.banzaiStarted)
                return;
            this.room.GetGameManager().StartGame();
            this.floorMap = new byte[this.room.GetGameMap().Model.MapSizeY, this.room.GetGameMap().Model.MapSizeX];
            this.field = new GameField(this.floorMap, true);
            this.timestarted = (double)CyberEnvironment.GetUnixTimestamp();
            this.room.GetGameManager().LockGates();
            int index = 1;
            while (index < 5)
            {
                this.room.GetGameManager().Points[index] = 0;
                checked { ++index; }
            }
            foreach (RoomItem roomItem in (IEnumerable)this.banzaiTiles.Values)
            {
                roomItem.ExtraData = "1";
                roomItem.value = 0;
                roomItem.team = Team.none;
                roomItem.UpdateState();
            }
            this.room.GetRoomItemHandler().mFloorItems.QueueDelegate(new onCycleDoneDelegate(this.ResetTiles));
            this.banzaiStarted = true;
            this.room.GetWiredHandler().ExecuteWired(WiredItemType.TriggerGameStarts, new object[0]);
            foreach (RoomUser roomUser in this.room.GetRoomUserManager().GetRoomUsers())
                roomUser.LockedTilesCount = 0;
        }

        internal void ResetTiles()
        {
            foreach (RoomItem roomItem in this.room.GetRoomItemHandler().mFloorItems.Values)
            {
                switch (roomItem.GetBaseItem().InteractionType)
                {
                    case InteractionType.banzaiscoreblue:
                    case InteractionType.banzaiscorered:
                    case InteractionType.banzaiscoreyellow:
                    case InteractionType.banzaiscoregreen:
                        roomItem.ExtraData = "0";
                        roomItem.UpdateState();
                        break;
                }
            }
        }

        internal void BanzaiEnd()
        {
            this.banzaiStarted = false;
            this.room.GetGameManager().StopGame();
            this.floorMap = (byte[,])null;
            this.room.GetWiredHandler().ExecuteWired(WiredItemType.TriggerGameEnds, new object[0]);
            Team winningTeam = this.room.GetGameManager().getWinningTeam();
            this.room.GetGameManager().UnlockGates();
            foreach (RoomItem roomItem in (IEnumerable)this.banzaiTiles.Values)
            {
                if (roomItem.team == winningTeam)
                {
                    roomItem.interactionCount = (byte)0;
                    roomItem.interactionCountHelper = (byte)0;
                    roomItem.UpdateNeeded = true;
                }
                else if (roomItem.team == Team.none)
                {
                    roomItem.ExtraData = "0";
                    roomItem.UpdateState();
                }
            }
            if (winningTeam == Team.none)
                return;
            foreach (RoomUser Avatar in this.room.GetRoomUserManager().GetRoomUsers())
            {
                if (Avatar.team != Team.none && (double)CyberEnvironment.GetUnixTimestamp() - this.timestarted > 5.0)
                {
                    CyberEnvironment.GetGame().GetAchievementManager().ProgressUserAchievement(Avatar.GetClient(), "ACH_BattleBallTilesLocked", Avatar.LockedTilesCount, false);
                    CyberEnvironment.GetGame().GetAchievementManager().ProgressUserAchievement(Avatar.GetClient(), "ACH_BattleBallPlayer", 1, false);
                }

                if (winningTeam == Team.blue)
                {
                    if (Avatar.CurrentEffect == 35)
                    {
                        if ((double)CyberEnvironment.GetUnixTimestamp() - this.timestarted > 5.0)
                            CyberEnvironment.GetGame().GetAchievementManager().ProgressUserAchievement(Avatar.GetClient(), "ACH_BattleBallWinner", 1, false);
                        ServerMessage waveAtWin = new ServerMessage(Outgoing.RoomUserActionMessageComposer);
                        waveAtWin.AppendInt32(Avatar.VirtualId);
                        waveAtWin.AppendInt32(1);
                        this.room.SendMessage(waveAtWin);
                    }
                }
                else if (winningTeam == Team.red)
                {
                    if (Avatar.CurrentEffect == 33)
                    {
                        if ((double)CyberEnvironment.GetUnixTimestamp() - this.timestarted > 5.0)
                            CyberEnvironment.GetGame().GetAchievementManager().ProgressUserAchievement(Avatar.GetClient(), "ACH_BattleBallWinner", 1, false);
                        ServerMessage waveAtWin = new ServerMessage(Outgoing.RoomUserActionMessageComposer);
                        waveAtWin.AppendInt32(Avatar.VirtualId);
                        waveAtWin.AppendInt32(1);
                        this.room.SendMessage(waveAtWin);
                    }
                }
                else if (winningTeam == Team.green)
                {
                    if (Avatar.CurrentEffect == 34)
                    {
                        if ((double)CyberEnvironment.GetUnixTimestamp() - this.timestarted > 5.0)
                            CyberEnvironment.GetGame().GetAchievementManager().ProgressUserAchievement(Avatar.GetClient(), "ACH_BattleBallWinner", 1, false);
                        ServerMessage waveAtWin = new ServerMessage(Outgoing.RoomUserActionMessageComposer);
                        waveAtWin.AppendInt32(Avatar.VirtualId);
                        waveAtWin.AppendInt32(1);
                        this.room.SendMessage(waveAtWin);
                    }
                }
                else if (winningTeam == Team.yellow && Avatar.CurrentEffect == 36)
                {
                    if ((double)CyberEnvironment.GetUnixTimestamp() - this.timestarted > 5.0)
                        CyberEnvironment.GetGame().GetAchievementManager().ProgressUserAchievement(Avatar.GetClient(), "ACH_BattleBallWinner", 1, false);
                    ServerMessage waveAtWin = new ServerMessage(Outgoing.RoomUserActionMessageComposer);
                    waveAtWin.AppendInt32(Avatar.VirtualId);
                    waveAtWin.AppendInt32(1);
                    this.room.SendMessage(waveAtWin);
                }
            }
            this.field.destroy();
        }

        internal void MovePuck(RoomItem item, GameClient mover, int newX, int newY, Team team)
        {
            if (!this.room.GetGameMap().itemCanBePlacedHere(newX, newY))
                return;
            Point coordinate1 = item.Coordinate;
            double k = (double)this.room.GetGameMap().Model.SqFloorHeight[newX, newY];
            if (coordinate1.X == newX && coordinate1.Y == newY)
                return;
            item.ExtraData = ((int)team).ToString();
            item.UpdateNeeded = true;
            item.UpdateState();
            ServerMessage serverMessage1 = new ServerMessage();
            serverMessage1.Init(Outgoing.ItemAnimationMessageComposer);
            ServerMessage serverMessage2 = serverMessage1;
            Point coordinate2 = item.Coordinate;
            int x = coordinate2.X;
            serverMessage2.AppendInt32(x);
            ServerMessage serverMessage3 = serverMessage1;
            coordinate2 = item.Coordinate;
            int y = coordinate2.Y;
            serverMessage3.AppendInt32(y);
            serverMessage1.AppendInt32(newX);
            serverMessage1.AppendInt32(newY);
            serverMessage1.AppendInt32(1);
            serverMessage1.AppendUInt(item.Id);
            serverMessage1.AppendString(TextHandling.GetString(item.GetZ));
            serverMessage1.AppendString(TextHandling.GetString(k));
            serverMessage1.AppendInt32(1);
            this.room.SendMessage(serverMessage1);
            this.room.GetRoomItemHandler().SetFloorItem(mover, item, newX, newY, item.Rot, false, false, false, false);
            if (mover == null || mover.GetHabbo() == null)
                return;
            RoomUser roomUserByHabbo = mover.GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(mover.GetHabbo().Id);
            if (!this.banzaiStarted)
                return;
            this.HandleBanzaiTiles(new Point(newX, newY), team, roomUserByHabbo);
        }

        internal void MovePuck(RoomItem item, GameClient client, Point user, Point ball, int length, Team team)
        {
            int num1 = checked(user.X - ball.X);
            int num2 = checked(user.Y - ball.Y);
            if (num1 > 1 || num1 < -1 || num2 > 1 || num2 < -1)
                return;
            List<Point> list = new List<Point>();
            int num3 = ball.X;
            int num4 = ball.Y;
            int num5 = 1;
            while (num5 < length)
            {
                int num6 = checked(num1 * -num5);
                int num7 = checked(num2 * -num5);
                num3 = checked(num6 + item.GetX);
                num4 = checked(num7 + item.GetY);
                if (!this.room.GetGameMap().itemCanBePlacedHere(num3, num4))
                {
                    if (num5 != 1)
                    {
                        if (num5 != length)
                            list.Add(new Point(num3, num4));
                        int num8 = checked(num5 - 1);
                        int num9 = checked(num1 * -num8);
                        int num10 = checked(num2 * -num8);
                        num3 = checked(num9 + item.GetX);
                        num4 = checked(num10 + item.GetY);
                        break;
                    }
                    else
                        break;
                }
                else
                {
                    if (num5 != length)
                        list.Add(new Point(num3, num4));
                    checked { ++num5; }
                }
            }
            if (client == null || client.GetHabbo() == null)
                return;
            RoomUser roomUserByHabbo = client.GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(client.GetHabbo().Id);
            foreach (Point coord in list)
                this.HandleBanzaiTiles(coord, team, roomUserByHabbo);
            if (num3 != ball.X || num4 != ball.Y)
                this.MovePuck(item, client, num3, num4, team);
        }

        private void SetTile(RoomItem item, Team team, RoomUser user)
        {
            if (item.team == team)
            {
                if (item.value < 3)
                {
                    checked { ++item.value; }
                    if (item.value == 3)
                    {
                        checked { ++user.LockedTilesCount; }
                        this.room.GetGameManager().AddPointToTeam(item.team, user);
                        this.field.updateLocation(item.GetX, item.GetY, checked((byte)(uint)team));
                        foreach (PointField pointField in this.field.doUpdate(false))
                        {
                            Team team1 = (Team)pointField.forValue;
                            foreach (Point point in pointField.getPoints())
                            {
                                this.HandleMaxBanzaiTiles(new Point(point.X, point.Y), team1, user);
                                this.floorMap[point.Y, point.X] = pointField.forValue;
                            }
                        }
                    }
                }
            }
            else if (item.value < 3)
            {
                item.team = team;
                item.value = 1;
            }
            int num = checked(item.value + unchecked((int)item.team) * 3 - 1);
            item.ExtraData = num.ToString();
        }

        private void HandleBanzaiTiles(Point coord, Team team, RoomUser user)
        {
            if (team == Team.none)
                return;
            this.room.GetGameMap().GetCoordinatedItems(coord);
            int num = 0;
            foreach (RoomItem roomItem in (IEnumerable)this.banzaiTiles.Values)
            {
                if (roomItem.GetBaseItem().InteractionType != InteractionType.banzaifloor)
                {
                    user.team = Team.none;
                    user.ApplyEffect(0);
                }
                else if (roomItem.ExtraData.Equals("5") || roomItem.ExtraData.Equals("8") || roomItem.ExtraData.Equals("11") || roomItem.ExtraData.Equals("14"))
                    checked { ++num; }
                else if (roomItem.GetX == coord.X && roomItem.GetY == coord.Y)
                {
                    this.SetTile(roomItem, team, user);
                    if (roomItem.ExtraData.Equals("5") || roomItem.ExtraData.Equals("8") || roomItem.ExtraData.Equals("11") || roomItem.ExtraData.Equals("14"))
                        checked { ++num; }
                    roomItem.UpdateState(false, true);
                }
            }
            if (num != this.banzaiTiles.Count)
                return;
            this.BanzaiEnd();
        }

        private void HandleMaxBanzaiTiles(Point coord, Team team, RoomUser user)
        {
            if (team == Team.none)
                return;
            this.room.GetGameMap().GetCoordinatedItems(coord);
            foreach (RoomItem roomItem in (IEnumerable)this.banzaiTiles.Values)
            {
                if (roomItem.GetBaseItem().InteractionType == InteractionType.banzaifloor && (roomItem.GetX == coord.X && roomItem.GetY == coord.Y))
                {
                    BattleBanzai.SetMaxForTile(roomItem, team, user);
                    this.room.GetGameManager().AddPointToTeam(team, user);
                    roomItem.UpdateState(false, true);
                }
            }
        }

        private static void SetMaxForTile(RoomItem item, Team team, RoomUser user)
        {
            if (item.value < 3)
            {
                item.value = 3;
                item.team = team;
            }
            int num = checked(item.value + unchecked((int)item.team) * 3 - 1);
            item.ExtraData = num.ToString();
        }

        internal void Destroy()
        {
            this.banzaiTiles.Clear();
            this.pucks.Clear();
            Array.Clear((Array)this.floorMap, 0, this.floorMap.Length);
            this.field.destroy();
            this.room = (Room)null;
            this.banzaiTiles = (HybridDictionary)null;
            this.pucks = (QueuedDictionary<uint, RoomItem>)null;
            this.floorMap = (byte[,])null;
            this.field = (GameField)null;
        }
    }

}
