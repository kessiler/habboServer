using Cyber.HabboHotel.Items;
using Cyber.Messages;
using Cyber.Messages.Headers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Collections.Specialized;
using System.Linq;

namespace Cyber.HabboHotel.Rooms.Games
{
    internal class Freeze
    {
        private Room room;
        private HybridDictionary freezeTiles;
        private HybridDictionary freezeBlocks;
        private RoomItem exitTeleport;
        private Random rnd;
        private bool gameStarted;

        internal bool GameIsStarted
        {
            get
            {
                return this.gameStarted;
            }
        }

        internal RoomItem ExitTeleport
        {
            get
            {
                return this.exitTeleport;
            }
            set
            {
                this.exitTeleport = value;
            }
        }

        public bool isFreezeActive
        {
            get
            {
                return this.gameStarted;
            }
        }

        public Freeze(Room room)
        {
            this.room = room;
            this.freezeTiles = new HybridDictionary();
            this.freezeBlocks = new HybridDictionary();
            this.exitTeleport = null;
            this.rnd = new Random();
            this.gameStarted = false;
        }

        internal void StartGame()
        {
            this.gameStarted = true;
            this.CountTeamPoints();
            this.ResetGame();
            this.room.GetGameManager().LockGates();
            this.room.GetGameManager().StartGame();
        }

        internal void StopGame()
        {
            this.gameStarted = false;
            this.room.GetGameManager().UnlockGates();
            this.room.GetGameManager().StopGame();
            Team winningTeam = this.room.GetGameManager().getWinningTeam();
            foreach (RoomUser Avatar in this.room.GetRoomUserManager().UserList.Values)
            {
                Avatar.FreezeLives = 0;
                if (Avatar.team == winningTeam)
                {
                    Avatar.UnIdle();
                    Avatar.DanceId = 0;
                    ServerMessage waveAtWin = new ServerMessage(Outgoing.RoomUserActionMessageComposer);
                    waveAtWin.AppendInt32(Avatar.VirtualId);
                    waveAtWin.AppendInt32(1);
                    this.room.SendMessage(waveAtWin);
                }
            }
        }

        internal static void OnCycle()
        {
        }

        internal void CycleUser(RoomUser user)
        {
            if (user.Freezed)
            {
                checked { ++user.FreezeCounter; }
                if (user.FreezeCounter > 10)
                {
                    user.Freezed = false;
                    user.FreezeCounter = 0;
                    Freeze.ActivateShield(user);
                }
            }
            if (!user.shieldActive)
                return;
            checked { ++user.shieldCounter; }
            if (user.shieldCounter > 10)
            {
                user.shieldActive = false;
                user.shieldCounter = 10;
                user.ApplyEffect((int)(user.team + 39));
            }
        }

        internal void ResetGame()
        {
            foreach (RoomItem roomItem in (IEnumerable)this.freezeBlocks.Values)
            {
                if (!string.IsNullOrEmpty(roomItem.ExtraData) && !roomItem.GetBaseItem().InteractionType.Equals(InteractionType.freezebluegate) && (!roomItem.GetBaseItem().InteractionType.Equals(InteractionType.freezeredgate) && !roomItem.GetBaseItem().InteractionType.Equals(InteractionType.freezegreengate)) && !roomItem.GetBaseItem().InteractionType.Equals(InteractionType.freezeyellowgate))
                {
                    roomItem.ExtraData = "";
                    roomItem.UpdateState(false, true);
                    this.room.GetGameMap().AddItemToMap(roomItem, false);
                }
            }
        }

        internal void OnUserWalk(RoomUser user)
        {
            if (!this.gameStarted || user.team == Team.none)
                return;
            if (user.X == user.GoalX && user.GoalY == user.Y && user.throwBallAtGoal)
            {
                foreach (RoomItem roomItem in (IEnumerable)this.freezeTiles.Values)
                {
                    if ((int)roomItem.interactionCountHelper == 0 && (roomItem.GetX == user.X && roomItem.GetY == user.Y))
                    {
                        roomItem.interactionCountHelper = (byte)1;
                        roomItem.ExtraData = "1000";
                        roomItem.UpdateState();
                        roomItem.InteractingUser = user.UserID;
                        roomItem.freezePowerUp = user.banzaiPowerUp;
                        roomItem.ReqUpdate(4, true);
                        switch (user.banzaiPowerUp)
                        {
                            case FreezePowerUp.GreenArrow:
                            case FreezePowerUp.OrangeSnowball:
                                user.banzaiPowerUp = FreezePowerUp.None;
                                goto label_12;
                            default:
                                goto label_12;
                        }
                    }
                }
            label_12: ;
            }
            foreach (RoomItem roomItem in (IEnumerable)this.freezeBlocks.Values)
            {
                if (user.X == roomItem.GetX && user.Y == roomItem.GetY && roomItem.freezePowerUp != FreezePowerUp.None)
                    this.PickUpPowerUp(roomItem, user);
            }
        }

        private void CountTeamPoints()
        {
            this.room.GetGameManager().Reset();
            foreach (RoomUser roomUser in this.room.GetRoomUserManager().UserList.Values)
            {
                if (!roomUser.IsBot && roomUser.team != Team.none && roomUser.GetClient() != null)
                {
                    roomUser.banzaiPowerUp = FreezePowerUp.None;
                    roomUser.FreezeLives = 3;
                    roomUser.shieldActive = false;
                    roomUser.shieldCounter = 11;
                    this.room.GetGameManager().AddPointToTeam(roomUser.team, 30, (RoomUser)null);
                    ServerMessage serverMessage = new ServerMessage();
                    serverMessage.Init(Outgoing.UpdateFreezeLivesMessageComposer);
                    serverMessage.AppendInt32(roomUser.InternalRoomID);
                    serverMessage.AppendInt32(roomUser.FreezeLives);
                    roomUser.GetClient().SendMessage(serverMessage);
                }
            }
        }

        private static void RemoveUserFromTeam(RoomUser user)
        {
            user.team = Team.none;
            user.ApplyEffect(-1);
        }

        private RoomItem GetFirstTile(int x, int y)
        {
            foreach (RoomItem roomItem in this.room.GetGameMap().GetCoordinatedItems(new Point(x, y)))
            {
                if (roomItem.GetBaseItem().InteractionType == InteractionType.freezetile)
                    return roomItem;
            }
            return (RoomItem)null;
        }

        internal void onFreezeTiles(RoomItem item, FreezePowerUp powerUp, uint userID)
        {
            if (this.room.GetRoomUserManager().GetRoomUserByHabbo(userID) == null)
                return;
            List<RoomItem> items;
            switch (powerUp)
            {
                case FreezePowerUp.BlueArrow:
                    items = this.GetVerticalItems(item.GetX, item.GetY, 5);
                    break;
                case FreezePowerUp.GreenArrow:
                    items = this.GetDiagonalItems(item.GetX, item.GetY, 5);
                    break;
                case FreezePowerUp.OrangeSnowball:
                    items = this.GetVerticalItems(item.GetX, item.GetY, 5);
                    items.AddRange((IEnumerable<RoomItem>)this.GetDiagonalItems(item.GetX, item.GetY, 5));
                    break;
                default:
                    items = this.GetVerticalItems(item.GetX, item.GetY, 3);
                    break;
            }
            this.HandleBanzaiFreezeItems(items);
        }

        private static void ActivateShield(RoomUser user)
        {
            user.ApplyEffect((int)(user.team + 48));
            user.shieldActive = true;
            user.shieldCounter = 0;
        }

        private void HandleBanzaiFreezeItems(List<RoomItem> items)
        {
            foreach (RoomItem roomItem in items)
            {
                switch (roomItem.GetBaseItem().InteractionType)
                {
                    case InteractionType.freezetileblock:
                        this.SetRandomPowerUp(roomItem);
                        roomItem.UpdateState(false, true);
                        continue;
                    case InteractionType.freezetile:
                        roomItem.ExtraData = "11000";
                        roomItem.UpdateState(false, true);
                        continue;
                    default:
                        continue;
                }
            }
        }

        private void SetRandomPowerUp(RoomItem item)
        {
            if (!string.IsNullOrEmpty(item.ExtraData))
                return;
            switch (this.rnd.Next(1, 14))
            {
                case 2:
                    item.ExtraData = "2000";
                    item.freezePowerUp = FreezePowerUp.BlueArrow;
                    break;
                case 3:
                    item.ExtraData = "3000";
                    item.freezePowerUp = FreezePowerUp.Snowballs;
                    break;
                case 4:
                    item.ExtraData = "4000";
                    item.freezePowerUp = FreezePowerUp.GreenArrow;
                    break;
                case 5:
                    item.ExtraData = "5000";
                    item.freezePowerUp = FreezePowerUp.OrangeSnowball;
                    break;
                case 6:
                    item.ExtraData = "6000";
                    item.freezePowerUp = FreezePowerUp.Heart;
                    break;
                case 7:
                    item.ExtraData = "7000";
                    item.freezePowerUp = FreezePowerUp.Shield;
                    break;
                default:
                    item.ExtraData = "1000";
                    item.freezePowerUp = FreezePowerUp.None;
                    break;
            }
            this.room.GetGameMap().RemoveFromMap(item, false);
            item.UpdateState(false, true);
        }

        private void PickUpPowerUp(RoomItem item, RoomUser user)
        {
            switch (item.freezePowerUp)
            {
                case FreezePowerUp.BlueArrow:
                case FreezePowerUp.GreenArrow:
                case FreezePowerUp.OrangeSnowball:
                    user.banzaiPowerUp = item.freezePowerUp;
                    break;
                case FreezePowerUp.Shield:
                    Freeze.ActivateShield(user);
                    break;
                case FreezePowerUp.Heart:
                    if (user.FreezeLives < 5)
                    {
                        checked { ++user.FreezeLives; }
                        this.room.GetGameManager().AddPointToTeam(user.team, 10, user);
                    }
                    ServerMessage serverMessage = new ServerMessage();
                    serverMessage.Init(Outgoing.UpdateFreezeLivesMessageComposer);
                    serverMessage.AppendInt32(user.InternalRoomID);
                    serverMessage.AppendInt32(user.FreezeLives);
                    user.GetClient().SendMessage(serverMessage);
                    break;
            }
            item.freezePowerUp = FreezePowerUp.None;
            item.ExtraData = "1" + item.ExtraData;
            item.UpdateState(false, true);
        }

        internal void AddFreezeTile(RoomItem item)
        {
            if (!this.freezeTiles.Contains(item.Id))
                this.freezeTiles.Remove(item.Id);
            this.freezeTiles.Add(item.Id, item);
        }

        internal void RemoveFreezeTile(uint itemID)
        {
            if (!this.freezeTiles.Contains(itemID))
                return;
            this.freezeTiles.Remove(itemID);
        }

        internal void AddFreezeBlock(RoomItem item)
        {
            if (this.freezeBlocks.Contains(item.Id))
                this.freezeBlocks.Remove(item.Id);
            this.freezeBlocks.Add(item.Id, item);
        }

        internal void RemoveFreezeBlock(uint itemID)
        {
            this.freezeBlocks.Remove(itemID);
        }

        private void HandleUserFreeze(Point point)
        {
            RoomUser user = Enumerable.FirstOrDefault<RoomUser>((IEnumerable<RoomUser>)this.room.GetGameMap().GetRoomUsers(point));
            if (user == null || user.IsWalking && user.SetX != point.X && user.SetY != point.Y)
                return;
            this.FreezeUser(user);
        }

        private void FreezeUser(RoomUser user)
        {
            if (user.IsBot || user.shieldActive || user.team == Team.none || user.Freezed)
                return;
            user.Freezed = true;
            user.FreezeCounter = 0;
            checked { --user.FreezeLives; }
            if (user.FreezeLives <= 0)
            {
                ServerMessage serverMessage = new ServerMessage();
                serverMessage.Init(Outgoing.UpdateFreezeLivesMessageComposer);
                serverMessage.AppendInt32(user.InternalRoomID);
                serverMessage.AppendInt32(user.FreezeLives);
                user.GetClient().SendMessage(serverMessage);
                user.ApplyEffect(-1);
                this.room.GetGameManager().AddPointToTeam(user.team, -10, user);
                TeamManager managerForFreeze = this.room.GetTeamManagerForFreeze();
                managerForFreeze.OnUserLeave(user);
                user.team = Team.none;
                if (this.exitTeleport != null)
                    this.room.GetGameMap().TeleportToItem(user, this.exitTeleport);
                user.Freezed = false;
                user.SetStep = false;
                user.IsWalking = false;
                user.UpdateNeeded = true;
                if (managerForFreeze.BlueTeam.Count <= 0 && managerForFreeze.RedTeam.Count <= 0 && managerForFreeze.GreenTeam.Count <= 0 && managerForFreeze.YellowTeam.Count > 0)
                    this.StopGame();
                else if (managerForFreeze.BlueTeam.Count > 0 && managerForFreeze.RedTeam.Count <= 0 && managerForFreeze.GreenTeam.Count <= 0 && managerForFreeze.YellowTeam.Count <= 0)
                    this.StopGame();
                else if (managerForFreeze.BlueTeam.Count <= 0 && managerForFreeze.RedTeam.Count > 0 && managerForFreeze.GreenTeam.Count <= 0 && managerForFreeze.YellowTeam.Count <= 0)
                {
                    this.StopGame();
                }
                else
                {
                    if (managerForFreeze.BlueTeam.Count > 0 || managerForFreeze.RedTeam.Count > 0 || managerForFreeze.GreenTeam.Count <= 0 || managerForFreeze.YellowTeam.Count > 0)
                        return;
                    this.StopGame();
                }
            }
            else
            {
                this.room.GetGameManager().AddPointToTeam(user.team, -10, user);
                user.ApplyEffect(12);
                ServerMessage serverMessage = new ServerMessage();
                serverMessage.Init(Outgoing.UpdateFreezeLivesMessageComposer);
                serverMessage.AppendInt32(user.InternalRoomID);
                serverMessage.AppendInt32(user.FreezeLives);
                user.GetClient().SendMessage(serverMessage);
            }
        }

        private static void ExitGame(RoomUser user)
        {
            user.team = Team.none;
        }

        private List<RoomItem> GetVerticalItems(int x, int y, int length)
        {
            List<RoomItem> list = new List<RoomItem>();
            int num1 = 0;
            Point point;
            while (num1 < length)
            {
                point = new Point(checked(x + num1), y);
                List<RoomItem> itemsForSquare = this.GetItemsForSquare(point);
                if (Freeze.SquareGotFreezeTile(itemsForSquare))
                {
                    this.HandleUserFreeze(point);
                    list.AddRange((IEnumerable<RoomItem>)itemsForSquare);
                    if (!Freeze.SquareGotFreezeBlock(itemsForSquare))
                        checked { ++num1; }
                    else
                        break;
                }
                else
                    break;
            }
            int num2 = 1;
            while (num2 < length)
            {
                point = new Point(x, checked(y + num2));
                List<RoomItem> itemsForSquare = this.GetItemsForSquare(point);
                if (Freeze.SquareGotFreezeTile(itemsForSquare))
                {
                    this.HandleUserFreeze(point);
                    list.AddRange((IEnumerable<RoomItem>)itemsForSquare);
                    if (!Freeze.SquareGotFreezeBlock(itemsForSquare))
                        checked { ++num2; }
                    else
                        break;
                }
                else
                    break;
            }
            int num3 = 1;
            while (num3 < length)
            {
                point = new Point(checked(x - num3), y);
                List<RoomItem> itemsForSquare = this.GetItemsForSquare(point);
                if (Freeze.SquareGotFreezeTile(itemsForSquare))
                {
                    this.HandleUserFreeze(point);
                    list.AddRange((IEnumerable<RoomItem>)itemsForSquare);
                    if (!Freeze.SquareGotFreezeBlock(itemsForSquare))
                        checked { ++num3; }
                    else
                        break;
                }
                else
                    break;
            }
            int num4 = 1;
            while (num4 < length)
            {
                point = new Point(x, checked(y - num4));
                List<RoomItem> itemsForSquare = this.GetItemsForSquare(point);
                if (Freeze.SquareGotFreezeTile(itemsForSquare))
                {
                    this.HandleUserFreeze(point);
                    list.AddRange((IEnumerable<RoomItem>)itemsForSquare);
                    if (!Freeze.SquareGotFreezeBlock(itemsForSquare))
                        checked { ++num4; }
                    else
                        break;
                }
                else
                    break;
            }
            return list;
        }

        private List<RoomItem> GetDiagonalItems(int x, int y, int length)
        {
            List<RoomItem> list = new List<RoomItem>();
            int num1 = 0;
            Point point;
            while (num1 < length)
            {
                point = new Point(checked(x + num1), checked(y + num1));
                List<RoomItem> itemsForSquare = this.GetItemsForSquare(point);
                if (Freeze.SquareGotFreezeTile(itemsForSquare))
                {
                    this.HandleUserFreeze(point);
                    list.AddRange((IEnumerable<RoomItem>)itemsForSquare);
                    if (!Freeze.SquareGotFreezeBlock(itemsForSquare))
                        checked { ++num1; }
                    else
                        break;
                }
                else
                    break;
            }
            int num2 = 0;
            while (num2 < length)
            {
                point = new Point(checked(x - num2), checked(y - num2));
                List<RoomItem> itemsForSquare = this.GetItemsForSquare(point);
                if (Freeze.SquareGotFreezeTile(itemsForSquare))
                {
                    this.HandleUserFreeze(point);
                    list.AddRange((IEnumerable<RoomItem>)itemsForSquare);
                    if (!Freeze.SquareGotFreezeBlock(itemsForSquare))
                        checked { ++num2; }
                    else
                        break;
                }
                else
                    break;
            }
            int num3 = 0;
            while (num3 < length)
            {
                point = new Point(checked(x - num3), checked(y + num3));
                List<RoomItem> itemsForSquare = this.GetItemsForSquare(point);
                if (Freeze.SquareGotFreezeTile(itemsForSquare))
                {
                    this.HandleUserFreeze(point);
                    list.AddRange((IEnumerable<RoomItem>)itemsForSquare);
                    if (!Freeze.SquareGotFreezeBlock(itemsForSquare))
                        checked { ++num3; }
                    else
                        break;
                }
                else
                    break;
            }
            int num4 = 0;
            while (num4 < length)
            {
                point = new Point(checked(x + num4), checked(y - num4));
                List<RoomItem> itemsForSquare = this.GetItemsForSquare(point);
                if (Freeze.SquareGotFreezeTile(itemsForSquare))
                {
                    this.HandleUserFreeze(point);
                    list.AddRange((IEnumerable<RoomItem>)itemsForSquare);
                    if (!Freeze.SquareGotFreezeBlock(itemsForSquare))
                        checked { ++num4; }
                    else
                        break;
                }
                else
                    break;
            }
            return list;
        }

        private List<RoomItem> GetItemsForSquare(Point point)
        {
            return this.room.GetGameMap().GetCoordinatedItems(point);
        }

        private static bool SquareGotFreezeTile(List<RoomItem> items)
        {
            foreach (RoomItem roomItem in items)
            {
                if (roomItem.GetBaseItem().InteractionType == InteractionType.freezetile)
                    return true;
            }
            return false;
        }

        private static bool SquareGotFreezeBlock(List<RoomItem> items)
        {
            foreach (RoomItem roomItem in items)
            {
                if (roomItem.GetBaseItem().InteractionType == InteractionType.freezetileblock)
                    return true;
            }
            return false;
        }

        internal void Destroy()
        {
            this.freezeBlocks.Clear();
            this.freezeTiles.Clear();
            this.room = (Room)null;
            this.freezeTiles = (HybridDictionary)null;
            this.freezeBlocks = (HybridDictionary)null;
            this.exitTeleport = (RoomItem)null;
            this.rnd = (Random)null;
        }

        internal void FreezeStart()
        {
            throw new NotImplementedException();
        }

        internal void FreezeEnd()
        {
            throw new NotImplementedException();
        }
    }

}
