using Cyber.Core;
using Cyber.HabboHotel.Items;
using Cyber.HabboHotel.Pathfinding;
using Cyber.HabboHotel.PathFinding;
using Cyber.HabboHotel.Rooms.Games;
using Cyber.Messages;
using Cyber.Messages.Headers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Collections.Specialized;

namespace Cyber.HabboHotel.Rooms
{
    internal class Gamemap
    {
        private Room room;
        private RoomModel mStaticModel;
        private DynamicRoomModel mDynamicModel;
        private HybridDictionary mCoordinatedItems;
        private byte[,] mGameMap;
        private byte[,] mUserItemEffect;
        private double[,] mItemHeightMap;
        internal bool DiagonalEnabled;
        internal bool gotPublicPool;
        private HybridDictionary userMap;
        internal ServerMessage SerializedFloormap;
        internal HashSet<Point> walkableList;

        internal DynamicRoomModel Model
        {
            get
            {
                return this.mDynamicModel;
            }
        }
        internal RoomModel StaticModel
        {
            get
            {
                return this.mStaticModel;
            }
        }
        internal byte[,] EffectMap
        {
            get
            {
                return this.mUserItemEffect;
            }
        }
        internal HybridDictionary CoordinatedItems
        {
            get
            {
                return this.mCoordinatedItems;
            }
        }
        internal byte[,] GameMap
        {
            get
            {
                return this.mGameMap;
            }
        }
        internal double[,] ItemHeightMap
        {
            get
            {
                return this.mItemHeightMap;
            }
        }
        private static bool IsSoccerGoal(InteractionType Type)
        {
            return Type == InteractionType.footballgoalblue || Type == InteractionType.footballgoalgreen || Type == InteractionType.footballgoalred || Type == InteractionType.footballgoalyellow;
        }

        public Gamemap(Room room)
        {
            this.room = room;
            this.DiagonalEnabled = true;
            this.mStaticModel = CyberEnvironment.GetGame().GetRoomManager().GetModel(room.ModelName);
            if (this.mStaticModel == null)
            {
                throw new Exception("No modeldata found for roomID " + room.RoomId);
            }
            this.mDynamicModel = new DynamicRoomModel(this.mStaticModel, room);
            this.mCoordinatedItems = new HybridDictionary();
            this.gotPublicPool = room.RoomData.Model.gotPublicPool;
            this.mGameMap = new byte[this.Model.MapSizeX, this.Model.MapSizeY];
            this.mItemHeightMap = new double[this.Model.MapSizeX, this.Model.MapSizeY];
            this.userMap = new HybridDictionary();
            this.walkableList = this.GetWalkablePoints();
        }

        internal void AddUserToMap(RoomUser user, Point coord)
        {
            if (this.userMap.Contains(coord))
            {
                ((List<RoomUser>)this.userMap[coord]).Add(user);
                return;
            }
            List<RoomUser> list = new List<RoomUser>();
            list.Add(user);
            this.userMap.Add(coord, list);
        }
        internal void TeleportToItem(RoomUser user, RoomItem item)
        {
            this.GameMap[user.X, user.Y] = user.SqState;
            this.UpdateUserMovement(new Point(user.Coordinate.X, user.Coordinate.Y), new Point(item.Coordinate.X, item.Coordinate.Y), user);
            user.X = item.GetX;
            user.Y = item.GetY;
            user.Z = item.GetZ;
            user.SqState = this.GameMap[item.GetX, item.GetY];
            this.GameMap[user.X, user.Y] = 1;
            user.RotBody = item.Rot;
            user.RotHead = item.Rot;
            user.GoalX = user.X;
            user.GoalY = user.Y;
            user.SetStep = false;
            user.IsWalking = false;
            user.UpdateNeeded = true;
        }
        internal void UpdateUserMovement(Point oldCoord, Point newCoord, RoomUser user)
        {
            this.RemoveUserFromMap(user, oldCoord);
            this.AddUserToMap(user, newCoord);
        }
        internal void RemoveUserFromMap(RoomUser user, Point coord)
        {
            if (this.userMap.Contains(coord))
            {
                ((List<RoomUser>)this.userMap[coord]).Remove(user);
            }
        }
        internal bool MapGotUser(Point coord)
        {
            return this.GetRoomUsers(coord).Count > 0;
        }
        internal List<RoomUser> GetRoomUsers(Point coord)
        {
            if (this.userMap.Contains(coord))
            {
                return (List<RoomUser>)this.userMap[coord];
            }
            return new List<RoomUser>();
        }
        internal Point getRandomWalkableSquare()
        {
            List<Point> list = new List<Point>();
            checked
            {
                for (int i = 0; i < this.mGameMap.GetUpperBound(1) - 1; i++)
                {
                    for (int j = 0; j < this.mGameMap.GetUpperBound(0) - 1; j++)
                    {
                        if (this.mStaticModel.DoorX != j && this.mStaticModel.DoorY != i && this.mGameMap[j, i] == 1)
                        {
                            list.Add(new Point(j, i));
                        }
                    }
                }
                int randomNumber = CyberEnvironment.GetRandomNumber(0, list.Count);
                int num = 0;
                foreach (Point current in list)
                {
                    if (num == randomNumber)
                    {
                        return current;
                    }
                    num++;
                }
                return new Point(0, 0);
            }
        }
        internal string GenerateMapDump()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("Game map:");
            checked
            {
                for (int i = 0; i < this.Model.MapSizeY; i++)
                {
                    StringBuilder stringBuilder2 = new StringBuilder();
                    for (int j = 0; j < this.Model.MapSizeX; j++)
                    {
                        stringBuilder2.Append(this.mGameMap[j, i].ToString());
                    }
                    stringBuilder.AppendLine(stringBuilder2.ToString());
                }
                stringBuilder.AppendLine();
                stringBuilder.AppendLine("Item height map:");
                for (int k = 0; k < this.Model.MapSizeY; k++)
                {
                    StringBuilder stringBuilder3 = new StringBuilder();
                    for (int l = 0; l < this.Model.MapSizeX; l++)
                    {
                        stringBuilder3.Append("[" + this.mItemHeightMap[l, k].ToString() + "]");
                    }
                    stringBuilder.AppendLine(stringBuilder3.ToString());
                }
                stringBuilder.AppendLine();
                stringBuilder.AppendLine("Static data:");
                for (int m = 0; m < this.Model.MapSizeY; m++)
                {
                    StringBuilder stringBuilder4 = new StringBuilder();
                    for (int n = 0; n < this.Model.MapSizeX; n++)
                    {
                        stringBuilder4.Append("[" + this.Model.SqState[n, m].ToString() + "]");
                    }
                    stringBuilder.AppendLine(stringBuilder4.ToString());
                }
                stringBuilder.AppendLine();
                stringBuilder.AppendLine("Static data height:");
                for (int num = 0; num < this.Model.MapSizeY; num++)
                {
                    StringBuilder stringBuilder5 = new StringBuilder();
                    for (int num2 = 0; num2 < this.Model.MapSizeX; num2++)
                    {
                        stringBuilder5.Append("[" + this.Model.SqFloorHeight[num2, num].ToString() + "]");
                    }
                    stringBuilder.AppendLine(stringBuilder5.ToString());
                }
                stringBuilder.AppendLine();
                stringBuilder.AppendLine("Pool map:");
                for (int num3 = 0; num3 < this.Model.MapSizeY; num3++)
                {
                    StringBuilder stringBuilder6 = new StringBuilder();
                    for (int num4 = 0; num4 < this.Model.MapSizeX; num4++)
                    {
                        stringBuilder6.Append("[" + this.mUserItemEffect[num4, num3].ToString() + "]");
                    }
                    stringBuilder.AppendLine(stringBuilder6.ToString());
                }
                stringBuilder.AppendLine();
                return stringBuilder.ToString();
            }
        }
        internal void AddToMap(RoomItem item)
        {
            this.AddItemToMap(item, true);
        }
        private void SetDefaultValue(int x, int y)
        {
            this.mGameMap[x, y] = 0;
            this.mUserItemEffect[x, y] = 0;
            this.mItemHeightMap[x, y] = 0.0;
            if (x == this.Model.DoorX && y == this.Model.DoorY)
            {
                this.mGameMap[x, y] = 3;
                return;
            }
            if (this.Model.SqState[x, y] == SquareState.OPEN)
            {
                this.mGameMap[x, y] = 1;
                return;
            }
            if (this.Model.SqState[x, y] == SquareState.SEAT)
            {
                this.mGameMap[x, y] = 2;
            }
        }
        internal void updateMapForItem(RoomItem item)
        {
            this.RemoveFromMap(item);
            this.AddToMap(item);
        }
        internal void GenerateMaps(bool checkLines = true)
        {
            int maxX = 0;
            int maxY = 0;
            this.mCoordinatedItems = new HybridDictionary();
            if (checkLines)
            {
                foreach (RoomItem roomItem in this.room.GetRoomItemHandler().mFloorItems.Values.ToArray())
                {
                    if (roomItem.GetX > this.Model.MapSizeX && roomItem.GetX > maxX)
                    {
                        maxX = roomItem.GetX;
                    }
                    if (roomItem.GetY > this.Model.MapSizeY && roomItem.GetY > maxY)
                    {
                        maxY = roomItem.GetY;
                    }
                }
            }

            if (maxX > this.Model.MapSizeX - 1 || maxY > this.Model.MapSizeY - 1)
            {
                if (maxX < Model.MapSizeX)
                    maxX = this.Model.MapSizeX;
                if (maxY < this.Model.MapSizeY)
                    maxY = this.Model.MapSizeY;
                this.Model.SetMapsize(maxX + 7, maxY + 7);
                this.GenerateMaps(false);
                return;
            }
            if (maxX != this.StaticModel.MapSizeX || maxY != this.StaticModel.MapSizeY)
            {
                this.mUserItemEffect = new byte[this.Model.MapSizeX, this.Model.MapSizeY];
                this.mGameMap = new byte[this.Model.MapSizeX, this.Model.MapSizeY];
                this.mItemHeightMap = new double[this.Model.MapSizeX, this.Model.MapSizeY];
                for (int j = 0; j < this.Model.MapSizeY; j++)
                {
                    for (int k = 0; k < this.Model.MapSizeX; k++)
                    {
                        this.mGameMap[k, j] = 0;
                        this.mUserItemEffect[k, j] = 0;
                        if (k == this.Model.DoorX && j == this.Model.DoorY)
                        {
                            this.mGameMap[k, j] = 3;
                        }
                        else if (this.Model.SqState[k, j] == SquareState.OPEN)
                        {
                            this.mGameMap[k, j] = 1;
                        }
                        else if (this.Model.SqState[k, j] == SquareState.SEAT)
                        {
                            this.mGameMap[k, j] = 2;
                        }
                        else if (this.Model.SqState[k, j] == SquareState.POOL)
                        {
                            this.mUserItemEffect[k, j] = 6;
                        }
                    }
                }
                if (this.gotPublicPool)
                {
                    for (int l = 0; l < this.StaticModel.MapSizeY; l++)
                    {
                        for (int m = 0; m < this.StaticModel.MapSizeX; m++)
                        {
                            if (this.StaticModel.mRoomModelfx[m, l] != 0)
                            {
                                this.mUserItemEffect[m, l] = this.StaticModel.mRoomModelfx[m, l];
                            }
                        }
                    }
                }
            }
            else
            {
                this.mUserItemEffect = new byte[this.Model.MapSizeX, this.Model.MapSizeY];
                this.mGameMap = new byte[this.Model.MapSizeX, this.Model.MapSizeY];
                this.mItemHeightMap = new double[this.Model.MapSizeX, this.Model.MapSizeY];
                for (int n = 0; n < this.Model.MapSizeY; n++)
                {
                    for (int num3 = 0; num3 < this.Model.MapSizeX; num3++)
                    {
                        this.mGameMap[num3, n] = 0;
                        this.mUserItemEffect[num3, n] = 0;
                        if (num3 == this.Model.DoorX && n == this.Model.DoorY)
                        {
                            this.mGameMap[num3, n] = 3;
                        }
                        else if (this.Model.SqState[num3, n] == SquareState.OPEN)
                        {
                            this.mGameMap[num3, n] = 1;
                        }
                        else if (this.Model.SqState[num3, n] == SquareState.SEAT)
                        {
                            this.mGameMap[num3, n] = 2;
                        }
                        else if (this.Model.SqState[num3, n] == SquareState.POOL)
                        {
                            this.mUserItemEffect[num3, n] = 6;
                        }
                    }
                }
                if (this.gotPublicPool)
                {
                    for (int num4 = 0; num4 < this.StaticModel.MapSizeY; num4++)
                    {
                        for (int num5 = 0; num5 < this.StaticModel.MapSizeX; num5++)
                        {
                            if (this.StaticModel.mRoomModelfx[num5, num4] != 0)
                            {
                                this.mUserItemEffect[num5, num4] = this.StaticModel.mRoomModelfx[num5, num4];
                            }
                        }
                    }
                }
            }
            foreach (RoomItem item in this.room.GetRoomItemHandler().mFloorItems.Values.ToArray())
            {
                if (!this.AddItemToMap(item, true))
                {
                    break;
                }
            }
            if (this.room.AllowWalkthrough == 0)
            {
                foreach (RoomUser current in this.room.GetRoomUserManager().UserList.Values)
                {
                    current.SqState = this.mGameMap[current.X, current.Y];
                    this.mGameMap[current.X, current.Y] = 0;
                }
            }
            this.mGameMap[this.Model.DoorX, this.Model.DoorY] = 3;
        }

        public void lazyWalkablePoints()
        {
            this.walkableList = this.GetWalkablePoints();
        }

        private HashSet<Point> GetWalkablePoints()
        {
            HashSet<Point> list = new HashSet<Point>();
            checked
            {
                for (int i = 0; i < this.mGameMap.GetUpperBound(1) - 1; i++)
                {
                    for (int j = 0; j < this.mGameMap.GetUpperBound(0) - 1; j++)
                    {
                        if (this.mStaticModel.DoorX != j && this.mStaticModel.DoorY != i && this.mGameMap[j, i] == 1)
                        {
                            list.Add(new Point(j, i));
                        }
                    }
                }
                return list;
            }
        }
        private bool ConstructMapForItem(RoomItem Item, Point Coord)
        {
            try
            {
                checked
                {
                    if (Coord.X > this.Model.MapSizeX - 1)
                    {
                        this.Model.AddX();
                        this.GenerateMaps(true);
                        bool result = false;
                        return result;
                    }
                    if (Coord.Y > this.Model.MapSizeY - 1)
                    {
                        this.Model.AddY();
                        this.GenerateMaps(true);
                        bool result = false;
                        return result;
                    }
                    if (this.Model.SqState[Coord.X, Coord.Y] == SquareState.BLOCKED)
                    {
                        this.Model.OpenSquare(Coord.X, Coord.Y, Item.GetZ);
                        this.Model.SetUpdateState();
                    }
                }
                if (this.mItemHeightMap[Coord.X, Coord.Y] <= Item.TotalHeight)
                {
                    this.mItemHeightMap[Coord.X, Coord.Y] = Item.TotalHeight - (double)this.mDynamicModel.SqFloorHeight[Item.GetX, Item.GetY];
                    this.mUserItemEffect[Coord.X, Coord.Y] = 0;
                    InteractionType interactionType = Item.GetBaseItem().InteractionType;
                    if (interactionType != InteractionType.pool)
                    {
                        switch (interactionType)
                        {
                            case InteractionType.iceskates:
                                this.mUserItemEffect[Coord.X, Coord.Y] = 3;
                                break;
                            case InteractionType.normslaskates:
                                this.mUserItemEffect[Coord.X, Coord.Y] = 2;
                                break;
                            case InteractionType.lowpool:
                                this.mUserItemEffect[Coord.X, Coord.Y] = 4;
                                break;
                            case InteractionType.haloweenpool:
                                this.mUserItemEffect[Coord.X, Coord.Y] = 5;
                                break;

                            case InteractionType.snowboardslope:
                                this.mUserItemEffect[Coord.X, Coord.Y] = 7;
                                break;
                        }
                    }
                    else
                    {
                        this.mUserItemEffect[Coord.X, Coord.Y] = 1;
                    }
                    if (Item.GetBaseItem().Walkable)
                    {
                        if (this.mGameMap[Coord.X, Coord.Y] != 3)
                        {
                            this.mGameMap[Coord.X, Coord.Y] = 1;
                        }
                    }
                    else
                    {
                        if (Item.GetZ <= (double)this.Model.SqFloorHeight[Item.GetX, Item.GetY] + 0.1 && Item.GetBaseItem().InteractionType == InteractionType.gate && Item.ExtraData == "1")
                        {
                            if (this.mGameMap[Coord.X, Coord.Y] != 3)
                            {
                                this.mGameMap[Coord.X, Coord.Y] = 1;
                            }
                        }
                        else
                        {
                            if (Item.GetBaseItem().IsSeat)
                            {
                                this.mGameMap[Coord.X, Coord.Y] = 3;
                            }
                            else if (Item.GetBaseItem().InteractionType == InteractionType.bed || Item.GetBaseItem().InteractionType == InteractionType.bedtent)
                            {
                                if (Coord.X == Item.GetX && Coord.Y == Item.GetY)
                                {
                                    this.mGameMap[Coord.X, Coord.Y] = 3;
                                }
                            }
                            else
                            {
                                if (this.mGameMap[Coord.X, Coord.Y] != 3)
                                {
                                    this.mGameMap[Coord.X, Coord.Y] = 0;
                                }
                            }
                        }
                    }
                }
                if (Item.GetBaseItem().InteractionType == InteractionType.bed || Item.GetBaseItem().InteractionType == InteractionType.bedtent)
                {
                    this.mGameMap[Coord.X, Coord.Y] = 3;
                }
            }
            catch (Exception ex)
            {
                Logging.LogException(string.Concat(new object[]
				{
					"Error during map generation for room ",
					this.room.RoomId,
					". Exception: ",
					ex.ToString()
				}));
            }
            return true;
        }
        internal void AddCoordinatedItem(RoomItem item, Point coord)
        {
            List<RoomItem> list = new List<RoomItem>();
            if (!this.mCoordinatedItems.Contains(coord))
            {
                list = new List<RoomItem>();
                list.Add(item);
                this.mCoordinatedItems.Add(coord, list);
                return;
            }
            list = (List<RoomItem>)this.mCoordinatedItems[coord];
            if (!list.Contains(item))
            {
                list.Add(item);
                this.mCoordinatedItems[coord] = list;
            }
        }
        internal List<RoomItem> GetCoordinatedItems(Point coord)
        {
            Point point = new Point(coord.X, coord.Y);
            if (this.mCoordinatedItems.Contains(point))
            {
                return (List<RoomItem>)this.mCoordinatedItems[point];
            }
            return new List<RoomItem>();
        }
        internal bool RemoveCoordinatedItem(RoomItem item, Point coord)
        {
            Point point = new Point(coord.X, coord.Y);
            if (this.mCoordinatedItems.Contains(point))
            {
                ((List<RoomItem>)this.mCoordinatedItems[point]).Remove(item);
                return true;
            }
            return false;
        }

        private void AddSpecialItems(RoomItem item)
        {
            switch (item.GetBaseItem().InteractionType)
            {
                case InteractionType.banzaifloor:
                    this.room.GetBanzai().AddTile(item, item.Id);
                    break;
                case InteractionType.banzaitele:
                    this.room.GetGameItemHandler().AddTeleport(item, item.Id);
                    item.ExtraData = "";
                    break;
                case InteractionType.banzaipuck:
                    this.room.GetBanzai().AddPuck(item);
                    break;
                case InteractionType.banzaipyramid:
                    this.room.GetGameItemHandler().AddPyramid(item, item.Id);
                    break;
                case InteractionType.freezeexit:
                    RoomItem exitTeleport = this.room.GetFreeze().ExitTeleport;
                    if (exitTeleport == null || (int)item.Id != (int)exitTeleport.Id)
                        break;
                    this.room.GetFreeze().ExitTeleport = (RoomItem)null;
                    break;
                case InteractionType.freezetileblock:
                    this.room.GetFreeze().AddFreezeBlock(item);
                    break;
                case InteractionType.freezetile:
                    this.room.GetFreeze().AddFreezeTile(item);
                    break;


                case InteractionType.football:
                    this.room.GetSoccer().AddBall(item);
                    break;
            }
        }

        private void RemoveSpecialItem(RoomItem item)
        {
            switch (item.GetBaseItem().InteractionType)
            {
                case InteractionType.banzaifloor:
                    this.room.GetBanzai().RemoveTile(item.Id);
                    break;
                case InteractionType.banzaitele:
                    this.room.GetGameItemHandler().RemoveTeleport(item.Id);
                    break;
                case InteractionType.banzaipuck:
                    this.room.GetBanzai().RemovePuck(item.Id);
                    break;
                case InteractionType.banzaipyramid:
                    this.room.GetGameItemHandler().RemovePyramid(item.Id);
                    break;
                case InteractionType.freezetileblock:
                    this.room.GetFreeze().RemoveFreezeBlock(item.Id);
                    break;
                case InteractionType.freezetile:
                    this.room.GetFreeze().RemoveFreezeTile(item.Id);
                    break;
                case InteractionType.fbgate:
                    //   this.room.GetSoccer().UnRegisterGate(item);
                    break;
                case InteractionType.football:
                    this.room.GetSoccer().RemoveBall(item.Id);
                    break;
            }
        }


        internal bool RemoveFromMap(RoomItem item, bool handleGameItem)
        {
            if (handleGameItem)
            {
                this.RemoveSpecialItem(item);
            }
            if (this.room.GotSoccer())
            {
                this.room.GetSoccer().onGateRemove(item);
            }
            bool result = false;
            foreach (Point current in item.GetCoords)
            {
                if (this.RemoveCoordinatedItem(item, current))
                {
                    result = true;
                }
            }
            HybridDictionary HybridDictionary = new HybridDictionary();
            foreach (Point current2 in item.GetCoords)
            {
                Point point = new Point(current2.X, current2.Y);
                if (this.mCoordinatedItems.Contains(point))
                {
                    List<RoomItem> value = (List<RoomItem>)this.mCoordinatedItems[point];
                    if (!HybridDictionary.Contains(current2))
                    {
                        HybridDictionary.Add(current2, value);
                    }
                }
                this.SetDefaultValue(current2.X, current2.Y);
            }
            foreach (Point point2 in HybridDictionary.Keys)
            {
                if (HybridDictionary.Contains(point2))
                {
                    List<RoomItem> list = (List<RoomItem>)HybridDictionary[point2];
                    foreach (RoomItem current3 in list)
                    {
                        this.ConstructMapForItem(current3, point2);
                    }
                }
            }
            room.GetRoomItemHandler().OnHeightmapUpdate(HybridDictionary.Keys);
            HybridDictionary.Clear();
            HybridDictionary = null;
            return result;
        }
        internal bool RemoveFromMap(RoomItem item)
        {
            return this.RemoveFromMap(item, true);
        }
        internal bool AddItemToMap(RoomItem Item, bool handleGameItem, bool NewItem = true)
        {
            if (handleGameItem)
            {
                this.AddSpecialItems(Item);
                InteractionType interactionType = Item.GetBaseItem().InteractionType;
                if (interactionType != InteractionType.roller)
                {
                    switch (interactionType)
                    {
                        case InteractionType.footballgoalgreen:
                        case InteractionType.footballcountergreen:
                        case InteractionType.banzaigategreen:
                        case InteractionType.banzaiscoregreen:
                        case InteractionType.freezegreencounter:
                        case InteractionType.freezegreengate:
                            this.room.GetGameManager().AddFurnitureToTeam(Item, Team.green);
                            break;
                        case InteractionType.footballgoalyellow:
                        case InteractionType.footballcounteryellow:
                        case InteractionType.banzaigateyellow:
                        case InteractionType.banzaiscoreyellow:
                        case InteractionType.freezeyellowcounter:
                        case InteractionType.freezeyellowgate:
                            this.room.GetGameManager().AddFurnitureToTeam(Item, Team.yellow);
                            break;
                        case InteractionType.footballgoalblue:
                        case InteractionType.footballcounterblue:
                        case InteractionType.banzaigateblue:
                        case InteractionType.banzaiscoreblue:
                        case InteractionType.freezebluecounter:
                        case InteractionType.freezebluegate:
                            this.room.GetGameManager().AddFurnitureToTeam(Item, Team.blue);
                            break;
                        case InteractionType.footballgoalred:
                        case InteractionType.footballcounterred:
                        case InteractionType.banzaigatered:
                        case InteractionType.banzaiscorered:
                        case InteractionType.freezeredcounter:
                        case InteractionType.freezeredgate:
                            this.room.GetGameManager().AddFurnitureToTeam(Item, Team.red);
                            break;
                        case InteractionType.freezeexit:
                            this.room.GetFreeze().ExitTeleport = Item;
                            break;
                    }
                }
                else
                {
                    if (!this.room.GetRoomItemHandler().mRollers.ContainsKey(Item.Id))
                    {
                        this.room.GetRoomItemHandler().mRollers.Add(Item.Id, Item);
                    }
                }
            }
            if (Item.GetBaseItem().Type != 's')
            {
                return true;
            }
            foreach (Point current in Item.GetCoords)
            {
                Point coord = new Point(current.X, current.Y);
                this.AddCoordinatedItem(Item, coord);
            }
            checked
            {
                if (Item.GetX > this.Model.MapSizeX - 1)
                {
                    this.Model.AddX();
                    this.GenerateMaps(true);
                    return false;
                }
                if (Item.GetY > this.Model.MapSizeY - 1)
                {
                    this.Model.AddY();
                    this.GenerateMaps(true);
                    return false;
                }

                foreach (Point current2 in Item.GetCoords)
                {

                    if (!this.ConstructMapForItem(Item, current2))
                    {
                        return false;
                    }
                }
                return true;
            }
        }
        internal bool CanWalk(int X, int Y, bool Override, uint HorseId = 0u)
        {
            return this.room.AllowWalkthrough == 1 || Override || this.room.GetRoomUserManager().GetUserForSquare(X, Y) == null || this.room.AllowWalkthrough != 0;
        }
        internal bool AddItemToMap(RoomItem Item, bool NewItem = true)
        {
            return this.AddItemToMap(Item, true, NewItem);
        }
        internal byte GetFloorStatus(Point coord)
        {
            if (coord.X > this.mGameMap.GetUpperBound(0) || coord.Y > this.mGameMap.GetUpperBound(1))
            {
                return 1;
            }
            return this.mGameMap[coord.X, coord.Y];
        }
        internal double GetHeightForSquareFromData(Point coord)
        {
            if (coord.X > this.mDynamicModel.SqFloorHeight.GetUpperBound(0) || coord.Y > this.mDynamicModel.SqFloorHeight.GetUpperBound(1))
            {
                return 1.0;
            }
            return (double)this.mDynamicModel.SqFloorHeight[coord.X, coord.Y];
        }
        internal bool CanRollItemHere(int x, int y)
        {
            return this.ValidTile(x, y) && this.Model.SqState[x, y] != SquareState.BLOCKED;
        }
        internal bool SquareIsOpen(int x, int y, bool pOverride)
        {
            return checked(this.mDynamicModel.MapSizeX - 1 >= x && this.mDynamicModel.MapSizeY - 1 >= y) && Gamemap.CanWalk(this.mGameMap[x, y], pOverride); 
        }

        internal bool IsValidStep2(RoomUser User, Point From, Point To, bool EndOfPath, bool Override)
        {
            if (!this.validTile(To.X, To.Y))
            {
                return false;
            }
            if (Override)
            {
                return true;
            }
            if ((int)this.mGameMap[To.X, To.Y] == 3 && !EndOfPath || (int)this.mGameMap[To.X, To.Y] == 0 || (int)this.mGameMap[To.X, To.Y] == 2 && !EndOfPath || this.SqAbsoluteHeight(To.X, To.Y) - this.SqAbsoluteHeight(From.X, From.Y) > 1.5)
            {
                return false;
            }
            RoomUser userForSquare = this.room.GetRoomUserManager().GetUserForSquare(To.X, To.Y);
            if (userForSquare != null && EndOfPath && this.room.AllowWalkthrough == 0)
            {
                User.HasPathBlocked = true;
                User.Path.Clear();
                User.IsWalking = false;
                User.RemoveStatus("mv");
                this.room.GetRoomUserManager().UpdateUserStatus(User, false);
                if (User.RidingHorse && !User.IsPet && !User.IsBot)
                {
                    RoomUser roomUserByVirtualId = this.room.GetRoomUserManager().GetRoomUserByVirtualId(Convert.ToInt32(User.HorseID));
                    roomUserByVirtualId.IsWalking = false;
                    roomUserByVirtualId.RemoveStatus("mv");
                    ServerMessage Message = new ServerMessage(Outgoing.UpdateUserStatusMessageComposer);
                    Message.AppendInt32(1);
                    roomUserByVirtualId.SerializeStatus(Message, "");
                    User.GetClient().GetHabbo().CurrentRoom.SendMessage(Message);
                }
            }
            else if (userForSquare != null && this.room.AllowWalkthrough == 0 && !userForSquare.IsWalking)
            {
                return false;
            }
            User.HasPathBlocked = false;
            return true;
        }


        internal bool AntiChoques(int X, int Y, RoomUser User)
        {
            RoomUser roomUser = null;
            this.room.GetRoomUserManager().ToSet.TryGetValue(new Point(X, Y), out roomUser);
            return (roomUser == null || roomUser == User);
        }

        internal bool IsValidStep(RoomUser User, Vector2D From, Vector2D To, bool EndOfPath, bool Override)
        {
            if (!this.ValidTile(To.X, To.Y))
                return false;
            if (Override)
                return true;
            if ((int)this.mGameMap[To.X, To.Y] == 3 && !EndOfPath || (int)this.mGameMap[To.X, To.Y] == 0 || (int)this.mGameMap[To.X, To.Y] == 2 && !EndOfPath || this.SqAbsoluteHeight(To.X, To.Y) - this.SqAbsoluteHeight(From.X, From.Y) > 1.5)
                return false;
            RoomUser userForSquare = this.room.GetRoomUserManager().GetUserForSquare(To.X, To.Y);
            if (userForSquare != null && EndOfPath && this.room.AllowWalkthrough == 0)
            {
                User.HasPathBlocked = true;
                User.Path.Clear();
                User.IsWalking = false;
                User.RemoveStatus("mv");
                this.room.GetRoomUserManager().UpdateUserStatus(User, false);
                if (User.RidingHorse && !User.IsPet && !User.IsBot)
                {
                    RoomUser roomUserByVirtualId = this.room.GetRoomUserManager().GetRoomUserByVirtualId(Convert.ToInt32(User.HorseID));
                    roomUserByVirtualId.IsWalking = false;
                    roomUserByVirtualId.RemoveStatus("mv");
                    ServerMessage Message = new ServerMessage(Outgoing.UpdateUserStatusMessageComposer);
                    Message.AppendInt32(1);
                    roomUserByVirtualId.SerializeStatus(Message, "");
                    User.GetClient().GetHabbo().CurrentRoom.SendMessage(Message);
                }
            }
            else if (userForSquare != null && this.room.AllowWalkthrough == 0 && !userForSquare.IsWalking)
                return false;
            return true;
        }



        internal static bool CanWalk(byte pState, bool pOverride)
        {
            return pOverride || pState == 3 || pState == 1;
        }
        internal bool validTile(int x, int y)
        {
            if (!this.ValidTile(x, y))
            {
                return false;
            }
            bool result;
            try
            {
                result = (this.mDynamicModel.SqState[x, y] == SquareState.OPEN);
            }
            catch
            {
                result = false;
            }
            return result;
        }
        internal bool itemCanBePlacedHere(int x, int y)
        {
            return checked(this.mDynamicModel.MapSizeX - 1 >= x && this.mDynamicModel.MapSizeY - 1 >= y) && (x != this.mDynamicModel.DoorX || y != this.mDynamicModel.DoorY) && this.mGameMap[x, y] == 1;
        }
        internal double SqAbsoluteHeight(int X, int Y)
        {
            Point point = new Point(X, Y);
            if (this.mCoordinatedItems.Contains(point))
            {
                List<RoomItem> itemsOnSquare = (List<RoomItem>)this.mCoordinatedItems[point];
                return this.SqAbsoluteHeight(X, Y, itemsOnSquare);
            }
            return (double)this.mDynamicModel.SqFloorHeight[X, Y];
        }
        internal double SqAbsoluteHeight(int X, int Y, List<RoomItem> ItemsOnSquare)
        {
            double result;
            try
            {
                double num = 0.0;
                bool flag = false;
                double num2 = 0.0;
                foreach (RoomItem current in ItemsOnSquare)
                {
                    if (current.GetBaseItem().InteractionType == InteractionType.tilestackmagic)
                    {
                        return current.GetZ;
                    }
                    if (current.TotalHeight > num)
                    {
                        if (current.GetBaseItem().IsSeat || current.GetBaseItem().InteractionType == InteractionType.bed || current.GetBaseItem().InteractionType == InteractionType.bedtent)
                        {
                            flag = true;
                            num2 = current.GetBaseItem().Height;
                        }
                        else
                        {
                            flag = false;
                        }
                        num = current.TotalHeight;
                    }
                }
                double num3 = (double)this.Model.SqFloorHeight[X, Y];
                double num4 = num - (double)this.Model.SqFloorHeight[X, Y];
                if (flag)
                {
                    num4 -= num2;
                }
                if (num4 < 0.0)
                {
                    num4 = 0.0;
                }
                result = num3 + num4;
            }
            catch (Exception pException)
            {
                Logging.HandleException(pException, "Room.SqAbsoluteHeight");
                result = 0.0;
            }
            return result;
        }
        internal bool ValidTile(int X, int Y)
        {
            return X >= 0 && Y >= 0 && X < this.Model.MapSizeX && Y < this.Model.MapSizeY && !this.SquareHasUsers(X, Y);
        }
        internal static Dictionary<int, ThreeDCoord> GetAffectedTiles(InteractionType type, int Length, int Width, int PosX, int PosY, int Rotation)
        {
            checked
            {
                if (!Gamemap.IsSoccerGoal(type))
                {
                    int num = 0;
                    Dictionary<int, ThreeDCoord> dictionary = new Dictionary<int, ThreeDCoord>();
                    if (Length > 1)
                    {
                        if (Rotation == 0 || Rotation == 4)
                        {
                            for (int i = 1; i < Length; i++)
                            {
                                dictionary.Add(num++, new ThreeDCoord(PosX, PosY + i, i));
                                for (int j = 1; j < Width; j++)
                                {
                                    dictionary.Add(num++, new ThreeDCoord(PosX + j, PosY + i, (i < j) ? j : i));
                                }
                            }
                        }
                        else
                        {
                            if (Rotation == 2 || Rotation == 6)
                            {
                                for (int k = 1; k < Length; k++)
                                {
                                    dictionary.Add(num++, new ThreeDCoord(PosX + k, PosY, k));
                                    for (int l = 1; l < Width; l++)
                                    {
                                        dictionary.Add(num++, new ThreeDCoord(PosX + k, PosY + l, (k < l) ? l : k));
                                    }
                                }
                            }
                        }
                    }
                    if (Width > 1)
                    {
                        if (Rotation == 0 || Rotation == 4)
                        {
                            for (int m = 1; m < Width; m++)
                            {
                                dictionary.Add(num++, new ThreeDCoord(PosX + m, PosY, m));
                                for (int n = 1; n < Length; n++)
                                {
                                    dictionary.Add(num++, new ThreeDCoord(PosX + m, PosY + n, (m < n) ? n : m));
                                }
                            }
                        }
                        else
                        {
                            if (Rotation == 2 || Rotation == 6)
                            {
                                for (int num2 = 1; num2 < Width; num2++)
                                {
                                    dictionary.Add(num++, new ThreeDCoord(PosX, PosY + num2, num2));
                                    for (int num3 = 1; num3 < Length; num3++)
                                    {
                                        dictionary.Add(num++, new ThreeDCoord(PosX + num3, PosY + num2, (num2 < num3) ? num3 : num2));
                                    }
                                }
                            }
                        }
                    }
                    return dictionary;
                }
                int num4 = 0;
                Dictionary<int, ThreeDCoord> dictionary2 = new Dictionary<int, ThreeDCoord>();
                if (Length > 1)
                {
                    if (Rotation == 0 || Rotation == 4)
                    {
                        for (int num5 = 1; num5 < Length; num5++)
                        {
                            dictionary2.Add(num4++, new ThreeDCoord(PosX, PosY + num5, num5));
                            for (int num6 = 1; num6 < Width; num6++)
                            {
                                dictionary2.Add(num4++, new ThreeDCoord(PosX + num6, PosY + num5, num5));
                            }
                        }
                    }
                    else
                    {
                        if (Rotation == 2 || Rotation == 6)
                        {
                            for (int num7 = 1; num7 < Length; num7++)
                            {
                                dictionary2.Add(num4++, new ThreeDCoord(PosX + num7, PosY, num7));
                                for (int num8 = 1; num8 < Width; num8++)
                                {
                                    dictionary2.Add(num4++, new ThreeDCoord(PosX + num7, PosY + num8, num7));
                                }
                            }
                        }
                    }
                }
                if (Width > 1)
                {
                    if (Rotation == 0 || Rotation == 4)
                    {
                        for (int num9 = 1; num9 < Width; num9++)
                        {
                            dictionary2.Add(num4++, new ThreeDCoord(PosX + num9, PosY, num9));
                            for (int num10 = 1; num10 < Length; num10++)
                            {
                                dictionary2.Add(num4++, new ThreeDCoord(PosX + num9, PosY + num10, num9));
                            }
                        }
                    }
                    else
                    {
                        if (Rotation == 2 || Rotation == 6)
                        {
                            for (int num11 = 1; num11 < Width; num11++)
                            {
                                dictionary2.Add(num4++, new ThreeDCoord(PosX, PosY + num11, num11));
                                for (int num12 = 1; num12 < Length; num12++)
                                {
                                    dictionary2.Add(num4++, new ThreeDCoord(PosX + num12, PosY + num11, num11));
                                }
                            }
                        }
                    }
                }
                return dictionary2;
            }
        }
        internal List<RoomItem> GetRoomItemForSquare(int pX, int pY)
        {
            Point point = new Point(pX, pY);
            List<RoomItem> list = new List<RoomItem>();
            if (this.mCoordinatedItems.Contains(point))
            {
                List<RoomItem> list2 = (List<RoomItem>)this.mCoordinatedItems[point];
                foreach (RoomItem current in list2)
                {
                    if (current.Coordinate.X == point.X && current.Coordinate.Y == point.Y)
                    {
                        list.Add(current);
                    }
                }
            }
            return list;
        }
        internal List<RoomItem> GetAllRoomItemForSquare(int pX, int pY)
        {
            Point point = new Point(pX, pY);
            List<RoomItem> list = new List<RoomItem>();
            if (this.mCoordinatedItems.Contains(point))
            {
                List<RoomItem> list2 = (List<RoomItem>)this.mCoordinatedItems[point];
                foreach (RoomItem current in list2)
                {
                    if (!list.Contains(current))
                    {
                        list.Add(current);
                    }
                }
            }
            return list;
        }
        internal bool SquareHasUsers(int X, int Y)
        {
            return this.MapGotUser(new Point(X, Y));
        }
        internal static bool TilesTouching(int X1, int Y1, int X2, int Y2)
        {
            return checked(Math.Abs(X1 - X2) <= 1 && Math.Abs(Y1 - Y2) <= 1) || (X1 == X2 && Y1 == Y2);
        }
        internal static int TileDistance(int X1, int Y1, int X2, int Y2)
        {
            return checked(Math.Abs(X1 - X2) + Math.Abs(Y1 - Y2));
        }
        internal void Destroy()
        {
            this.userMap.Clear();
            this.mDynamicModel.Destroy();
            this.mCoordinatedItems.Clear();
            Array.Clear(this.mGameMap, 0, this.mGameMap.Length);
            Array.Clear(this.mUserItemEffect, 0, this.mUserItemEffect.Length);
            Array.Clear(this.mItemHeightMap, 0, this.mItemHeightMap.Length);
            this.userMap = null;
            this.mGameMap = null;
            this.mUserItemEffect = null;
            this.mItemHeightMap = null;
            this.mCoordinatedItems = null;
            this.mDynamicModel = null;
            this.room = null;
            this.mStaticModel = null;
        }
        internal RoomItem GetHighestItemForSquare(int X, int Y, out double Z, RoomItem Exception = null)
        {
            RoomItem roomItem = null;
            double num = -1.0;
            double num2 = 0.0;
            foreach (RoomItem current in this.GetRoomItemForSquare(X, Y))
            {
                if (current.GetZ > num)
                {
                    num = current.GetZ;
                    num2 = current.GetBaseItem().Height;
                    roomItem = current;
                }
                if (Exception != null && Exception == roomItem)
                {
                    num = -1.0;
                    num2 = 0.0;
                    roomItem = null;
                }
            }
            Z = num + num2;
            return roomItem;
        }
        internal ServerMessage GetNewHeightmap()
        {
            if (SerializedFloormap != null)
            {
                return SerializedFloormap;
            }
            else
            {
                SerializedFloormap = NewHeightMap();
                return SerializedFloormap;
            }
        }
        private ServerMessage NewHeightMap()
        {
            ServerMessage serverMessage = new ServerMessage();
            serverMessage.Init(Outgoing.HeightMapMessageComposer);
            serverMessage.AppendInt32(this.Model.MapSizeX);
            checked
            {
                serverMessage.AppendInt32(this.Model.MapSizeX * this.Model.MapSizeY);
                for (int i = 0; i < this.Model.MapSizeY; i++)
                {
                    for (int j = 0; j < this.Model.MapSizeX; j++)
                    {
                        serverMessage.AppendShort((short)(this.SqAbsoluteHeight(j, i) * 256));
                        //  serverMessage.AppendShort(this.Model.SqFloorHeight[j, i] * 256);
                    }
                }
                return serverMessage;
            }
        }
    }
}
