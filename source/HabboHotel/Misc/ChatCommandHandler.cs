using Database_Manager.Database.Session_Details.Interfaces;
using Cyber;
using Cyber.HabboHotel.PathFinding;
using Cyber.Core;
using Cyber.HabboHotel.GameClients;
using Cyber.HabboHotel.Groups;
using Cyber.HabboHotel.Items;
using Cyber.HabboHotel.Pets;
using Cyber.HabboHotel.Rooms;
using Cyber.HabboHotel.Rooms.Games;
using Cyber.HabboHotel.Rooms.RoomInvokedItems;
using Cyber.HabboHotel.Users;
using Cyber.Messages;
using Cyber.Messages.Headers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;

namespace Cyber.HabboHotel.Misc
{

    internal class ChatCommandHandler
    {
        public DataTable Commands;
        public GameClient Session;

        public ChatCommandHandler(GameClient Session)
        {
            this.Session = Session;
            this.LoadCommandsList();
        }

        public void LoadCommandsList()
        {
            using (IQueryAdapter dbClient = CyberEnvironment.GetDatabaseManager().getQueryReactor())
            {
                dbClient.setQuery("SELECT command,params,description FROM fuse_cmds WHERE rank <= " + this.Session.GetHabbo().Rank + " ORDER BY command ASC");
                this.Commands = dbClient.getTable();
            }
        }

        public static string MergeParams(string[] Params, int Start)
        {
            StringBuilder Builder = new StringBuilder();
            for (int i = 0; i < Params.Length; i++)
            {
                if (i >= Start)
                {
                    if (i > Start)
                    {
                        Builder.Append(" ");
                    }
                    Builder.Append(Params[i]);
                }
            }
            return Builder.ToString();
        }

        public bool Parse(string Input)
        {
            if (Input.StartsWith(":"))
            {
                Input = Input.Substring(1);
                string[] Params = Input.Split(new char[] { ' ' });
                switch (Params[0].ToLower())
                {
                    case "commands":
                    case "comandos":
                        if (this.Session.GetHabbo().GotCommand("commands"))
                        {
                            StringBuilder builder = new StringBuilder();
                            builder.Append("Your commands:\n");
                            foreach (DataRow row in this.Commands.Rows)
                            {
                                builder.Append(":" + Convert.ToString(row[0]) + " " + Convert.ToString(row[1]) + "\n - " + Convert.ToString(row[2]) + "\n");
                            }
                            this.Session.SendNotifWithScroll(builder.ToString());
                        }
                        return true;
                    case "about":
                    case "info":
                        {
                            ServerMessage Message = new ServerMessage(Outgoing.SuperNotificationMessageComposer);

                            Message.AppendString("mercury22");
                            Message.AppendInt32(4);
                            Message.AppendString("title");
                            Message.AppendString("About the server");
                            Message.AppendString("message");

                            StringBuilder info = new StringBuilder();
                            info.Append("This hotel uses an extremely-modified PlusEMU.");
                            info.Append("<br /><br />");
                            info.Append("<font color=\"#002C59\" size=\"16\"><b>Cyber Emulator" + CyberEnvironment.PrettyBuild + "</b></font><br />Developed by <b>Kessiler Rodrigues</b><br />");
                            info.Append("<br />");
                            info.Append("<b>Build:</b> " + CyberEnvironment.PrettyRelease);
                            info.Append("<br />");

                            int userCount = CyberEnvironment.GetGame().GetClientManager().clients.Count;
                            int roomsCount = CyberEnvironment.GetGame().GetRoomManager().loadedRooms.Count;
                            info.Append("<b>Users:</b> " + userCount + " in " + roomsCount + ((roomsCount == 1) ? " Room" : " Rooms") + ".<br /><br /><br />");
                            Message.AppendString(info.ToString());
                            Message.AppendString("linkUrl");
                            Message.AppendString("event:");
                            Message.AppendString("linkTitle");
                            Message.AppendString("ok");
                            this.Session.SendMessage(Message);
                        }
                        return true;

                    case "sit":
                        {
                            Room currentRoom = this.Session.GetHabbo().CurrentRoom;
                            RoomUser roomUserByHabbo = currentRoom.GetRoomUserManager().GetRoomUserByHabbo(this.Session.GetHabbo().Id);
                            if ((currentRoom != null) && (roomUserByHabbo != null))
                            {
                                if (((!roomUserByHabbo.IsLyingDown && !roomUserByHabbo.IsLyingDown) && (!roomUserByHabbo.RidingHorse && !roomUserByHabbo.IsWalking)) && !roomUserByHabbo.Statusses.ContainsKey("sit"))
                                {
                                    if ((roomUserByHabbo.RotBody % 2) != 0)
                                    {
                                        roomUserByHabbo.RotBody--;
                                    }
                                    roomUserByHabbo.Statusses.Add("sit", "0.55");
                                    roomUserByHabbo.IsSitting = true;
                                    roomUserByHabbo.UpdateNeeded = true;
                                }
                                return true;
                            }
                            return true;
                        }

                    case "lay":
                        {
                            Room currentRoom = this.Session.GetHabbo().CurrentRoom;
                            RoomUser roomUserByHabbo = currentRoom.GetRoomUserManager().GetRoomUserByHabbo(this.Session.GetHabbo().Id);
                            if ((currentRoom != null) && (roomUserByHabbo != null))
                            {
                                if (((!roomUserByHabbo.IsSitting && !roomUserByHabbo.IsSitting) && (!roomUserByHabbo.RidingHorse && !roomUserByHabbo.IsWalking)) && !roomUserByHabbo.Statusses.ContainsKey("lay"))
                                {
                                    if ((roomUserByHabbo.RotBody % 2) != 0)
                                    {
                                        roomUserByHabbo.RotBody--;
                                    }
                                    roomUserByHabbo.Statusses.Add("lay", "0.55");
                                    roomUserByHabbo.IsLyingDown = true;
                                    roomUserByHabbo.UpdateNeeded = true;
                                }
                                return true;
                            }
                            return true;
                        }

                    case "stand":
                        {
                            Room room2 = this.Session.GetHabbo().CurrentRoom;
                            RoomUser user2 = room2.GetRoomUserManager().GetRoomUserByHabbo(this.Session.GetHabbo().Id);
                            if ((room2 != null) && (user2 != null))
                            {
                                if (user2.IsSitting)
                                {
                                    user2.Statusses.Remove("sit");
                                    user2.IsSitting = false;
                                    user2.UpdateNeeded = true;
                                }
                                else if (user2.IsLyingDown)
                                {
                                    user2.Statusses.Remove("lay");
                                    user2.IsLyingDown = false;
                                    user2.UpdateNeeded = true;
                                }
                                return true;
                            }
                            return true;
                        }

                    case "ejectpets":
                    case "pickpets":
                        {
                            if (!this.Session.GetHabbo().GotCommand("pickall"))
                            {
                                return true;//1121;
                            }

                            Room room3 = this.Session.GetHabbo().CurrentRoom;
                            if (room3 == null)
                            {
                                return true;//1121;
                            }

                            foreach (Pet Pet in room3.GetRoomUserManager().GetPets())
                            {
                                if (Pet.OwnerId != Session.GetHabbo().Id)
                                {
                                    continue;
                                }
                                this.Session.GetHabbo().GetInventoryComponent().AddPet(Pet);
                                room3.GetRoomUserManager().RemoveBot(Pet.VirtualId, false);
                            }
                            this.Session.SendMessage(this.Session.GetHabbo().GetInventoryComponent().SerializePetInventory());
                            return true;
                        }

                    case "pickall":
                        {
                            if (!this.Session.GetHabbo().GotCommand("pickall"))
                            {
                                return true;//1121;
                            }
                            Room room3 = this.Session.GetHabbo().CurrentRoom;
                            if ((room3 == null) || !room3.CheckRights(this.Session, true, false))
                            {
                                this.Session.SendNotif("Ocurri\x00f3 un error.");
                                return true;//1121;
                            }
                            List<RoomItem> roomItemList = room3.GetRoomItemHandler().RemoveAllFurniture(this.Session);
                            if (this.Session.GetHabbo().GetInventoryComponent() != null)
                            {
                                this.Session.GetHabbo().GetInventoryComponent().AddItemArray(roomItemList);
                                this.Session.GetHabbo().GetInventoryComponent().UpdateItems(false);
                            }

                            return true;
                        }

                    case "unbugwalk":
                    case "unbug":
                    case "desbuggear":
                    case "desbuggearsala":
                        {
                            Room room = this.Session.GetHabbo().CurrentRoom;
                            if ((room != null) && room.CheckRights(this.Session, true))
                            {
                                room.GetRoomUserManager().ToSet.Clear();
                                foreach (RoomUser User in room.GetRoomUserManager().GetRoomUsers())
                                {
                                    User.ClearMovement(true);
                                }
                            }
                            return true;
                        }
                    case "unload":
                    case "reload":
                        if (this.Session.GetHabbo().GotCommand("reload"))
                        {
                            Room room = this.Session.GetHabbo().CurrentRoom;
                            if ((room != null) && room.CheckRights(this.Session, true, false))
                            {
                                CyberEnvironment.GetGame().GetRoomManager().UnloadRoom(room);
                                room.RequestReload();
                            }
                        }
                        return true;

                    case "setmax":
                        if (!this.Session.GetHabbo().GotCommand("setmax"))
                        {
                            return true;//1242;
                        }
                        if (Params.Length != 1)
                        {
                            try
                            {
                                int maxUsers = int.Parse(Params[1]);
                                if ((maxUsers > 100) && (this.Session.GetHabbo().Rank < 3))
                                {
                                    this.Session.SendNotif("El m\x00e1ximo es 100");
                                    return true;
                                }
                                if ((maxUsers < 10) && (this.Session.GetHabbo().Rank < 3))
                                {
                                    this.Session.SendNotif("El m\x00ednimo es 10");
                                    return true;
                                }
                                Room room5 = this.Session.GetHabbo().CurrentRoom;
                                room5.UsersMax = maxUsers;
                                room5.SetMaxUsers(maxUsers);
                            }
                            catch
                            {
                            }
                            return true;//1242;
                        }
                        SendChatMessage(this.Session, "You must enter a number to set");
                        return true;

                    case "userinfo":
                    case "ui":
                        {
                            if (!this.Session.GetHabbo().GotCommand("userinfo"))
                            {
                                return true;//1549;
                            }
                            string str = Params[1];
                            bool flag = true;
                            if (!string.IsNullOrEmpty(str))
                            {
                                GameClient clientByUsername = CyberEnvironment.GetGame().GetClientManager().GetClientByUsername(str);
                                if ((clientByUsername == null) || (clientByUsername.GetHabbo() == null))
                                {
                                    using (IQueryAdapter adapter = CyberEnvironment.GetDatabaseManager().getQueryReactor())
                                    {
                                        adapter.setQuery("SELECT username, rank, online, id, motto, credits FROM users WHERE username=@user LIMIT 1");
                                        adapter.addParameter("user", str);
                                        DataRow row2 = adapter.getRow();
                                        this.Session.SendNotif("User Info for " + str + ":\rRank: " + row2[1].ToString() + " \rUser Id: " + row2[3].ToString() + " \rMotto: " + row2[4].ToString() + " \rCredits: " + row2[5].ToString() + " \r");
                                    }
                                    return true;
                                }
                                Habbo habbo = clientByUsername.GetHabbo();
                                StringBuilder builder3 = new StringBuilder();
                                if (habbo.CurrentRoom != null)
                                {
                                    builder3.Append(" - ROOM INFORMAtiON [" + habbo.CurrentRoom.RoomId + "] - \r");
                                    builder3.Append("Owner: " + habbo.CurrentRoom.Owner + "\r");
                                    builder3.Append("Room Name: " + habbo.CurrentRoom.Name + "\r");
                                    builder3.Append(string.Concat(new object[] { "Current Users: ", habbo.CurrentRoom.UserCount, "/", habbo.CurrentRoom.UsersMax }));
                                }
                                this.Session.SendNotif(string.Concat(new object[] { 
                                "User info for: ", str, ":\rRank: ", habbo.Rank, " \rOnline: ", flag.ToString(), " \rUser Id: ", habbo.Id, " \rCurrent Room: ", habbo.CurrentRoomId, " \rMotto: ", habbo.Motto, " \rCredits: ", habbo.Credits, " \rMuted: ", habbo.Muted.ToString(), 
                                "\r\r\r", builder3.ToString()
                             }));
                                return true;
                            }
                            this.Session.SendNotif("Please enter a username");
                            return true;
                        }
                    case "disablediagonal":
                    case "disablediag":
                    case "togglediagonal":
                        if (this.Session.GetHabbo().GotCommand("disablediagonal"))
                        {
                            Room room6 = this.Session.GetHabbo().CurrentRoom;
                            room6 = this.Session.GetHabbo().CurrentRoom;
                            if ((room6 != null) && room6.CheckRights(this.Session, true, false))
                            {
                                if (!room6.GetGameMap().DiagonalEnabled)
                                {
                                    room6.GetGameMap().DiagonalEnabled = true;
                                }
                                else
                                {
                                    room6.GetGameMap().DiagonalEnabled = false;
                                }
                            }
                        }
                        return true;

                    case "freeze":
                        if (this.Session.GetHabbo().GotCommand("freeze"))
                        {
                            Room room1 = this.Session.GetHabbo().CurrentRoom;
                            RoomUser user3 = this.Session.GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(Params[1]);
                            if (user3 != null)
                            {
                                user3.Frozen = true;
                            }
                        }
                        return true;

                    case "unfreeze":
                        if (this.Session.GetHabbo().GotCommand("unfreeze"))
                        {
                            Room room41 = this.Session.GetHabbo().CurrentRoom;
                            RoomUser user4 = this.Session.GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(Params[1]);
                            if (user4 != null)
                            {
                                user4.Frozen = false;
                            }
                        }
                        return true;

                    case "setspeed":
                        if (this.Session.GetHabbo().GotCommand("setspeed"))
                        {
                            Room room7 = this.Session.GetHabbo().CurrentRoom;
                            room7 = this.Session.GetHabbo().CurrentRoom;
                            if ((room7 != null) && room7.CheckRights(this.Session, true, false))
                            {
                                try
                                {
                                    this.Session.GetHabbo().CurrentRoom.GetRoomItemHandler().SetSpeed(int.Parse(Params[1]));
                                }
                                catch
                                {
                                    this.Session.SendNotif("Numbers Only!");
                                }
                            }
                        }
                        return true;

                    case "regenmaps":
                    case "reloadmaps":
                    case "fixroom":
                        {
                            if (!this.Session.GetHabbo().GotCommand("regenmaps"))
                            {
                                return true;
                            }
                            Room room8 = this.Session.GetHabbo().CurrentRoom;
                            if (room8 != null)
                            {
                                if (!room8.CheckRights(this.Session, true, false))
                                {
                                    return true;
                                }
                                room8.GetGameMap().GenerateMaps(true);
                                this.Session.SendNotif("The room map has been refreshed!");
                                return true;
                            }
                            return true;
                        }

                    case "convertcredits":
                    case "redeemall":
                        if (this.Session.GetHabbo().GotCommand("redeemall"))
                        {
                            try
                            {
                                this.Session.GetHabbo().GetInventoryComponent().Redeemcredits(this.Session);
                                SendChatMessage(this.Session, "\x00a1Todos los cr\x00e9ditos fueron convertidos!");
                            }
                            catch (Exception exception)
                            {
                                Console.WriteLine(exception);
                            }
                        }
                        return true;

                    case "setvideo":
                    case "ponervideo":
                    case "colocarvideo":
                        if (this.Session.GetHabbo().GotCommand("redeemall"))
                        {
                            Room Room = this.Session.GetHabbo().CurrentRoom;
                            if (Room == null || !Room.CheckRights(Session))
                                return true;

                            if (Params.Length < 2)
                                return true;

                            string Video = Params[1].Replace("https://", "http://").Split('&')[0];

                            if (CyberEnvironment.GetGame().GetVideoManager().PlayVideoInRoom(Room, Video))
                            {
                                Session.SendNotif("Felicidades, ¡ya has puesto tu vídeo custom en todas las TVs de esta Sala! Haz doble clic sobre una, espera y aparecerá.");
                            }
                            else
                            {
                                Session.SendNotif("Ha habido un error mientras intentabas poner tu vídeo. Verifica si has escrito bien el link.");
                            }


                        }
                        return true;

                    case "mutebots":
                    case "mutepets":
                        if (this.Session.GetHabbo().GotCommand("mutebots") && this.Session.GetHabbo().CurrentRoom.CheckRights(this.Session, true, false))
                        {
                            Room room9 = this.Session.GetHabbo().CurrentRoom;
                            if (!room9.MutedBots)
                            {
                                room9.MutedBots = true;
                            }
                            else
                            {
                                room9.MutedBots = false;
                            }
                            SendChatMessage(this.Session, "Muted bots have been toggled");
                        }
                        return true;

                    case "dance":
                        {
                            int result = 1;
                            if ((Params.Length > 1) && int.TryParse(Params[1], out result))
                            {
                                result = 1;
                            }
                            if ((result > 4) || (result < 0))
                            {
                                this.Session.SendWhisper("The dance ID must be between 0 and 4!");
                                result = 0;
                            }
                            ServerMessage message2 = new ServerMessage();
                            message2.Init(Outgoing.DanceStatusMessageComposer);
                            message2.AppendInt32(this.Session.CurrentRoomUserID);
                            message2.AppendInt32(result);
                            this.Session.GetHabbo().CurrentRoom.SendMessage(message2);
                            return true;
                        }
                    case "deletegroup":
                        {
                            if (!this.Session.GetHabbo().GotCommand("deletegroup"))
                            {
                                return true;//1E19;
                            }
                            Room room12 = this.Session.GetHabbo().CurrentRoom;
                            if (room12.CheckRights(this.Session, true, false))
                            {
                                if ((Params.Length == 1) || (Params[1].ToLower() != "yes"))
                                {
                                    this.Session.SendNotif("Are you sure you want to delete this group?\nOnce you delete it you will never be able to get it back.\n\nTo continue, type ':deletegroup yes' (without '')");
                                    return true;
                                }
                                if (room12.Group == null)
                                {
                                    this.Session.SendNotif("This room does not have a group.");
                                    return true;
                                }
                                Guild group = room12.Group;
                                foreach (GroupUser user7 in group.Members.Values)
                                {
                                    GameClient clientByUserID = CyberEnvironment.GetGame().GetClientManager().GetClientByUserID(user7.Id);
                                    if (clientByUserID != null)
                                    {
                                        clientByUserID.GetHabbo().UserGroups.Remove(user7);
                                        if (clientByUserID.GetHabbo().FavouriteGroup == group.Id)
                                        {
                                            clientByUserID.GetHabbo().FavouriteGroup = 0;
                                        }
                                    }
                                }
                                room12.RoomData.Group = null;
                                CyberEnvironment.GetGame().GetGroupManager().DeleteGroup(group.Id);
                                CyberEnvironment.GetGame().GetRoomManager().UnloadRoom(room12);
                                return true;//1E19;
                            }
                            this.Session.SendNotif("You do not own this room!");
                            return true;
                        }
                    case "moonwalk":
                        {
                            if (!this.Session.GetHabbo().HasFuse("fuse_vip_commands") && !this.Session.GetHabbo().VIP)
                            {
                                return false;
                            }
                            Room room13 = this.Session.GetHabbo().CurrentRoom;
                            if (room13 != null)
                            {
                                RoomUser user8 = room13.GetRoomUserManager().GetRoomUserByHabbo(this.Session.GetHabbo().Id);
                                user8.IsMoonwalking = !user8.IsMoonwalking;
                                return true;
                            }
                            return true;
                        }
                    case "habnam":
                        {
                            if (!this.Session.GetHabbo().HasFuse("fuse_vip_commands") && !this.Session.GetHabbo().VIP)
                            {
                                return false;
                            }
                            Room room14 = this.Session.GetHabbo().CurrentRoom;
                            if (room14 != null)
                            {
                                RoomUser user9 = room14.GetRoomUserManager().GetRoomUserByHabbo(this.Session.GetHabbo().Id);
                                this.Session.GetHabbo().GetAvatarEffectsInventoryComponent().ActivateCustomEffect((user9.CurrentEffect != 140) ? 140 : 0);
                                return true;
                            }
                            return true;
                        }

                    case "mimic":
                    case "copylook":
                        if (this.Session.GetHabbo().HasFuse("fuse_vip_commands") || this.Session.GetHabbo().VIP)
                        {
                            string pName = Params[1];
                            RoomUser user10 = this.Session.GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(pName);
                            string query = null;
                            string look = null;
                            if (user10 != null)
                            {
                                query = user10.GetClient().GetHabbo().Gender;
                                look = user10.GetClient().GetHabbo().Look;
                                this.Session.GetHabbo().Gender = query;
                                this.Session.GetHabbo().Look = look;
                                using (IQueryAdapter adapter4 = CyberEnvironment.GetDatabaseManager().getQueryReactor())
                                {
                                    adapter4.setQuery("UPDATE users SET gender = @gender, look = @look WHERE username = @username");
                                    adapter4.addParameter("gender", query);
                                    adapter4.addParameter("look", look);
                                    adapter4.addParameter("username", this.Session.GetHabbo().Username);
                                    adapter4.runQuery();
                                }
                                Room room16 = this.Session.GetHabbo().CurrentRoom;
                                if (room16 == null)
                                {
                                    return true;
                                }
                                RoomUser user11 = room16.GetRoomUserManager().GetRoomUserByHabbo(this.Session.GetHabbo().Id);
                                if (user11 == null)
                                {
                                    return true;
                                }
                                this.Session.GetMessageHandler().GetResponse().Init(Outgoing.UpdateUserDataMessageComposer);
                                this.Session.GetMessageHandler().GetResponse().AppendInt32(-1);
                                this.Session.GetMessageHandler().GetResponse().AppendString(this.Session.GetHabbo().Look);
                                this.Session.GetMessageHandler().GetResponse().AppendString(this.Session.GetHabbo().Gender.ToLower());
                                this.Session.GetMessageHandler().GetResponse().AppendString(this.Session.GetHabbo().Motto);
                                this.Session.GetMessageHandler().GetResponse().AppendInt32(this.Session.GetHabbo().AchievementPoints);
                                this.Session.GetMessageHandler().SendResponse();
                                ServerMessage message3 = new ServerMessage(Outgoing.UpdateUserDataMessageComposer);
                                message3.AppendInt32(user11.VirtualId);
                                message3.AppendString(this.Session.GetHabbo().Look);
                                message3.AppendString(this.Session.GetHabbo().Gender.ToLower());
                                message3.AppendString(this.Session.GetHabbo().Motto);
                                message3.AppendInt32(this.Session.GetHabbo().AchievementPoints);
                                room16.SendMessage(message3);
                            }
                        }
                        return true;

                    case "push":
                        {
                            if (!this.Session.GetHabbo().HasFuse("fuse_vip_commands") && !this.Session.GetHabbo().VIP)
                            {
                                return true;//256A;
                            }
                            Room room17 = CyberEnvironment.GetGame().GetRoomManager().GetRoom(this.Session.GetHabbo().CurrentRoomId);
                            if (room17 != null)
                            {
                                if (Params.Length == 1)
                                {
                                    SendChatMessage(this.Session, "Ingresa un usuario");
                                    return true;
                                }
                                RoomUser user12 = room17.GetRoomUserManager().GetRoomUserByHabbo(Convert.ToString(Params[1]));
                                if (user12 == null)
                                {
                                    SendChatMessage(this.Session, "No se pudo encontrar el user!");
                                    return true;
                                }
                                if (user12.GetUsername() == this.Session.GetHabbo().Username)
                                {
                                    SendChatMessage(this.Session, "S\x00e9 que no quieres empujarte a ti mismo");
                                    return true;
                                }
                                RoomUser user13 = room17.GetRoomUserManager().GetRoomUserByHabbo(this.Session.GetHabbo().Id);
                                if ((user13 != null) && !user12.TeleportEnabled)
                                {
                                    if ((Math.Abs((int)(user12.X - user13.X)) < 2) && (Math.Abs((int)(user12.Y - user13.Y)) < 2))
                                    {
                                        if (user13.RotBody == 4)
                                        {
                                            user12.MoveTo(user12.X, user12.Y + 1);
                                        }
                                        if (user13.RotBody == 0)
                                        {
                                            user12.MoveTo(user12.X, user12.Y - 1);
                                        }
                                        if (user13.RotBody == 6)
                                        {
                                            user12.MoveTo(user12.X - 1, user12.Y);
                                        }
                                        if (user13.RotBody == 2)
                                        {
                                            user12.MoveTo(user12.X + 1, user12.Y);
                                        }
                                        if (user13.RotBody == 3)
                                        {
                                            user12.MoveTo(user12.X + 1, user12.Y + 1);
                                        }
                                        if (user13.RotBody == 1)
                                        {
                                            user12.MoveTo(user12.X + 1, user12.Y - 1);
                                        }
                                        if (user13.RotBody == 7)
                                        {
                                            user12.MoveTo(user12.X - 1, user12.Y - 1);
                                        }
                                        if (user13.RotBody == 5)
                                        {
                                            user12.MoveTo(user12.X - 1, user12.Y + 1);
                                        }
                                        user12.UpdateNeeded = true;
                                        user13.UpdateNeeded = true;
                                        user13.SetRot(PathFinder.CalculateRotation(user13.X, user13.Y, user12.GoalX, user12.GoalY));
                                    }
                                    else
                                    {
                                        SendChatMessage(this.Session, Params[1] + " no est\x00e1 tan cerca.");
                                    }
                                    return true;//256A;
                                }
                            }
                            return true;
                        }
                    case "pull":
                        if (this.Session.GetHabbo().HasFuse("fuse_vip_commands") || this.Session.GetHabbo().VIP)
                        {
                            Room room18 = this.Session.GetHabbo().CurrentRoom;
                            if (room18 == null)
                            {
                                return true;
                            }
                            RoomUser user14 = room18.GetRoomUserManager().GetRoomUserByHabbo(this.Session.GetHabbo().Id);
                            if (user14 == null)
                            {
                                return true;
                            }
                            if (Params.Length == 1)
                            {
                                SendChatMessage(this.Session, "\x00a1No hay user que se llame as\x00ed!");
                                return true;
                            }
                            GameClient client4 = CyberEnvironment.GetGame().GetClientManager().GetClientByUsername(Params[1]);
                            if (client4 == null)
                            {
                                return true;
                            }
                            if (client4.GetHabbo().Id == this.Session.GetHabbo().Id)
                            {
                                SendChatMessage(this.Session, "No puedes atraerte a ti mismo!");
                                return true;
                            }
                            RoomUser user15 = room18.GetRoomUserManager().GetRoomUserByHabbo(client4.GetHabbo().Id);
                            if (user15 == null)
                            {
                                return true;
                            }
                            if (user15.TeleportEnabled)
                            {
                                return true;
                            }
                            if ((Math.Abs((int)(user14.X - user15.X)) >= 3) || (Math.Abs((int)(user14.Y - user15.Y)) >= 3))
                            {
                                SendChatMessage(this.Session, "El usuario est\x00e1 muy lejos.");
                                return true;
                            }
                            if ((user14.RotBody % 2) != 0)
                            {
                                user14.RotBody--;
                            }
                            if (user14.RotBody == 0)
                            {
                                user15.MoveTo(user14.X, user14.Y - 1);
                            }
                            else if (user14.RotBody == 2)
                            {
                                user15.MoveTo(user14.X + 1, user14.Y);
                            }
                            else if (user14.RotBody == 4)
                            {
                                user15.MoveTo(user14.X, user14.Y + 1);
                            }
                            else if (user14.RotBody == 6)
                            {
                                user15.MoveTo(user14.X - 1, user14.Y);
                            }
                        }
                        return true;

                    case "enable":
                        {
                            if (!this.Session.GetHabbo().HasFuse("fuse_vip_commands") && !this.Session.GetHabbo().VIP)
                            {
                                return false;
                            }

                            if (Params.Length == 1)
                            {
                                SendChatMessage(this.Session, "Escribe un effect ID");
                                return true;
                            }
                            RoomUser user16 = this.Session.GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(this.Session.GetHabbo().Username);
                            if (user16.RidingHorse)
                            {
                                SendChatMessage(this.Session, "No puedes hacer eso mientras est\x00e1s montado en un caballo!");
                                return true;
                            }
                            if (user16.team == Team.none)
                            {
                                double num6;
                                if (user16.IsLyingDown)
                                {
                                    return true;
                                }
                                string s = Params[1];
                                if (double.TryParse(s, out num6))
                                {
                                    this.Session.GetHabbo().GetAvatarEffectsInventoryComponent().ActivateCustomEffect(int.Parse(num6.ToString()));
                                    return true;
                                }
                                SendChatMessage(this.Session, "\x00bfAcaso no has aprendido matem\x00e1ticas nunca? '" + s + "' no es un n\x00famero.");
                            }
                            return true;
                        }
                    case "handitem":
                        {
                            if (!this.Session.GetHabbo().HasFuse("fuse_vip_commands") && !this.Session.GetHabbo().VIP)
                            {
                                return false;
                            }
                            if (Params.Length == 1)
                            {
                                SendChatMessage(this.Session, "Escribe un item ID");
                                return true;
                            }
                            RoomUser user17 = this.Session.GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(this.Session.GetHabbo().Username);
                            if (user17.RidingHorse)
                            {
                                SendChatMessage(this.Session, "No puedes hacer eso mientras est\x00e1s montado en un caballo!");
                                return true;
                            }
                            if (user17.team == Team.none)
                            {
                                double num7;
                                if (user17.IsLyingDown)
                                {
                                    return true;
                                }
                                string str6 = Params[1];
                                if (double.TryParse(str6, out num7))
                                {
                                    user17.CarryItem(int.Parse(num7.ToString()));
                                    return true;
                                }
                                SendChatMessage(this.Session, "\x00bfAcaso no has aprendido matem\x00e1ticas nunca? '" + str6 + "' no es un n\x00famero.");
                            }
                            return true;
                        }
                    case "empty":
                        if (this.Session.GetHabbo().GotCommand("empty"))
                        {
                            this.Session.GetHabbo().GetInventoryComponent().ClearItems();
                        }
                        return true;

                    case "emptysom":
                        if (!this.Session.GetHabbo().GotCommand("emptysom"))
                        {
                            return false;
                        }
                        if (Params.Length != 1)
                        {
                            GameClient client5 = CyberEnvironment.GetGame().GetClientManager().GetClientByUsername(Params[1]);
                            if (client5 == null || client5.GetHabbo().Rank >= this.Session.GetHabbo().Rank)
                            {
                                return true;
                            }
                            client5.GetHabbo().GetInventoryComponent().ClearItems();
                            return false;
                        }
                        return true;

                    case "hit":
                        if (this.Session.GetHabbo().HasFuse("fuse_vip_commands") || this.Session.GetHabbo().VIP)
                        {
                            Room room19 = CyberEnvironment.GetGame().GetRoomManager().GetRoom(this.Session.GetHabbo().CurrentRoomId);
                            if (room19 == null)
                            {
                                return true;
                            }
                            if (Params.Length == 1)
                            {
                                SendChatMessage(this.Session, "\x00bfUsuario?");
                                return true;
                            }
                            RoomUser user18 = room19.GetRoomUserManager().GetRoomUserByHabbo(Convert.ToString(Params[1]));
                            if (user18 == null)
                            {
                                SendChatMessage(this.Session, "El usuario no se encontr\x00f3");
                                return true;
                            }
                            if (user18.GetUsername() == this.Session.GetHabbo().Username)
                            {
                                SendChatMessage(this.Session, "\x00a1No querr\x00e1s golpearte a ti mismo!");
                                return true;
                            }
                            RoomUser user19 = room19.GetRoomUserManager().GetRoomUserByHabbo(this.Session.GetHabbo().Id);
                            if (user19 == null)
                            {
                                return true;
                            }
                            if ((Math.Abs((int)(user18.X - user19.X)) < 2) && (Math.Abs((int)(user18.Y - user19.Y)) < 2))
                            {
                                if (user19.RotBody == 4)
                                {
                                    user18.MoveTo(user18.X, user18.Y + 1);
                                }
                                if (user19.RotBody == 0)
                                {
                                    user18.MoveTo(user18.X, user18.Y - 1);
                                }
                                if (user19.RotBody == 6)
                                {
                                    user18.MoveTo(user18.X - 1, user18.Y);
                                }
                                if (user19.RotBody == 2)
                                {
                                    user18.MoveTo(user18.X + 1, user18.Y);
                                }
                                if (user19.RotBody == 3)
                                {
                                    user18.MoveTo(user18.X + 1, user18.Y);
                                    user18.MoveTo(user18.X, user18.Y + 1);
                                }
                                if (user19.RotBody == 1)
                                {
                                    user18.MoveTo(user18.X + 1, user18.Y);
                                    user18.MoveTo(user18.X, user18.Y - 1);
                                }
                                if (user19.RotBody == 7)
                                {
                                    user18.MoveTo(user18.X - 1, user18.Y);
                                    user18.MoveTo(user18.X, user18.Y - 1);
                                }
                                if (user19.RotBody == 5)
                                {
                                    user18.MoveTo(user18.X - 1, user18.Y);
                                    user18.MoveTo(user18.X, user18.Y + 1);
                                }
                                user18.UpdateNeeded = true;
                            }
                        }
                        return true;

                    case "roomalert":
                    case "alertroom":
                    case "ra":
                    case "alertarsala":
                        if (!this.Session.GetHabbo().GotCommand("alert"))
                        {
                            return false;//2D3B;
                        }
                        string Alert = MergeParams(Params, 1);

                        foreach (RoomUser user in Session.GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUsers())
                        {
                            if (user.IsBot || user.GetClient() == null)
                            {
                                continue;
                            }
                            user.GetClient().SendNotif(Alert);
                        }
                        return true;

                    case "alert":
                        if (!this.Session.GetHabbo().GotCommand("alert"))
                        {
                            return false;//2D3B;
                        }
                        if ((Params[1] != null) && (Params[2] != null))
                        {
                            string username = null;
                            GameClient client6 = null;
                            username = Params[1];
                            client6 = CyberEnvironment.GetGame().GetClientManager().GetClientByUsername(username);
                            Room room42 = client6.GetHabbo().CurrentRoom;
                            if (client6 == null)
                            {
                                this.Session.SendNotif("User could not be found.");
                                return true;
                            }
                            client6.SendNotif(Params[2] + " -" + this.Session.GetHabbo().Username);
                            return true;//2D3B;
                        }
                        this.Session.SendNotif("You left something empty.");
                        return true;

                    case "kick":
                        if (this.Session.GetHabbo().GotCommand("kick"))
                        {
                            string str8 = null;
                            GameClient session = null;
                            str8 = Params[1];
                            session = CyberEnvironment.GetGame().GetClientManager().GetClientByUsername(str8);
                            Room room20 = session.GetHabbo().CurrentRoom;
                            if (session != null)
                            {
                                if (this.Session.GetHabbo().Rank <= session.GetHabbo().Rank)
                                {
                                    this.Session.SendNotif("You are not allowed to kick that user.");
                                    return true;
                                }
                                if (session.GetHabbo().CurrentRoomId < 1)
                                {
                                    this.Session.SendNotif("That user is not in a room and can not be kicked.");
                                    return true;
                                }
                                room20 = CyberEnvironment.GetGame().GetRoomManager().GetRoom(session.GetHabbo().CurrentRoomId);
                                if (room20 != null)
                                {
                                    room20.GetRoomUserManager().RemoveUserFromRoom(session, true, false);
                                    session.CurrentRoomUserID = -1;
                                    if (Params.Length > 2)
                                    {
                                        session.SendNotif("A moderator has kicked you from the room for the following reason: " + MergeParams(Params, 2));
                                    }
                                    else
                                    {
                                        session.SendNotif("A moderator has kicked you from the room.");
                                    }
                                }
                                return true;
                            }
                            this.Session.SendNotif("User could not be found.");
                        }
                        return true;

                    case "roommute":
                        if (this.Session.GetHabbo().GotCommand("roommute") || this.Session.GetHabbo().GotCommand("roomunmute"))
                        {
                            Room room21 = this.Session.GetHabbo().CurrentRoom;
                            if (!this.Session.GetHabbo().CurrentRoom.RoomMuted)
                            {
                                this.Session.GetHabbo().CurrentRoom.RoomMuted = true;
                            }
                            string str9 = MergeParams(Params, 1);
                            ServerMessage message4 = new ServerMessage();
                            message4.Init(Outgoing.AlertNotificationMessageComposer);
                            message4.AppendString("La sala fue muteada debido a:\n" + str9);
                            message4.AppendString("");
                            room21.SendMessage(message4);
                            CyberEnvironment.GetGame().GetModerationTool().LogStaffEntry(this.Session.GetHabbo().Username, string.Empty, "Room Mute", "Room muted");
                        }
                        return true;

                    case "roomunmute":
                        if (this.Session.GetHabbo().GotCommand("roomunmute"))
                        {
                            Room room22 = this.Session.GetHabbo().CurrentRoom;
                            if (this.Session.GetHabbo().CurrentRoom.RoomMuted)
                            {
                                this.Session.GetHabbo().CurrentRoom.RoomMuted = false;
                            }
                            ServerMessage message5 = new ServerMessage();
                            message5.Init(Outgoing.AlertNotificationMessageComposer);
                            message5.AppendString("Fuiste desmuteado");
                            message5.AppendString("");
                            room22.SendMessage(message5);
                        }
                        return true;

                    case "mute":
                        {
                            if (!this.Session.GetHabbo().GotCommand("mute"))
                            {
                                return true;//308F;
                            }
                            string str10 = null;
                            GameClient client8 = null;
                            Room room43 = this.Session.GetHabbo().CurrentRoom;
                            str10 = Params[1];
                            client8 = CyberEnvironment.GetGame().GetClientManager().GetClientByUsername(str10);
                            if ((client8 != null) && (client8.GetHabbo() != null))
                            {
                                if (client8.GetHabbo().Rank >= 4)
                                {
                                    this.Session.SendNotif("You are not allowed to (un)mute that user.");
                                    return true;
                                }
                                CyberEnvironment.GetGame().GetModerationTool().LogStaffEntry(this.Session.GetHabbo().Username, client8.GetHabbo().Username, "Mute", "Muted user");
                                client8.GetHabbo().Mute();
                                return true;//308F;
                            }
                            this.Session.SendNotif("User could not be found.");
                            return true;
                        }
                    case "flood":
                        {
                            if (!this.Session.GetHabbo().GotCommand("flood"))
                            {
                                return true;//311C;
                            }
                            string str11 = null;
                            GameClient client9 = null;
                            str11 = Params[1];
                            client9 = CyberEnvironment.GetGame().GetClientManager().GetClientByUsername(str11);
                            if (Params.Length == 3)
                            {
                                client9.GetHabbo().FloodTime = CyberEnvironment.GetUnixTimestamp() + Convert.ToInt32(Params[2]);
                                ServerMessage message6 = new ServerMessage(Outgoing.FloodFilterMessageComposer);
                                message6.AppendInt32(Convert.ToInt32(Params[2]));
                                client9.SendMessage(message6);
                                return true;//311C;
                            }
                            this.Session.SendNotif("You must include a username and a time for the person you want to flood.");
                            return true;
                        }
                    case "unmute":
                        {
                            if (!this.Session.GetHabbo().GotCommand("unmute"))
                            {
                                return true;//31D7;
                            }
                            string str12 = null;
                            GameClient client10 = null;
                            Room room44 = this.Session.GetHabbo().CurrentRoom;
                            str12 = Params[1];
                            client10 = CyberEnvironment.GetGame().GetClientManager().GetClientByUsername(str12);
                            if ((client10 != null) && (client10.GetHabbo() != null))
                            {
                                if (!client10.GetHabbo().Muted)
                                {
                                    return true;
                                }
                                CyberEnvironment.GetGame().GetModerationTool().LogStaffEntry(this.Session.GetHabbo().Username, client10.GetHabbo().Username, "Mute", "Un Muted user");
                                client10.GetHabbo().Unmute();
                                return true;//31D7;
                            }
                            this.Session.SendNotif("User could not be found.");
                            return true;
                        }
                    case "summon":
                    case "traer":
                    case "come":
                        if (this.Session.GetHabbo().GotCommand("summon"))
                        {
                            if (Params.Length >= 1)
                            {
                                string str13 = Params[1];
                                if (str13.ToLower() == Session.GetHabbo().Username.ToLower())
                                {
                                    Session.SendNotif("You can't summon yourself!");
                                    return true;
                                }

                                GameClient client11 = CyberEnvironment.GetGame().GetClientManager().GetClientByUsername(str13);
                                if (client11 == null)
                                {
                                    this.Session.SendNotif("Could not find user \"" + str13 + "\"");
                                    return true;
                                }

                                if (Session.GetHabbo().CurrentRoom != null && Session.GetHabbo().CurrentRoomId != client11.GetHabbo().CurrentRoomId)
                                {
                                    client11.GetMessageHandler().PrepareRoomForUser(Session.GetHabbo().CurrentRoom.RoomId, Session.GetHabbo().CurrentRoom.Password);
                                }
                                return true;
                            }
                            this.Session.SendNotif("No use specified");
                        }
                        return true;

                    case "summonall":
                        if (this.Session.GetHabbo().GotCommand("summonall"))
                        {
                            string str14 = Input.Substring(9);
                            foreach (GameClient client12 in CyberEnvironment.GetGame().GetClientManager().clients.Values)
                            {
                                client12.SendNotif("* Todos hab\x00e9is sido atra\x00eddos por " + this.Session.GetHabbo().Username + ":\r\n" + str14);
                                CyberEnvironment.GetGame().GetRoomManager().GenerateRoomData(this.Session.GetHabbo().CurrentRoomId).SerializeRoomData(new ServerMessage(), client12.GetHabbo().CurrentRoom == null, client12, false);

                                ServerMessage roomFwd = new ServerMessage(Outgoing.RoomForwardMessageComposer);
                                roomFwd.AppendUInt(this.Session.GetHabbo().CurrentRoomId);
                                client12.SendMessage(roomFwd);
                            }
                        }
                        return true;

                    case "follow":
                    case "seguir":
                    case "stalk":
                        if (this.Session.GetHabbo().GotCommand("follow"))
                        {
                            GameClient client13 = CyberEnvironment.GetGame().GetClientManager().GetClientByUsername(Params[1]);
                            if ((client13 != null) && (client13.GetHabbo() != null))
                            {
                                if (((client13 != null) && (client13.GetHabbo().CurrentRoom != null)) && (client13.GetHabbo().CurrentRoom != this.Session.GetHabbo().CurrentRoom))
                                {
                                    ServerMessage roomFwd = new ServerMessage(Outgoing.RoomForwardMessageComposer);
                                    roomFwd.AppendUInt(client13.GetHabbo().CurrentRoom.RoomId);
                                    Session.SendMessage(roomFwd);
                                }
                                return true;
                            }
                            this.Session.SendNotif("This user could not be found");
                        }
                        return true;

                    case "roomkick":
                        if (this.Session.GetHabbo().GotCommand("roomkick"))
                        {
                            Room room23 = this.Session.GetHabbo().CurrentRoom;
                            room23 = CyberEnvironment.GetGame().GetRoomManager().GetRoom(this.Session.GetHabbo().CurrentRoomId);
                            if (room23 != null)
                            {
                                string allert = MergeParams(Params, 1);
                                RoomKick kick = new RoomKick(allert, (int)this.Session.GetHabbo().Rank);
                                CyberEnvironment.GetGame().GetModerationTool().LogStaffEntry(this.Session.GetHabbo().Username, string.Empty, "Room kick", "Kicked the whole room");
                                room23.QueueRoomKick(kick);
                                return true;
                            }
                        }
                        return true;

                    case "banear":
                    case "ban":
                        if (this.Session.GetHabbo().GotCommand("ban"))
                        {
                            GameClient user = CyberEnvironment.GetGame().GetClientManager().GetClientByUsername(Params[1]);

                            if (user != null)
                            {
                                if (user.GetHabbo().Rank >= this.Session.GetHabbo().Rank)
                                {
                                    this.Session.SendNotif("You are not allowed to ban that user.");
                                    return true;
                                }
                                int Length = int.Parse(Params[2]);
                                string Message = MergeParams(Params, 3);
                                if (string.IsNullOrWhiteSpace(Message))
                                {
                                    Message = "El moderador no ha visto necesario darte un motivo de baneo";
                                }
                                Cyber.HabboHotel.Support.ModerationTool.BanUser(Session, user.GetHabbo().Id, Length, Message);
                                CyberEnvironment.GetGame().GetModerationTool().LogStaffEntry(this.Session.GetHabbo().Username, user.GetHabbo().Username, "Ban", "User have been banned [" + Params[2] + "]");
                            }
                            this.Session.SendNotif("User could not be found.");
                            return true;
                        }
                        return true;

                    case "desbanear":
                    case "unban":
                        if (this.Session.GetHabbo().GotCommand("unban"))
                        {
                            //GameClient user = CyberEnvironment.GetGame().GetClientManager().GetClientByUsername(@params[1]);
                            Habbo user = CyberEnvironment.getHabboForName(Params[1]);

                            if (user != null)
                            {
                                if (user.Rank >= this.Session.GetHabbo().Rank)
                                {
                                    this.Session.SendNotif("You are not allowed to unban that user.");
                                    return true;
                                }
                                using (IQueryAdapter adapter = CyberEnvironment.GetDatabaseManager().getQueryReactor())
                                {
                                    adapter.setQuery("DELETE FROM bans WHERE value = '" + user.Username + "'");
                                    adapter.runQuery();
                                    CyberEnvironment.GetGame().GetModerationTool().LogStaffEntry(this.Session.GetHabbo().Username, user.Username, "Unban", "Se ha desbaneado al usuario [" + Params[2] + "]");
                                }
                            }
                            this.Session.SendNotif("User could not be found.");
                            return true;
                        }
                        return true;

                    case "superban":
                        {
                            if (!this.Session.GetHabbo().GotCommand("superban"))
                            {
                                return true;//35DC;
                            }
                            GameClient client14 = null;
                            Room room45 = this.Session.GetHabbo().CurrentRoom;
                            client14 = CyberEnvironment.GetGame().GetClientManager().GetClientByUsername(Params[1]);
                            if (client14 != null)
                            {
                                if (client14.GetHabbo().Rank >= this.Session.GetHabbo().Rank)
                                {
                                    this.Session.SendNotif("You are not allowed to ban that user.");
                                    return true;
                                }
                                CyberEnvironment.GetGame().GetModerationTool().LogStaffEntry(this.Session.GetHabbo().Username, client14.GetHabbo().Username, "Ban", "Long ban forever");
                                CyberEnvironment.GetGame().GetBanManager().BanUser(client14, this.Session.GetHabbo().Username, 788922000.0, MergeParams(Params, 2), false, false);
                                return true;//35DC;
                            }
                            this.Session.SendNotif("User could not be found.");
                            return true;
                        }
                    case "togglewhisper":
                        this.Session.GetHabbo().GotCommand("togglewhisper");
                        return true;

                    case "fastwalk":
                    case "run":
                        if (this.Session.GetHabbo().HasFuse("fuse_vip_commands") || this.Session.GetHabbo().VIP)
                        {
                            RoomUser user20 = CyberEnvironment.GetGame().GetRoomManager().GetRoom(this.Session.GetHabbo().CurrentRoomId).GetRoomUserManager().GetRoomUserByHabbo(this.Session.GetHabbo().Id);
                            if (!user20.FastWalking)
                            {
                                user20.FastWalking = true;
                            }
                            else
                            {
                                user20.FastWalking = false;
                            }
                        }
                        return true;

                    case "promoteroom":
                        if (!this.Session.GetHabbo().GotCommand("promoteroom"))
                        {
                            return true;//37B3;
                        }
                        if (Params[1] != null)
                        {
                            int num9;
                            if (!int.TryParse(Params[1], out num9))
                            {
                                this.Session.SendNotif("You need use command like :promoteroom time (time being how long to run event for in seconds).");
                                return true;
                            }
                            Room room25 = CyberEnvironment.GetGame().GetRoomManager().GetRoom(this.Session.GetHabbo().CurrentRoomId);
                            CyberEnvironment.GetGame().GetRoomEvents().AddNewEvent(room25.RoomId, "Default Name", "Default Desc", this.Session, 0x1c20);
                            return true;//37B3;
                        }
                        this.Session.SendNotif("You need to enter event name and description.");
                        return true;

                    case "massdance":
                        if (this.Session.GetHabbo().GotCommand("massdance"))
                        {
                            Room room26 = CyberEnvironment.GetGame().GetRoomManager().GetRoom(this.Session.GetHabbo().CurrentRoomId);
                            RoomUser user21 = room26.GetRoomUserManager().GetRoomUserByHabbo(this.Session.GetHabbo().Id);
                            HashSet<RoomUser> roomUsers = room26.GetRoomUserManager().GetRoomUsers();
                            int i = Convert.ToInt32(Params[1]);
                            if (i <= 4)
                            {
                                if ((i > 0) && (user21.CarryItemID > 0))
                                {
                                    user21.CarryItem(0);
                                }
                                user21.DanceId = i;
                                foreach (RoomUser user22 in roomUsers)
                                {
                                    ServerMessage message7 = new ServerMessage(Outgoing.DanceStatusMessageComposer);
                                    message7.AppendInt32(user22.VirtualId);
                                    message7.AppendInt32(i);
                                    room26.SendMessage(message7);
                                    user22.DanceId = i;
                                }
                                return true;
                            }
                            this.Session.SendNotif("That is an invalid dance ID");
                        }
                        return true;

                    case "goboom":
                        if (this.Session.GetHabbo().GotCommand("goboom"))
                        {
                            Room room27 = CyberEnvironment.GetGame().GetRoomManager().GetRoom(this.Session.GetHabbo().CurrentRoomId);
                            room27.GetRoomUserManager().GetRoomUserByHabbo(this.Session.GetHabbo().Id);
                            foreach (RoomUser user23 in room27.GetRoomUserManager().GetRoomUsers())
                            {
                                user23.ApplyEffect(0x6c);
                            }
                        }
                        return true;

                    case "massenable":
                        if (this.Session.GetHabbo().GotCommand("massenable"))
                        {
                            Room room28 = CyberEnvironment.GetGame().GetRoomManager().GetRoom(this.Session.GetHabbo().CurrentRoomId);
                            room28.GetRoomUserManager().GetRoomUserByHabbo(this.Session.GetHabbo().Id);
                            foreach (RoomUser user24 in room28.GetRoomUserManager().GetRoomUsers())
                            {
                                if (!user24.RidingHorse)
                                {
                                    user24.ApplyEffect(Convert.ToInt32(Params[1]));
                                }
                            }
                        }
                        return true;

                    case "givecredits":
                    case "credits":
                    case "coins":
                        if (this.Session.GetHabbo().GotCommand("credits"))
                        {
                            int num11;
                            GameClient client16 = null;
                            Room room47 = this.Session.GetHabbo().CurrentRoom;
                            client16 = CyberEnvironment.GetGame().GetClientManager().GetClientByUsername(Params[1]);
                            if (client16 == null)
                            {
                                this.Session.SendNotif("User could not be found.");
                                return true;
                            }
                            if (!int.TryParse(Params[2], out num11))
                            {
                                this.Session.SendNotif("Invalid numbers no, please!");
                                return true;
                            }
                            client16.GetHabbo().Credits += num11;
                            client16.GetHabbo().UpdateCreditsBalance();
                            client16.SendNotif(this.Session.GetHabbo().Username + " gave you " + num11.ToString() + " credits :3");
                            this.Session.SendNotif("You succesfully updated the user's purse!");
                        }
                        return true;

                    case "pixels":
                    case "givepixels":
                    case "duckets":
                        if (this.Session.GetHabbo().GotCommand("duckets"))
                        {
                            int num12;
                            GameClient client17 = null;
                            Room room48 = this.Session.GetHabbo().CurrentRoom;
                            client17 = CyberEnvironment.GetGame().GetClientManager().GetClientByUsername(Params[1]);
                            if (client17 == null)
                            {
                                this.Session.SendNotif("User could not be found.");
                                return true;
                            }
                            if (!int.TryParse(Params[2], out num12))
                            {
                                this.Session.SendNotif("Invalid numbers no, please!");
                                return true;
                            }
                            client17.GetHabbo().ActivityPoints += num12;
                            this.Session.GetHabbo().NotifyNewPixels(num12);
                            client17.GetHabbo().UpdateActivityPointsBalance();
                            client17.SendNotif(this.Session.GetHabbo().Username + " gave you " + num12.ToString() + " Duckets!");
                            this.Session.SendNotif("You succesfully updated the User's Duckets balance!");
                        }
                        return true;

                    case "belcredits":
                    case "diamonds":
                    case "diamantes":
                    case "nuxeros":
                    case "givediamonds":
                    case "darnuxeros":
                        if (this.Session.GetHabbo().GotCommand("nuxeros"))
                        {
                            int num13;
                            GameClient client18 = null;
                            Room room49 = this.Session.GetHabbo().CurrentRoom;
                            client18 = CyberEnvironment.GetGame().GetClientManager().GetClientByUsername(Params[1]);
                            if (client18 == null)
                            {
                                this.Session.SendNotif("User could not be found.");
                                return true;
                            }
                            if (!int.TryParse(Params[2], out num13))
                            {
                                this.Session.SendNotif("OMG Numbers only, please!");
                                return true;
                            }
                            client18.GetHabbo().BelCredits += num13;
                            client18.GetHabbo().UpdateSeasonalCurrencyBalance();
                            client18.SendNotif(this.Session.GetHabbo().Username + " gave you " + num13.ToString() + " Diamonds!");
                            this.Session.SendNotif("You gave him/her diamonds!");
                        }
                        return true;

                    case "oldha":
                    case "ha":
                        if (this.Session.GetHabbo().GotCommand("ha"))
                        {
                            string str16 = MergeParams(Params, 1);
                            ServerMessage message8 = new ServerMessage(Outgoing.BroadcastNotifMessageComposer);
                            message8.AppendString(str16 + "\r\n- " + this.Session.GetHabbo().Username);
                            CyberEnvironment.GetGame().GetClientManager().QueueBroadcaseMessage(message8);
                            CyberEnvironment.GetGame().GetModerationTool().LogStaffEntry(this.Session.GetHabbo().Username, string.Empty, "HotelAlert", "Hotel alert [" + str16 + "]");
                        }
                        return true;

                    case "apagar":
                    case "shutdown":
                        if (this.Session.GetHabbo().GotCommand("shutdown"))
                        {
                            new Task(new Action(CyberEnvironment.PerformShutDown)).Start();
                            CyberEnvironment.GetGame().GetModerationTool().LogStaffEntry(this.Session.GetHabbo().Username, string.Empty, "Shutdown", "Issued shutdown command");
                        }
                        return true;

                    case "disconnect":
                    case "dc":
                        {
                            if (!this.Session.GetHabbo().GotCommand("dc"))
                            {
                                return true;//3FFD;
                            }
                            GameClient client20 = null;
                            Room room51 = this.Session.GetHabbo().CurrentRoom;
                            client20 = CyberEnvironment.GetGame().GetClientManager().GetClientByUsername(Params[1]);
                            if (client20 != null)
                            {
                                if ((client20.GetHabbo().Rank >= this.Session.GetHabbo().Rank))
                                {
                                    this.Session.SendNotif("You are not allowed to disconnect that user.");
                                    return true;
                                }
                                CyberEnvironment.GetGame().GetModerationTool().LogStaffEntry(this.Session.GetHabbo().Username, client20.GetHabbo().Username, "Disconnect", "User disconnected by user");
                                client20.GetConnection().Dispose();
                                return true;//3FFD;
                            }
                            this.Session.SendNotif("User not found.");
                            return true;
                        }
                    case "superha":
                    case "supernotif":
                        {
                            if (!this.Session.GetHabbo().GotCommand("superha"))
                            {
                                return false;
                            }
                            string notice = "";
                            string picture = "alert";
                            notice = MergeParams(Params, 1);
                            CyberEnvironment.GetGame().GetClientManager().SendSuperNotif("Notificaci\x00f3n", notice, picture, this.Session, "", "", true, false);
                            return true;
                        }

                    case "update_furnisprites":
                        {
                            if (this.Session.GetHabbo().GotCommand("ri"))
                            {
                                CyberEnvironment.GetGame().GetItemManager().UpdateFlats();
                            }
                            return true;
                        }

                    case "testgccollect":
                        {
                            if (this.Session.GetHabbo().GotCommand("ri"))
                            {
                                GC.Collect();
                            }
                            return true;
                        }

                    case "anonha":
                        {
                            if (this.Session.GetHabbo().GotCommand("ri"))
                            {
                                string str16 = MergeParams(Params, 1);
                                ServerMessage message8 = new ServerMessage(Outgoing.BroadcastNotifMessageComposer);
                                message8.AppendString(str16);
                                CyberEnvironment.GetGame().GetClientManager().QueueBroadcaseMessage(message8);

                            }
                            return true;
                        }
                    case "eventha":
                        {
                            if (!this.Session.GetHabbo().GotCommand("eventha"))
                            {
                                return false;
                            }
                            string str19 = MergeParams(Params, 1);
                            CyberEnvironment.GetGame().GetClientManager().SendSuperNotif("\x00a1Evento!", str19, "game_promo_small", this.Session, "event:navigator/goto/" + this.Session.GetHabbo().CurrentRoom.RoomId, "Ir para o quarto", true, true);
                            return true;
                        }
                    case "dcroom":
                        if (!this.Session.GetHabbo().GotCommand("dcroom"))
                        {
                            return false;
                        }
                        foreach (RoomUser user25 in this.Session.GetHabbo().CurrentRoom.GetRoomUserManager().UserList.Values)
                        {
                            if (((user25 != null) && (user25.GetClient() != null)) && ((this.Session.GetHabbo().Id != user25.GetClient().GetHabbo().Id) && (user25.GetClient().GetHabbo().Rank < this.Session.GetHabbo().Rank)))
                            {
                                user25.GetClient().GetConnection().Dispose();
                            }
                        }
                        return true;

                    case "dchotel":
                        if (!this.Session.GetHabbo().GotCommand("dchotel"))
                        {
                            return false;
                        }
                        foreach (GameClient client in CyberEnvironment.GetGame().GetClientManager().clients.Values)
                        {
                            if (client != null && client != Session)
                            {
                                client.GetConnection().Dispose();
                            }
                        }
                        return true;

                    case "reloadall":
                        if (this.Session.GetHabbo().GotCommand("reloadall"))
                        {
                            foreach (Room room30 in CyberEnvironment.GetGame().GetRoomManager().loadedRooms.Values)
                            {
                                if ((room30 != null) && room30.CheckRights(this.Session, true, false))
                                {
                                    CyberEnvironment.GetGame().GetRoomManager().UnloadRoom(room30);
                                    //     room30.RequestReload();
                                }
                            }
                        }
                        return false;

                    case "coord":
                    case "coords":
                    case "position":
                        {
                            if (!this.Session.GetHabbo().GotCommand("coords"))
                            {
                                return true;//436A;
                            }
                            Room room31 = this.Session.GetHabbo().CurrentRoom;
                            RoomUser user26 = null;
                            room31 = CyberEnvironment.GetGame().GetRoomManager().GetRoom(this.Session.GetHabbo().CurrentRoomId);
                            if (room31 != null)
                            {
                                user26 = room31.GetRoomUserManager().GetRoomUserByHabbo(this.Session.GetHabbo().Id);
                                if (user26 == null)
                                {
                                    return true;
                                }
                                this.Session.SendNotif(string.Concat(new object[] { "X: ", user26.X, "\n - Y: ", user26.Y, "\n - Z: ", user26.Z, "\n - Rot: ", user26.RotBody, ", sqState: ", room31.GetGameMap().GameMap[user26.X, user26.Y].ToString(), "\n\n - RoomID: ", this.Session.GetHabbo().CurrentRoomId }));
                                return true;//436A;
                            }
                            return true;
                        }
                    case "teleport":
                    case "tele":
                        {
                            if (!this.Session.GetHabbo().GotCommand("tele"))
                            {
                                return true;//4419;
                            }
                            Room room32 = this.Session.GetHabbo().CurrentRoom;
                            RoomUser user27 = null;
                            room32 = CyberEnvironment.GetGame().GetRoomManager().GetRoom(this.Session.GetHabbo().CurrentRoomId);
                            user27 = room32.GetRoomUserManager().GetRoomUserByHabbo(this.Session.GetHabbo().Id);
                            if (!user27.RidingHorse)
                            {
                                if (user27 == null)
                                {
                                    return true;
                                }
                                user27.TeleportEnabled = !user27.TeleportEnabled;
                                room32.GetGameMap().GenerateMaps(true);
                                return true;//4419;
                            }
                            SendChatMessage(this.Session, "You cannot teleport whilst riding a horse!");
                            return true;
                        }
                    case "update_youtube":
                    case "refresh_youtube":
                        if (!this.Session.GetHabbo().GotCommand("ri"))
                        {
                            return false;
                        }
                        this.Session.SendWhisper("Por favor espera, recargando los playlists YouTube...");
                        using (IQueryAdapter adapter5 = CyberEnvironment.GetDatabaseManager().getQueryReactor())
                        {
                            CyberEnvironment.GetGame().GetVideoManager().Load(adapter5);
                        }
                        this.Session.SendWhisper("\x00a1Listo! Los playlists YouTube han sido recargados.");
                        return true;

                    case "reload_polls":
                        if (!this.Session.GetHabbo().GotCommand("ri"))
                        {
                            return false;
                        }
                        using (IQueryAdapter adapter5 = CyberEnvironment.GetDatabaseManager().getQueryReactor())
                        {
                            CyberEnvironment.GetGame().GetPollManager().Init(adapter5);
                        }
                        return true;

                    case "update_breeds":
                        if (!this.Session.GetHabbo().GotCommand("ri"))
                        {
                            return false;
                        }
                        using (IQueryAdapter adapter6 = CyberEnvironment.GetDatabaseManager().getQueryReactor())
                        {
                            PetRace.Init(adapter6);
                        }
                        return true;

                    case "update_publi":
                        if (!this.Session.GetHabbo().GotCommand("ri"))
                        {
                            return false;
                        }
                        using (IQueryAdapter adapter7 = CyberEnvironment.GetDatabaseManager().getQueryReactor())
                        {
                            AntiPublicistas.Load(adapter7);
                        }
                        return true;

                    case "update_songs":
                        if (!this.Session.GetHabbo().GotCommand("ri"))
                        {
                            return false;
                        }
                        Cyber.HabboHotel.SoundMachine.SongManager.Initialize();

                        return true;

                    case "update_achievements":
                        if (!this.Session.GetHabbo().GotCommand("ri"))
                        {
                            return false;
                        }
                        CyberEnvironment.GetGame().GetAchievementManager().LoadAchievements(CyberEnvironment.GetDatabaseManager().getQueryReactor());

                        return true;



                    case "update_catalog":
                    case "reload_catalog":
                    case "recache_catalog":
                    case "refresh_catalog":
                    case "update_catalogue":
                    case "reload_catalogue":
                    case "recache_catalogue":
                    case "refresh_catalogue":
                    case "refreshcata":
                    case "rc":
                        if (this.Session.GetHabbo().GotCommand("rc"))
                        {
                            using (IQueryAdapter adapter8 = CyberEnvironment.GetDatabaseManager().getQueryReactor())
                            {
                                CyberEnvironment.GetGame().GetCatalog().Initialize(adapter8);
                            }
                            CyberEnvironment.GetGame().GetClientManager().QueueBroadcaseMessage(new ServerMessage(Outgoing.PublishShopMessageComposer));
                        }
                        return true;

                    case "refresh_promos":
                        if (this.Session.GetHabbo().GotCommand("ri"))
                        {
                            CyberEnvironment.GetGame().GetHotelView().RefreshPromoList();
                        }
                        return true;

                    case "update_items":
                    case "reload_items":
                    case "recache_items":
                    case "refresh_items":
                    case "ri":
                        if (this.Session.GetHabbo().GotCommand("ri"))
                        {
                            using (IQueryAdapter adapter9 = CyberEnvironment.GetDatabaseManager().getQueryReactor())
                            {
                                CyberEnvironment.GetGame().GetItemManager().LoadItems(adapter9);
                            }
                        }
                        return true;

                    case "update_navigator":
                    case "reload_navigator":
                    case "recache_navigator":
                    case "refresh_navigator":
                        if (this.Session.GetHabbo().GotCommand("ri"))
                        {
                            using (IQueryAdapter adapter11 = CyberEnvironment.GetDatabaseManager().getQueryReactor())
                            {
                                CyberEnvironment.GetGame().GetNavigator().Initialize(adapter11);
                            }
                            this.Session.SendNotif("The navigator has been updated!");
                        }
                        return true;

                    case "update_ranks":
                    case "reload_ranks":
                    case "recache_ranks":
                    case "refresh_ranks":
                        if (this.Session.GetHabbo().GotCommand("ri"))
                        {
                            using (IQueryAdapter adapter12 = CyberEnvironment.GetDatabaseManager().getQueryReactor())
                            {
                                CyberEnvironment.GetGame().GetRoleManager().LoadRights(adapter12);
                            }
                            this.Session.SendNotif("Ranks have been refreshed!");
                        }
                        return true;

                    case "update_settings":
                    case "reload_settings":
                    case "recache_settings":
                    case "refresh_settings":
                        if (this.Session.GetHabbo().GotCommand("ri"))
                        {
                            using (IQueryAdapter adapter13 = CyberEnvironment.GetDatabaseManager().getQueryReactor())
                            {
                                CyberEnvironment.ConfigData = new ConfigData(adapter13);
                            }
                        }
                        return true;

                    case "update_groups":
                    case "reload_groups":
                    case "recache_groups":
                    case "refresh_groups":
                        if (this.Session.GetHabbo().GotCommand("ri"))
                        {
                            using (IQueryAdapter dbClient = CyberEnvironment.GetDatabaseManager().getQueryReactor())
                            {
                                CyberEnvironment.GetGame().GetGroupManager().InitGroups(dbClient);
                            }
                            this.Session.SendNotif("Groups have been successfully reloaded");
                        }
                        return true;

                    case "update_bans":
                        if (this.Session.GetHabbo().GotCommand("ri"))
                        {
                            using (IQueryAdapter adapter14 = CyberEnvironment.GetDatabaseManager().getQueryReactor())
                            {
                                CyberEnvironment.GetGame().GetBanManager().LoadBans(adapter14);
                            }
                            this.Session.SendNotif("Bans have been refreshed!");
                        }
                        return true;

                    case "update_quests":
                        if (this.Session.GetHabbo().GotCommand("ri"))
                        {
                            CyberEnvironment.GetGame().GetQuestManager().Initialize(CyberEnvironment.GetDatabaseManager().getQueryReactor());
                            this.Session.SendNotif("Quests have been successfully reloaed!");
                        }
                        return true;

                    case "spull":
                        if (this.Session.GetHabbo().HasFuse("fuse_vip_commands") || this.Session.GetHabbo().VIP)
                        {
                            Room room33 = this.Session.GetHabbo().CurrentRoom;
                            if (room33 == null)
                            {
                                return true;
                            }
                            RoomUser user28 = room33.GetRoomUserManager().GetRoomUserByHabbo(this.Session.GetHabbo().Id);
                            if (user28 == null)
                            {
                                return true;
                            }
                            if (Params.Length == 1)
                            {
                                return true;
                            }
                            GameClient client21 = CyberEnvironment.GetGame().GetClientManager().GetClientByUsername(Params[1]);
                            if (client21 == null)
                            {
                                return true;
                            }

                            RoomUser user29 = room33.GetRoomUserManager().GetRoomUserByHabbo(client21.GetHabbo().Id);
                            if (client21.GetHabbo().Id == this.Session.GetHabbo().Id)
                            {
                                SendChatMessage(this.Session, "\x00a1No puedes empujarte a ti mismo!");
                                return true;
                            }
                            if (user29.TeleportEnabled)
                            {
                                return true;
                            }
                            if ((user28.RotBody % 2) != 0)
                            {
                                user28.RotBody--;
                            }
                            if (user28.RotBody == 0)
                            {
                                user29.MoveTo(user28.X, user28.Y - 1);
                            }
                            else if (user28.RotBody == 2)
                            {
                                user29.MoveTo(user28.X + 1, user28.Y);
                            }
                            else if (user28.RotBody == 4)
                            {
                                user29.MoveTo(user28.X, user28.Y + 1);
                            }
                            else if (user28.RotBody == 6)
                            {
                                user29.MoveTo(user28.X - 1, user28.Y);
                            }
                        }
                        return true;

                    case "globalcredits":
                        if (!this.Session.GetHabbo().GotCommand("globalcredits"))
                        {
                            return true;//4A0D;
                        }
                        if (Params.Length != 1)
                        {
                            try
                            {
                                int amount = int.Parse(Params[1]);
                                using (IQueryAdapter adapter15 = CyberEnvironment.GetDatabaseManager().getQueryReactor())
                                {
                                    adapter15.runFastQuery("UPDATE users SET credits=credits+" + amount);
                                }

                                foreach (GameClient Client in CyberEnvironment.GetGame().GetClientManager().clients.Values)
                                {
                                    if (Client.GetHabbo() != null)
                                    {
                                        Client.GetHabbo().Credits += amount;
                                        Client.GetHabbo().UpdateCreditsBalance();
                                    }
                                }
                            }
                            catch
                            {
                            }
                            return true;//4A0D;
                        }
                        this.Session.SendNotif("You need to enter an amount!");
                        return true;

                    case "masscredits":
                        if (!this.Session.GetHabbo().GotCommand("masscredits"))
                        {
                            return true;//4A9D;
                        }
                        if (Params.Length != 1)
                        {
                            try
                            {
                                int num16 = int.Parse(Params[1]);
                                foreach (GameClient Client in CyberEnvironment.GetGame().GetClientManager().clients.Values)
                                {
                                    if (Client.GetHabbo() != null)
                                    {
                                        Client.GetHabbo().Credits += num16;
                                        Client.GetHabbo().UpdateCreditsBalance();
                                    }
                                }
                            }
                            catch
                            {
                            }
                            return true;//4A9D;
                        }
                        this.Session.SendNotif("You need to enter an amount!");
                        return true;

                    case "massnux":
                    case "massbelcredits":
                    case "massnuxeros":
                    case "massdiamonds":
                        if (!this.Session.GetHabbo().GotCommand("masscredits"))
                        {
                            return true;//4B40;
                        }
                        if (Params.Length != 1)
                        {
                            try
                            {
                                int num17 = int.Parse(Params[1]);
                                foreach (GameClient client22 in CyberEnvironment.GetGame().GetClientManager().clients.Values)
                                {
                                    Habbo habbo4 = client22.GetHabbo();
                                    habbo4.BelCredits += num17;
                                    client22.GetHabbo().UpdateSeasonalCurrencyBalance();
                                }
                            }
                            catch
                            {
                            }
                            return true;//4B40;
                        }
                        this.Session.SendNotif("Ingresa monto.");
                        return true;

                    case "placa":
                    case "darplaca":
                    case "badge":
                    case "givebadge":
                        if (this.Session.GetHabbo().GotCommand("givebadge"))
                        {
                            if (Params.Length == 3)
                            {
                                GameClient client23 = null;
                                Room room52 = this.Session.GetHabbo().CurrentRoom;
                                client23 = CyberEnvironment.GetGame().GetClientManager().GetClientByUsername(Params[1]);
                                if (client23 != null)
                                {
                                    client23.GetHabbo().GetBadgeComponent().GiveBadge(Params[2], true, client23, false);
                                    CyberEnvironment.GetGame().GetModerationTool().LogStaffEntry(this.Session.GetHabbo().Username, client23.GetHabbo().Username, "Badge", "Badge given to user [" + Params[2] + "]");
                                    return true;
                                }
                                this.Session.SendNotif("User no se encontr\x00f3.");
                                return true;
                            }
                            this.Session.SendNotif("\x00a1Incluye c\x00f3digo de placa y User!");
                        }
                        return true;

                    case "quitarplaca":
                    case "takebadge":
                        if (this.Session.GetHabbo().GotCommand("takebadge"))
                        {
                            if (Params.Length == 3)
                            {
                                GameClient client23 = null;
                                Room room52 = this.Session.GetHabbo().CurrentRoom;
                                client23 = CyberEnvironment.GetGame().GetClientManager().GetClientByUsername(Params[1]);
                                if (client23 != null)
                                {
                                    if (client23.GetHabbo().GetBadgeComponent().HasBadge(Params[2]))
                                    {
                                        client23.GetHabbo().GetBadgeComponent().RemoveBadge(Params[2], client23);
                                        CyberEnvironment.GetGame().GetModerationTool().LogStaffEntry(this.Session.GetHabbo().Username, client23.GetHabbo().Username, "Badge Taken", "Badge taken from user [" + Params[2] + "]");
                                        return true;
                                    }
                                    else
                                    {
                                        this.Session.SendNotif("El User no tiene esa placa.");
                                        return true;
                                    }
                                }
                                this.Session.SendNotif("User no se encontr\x00f3.");
                                return true;
                            }
                            this.Session.SendNotif("\x00a1Incluye c\x00f3digo de placa y User!");
                        }
                        return true;

                    case "roombadge":
                        if (!this.Session.GetHabbo().GotCommand("roombadge"))
                        {
                            return true;//4CDC;
                        }
                        if (Params.Length != 1)
                        {
                            Room room34 = this.Session.GetHabbo().CurrentRoom;
                            if (this.Session.GetHabbo().CurrentRoom == null)
                            {
                                return true;
                            }

                            foreach (RoomUser current in room34.GetRoomUserManager().UserList.Values)
                            {
                                try
                                {
                                    if (!current.IsBot && current.GetClient() != null && current.GetClient().GetHabbo() != null)
                                    {
                                        current.GetClient().GetHabbo().GetBadgeComponent().GiveBadge(Params[1], true, current.GetClient(), false);
                                    }
                                }
                                catch
                                {
                                }
                            }

                            CyberEnvironment.GetGame().GetModerationTool().LogStaffEntry(this.Session.GetHabbo().Username, string.Empty, "Badge", string.Concat(new object[] { "Roombadge in room [", room34.RoomId, "] with badge [", Params[1], "]" }));
                            return true;//4CDC;
                        }

                        this.Session.SendNotif("You must enter a badge code!");
                        return true;

                    case "massbadge":
                        if (!this.Session.GetHabbo().GotCommand("massbadge"))
                        {
                            return true;//4D71;
                        }
                        if (Params.Length != 1)
                        {
                            Room room53 = this.Session.GetHabbo().CurrentRoom;
                            CyberEnvironment.GetGame().GetClientManager().QueueBadgeUpdate(Params[1]);
                            CyberEnvironment.GetGame().GetModerationTool().LogStaffEntry(this.Session.GetHabbo().Username, string.Empty, "Badge", "Mass badge with badge [" + Params[1] + "]");
                            new ServerMessage();
                            return true;//4D71;
                        }
                        this.Session.SendNotif("You must enter a badge code!");
                        return true;

                    case "alleyesonme":
                        {
                            if (!this.Session.GetHabbo().GotCommand("alleyesonme"))
                            {
                                return false;//4E6D;
                            }
                            Room room54 = this.Session.GetHabbo().CurrentRoom;
                            if (this.Session.GetHabbo().CurrentRoom != null)
                            {
                                Room room35 = CyberEnvironment.GetGame().GetRoomManager().GetRoom(this.Session.GetHabbo().CurrentRoomId);
                                RoomUser user30 = room35.GetRoomUserManager().GetRoomUserByHabbo(this.Session.GetHabbo().Id);
                                foreach (RoomUser user31 in room35.GetRoomUserManager().GetRoomUsers())
                                {
                                    if (this.Session.GetHabbo().Id != user31.UserID)
                                    {
                                        user31.SetRot(PathFinder.CalculateRotation(user31.X, user31.Y, user30.X, user30.Y));
                                    }
                                }
                                return true;//4E6D;
                            }
                            return true;
                        }
                    case "ipban":
                    case "banip":
                        if (this.Session.GetHabbo().GotCommand("ipban"))
                        {
                            if (Params.Length == 1)
                            {
                                this.Session.SendNotif("You must include a username and reason!");
                            }
                            GameClient client24 = CyberEnvironment.GetGame().GetClientManager().GetClientByUsername(Params[1].ToString());
                            if (client24 == null)
                            {
                                this.Session.SendNotif("An unknown error occured whilst finding this user!");
                                return true;
                            }
                            try
                            {
                                CyberEnvironment.GetGame().GetBanManager().BanUser(client24, this.Session.GetHabbo().Username, 788922000.0, MergeParams(Params, 2), true, false);
                            }
                            catch (Exception exception2)
                            {
                                Console.WriteLine(exception2);
                            }
                        }
                        return true;

                    case "machineban":
                    case "banmachine":
                    case "mban":
                        if (this.Session.GetHabbo().GotCommand("machineban"))
                        {
                            if (Params.Length == 1)
                            {
                                this.Session.SendNotif("You must include a username and reason!");
                            }
                            GameClient client25 = CyberEnvironment.GetGame().GetClientManager().GetClientByUsername(Params[1].ToString());
                            if (client25 == null)
                            {
                                this.Session.SendNotif("An unknown error occured whilst finding this user!");
                                return true;
                            }
                            if (string.IsNullOrWhiteSpace(client25.MachineId))
                            {
                                this.Session.SendNotif("Unable to ban this user, they don't have a machine ID");
                                return true;
                            }
                            try
                            {
                                CyberEnvironment.GetGame().GetBanManager().BanUser(client25, this.Session.GetHabbo().Username, 2678400.0, "You have been banned! The reason given was:\n" + MergeParams(Params, 2), false, true);
                            }
                            catch (Exception exception3)
                            {
                                Console.WriteLine(exception3);
                            }
                        }
                        return true;

                    case "mip":
                        if (this.Session.GetHabbo().GotCommand("mip"))
                        {
                            if (Params.Length == 1)
                            {
                                this.Session.SendNotif("You must include a username and reason!");
                            }
                            GameClient client26 = CyberEnvironment.GetGame().GetClientManager().GetClientByUsername(Params[1].ToString());
                            if (client26 == null)
                            {
                                this.Session.SendNotif("An unknown error occured whilst finding this user!");
                                return true;
                            }
                            try
                            {
                                if (string.IsNullOrWhiteSpace(client26.MachineId))
                                {
                                    this.Session.SendNotif("Unable to ban this user, they don't have a machine ID");
                                    return true;
                                }
                                CyberEnvironment.GetGame().GetBanManager().BanUser(client26, this.Session.GetHabbo().Username, 2678400.0, "You have been banned! The reason given was:\n" + MergeParams(Params, 2), false, true);
                                CyberEnvironment.GetGame().GetBanManager().BanUser(client26, this.Session.GetHabbo().Username, 788922000.0, MergeParams(Params, 2), true, false);
                            }
                            catch (Exception exception4)
                            {
                                Console.WriteLine(exception4);
                            }
                        }
                        return true;

                    case "allaroundme":
                        {
                            if (!this.Session.GetHabbo().GotCommand("allaroundme"))
                            {
                                return true;//5258;
                            }
                            Room room55 = this.Session.GetHabbo().CurrentRoom;
                            if (this.Session.GetHabbo().CurrentRoom != null)
                            {
                                Room room36 = CyberEnvironment.GetGame().GetRoomManager().GetRoom(this.Session.GetHabbo().CurrentRoomId);
                                RoomUser user32 = room36.GetRoomUserManager().GetRoomUserByHabbo(this.Session.GetHabbo().Id);
                                HashSet<RoomUser> list6 = room36.GetRoomUserManager().GetRoomUsers();
                                foreach (RoomUser user33 in list6)
                                {
                                    if (this.Session.GetHabbo().Id != user33.UserID)
                                    {
                                        user33.MoveTo(user32.X, user32.Y, true);
                                    }
                                }
                                if ((Params.Length == 2) && (Params[1] == "override"))
                                {
                                    foreach (RoomUser user34 in list6)
                                    {
                                        if (this.Session.GetHabbo().Id != user34.UserID)
                                        {
                                            user34.AllowOverride = true;
                                            user34.MoveTo(user32.X, user32.Y, true);
                                            user34.AllowOverride = false;
                                        }
                                    }
                                }
                                return true;//5258;
                            }
                            this.Session.SendNotif("An unknown error occured!");
                            return true;
                        }
                    case "sayall":
                        if (this.Session.GetHabbo().GotCommand("sayall"))
                        {
                            Room room37 = this.Session.GetHabbo().CurrentRoom;
                            if (room37 != null)
                            {
                                string str20 = MergeParams(Params, 1);
                                if (str20 != "")
                                {
                                    foreach (RoomUser user35 in room37.GetRoomUserManager().GetRoomUsers())
                                    {
                                        user35.Chat(user35.GetClient(), str20, false, 0, 0);
                                    }
                                }
                            }
                        }
                        return true;

                    case "hal":
                        if (this.Session.GetHabbo().GotCommand("hal"))
                        {
                            Room room56 = this.Session.GetHabbo().CurrentRoom;
                            string str21 = Params[1];
                            string str22 = MergeParams(Params, 2);
                            ServerMessage message9 = new ServerMessage(Outgoing.AlertNotificationMessageComposer);
                            message9.AppendString(str22 + "\r\n-" + this.Session.GetHabbo().Username);
                            message9.AppendString(str21);
                            CyberEnvironment.GetGame().GetClientManager().QueueBroadcaseMessage(message9);
                            CyberEnvironment.GetGame().GetModerationTool().LogStaffEntry(this.Session.GetHabbo().Username, string.Empty, "HotelAlert", "Hotel alert [" + str22 + "]");
                        }
                        return true;

                    case "sa":
                    case "sm":
                        if (this.Session.GetHabbo().GotCommand("sa"))
                        {
                            string str23 = MergeParams(Params, 1);
                            ServerMessage message10 = new ServerMessage(Outgoing.SuperNotificationMessageComposer);
                            message10.AppendString("staffcloud");
                            message10.AppendInt32(2);
                            message10.AppendString("title");
                            message10.AppendString("Mensaje entre Staff");
                            message10.AppendString("message");
                            message10.AppendString("<b><font color=\"#6E09A0\" size=\"14\">Mensaje entre Staffs:</font></b>\r\n" + str23 + "\r\n- <i>" + this.Session.GetHabbo().Username + "</i>");
                            CyberEnvironment.GetGame().GetClientManager().StaffAlert(message10, 0);
                            CyberEnvironment.GetGame().GetModerationTool().LogStaffEntry(this.Session.GetHabbo().Username, string.Empty, "StaffAlert", "Staff alert [" + str23 + "]");
                        }
                        return true;

                    case "invisible":
                    case "spec":
                    case "spectatorsmode":
                        if (this.Session.GetHabbo().GotCommand("invisible"))
                        {
                            if (this.Session.GetHabbo().SpectatorMode)
                            {
                                this.Session.GetHabbo().SpectatorMode = false;
                                this.Session.SendNotif("You are not invisible anymore.");
                            }
                            else
                            {
                                this.Session.GetHabbo().SpectatorMode = true;
                                this.Session.SendNotif("Reload the room to be invisible");
                            }
                        }
                        return true;


                    case "makepublic":
                        {
                            if (!this.Session.GetHabbo().GotCommand("makepublic"))
                            {
                                return true;//56EA;
                            }
                            Room room38 = this.Session.GetHabbo().CurrentRoom;
                            if (room38 != null)
                            {
                                using (IQueryAdapter adapter17 = CyberEnvironment.GetDatabaseManager().getQueryReactor())
                                {
                                    adapter17.runFastQuery("UPDATE rooms SET roomtype='public' WHERE id=" + room38.RoomId + " LIMIT 1");
                                }
                                room38.Type = "public";
                                room38.RoomData.Type = "public";
                                room38.RoomData.SerializeRoomData(new ServerMessage(), false, this.Session, true);
                                return true;//56EA;
                            }
                            return true;
                        }
                    case "makeprivate":
                        {
                            if (!this.Session.GetHabbo().GotCommand("makeprivate"))
                            {
                                return true;//5790
                            }
                            Room room39 = this.Session.GetHabbo().CurrentRoom;
                            if (room39 != null)
                            {
                                using (IQueryAdapter adapter18 = CyberEnvironment.GetDatabaseManager().getQueryReactor())
                                {
                                    adapter18.runFastQuery("UPDATE rooms SET roomtype='private' WHERE id=" + room39.RoomId + " LIMIT 1");
                                }
                                room39.Type = "private";
                                room39.RoomData.Type = "private";
                                room39.RoomData.SerializeRoomData(new ServerMessage(), false, this.Session, true);
                                return true;//5790;
                            }
                            return true;
                        }

                    case "roomaction":
                        if (this.Session.GetHabbo().GotCommand("roomaction"))
                        {
                            try
                            {
                                Room room40 = this.Session.GetHabbo().CurrentRoom;
                                HashSet<RoomUser> list7 = room40.GetRoomUserManager().GetRoomUsers();
                                int action = short.Parse(Params[1]);
                                new ServerMessage();
                                foreach (RoomUser user37 in list7)
                                {
                                    if (user37 != null)
                                    {
                                        ServerMessage ActionMsg = new ServerMessage();
                                        ActionMsg.Init(Outgoing.RoomUserActionMessageComposer);
                                        ActionMsg.AppendInt32(user37.VirtualId);
                                        ActionMsg.AppendInt32(action);
                                        room40.SendMessage(ActionMsg);
                                    }
                                }
                            }
                            catch
                            {
                            }
                        }
                        return true;


                }
            }
            return false;
        }

        public static void SendChatMessage(GameClient Session, string Message)
        {
            Session.SendWhisper(Message);
        }
    }
}
