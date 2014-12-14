using Database_Manager.Database.Session_Details.Interfaces;
using Cyber.Collections;
using Cyber.Core;
using Cyber.HabboHotel.GameClients;
using Cyber.HabboHotel.Items;
using Cyber.HabboHotel.PathFinding;
using Cyber.HabboHotel.Pets;
using Cyber.HabboHotel.Quests;
using Cyber.HabboHotel.RoomBots;
using Cyber.HabboHotel.Navigators;
using Cyber.HabboHotel.Rooms.Games;
using Cyber.HabboHotel.Users.Inventory;
using Cyber.Messages;
using Cyber.Messages.Headers;
using Cyber.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Collections.Specialized;

namespace Cyber.HabboHotel.Rooms
{
    internal class RoomUserManager
    {
        private Room room;
        internal HybridDictionary usersByUsername;
        internal HybridDictionary usersByUserID;
        private HybridDictionary pets;
        private HybridDictionary bots;
        private QueuedDictionary<int, RoomUser> userlist;
        internal Dictionary<Point, RoomUser> ToSet;
        private int petCount;
        private int userCount;
        private int primaryPrivateUserID;
        private int secondaryPrivateUserID;
        private List<RoomUser> ToRemove;
        internal event RoomEventDelegate OnUserEnter;
        internal int PetCount
        {
            get
            {
                return this.petCount;
            }
        }
        internal QueuedDictionary<int, RoomUser> UserList
        {
            get
            {
                return this.userlist;
            }
        }
        internal int GetRoomUserCount()
        {
            return (this.userlist.Inner.Count - bots.Count - pets.Count);
        }
        public RoomUserManager(Room room)
        {
            this.room = room;
            this.userlist = new QueuedDictionary<int, RoomUser>(new EventHandler(this.OnUserAdd), null, new EventHandler(this.onRemove), null);
            this.pets = new HybridDictionary();
            this.bots = new HybridDictionary();
            this.usersByUsername = new HybridDictionary();
            this.usersByUserID = new HybridDictionary();
            this.primaryPrivateUserID = 0;
            this.secondaryPrivateUserID = 0;
            this.ToRemove = new List<RoomUser>(room.UsersMax);
            this.ToSet = new Dictionary<Point, RoomUser>();
            this.petCount = 0;
            this.userCount = 0;
        }
        internal RoomUser DeployBot(RoomBot Bot, Pet PetData)
        {
            int virtualId = this.primaryPrivateUserID++;
            RoomUser roomUser = new RoomUser(0u, this.room.RoomId, virtualId, this.room, false);
            int num = this.secondaryPrivateUserID++;
            roomUser.InternalRoomID = num;
            this.userlist.Add(num, roomUser);
            DynamicRoomModel model = this.room.GetGameMap().Model;
            Point coord = new Point(Bot.X, Bot.Y);
            if (Bot.X > 0 && Bot.Y > 0 && Bot.X < model.MapSizeX && Bot.Y < model.MapSizeY)
            {
                this.room.GetGameMap().AddUserToMap(roomUser, coord);
                roomUser.SetPos(Bot.X, Bot.Y, Bot.Z);
                roomUser.SetRot(Bot.Rot, false);
            }
            else
            {
                Bot.X = model.DoorX;
                Bot.Y = model.DoorY;
                roomUser.SetPos(model.DoorX, model.DoorY, model.DoorZ);
                roomUser.SetRot(model.DoorOrientation, false);
            }
            roomUser.BotData = Bot;
            checked
            {
                roomUser.BotAI = Bot.GenerateBotAI(roomUser.VirtualId, (int)Bot.BotId);
                if (roomUser.IsPet)
                {
                    roomUser.BotAI.Init(Bot.BotId, roomUser.VirtualId, this.room.RoomId, roomUser, this.room);
                    roomUser.PetData = PetData;
                    roomUser.PetData.VirtualId = roomUser.VirtualId;
                }
                else
                {
                    roomUser.BotAI.Init(Bot.BotId, roomUser.VirtualId, this.room.RoomId, roomUser, this.room);
                }
                this.UpdateUserStatus(roomUser, false);
                roomUser.UpdateNeeded = true;
                ServerMessage serverMessage = new ServerMessage(Outgoing.SetRoomUserMessageComposer);
                serverMessage.AppendInt32(1);
                roomUser.Serialize(serverMessage, this.room.GetGameMap().gotPublicPool);
                this.room.SendMessage(serverMessage);
                roomUser.BotAI.OnSelfEnterRoom();
                if (roomUser.IsPet)
                {
                    if (this.pets.Contains(roomUser.PetData.PetId))
                    {
                        this.pets[roomUser.PetData.PetId] = roomUser;
                    }
                    else
                    {
                        this.pets.Add(roomUser.PetData.PetId, roomUser);
                    }
                    this.petCount++;
                }
                if (roomUser.BotData.AiType == AIType.Generic)
                {
                    if (this.bots.Contains(roomUser.BotData.BotId))
                    {
                        this.bots[roomUser.BotData.BotId] = roomUser;
                    }
                    else
                    {
                        this.bots.Add(roomUser.BotData.BotId, roomUser);
                    }
                    serverMessage.Init(Outgoing.DanceStatusMessageComposer);
                    serverMessage.AppendInt32(roomUser.VirtualId);
                    serverMessage.AppendInt32(roomUser.BotData.DanceId);
                    this.room.SendMessage(serverMessage);
                    this.petCount++;
                }
                return roomUser;
            }
        }
        internal void RemoveBot(int VirtualId, bool Kicked)
        {
            RoomUser roomUserByVirtualId = this.GetRoomUserByVirtualId(VirtualId);
            checked
            {
                if (roomUserByVirtualId != null && roomUserByVirtualId.IsBot)
                {
                    if (roomUserByVirtualId.IsPet)
                    {
                        this.pets.Remove(roomUserByVirtualId.PetData.PetId);
                        this.petCount--;
                    }
                    roomUserByVirtualId.BotData.WasPicked = true;
                    roomUserByVirtualId.BotAI.OnSelfLeaveRoom(Kicked);
                    ServerMessage serverMessage = new ServerMessage(Outgoing.UserLeftRoomMessageComposer);
                    serverMessage.AppendString(roomUserByVirtualId.VirtualId.ToString());
                    this.room.SendMessage(serverMessage);
                    this.userlist.Remove(roomUserByVirtualId.InternalRoomID);
                }
            }
        }
        private void UpdateUserEffect(RoomUser User, int x, int y)
        {
            if (User.IsBot)
            {
                return;
            }
            try
            {
                byte b = this.room.GetGameMap().EffectMap[x, y];
                if (b > 0)
                {
                    if (User.GetClient().GetHabbo().GetAvatarEffectsInventoryComponent().CurrentEffect == 0)
                    {
                        User.CurrentItemEffect = ItemEffectType.None;
                    }
                    ItemEffectType itemEffectType = ByteToItemEffectEnum.Parse(b);
                    if (itemEffectType != User.CurrentItemEffect)
                    {
                        switch (itemEffectType)
                        {
                            case ItemEffectType.None:
                                User.GetClient().GetHabbo().GetAvatarEffectsInventoryComponent().ActivateCustomEffect(-1);
                                User.CurrentItemEffect = itemEffectType;
                                break;
                            case ItemEffectType.Swim:
                                User.GetClient().GetHabbo().GetAvatarEffectsInventoryComponent().ActivateCustomEffect(28);
                                User.CurrentItemEffect = itemEffectType;
                                break;
                            case ItemEffectType.SwimLow:
                                User.GetClient().GetHabbo().GetAvatarEffectsInventoryComponent().ActivateCustomEffect(30);
                                User.CurrentItemEffect = itemEffectType;
                                break;
                            case ItemEffectType.SwimHalloween:
                                User.GetClient().GetHabbo().GetAvatarEffectsInventoryComponent().ActivateCustomEffect(37);
                                User.CurrentItemEffect = itemEffectType;
                                break;
                            case ItemEffectType.Iceskates:
                                if (User.GetClient().GetHabbo().Gender.ToUpper() == "M")
                                {
                                    User.GetClient().GetHabbo().GetAvatarEffectsInventoryComponent().ActivateCustomEffect(38);
                                }
                                else
                                {
                                    User.GetClient().GetHabbo().GetAvatarEffectsInventoryComponent().ActivateCustomEffect(39);
                                }
                                User.CurrentItemEffect = ItemEffectType.Iceskates;
                                break;
                            case ItemEffectType.Normalskates:
                                if (User.GetClient().GetHabbo().Gender.ToUpper() == "M")
                                {
                                    User.GetClient().GetHabbo().GetAvatarEffectsInventoryComponent().ActivateCustomEffect(55);
                                }
                                else
                                {
                                    User.GetClient().GetHabbo().GetAvatarEffectsInventoryComponent().ActivateCustomEffect(56);
                                }
                                User.CurrentItemEffect = itemEffectType;
                                break;

                            case ItemEffectType.SnowBoard:
                                {
                                    User.GetClient().GetHabbo().GetAvatarEffectsInventoryComponent().ActivateCustomEffect(97);
                                    User.CurrentItemEffect = itemEffectType;
                                }
                                break;
                        }
                    }
                }
                else
                {
                    if (User.CurrentItemEffect != ItemEffectType.None && b == 0)
                    {
                        User.GetClient().GetHabbo().GetAvatarEffectsInventoryComponent().ActivateCustomEffect(-1);
                        User.CurrentItemEffect = ItemEffectType.None;
                    }
                }
            }
            catch
            {
            }
        }
        internal RoomUser GetUserForSquare(int x, int y)
        {
            return this.room.GetGameMap().GetRoomUsers(new Point(x, y)).FirstOrDefault<RoomUser>();
        }
        internal void AddUserToRoom(GameClient Session, bool Spectator, bool snow = false)
        {
            RoomUser roomUser = new RoomUser(Session.GetHabbo().Id, this.room.RoomId, this.primaryPrivateUserID++, this.room, Spectator);
            if (roomUser == null || roomUser.GetClient() == null)
            {
                return;
            }
            roomUser.GetClient().GetHabbo();
            roomUser.UserID = Session.GetHabbo().Id;
            string username = Session.GetHabbo().Username;
            uint userID = roomUser.UserID;
            if (this.usersByUsername.Contains(username.ToLower()))
            {
                this.usersByUsername.Remove(username.ToLower());
            }
            if (this.usersByUserID.Contains(userID))
            {
                this.usersByUserID.Remove(userID);
            }
            this.usersByUsername.Add(Session.GetHabbo().Username.ToLower(), roomUser);
            this.usersByUserID.Add(Session.GetHabbo().Id, roomUser);
            int num = this.secondaryPrivateUserID++;
            roomUser.InternalRoomID = num;
            Session.CurrentRoomUserID = num;
            Session.GetHabbo().CurrentRoomId = this.room.RoomId;
            this.UserList.Add(num, roomUser);

            if (CyberEnvironment.GetGame().GetNavigator().PrivateCategories.Contains(room.Category))
            {
                ((FlatCat)CyberEnvironment.GetGame().GetNavigator().PrivateCategories[room.Category]).addUser();
            }
        }
        private void OnUserAdd(object sender, EventArgs args)
        {
            try
            {
                if (sender != null)
                {
                    RoomUser value = ((KeyValuePair<int, RoomUser>)sender).Value;
                    if (value != null && value.GetClient() != null && value.GetClient().GetHabbo() != null)
                    {
                        GameClient client = value.GetClient();
                        if (client != null && client.GetHabbo() != null && this.room != null)
                        {
                            if (!value.IsSpectator)
                            {
                                DynamicRoomModel model = this.room.GetGameMap().Model;
                                if (model == null)
                                {
                                    return;
                                }
                                value.SetPos(model.DoorX, model.DoorY, model.DoorZ);
                                value.SetRot(model.DoorOrientation, false);
                                if (this.room.CheckRights(client, true, false))
                                {
                                    value.AddStatus("flatctrl 4", "");
                                }
                                else
                                {
                                    if (this.room.CheckRights(client, false, true))
                                    {
                                        value.AddStatus("flatctrl 1", "");
                                    }
                                    else
                                    {
                                        if (this.room.CheckRights(client))
                                        {
                                            value.AddStatus("flatctrl 1", "");
                                        }
                                    }
                                }
                                value.CurrentItemEffect = ItemEffectType.None;
                                if (!value.IsBot && value.GetClient().GetHabbo().IsTeleporting)
                                {
                                    RoomItem item = this.room.GetRoomItemHandler().GetItem(value.GetClient().GetHabbo().TeleporterId);
                                    if (item != null)
                                    {
                                        item.ExtraData = "2";
                                        item.UpdateState(false, true);
                                        value.SetPos(item.GetX, item.GetY, item.GetZ);
                                        value.SetRot(item.Rot, false);
                                        item.InteractingUser2 = client.GetHabbo().Id;
                                        item.ExtraData = "0";
                                        item.UpdateState(false, true);
                                    }
                                }
                                if (!value.IsBot && value.GetClient().GetHabbo().IsHopping)
                                {
                                    RoomItem item2 = this.room.GetRoomItemHandler().GetItem(value.GetClient().GetHabbo().HopperId);
                                    if (item2 != null)
                                    {
                                        item2.ExtraData = "1";
                                        item2.UpdateState(false, true);
                                        value.SetPos(item2.GetX, item2.GetY, item2.GetZ);
                                        value.SetRot(item2.Rot, false);
                                        value.AllowOverride = false;
                                        item2.InteractingUser2 = client.GetHabbo().Id;
                                        item2.ExtraData = "2";
                                        item2.UpdateState(false, true);
                                    }
                                }

                                if (!value.IsSpectator)
                                {
                                    ServerMessage serverMessage = new ServerMessage(Outgoing.SetRoomUserMessageComposer);
                                    serverMessage.AppendInt32(1);
                                    value.Serialize(serverMessage, this.room.GetGameMap().gotPublicPool);
                                    this.room.SendMessage(serverMessage);
                                }
                                if (!value.IsBot)
                                {
                                    ServerMessage serverMessage2 = new ServerMessage();
                                    serverMessage2.Init(Outgoing.UpdateUserDataMessageComposer);
                                    serverMessage2.AppendInt32(value.VirtualId);
                                    serverMessage2.AppendString(value.GetClient().GetHabbo().Look);
                                    serverMessage2.AppendString(value.GetClient().GetHabbo().Gender.ToLower());
                                    serverMessage2.AppendString(value.GetClient().GetHabbo().Motto);
                                    serverMessage2.AppendInt32(value.GetClient().GetHabbo().AchievementPoints);
                                    this.room.SendMessage(serverMessage2);
                                }
                                if (this.room.Owner != client.GetHabbo().Username)
                                {
                                    CyberEnvironment.GetGame().GetQuestManager().ProgressUserQuest(value.GetClient(), QuestType.SOCIAL_VISIT, 0u);
                                    CyberEnvironment.GetGame().GetAchievementManager().ProgressUserAchievement(value.GetClient(), "ACH_RoomEntry", 1, false);
                                }
                            }
                            if (client.GetHabbo().GetMessenger() != null)
                            {
                                client.GetHabbo().GetMessenger().OnStatusChanged(true);
                            }
                            value.GetClient().GetMessageHandler().OnRoomUserAdd();


                            if (this.OnUserEnter != null)
                            {
                                this.OnUserEnter(value, null);
                            }
                            if (this.room.GotMusicController() && this.room.GotMusicController())
                            {
                                this.room.GetRoomMusicController().OnNewUserEnter(value);
                            }


                            this.room.OnUserEnter(value);

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logging.LogCriticalException(ex.ToString());
            }
        }
        internal void UpdateUser(string OldName, string NewName)
        {
            if (OldName == NewName)
            {
                return;
            }

            if (this.usersByUsername.Contains(OldName))
            {
                this.usersByUsername.Add(NewName, this.usersByUsername[OldName]);
                this.usersByUsername.Remove(OldName);
                // 
                CyberEnvironment.GetGame().GetClientManager().UpdateClient(OldName, NewName);
            }
        }
        internal void RequestRoomReload()
        {
            this.userlist.QueueDelegate(new onCycleDoneDelegate(this.room.onReload));
        }
        internal void UpdateUserStats(HashSet<RoomUser> users, Hashtable userID, Hashtable userName, int primaryID, int secondaryID)
        {
            foreach (RoomUser current in users)
            {
                this.userlist.Inner.Add(current.InternalRoomID, current);
            }
            foreach (RoomUser roomUser in userName.Values)
            {
                this.usersByUsername.Add(roomUser.GetClient().GetHabbo().Username.ToLower(), roomUser);
            }
            foreach (RoomUser roomUser2 in userID.Values)
            {
                this.usersByUserID.Add(roomUser2.UserID, roomUser2);
            }
            this.primaryPrivateUserID = primaryID;
            this.secondaryPrivateUserID = secondaryID;
        }
        internal void RemoveUserFromRoom(GameClient Session, bool NotifyClient, bool NotifyKick)
        {
            try
            {
                if (Session == null || Session.GetHabbo() == null || this.room == null)
                {
                    return;
                }
                Session.GetHabbo().GetAvatarEffectsInventoryComponent().OnRoomExit();
                using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
                {
                    queryreactor.runFastQuery(string.Concat(new object[]
					{
						"UPDATE user_roomvisits SET exit_timestamp = '",
						CyberEnvironment.GetUnixTimestamp(),
						"' WHERE room_id = '",
						this.room.RoomId,
						"' AND user_id = '",
						Session.GetHabbo().Id,
						"' ORDER BY entry_timestamp DESC LIMIT 1"
					}));
                }
                RoomUser roomUserByHabbo = this.GetRoomUserByHabbo(Session.GetHabbo().Id);
                if (roomUserByHabbo == null)
                {
                    return;
                }
                if (NotifyKick || (NotifyClient && NotifyKick))
                {
                    Room room = CyberEnvironment.GetGame().GetRoomManager().GetRoom(roomUserByHabbo.RoomId);
                    DynamicRoomModel model = room.GetGameMap().Model;
                    roomUserByHabbo.MoveTo(model.DoorX, model.DoorY);
                    roomUserByHabbo.CanWalk = false;
                    Session.GetMessageHandler().GetResponse().Init(Outgoing.RoomErrorMessageComposer);
                    Session.GetMessageHandler().GetResponse().AppendInt32(4008);
                    Session.GetMessageHandler().SendResponse();


                    Session.GetMessageHandler().GetResponse().Init(Outgoing.OutOfRoomMessageComposer);
                    Session.GetMessageHandler().GetResponse().AppendShort(2);
                    Session.GetMessageHandler().SendResponse();
                }
                else
                {
                    if (NotifyClient && !NotifyKick)
                    {
                        ServerMessage serverMessage = new ServerMessage(Outgoing.UserIsPlayingFreezeMessageComposer);
                        serverMessage.AppendBoolean(roomUserByHabbo.team != Team.none);
                        roomUserByHabbo.GetClient().SendMessage(serverMessage);
                        Session.GetMessageHandler().GetResponse().Init(Outgoing.OutOfRoomMessageComposer);
                        Session.GetMessageHandler().GetResponse().AppendShort(2);
                        Session.GetMessageHandler().SendResponse();
                    }
                }
                if (roomUserByHabbo != null)
                {
                    if (roomUserByHabbo.team != Team.none)
                    {
                        this.room.GetTeamManagerForBanzai().OnUserLeave(roomUserByHabbo);
                        this.room.GetTeamManagerForFreeze().OnUserLeave(roomUserByHabbo);
                    }
                    if (roomUserByHabbo.RidingHorse)
                    {
                        roomUserByHabbo.RidingHorse = false;
                        RoomUser Horse = this.GetRoomUserByVirtualId((int)roomUserByHabbo.HorseID);
                        if (Horse != null)
                        {
                            Horse.RidingHorse = false;
                            Horse.HorseID = 0u;
                        }
                    }
                    if (roomUserByHabbo.IsLyingDown || roomUserByHabbo.IsSitting)
                    {
                        roomUserByHabbo.IsSitting = false;
                        roomUserByHabbo.IsLyingDown = false;
                    }
                    this.RemoveRoomUser(roomUserByHabbo);
                    if (Session.GetHabbo() != null && !roomUserByHabbo.IsSpectator)
                    {
                        if (roomUserByHabbo.CurrentItemEffect != ItemEffectType.None)
                        {
                            roomUserByHabbo.GetClient().GetHabbo().GetAvatarEffectsInventoryComponent().CurrentEffect = -1;
                        }
                        if (Session.GetHabbo() != null)
                        {
                            if (this.room.HasActiveTrade(Session.GetHabbo().Id))
                            {
                                this.room.TryStopTrade(Session.GetHabbo().Id);
                            }
                            Session.GetHabbo().CurrentRoomId = 0u;
                            if (Session.GetHabbo().GetMessenger() != null)
                            {
                                Session.GetHabbo().GetMessenger().OnStatusChanged(true);
                            }
                        }
                        DateTime now = DateTime.Now;
                        using (IQueryAdapter queryreactor2 = CyberEnvironment.GetDatabaseManager().getQueryReactor())
                        {

                            if (Session.GetHabbo() != null)
                            {
                                queryreactor2.runFastQuery(string.Concat(new object[]
								{
									"UPDATE user_roomvisits SET exit_timestamp = '",
									CyberEnvironment.GetUnixTimestamp(),
									"' WHERE room_id = '",
									this.room.RoomId,
									"' AND user_id = '",
									Session.GetHabbo().Id,
									"' ORDER BY exit_timestamp DESC LIMIT 1"
								}));
                            }
                        }
                    }
                    this.usersByUserID.Remove(roomUserByHabbo.UserID);
                    if (Session.GetHabbo() != null)
                    {
                        this.usersByUsername.Remove(Session.GetHabbo().Username.ToLower());
                    }
                    roomUserByHabbo.Dispose();
                }
            }
            catch (Exception ex)
            {
                Logging.LogCriticalException("Error during removing user from room:" + ex.ToString());
            }
        }
        private void onRemove(object sender, EventArgs args)
        {
            try
            {
                KeyValuePair<int, RoomUser> keyValuePair = (KeyValuePair<int, RoomUser>)sender;
                RoomUser value = keyValuePair.Value;
                GameClient client = value.GetClient();
                int arg_1D_0 = keyValuePair.Key;
                List<RoomUser> list = new List<RoomUser>();
                foreach (RoomUser current in this.UserList.Values)
                {
                    if (current.IsBot && !current.IsPet)
                    {
                        list.Add(current);
                    }
                }
                List<RoomUser> list2 = new List<RoomUser>();
                foreach (RoomUser current2 in list)
                {
                    current2.BotAI.OnUserLeaveRoom(client);
                    if (current2.IsPet && current2.PetData.OwnerId == value.UserID && !this.room.CheckRights(client, true, false))
                    {
                        list2.Add(current2);
                    }
                }
                foreach (RoomUser current3 in list2)
                {
                    if (value.GetClient() != null && value.GetClient().GetHabbo() != null && value.GetClient().GetHabbo().GetInventoryComponent() != null)
                    {
                        value.GetClient().GetHabbo().GetInventoryComponent().AddPet(current3.PetData);
                        this.RemoveBot(current3.VirtualId, false);
                    }
                }
                this.room.GetGameMap().RemoveUserFromMap(value, new Point(value.X, value.Y));
            }
            catch (Exception ex)
            {
                Logging.LogCriticalException(ex.ToString());
            }
        }
        internal void RemoveRoomUser(RoomUser user)
        {
            this.UserList.Remove(user.InternalRoomID);
            user.InternalRoomID = -1;
            this.room.GetGameMap().GameMap[user.X, user.Y] = user.SqState;
            this.room.GetGameMap().RemoveUserFromMap(user, new Point(user.X, user.Y));
            ServerMessage serverMessage = new ServerMessage(Outgoing.UserLeftRoomMessageComposer);
            serverMessage.AppendString(user.VirtualId.ToString());
            this.room.SendMessage(serverMessage);
        }
        internal RoomUser GetPet(uint PetId)
        {
            if (this.pets.Contains(PetId))
            {
                return (RoomUser)this.pets[PetId];
            }
            return null;
        }
        internal RoomUser GetBot(uint BotId)
        {
            if (this.bots.Contains(BotId))
            {
                return (RoomUser)this.bots[BotId];
            }
            Logging.WriteLine("Couldn't get BOT: " + BotId, ConsoleColor.Gray);
            return null;
        }
        internal void UpdateUserCount(int count)
        {
            this.userCount = count;
            this.room.RoomData.UsersNow = count;
            using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
            {
                queryreactor.runFastQuery(string.Concat(new object[]
				{
					"UPDATE rooms SET users_now = ",
					count,
					" WHERE id = ",
					this.room.RoomId,
					" LIMIT 1"
				}));
            }
            CyberEnvironment.GetGame().GetRoomManager().QueueActiveRoomUpdate(this.room.RoomData);
        }
        internal RoomUser GetRoomUserByVirtualId(int VirtualId)
        {
            return this.UserList.GetValue(VirtualId);
        }
        public RoomUser GetRoomUserByHabbo(uint pId)
        {
            if (this.usersByUserID.Contains(pId))
            {
                return (RoomUser)this.usersByUserID[pId];
            }
            return null;
        }

        internal List<RoomUser> GetUsersInCampingTent()
        {
            return GetRoomUsers().Where(x => x.OnCampingTent == true).ToList();
        }

        internal HashSet<RoomUser> GetRoomUsers()
        {
            return new HashSet<RoomUser>(this.UserList.Values.Where(x => x.IsBot == false));
        }

        internal List<RoomUser> GetRoomUserByRank(int minRank)
        {
            List<RoomUser> list = new List<RoomUser>();
            foreach (RoomUser current in this.UserList.Values)
            {
                if (!current.IsBot && current.GetClient() != null && current.GetClient().GetHabbo() != null && (ulong)current.GetClient().GetHabbo().Rank > (ulong)((long)minRank))
                {
                    list.Add(current);
                }
            }
            return list;
        }
        internal RoomUser GetRoomUserByHabbo(string pName)
        {
            if (this.usersByUsername.Contains(pName.ToLower()))
            {
                return (RoomUser)this.usersByUsername[pName.ToLower()];
            }
            return null;
        }
        internal void SavePets(IQueryAdapter dbClient)
        {
            try
            {
                if (this.GetPets().Count > 0)
                {
                    this.AppendPetsUpdateString(dbClient);
                }
            }
            catch (Exception ex)
            {
                Logging.LogCriticalException(string.Concat(new object[]
				{
					"Error during saving furniture for room ",
					this.room.RoomId,
					". Stack: ",
					ex.ToString()
				}));
            }
        }
        internal void AppendPetsUpdateString(IQueryAdapter dbClient)
        {
            QueryChunk queryChunk = new QueryChunk("INSERT INTO bots (id,user_id,room_id,name,x,y,z) VALUES ");
            QueryChunk queryChunk2 = new QueryChunk("INSERT INTO bots_petdata (type,race,color,experience,energy,createstamp,nutrition,respect) VALUES ");
            QueryChunk queryChunk3 = new QueryChunk();
            List<uint> list = new List<uint>();
            foreach (Pet current in this.GetPets())
            {
                if (!list.Contains(current.PetId))
                {
                    list.Add(current.PetId);
                    if (current.DBState == DatabaseUpdateState.NeedsInsert)
                    {
                        queryChunk.AddParameter(current.PetId + "name", current.Name);
                        queryChunk2.AddParameter(current.PetId + "race", current.Race);
                        queryChunk2.AddParameter(current.PetId + "color", current.Color);
                        queryChunk.AddQuery(string.Concat(new object[]
						{
							"(",
							current.PetId,
							",",
							current.OwnerId,
							",",
							current.RoomId,
							",@",
							current.PetId,
							"name,", current.X, ",", current.Y, ",", current.Z, ")"
						}));
                        queryChunk2.AddQuery(string.Concat(new object[]
						{
							"(",
							current.Type,
							",@",
							current.PetId,
							"race,@",
							current.PetId,
							"color,0,100,'",
							current.CreationStamp,
							"',0,0)"
						}));
                    }
                    else
                    {
                        if (current.DBState == DatabaseUpdateState.NeedsUpdate)
                        {
                            queryChunk3.AddParameter(current.PetId + "name", current.Name);
                            queryChunk3.AddParameter(current.PetId + "race", current.Race);
                            queryChunk3.AddParameter(current.PetId + "color", current.Color);
                            queryChunk3.AddQuery(string.Concat(new object[]
							{
								"UPDATE bots SET room_id = ",
								current.RoomId,
								", name = @",
								current.PetId,
								"name, x = ",
								current.X,
								", Y = ",
								current.Y,
								", Z = ",
								current.Z,
								" WHERE id = ",
								current.PetId
							}));
                            queryChunk3.AddQuery(string.Concat(new object[]
							{
								"UPDATE bots_petdata SET race = @",
								current.PetId,
								"race, color = @",
								current.PetId,
								"color, type = ",
								current.Type,
								", experience = ",
								current.Experience,
								", energy = ",
								current.Energy,
								", nutrition = ",
								current.Nutrition,
								", respect = ",
								current.Respect,
								", createstamp = '",
								current.CreationStamp,
								"' WHERE id = ",
								current.PetId
							}));
                        }
                    }
                    current.DBState = DatabaseUpdateState.Updated;
                }
            }
            queryChunk.Execute(dbClient);
            queryChunk3.Execute(dbClient);
            queryChunk.Dispose();
            queryChunk3.Dispose();
            queryChunk = null;
            queryChunk3 = null;
        }
        internal List<Pet> GetPets()
        {
            List<KeyValuePair<int, RoomUser>> list = this.UserList.ToList();
            List<Pet> list2 = new List<Pet>();
            foreach (KeyValuePair<int, RoomUser> current in list)
            {
                RoomUser value = current.Value;
                if (value.IsPet)
                {
                    list2.Add(value.PetData);
                }
            }
            return list2;
        }
        internal ServerMessage SerializeStatusUpdates(bool All)
        {
            List<RoomUser> list = new List<RoomUser>();
            foreach (RoomUser current in this.UserList.Values)
            {
                if (!All)
                {
                    if (!current.UpdateNeeded)
                    {
                        continue;
                    }
                    current.UpdateNeeded = false;
                }
                list.Add(current);
            }
            if (list.Count == 0)
            {
                return null;
            }
            ServerMessage serverMessage = new ServerMessage(Outgoing.UpdateUserStatusMessageComposer);
            serverMessage.AppendInt32(list.Count);
            foreach (RoomUser current2 in list)
            {
                current2.SerializeStatus(serverMessage);
            }
            return serverMessage;
        }
        internal void UpdateUserStatusses()
        {
            onCycleDoneDelegate function = new onCycleDoneDelegate(this.onUserUpdateStatus);
            this.UserList.QueueDelegate(function);
        }
        private void onUserUpdateStatus()
        {
            foreach (RoomUser current in this.UserList.Values)
            {
                this.UpdateUserStatus(current, false);
            }
        }
        internal void backupCounters(ref int primaryCounter, ref int secondaryCounter)
        {
            primaryCounter = this.primaryPrivateUserID;
            secondaryCounter = this.secondaryPrivateUserID;
        }
        private bool isValid(RoomUser user)
        {
            return user.IsBot || (user.GetClient() != null && user.GetClient().GetHabbo() != null && user.GetClient().GetHabbo().CurrentRoomId == this.room.RoomId);
        }
        internal void UpdateUserStatus(RoomUser User, bool cyclegameitems)
        {
            try
            {
                if (User != null)
                {
                    bool isBot = User.IsBot;
                    if (isBot)
                    {
                        cyclegameitems = false;
                    }
                    if (checked(User.SignTime - CyberEnvironment.GetUnixTimestamp()) < 0 && User.Statusses.ContainsKey("sign"))
                    {
                        User.Statusses.Remove("sign");
                        User.UpdateNeeded = true;
                    }
                    if ((User.Statusses.ContainsKey("lay") && !User.IsLyingDown) || (User.Statusses.ContainsKey("sit") && !User.IsSitting))
                    {
                        User.Statusses.Remove("lay");
                        User.Statusses.Remove("sit");
                        User.UpdateNeeded = true;
                    }
                    else
                    {
                        if (User.IsLyingDown || User.IsSitting)
                        {
                            return;
                        }
                    }
                    CoordItemSearch coordItemSearch = new CoordItemSearch(this.room.GetGameMap().CoordinatedItems);
                    List<RoomItem> allRoomItemForSquare = coordItemSearch.GetAllRoomItemForSquare(User.X, User.Y);
                    double num;
                    if (User.RidingHorse && !User.IsPet)
                    {
                        num = this.room.GetGameMap().SqAbsoluteHeight(User.X, User.Y, allRoomItemForSquare) + 1.0;
                    }
                    else
                    {
                        num = this.room.GetGameMap().SqAbsoluteHeight(User.X, User.Y, allRoomItemForSquare);
                    }
                    if (num != User.Z)
                    {
                        User.Z = num;
                        User.UpdateNeeded = true;
                    }
                    DynamicRoomModel model = this.room.GetGameMap().Model;
                    if (model.SqState[User.X, User.Y] == SquareState.SEAT || User.IsSitting || User.IsLyingDown)
                    {
                        if (User.IsSitting)
                        {
                            if (!User.Statusses.ContainsKey("sit"))
                            {
                                User.Statusses.Add("sit", Convert.ToString((double)model.SqFloorHeight[User.X, User.Y] + 0.55));
                            }
                            User.Z = (double)model.SqFloorHeight[User.X, User.Y];
                            User.UpdateNeeded = true;
                        }
                        else
                        {
                            if (User.IsLyingDown)
                            {
                                if (!User.Statusses.ContainsKey("lay"))
                                {
                                    User.Statusses.Add("lay", Convert.ToString((double)model.SqFloorHeight[User.X, User.Y] + 0.55));
                                }
                                User.Z = (double)model.SqFloorHeight[User.X, User.Y];
                                User.UpdateNeeded = true;
                            }
                            else
                            {
                                if (!User.Statusses.ContainsKey("sit"))
                                {
                                    User.Statusses.Add("sit", "1.0");
                                }
                                User.Z = (double)model.SqFloorHeight[User.X, User.Y];
                                User.RotHead = (int)model.SqSeatRot[User.X, User.Y];
                                User.RotBody = (int)model.SqSeatRot[User.X, User.Y];
                                User.UpdateNeeded = true;
                            }
                        }
                    }
                    if (allRoomItemForSquare.Count == 0)
                    {
                        User.LastItem = 0;
                    }
                    using (List<RoomItem>.Enumerator enumerator = allRoomItemForSquare.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            RoomItem Item = enumerator.Current;
                            if (cyclegameitems)
                            {
                                Item.UserWalksOnFurni(User);
                                CyberEnvironment.GetGame().GetQuestManager().ProgressUserQuest(User.GetClient(), QuestType.STAND_ON, Item.GetBaseItem().ItemId);
                            }
                            if (Item.GetBaseItem().IsSeat)
                            {
                                if (!User.Statusses.ContainsKey("sit"))
                                {
                                    if (Item.GetBaseItem().StackMultipler && !string.IsNullOrWhiteSpace(Item.ExtraData))
                                    {
                                        if (Item.ExtraData != "0")
                                        {
                                            int num2 = Convert.ToInt32(Item.ExtraData);
                                            User.Statusses.Add("sit", Item.GetBaseItem().ToggleHeight[num2].ToString());
                                        }
                                        else
                                        {
                                            User.Statusses.Add("sit", TextHandling.GetString(Item.GetBaseItem().Height));
                                        }
                                    }
                                    else
                                    {
                                        User.Statusses.Add("sit", TextHandling.GetString(Item.GetBaseItem().Height));
                                    }
                                }
                                User.Z = Item.GetZ;
                                User.RotHead = Item.Rot;
                                User.RotBody = Item.Rot;
                                User.UpdateNeeded = true;
                            }
                            InteractionType interactionType = Item.GetBaseItem().InteractionType;
                            checked
                            {
                                if (interactionType <= InteractionType.banzaigategreen)
                                {
                                    if (interactionType != InteractionType.bed)
                                    {
                                        if (interactionType != InteractionType.fbgate)
                                        {
                                            switch (interactionType)
                                            {
                                                case InteractionType.banzaigateblue:
                                                case InteractionType.banzaigatered:
                                                case InteractionType.banzaigateyellow:
                                                case InteractionType.banzaigategreen:
                                                    {
                                                        if (!cyclegameitems)
                                                        {
                                                            continue;
                                                        }
                                                        int num3 = (int)(Item.team + 32);
                                                        TeamManager teamManagerForBanzai = User.GetClient().GetHabbo().CurrentRoom.GetTeamManagerForBanzai();
                                                        AvatarEffectsInventoryComponent avatarEffectsInventoryComponent = User.GetClient().GetHabbo().GetAvatarEffectsInventoryComponent();
                                                        if (User.team == Team.none)
                                                        {
                                                            if (!teamManagerForBanzai.CanEnterOnTeam(Item.team))
                                                            {
                                                                continue;
                                                            }
                                                            if (User.team != Team.none)
                                                            {
                                                                teamManagerForBanzai.OnUserLeave(User);
                                                            }
                                                            User.team = Item.team;
                                                            teamManagerForBanzai.AddUser(User);
                                                            if (avatarEffectsInventoryComponent.CurrentEffect != num3)
                                                            {
                                                                avatarEffectsInventoryComponent.ActivateCustomEffect(num3);
                                                                continue;
                                                            }
                                                            continue;
                                                        }
                                                        else
                                                        {
                                                            if (User.team != Team.none && User.team != Item.team)
                                                            {
                                                                teamManagerForBanzai.OnUserLeave(User);
                                                                User.team = Team.none;
                                                                avatarEffectsInventoryComponent.ActivateCustomEffect(0);
                                                                continue;
                                                            }
                                                            teamManagerForBanzai.OnUserLeave(User);
                                                            if (avatarEffectsInventoryComponent.CurrentEffect == num3)
                                                            {
                                                                avatarEffectsInventoryComponent.ActivateCustomEffect(0);
                                                            }
                                                            User.team = Team.none;
                                                            continue;
                                                        }
                                                    }
                                                default:
                                                    continue;
                                            }
                                        }
                                        else
                                        {
                                            if (User.IsBot)
                                            {
                                                continue;
                                            }

                                            string look = Item.ExtraData.Split(';')[0];
                                            if (User.GetClient().GetHabbo().Gender.ToUpper() == "F")
                                            {
                                                look = Item.ExtraData.Split(';')[1];
                                            }
                                            look = look.Replace("hd-99999-99999", User.GetClient().GetHabbo().HeadPart);
                                            User.GetClient().GetHabbo().Look = look;

                                            ServerMessage serverMessage = new ServerMessage();
                                            serverMessage.Init(Outgoing.UpdateUserDataMessageComposer);
                                            serverMessage.AppendInt32(-1);
                                            serverMessage.AppendString(User.GetClient().GetHabbo().Look);
                                            serverMessage.AppendString(User.GetClient().GetHabbo().Gender.ToLower());
                                            serverMessage.AppendString(User.GetClient().GetHabbo().Motto);
                                            serverMessage.AppendInt32(User.GetClient().GetHabbo().AchievementPoints);
                                            User.GetClient().SendMessage(serverMessage);
                                            ServerMessage serverMessage2 = new ServerMessage();
                                            serverMessage2.Init(Outgoing.UpdateUserDataMessageComposer);
                                            serverMessage2.AppendInt32(User.VirtualId);
                                            serverMessage2.AppendString(User.GetClient().GetHabbo().Look);
                                            serverMessage2.AppendString(User.GetClient().GetHabbo().Gender.ToLower());
                                            serverMessage2.AppendString(User.GetClient().GetHabbo().Motto);
                                            serverMessage2.AppendInt32(User.GetClient().GetHabbo().AchievementPoints);
                                            this.room.SendMessage(serverMessage2);

                                            ServerMessage serverMessage3 = new ServerMessage();
                                            serverMessage3.Init(Outgoing.UpdateAvatarAspectMessageComposer);
                                            serverMessage3.AppendString(User.GetClient().GetHabbo().Look);
                                            serverMessage3.AppendString(User.GetClient().GetHabbo().Gender.ToUpper());
                                            User.GetClient().SendMessage(serverMessage3);

                                            continue;
                                        }
                                    }
                                }
                                else
                                {
                                    if (interactionType <= InteractionType.freezebluegate)
                                    {
                                        if (interactionType == InteractionType.banzaitele)
                                        {
                                            this.room.GetGameItemHandler().onTeleportRoomUserEnter(User, Item);
                                            continue;
                                        }
                                        switch (interactionType)
                                        {
                                            case InteractionType.freezeyellowgate:
                                            case InteractionType.freezeredgate:
                                            case InteractionType.freezegreengate:
                                            case InteractionType.freezebluegate:
                                                if (cyclegameitems)
                                                {
                                                    int num4 = (int)(Item.team + 39);
                                                    TeamManager teamManagerForFreeze = User.GetClient().GetHabbo().CurrentRoom.GetTeamManagerForFreeze();
                                                    AvatarEffectsInventoryComponent avatarEffectsInventoryComponent2 = User.GetClient().GetHabbo().GetAvatarEffectsInventoryComponent();
                                                    if (User.team != Item.team)
                                                    {
                                                        if (teamManagerForFreeze.CanEnterOnTeam(Item.team))
                                                        {
                                                            if (User.team != Team.none)
                                                            {
                                                                teamManagerForFreeze.OnUserLeave(User);
                                                            }
                                                            User.team = Item.team;
                                                            teamManagerForFreeze.AddUser(User);
                                                            if (avatarEffectsInventoryComponent2.CurrentEffect != num4)
                                                            {
                                                                avatarEffectsInventoryComponent2.ActivateCustomEffect(num4);
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        teamManagerForFreeze.OnUserLeave(User);
                                                        if (avatarEffectsInventoryComponent2.CurrentEffect == num4)
                                                        {
                                                            avatarEffectsInventoryComponent2.ActivateCustomEffect(0);
                                                        }
                                                        User.team = Team.none;
                                                    }
                                                    ServerMessage serverMessage3 = new ServerMessage(Outgoing.UserIsPlayingFreezeMessageComposer);
                                                    serverMessage3.AppendBoolean(User.team != Team.none);
                                                    User.GetClient().SendMessage(serverMessage3);
                                                    continue;
                                                }
                                                continue;
                                            default:
                                                continue;
                                        }
                                    }
                                    else
                                    {
                                        if (interactionType != InteractionType.jump)
                                        {
                                            switch (interactionType)
                                            {
                                                case InteractionType.pinata:
                                                    {
                                                        if (!User.IsWalking || Item.ExtraData.Length <= 0)
                                                        {
                                                            continue;
                                                        }
                                                        int num5 = int.Parse(Item.ExtraData);
                                                        if (num5 >= 100 || User.CurrentEffect != 158)
                                                        {
                                                            continue;
                                                        }
                                                        int num6 = num5 + 1;
                                                        Item.ExtraData = num6.ToString();

                                                        Item.UpdateState();
                                                        CyberEnvironment.GetGame().GetAchievementManager().ProgressUserAchievement(User.GetClient(), "ACH_PinataWhacker", 1, false);
                                                        if (num6 == 100)
                                                        {
                                                            CyberEnvironment.GetGame().GetPinataHandler().DeliverRandomPinataItem(User, this.room, Item);
                                                            CyberEnvironment.GetGame().GetAchievementManager().ProgressUserAchievement(User.GetClient(), "ACH_PinataBreaker", 1, false);
                                                            continue;
                                                        }
                                                        continue;
                                                    }
                                                case InteractionType.tilestackmagic:
                                                case InteractionType.poster:
                                                    continue;

                                                case InteractionType.tent:
                                                case InteractionType.bedtent:
                                                    if (User.LastItem == Item.Id)
                                                        continue;

                                                    if (!User.IsBot && !User.OnCampingTent)
                                                    {
                                                        ServerMessage serverMessage = new ServerMessage();
                                                        serverMessage.Init(Outgoing.UpdateFloorItemExtraDataMessageComposer);
                                                        serverMessage.AppendString(Item.Id.ToString());
                                                        serverMessage.AppendInt32(0);
                                                        serverMessage.AppendString("1");
                                                        User.GetClient().SendMessage(serverMessage);
                                                        User.OnCampingTent = true;
                                                        User.LastItem = Item.Id;
                                                    }
                                                    continue;
                                                case InteractionType.runwaysage:
                                                    {
                                                        int num7 = new Random().Next(1, 4);
                                                        Item.ExtraData = num7.ToString();
                                                        Item.UpdateState();
                                                        continue;
                                                    }
                                                case InteractionType.shower:
                                                    Item.ExtraData = "1";
                                                    Item.UpdateState();
                                                    continue;
                                                default:
                                                    continue;
                                            }
                                        }
                                        else
                                        {
                                            if ((User.Y == Item.GetY || User.Y == Item.GetY - 1) && User.X == Item.GetX + 1)
                                            {
                                                continue;
                                            }
                                            continue;
                                        }
                                    }
                                }
                                if (Item.GetBaseItem().InteractionType == InteractionType.bedtent)
                                {
                                    User.OnCampingTent = true;
                                }
                                if (!User.Statusses.ContainsKey("lay"))
                                {
                                    User.Statusses.Add("lay", TextHandling.GetString(Item.GetBaseItem().Height) + " null");
                                }
                                User.Z = Item.GetZ;
                            }
                            User.RotHead = Item.Rot;
                            User.RotBody = Item.Rot;
                            User.UpdateNeeded = true;
                        }
                    }
                    if (User.IsSitting && User.TeleportEnabled)
                    {
                        User.Z -= 0.35;
                        User.UpdateNeeded = true;
                    }
                    if (cyclegameitems)
                    {
                        if (this.room.GotSoccer())
                        {
                            this.room.GetSoccer().OnUserWalk(User);
                        }
                        if (this.room.GotBanzai())
                        {
                            this.room.GetBanzai().OnUserWalk(User);
                        }
                        this.room.GetFreeze().OnUserWalk(User);
                    }
                }
            }
            catch
            {
            }
        }
        internal void TurnHeads(int X, int Y, uint SenderId)
        {
            foreach (RoomUser current in this.UserList.Values)
            {
                if (current.HabboId != SenderId && !current.RidingHorse && !current.IsPet)
                {
                    current.SetRot(PathFinder.CalculateRotation(current.X, current.Y, X, Y), true);
                }
            }
        }
        internal void OnCycle(ref int idleCount)
        {
            this.ToRemove.Clear();
            int count = 0;
            foreach (RoomUser roomUser in this.UserList.Values)
            {
                if (!this.isValid(roomUser))
                {
                    if (roomUser.GetClient() != null)
                        this.RemoveUserFromRoom(roomUser.GetClient(), false, false);
                    else
                        this.RemoveRoomUser(roomUser);
                }
                bool updated = false;
                roomUser.IdleTime++;
                if (!roomUser.IsBot && !roomUser.IsAsleep && roomUser.IdleTime >= 600)
                {
                    roomUser.IsAsleep = true;
                    ServerMessage sleepMsg = new ServerMessage(Outgoing.RoomUserIdleMessageComposer);
                    sleepMsg.AppendInt32(roomUser.VirtualId);
                    sleepMsg.AppendBoolean(true);
                    this.room.SendMessage(sleepMsg);
                }
                if (roomUser.NeedsAutokick && !this.ToRemove.Contains(roomUser))
                {
                    this.ToRemove.Add(roomUser);
                    continue;
                }
                else
                {
                    if (roomUser.CarryItemID > 0)
                    {
                        roomUser.CarryTimer--;
                        if (roomUser.CarryTimer <= 0)
                            roomUser.CarryItem(0);
                    }
                    if (this.room.GotFreeze())
                    {
                        this.room.GetFreeze().CycleUser(roomUser);
                    }

                    if (roomUser.IsPet)
                    {
                        if (!roomUser.IsWalking && roomUser.Statusses.ContainsKey("mv"))
                        {
                            roomUser.ClearMovement(true);
                        }
                    }

                    bool invalidStep = false;
                    if (roomUser.SetStep)
                    {
                        if (this.room.GetGameMap().CanWalk(roomUser.SetX, roomUser.SetY, roomUser.AllowOverride) || roomUser.RidingHorse)
                        {
                            Gamemap gameMap = this.room.GetGameMap();


                            Point coordinate = roomUser.Coordinate;
                            int x = coordinate.X;
                            coordinate = roomUser.Coordinate;
                            int y = coordinate.Y;
                            Point oldCoord = new Point(x, y);
                            Point newCoord = new Point(roomUser.SetX, roomUser.SetY);
                            RoomUser user = roomUser;
                            gameMap.UpdateUserMovement(oldCoord, newCoord, user);
                            List<RoomItem> coordinatedItems = this.room.GetGameMap().GetCoordinatedItems(new Point(roomUser.X, roomUser.Y));
                            roomUser.X = roomUser.SetX;
                            roomUser.Y = roomUser.SetY;
                            roomUser.Z = roomUser.SetZ;
                            this.ToSet.Remove(new Point(roomUser.SetX, roomUser.SetY));

                            if (this.room.GotSoccer())
                            {
                                this.room.GetSoccer().OnUserWalk(roomUser);
                            }

                            lock (coordinatedItems)
                            {
                                foreach (RoomItem itemE in coordinatedItems)
                                {
                                    itemE.UserWalksOffFurni(roomUser);

                                    switch (itemE.GetBaseItem().InteractionType)
                                    {
                                        case InteractionType.tent:
                                        case InteractionType.bedtent:
                                            if (!roomUser.IsBot && roomUser.OnCampingTent)
                                            {
                                                ServerMessage serverMessage = new ServerMessage();
                                                serverMessage.Init(Outgoing.UpdateFloorItemExtraDataMessageComposer);
                                                serverMessage.AppendString(itemE.Id.ToString());
                                                serverMessage.AppendInt32(0);
                                                serverMessage.AppendString("0");
                                                roomUser.GetClient().SendMessage(serverMessage);
                                                roomUser.OnCampingTent = false;
                                            }
                                            break;

                                        case InteractionType.runwaysage:
                                        case InteractionType.shower:
                                            itemE.ExtraData = "0";
                                            itemE.UpdateState();
                                            break;
                                    }
                                }
                            }

                            if (roomUser.X == this.room.GetGameMap().Model.DoorX && roomUser.Y == this.room.GetGameMap().Model.DoorY && !this.ToRemove.Contains(roomUser) && !roomUser.IsBot)
                            {
                                this.ToRemove.Add(roomUser);
                                continue;
                            }
                            else
                                this.UpdateUserStatus(roomUser, true);
                        }
                        else
                        {
                            invalidStep = true;
                        }
                        roomUser.SetStep = false;
                    }

                    if (roomUser.PathRecalcNeeded)
                    {
                        roomUser.Path.Clear();
                        roomUser.Path = PathFinder.FindPath(roomUser, this.room.GetGameMap().DiagonalEnabled, this.room.GetGameMap(), new Vector2D(roomUser.X, roomUser.Y), new Vector2D(roomUser.GoalX, roomUser.GoalY));
                        if (roomUser.Path.Count > 1)
                        {
                            roomUser.PathStep = 1;
                            roomUser.IsWalking = true;
                            roomUser.PathRecalcNeeded = false;
                        }
                        else
                        {
                            roomUser.PathRecalcNeeded = false;
                            roomUser.Path.Clear();
                        }
                    }
                    if (roomUser.IsWalking && !roomUser.Freezed)
                    {
                        if (!roomUser.HasPathBlocked && (invalidStep || roomUser.PathStep >= roomUser.Path.Count || roomUser.GoalX == roomUser.X && roomUser.GoalY == roomUser.Y))
                        {
                            roomUser.IsWalking = false;
                            roomUser.RemoveStatus("mv");
                            this.UpdateUserStatus(roomUser, false);

                            if (roomUser.RidingHorse && !roomUser.IsPet && !roomUser.IsBot)
                            {
                                RoomUser roomUserByVirtualId = this.GetRoomUserByVirtualId(Convert.ToInt32(roomUser.HorseID));
                                roomUserByVirtualId.IsWalking = false;
                                roomUserByVirtualId.RemoveStatus("mv");
                                ServerMessage Message = new ServerMessage(Outgoing.UpdateUserStatusMessageComposer);
                                Message.AppendInt32(1);
                                roomUserByVirtualId.SerializeStatus(Message, "");
                                roomUser.GetClient().GetHabbo().CurrentRoom.SendMessage(Message);
                            }
                        }
                        else
                        {
                            int index1 = checked(roomUser.Path.Count - roomUser.PathStep - 1);
                            Vector2D vector2D = roomUser.Path[index1];
                            checked { ++roomUser.PathStep; }

                            if (roomUser.FastWalking && roomUser.PathStep < roomUser.Path.Count)
                            {
                                int index2 = checked(roomUser.Path.Count - roomUser.PathStep - 1);
                                vector2D = roomUser.Path[index2];
                                checked { ++roomUser.PathStep; }
                            }
                            int x = vector2D.X;
                            int y = vector2D.Y;
                            roomUser.RemoveStatus("mv");

                            if (this.room.GetGameMap().IsValidStep2(roomUser, new Point(roomUser.X, roomUser.Y), new Point(x, y), roomUser.GoalX == x && roomUser.GoalY == y, roomUser.AllowOverride) && this.room.GetGameMap().AntiChoques(x, y, roomUser))
                            {
                                double k = this.room.GetGameMap().SqAbsoluteHeight(x, y);
                                roomUser.SetX = x;
                                roomUser.SetY = y;
                                roomUser.SetZ = k;

                                int num = PathFinder.CalculateRotation(roomUser.X, roomUser.Y, x, y);
                                roomUser.RotBody = num;
                                roomUser.RotHead = num;
                                roomUser.SetStep = true;

                                if (!roomUser.IsBot)
                                {
                                    if (roomUser.IsSitting)
                                    {
                                        roomUser.Statusses.Remove("sit");
                                        roomUser.Z += 0.35;
                                        roomUser.IsSitting = false;
                                        roomUser.UpdateNeeded = true;
                                    }
                                    else if (roomUser.IsLyingDown)
                                    {
                                        roomUser.Statusses.Remove("sit");
                                        roomUser.Z += 0.35;
                                        roomUser.IsLyingDown = false;
                                        roomUser.UpdateNeeded = true;
                                    }
                                }
                                if (!roomUser.IsBot)
                                {
                                    roomUser.Statusses.Remove("lay");
                                    roomUser.Statusses.Remove("sit");
                                }
                                string Status1 = "";
                                string Status2 = "";
                                if (!roomUser.IsBot && !roomUser.IsPet && roomUser.GetClient() != null)
                                {
                                    if (roomUser.GetClient().GetHabbo().IsTeleporting)
                                    {
                                        roomUser.TeleportEnabled = false;
                                        roomUser.GetClient().GetHabbo().IsTeleporting = false;
                                        roomUser.GetClient().GetHabbo().TeleporterId = 0U;
                                    }
                                    else if (roomUser.GetClient().GetHabbo().IsHopping)
                                    {
                                        roomUser.GetClient().GetHabbo().IsHopping = false;
                                        roomUser.GetClient().GetHabbo().HopperId = 0U;
                                    }
                                }


                                if (!ToSet.ContainsKey(new Point(roomUser.SetX, roomUser.SetY)))
                                {
                                    this.ToSet.Add(new Point(roomUser.SetX, roomUser.SetY), roomUser);
                                }

                                if (!roomUser.IsBot && roomUser.RidingHorse && !roomUser.IsPet)
                                {
                                    Status1 = "mv " + x + "," + y + "," + TextHandling.GetString(k + 1.0);
                                    roomUser.AddStatus("mv", x + "," + y + "," + TextHandling.GetString(k + 1.0));
                                    Status2 = "mv " + x + "," + y + "," + TextHandling.GetString(k);
                                }
                                else
                                    roomUser.AddStatus("mv", x + "," + y + "," + TextHandling.GetString(k));

                                this.UpdateUserEffect(roomUser, roomUser.SetX, roomUser.SetY);
                                updated = true;
                                this.room.GetGameMap().GameMap[roomUser.X, roomUser.Y] = roomUser.SqState;
                                roomUser.SqState = this.room.GetGameMap().GameMap[roomUser.SetX, roomUser.SetY];


                                if (!roomUser.IsBot)
                                {
                                    if (roomUser.IsSitting)
                                        roomUser.IsSitting = false;
                                    if (roomUser.IsLyingDown)
                                        roomUser.IsLyingDown = false;
                                }
                                if (roomUser.RidingHorse && !roomUser.IsPet && !roomUser.IsBot)
                                {
                                    RoomUser roomUserByVirtualId = this.GetRoomUserByVirtualId(Convert.ToInt32(roomUser.HorseID));
                                    roomUserByVirtualId.RotBody = num;
                                    roomUserByVirtualId.RotHead = num;
                                    roomUserByVirtualId.SetStep = true;
                                    roomUserByVirtualId.SetX = x;
                                    roomUserByVirtualId.SetY = y;
                                    roomUserByVirtualId.SetZ = k;
                                    this.UpdateUserEffect(roomUserByVirtualId, roomUserByVirtualId.SetX, roomUserByVirtualId.SetY);
                                    updated = true;
                                    ServerMessage Message = new ServerMessage(Outgoing.UpdateUserStatusMessageComposer);
                                    Message.AppendInt32(2);
                                    roomUser.SerializeStatus(Message, Status1);
                                    roomUserByVirtualId.SerializeStatus(Message, Status2);
                                    roomUser.GetClient().GetHabbo().CurrentRoom.SendMessage(Message);
                                    this.UpdateUserEffect(roomUser, roomUser.SetX, roomUser.SetY);
                                    roomUserByVirtualId.UpdateNeeded = true;
                                }

                            }
                            else
                            {
                                roomUser.PathRecalcNeeded = true;
                            }
                        }
                        if (!roomUser.RidingHorse)
                            roomUser.UpdateNeeded = true;
                    }
                    else if (roomUser.Statusses.ContainsKey("mv"))
                    {
                        RoomUser roomUserByVirtualId = this.GetRoomUserByVirtualId(Convert.ToInt32(roomUser.HorseID));
                        roomUser.RemoveStatus("mv");
                        roomUser.UpdateNeeded = true;
                        if (roomUser.RidingHorse)
                        {
                            roomUserByVirtualId.RemoveStatus("mv");
                            roomUserByVirtualId.UpdateNeeded = true;
                        }
                    }
                    if (roomUser.RidingHorse)
                    {
                        roomUser.ApplyEffect(77);
                    }
                    if (roomUser.IsBot)
                    {
                        roomUser.BotAI.OnTimerTick();
                    }
                    else
                    {
                        count++;
                    }
                    if (!updated)
                        this.UpdateUserEffect(roomUser, roomUser.X, roomUser.Y);
                }
            }
            if (count == 0) idleCount++;

            lock (ToRemove)
            {
                foreach (RoomUser user in this.ToRemove)
                {
                    GameClient clientByUserId = CyberEnvironment.GetGame().GetClientManager().GetClientByUserID(user.HabboId);
                    if (clientByUserId != null)
                    {
                        this.RemoveUserFromRoom(clientByUserId, true, false);
                        clientByUserId.CurrentRoomUserID = -1;
                    }
                    else
                        this.RemoveRoomUser(user);
                }
            }
            if (this.userCount == count)
                return;
            this.UpdateUserCount(count);
        }


        internal void Destroy()
        {
            this.room = null;
            this.usersByUsername.Clear();
            this.usersByUsername = null;
            this.usersByUserID.Clear();
            this.usersByUserID = null;
            this.OnUserEnter = null;
            this.pets.Clear();
            this.bots.Clear();
            this.pets = null;
            this.bots = null;
            this.userlist.Destroy();
            this.userlist = null;
        }
    }
}
