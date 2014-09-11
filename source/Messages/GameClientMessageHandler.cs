using Database_Manager.Database.Session_Details.Interfaces;
using HabboEncryption;
using HabboEncryption.CodeProject.Utils;
using HabboEncryption.Hurlant.Crypto.Prng;
using Cyber.Core;
using Cyber.Util;
using Cyber.HabboHotel.Achievements;
using Cyber.HabboHotel.Catalogs;
using Cyber.HabboHotel.GameClients;
using Cyber.HabboHotel.Groups;
using Cyber.HabboHotel.Items;
using Cyber.HabboHotel.Navigators;
using Cyber.HabboHotel.Pathfinding;
using Cyber.HabboHotel.Pets;
using Cyber.HabboHotel.Polls;
using Cyber.HabboHotel.Quests;
using Cyber.HabboHotel.Quests.Composer;
using Cyber.HabboHotel.RoomBots;
using Cyber.HabboHotel.Rooms;
using Cyber.HabboHotel.Rooms.Wired;
using Cyber.HabboHotel.SoundMachine;
using Cyber.HabboHotel.SoundMachine.Composers;
using Cyber.HabboHotel.Support;
using Cyber.HabboHotel.Users;
using Cyber.HabboHotel.Users.Badges;
using Cyber.HabboHotel.Users.Messenger;
using Cyber.HabboHotel.Users.Relationships;
using Cyber.HabboHotel.YouTube;
using Cyber.Messages.Headers;
using Cyber.Messages.StaticMessageHandlers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using Cyber.HabboHotel.PathFinding;
using System.Linq;
using System.Text;
using System.Threading;

namespace Cyber.Messages
{
	internal class GameClientMessageHandler
	{
		private GameClient Session;
		private ClientMessage Request;
		private ServerMessage Response;
		internal Room CurrentLoadingRoom;
        
        public void ReceiveNuxGifts()
        {
            if (!ExtraSettings.NEW_USER_GIFTS_ENABLED)
            {
                Session.SendNotif("Sorry, but NUX gifts are not enabled!");
                return;
            }
            else if (Session.GetHabbo().NUXPassed)
            {
                Session.SendNotif("You already got your gifts. You can't get them again!");
                return;
            }

            var item = Session.GetHabbo().GetInventoryComponent().AddNewItem(0, ExtraSettings.NEW_USER_GIFT_YTTV2_ID, "", 0, true, false, 0, 0);
            Session.GetHabbo().GetInventoryComponent().UpdateItems(false);

            Session.GetHabbo().BelCredits += 25;
            Session.GetHabbo().UpdateSeasonalCurrencyBalance();
            Session.GetHabbo().GetInventoryComponent().SendNewItems(item.Id);

            using (IQueryAdapter dbClient = CyberEnvironment.GetDatabaseManager().getQueryReactor())
            {
                if (Session.GetHabbo().VIP)
                {
                    dbClient.runFastQuery("UPDATE users SET vip = '1', vip_expire = DATE_ADD(vip_expire, INTERVAL 1 DAY), nux_passed = '1' WHERE id = " + Session.GetHabbo().Id);
                }
                else
                {
                    dbClient.runFastQuery("UPDATE users SET vip = '1', vip_expire = DATE_ADD(NOW(), INTERVAL 1 DAY), nux_passed = '1' WHERE id = " + Session.GetHabbo().Id);
                }
            }

            Session.GetHabbo().NUXPassed = true;
            Session.GetHabbo().VIP = true;
        }

        public void AcceptNuxGifts()
        {
            if (ExtraSettings.NEW_USER_GIFTS_ENABLED == false || Request.PopWiredInt32() != 0)
            {
                return;
            }

            ServerMessage NuxGifts = new ServerMessage(Outgoing.NuxListGiftsMessageComposer);
            NuxGifts.AppendInt32(3);//Cantidad

            NuxGifts.AppendInt32(0);
            NuxGifts.AppendInt32(0);
            NuxGifts.AppendInt32(1);//Cantidad
            // ahora nuevo bucle
            NuxGifts.AppendString("");
            NuxGifts.AppendString("nux/gift_yttv2.png");
            NuxGifts.AppendInt32(1);//cantidad
            //Ahora nuevo bucle...
            NuxGifts.AppendString("yttv2");
            NuxGifts.AppendString("");

            NuxGifts.AppendInt32(2);
            NuxGifts.AppendInt32(1);
            NuxGifts.AppendInt32(1);
            NuxGifts.AppendString("");
            NuxGifts.AppendString("nux/gift_diamonds.png");
            NuxGifts.AppendInt32(1);
            NuxGifts.AppendString("nux_gift_diamonds");
            NuxGifts.AppendString("");

            NuxGifts.AppendInt32(3);
            NuxGifts.AppendInt32(1);
            NuxGifts.AppendInt32(1);
            NuxGifts.AppendString("");
            NuxGifts.AppendString("nux/gift_vip1day.png");
            NuxGifts.AppendInt32(1);
            NuxGifts.AppendString("nux_gift_vip_1_day");
            NuxGifts.AppendString("");

            Session.SendMessage(NuxGifts);
        }

        public void CatalogueIndex()
        {
            if (Request.PopFixedString().ToUpper() != "NORMAL")
            {
                return;
            }
            uint rank = Session.GetHabbo().Rank;
            if (rank < 0)
            {
                rank = 1;
            }
            if (rank > 8)
            {
                rank = 8;
            }

            Session.SendMessage(CyberEnvironment.GetGame().GetCatalog().CachedIndexes[rank]);
        }

        public void CataloguePage()
        {
            int PageId = Request.PopWiredInt32();
            Request.PopWiredInt32();

            if (Request.PopFixedString().ToUpper() != "NORMAL")
            {
                return;
            }
            CatalogPage CPage = CyberEnvironment.GetGame().GetCatalog().GetPage(PageId);

            if (CPage == null || !CPage.Enabled || !CPage.Visible || CPage.MinRank > Session.GetHabbo().Rank)
            {
                return;
            }

            Session.SendMessage(CPage.CachedContentsMessage);
        }

        public void CatalogueClubPage()
        {
            int requestType = Request.PopWiredInt32();
            Session.SendMessage(CatalogPacket.ComposeClubPurchasePage(Session, requestType));
        }

        public void ReloadEcotron()
        {
            this.Response.Init(Outgoing.ReloadEcotronMessageComposer);
            this.Response.AppendInt32(1);
            this.Response.AppendInt32(0);
            this.SendResponse();
        }

        public void GiftWrappingConfig()
        {
            this.Response.Init(Outgoing.GiftWrappingConfigurationMessageComposer);
            this.Response.AppendBoolean(true);
            this.Response.AppendInt32(1);
            this.Response.AppendInt32(CyberEnvironment.GiftWrapper.GetGiftWrappersList.Count);
            using (List<uint>.Enumerator enumerator = CyberEnvironment.GiftWrapper.GetGiftWrappersList.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    int i = checked((int)enumerator.Current);
                    this.Response.AppendInt32(i);
                }
            }
            this.Response.AppendInt32(8);
            this.Response.AppendInt32(0);
            this.Response.AppendInt32(1);
            this.Response.AppendInt32(2);
            this.Response.AppendInt32(3);
            this.Response.AppendInt32(4);
            this.Response.AppendInt32(5);
            this.Response.AppendInt32(6);
            this.Response.AppendInt32(8);
            this.Response.AppendInt32(11);
            this.Response.AppendInt32(0);
            this.Response.AppendInt32(1);
            this.Response.AppendInt32(2);
            this.Response.AppendInt32(3);
            this.Response.AppendInt32(4);
            this.Response.AppendInt32(5);
            this.Response.AppendInt32(6);
            this.Response.AppendInt32(7);
            this.Response.AppendInt32(8);
            this.Response.AppendInt32(9);
            this.Response.AppendInt32(10);
            this.Response.AppendInt32(7);
            this.Response.AppendInt32(187);
            this.Response.AppendInt32(188);
            this.Response.AppendInt32(189);
            this.Response.AppendInt32(190);
            this.Response.AppendInt32(191);
            this.Response.AppendInt32(192);
            this.Response.AppendInt32(193);
            this.SendResponse();
        }

        public void GetRecyclerRewards()
        {
            this.Response.Init(Outgoing.RecyclerRewardsMessageComposer);
            List<int> ecotronRewardsLevels = CyberEnvironment.GetGame().GetCatalog().GetEcotronRewardsLevels();
            this.Response.AppendInt32(ecotronRewardsLevels.Count);
            foreach (int current in ecotronRewardsLevels)
            {
                this.Response.AppendInt32(current);
                this.Response.AppendInt32(current);
                List<EcotronReward> ecotronRewardsForLevel = CyberEnvironment.GetGame().GetCatalog().GetEcotronRewardsForLevel(uint.Parse(current.ToString()));
                this.Response.AppendInt32(ecotronRewardsForLevel.Count);
                foreach (EcotronReward current2 in ecotronRewardsForLevel)
                {
                    this.Response.AppendString(current2.GetBaseItem().PublicName);
                    this.Response.AppendInt32(1);
                    this.Response.AppendString(current2.GetBaseItem().Type.ToString());
                    this.Response.AppendInt32(current2.GetBaseItem().SpriteId);
                }
            }
            this.SendResponse();
        }

        public void GetPetBreeds()
        {
            string type = Request.PopFixedString();
            string PetType = "";
            int PetId = PetRace.GetPetId(type, out PetType);
            var Races = PetRace.GetRacesForRaceId(PetId);

            ServerMessage Message = new ServerMessage(Outgoing.SellablePetBreedsMessageComposer);
            Message.AppendString(PetType);
			Message.AppendInt32(Races.Count);
			foreach (PetRace current in Races)
			{
				Message.AppendInt32(PetId);
				Message.AppendInt32(current.Color1);
				Message.AppendInt32(current.Color2);
				Message.AppendBoolean(current.Has1Color);
				Message.AppendBoolean(current.Has2Color);
			}
            Session.SendMessage(Message);
        }

        public void PurchaseItem()
        {
            int pageId = Request.PopWiredInt32();
            int itemId = Request.PopWiredInt32();
            string extraData = Request.PopFixedString();
            int priceAmount = Request.PopWiredInt32();
            CyberEnvironment.GetGame().GetCatalog().HandlePurchase(Session, pageId, itemId, extraData, priceAmount, false, "", "", 0, 0, 0, false, 0u);
        }

        public void PurchaseGift()
        {
            int pageId = Request.PopWiredInt32();
            int itemId = Request.PopWiredInt32();
            string extraData = Request.PopFixedString();
            string giftUser = Request.PopFixedString();
            string giftMessage = Request.PopFixedString();
            int giftSpriteId = Request.PopWiredInt32();
            int giftLazo = Request.PopWiredInt32();
            int giftColor = Request.PopWiredInt32();
            bool undef = Request.PopWiredBoolean();
            CyberEnvironment.GetGame().GetCatalog().HandlePurchase(Session, pageId, itemId, extraData, 1, true, giftUser, giftMessage, giftSpriteId, giftLazo, giftColor, undef, 0u);
        }

        public void CheckPetName()
        {
            string PetName = Request.PopFixedString();

            int i = 0;
            if (PetName.Length > 15)
            {
                i = 1;
            }
            else
            {
                if (PetName.Length < 3)
                {
                    i = 2;
                }
                else
                {
                    if (!CyberEnvironment.IsValidAlphaNumeric(PetName))
                    {
                        i = 3;
                    }
                }
            }
            this.Response.Init(Outgoing.CheckPetNameMessageComposer);
            this.Response.AppendInt32(i);
            this.Response.AppendString(PetName);
            this.SendResponse();
        }

        public void CatalogueOffer()
        {
            int num = Request.PopWiredInt32();
            CatalogItem catalogItem = CyberEnvironment.GetGame().GetCatalog().GetItemFromOffer(num);
            

            if (catalogItem != null && Cyber.HabboHotel.Catalogs.Catalog.LastSentOffer != num)
            {
                Catalog.LastSentOffer = num;

                ServerMessage Message = new ServerMessage(Outgoing.CatalogOfferMessageComposer);
                CatalogPacket.ComposeItem(catalogItem, Message);
                Session.SendMessage(Message);
            }
        }

        public void CatalogueOfferConfig()
        {
            this.Response.Init(Outgoing.CatalogueOfferConfigMessageComposer);
            this.Response.AppendInt32(100);
            this.Response.AppendInt32(6);
            this.Response.AppendInt32(1);
            this.Response.AppendInt32(1);
            this.Response.AppendInt32(2);
            this.Response.AppendInt32(40);
            this.Response.AppendInt32(99);
            this.SendResponse();
        }

        public void Whisper()
        {
            if (!Session.GetHabbo().InRoom)
            {
                return;
            }
            Room currentRoom = Session.GetHabbo().CurrentRoom;
            string text = Request.PopFixedString();
            string text2 = text.Split(' ')[0];

            checked
            {
                string text3 = text.Substring(text2.Length + 1);
                int colour = Request.PopWiredInt32();
                RoomUser roomUserByHabbo = currentRoom.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
                RoomUser roomUserByHabbo2 = currentRoom.GetRoomUserManager().GetRoomUserByHabbo(text2);

                foreach (string current in currentRoom.WordFilter)
                {
                    text3 = System.Text.RegularExpressions.Regex.Replace(text3, current, "bobba", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                }

                if (Session.GetHabbo().Rank < 9 && AntiPublicistas.CheckPublicistas(text3))
                {
                    Session.PublicistaCount++;
                    Session.HandlePublicista(text3);
                    return;
                }

                if (roomUserByHabbo == null || roomUserByHabbo2 == null)
                {
                    Session.SendWhisper(text3);
                    return;
                }
                if (Session.GetHabbo().Rank < 4 && currentRoom.CheckMute(Session))
                {
                    return;
                }
                currentRoom.AddChatlog(Session.GetHabbo().Id, "<Whispering to " + text2 + ">: " + text3, false);

                CyberEnvironment.GetGame().GetQuestManager().ProgressUserQuest(Session, QuestType.SOCIAL_CHAT, 0u);
                int colour2 = colour;
                if (!roomUserByHabbo.IsBot)
                {
                    if (colour2 == 2 || (colour2 == 23 && !Session.GetHabbo().HasFuse("fuse_mod")) || colour2 < 0 || colour2 > 29)
                    {
                        colour2 = roomUserByHabbo.LastBubble; // or can also be just 0
                    }
                }

                roomUserByHabbo.UnIdle();

                ServerMessage Whisp = new ServerMessage(Outgoing.WhisperMessageComposer);
                Whisp.AppendInt32(roomUserByHabbo.VirtualId);
			Whisp.AppendString(text3);
			Whisp.AppendInt32(0);
			Whisp.AppendInt32(colour2);
			Whisp.AppendInt32(0);
            Whisp.AppendInt32(-1);

            roomUserByHabbo.GetClient().SendMessage(Whisp);
                if (roomUserByHabbo2 != null && !roomUserByHabbo2.IsBot && roomUserByHabbo2.UserID != roomUserByHabbo.UserID && !roomUserByHabbo2.GetClient().GetHabbo().MutedUsers.Contains(Session.GetHabbo().Id))
                {
                    roomUserByHabbo2.GetClient().SendMessage(Whisp);
                }
                List<RoomUser> roomUserByRank = currentRoom.GetRoomUserManager().GetRoomUserByRank(4);
                if (roomUserByRank.Count > 0)
                {
                    foreach (RoomUser current2 in roomUserByRank)
                    {
                        if (current2 != null && current2.HabboId != roomUserByHabbo2.HabboId && current2.HabboId != roomUserByHabbo.HabboId && current2.GetClient() != null)
                        {
                            ServerMessage WhispStaff = new ServerMessage(Outgoing.WhisperMessageComposer);
                            WhispStaff.AppendInt32(roomUserByHabbo.VirtualId);
                            WhispStaff.AppendString("Whisper to " + text2 + ": " + text3);
                            WhispStaff.AppendInt32(0);
                            WhispStaff.AppendInt32(colour2);
                            WhispStaff.AppendInt32(0);
                            WhispStaff.AppendInt32(-1);
                            current2.GetClient().SendMessage(WhispStaff);
                        }
                    }
                }
            }
        }
        public void Chat()
        {
            Room room = CyberEnvironment.GetGame().GetRoomManager().GetRoom(Session.GetHabbo().CurrentRoomId);
            if (room != null)
            {
                RoomUser RoomUser = room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
                if (RoomUser != null)
                {
                    string Message = Request.PopFixedString();
                    int Bubble = Request.PopWiredInt32();
                    int count = Request.PopWiredInt32();

                    if (!RoomUser.IsBot)
                    {
                        if (Bubble == 2 || (Bubble == 23 && !Session.GetHabbo().HasFuse("fuse_mod")) || Bubble < 0 || Bubble > 29)
                        {
                            Bubble = RoomUser.LastBubble; // or can also be just 0
                        }
                    }

                    RoomUser.Chat(Session, Message, false, count, Bubble);
                }
            }
        }

        public void Shout()
        {
            Room room = CyberEnvironment.GetGame().GetRoomManager().GetRoom(Session.GetHabbo().CurrentRoomId);
            if (room != null)
            {
                RoomUser roomUserByHabbo = room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
                if (roomUserByHabbo != null)
                {
                    string Msg = Request.PopFixedString();
                    int Bubble = Request.PopWiredInt32();
                    if (!roomUserByHabbo.IsBot)
                    {
                        if (Bubble == 2 || (Bubble == 23 && !Session.GetHabbo().HasFuse("fuse_mod")) || Bubble < 0 || Bubble > 29)
                        {
                            Bubble = roomUserByHabbo.LastBubble; // or can also be just 0
                        }
                    }

                    roomUserByHabbo.Chat(Session, Msg, true, -1, Bubble);
                }
            }
        }

        public void GetFloorPlanUsedCoords()
        {
            this.Response.Init(Outgoing.GetFloorPlanUsedCoordsMessageComposer);

            Room Room = Session.GetHabbo().CurrentRoom;

            if (Room == null)
            {
                this.Response.AppendInt32(0);
            }
            else
            {
                var Coords = Room.GetGameMap().CoordinatedItems.Keys.OfType<Point>();

                this.Response.AppendInt32(Coords.Count());

                foreach (Point Point in Coords)
                {
                    this.Response.AppendInt32(Point.X);
                    this.Response.AppendInt32(Point.Y);
                }
            }

            this.SendResponse();
        }

        public void GetFloorPlanDoor()
        {
            Room Room = Session.GetHabbo().CurrentRoom;
            if (Room == null)
            {
                return;
            }
            else
            {
                this.Response.Init(Outgoing.SetFloorPlanDoorMessageComposer);
                this.Response.AppendInt32(Room.GetGameMap().Model.DoorX);
                this.Response.AppendInt32(Room.GetGameMap().Model.DoorY);
                this.Response.AppendInt32(Room.GetGameMap().Model.DoorOrientation);
                this.SendResponse();
            }
        }

        public void SendBullyReport()
        {
            uint ReportedId = Request.PopWiredUInt();
            CyberEnvironment.GetGame().GetModerationTool().SendNewTicket(Session, 104, ReportedId, "Reporte automático por acoso.", 7, new List<string>());

            this.Response.Init(Outgoing.BullyReportSentMessageComposer);
            this.Response.AppendInt32(0);
            this.SendResponse();
        }

        public void OpenBullyReporting()
        {
            this.Response.Init(Outgoing.OpenBullyReportMessageComposer);
            this.Response.AppendInt32(0);
            this.SendResponse();
        }

		internal void SerializeGroupPurchasePage()
		{
			HashSet<RoomData> list = new HashSet<RoomData>(Session.GetHabbo().UsersRooms.Where(x => x.Group == null));

            this.Response.Init(Outgoing.GroupPurchasePageMessageComposer);
			this.Response.AppendInt32(10);
			this.Response.AppendInt32(list.Count);
			foreach (RoomData current2 in list)
			{
				this.Response.AppendUInt(current2.Id);
				this.Response.AppendString(current2.Name);
				this.Response.AppendBoolean(false);
			}
			this.Response.AppendInt32(5);
			this.Response.AppendInt32(5);
			this.Response.AppendInt32(11);
			this.Response.AppendInt32(4);
			this.Response.AppendInt32(6);
			this.Response.AppendInt32(11);
			this.Response.AppendInt32(4);
			this.Response.AppendInt32(0);
			this.Response.AppendInt32(0);
			this.Response.AppendInt32(0);
			this.Response.AppendInt32(0);
			this.Response.AppendInt32(0);
			this.Response.AppendInt32(0);
			this.Response.AppendInt32(0);
			this.Response.AppendInt32(0);
			this.Response.AppendInt32(0);
			this.SendResponse();
		}
		internal void SerializeGroupPurchaseParts()
		{
			this.Response.Init(Outgoing.GroupPurchasePartsMessageComposer);
			this.Response.AppendInt32(CyberEnvironment.GetGame().GetGroupManager().Bases.Count);
			foreach (GroupBases current in CyberEnvironment.GetGame().GetGroupManager().Bases)
			{
				this.Response.AppendInt32(current.Id);
				this.Response.AppendString(current.Value1);
				this.Response.AppendString(current.Value2);
			}
			this.Response.AppendInt32(CyberEnvironment.GetGame().GetGroupManager().Symbols.Count);
			foreach (GroupSymbols current2 in CyberEnvironment.GetGame().GetGroupManager().Symbols)
			{
				this.Response.AppendInt32(current2.Id);
				this.Response.AppendString(current2.Value1);
				this.Response.AppendString(current2.Value2);
			}
			this.Response.AppendInt32(CyberEnvironment.GetGame().GetGroupManager().BaseColours.Count);
			foreach (GroupBaseColours current3 in CyberEnvironment.GetGame().GetGroupManager().BaseColours)
			{
				this.Response.AppendInt32(current3.Id);
				this.Response.AppendString(current3.Colour);
			}
			this.Response.AppendInt32(CyberEnvironment.GetGame().GetGroupManager().SymbolColours.Count);
			foreach (GroupSymbolColours current4 in CyberEnvironment.GetGame().GetGroupManager().SymbolColours.Values)
			{
				this.Response.AppendInt32(current4.Id);
				this.Response.AppendString(current4.Colour);
			}
			this.Response.AppendInt32(CyberEnvironment.GetGame().GetGroupManager().BackGroundColours.Count);
			foreach (GroupBackGroundColours current5 in CyberEnvironment.GetGame().GetGroupManager().BackGroundColours.Values)
			{
				this.Response.AppendInt32(current5.Id);
				this.Response.AppendString(current5.Colour);
			}
			this.SendResponse();
		}
		internal void PurchaseGroup()
		{
            if ((this.Session != null) && (this.Session.GetHabbo().Credits >= 10))
            {
                List<int> gStates = new List<int>();
                string name = this.Request.PopFixedString();
                string description = this.Request.PopFixedString();
                uint roomid = this.Request.PopWiredUInt();
                int color = this.Request.PopWiredInt32();
                int num3 = this.Request.PopWiredInt32();
                this.Request.PopWiredInt32();
                int guildBase = this.Request.PopWiredInt32();
                int guildBaseColor = this.Request.PopWiredInt32();
                int num6 = this.Request.PopWiredInt32();

                RoomData roomData = CyberEnvironment.GetGame().GetRoomManager().GenerateRoomData(roomid);
                if (roomData.Owner != Session.GetHabbo().Username)
                    return;

                for (int i = 0; i < (num6 * 3); i++)
                {
                    int item = this.Request.PopWiredInt32();
                    gStates.Add(item);
                }

                string image = CyberEnvironment.GetGame().GetGroupManager().GenerateGuildImage(guildBase, guildBaseColor, gStates);

                Guild group;
                CyberEnvironment.GetGame().GetGroupManager().CreateGroup(name, description, roomid, image, this.Session, (!CyberEnvironment.GetGame().GetGroupManager().SymbolColours.Contains(color)) ? 1 : color, (!CyberEnvironment.GetGame().GetGroupManager().BackGroundColours.Contains(num3)) ? 1 : num3, out group);

                this.Session.SendMessage(CatalogPacket.PurchaseOK());
                this.Response.Init(Outgoing.GroupRoomMessageComposer);
                this.Response.AppendUInt(roomid);
                this.Response.AppendUInt(group.Id);
                this.SendResponse();
               
                roomData.Group = group;
                roomData.GroupId = group.Id;
                roomData.SerializeRoomData(this.Response, true, this.Session, false);
                if (this.Session.GetHabbo().CurrentRoom.RoomId != roomData.Id)
                {
                    ServerMessage roomFwd = new ServerMessage(Outgoing.RoomForwardMessageComposer);
                    roomFwd.AppendUInt(roomData.Id);
                    this.Session.SendMessage(roomFwd);
                }
                if (this.Session.GetHabbo().CurrentRoom != null && !this.Session.GetHabbo().CurrentRoom.LoadedGroups.ContainsKey(group.Id))
                {
                    this.Session.GetHabbo().CurrentRoom.LoadedGroups.Add(group.Id, group.Badge);
                }
                if (this.CurrentLoadingRoom != null && !this.CurrentLoadingRoom.LoadedGroups.ContainsKey(group.Id))
                {
                    this.CurrentLoadingRoom.LoadedGroups.Add(group.Id, group.Badge);
                }
                ServerMessage serverMessage = new ServerMessage(Outgoing.RoomGroupMessageComposer);
                serverMessage.AppendInt32(this.Session.GetHabbo().CurrentRoom.LoadedGroups.Count);
                foreach (KeyValuePair<uint, string> current in this.Session.GetHabbo().CurrentRoom.LoadedGroups)
                {
                    serverMessage.AppendUInt(current.Key);
                    serverMessage.AppendString(current.Value);
                }
                if (this.CurrentLoadingRoom != null)
                {
                    this.CurrentLoadingRoom.SendMessage(serverMessage);
                }

                if (this.Session.GetHabbo().FavouriteGroup == group.Id)
                {
                    ServerMessage serverMessage2 = new ServerMessage(Outgoing.ChangeFavouriteGroupMessageComposer);
                    serverMessage2.AppendInt32(this.CurrentLoadingRoom.GetRoomUserManager().GetRoomUserByHabbo(this.Session.GetHabbo().Id).VirtualId);
                    serverMessage2.AppendUInt(group.Id);
                    serverMessage2.AppendInt32(3);
                    serverMessage2.AppendString(group.Name);
                    this.CurrentLoadingRoom.SendMessage(serverMessage2);
                }
            }
		}
		internal void SerializeGroupInfo()
		{
			uint groupId = this.Request.PopWiredUInt();
			bool newWindow = this.Request.PopWiredBoolean();
			Guild group = CyberEnvironment.GetGame().GetGroupManager().GetGroup(groupId);
			if (group == null)
			{
				return;
			}
			CyberEnvironment.GetGame().GetGroupManager().SerializeGroupInfo(group, this.Response, this.Session, newWindow);
		}
		internal void SerializeGroupMembers()
		{
			uint groupId = this.Request.PopWiredUInt();
			int page = this.Request.PopWiredInt32();
			string searchVal = this.Request.PopFixedString();
			uint reqType = this.Request.PopWiredUInt();
			Guild group = CyberEnvironment.GetGame().GetGroupManager().GetGroup(groupId);
			this.Response.Init(Outgoing.GroupMembersMessageComposer);
			CyberEnvironment.GetGame().GetGroupManager().SerializeGroupMembers(this.Response, group, reqType, this.Session, searchVal, page);
			this.SendResponse();
		}
		internal void MakeGroupAdmin()
		{
			uint num = this.Request.PopWiredUInt();
			uint num2 = this.Request.PopWiredUInt();
			Guild group = CyberEnvironment.GetGame().GetGroupManager().GetGroup(num);
			if (this.Session.GetHabbo().Id != group.CreatorId || !group.Members.ContainsKey(num2) || group.Admins.ContainsKey(num2))
			{
				return;
			}
			group.Members[num2].Rank = 1;
			group.Admins.Add(num2, group.Members[num2]);
			this.Response.Init(Outgoing.GroupMembersMessageComposer);
			CyberEnvironment.GetGame().GetGroupManager().SerializeGroupMembers(this.Response, group, 1u, this.Session, "", 0);
			this.SendResponse();
			Room room = CyberEnvironment.GetGame().GetRoomManager().GetRoom(group.RoomId);
			if (room != null)
			{
				RoomUser roomUserByHabbo = room.GetRoomUserManager().GetRoomUserByHabbo(CyberEnvironment.getHabboForId(num2).Username);
				if (roomUserByHabbo != null)
				{
					if (!roomUserByHabbo.Statusses.ContainsKey("flatctrl 1"))
					{
						roomUserByHabbo.AddStatus("flatctrl 1", "");
					}
					this.Response.Init(Outgoing.RoomRightsLevelMessageComposer);
					this.Response.AppendInt32(1);
					roomUserByHabbo.GetClient().SendMessage(this.GetResponse());
					roomUserByHabbo.UpdateNeeded = true;
				}
			}
			using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
			{
				queryreactor.runFastQuery(string.Concat(new object[]
				{
					"UPDATE group_memberships SET rank='1' WHERE group_id=",
					num,
					" AND user_id=",
					num2,
					" LIMIT 1;"
				}));
			}
		}
		internal void RemoveGroupAdmin()
		{
			uint num = this.Request.PopWiredUInt();
			uint num2 = this.Request.PopWiredUInt();
			Guild group = CyberEnvironment.GetGame().GetGroupManager().GetGroup(num);
			if (this.Session.GetHabbo().Id != group.CreatorId || !group.Members.ContainsKey(num2) || !group.Admins.ContainsKey(num2))
			{
				return;
			}
			group.Members[num2].Rank = 0;
			group.Admins.Remove(num2);
			this.Response.Init(Outgoing.GroupMembersMessageComposer);
			CyberEnvironment.GetGame().GetGroupManager().SerializeGroupMembers(this.Response, group, 0u, this.Session, "", 0);
			this.SendResponse();
			Room room = CyberEnvironment.GetGame().GetRoomManager().GetRoom(group.RoomId);
			if (room != null)
			{
				RoomUser roomUserByHabbo = room.GetRoomUserManager().GetRoomUserByHabbo(CyberEnvironment.getHabboForId(num2).Username);
				if (roomUserByHabbo != null)
				{
					if (roomUserByHabbo.Statusses.ContainsKey("flatctrl 1"))
					{
						roomUserByHabbo.RemoveStatus("flatctrl 1");
					}
					this.Response.Init(Outgoing.RoomRightsLevelMessageComposer);
					this.Response.AppendInt32(0);
					roomUserByHabbo.GetClient().SendMessage(this.GetResponse());
					roomUserByHabbo.UpdateNeeded = true;
				}
			}
			using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
			{
				queryreactor.runFastQuery(string.Concat(new object[]
				{
					"UPDATE group_memberships SET rank='0' WHERE group_id=",
					num,
					" AND user_id=",
					num2,
					" LIMIT 1;"
				}));
			}
		}
		internal void AcceptMembership()
		{
			uint num = this.Request.PopWiredUInt();
			uint num2 = this.Request.PopWiredUInt();
			Guild group = CyberEnvironment.GetGame().GetGroupManager().GetGroup(num);
			if (this.Session.GetHabbo().Id != group.CreatorId && !group.Admins.ContainsKey(this.Session.GetHabbo().Id) && !group.Requests.Contains(num2))
			{
				return;
			}
			if (group.Members.ContainsKey(num2))
			{
				group.Requests.Remove(num2);
				using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
				{
					queryreactor.runFastQuery(string.Concat(new object[]
					{
						"DELETE FROM group_requests WHERE group_id=",
						num,
						" AND user_id=",
						num2,
						" LIMIT 1;"
					}));
				}
				return;
			}
			group.Requests.Remove(num2);
			group.Members.Add(num2, new GroupUser(num2, num, 0));
            group.Admins.Add(num2, group.Members[num2]);
			CyberEnvironment.GetGame().GetGroupManager().SerializeGroupInfo(group, this.Response, this.Session, false);
			this.Response.Init(Outgoing.GroupMembersMessageComposer);
			CyberEnvironment.GetGame().GetGroupManager().SerializeGroupMembers(this.Response, group, 0u, this.Session, "", 0);
			this.SendResponse();
			using (IQueryAdapter queryreactor2 = CyberEnvironment.GetDatabaseManager().getQueryReactor())
			{
				queryreactor2.runFastQuery(string.Concat(new object[]
				{
					"DELETE FROM group_requests WHERE group_id=",
					num,
					" AND user_id=",
					num2,
					" LIMIT 1;INSERT INTO group_memberships (group_id, user_id) VALUES (",
					num,
					", ",
					num2,
					")"
				}));
			}
		}
		internal void DeclineMembership()
		{
			uint num = this.Request.PopWiredUInt();
			uint num2 = this.Request.PopWiredUInt();
			Guild group = CyberEnvironment.GetGame().GetGroupManager().GetGroup(num);
			if (this.Session.GetHabbo().Id != group.CreatorId && !group.Admins.ContainsKey(this.Session.GetHabbo().Id) && !group.Requests.Contains(num2))
			{
				return;
			}
			group.Requests.Remove(num2);
			this.Response.Init(Outgoing.GroupMembersMessageComposer);
			CyberEnvironment.GetGame().GetGroupManager().SerializeGroupMembers(this.Response, group, 2u, this.Session, "", 0);
			this.SendResponse();
			Room room = CyberEnvironment.GetGame().GetRoomManager().GetRoom(group.RoomId);
			if (room != null)
			{
				RoomUser roomUserByHabbo = room.GetRoomUserManager().GetRoomUserByHabbo(CyberEnvironment.getHabboForId(num2).Username);
				if (roomUserByHabbo != null)
				{
					if (roomUserByHabbo.Statusses.ContainsKey("flatctrl 1"))
					{
						roomUserByHabbo.RemoveStatus("flatctrl 1");
					}
					roomUserByHabbo.UpdateNeeded = true;
				}
			}
			CyberEnvironment.GetGame().GetGroupManager().SerializeGroupInfo(group, this.Response, this.Session, false);
			using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
			{
				queryreactor.runFastQuery(string.Concat(new object[]
				{
					"DELETE FROM group_requests WHERE group_id=",
					num,
					" AND user_id=",
					num2,
					" LIMIT 1;"
				}));
			}
		}
		internal void JoinGroup()
		{
			uint num = this.Request.PopWiredUInt();
			Guild group = CyberEnvironment.GetGame().GetGroupManager().GetGroup(num);
			if (group.Members.ContainsKey(this.Session.GetHabbo().Id))
			{
				return;
			}
			if (group.State == 0u)
			{
				using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
				{
					queryreactor.runFastQuery(string.Concat(new object[]
					{
						"INSERT INTO group_memberships (user_id, group_id) VALUES (",
						this.Session.GetHabbo().Id,
						",",
						num,
						")"
					}));
					queryreactor.runFastQuery(string.Concat(new object[]
					{
						"UPDATE user_stats SET favourite_group=",
						num,
						" WHERE id= ",
						this.Session.GetHabbo().Id,
						" LIMIT 1"
					}));
				}
				group.Members.Add(this.Session.GetHabbo().Id, new GroupUser(this.Session.GetHabbo().Id, group.Id, 0));
			}
			else
			{
				using (IQueryAdapter queryreactor2 = CyberEnvironment.GetDatabaseManager().getQueryReactor())
				{
					queryreactor2.runFastQuery(string.Concat(new object[]
					{
						"INSERT INTO group_requests (user_id, group_id) VALUES (",
						this.Session.GetHabbo().Id,
						",",
						num,
						")"
					}));
				}
				group.Requests.Add(this.Session.GetHabbo().Id);
			}
			CyberEnvironment.GetGame().GetGroupManager().SerializeGroupInfo(group, this.Response, this.Session, false);
		}
		internal void RemoveMember()
		{
			uint num = this.Request.PopWiredUInt();
			uint num2 = this.Request.PopWiredUInt();
			Guild group = CyberEnvironment.GetGame().GetGroupManager().GetGroup(num);
			if (num2 == this.Session.GetHabbo().Id)
			{
				if (group.Members.ContainsKey(num2))
				{
					group.Members.Remove(num2);
				}
				if (group.Admins.ContainsKey(num2))
				{
					group.Admins.Remove(num2);
				}
				using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
				{
					queryreactor.runFastQuery(string.Concat(new object[]
					{
						"DELETE FROM group_memberships WHERE user_id=",
						num2,
						" AND group_id=",
						num,
						" LIMIT 1"
					}));
				}
				CyberEnvironment.GetGame().GetGroupManager().SerializeGroupInfo(group, this.Response, this.Session, false);
				if (this.Session.GetHabbo().FavouriteGroup == num)
				{
					this.Session.GetHabbo().FavouriteGroup = 0u;
					using (IQueryAdapter queryreactor2 = CyberEnvironment.GetDatabaseManager().getQueryReactor())
					{
						queryreactor2.runFastQuery("UPDATE user_stats SET favourite_group=0 WHERE id=" + num2 + " LIMIT 1");
					}
					this.Response.Init(Outgoing.FavouriteGroupMessageComposer);
					this.Response.AppendUInt(this.Session.GetHabbo().Id);
					this.Session.GetHabbo().CurrentRoom.SendMessage(this.Response);
					this.Response.Init(Outgoing.ChangeFavouriteGroupMessageComposer);
					this.Response.AppendInt32(0);
					this.Response.AppendInt32(-1);
					this.Response.AppendInt32(-1);
					this.Response.AppendString("");
					this.Session.GetHabbo().CurrentRoom.SendMessage(this.Response);
					if (group.AdminOnlyDeco == 0u)
					{
						RoomUser roomUserByHabbo = CyberEnvironment.GetGame().GetRoomManager().GetRoom(group.RoomId).GetRoomUserManager().GetRoomUserByHabbo(CyberEnvironment.getHabboForId(num2).Username);
						if (roomUserByHabbo == null)
						{
							return;
						}
						roomUserByHabbo.RemoveStatus("flatctrl 1");
						this.Response.Init(Outgoing.RoomRightsLevelMessageComposer);
						this.Response.AppendInt32(0);
						roomUserByHabbo.GetClient().SendMessage(this.GetResponse());
					}
				}
				return;
			}
			if (this.Session.GetHabbo().Id != group.CreatorId || !group.Members.ContainsKey(num2))
			{
				return;
			}
			group.Members.Remove(num2);
			if (group.Admins.ContainsKey(num2))
			{
				group.Admins.Remove(num2);
			}
			CyberEnvironment.GetGame().GetGroupManager().SerializeGroupInfo(group, this.Response, this.Session, false);
			this.Response.Init(Outgoing.GroupMembersMessageComposer);
			CyberEnvironment.GetGame().GetGroupManager().SerializeGroupMembers(this.Response, group, 0u, this.Session, "", 0);
			this.SendResponse();
			using (IQueryAdapter queryreactor3 = CyberEnvironment.GetDatabaseManager().getQueryReactor())
			{
				queryreactor3.runFastQuery(string.Concat(new object[]
				{
					"DELETE FROM group_memberships WHERE group_id=",
					num,
					" AND user_id=",
					num2,
					" LIMIT 1;"
				}));
			}
		}
		internal void MakeFav()
		{
			uint groupId = this.Request.PopWiredUInt();
			Guild group = CyberEnvironment.GetGame().GetGroupManager().GetGroup(groupId);
			if (group == null)
			{
				return;
			}
			if (!group.Members.ContainsKey(this.Session.GetHabbo().Id))
			{
				Logging.WriteLine("Failed to favorite a group", ConsoleColor.Gray);
				return;
			}
			this.Session.GetHabbo().FavouriteGroup = group.Id;
			CyberEnvironment.GetGame().GetGroupManager().SerializeGroupInfo(group, this.Response, this.Session, false);
			using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
			{
				queryreactor.runFastQuery(string.Concat(new object[]
				{
					"UPDATE user_stats SET favourite_group =",
					group.Id,
					" WHERE id=",
					this.Session.GetHabbo().Id,
					" LIMIT 1;"
				}));
			}
			this.Response.Init(Outgoing.FavouriteGroupMessageComposer);
			this.Response.AppendUInt(this.Session.GetHabbo().Id);
			this.Session.GetHabbo().CurrentRoom.SendMessage(this.Response);
			if (!this.Session.GetHabbo().CurrentRoom.LoadedGroups.ContainsKey(group.Id))
			{
				this.Session.GetHabbo().CurrentRoom.LoadedGroups.Add(group.Id, group.Badge);
				this.Response.Init(Outgoing.RoomGroupMessageComposer);
				this.Response.AppendInt32(this.Session.GetHabbo().CurrentRoom.LoadedGroups.Count);
				foreach (KeyValuePair<uint, string> current in this.Session.GetHabbo().CurrentRoom.LoadedGroups)
				{
					this.Response.AppendUInt(current.Key);
					this.Response.AppendString(current.Value);
				}
				this.Session.GetHabbo().CurrentRoom.SendMessage(this.Response);
			}
			this.Response.Init(Outgoing.ChangeFavouriteGroupMessageComposer);
			this.Response.AppendInt32(0);
			this.Response.AppendUInt(group.Id);
			this.Response.AppendInt32(3);
			this.Response.AppendString(group.Name);
			this.Session.GetHabbo().CurrentRoom.SendMessage(this.Response);
		}
		internal void RemoveFav()
		{
			this.Request.PopWiredUInt();
			this.Session.GetHabbo().FavouriteGroup = 0u;
			using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
			{
				queryreactor.runFastQuery("UPDATE user_stats SET favourite_group=0 WHERE id=" + this.Session.GetHabbo().Id + " LIMIT 1;");
			}
			this.Response.Init(Outgoing.FavouriteGroupMessageComposer);
			this.Response.AppendUInt(this.Session.GetHabbo().Id);

            if (Session.GetHabbo().CurrentRoom != null)
            {
                Session.GetHabbo().CurrentRoom.SendMessage(this.Response);
            }
			this.Response.Init(Outgoing.ChangeFavouriteGroupMessageComposer);
			this.Response.AppendInt32(0);
			this.Response.AppendInt32(-1);
			this.Response.AppendInt32(-1);
			this.Response.AppendString("");
            if (Session.GetHabbo().CurrentRoom != null)
            {
                Session.GetHabbo().CurrentRoom.SendMessage(this.Response);
            }
		}

       

        internal void PublishForumThread()
        {
            if ((CyberEnvironment.GetUnixTimestamp() - Session.GetHabbo().lastSqlQuery) < 20)
            {
                Session.SendNotif("Please wait 20 seconds before publishing the next post!");
                return;
            }

            uint GroupId = Request.PopWiredUInt();
            uint ThreadId = Request.PopWiredUInt();
            string Subject = Request.PopFixedString();
            string Content = Request.PopFixedString();

            Guild Group = CyberEnvironment.GetGame().GetGroupManager().GetGroup(GroupId);
            
            if (Group == null || !Group.HasForum)
            {
                return;
            }

            int Timestamp = CyberEnvironment.GetUnixTimestamp();

            using (IQueryAdapter dbClient = CyberEnvironment.GetDatabaseManager().getQueryReactor())
            {
                if (ThreadId != 0)
                {
                    dbClient.setQuery("SELECT * FROM group_forum_posts WHERE id = " + ThreadId);
                    DataRow Row = dbClient.getRow();
                    GroupForumPost Post = new GroupForumPost(Row);

                    if (Post.Locked || Post.Hidden)
                    {
                        Session.SendNotif("¡No puedes publicar a este tema porque está oculto y/o bloqueado! Por favor, haz clic en 'Cancelar'.");
                        return;
                    }
                }

                this.Session.GetHabbo().lastSqlQuery = CyberEnvironment.GetUnixTimestamp();
                dbClient.setQuery("INSERT INTO group_forum_posts (group_id, parent_id, timestamp, poster_id, poster_name, poster_look, subject, post_content) VALUES (@gid, @pard, @ts, @pid, @pnm, @plk, @subjc, @content)");
                dbClient.addParameter("gid", GroupId);
                dbClient.addParameter("pard", ThreadId);
                dbClient.addParameter("ts", Timestamp);
                dbClient.addParameter("pid", Session.GetHabbo().Id);
                dbClient.addParameter("pnm", Session.GetHabbo().Username);
                dbClient.addParameter("plk", Session.GetHabbo().Look);
                dbClient.addParameter("subjc", Subject);
                dbClient.addParameter("content", Content);
                ThreadId = (uint)dbClient.getInteger();
            }

            Group.ForumScore += 0.25;
            Group.ForumLastPosterName = Session.GetHabbo().Username;
            Group.ForumLastPosterId = Session.GetHabbo().Id;
            Group.ForumLastPosterTimestamp = Timestamp;
            Group.ForumMessagesCount++;
            Group.UpdateForum();

            if (ThreadId == 0)
            {
                ServerMessage Message = new ServerMessage(Outgoing.GroupForumNewThreadMessageComposer);
                Message.AppendUInt(GroupId);
                Message.AppendUInt(ThreadId);
                Message.AppendUInt(Session.GetHabbo().Id);
                Message.AppendString(Subject);
                Message.AppendString(Content);
                Message.AppendBoolean(false);
                Message.AppendBoolean(false);
                Message.AppendInt32((CyberEnvironment.GetUnixTimestamp() - Timestamp));
                Message.AppendInt32(1);
                Message.AppendInt32(0);
                Message.AppendInt32(0);//readtimes?
                Message.AppendInt32(1);
                Message.AppendString("");
                Message.AppendInt32((CyberEnvironment.GetUnixTimestamp() - Timestamp));
                Message.AppendByte(1);
                Message.AppendInt32(1);
                Message.AppendString("");
                Message.AppendInt32(0);//Unknown, but unused. Parece ID
                Session.SendMessage(Message);
            }
            else
            {
                ServerMessage Message = new ServerMessage(Outgoing.GroupForumNewResponseMessageComposer);
                Message.AppendUInt(GroupId);
                Message.AppendUInt(ThreadId);
                Message.AppendUInt(Group.ForumMessagesCount);//something quick
                Message.AppendInt32(0);//something quick
                Message.AppendUInt(Session.GetHabbo().Id);
                Message.AppendString(Session.GetHabbo().Username);
                Message.AppendString(Session.GetHabbo().Look);
                Message.AppendInt32((CyberEnvironment.GetUnixTimestamp() - Timestamp));
                Message.AppendString(Content);
                Message.AppendByte(0);
                Message.AppendInt32(0);
                Message.AppendString("");
                Message.AppendInt32(0);//Unknown, but unused. Parece ID
                Session.SendMessage(Message);
            }
        }

        internal void UpdateThreadState()
        {
            uint GroupId = Request.PopWiredUInt();
            uint ThreadId = Request.PopWiredUInt();
			bool Pin = Request.PopWiredBoolean();
            bool Lock = Request.PopWiredBoolean();


            using (IQueryAdapter dbClient = CyberEnvironment.GetDatabaseManager().getQueryReactor())
            {
                dbClient.setQuery("SELECT * FROM group_forum_posts WHERE group_id = '" + GroupId + "' AND id = '" + ThreadId + "' LIMIT 1;");
                DataRow Row = dbClient.getRow();
                Guild Group = CyberEnvironment.GetGame().GetGroupManager().GetGroup(GroupId);
                if (Row != null)
                {
                    if ((uint)Row["poster_id"] == Session.GetHabbo().Id || Group.Admins.ContainsKey(Session.GetHabbo().Id))
                    {
                        dbClient.setQuery("UPDATE group_forum_posts SET pinned = @pin , locked = @lock WHERE id = " + ThreadId + ";");
                        dbClient.addParameter("pin", (Pin) ? "1" : "0");
						dbClient.addParameter("lock", (Lock) ? "1" : "0");
                        dbClient.runQuery();
                    }
                }

                GroupForumPost Thread = new GroupForumPost(Row);

                if (Thread.Pinned != Pin)
                {
                    ServerMessage Notif = new ServerMessage(Outgoing.SuperNotificationMessageComposer);
                    Notif.AppendString((Pin) ? "forums.thread.pinned" : "forums.thread.unpinned");
                    Notif.AppendInt32(0);
                    Session.SendMessage(Notif);
                }
                if (Thread.Locked != Lock)
                {
                    ServerMessage Notif2 = new ServerMessage(Outgoing.SuperNotificationMessageComposer);
                    Notif2.AppendString((Lock) ? "forums.thread.locked" : "forums.thread.unlocked");
                    Notif2.AppendInt32(0);
                    Session.SendMessage(Notif2);
                }
					

                if (Thread.ParentId != 0)
                {
                    return;
                }
                ServerMessage Message = new ServerMessage(Outgoing.GroupForumThreadUpdateMessageComposer);
                Message.AppendUInt(GroupId);
                Message.AppendUInt(Thread.Id);
                Message.AppendUInt(Thread.PosterId);
                Message.AppendString(Thread.PosterName);
                Message.AppendString(Thread.Subject);
                Message.AppendBoolean(Pin);
                Message.AppendBoolean(Lock);
                Message.AppendInt32((CyberEnvironment.GetUnixTimestamp() - Thread.Timestamp));
                Message.AppendInt32(Thread.MessageCount + 1);
                Message.AppendInt32(0);
                Message.AppendInt32(0);//readtimes?
                Message.AppendInt32(1);
                Message.AppendString("");
                Message.AppendInt32((CyberEnvironment.GetUnixTimestamp() - Thread.Timestamp));
                Message.AppendByte((Thread.Hidden) ? 10 : 1);
                Message.AppendInt32(1);
                Message.AppendString(Thread.Hider); // if someone
                Message.AppendInt32(0);//Unknown, but unused. Parece ID
                Session.SendMessage(Message);
            }
        }


        internal void AlterForumThreadState()
        {
            uint GroupId = Request.PopWiredUInt();
            uint ThreadId = Request.PopWiredUInt();
            int StateToSet = Request.PopWiredInt32();


            using (IQueryAdapter dbClient = CyberEnvironment.GetDatabaseManager().getQueryReactor())
            {
                dbClient.setQuery("SELECT * FROM group_forum_posts WHERE group_id = '" + GroupId + "' AND id = '" + ThreadId + "' LIMIT 1;");
                DataRow Row = dbClient.getRow();
                Guild Group = CyberEnvironment.GetGame().GetGroupManager().GetGroup(GroupId);
                if (Row != null)
                {
                    if ((uint)Row["poster_id"] == Session.GetHabbo().Id || Group.Admins.ContainsKey(Session.GetHabbo().Id))
                    {
                        dbClient.setQuery("UPDATE group_forum_posts SET hidden = @hid WHERE id = " + ThreadId + ";");
                        dbClient.addParameter("hid", (StateToSet == 10) ? "1" : "0");
                        dbClient.runQuery();
                    }
                }

                GroupForumPost Thread = new GroupForumPost(Row);

                ServerMessage Notif = new ServerMessage(Outgoing.SuperNotificationMessageComposer);
                Notif.AppendString((StateToSet == 10) ? "forums.thread.hidden" : "forums.thread.restored");
                    Notif.AppendInt32(0);
                    Session.SendMessage(Notif);

                if (Thread.ParentId != 0)
                {
                    return;
                }
                ServerMessage Message = new ServerMessage(Outgoing.GroupForumThreadUpdateMessageComposer);
                Message.AppendUInt(GroupId);
                Message.AppendUInt(Thread.Id);
                Message.AppendUInt(Thread.PosterId);
                Message.AppendString(Thread.PosterName);
                Message.AppendString(Thread.Subject);
                Message.AppendBoolean(Thread.Pinned);
                Message.AppendBoolean(Thread.Locked);
                Message.AppendInt32((CyberEnvironment.GetUnixTimestamp() - Thread.Timestamp));
                Message.AppendInt32(Thread.MessageCount + 1);
                Message.AppendInt32(0);
                Message.AppendInt32(0);//readtimes?
                Message.AppendInt32(0);
                Message.AppendString("");
                Message.AppendInt32((CyberEnvironment.GetUnixTimestamp() - Thread.Timestamp));
                Message.AppendByte(StateToSet);
                Message.AppendInt32(0);
                Message.AppendString(Thread.Hider);
                Message.AppendInt32(0);//Unknown, but unused. Parece ID
                Session.SendMessage(Message);
            }
        }

        internal void ReadForumThread()
        {
            uint GroupId = Request.PopWiredUInt();
            uint ThreadId = Request.PopWiredUInt();
            int StartIndex = Request.PopWiredInt32();
            int EndIndex = Request.PopWiredInt32();

            Guild Group = CyberEnvironment.GetGame().GetGroupManager().GetGroup(GroupId);
            if (Group == null || !Group.HasForum)
            {
                return;
            }

            using (IQueryAdapter dbClient = CyberEnvironment.GetDatabaseManager().getQueryReactor())
            {
                dbClient.setQuery("SELECT * FROM group_forum_posts WHERE group_id = '" + GroupId + "' AND parent_id = '" + ThreadId + "' OR id = '" + ThreadId + "' ORDER BY timestamp ASC;");
                DataTable Table = dbClient.getTable();

                if (Table == null)
                {
                    return;
                }

                int b = (Table.Rows.Count <= 20) ? Table.Rows.Count : 20;
                List<GroupForumPost> posts = new List<GroupForumPost>();
                int i = 1;
                while (i <= b)
                {
                    DataRow Row = Table.Rows[i - 1];
                    if (Row == null)
                    {
                        b--;
                        continue;
                    }
                    GroupForumPost thread = new GroupForumPost(Row);
                    if (thread.ParentId == 0 && thread.Hidden)
                    {
                        return;//
                    }

                    posts.Add(thread);
                    i++;
                }

                ServerMessage Message = new ServerMessage(Outgoing.GroupForumReadThreadMessageComposer);
                Message.AppendUInt(GroupId);
                Message.AppendUInt(ThreadId);
                Message.AppendInt32(StartIndex);
                Message.AppendInt32(b);

                int indx = 0;

                foreach (GroupForumPost Post in posts)
                {
                    Message.AppendInt32(indx++ - 1);
                    Message.AppendInt32(indx - 1);
                    Message.AppendUInt(Post.PosterId);
                    Message.AppendString(Post.PosterName);
                    Message.AppendString(Post.PosterLook);
                    Message.AppendInt32((CyberEnvironment.GetUnixTimestamp() - Post.Timestamp));
                    Message.AppendString(Post.PostContent);
                    Message.AppendByte(0);
                    Message.AppendInt32(0);
                    Message.AppendString(Post.Hider);
                    Message.AppendInt32(0);//Unknown, but unused. Parece ID
                }
                Session.SendMessage(Message);
            }
        }

        internal void GetGroupForumThreadRoot()
        {
            uint GroupId = Request.PopWiredUInt();
            int StartIndex = Request.PopWiredInt32();
            int EndIndex = Request.PopWiredInt32(); // igual siempre será 20.

            Guild Group = CyberEnvironment.GetGame().GetGroupManager().GetGroup(GroupId);
            if (Group == null || !Group.HasForum)
            {
                return;
            }

            using (IQueryAdapter dbClient = CyberEnvironment.GetDatabaseManager().getQueryReactor())
            {
                dbClient.setQuery("SELECT * FROM group_forum_posts WHERE group_id = '" + GroupId + "' AND parent_id = 0 ORDER BY timestamp DESC;");
                DataTable Table = dbClient.getTable();

                if (Table == null)
                {
                    ServerMessage Méssich = new ServerMessage(Outgoing.GroupForumThreadRootMessageComposer);
                    Méssich.AppendUInt(GroupId);
                    Méssich.AppendInt32(0);
                    Méssich.AppendInt32(0);
                    Session.SendMessage(Méssich);
                    return;
                }

                int b = (Table.Rows.Count <= 20) ? Table.Rows.Count : 20;
                List<GroupForumPost> Threads = new List<GroupForumPost>();
                int i = 1;
                while (i <= b)
                {
                    DataRow Row = Table.Rows[i - 1];
                    if (Row == null)
                    {
                        b--;
                        continue;
                    }
                    GroupForumPost thread = new GroupForumPost(Row);
                    
                    Threads.Add(thread);
                    i++;
                }
                Threads = Threads.OrderByDescending(x => x.Pinned).ToList();

                ServerMessage Message = new ServerMessage(Outgoing.GroupForumThreadRootMessageComposer);
                Message.AppendUInt(GroupId);
                Message.AppendInt32(StartIndex);
                Message.AppendInt32(b);
                foreach (GroupForumPost Thread in Threads)
                {
                    Message.AppendUInt(Thread.Id);
                    Message.AppendUInt(Thread.PosterId);
                    Message.AppendString(Thread.PosterName);
                    Message.AppendString(Thread.Subject);
                    Message.AppendBoolean(Thread.Pinned);
                    Message.AppendBoolean(Thread.Locked);
                    Message.AppendInt32((CyberEnvironment.GetUnixTimestamp() - Thread.Timestamp));
                    Message.AppendInt32(Thread.MessageCount + 1);
                    Message.AppendInt32(0);
                    Message.AppendInt32(0);//readtimes?
                    Message.AppendInt32(0);
                    Message.AppendString("");
                    Message.AppendInt32((CyberEnvironment.GetUnixTimestamp() - Thread.Timestamp));
                    Message.AppendByte((Thread.Hidden) ? 10 : 1);
                    Message.AppendInt32(0);
                    Message.AppendString(Thread.Hider);
                    Message.AppendInt32(0);//Unknown, but unused. Parece ID
                }
                Session.SendMessage(Message);
            }
        }

        internal void GetGroupForumData()
        {
            //Cyber Emulator V1.5
            uint GroupId = Request.PopWiredUInt();

            Guild Group = CyberEnvironment.GetGame().GetGroupManager().GetGroup(GroupId);
            if (Group == null || !Group.HasForum)
            {
                return;
            }

            Session.SendMessage(Group.ForumDataMessage(Session.GetHabbo().Id));
        }

        internal void GetGroupForums()
        {
            // Cyber Emulator V1.5
            int SelectType = Request.PopWiredInt32();
            int StartIndex = Request.PopWiredInt32();
            int EndIndex = Request.PopWiredInt32(); // aunque siempre 20 :P

            ServerMessage Message = new ServerMessage(Outgoing.GroupForumListingsMessageComposer);
            Message.AppendInt32(SelectType);

            if (SelectType < 0 || SelectType > 2)
            {
                Message.AppendInt32(0); // Cantidad total
                Message.AppendInt32(0);// start index
                Message.AppendInt32(0); // end index
                Session.SendMessage(Message);
                return;
            }
            List<Guild> GroupList = new List<Guild>();
            List<Guild> FinalGroupList = new List<Guild>();
            // ojalá esta mierda no cause mucho consumo

            switch (SelectType)
            {
                case 0:
                case 1:
                    //ESTO.
                    using (IQueryAdapter dbClient = CyberEnvironment.GetDatabaseManager().getQueryReactor())
                    {
                        dbClient.setQuery("SELECT id FROM groups WHERE has_forum = '1' AND forum_messages_count > 0 ORDER BY forum_messages_count DESC LIMIT 75;");
                        DataTable Table = dbClient.getTable();

                        if (Table == null)
                        {
                            return;
                        }

                        Message.AppendInt32(Table.Rows.Count);
                        Message.AppendInt32(StartIndex);

                        int b = (Table.Rows.Count <= 20) ? Table.Rows.Count : 20;
                        List<Guild> Groups = new List<Guild>();
                        int i = 1;
                        while (i <= b)
                        {
                            DataRow Row = Table.Rows[i - 1];
                            if (Row == null)
                            {
                                b--;
                                continue;
                            }
                            uint GroupId = uint.Parse(Row["id"].ToString());
                            Guild Guild = CyberEnvironment.GetGame().GetGroupManager().GetGroup(GroupId);
                            if (Guild == null || !Guild.HasForum)
                            {
                                b--;
                                continue;
                            }
                            Groups.Add(Guild);
                            i++;
                        }

                         Message.AppendInt32(b);
                        foreach (Guild Group in Groups)
                        {
                            Group.SerializeForumRoot(Message);
                        }
                        Session.SendMessage(Message);
                    }
                    break;

                case 2: // mis foros

                    foreach (GroupUser GU in Session.GetHabbo().UserGroups)
                    {
                        Guild AGroup = CyberEnvironment.GetGame().GetGroupManager().GetGroup(GU.GroupId);
                        if (AGroup == null)
                        {
                            continue;
                        }
                        else if (AGroup.HasForum)
                        {
                            GroupList.Add(AGroup);
                        }
                    }

                    try
                    {
                        FinalGroupList = GroupList.OrderByDescending(x => x.ForumMessagesCount).Skip(StartIndex).Take(20).ToList();

                        Message.AppendInt32(GroupList.Count);
                        Message.AppendInt32(StartIndex);
                        Message.AppendInt32(FinalGroupList.Count);
                    }
                    catch
                    {
                        Message.AppendInt32(0);
                        Message.AppendInt32(0);
                        Message.AppendInt32(0);
                        Session.SendMessage(Message);
                        return;
                    }

                    foreach (Guild Group in FinalGroupList)
                    {
                        Group.SerializeForumRoot(Message);
                    }
                    Session.SendMessage(Message);
                    break;
            }
        }

		internal void ManageGroup()
		{
			uint groupId = this.Request.PopWiredUInt();
			Guild group = CyberEnvironment.GetGame().GetGroupManager().GetGroup(groupId);
			if (group == null)
			{
				return;
			}
            if (group.Admins.ContainsKey(this.Session.GetHabbo().Id) || group.CreatorId == Session.GetHabbo().Id || Session.GetHabbo().Rank >= 7)
            {
                this.Response.Init(Outgoing.GroupDataEditMessageComposer);
                this.Response.AppendInt32(0);
                this.Response.AppendBoolean(true);
                this.Response.AppendUInt(group.Id);
                this.Response.AppendString(group.Name);
                this.Response.AppendString(group.Description);
                this.Response.AppendUInt(group.RoomId);
                this.Response.AppendInt32(group.Colour1);
                this.Response.AppendInt32(group.Colour2);
                this.Response.AppendUInt(group.State);
                this.Response.AppendUInt(group.AdminOnlyDeco);
                this.Response.AppendBoolean(false);
                this.Response.AppendString("");
                string[] array = group.Badge.Replace("b", "").Split('s');
                this.Response.AppendInt32(5);
                int num = checked(5 - array.Length);
                int num2 = 0;
                string[] array2 = array;
                for (int i = 0; i < array2.Length; i++)
                {
                    //Fixed by Finn. NOW GROUP BADGES 100%!
                    string text = array2[i];
                    this.Response.AppendUInt((text.Length >= 6) ? uint.Parse(text.Substring(0, 3)) : uint.Parse(text.Substring(0, 2)));
                    this.Response.AppendUInt((text.Length >= 6) ? uint.Parse(text.Substring(3, 2)) : uint.Parse(text.Substring(2, 2)));
                    if (text.Length < 5)
                    {
                        this.Response.AppendInt32(0);
                    }
                    else if (text.Length >= 6)
                    {
                        this.Response.AppendUInt(uint.Parse(text.Substring(5, 1)));
                    }
                    else
                    {
                        this.Response.AppendUInt(uint.Parse(text.Substring(4, 1)));
                    }

                }
                while (num2 != num)
                {
                    this.Response.AppendInt32(0);
                    this.Response.AppendInt32(0);
                    this.Response.AppendInt32(0);
                    num2++;
                }
                this.Response.AppendString(group.Badge);
                this.Response.AppendInt32(group.Members.Count);
                this.SendResponse();
            }
		}
		internal void UpdateGroupName()
		{
			uint num = this.Request.PopWiredUInt();
			string text = this.Request.PopFixedString();
			string text2 = this.Request.PopFixedString();
			Guild group = CyberEnvironment.GetGame().GetGroupManager().GetGroup(num);
			if (group == null)
			{
				return;
			}
			if (group.CreatorId != this.Session.GetHabbo().Id)
			{
				return;
			}
			using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
			{
				queryreactor.setQuery("UPDATE groups SET `name`=@name, `desc`=@desc WHERE id=" + num + " LIMIT 1");
				queryreactor.addParameter("name", text);
				queryreactor.addParameter("desc", text2);
				queryreactor.runQuery();
			}
			group.Name = text;
			group.Description = text2;
			CyberEnvironment.GetGame().GetGroupManager().SerializeGroupInfo(group, this.Response, this.Session, this.Session.GetHabbo().CurrentRoom, false);
		}
        internal void UpdateGroupBadge()
        {
            uint guildId = this.Request.PopWiredUInt();
            Guild guild = CyberEnvironment.GetGame().GetGroupManager().GetGroup(guildId);
            if (guild != null)
            {
                Room room = CyberEnvironment.GetGame().GetRoomManager().GetRoom(guild.RoomId);
                if (room != null)
                {
                    this.Request.PopWiredInt32();
                    int Base = this.Request.PopWiredInt32();
                    int baseColor = this.Request.PopWiredInt32();
                    this.Request.PopWiredInt32();
                    List<int> guildStates = new List<int>();

                    for (int i = 0; i < 12; i++)
                    {
                        int item = this.Request.PopWiredInt32();
                        guildStates.Add(item);
                    }
                    string badge = CyberEnvironment.GetGame().GetGroupManager().GenerateGuildImage(Base, baseColor, guildStates);
                    guild.Badge = badge;

                    this.Response.Init(Outgoing.RoomGroupMessageComposer);
                    this.Response.AppendInt32(room.LoadedGroups.Count);
                    foreach (KeyValuePair<uint, string> current2 in room.LoadedGroups)
                    {
                        this.Response.AppendUInt(current2.Key);
                        this.Response.AppendString(current2.Value);
                    }
                    room.SendMessage(this.Response);
                    CyberEnvironment.GetGame().GetGroupManager().SerializeGroupInfo(guild, this.Response, this.Session, room, false);

                    using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
                    {
                        queryreactor.setQuery("UPDATE groups SET badge = @badgi WHERE id = " + guildId);
                        queryreactor.addParameter("badgi", badge);
                        queryreactor.runQuery();
                    }

                    if (Session.GetHabbo().CurrentRoom != null)
                    {
                        this.Session.GetHabbo().CurrentRoom.LoadedGroups[guildId] = guild.Badge;
                        this.Response.Init(Outgoing.RoomGroupMessageComposer);
                        this.Response.AppendInt32(this.Session.GetHabbo().CurrentRoom.LoadedGroups.Count);
                        foreach (KeyValuePair<uint, string> current in this.Session.GetHabbo().CurrentRoom.LoadedGroups)
                        {
                            this.Response.AppendUInt(current.Key);
                            this.Response.AppendString(current.Value);
                        }
                        this.Session.GetHabbo().CurrentRoom.SendMessage(this.Response);
                        CyberEnvironment.GetGame().GetGroupManager().SerializeGroupInfo(guild, this.Response, this.Session, this.Session.GetHabbo().CurrentRoom, false);
                    }
                }
            }
        }

		internal void UpdateGroupColours()
		{
			uint groupId = this.Request.PopWiredUInt();
			int num = this.Request.PopWiredInt32();
			int num2 = this.Request.PopWiredInt32();
			Guild group = CyberEnvironment.GetGame().GetGroupManager().GetGroup(groupId);
			if (group == null)
			{
				return;
			}
			if (group.CreatorId != this.Session.GetHabbo().Id)
			{
				return;
			}
			using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
			{
				queryreactor.runFastQuery(string.Concat(new object[]
				{
					"UPDATE groups SET colour1= ",
					num,
					", colour2=",
					num2,
					" WHERE id=",
					group.Id,
					" LIMIT 1"
				}));
			}
			group.Colour1 = num;
			group.Colour2 = num2;
			CyberEnvironment.GetGame().GetGroupManager().SerializeGroupInfo(group, this.Response, this.Session, this.Session.GetHabbo().CurrentRoom, false);
		}
		internal void UpdateGroupSettings()
		{
			uint groupId = this.Request.PopWiredUInt();
			uint num = this.Request.PopWiredUInt();
			uint num2 = this.Request.PopWiredUInt();
			Guild group = CyberEnvironment.GetGame().GetGroupManager().GetGroup(groupId);
			if (group == null)
			{
				return;
			}
			if (group.CreatorId != this.Session.GetHabbo().Id)
			{
				return;
			}
			using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
			{
				queryreactor.runFastQuery(string.Concat(new object[]
				{
					"UPDATE groups SET state='",
					num,
					"', admindeco='",
					num2,
					"' WHERE id=",
					group.Id,
					" LIMIT 1"
				}));
			}
			group.State = num;
			group.AdminOnlyDeco = num2;
			Room room = CyberEnvironment.GetGame().GetRoomManager().GetRoom(group.RoomId);
			if (room != null)
			{
				foreach (RoomUser current in room.GetRoomUserManager().GetRoomUsers())
				{
					if ((long)room.OwnerId != (long)((ulong)current.UserID) && !group.Admins.ContainsKey(current.UserID) && group.Members.ContainsKey(current.UserID))
					{
						if (num2 == 1u)
						{
							current.RemoveStatus("flatctrl 1");
							this.Response.Init(Outgoing.RoomRightsLevelMessageComposer);
							this.Response.AppendInt32(0);
							current.GetClient().SendMessage(this.GetResponse());
						}
						else
						{
							if (num2 == 0u && !current.Statusses.ContainsKey("flatctrl 1"))
							{
								current.AddStatus("flatctrl 1", "");
								this.Response.Init(Outgoing.RoomRightsLevelMessageComposer);
								this.Response.AppendInt32(1);
								current.GetClient().SendMessage(this.GetResponse());
							}
						}
						current.UpdateNeeded = true;
					}
				}
			}
			CyberEnvironment.GetGame().GetGroupManager().SerializeGroupInfo(group, this.Response, this.Session, this.Session.GetHabbo().CurrentRoom, false);
		}
		internal void SerializeGroupFurniPage()
		{
            HashSet<GroupUser> userGroups = CyberEnvironment.GetGame().GetGroupManager().GetUserGroups(this.Session.GetHabbo().Id);
			this.Response.Init(Outgoing.GroupFurniturePageMessageComposer);
			this.Response.AppendInt32(userGroups.Count);
			foreach (GroupUser current in userGroups)
			{
				Guild group = CyberEnvironment.GetGame().GetGroupManager().GetGroup(current.GroupId);
				this.Response.AppendUInt(group.Id);
				this.Response.AppendString(group.Name);
				this.Response.AppendString(group.Badge);
                this.Response.AppendString(CyberEnvironment.GetGame().GetGroupManager().SymbolColours.Contains(group.Colour1) ? ((GroupSymbolColours)CyberEnvironment.GetGame().GetGroupManager().SymbolColours[group.Colour1]).Colour : "4f8a00");
                this.Response.AppendString(CyberEnvironment.GetGame().GetGroupManager().BackGroundColours.Contains(group.Colour2) ? ((GroupBackGroundColours)CyberEnvironment.GetGame().GetGroupManager().BackGroundColours[group.Colour2]).Colour : "4f8a00");
				this.Response.AppendBoolean(group.CreatorId == Session.GetHabbo().Id);
                this.Response.AppendUInt(group.CreatorId);
                this.Response.AppendBoolean(group.HasForum);
			}
			this.SendResponse();
		}
		internal void Talents()
		{
			List<Talent> talents = CyberEnvironment.GetGame().GetTalentManager().GetTalents(-1);
			bool flag = true;
			if (talents == null)
			{
				throw new NullReferenceException("La lista de talentos es NULL.");
			}
			this.Response.Init(Outgoing.TalentsTrackMessageComposer);
			this.Response.AppendString(this.Session.GetHabbo().TalentStatus);
			this.Response.AppendInt32(talents.Count);
			foreach (Talent current in talents)
			{
				this.Response.AppendInt32(current.Level);
				this.Response.AppendInt32(1);
				List<Talent> talents2 = CyberEnvironment.GetGame().GetTalentManager().GetTalents(current.Level);
				this.Response.AppendInt32(talents2.Count);
				foreach (Talent arg_CA_0 in talents2)
				{
					int num = (!flag) ? 0 : ((this.Session.GetHabbo().GetAchievementData(current.AchievementGroup) != null && this.Session.GetHabbo().GetAchievementData(current.AchievementGroup).Level >= current.AchievementLevel) ? 2 : 1);
					this.Response.AppendUInt(current.GetAchievement().Id);
					this.Response.AppendInt32(1);
					this.Response.AppendString(current.AchievementGroup + current.AchievementLevel);
					this.Response.AppendInt32(num);
					this.Response.AppendInt32((this.Session.GetHabbo().GetAchievementData(current.AchievementGroup) != null) ? this.Session.GetHabbo().GetAchievementData(current.AchievementGroup).Progress : 0);
					this.Response.AppendInt32(current.GetAchievement().Levels[current.AchievementLevel].Requirement);
					if (num != 2 && flag)
					{
						flag = false;
					}
				}
				this.Response.AppendInt32(0);
				this.Response.AppendInt32(1);
				this.Response.AppendString(current.Prize);
				this.Response.AppendInt32(0);
			}
			this.SendResponse();
		}
		internal void CompleteSafetyQuiz()
		{
			CyberEnvironment.GetGame().GetAchievementManager().ProgressUserAchievement(this.Session, "ACH_SafetyQuizGraduate", 1, false);
		}

		internal GameClientMessageHandler(GameClient Session)
		{
			this.Session = Session;
			this.Response = new ServerMessage();
		}
		internal GameClient GetSession()
		{
			return this.Session;
		}
		internal ServerMessage GetResponse()
		{
			return this.Response;
		}
		internal void Destroy()
		{
			this.Session = null;
		}
        internal void HandleRequest(ClientMessage request)
        {
            this.Request = request;
            StaticClientMessageHandler.HandlePacket(this, request);
        }
		internal void SendResponse()
		{
			if (this.Response != null && this.Response.Id > 0 && this.Session.GetConnection() != null)
			{
				this.Session.GetConnection().SendData(this.Response.GetBytes());
			}
		}
		internal void InitHelpTool()
		{
			this.Response.Init(Outgoing.OpenHelpToolMessageComposer);
			this.Response.AppendInt32(0);
			this.SendResponse();
		}

		internal void SubmitHelpTicket()
		{
			if (!CyberEnvironment.GetGame().GetModerationTool().UsersHasPendingTicket(this.Session.GetHabbo().Id))
			{
				string Message = this.Request.PopFixedString();
				int Category = this.Request.PopWiredInt32();
				uint ReportedUser = this.Request.PopWiredUInt();
                uint Room = this.Request.PopWiredUInt();

                int Messagecount = this.Request.PopWiredInt32();

                List<string> Chats = new List<string>();

                for (int i = 0; i < Messagecount; i++)
                {
                    Request.PopWiredInt32();
                    Chats.Add(Request.PopFixedString());
                }
                CyberEnvironment.GetGame().GetModerationTool().SendNewTicket(this.Session, Category, ReportedUser, Message, 7, Chats);
			}
		}

		internal void DeletePendingCFH()
		{
			if (!CyberEnvironment.GetGame().GetModerationTool().UsersHasPendingTicket(this.Session.GetHabbo().Id))
			{
				return;
			}
			CyberEnvironment.GetGame().GetModerationTool().DeletePendingTicketForUser(this.Session.GetHabbo().Id);
		}
		internal void ModGetUserInfo()
		{
			if (!this.Session.GetHabbo().HasFuse("fuse_mod"))
			{
				return;
			}
			uint num = this.Request.PopWiredUInt();
			if (CyberEnvironment.GetGame().GetClientManager().GetNameById(num) != "Unknown User")
			{
				this.Session.SendMessage(ModerationTool.SerializeUserInfo(num));
				return;
			}
			this.Session.SendNotif("No se pudo cargar la información del usuario.");
		}
		internal void ModGetUserChatlog()
		{
			if (!this.Session.GetHabbo().HasFuse("fuse_chatlogs"))
			{
				return;
			}
			this.Session.SendMessage(ModerationTool.SerializeUserChatlog(this.Request.PopWiredUInt()));
		}
		internal void ModGetRoomChatlog()
		{
			if (!this.Session.GetHabbo().HasFuse("fuse_chatlogs"))
			{
				this.Session.SendBroadcastMessage("No puedes ver chatlogs. No estás autorizado");
				return;
			}
			this.Request.PopWiredInt32();
			uint roomID = this.Request.PopWiredUInt();
			if (CyberEnvironment.GetGame().GetRoomManager().GetRoom(roomID) != null)
			{
                this.Session.SendMessage(ModerationTool.SerializeRoomChatlog(roomID));
			}
		}
		internal void ModGetRoomTool()
		{
			if (!this.Session.GetHabbo().HasFuse("fuse_mod"))
			{
				return;
			}
			uint roomId = this.Request.PopWiredUInt();
			RoomData data = CyberEnvironment.GetGame().GetRoomManager().GenerateNullableRoomData(roomId);
			this.Session.SendMessage(ModerationTool.SerializeRoomTool(data));
		}
		internal void ModPickTicket()
		{
			if (!this.Session.GetHabbo().HasFuse("fuse_mod"))
			{
				return;
			}
			this.Request.PopWiredInt32();
			uint ticketId = this.Request.PopWiredUInt();
			CyberEnvironment.GetGame().GetModerationTool().PickTicket(this.Session, ticketId);
		}
		internal void ModReleaseTicket()
		{
			if (!this.Session.GetHabbo().HasFuse("fuse_mod"))
			{
				return;
			}
			int num = this.Request.PopWiredInt32();
			checked
			{
				for (int i = 0; i < num; i++)
				{
					uint ticketId = this.Request.PopWiredUInt();
					CyberEnvironment.GetGame().GetModerationTool().ReleaseTicket(this.Session, ticketId);
				}
			}
		}
		internal void ModCloseTicket()
		{
			if (!this.Session.GetHabbo().HasFuse("fuse_mod"))
			{
				return;
			}
			int result = this.Request.PopWiredInt32();
			this.Request.PopWiredInt32();
			uint ticketId = this.Request.PopWiredUInt();
			CyberEnvironment.GetGame().GetModerationTool().CloseTicket(this.Session, ticketId, result);
		}
		internal void ModGetTicketChatlog()
		{
			if (!this.Session.GetHabbo().HasFuse("fuse_mod"))
			{
				return;
			}
			SupportTicket ticket = CyberEnvironment.GetGame().GetModerationTool().GetTicket(this.Request.PopWiredUInt());
			if (ticket == null)
			{
				return;
			}
			RoomData roomData = CyberEnvironment.GetGame().GetRoomManager().GenerateNullableRoomData(ticket.RoomId);
			if (roomData == null)
			{
				return;
			}
			this.Session.SendMessage(ModerationTool.SerializeTicketChatlog(ticket, roomData, ticket.Timestamp));
		}
		internal void ModGetRoomVisits()
		{
			if (!this.Session.GetHabbo().HasFuse("fuse_mod"))
			{
				return;
			}
			uint userId = this.Request.PopWiredUInt();
			this.Session.SendMessage(ModerationTool.SerializeRoomVisits(userId));
		}
		internal void ModSendRoomAlert()
		{
			if (!this.Session.GetHabbo().HasFuse("fuse_alert"))
			{
				return;
			}
			this.Request.PopWiredInt32();
			string str = this.Request.PopFixedString();
			ServerMessage serverMessage = new ServerMessage(Outgoing.SuperNotificationMessageComposer);
			serverMessage.AppendString("admin");
			serverMessage.AppendInt32(3);
			serverMessage.AppendString("message");
			serverMessage.AppendString(str + "\r\n\r\n- " + this.Session.GetHabbo().Username);
            serverMessage.AppendString("link");
            serverMessage.AppendString("event:");
            serverMessage.AppendString("linkTitle");
            serverMessage.AppendString("ok");
			this.Session.GetHabbo().CurrentRoom.SendMessage(serverMessage);
		}
		internal void ModPerformRoomAction()
		{
			if (!this.Session.GetHabbo().HasFuse("fuse_mod"))
			{
				return;
			}
			uint roomId = this.Request.PopWiredUInt();
			bool lockRoom = this.Request.PopWiredInt32() == 1;
			bool inappropriateRoom = this.Request.PopWiredInt32() == 1;
			bool kickUsers = this.Request.PopWiredInt32() == 1;
			ModerationTool.PerformRoomAction(this.Session, roomId, kickUsers, lockRoom, inappropriateRoom, this.Response);
		}
		internal void ModSendUserCaution()
		{
			if (!this.Session.GetHabbo().HasFuse("fuse_alert"))
			{
				return;
			}
			uint userId = this.Request.PopWiredUInt();
			string message = this.Request.PopFixedString();
			ModerationTool.AlertUser(this.Session, userId, message, true);
		}
		internal void ModSendUserMessage()
		{
			if (!this.Session.GetHabbo().HasFuse("fuse_alert"))
			{
				return;
			}
			uint userId = this.Request.PopWiredUInt();
			string message = this.Request.PopFixedString();
			ModerationTool.AlertUser(this.Session, userId, message, false);
		}
		internal void ModMuteUser()
		{
			if (!this.Session.GetHabbo().HasFuse("fuse_mute"))
			{
				return;
			}
			uint userID = this.Request.PopWiredUInt();
			string message = this.Request.PopFixedString();
			GameClient clientByUserID = CyberEnvironment.GetGame().GetClientManager().GetClientByUserID(userID);
			clientByUserID.GetHabbo().Mute();
			clientByUserID.SendNotif(message);
		}
		internal void ModLockTrade()
		{
			if (!this.Session.GetHabbo().HasFuse("fuse_lock_trade"))
			{
				return;
			}
			uint userId = this.Request.PopWiredUInt();
			string message = this.Request.PopFixedString();
			int length = checked(this.Request.PopWiredInt32() * 3600);
			ModerationTool.LockTrade(this.Session, userId, message, length);
		}
		internal void ModKickUser()
		{
			if (!this.Session.GetHabbo().HasFuse("fuse_kick"))
			{
				return;
			}
			uint userId = this.Request.PopWiredUInt();
			string message = this.Request.PopFixedString();
			ModerationTool.KickUser(this.Session, userId, message, false);
		}
		internal void ModBanUser()
		{
			if (!this.Session.GetHabbo().HasFuse("fuse_ban"))
			{
				return;
			}
			uint userId = this.Request.PopWiredUInt();
			string message = this.Request.PopFixedString();
			int length = checked(this.Request.PopWiredInt32() * 3600);
			ModerationTool.BanUser(this.Session, userId, length, message);
		}
		internal void GoRoom()
		{
			if (this.Session.GetHabbo() == null)
			{
				return;
			}
			uint num = this.Request.PopWiredUInt();
			RoomData roomData = CyberEnvironment.GetGame().GetRoomManager().GenerateRoomData(num);
			this.Session.GetHabbo().GetInventoryComponent().RunDBUpdate();
			if (roomData == null || roomData.Id == this.Session.GetHabbo().CurrentRoomId)
			{
				return;
			}
			roomData.SerializeRoomData(this.Response, !this.Session.GetHabbo().InRoom, this.Session, false);
			this.PrepareRoomForUser(num, roomData.Password);
		}
		internal void InitMessenger()
		{
			this.Session.GetHabbo().InitMessenger();
		}
		internal void FriendsListUpdate()
		{
			this.Session.GetHabbo().GetMessenger();
		}
		internal void RemoveBuddy()
		{
			if (this.Session.GetHabbo().GetMessenger() == null)
			{
				return;
			}
			int num = this.Request.PopWiredInt32();
			checked
			{
				for (int i = 0; i < num; i++)
				{
					uint num2 = this.Request.PopWiredUInt();
					if (this.Session.GetHabbo().Relationships.ContainsKey(Convert.ToInt32(num2)))
					{
						this.Session.SendNotif("Could not remove someone, they are set to your relationship!");
					}
					else
					{
						this.Session.GetHabbo().GetMessenger().DestroyFriendship(num2);
					}
				}
			}
		}
		internal void SearchHabbo()
		{
			if (this.Session.GetHabbo().GetMessenger() == null)
			{
				return;
			}
			this.Session.SendMessage(this.Session.GetHabbo().GetMessenger().PerformSearch(this.Request.PopFixedString()));
		}
		internal void AcceptRequest()
		{
			if (this.Session.GetHabbo().GetMessenger() == null)
			{
				return;
			}
			int num = this.Request.PopWiredInt32();
			checked
			{
				for (int i = 0; i < num; i++)
				{
					uint num2 = this.Request.PopWiredUInt();
					MessengerRequest request = this.Session.GetHabbo().GetMessenger().GetRequest(num2);
					if (request != null)
					{
						if (request.To != this.Session.GetHabbo().Id)
						{
							return;
						}
						if (!this.Session.GetHabbo().GetMessenger().FriendshipExists(request.To))
						{
							this.Session.GetHabbo().GetMessenger().CreateFriendship(request.From);
						}
						this.Session.GetHabbo().GetMessenger().HandleRequest(num2);
					}
				}
			}
		}
		internal void DeclineRequest()
		{
			if (this.Session.GetHabbo().GetMessenger() == null)
			{
				return;
			}
			bool flag = this.Request.PopWiredBoolean();
			this.Request.PopWiredInt32();
			if (!flag)
			{
				uint sender = this.Request.PopWiredUInt();
				this.Session.GetHabbo().GetMessenger().HandleRequest(sender);
				return;
			}
			this.Session.GetHabbo().GetMessenger().HandleAllRequests();
		}
		internal void RequestBuddy()
		{
			if (this.Session.GetHabbo().GetMessenger() == null)
			{
				return;
			}
			if (this.Session.GetHabbo().GetMessenger().RequestBuddy(this.Request.PopFixedString()))
			{
				CyberEnvironment.GetGame().GetQuestManager().ProgressUserQuest(this.Session, QuestType.SOCIAL_FRIEND, 0u);
			}
		}
		internal void SendInstantMessenger()
		{
			uint toId = this.Request.PopWiredUInt();
			string text = this.Request.PopFixedString();
			if (this.Session.GetHabbo().GetMessenger() == null)
			{
				return;
			}
			if (!string.IsNullOrWhiteSpace(text))
			{
				this.Session.GetHabbo().GetMessenger().SendInstantMessage(toId, text);
			}
		}
		internal void FollowBuddy()
		{
			uint userID = this.Request.PopWiredUInt();
			GameClient clientByUserID = CyberEnvironment.GetGame().GetClientManager().GetClientByUserID(userID);

            if (clientByUserID != null && clientByUserID.GetHabbo() != null)
            {
                if (clientByUserID.GetHabbo().GetMessenger() == null || clientByUserID.GetHabbo().CurrentRoom == null)
                {
                    if (Session.GetHabbo().GetMessenger() != null)
                    {
                        this.Response.Init(Outgoing.FollowFriendErrorMessageComposer);
                        this.Response.AppendInt32(2);
                        this.SendResponse();
                        Session.GetHabbo().GetMessenger().UpdateFriend(userID, clientByUserID, true);
                    }
                    return;
                }
                else if (Session.GetHabbo().GetMessenger() != null && !Session.GetHabbo().GetMessenger().FriendshipExists(userID))
                {
                    this.Response.Init(Outgoing.FollowFriendErrorMessageComposer);
                    this.Response.AppendInt32(0);
                    this.SendResponse();
                    return;
                }

                ServerMessage roomFwd = new ServerMessage(Outgoing.RoomForwardMessageComposer);
                roomFwd.AppendUInt(clientByUserID.GetHabbo().CurrentRoom.RoomId);
                this.Session.SendMessage(roomFwd);
            }
		}
		internal void SendInstantInvite()
		{
			int num = this.Request.PopWiredInt32();
			List<uint> list = new List<uint>();
			checked
			{
				for (int i = 0; i < num; i++)
				{
					list.Add(this.Request.PopWiredUInt());
				}
                string s = this.Request.PopFixedString();
				ServerMessage serverMessage = new ServerMessage(Outgoing.ConsoleInvitationMessageComposer);
				serverMessage.AppendUInt(this.Session.GetHabbo().Id);
				serverMessage.AppendString(s);
				foreach (uint current in list)
				{
					if (this.Session.GetHabbo().GetMessenger().FriendshipExists(current))
					{
						GameClient clientByUserID = CyberEnvironment.GetGame().GetClientManager().GetClientByUserID(current);
						if (clientByUserID == null)
						{
							break;
						}
						clientByUserID.SendMessage(serverMessage);
					}
				}
			}
		}
		internal void AddFavorite()
		{
			if (this.Session.GetHabbo() == null)
			{
				return;
			}
			uint num = this.Request.PopWiredUInt();

			this.GetResponse().Init(Outgoing.FavouriteRoomsUpdateMessageComposer);
			this.GetResponse().AppendUInt(num);
			this.GetResponse().AppendBoolean(true);
			this.SendResponse();

			this.Session.GetHabbo().FavoriteRooms.Add(num);
			using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
			{
				queryreactor.runFastQuery(string.Concat(new object[]
				{
					"INSERT INTO user_favorites (user_id,room_id) VALUES (",
					this.Session.GetHabbo().Id,
					",",
					num,
					")"
				}));
			}
		}
		internal void AddStaffPick()
		{
			this.Session.SendNotif("You do not have permission to do that yet!");
		}
		internal void RemoveFavorite()
		{
			if (this.Session.GetHabbo() == null)
			{
				return;
			}
			uint num = this.Request.PopWiredUInt();
			this.Session.GetHabbo().FavoriteRooms.Remove(num);
			this.GetResponse().Init(Outgoing.FavouriteRoomsUpdateMessageComposer);
			this.GetResponse().AppendUInt(num);
			this.GetResponse().AppendBoolean(false);
			this.SendResponse();
			using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
			{
				queryreactor.runFastQuery(string.Concat(new object[]
				{
					"DELETE FROM user_favorites WHERE user_id = ",
					this.Session.GetHabbo().Id,
					" AND room_id = ",
					num
				}));
			}
		}
		internal void OnlineConfirmationEvent()
		{
			Logging.WriteLine("[UserMgr] [ " + this.Request.PopFixedString() + " ] is now online.", ConsoleColor.DarkGreen);
		}
		internal void RetrieveCitizenship()
		{
			this.GetResponse().Init(Outgoing.CitizenshipStatusMessageComposer);
			this.GetResponse().AppendString(this.Request.PopFixedString());
			this.GetResponse().AppendInt32(4);
			this.GetResponse().AppendInt32(4);
		}
		internal void GoToHotelView()
		{
			HotelView hotelView = CyberEnvironment.GetGame().GetHotelView();
			if (hotelView.FurniRewardName != null)
			{
				ServerMessage serverMessage = new ServerMessage(Outgoing.LandingRewardMessageComposer);
				serverMessage.AppendString(hotelView.FurniRewardName);
				serverMessage.AppendInt32(hotelView.FurniRewardId);
				serverMessage.AppendInt32(120);
				serverMessage.AppendInt32(checked(120 - this.Session.GetHabbo().Respect));
				this.Session.SendMessage(serverMessage);
			}
			if (this.Session == null || this.Session.GetHabbo() == null)
			{
				return;
			}
			if (this.Session.GetHabbo().InRoom)
			{
				Room room = CyberEnvironment.GetGame().GetRoomManager().GetRoom(this.Session.GetHabbo().CurrentRoomId);
				if (room != null)
				{
					room.GetRoomUserManager().RemoveUserFromRoom(this.Session, true, false);
				}
				this.Session.CurrentRoomUserID = -1;
			}
		}

        internal void LandingCommunityGoal()
        {
            int onlineFriends = Session.GetHabbo().GetMessenger().friends.Where(x => x.Value.IsOnline).Count();
            ServerMessage GoalMeter = new ServerMessage(Outgoing.LandingCommunityChallengeMessageComposer);
            GoalMeter.AppendBoolean(true);//
            GoalMeter.AppendInt32(0);//points
            GoalMeter.AppendInt32(0);//my rank
            GoalMeter.AppendInt32(onlineFriends);//totalAmount
            GoalMeter.AppendInt32(onlineFriends >= 20 ? 1 : onlineFriends >= 50 ? 2 : onlineFriends >= 80 ? 3 : 0);//communityHighestAchievedLevel
            GoalMeter.AppendInt32(0);//scoreRemainingUntilNextLevel
            GoalMeter.AppendInt32(0);//percentCompletionTowardsNextLevel
            GoalMeter.AppendString("friendshipChallenge");//Type
            GoalMeter.AppendInt32(0);//unknown
            GoalMeter.AppendInt32(0);//ranks and loop
            Session.SendMessage(GoalMeter);
        }

		internal void WidgetContainers()
		{
			string text = this.Request.PopFixedString();
			ServerMessage serverMessage = new ServerMessage(Outgoing.LandingWidgetMessageComposer);
			if (!string.IsNullOrEmpty(text))
			{
				string[] array = text.Split(new char[]
				{
					','
				});
				if (array[1] == "gamesmaker")
				{
					return;
				}
				serverMessage.AppendString(text);
				serverMessage.AppendString(array[1]);
			}
			else
			{
				serverMessage.AppendString("");
				serverMessage.AppendString("");
			}
			this.Session.SendMessage(serverMessage);
		}
		internal void RefreshPromoEvent()
		{
			HotelView hotelView = CyberEnvironment.GetGame().GetHotelView();
			if (this.Session == null || this.Session.GetHabbo() == null)
			{
				return;
			}
			if (hotelView.HotelViewPromosIndexers.Count <= 0)
			{
				return;
			}
			ServerMessage message = hotelView.SmallPromoComposer(new ServerMessage(Outgoing.LandingPromosMessageComposer));
			this.Session.SendMessage(message);
		}
		internal void GetFlatCats()
		{
			if (this.Session.GetHabbo() == null)
			{
				return;
			}
			this.Session.SendMessage(CyberEnvironment.GetGame().GetNavigator().SerializeFlatCategories(this.Session));
		}
		internal void EnterInquiredRoom()
		{
		}
		internal void GetPubs()
		{
			if (this.Session.GetHabbo() == null)
			{
				return;
			}
			this.Session.SendMessage(CyberEnvironment.GetGame().GetNavigator().SerializePublicRooms());
		}
		internal void GetRoomInfo()
		{
			if (this.Session.GetHabbo() == null)
			{
				return;
			}
			uint roomId = this.Request.PopWiredUInt();
			this.Request.PopWiredBoolean();
			this.Request.PopWiredBoolean();
			RoomData roomData = CyberEnvironment.GetGame().GetRoomManager().GenerateRoomData(roomId);
			if (roomData == null)
			{
				return;
			}
			this.GetResponse().Init(1491);
			this.GetResponse().AppendInt32(0);
			roomData.Serialize(this.GetResponse(), false);
			this.SendResponse();
		}
		internal void GetPopularRooms()
		{
			if (this.Session.GetHabbo() == null)
			{
				return;
			}
			this.Session.SendMessage(CyberEnvironment.GetGame().GetNavigator().SerializeNavigator(this.Session, int.Parse(this.Request.PopFixedString())));
		}
        internal void GetRecommendedRooms()
        {
            if (this.Session.GetHabbo() == null)
            {
                return;
            }
            this.Session.SendMessage(CyberEnvironment.GetGame().GetNavigator().SerializeNavigator(this.Session, -1));
        }
		internal void GetPopularGroups()
		{
			if (this.Session.GetHabbo() == null)
			{
				return;
			}
			this.Session.SendMessage(CyberEnvironment.GetGame().GetNavigator().SerializeNavigator(this.Session, -14));
		}
		internal void GetHighRatedRooms()
		{
			if (this.Session.GetHabbo() == null)
			{
				return;
			}
			this.Session.SendMessage(CyberEnvironment.GetGame().GetNavigator().SerializeNavigator(this.Session, -2));
		}
		internal void GetFriendsRooms()
		{
			if (this.Session.GetHabbo() == null)
			{
				return;
			}
			this.Session.SendMessage(CyberEnvironment.GetGame().GetNavigator().SerializeNavigator(this.Session, -4));
		}
		internal void GetRoomsWithFriends()
		{
			if (this.Session.GetHabbo() == null)
			{
				return;
			}
			this.Session.SendMessage(CyberEnvironment.GetGame().GetNavigator().SerializeNavigator(this.Session, -5));
		}
		internal void GetOwnRooms()
		{
			if (this.Session.GetHabbo() == null)
			{
				return;
			}
			this.Session.SendMessage(CyberEnvironment.GetGame().GetNavigator().SerializeNavigator(this.Session, -3));
		}
        internal void NewNavigatorFlatCats()
        {
            if (this.Session.GetHabbo() == null)
            {
                return;
            }
            this.Session.SendMessage(CyberEnvironment.GetGame().GetNavigator().SerializeNewFlatCategories());
        }
		internal void GetFavoriteRooms()
		{
			if (this.Session.GetHabbo() == null)
			{
				return;
			}
			this.Session.SendMessage(CyberEnvironment.GetGame().GetNavigator().SerializeFavoriteRooms(this.Session));
		}
		internal void GetRecentRooms()
		{
			if (this.Session.GetHabbo() == null)
			{
				return;
			}
			this.Session.SendMessage(CyberEnvironment.GetGame().GetNavigator().SerializeRecentRooms(this.Session));
		}
		internal void GetPopularTags()
		{
			if (this.Session.GetHabbo() == null)
			{
				return;
			}
			this.Session.SendMessage(CyberEnvironment.GetGame().GetNavigator().SerializePopularRoomTags());
		}
		internal void GetEventRooms()
		{
			this.Session.SendMessage(CyberEnvironment.GetGame().GetNavigator().SerializeNavigator(this.Session, -16));
		}
		internal void PerformSearch()
		{
			if (this.Session.GetHabbo() == null)
			{
				return;
			}
			this.Session.SendMessage(CyberEnvironment.GetGame().GetNavigator().SerializeSearchResults(this.Request.PopFixedString()));
		}

        internal void SearchByTag()
        {
            if (this.Session.GetHabbo() == null)
            {
                return;
            }
            this.Session.SendMessage(CyberEnvironment.GetGame().GetNavigator().SerializeSearchResults("tag:" + this.Request.PopFixedString()));
        }
		internal void PerformSearch2()
		{
			if (this.Session.GetHabbo() == null)
			{
				return;
			}
			this.Request.PopWiredInt32();
			this.Session.SendMessage(CyberEnvironment.GetGame().GetNavigator().SerializeSearchResults(this.Request.PopFixedString()));
		}
		internal void OpenFlat()
		{
			if (this.Session.GetHabbo() == null)
			{
				return;
			}
			uint num = this.Request.PopWiredUInt();
			string password = this.Request.PopFixedString();
			RoomData roomData = CyberEnvironment.GetGame().GetRoomManager().GenerateRoomData(num);
			this.Session.GetHabbo().GetInventoryComponent().RunDBUpdate();
			if (roomData == null)
			{
				return;
			}
			this.PrepareRoomForUser(num, password);
		}
		public void OpenQuests()
		{
			CyberEnvironment.GetGame().GetQuestManager().GetList(this.Session, this.Request);
		}
		public void StartQuest()
		{
			CyberEnvironment.GetGame().GetQuestManager().ActivateQuest(this.Session, this.Request);
		}
		public void StopQuest()
		{
			CyberEnvironment.GetGame().GetQuestManager().CancelQuest(this.Session, this.Request);
		}
		public void GetCurrentQuest()
		{
			CyberEnvironment.GetGame().GetQuestManager().GetCurrentQuest(this.Session, this.Request);
		}
		public void StartSeasonalQuest()
		{
			RoomData roomData = null;
			using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
			{
				Quest quest = CyberEnvironment.GetGame().GetQuestManager().GetQuest(this.Request.PopWiredUInt());
				if (quest == null)
				{
					return;
				}
				queryreactor.runFastQuery(string.Concat(new object[]
				{
					"REPLACE INTO user_quests(user_id,quest_id) VALUES (",
					this.Session.GetHabbo().Id,
					", ",
					quest.Id,
					")"
				}));
				queryreactor.runFastQuery(string.Concat(new object[]
				{
					"UPDATE user_stats SET quest_id = ",
					quest.Id,
					" WHERE id = ",
					this.Session.GetHabbo().Id
				}));
				this.Session.GetHabbo().CurrentQuestId = quest.Id;
				this.Session.SendMessage(QuestStartedComposer.Compose(this.Session, quest));
				CyberEnvironment.GetGame().GetQuestManager().ActivateQuest(this.Session, this.Request);
				queryreactor.setQuery("SELECT id FROM rooms WHERE state='open' ORDER BY users_now DESC LIMIT 1");
				string @string = queryreactor.getString();
				roomData = CyberEnvironment.GetGame().GetRoomManager().GenerateRoomData(uint.Parse(@string));
			}
			if (roomData != null)
			{
				roomData.SerializeRoomData(this.Response, true, this.Session, false);
				this.Session.GetMessageHandler().PrepareRoomForUser(roomData.Id, "");
				return;
			}
			this.Session.SendNotif("Go to a room to start the quest!");
		}
		internal void LoadClubGifts()
		{
			if (this.Session == null || this.Session.GetHabbo() == null)
			{
				return;
			}
			int i = 0;
			int i2 = 0;
			this.Session.GetHabbo().GetSubscriptionManager().GetSubscription();
			ServerMessage serverMessage = new ServerMessage();
			serverMessage.Init(Outgoing.LoadCatalogClubGiftsMessageComposer);
			serverMessage.AppendInt32(i);
			serverMessage.AppendInt32(i2);
			serverMessage.AppendInt32(1);
		}
		internal void ChooseClubGift()
		{
			if (this.Session == null || this.Session.GetHabbo() == null)
			{
				return;
			}
			this.Request.PopFixedString();
		}
		internal void PlantMonsterplant(RoomItem Mopla, Room Room)
		{
			if (Room == null)
			{
				return;
			}
			if (Mopla == null)
			{
				return;
			}
			if (Mopla.GetBaseItem().InteractionType != InteractionType.moplaseed)
			{
				return;
			}
			int rarity = int.Parse(Mopla.ExtraData);
			int getX = Mopla.GetX;
			int getY = Mopla.GetY;
			Room.GetRoomItemHandler().RemoveFurniture(this.Session, Mopla.Id, false);
			Pet pet = Catalog.CreatePet(this.Session.GetHabbo().Id, "Monsterplant", 16, "0", "0", rarity);
			this.Response.Init(Outgoing.SendMonsterplantIdMessageComposer);
			this.Response.AppendUInt(pet.PetId);
			this.SendResponse();
			using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
			{
				queryreactor.runFastQuery(string.Concat(new object[]
				{
					"UPDATE bots SET room_id = '",
					Room.RoomId,
					"', x = '",
					getX,
					"', y = '",
					getY,
					"' WHERE id = '",
					pet.PetId,
					"'"
				}));
			}
			pet.PlacedInRoom = true;
			pet.RoomId = Room.RoomId;
			List<RandomSpeech> list = new List<RandomSpeech>();
			List<BotResponse> list2 = new List<BotResponse>();
			RoomBot bot = new RoomBot(pet.PetId, pet.OwnerId, pet.RoomId, AIType.Pet, "freeroam", pet.Name, "", pet.Look, getX, getY, 0.0, 4, 0, 0, 0, 0, ref list, ref list2, "", 0, false);
			Room.GetRoomUserManager().DeployBot(bot, pet);

			if (pet.DBState != DatabaseUpdateState.NeedsInsert)
			{
				pet.DBState = DatabaseUpdateState.NeedsUpdate;
			}

			using (IQueryAdapter queryreactor2 = CyberEnvironment.GetDatabaseManager().getQueryReactor())
			{
                queryreactor2.runFastQuery("DELETE FROM items WHERE id = " + Mopla.Id);
				Room.GetRoomUserManager().SavePets(queryreactor2);
			}
		}

		internal void SaveHeightmap()
		{
            if (Session.GetHabbo().VIP || Session.GetHabbo().Rank >= 5)
            {
                Room AffectedRoom = Session.GetHabbo().CurrentRoom;

                if (AffectedRoom == null)
                {
                    Session.SendNotif("You must be in room to save these changes.");
                    return;
                }

                if (!AffectedRoom.CheckRights(Session, true))
                {
                    Session.SendNotif("No puedes hacer esto en una Sala que no es tuya...");
                    return;
                }

                string HeightMap = Request.PopFixedString();
                int DoorX = Request.PopWiredInt32();
                int DoorY = Request.PopWiredInt32();
                int DoorDir = Request.PopWiredInt32();
                int WallThickness = Request.PopWiredInt32();
                int FloorThickness = Request.PopWiredInt32();
                int WallHeight = Request.PopWiredInt32();

                double DoorZ = 0.0;

                
                HeightMap = HeightMap.ToLower();
                HeightMap = HeightMap.Replace(Convert.ToChar(10) + "", "");
                string[] array = HeightMap.Split(Convert.ToChar(13));
                int MapSizeX = array[0].Length;
                int MapSizeY = array.Length;

                for (int y = 0; y < MapSizeY; y++)
                {
                    string Line = array[y].Replace(Convert.ToChar(13) + "", "");

                    for (int x = 0; x < MapSizeX; x++)
                    {
                        if (x == DoorX && y == DoorY)
                        {
                            char c = 'x';
                            try
                            {
                                c = Line[x];
                            }
                            catch (Exception)
                            {
                                continue;
                            }
                            string text = "0123456789abcdefghijklmnopqrstuvwxyz";

                            DoorZ = text.IndexOf(c);
                        }
                    }
                }

                string newModelName = "model_floorplan_" + AffectedRoom.RoomId;
                AffectedRoom.ModelName = newModelName;
                AffectedRoom.WallHeight = WallHeight;
                AffectedRoom.WallThickness = WallThickness;
                AffectedRoom.FloorThickness = FloorThickness;

                using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
                {
                    queryreactor.runFastQuery("UPDATE rooms SET model_name =  '" + newModelName + "', wallthick =  '" + WallThickness + "', floorthick =  '" + FloorThickness + "', walls_height =  '" + WallHeight + "' WHERE id = " + AffectedRoom.RoomId + ";");

                    queryreactor.setQuery("REPLACE INTO `room_models` (`id` ,`door_x` ,`door_y` ,`door_z` ,`door_dir`, `heightmap`, `public_items`, `club_only`, `poolmap`) VALUES (@modelname,  @doorx,  @doory,  @doorz,  @doordir,  @hm,  '',  '0',  '');");
                    queryreactor.addParameter("modelname", newModelName);
                    queryreactor.addParameter("doorx", DoorX);
                    queryreactor.addParameter("doory", DoorY);
                    queryreactor.addParameter("doorz", TextHandling.GetString(DoorZ));
                    queryreactor.addParameter("doordir", DoorDir);
                    queryreactor.addParameter("hm", HeightMap);
                    queryreactor.runQuery();
                }
                CyberEnvironment.GetGame().GetRoomManager().LoadNewModel(newModelName);
                AffectedRoom.ResetGamemap(newModelName, WallHeight, WallThickness, FloorThickness);
                CyberEnvironment.GetGame().GetRoomManager().UnloadRoom(AffectedRoom);

                Session.SendNotif("Your new floorplan was saved! Your room was unloaded for applying changes.");
            }
		}

		internal void AcceptPoll()
		{
			uint key = this.Request.PopWiredUInt();
			Poll poll = CyberEnvironment.GetGame().GetPollManager().Polls[key];
			ServerMessage serverMessage = new ServerMessage(Outgoing.PollQuestionsMessageComposer);
			serverMessage.AppendUInt(poll.Id);
			serverMessage.AppendString(poll.PollName);
			serverMessage.AppendString(poll.Thanks);
			serverMessage.AppendInt32(poll.Questions.Count);
			foreach (PollQuestion current in poll.Questions)
			{
				int questionNumber = checked(poll.Questions.IndexOf(current) + 1);
				current.Serialize(serverMessage, questionNumber);
			}
			this.Response = serverMessage;
			this.SendResponse();
		}
		internal void RefusePoll()
		{
			uint num = this.Request.PopWiredUInt();
            this.Session.GetHabbo().AnsweredPolls.Add(num);
			using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
			{
				queryreactor.setQuery("INSERT INTO user_polldata VALUES (@userid , @pollid , 0 , '0' , '')");
				queryreactor.addParameter("userid", this.Session.GetHabbo().Id);
				queryreactor.addParameter("pollid", num);
				queryreactor.runQuery();
			}
		}
		internal void AnswerPoll()
		{
			uint num = this.Request.PopWiredUInt();
			uint num2 = this.Request.PopWiredUInt();
			int num3 = this.Request.PopWiredInt32();
			List<string> list = new List<string>();
			checked
			{
				for (int i = 0; i < num3; i++)
				{
					list.Add(this.Request.PopFixedString());
				}
				string text = string.Join("\r\n", list);
                this.Session.GetHabbo().AnsweredPolls.Add(num);
				using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
				{
					queryreactor.setQuery("INSERT INTO user_polldata VALUES (@userid , @pollid , @questionid , '1' , @answer)");
					queryreactor.addParameter("userid", this.Session.GetHabbo().Id);
					queryreactor.addParameter("pollid", num);
					queryreactor.addParameter("questionid", num2);
					queryreactor.addParameter("answer", text);
					queryreactor.runQuery();
				}
				/*Poll poll = CyberEnvironment.GetGame().GetPollManager().Polls[num];
				if (poll.Type != Poll.PollType.Opinion && poll.Questions.Last<PollQuestion>().Index == num2)
				{
					foreach (PollQuestion current in poll.Questions)
					{
						foreach (UserPollData current2 in this.Session.GetHabbo().AnsweredPolls)
						{
							if (current2.PollId == num && current2.QuestionId == current.Index && current2.Answer != current.CorrectAnswer)
							{
								return;
							}
						}
					}
					if (poll.Type == Poll.PollType.Prize_Badge)
					{
						this.Session.GetHabbo().GetBadgeComponent().GiveBadge(poll.Prize, true, this.Session, false);
						return;
					}
					if (poll.Type == Poll.PollType.Prize_Furni)
					{
						this.Session.GetHabbo().GetInventoryComponent().AddNewItem(0u, uint.Parse(poll.Prize.ToString()), "", 0u, true, false, 0, 0, "");
					}
				}*/
			}
		}
		internal void TileStackMagicSetHeight()
		{
			Room Room = CyberEnvironment.GetGame().GetRoomManager().GetRoom(this.Session.GetHabbo().CurrentRoomId);
			if (Room == null)
			{
				return;
			}

			uint ItemId = this.Request.PopWiredUInt();
			RoomItem Item = Room.GetRoomItemHandler().GetItem(ItemId);
			if (Item == null || Item.GetBaseItem().InteractionType != InteractionType.tilestackmagic)
			{
				return;
			}

			int HeightToSet = this.Request.PopWiredInt32();
			if (HeightToSet > 1000)
			{
				HeightToSet = 1000;
			}
			double TotalZ = (double)(HeightToSet / 100);
			Room.GetRoomItemHandler().SetFloorItem(Item, Item.GetX, Item.GetY, TotalZ, Item.Rot, true);
		}

		internal void PromoteRoom()
		{
			int pageId = this.Request.PopWiredInt32();
			int item = this.Request.PopWiredInt32();

			CatalogPage page2 = CyberEnvironment.GetGame().GetCatalog().GetPage(pageId);
			if (page2 == null)
			{
				return;
			}
            CatalogItem catalogItem = page2.GetItem(item);

			if (catalogItem == null)
			{
				return;
			}

			uint num = this.Request.PopWiredUInt();
			string text = this.Request.PopFixedString();
			this.Request.PopWiredBoolean();

            string text2 = "";
            try
            {
                text2 = this.Request.PopFixedString();
            }
            catch (Exception) { }

			Room room = CyberEnvironment.GetGame().GetRoomManager().GetRoom(num);
			if (room == null)
			{
				RoomData roomData = CyberEnvironment.GetGame().GetRoomManager().GenerateRoomData(num);
				if (roomData == null)
				{
					return;
				}
				room = new Room(roomData);
			}

            if (!room.CheckRights(Session, true))
                return;

			checked
			{
				if (catalogItem.CreditsCost > 0)
				{
					if (catalogItem.CreditsCost > this.Session.GetHabbo().Credits)
					{
						return;
					}
					this.Session.GetHabbo().Credits -= catalogItem.CreditsCost;
					this.Session.GetHabbo().UpdateCreditsBalance();
				}
				if (catalogItem.DucketsCost > 0)
				{
					if (catalogItem.DucketsCost > this.Session.GetHabbo().ActivityPoints)
					{
						return;
					}
					this.Session.GetHabbo().ActivityPoints -= catalogItem.DucketsCost;
					this.Session.GetHabbo().UpdateActivityPointsBalance();
				}
				if (catalogItem.BelCreditsCost > 0 || catalogItem.LoyaltyCost > 0)
				{
					if (catalogItem.BelCreditsCost > this.Session.GetHabbo().BelCredits)
					{
						return;
					}
					this.Session.GetHabbo().BelCredits -= catalogItem.BelCreditsCost;
					this.Session.GetHabbo().UpdateSeasonalCurrencyBalance();
				}
                this.Session.SendMessage(CatalogPacket.PurchaseOK());

				if (room.Event != null && !room.Event.HasExpired)
				{
					room.Event.Time = CyberEnvironment.GetUnixTimestamp();
					CyberEnvironment.GetGame().GetRoomEvents().SerializeEventInfo(room.RoomId);
				}
				else
				{
					CyberEnvironment.GetGame().GetRoomEvents().AddNewEvent(room.RoomId, text, text2, this.Session, 7200);
					CyberEnvironment.GetGame().GetRoomEvents().SerializeEventInfo(room.RoomId);
				}
				this.Session.GetHabbo().GetBadgeComponent().GiveBadge("RADZZ", true, this.Session, false);
			}
		}
		internal void GetPromotionableRooms()
		{
			ServerMessage serverMessage = new ServerMessage();
			serverMessage.Init(Outgoing.CatalogPromotionGetRoomsMessageComposer);
			serverMessage.AppendBoolean(true);
			serverMessage.AppendInt32(this.Session.GetHabbo().UsersRooms.Count);
			foreach (RoomData current in this.Session.GetHabbo().UsersRooms)
			{
				serverMessage.AppendUInt(current.Id);
				serverMessage.AppendString(current.Name);
				serverMessage.AppendBoolean(false);
			}
			this.Response = serverMessage;
			this.SendResponse();
		}

		internal void GetTrainerPanel()
		{
			uint petId = this.Request.PopWiredUInt();
			Room currentRoom = this.Session.GetHabbo().CurrentRoom;
			if (currentRoom == null)
			{
				return;
			}
			Pet petData;
			if ((petData = currentRoom.GetRoomUserManager().GetPet(petId).PetData) == null)
			{
				return;
			}
			int arg_3F_0 = petData.Level;
			this.Response.Init(Outgoing.PetTrainerPanelMessageComposer);
			this.Response.AppendUInt(petData.PetId);

            List<short> AvailableCommands = new List<short>();

            this.Response.AppendInt32(petData.PetCommands.Count);
            foreach (short Sh in petData.PetCommands.Keys)
            {
                this.Response.AppendInt32(Sh);
                if (petData.PetCommands[Sh] == true)
                {
                    AvailableCommands.Add(Sh);
                }
            }

            this.Response.AppendInt32(AvailableCommands.Count);
            foreach (short Sh in AvailableCommands)
            {
                this.Response.AppendInt32(Sh);
            }

			this.SendResponse();
		}
		internal void InitRoomGroupBadges()
		{
			CyberEnvironment.GetGame().GetRoomManager().GetRoom(this.Session.GetHabbo().LoadingRoom);
		}
		internal void GetPub()
		{
			uint roomId = this.Request.PopWiredUInt();
			RoomData roomData = CyberEnvironment.GetGame().GetRoomManager().GenerateRoomData(roomId);
			if (roomData == null)
			{
				return;
			}
			this.GetResponse().Init(453);
			this.GetResponse().AppendUInt(roomData.Id);
			this.GetResponse().AppendString(roomData.CCTs);
			this.GetResponse().AppendUInt(roomData.Id);
			this.SendResponse();
		}
		internal void OpenPub()
		{
			this.Request.PopWiredInt32();
			uint roomId = this.Request.PopWiredUInt();
			this.Request.PopWiredInt32();
			RoomData roomData = CyberEnvironment.GetGame().GetRoomManager().GenerateRoomData(roomId);
			if (roomData == null)
			{
				return;
			}
			this.PrepareRoomForUser(roomData.Id, "");
		}
		internal void GetInventory()
		{
			QueuedServerMessage queuedServerMessage = new QueuedServerMessage(this.Session.GetConnection());
			queuedServerMessage.appendResponse(this.Session.GetHabbo().GetInventoryComponent().SerializeFloorItemInventory());
			queuedServerMessage.sendResponse();
		}
		internal void GetRoomData1()
		{
			if (this.Session.GetHabbo().LoadingRoom <= 0u)
			{
				return;
			}
			this.Response.Init(297);
			this.Response.AppendInt32(0);
			this.SendResponse();
		}
		internal void GetRoomData2()
		{
			try
			{
				QueuedServerMessage queuedServerMessage = new QueuedServerMessage(this.Session.GetConnection());
				if (this.Session.GetHabbo().LoadingRoom > 0u && this.CurrentLoadingRoom != null)
				{
					RoomData roomData = this.CurrentLoadingRoom.RoomData;
					if (roomData != null)
					{
						if (roomData.Model == null)
						{
							this.Session.SendNotif("Go To Fuck Off You Stupid Damn AssHOLE  !  Fuck You !!");
							this.Session.SendMessage(new ServerMessage(Outgoing.OutOfRoomMessageComposer));
							this.ClearRoomLoading();
						}
						else
						{
							queuedServerMessage.appendResponse(this.CurrentLoadingRoom.GetGameMap().GetNewHeightmap());
							queuedServerMessage.appendResponse(this.CurrentLoadingRoom.GetGameMap().Model.GetHeightmap());
							queuedServerMessage.sendResponse();
							this.GetRoomData3();
						}
					}
				}
			}
			catch (Exception ex)
			{
				Logging.LogException(string.Concat(new object[]
				{
					"Unable to load room ID [",
					this.Session.GetHabbo().LoadingRoom,
					"] ",
					ex.ToString()
				}));
			}
		}
		internal void GetRoomData3()
		{
			if (this.Session.GetHabbo().LoadingRoom <= 0u || !this.Session.GetHabbo().LoadingChecksPassed || this.CurrentLoadingRoom == null || this.Session == null)
			{
				return;
			}
			QueuedServerMessage queuedServerMessage;
			RoomItem[] array;
			RoomItem[] array2;
			RoomItem[] array3;
			checked
			{
				if (this.CurrentLoadingRoom.UsersNow + 1 > this.CurrentLoadingRoom.UsersMax && !this.Session.GetHabbo().HasFuse("fuse_enter_full_rooms"))
				{
					this.Session.SendNotif("La Sala está llena.");
					return;
				}
				this.ClearRoomLoading();
				queuedServerMessage = new QueuedServerMessage(this.Session.GetConnection());
				array = this.CurrentLoadingRoom.GetRoomItemHandler().mFloorItems.Values.ToArray<RoomItem>();
				array2 = this.CurrentLoadingRoom.GetRoomItemHandler().mWallItems.Values.ToArray<RoomItem>();
				this.Response.Init(Outgoing.RoomFloorItemsMessageComposer);
				if (this.CurrentLoadingRoom.Group != null)
				{
					if (this.CurrentLoadingRoom.Group.AdminOnlyDeco == 1u)
					{
						this.Response.AppendInt32(this.CurrentLoadingRoom.Group.Admins.Count + 1);
						using (Dictionary<uint, GroupUser>.ValueCollection.Enumerator enumerator = this.CurrentLoadingRoom.Group.Admins.Values.GetEnumerator())
						{
							while (enumerator.MoveNext())
							{
								GroupUser current = enumerator.Current;
								this.Response.AppendUInt(current.Id);
								this.Response.AppendString(CyberEnvironment.getHabboForId(current.Id).Username);
							}
							goto IL_220;
						}
					}
					this.Response.AppendInt32(this.CurrentLoadingRoom.Group.Members.Count + 1);
					foreach (GroupUser current2 in this.CurrentLoadingRoom.Group.Members.Values)
					{
						this.Response.AppendUInt(current2.Id);
						this.Response.AppendString(CyberEnvironment.getHabboForId(current2.Id).Username);
					}
					IL_220:
					this.Response.AppendInt32(this.CurrentLoadingRoom.OwnerId);
					this.Response.AppendString(this.CurrentLoadingRoom.Owner);
				}
				else
				{
					this.Response.AppendInt32(1);
					this.Response.AppendInt32(this.CurrentLoadingRoom.OwnerId);
					this.Response.AppendString(this.CurrentLoadingRoom.Owner);
				}
				this.Response.AppendInt32(array.Length);
				array3 = array;
			}
			for (int i = 0; i < array3.Length; i++)
			{
				RoomItem roomItem = array3[i];
				roomItem.Serialize(this.Response);
			}
			queuedServerMessage.appendResponse(this.GetResponse());
			this.Response.Init(Outgoing.RoomWallItemsMessageComposer);
			RoomItem[] array4;
			checked
			{
				if (this.CurrentLoadingRoom.Group != null)
				{
					if (this.CurrentLoadingRoom.Group.AdminOnlyDeco == 1u)
					{
						this.Response.AppendInt32(this.CurrentLoadingRoom.Group.Admins.Count + 1);
						using (Dictionary<uint, GroupUser>.ValueCollection.Enumerator enumerator3 = this.CurrentLoadingRoom.Group.Admins.Values.GetEnumerator())
						{
							while (enumerator3.MoveNext())
							{
								GroupUser current3 = enumerator3.Current;
								this.Response.AppendUInt(current3.Id);
								this.Response.AppendString(CyberEnvironment.getHabboForId(current3.Id).Username);
							}
							goto IL_423;
						}
					}
					this.Response.AppendInt32(this.CurrentLoadingRoom.Group.Members.Count + 1);
					foreach (GroupUser current4 in this.CurrentLoadingRoom.Group.Members.Values)
					{
						this.Response.AppendUInt(current4.Id);
						this.Response.AppendString(CyberEnvironment.getHabboForId(current4.Id).Username);
					}
					IL_423:
					this.Response.AppendInt32(this.CurrentLoadingRoom.OwnerId);
					this.Response.AppendString(this.CurrentLoadingRoom.Owner);
				}
				else
				{
					this.Response.AppendInt32(1);
					this.Response.AppendInt32(this.CurrentLoadingRoom.OwnerId);
					this.Response.AppendString(this.CurrentLoadingRoom.Owner);
				}
				this.Response.AppendInt32(array2.Length);
				array4 = array2;
			}
			for (int j = 0; j < array4.Length; j++)
			{
				RoomItem roomItem2 = array4[j];
				roomItem2.Serialize(this.Response);
			}
			queuedServerMessage.appendResponse(this.GetResponse());
			Array.Clear(array, 0, array.Length);
			Array.Clear(array2, 0, array2.Length);
			array = null;
			array2 = null;
			this.CurrentLoadingRoom.GetRoomUserManager().AddUserToRoom(this.Session, this.Session.GetHabbo().SpectatorMode, false);
			queuedServerMessage.sendResponse();
			if (CyberEnvironment.GetUnixTimestamp() < this.Session.GetHabbo().FloodTime && this.Session.GetHabbo().FloodTime != 0)
			{
				ServerMessage serverMessage = new ServerMessage(Outgoing.FloodFilterMessageComposer);
				serverMessage.AppendInt32(checked(this.Session.GetHabbo().FloodTime - CyberEnvironment.GetUnixTimestamp()));
				this.Session.SendMessage(serverMessage);
			}

			Poll poll = null;
			if (CyberEnvironment.GetGame().GetPollManager().TryGetPoll(this.CurrentLoadingRoom.RoomId, out poll) && !this.Session.GetHabbo().GotPollData(poll.Id))
			{
				this.Response.Init(Outgoing.SuggestPollMessageComposer);
                poll.Serialize(this.Response);
                this.SendResponse();
			}
		}
		internal void RequestFloorItems()
		{
		}
		internal void RequestWallItems()
		{
		}
		internal void SaveBranding()
		{
			uint pId = this.Request.PopWiredUInt();
			uint num = this.Request.PopWiredUInt();
			string text = "state" + Convert.ToChar(9) + "0";
			int num2 = 1;
			while ((long)num2 <= (long)((ulong)num))
			{
				text = text + Convert.ToChar(9) + this.Request.PopFixedString();
				checked
				{
					num2++;
				}
			}
			Room currentRoom = this.Session.GetHabbo().CurrentRoom;
			RoomItem item = currentRoom.GetRoomItemHandler().GetItem(pId);
			item.ExtraData = text;
			currentRoom.GetRoomItemHandler().SetFloorItem(this.Session, item, item.GetX, item.GetY, item.Rot, false, false, true);
		}
		internal void OnRoomUserAdd()
		{
			if (this.Session == null || this.GetResponse() == null)
			{
				return;
			}
			QueuedServerMessage queuedServerMessage = new QueuedServerMessage(this.Session.GetConnection());
			List<RoomUser> list = new List<RoomUser>();
			if (this.CurrentLoadingRoom == null || this.CurrentLoadingRoom.GetRoomUserManager() == null || this.CurrentLoadingRoom.GetRoomUserManager().UserList == null)
			{
				return;
			}
			foreach (RoomUser current in this.CurrentLoadingRoom.GetRoomUserManager().UserList.Values)
			{
				if (current != null && !current.IsSpectator)
				{
					list.Add(current);
				}
			}
			this.Response.Init(Outgoing.SetRoomUserMessageComposer);
			this.Response.AppendInt32(list.Count);
			foreach (RoomUser current2 in list)
			{
				current2.Serialize(this.Response, this.CurrentLoadingRoom.GetGameMap().gotPublicPool);
			}
			queuedServerMessage.appendResponse(this.GetResponse());
			queuedServerMessage.appendResponse(this.RoomFloorAndWallComposer(this.CurrentLoadingRoom));
			queuedServerMessage.appendResponse(this.GetResponse());

			this.Response.Init(Outgoing.RoomOwnershipMessageComposer);
			this.Response.AppendUInt(this.CurrentLoadingRoom.RoomId);
			this.Response.AppendBoolean(this.CurrentLoadingRoom.CheckRights(this.Session, true, false));
            
			queuedServerMessage.appendResponse(this.GetResponse());
			foreach (uint current3 in this.CurrentLoadingRoom.UsersWithRights)
			{
				Habbo habboForId = CyberEnvironment.getHabboForId(current3);
				if (habboForId != null)
				{
					this.GetResponse().Init(Outgoing.GiveRoomRightsMessageComposer);
					this.GetResponse().AppendUInt(this.CurrentLoadingRoom.RoomId);
					this.GetResponse().AppendUInt(habboForId.Id);
					this.GetResponse().AppendString(habboForId.Username);
					queuedServerMessage.appendResponse(this.GetResponse());
				}
			}
			ServerMessage serverMessage = this.CurrentLoadingRoom.GetRoomUserManager().SerializeStatusUpdates(true);
			if (serverMessage != null)
			{
				queuedServerMessage.appendResponse(serverMessage);
			}
			if (this.CurrentLoadingRoom.Event != null)
			{
				CyberEnvironment.GetGame().GetRoomEvents().SerializeEventInfo(this.CurrentLoadingRoom.RoomId);
			}
			foreach (RoomUser current4 in this.CurrentLoadingRoom.GetRoomUserManager().UserList.Values)
			{
				if (current4 != null)
				{
					if (current4.IsBot)
					{
						if (current4.BotData.DanceId > 0)
						{
							this.Response.Init(Outgoing.DanceStatusMessageComposer);
							this.Response.AppendInt32(current4.VirtualId);
							this.Response.AppendInt32(current4.BotData.DanceId);
							queuedServerMessage.appendResponse(this.GetResponse());
						}
					}
					else
					{
						if (current4.IsDancing)
						{
							this.Response.Init(Outgoing.DanceStatusMessageComposer);
							this.Response.AppendInt32(current4.VirtualId);
							this.Response.AppendInt32(current4.DanceId);
							queuedServerMessage.appendResponse(this.GetResponse());
						}
					}
					if (current4.IsAsleep)
					{
                        ServerMessage sleepMsg = new ServerMessage(Outgoing.RoomUserIdleMessageComposer);
                        sleepMsg.AppendInt32(current4.VirtualId);
                        sleepMsg.AppendBoolean(true);
                        queuedServerMessage.appendResponse(sleepMsg);
					}
					if (current4.CarryItemID > 0 && current4.CarryTimer > 0)
					{
						this.Response.Init(Outgoing.ApplyHanditemMessageComposer);
						this.Response.AppendInt32(current4.VirtualId);
						this.Response.AppendInt32(current4.CarryTimer);
						queuedServerMessage.appendResponse(this.GetResponse());
					}
					if (!current4.IsBot)
					{
						try
						{
							if (current4 != null && current4.GetClient() != null && current4.GetClient().GetHabbo() != null)
							{
								if (current4.GetClient().GetHabbo().GetAvatarEffectsInventoryComponent() != null && current4.CurrentEffect >= 1)
								{
									this.Response.Init(Outgoing.ApplyEffectMessageComposer);
									this.Response.AppendInt32(current4.VirtualId);
									this.Response.AppendInt32(current4.CurrentEffect);
									this.Response.AppendInt32(0);
									queuedServerMessage.appendResponse(this.GetResponse());
								}
								ServerMessage serverMessage2 = new ServerMessage(Outgoing.UpdateUserDataMessageComposer);
								serverMessage2.AppendInt32(current4.VirtualId);
								serverMessage2.AppendString(current4.GetClient().GetHabbo().Look);
								serverMessage2.AppendString(current4.GetClient().GetHabbo().Gender.ToLower());
								serverMessage2.AppendString(current4.GetClient().GetHabbo().Motto);
								serverMessage2.AppendInt32(current4.GetClient().GetHabbo().AchievementPoints);
								if (this.CurrentLoadingRoom != null)
								{
									this.CurrentLoadingRoom.SendMessage(serverMessage2);
								}
							}
						}
						catch (Exception pException)
						{
							Logging.HandleException(pException, "Rooms.SendRoomData3");
						}
					}
				}
			}
			queuedServerMessage.sendResponse();
		}
		internal void enterOnRoom()
		{
			uint id = this.Request.PopWiredUInt();
			string password = this.Request.PopFixedString();
			this.PrepareRoomForUser(id, password);
		}
		internal void PrepareRoomForUser(uint Id, string Password)
		{
			this.ClearRoomLoading();
			QueuedServerMessage queuedServerMessage = new QueuedServerMessage(this.Session.GetConnection());
			if (this.Session == null)
			{
				return;
			}
			if (CyberEnvironment.ShutdownStarted)
			{
				this.Session.SendNotif("The server is shutting down. What are you doing?");
				return;
			}
			if (this.Session.GetHabbo().InRoom)
			{
				Room room = CyberEnvironment.GetGame().GetRoomManager().GetRoom(this.Session.GetHabbo().CurrentRoomId);
				if (room != null && room.GetRoomUserManager() != null)
				{
					room.GetRoomUserManager().RemoveUserFromRoom(this.Session, false, false);
				}
			}
			Room room2 = CyberEnvironment.GetGame().GetRoomManager().LoadRoom(Id);
			if (room2 == null)
			{
				return;
			}

			if (room2.UserCount >= room2.UsersMax && !this.Session.GetHabbo().HasFuse("fuse_enter_full_rooms") && (ulong)this.Session.GetHabbo().Id != (ulong)((long)room2.OwnerId))
			{
				ServerMessage serverMessage = new ServerMessage(Outgoing.RoomEnterErrorMessageComposer);
				serverMessage.AppendInt32(1);
				this.Session.SendMessage(serverMessage);
				ServerMessage message = new ServerMessage(Outgoing.OutOfRoomMessageComposer);
				this.Session.SendMessage(message);
				return;
			}
			if (room2 == null || this.Session == null || this.Session.GetHabbo() == null)
			{
				return;
			}
			if (this.Session.GetHabbo().IsTeleporting && this.Session.GetHabbo().TeleportingRoomID != Id)
			{
				return;
			}
			this.Session.GetHabbo().LoadingRoom = Id;
			this.CurrentLoadingRoom = room2;
			if (!this.Session.GetHabbo().HasFuse("fuse_enter_any_room") && room2.UserIsBanned(this.Session.GetHabbo().Id))
			{
				if (!room2.HasBanExpired(this.Session.GetHabbo().Id))
				{
					ServerMessage serverMessage2 = new ServerMessage(Outgoing.RoomEnterErrorMessageComposer);
					serverMessage2.AppendInt32(4);
					this.Session.SendMessage(serverMessage2);
					this.Response.Init(Outgoing.OutOfRoomMessageComposer);
					queuedServerMessage.appendResponse(this.GetResponse());
					queuedServerMessage.sendResponse();
					return;
				}
				room2.RemoveBan(this.Session.GetHabbo().Id);
			}
			this.Response.Init(Outgoing.PrepareRoomMessageComposer);
			queuedServerMessage.appendResponse(this.GetResponse());
			if (!this.Session.GetHabbo().HasFuse("fuse_enter_any_room") && !room2.CheckRights(this.Session, true, false) && !this.Session.GetHabbo().IsTeleporting && !this.Session.GetHabbo().IsHopping)
			{
				if (room2.State == 1)
				{
					if (room2.UserCount == 0)
					{
						this.Response.Init(Outgoing.DoorbellNoOneMessageComposer);
						queuedServerMessage.appendResponse(this.GetResponse());
					}
					else
					{
						this.Response.Init(Outgoing.DoorbellMessageComposer);
						this.Response.AppendString("");
						queuedServerMessage.appendResponse(this.GetResponse());
						ServerMessage serverMessage3 = new ServerMessage(Outgoing.DoorbellMessageComposer);
						serverMessage3.AppendString(this.Session.GetHabbo().Username);
						room2.SendMessageToUsersWithRights(serverMessage3);
					}
					queuedServerMessage.sendResponse();
					return;
				}
				if (room2.State == 2 && Password.ToLower() != room2.Password.ToLower())
				{
					this.Response.Init(Outgoing.OutOfRoomMessageComposer);
					queuedServerMessage.appendResponse(this.GetResponse());
					queuedServerMessage.sendResponse();
					return;
				}
			}
			this.Session.GetHabbo().LoadingChecksPassed = true;
			queuedServerMessage.addBytes(this.LoadRoomForUser().getPacket);
			queuedServerMessage.sendResponse();
			if (this.Session.GetHabbo().RecentlyVisitedRooms.Contains(room2.RoomId))
			{
                this.Session.GetHabbo().RecentlyVisitedRooms.Remove(room2.RoomId);
			}
            this.Session.GetHabbo().RecentlyVisitedRooms.Add(room2.RoomId);
		}
		internal void ReqLoadRoomForUser()
		{
			this.LoadRoomForUser().sendResponse();
		}
		internal QueuedServerMessage LoadRoomForUser()
		{
			Room currentLoadingRoom = this.CurrentLoadingRoom;
			QueuedServerMessage queuedServerMessage = new QueuedServerMessage(this.Session.GetConnection());
			if (currentLoadingRoom == null || !this.Session.GetHabbo().LoadingChecksPassed)
			{
				return queuedServerMessage;
			}
			if (this.Session.GetHabbo().FavouriteGroup > 0u)
			{
				if (this.CurrentLoadingRoom.Group != null && !this.CurrentLoadingRoom.LoadedGroups.ContainsKey(this.CurrentLoadingRoom.Group.Id))
				{
					this.CurrentLoadingRoom.LoadedGroups.Add(this.CurrentLoadingRoom.Group.Id, this.CurrentLoadingRoom.Group.Badge);
				}
				if (!this.CurrentLoadingRoom.LoadedGroups.ContainsKey(this.Session.GetHabbo().FavouriteGroup) && CyberEnvironment.GetGame().GetGroupManager().GetGroup(this.Session.GetHabbo().FavouriteGroup) != null)
				{
					this.CurrentLoadingRoom.LoadedGroups.Add(this.Session.GetHabbo().FavouriteGroup, CyberEnvironment.GetGame().GetGroupManager().GetGroup(this.Session.GetHabbo().FavouriteGroup).Badge);
				}
			}
			this.Response.Init(Outgoing.RoomGroupMessageComposer);
			this.Response.AppendInt32(CurrentLoadingRoom.LoadedGroups.Count);
            foreach (var guild1 in CurrentLoadingRoom.LoadedGroups)
            {
                this.Response.AppendUInt(guild1.Key);
                this.Response.AppendString(guild1.Value);
            }
			queuedServerMessage.appendResponse(this.GetResponse());
			this.Response.Init(Outgoing.InitialRoomInfoMessageComposer);
			this.Response.AppendString(currentLoadingRoom.ModelName);
			this.Response.AppendUInt(currentLoadingRoom.RoomId);
			queuedServerMessage.appendResponse(this.GetResponse());
			if (this.Session.GetHabbo().SpectatorMode)
			{
				this.Response.Init(Outgoing.SpectatorModeMessageComposer);
				queuedServerMessage.appendResponse(this.GetResponse());
			}
			if (currentLoadingRoom.Wallpaper != "0.0")
			{
				this.Response.Init(Outgoing.RoomSpacesMessageComposer);
				this.Response.AppendString("wallpaper");
				this.Response.AppendString(currentLoadingRoom.Wallpaper);
				queuedServerMessage.appendResponse(this.GetResponse());
			}
			if (currentLoadingRoom.Floor != "0.0")
			{
				this.Response.Init(Outgoing.RoomSpacesMessageComposer);
				this.Response.AppendString("floor");
				this.Response.AppendString(currentLoadingRoom.Floor);
				queuedServerMessage.appendResponse(this.GetResponse());
			}
			this.Response.Init(Outgoing.RoomSpacesMessageComposer);
			this.Response.AppendString("landscape");
			this.Response.AppendString(currentLoadingRoom.Landscape);
			queuedServerMessage.appendResponse(this.GetResponse());
			if (currentLoadingRoom.CheckRights(this.Session, true, false))
			{
				this.Response.Init(Outgoing.RoomRightsLevelMessageComposer);
				this.Response.AppendInt32(4);
				queuedServerMessage.appendResponse(this.GetResponse());
				this.Response.Init(Outgoing.HasOwnerRightsMessageComposer);
				queuedServerMessage.appendResponse(this.GetResponse());
			}
			else
			{
				if (currentLoadingRoom.CheckRights(this.Session))
				{
					this.Response.Init(Outgoing.RoomRightsLevelMessageComposer);
					this.Response.AppendInt32(1);
					queuedServerMessage.appendResponse(this.GetResponse());
				}
				else
				{
					this.Response.Init(Outgoing.RoomRightsLevelMessageComposer);
					this.Response.AppendInt32(0);
					queuedServerMessage.appendResponse(this.GetResponse());
				}
			}
			this.Response.Init(Outgoing.RoomRatingMessageComposer);
			this.Response.AppendInt32(currentLoadingRoom.Score);
			this.Response.AppendBoolean(!this.Session.GetHabbo().RatedRooms.Contains(currentLoadingRoom.RoomId) && !currentLoadingRoom.CheckRights(this.Session, true, false));
			queuedServerMessage.appendResponse(this.GetResponse());
			return queuedServerMessage;
		}
		internal void ClearRoomLoading()
		{
			this.Session.GetHabbo().LoadingRoom = 0u;
			this.Session.GetHabbo().LoadingChecksPassed = false;
		}
		internal void Move()
		{
			Room currentRoom = this.Session.GetHabbo().CurrentRoom;
			if (currentRoom == null)
			{
				return;
			}
			RoomUser roomUserByHabbo = currentRoom.GetRoomUserManager().GetRoomUserByHabbo(this.Session.GetHabbo().Id);
			if (roomUserByHabbo == null || !roomUserByHabbo.CanWalk)
			{
				return;
			}
			int num = this.Request.PopWiredInt32();
			int num2 = this.Request.PopWiredInt32();
			if (num == roomUserByHabbo.X && num2 == roomUserByHabbo.Y)
			{
				return;
			}
			roomUserByHabbo.MoveTo(num, num2);
			if (roomUserByHabbo.RidingHorse)
			{
				RoomUser roomUserByVirtualId = currentRoom.GetRoomUserManager().GetRoomUserByVirtualId(Convert.ToInt32(roomUserByHabbo.HorseID));
				roomUserByVirtualId.MoveTo(num, num2);
			}
		}
		internal void CanCreateRoom()
		{
			this.Response.Init(Outgoing.CanCreateRoomMessageComposer);
			this.Response.AppendInt32((this.Session.GetHabbo().UsersRooms.Count >= 75) ? 1 : 0);
			this.Response.AppendInt32(75);
			this.SendResponse();
		}

		internal void CreateRoom()
		{
            if (Session.GetHabbo().UsersRooms.Count >= 75)
            {
                Session.SendNotif("You can't have more than 75 rooms. Try to delete some rooms!");
                return;
            }
            else if ((CyberEnvironment.GetUnixTimestamp() - Session.GetHabbo().lastSqlQuery) < 20)
            {
                Session.SendNotif("Please wait 20 seconds before creating the next room!");
                return;
            }

			string Name = this.Request.PopFixedString();
			string Description = this.Request.PopFixedString();
			string RoomModel = this.Request.PopFixedString();
			int Category = this.Request.PopWiredInt32();
			int MaxVisitors = this.Request.PopWiredInt32();
			int TradeState = this.Request.PopWiredInt32();

			RoomData Data = CyberEnvironment.GetGame().GetRoomManager().CreateRoom(this.Session, Name, Description, RoomModel, Category, MaxVisitors, TradeState);
			if (Data != null)
			{
                Session.GetHabbo().lastSqlQuery = CyberEnvironment.GetUnixTimestamp();
				this.Response.Init(Outgoing.OnCreateRoomInfoMessageComposer);
				this.Response.AppendUInt(Data.Id);
				this.Response.AppendString(Data.Name);
				this.SendResponse();
			}
		}

		internal void GetRoomEditData()
		{
			Room room = CyberEnvironment.GetGame().GetRoomManager().GetRoom(Convert.ToUInt32(this.Request.PopWiredInt32()));
			this.GetResponse().Init(Outgoing.RoomSettingsDataMessageComposer);
			this.GetResponse().AppendUInt(room.RoomId);
			this.GetResponse().AppendString(room.Name);
			this.GetResponse().AppendString(room.Description);
			this.GetResponse().AppendInt32(room.State);
			this.GetResponse().AppendInt32(room.Category);
			this.GetResponse().AppendInt32(room.UsersMax);
			this.GetResponse().AppendInt32((checked(room.RoomData.Model.MapSizeX * room.RoomData.Model.MapSizeY) > 200) ? 50 : 25);
			this.GetResponse().AppendInt32(room.TagCount);
			object[] array = room.Tags.ToArray();
			for (int i = 0; i < array.Length; i++)
			{
				string s = (string)array[i];
				this.GetResponse().AppendString(s);
			}
			this.GetResponse().AppendInt32(room.TradeState);
			this.GetResponse().AppendInt32(room.AllowPets);
			this.GetResponse().AppendInt32(room.AllowPetsEating);
			this.GetResponse().AppendInt32(room.AllowWalkthrough);
			this.GetResponse().AppendInt32(room.Hidewall);
			this.GetResponse().AppendInt32(room.WallThickness);
			this.GetResponse().AppendInt32(room.FloorThickness);
			this.GetResponse().AppendInt32(room.ChatType);
			this.GetResponse().AppendInt32(room.ChatBalloon);
			this.GetResponse().AppendInt32(room.ChatSpeed);
			this.GetResponse().AppendInt32(room.ChatMaxDistance);
			this.GetResponse().AppendInt32(room.ChatFloodProtection);
            this.GetResponse().AppendBoolean(false);//new build fix by Finn
			this.GetResponse().AppendInt32(room.WhoCanMute);
			this.GetResponse().AppendInt32(room.WhoCanKick);
			this.GetResponse().AppendInt32(room.WhoCanBan);
			this.SendResponse();
		}
		internal void RoomSettingsOkComposer(uint roomId)
		{
			this.GetResponse().Init(Outgoing.RoomSettingsSavedMessageComposer);
			this.GetResponse().AppendUInt(roomId);
			this.SendResponse();
		}
		internal void RoomUpdatedOkComposer(uint roomId)
		{
			this.GetResponse().Init(Outgoing.RoomUpdateMessageComposer);
			this.GetResponse().AppendUInt(roomId);
			this.SendResponse();
		}
		internal ServerMessage RoomFloorAndWallComposer(Room room)
		{
			ServerMessage serverMessage = new ServerMessage(Outgoing.RoomFloorWallLevelsMessageComposer);
			serverMessage.AppendBoolean(room.Hidewall == 1);
			serverMessage.AppendInt32(room.WallThickness);
			serverMessage.AppendInt32(room.FloorThickness);
			return serverMessage;
		}
		internal ServerMessage SerializeRoomChatOption(Room room)
		{
			ServerMessage serverMessage = new ServerMessage(Outgoing.RoomChatOptionsMessageComposer);
			serverMessage.AppendInt32(room.ChatType);
			serverMessage.AppendInt32(room.ChatBalloon);
			serverMessage.AppendInt32(room.ChatSpeed);
			serverMessage.AppendInt32(room.ChatMaxDistance);
			serverMessage.AppendInt32(room.ChatFloodProtection);
			return serverMessage;
		}
		internal void ParseRoomDataInformation()
		{
			uint id = this.Request.PopWiredUInt();
			int num = this.Request.PopWiredInt32();
			int num2 = this.Request.PopWiredInt32();
			Room room = CyberEnvironment.GetGame().GetRoomManager().LoadRoom(id);
			if (num == 0 && num2 == 1)
			{
				this.SerializeRoomInformation(room, false);
				return;
			}
			if (num == 1 && num2 == 0)
			{
				this.SerializeRoomInformation(room, true);
				return;
			}
			this.SerializeRoomInformation(room, true);
		}
		internal void SerializeRoomInformation(Room Room, bool show)
		{
			if (Room == null)
			{
				return;
			}

			this.GetResponse().Init(Outgoing.RoomDataMessageComposer);
			this.GetResponse().AppendBoolean(show);
			this.GetResponse().AppendUInt(Room.RoomId);
			this.GetResponse().AppendString(Room.Name);
			this.GetResponse().AppendBoolean(Room.Type == "private");
			this.GetResponse().AppendInt32(Room.OwnerId);
			this.GetResponse().AppendString(Room.Owner);
			this.GetResponse().AppendInt32(Room.State);
			this.GetResponse().AppendInt32(Room.UsersNow);
			this.GetResponse().AppendInt32(Room.UsersMax);
			this.GetResponse().AppendString(Room.Description);
			this.GetResponse().AppendInt32(Room.TradeState);
			this.GetResponse().AppendInt32(Room.Score);
			this.GetResponse().AppendInt32(0);
            this.GetResponse().AppendInt32(0);
			this.GetResponse().AppendInt32(Room.Category);

			if (Room.GroupId > 0 && Room.Group != null)
			{
				this.GetResponse().AppendUInt(Room.Group.Id);
				this.GetResponse().AppendString(Room.Group.Name);
				this.GetResponse().AppendString(Room.Group.Badge);
				this.Response.AppendString("");
			}
			else
			{
				this.GetResponse().AppendInt32(0);
				this.GetResponse().AppendString("");
				this.GetResponse().AppendString("");
				this.GetResponse().AppendString("");
			}
			this.GetResponse().AppendInt32(Room.TagCount);
			object[] array = Room.Tags.ToArray();
			for (int i = 0; i < array.Length; i++)
			{
				string s = (string)array[i];
				this.GetResponse().AppendString(s);
			}
			this.GetResponse().AppendInt32(0);
			this.GetResponse().AppendInt32(0);
			this.GetResponse().AppendInt32(0);
			this.GetResponse().AppendBoolean(Room.AllowPets == 1);
			this.GetResponse().AppendBoolean(Room.AllowPetsEating == 1);
			this.GetResponse().AppendString("");
			this.GetResponse().AppendString("");
			this.GetResponse().AppendInt32(0);
			this.GetResponse().AppendBoolean(Room.RoomId != this.Session.GetHabbo().CurrentRoomId);
            this.GetResponse().AppendBoolean(false);
			this.GetResponse().AppendBoolean(false);
			this.GetResponse().AppendBoolean(false);
			this.GetResponse().AppendInt32(Room.WhoCanMute);
			this.GetResponse().AppendInt32(Room.WhoCanKick);
			this.GetResponse().AppendInt32(Room.WhoCanBan);
			this.GetResponse().AppendBoolean(Room.CheckRights(this.Session, true, false));
			this.GetResponse().AppendInt32(Room.ChatType);
			this.GetResponse().AppendInt32(Room.ChatBalloon);
			this.GetResponse().AppendInt32(Room.ChatSpeed);
			this.GetResponse().AppendInt32(Room.ChatMaxDistance);
			this.GetResponse().AppendInt32(Room.ChatFloodProtection);
			this.SendResponse();
			if (Room.Group == null)
			{
				DataTable table;
				using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
				{
					queryreactor.setQuery("SELECT user_id FROM room_rights WHERE room_id=" + Room.RoomId);
					table = queryreactor.getTable();
				}
				this.Response.Init(Outgoing.LoadRoomRightsListMessageComposer);
				this.GetResponse().AppendUInt(Room.RoomData.Id);
				this.GetResponse().AppendInt32(table.Rows.Count);
				foreach (DataRow dataRow in table.Rows)
				{
					Habbo habboForId = CyberEnvironment.getHabboForId((uint)dataRow[0]);
					if (habboForId != null)
					{
						this.GetResponse().AppendUInt(habboForId.Id);
						this.GetResponse().AppendString(habboForId.Username);
					}
				}
				this.SendResponse();
			}
		}
		internal void SaveRoomData()
		{
			Room room = CyberEnvironment.GetGame().GetRoomManager().GetRoom(this.Session.GetHabbo().CurrentRoomId);
			RoomData arg_26_0 = room.RoomData;
			if (room == null || !room.CheckRights(this.Session, true, false))
			{
				return;
			}
			this.Request.PopWiredInt32();
			string text = this.Request.PopFixedString();
			string description = this.Request.PopFixedString();
			int num = this.Request.PopWiredInt32();
			string password = this.Request.PopFixedString();
			int num2 = this.Request.PopWiredInt32();
			int num3 = this.Request.PopWiredInt32();
			int num4 = this.Request.PopWiredInt32();
			List<string> list = new List<string>();
			StringBuilder stringBuilder = new StringBuilder();
			int allowPets;
			int allowPetsEating;
			int allowWalkthrough;
			int hidewall;
			int num5;
			int num6;
			int whoCanMute;
			int whoCanKick;
			int whoCanBan;
			int chatType;
			int chatBalloon;
			int chatSpeed;
			int num7;
			int chatFloodProtection;
            int tradestate;
			FlatCat flatCat;
			checked
			{
				for (int i = 0; i < num4; i++)
				{
					if (i > 0)
					{
						stringBuilder.Append(",");
					}
					string text2 = this.Request.PopFixedString().ToLower();
					list.Add(text2);
					stringBuilder.Append(text2);
				}
				tradestate = this.Request.PopWiredInt32();
				allowPets = Convert.ToInt32(CyberEnvironment.BoolToEnum(this.Request.PopWiredBoolean()));
				allowPetsEating = Convert.ToInt32(CyberEnvironment.BoolToEnum(this.Request.PopWiredBoolean()));
				allowWalkthrough = Convert.ToInt32(CyberEnvironment.BoolToEnum(this.Request.PopWiredBoolean()));
				hidewall = Convert.ToInt32(CyberEnvironment.BoolToEnum(this.Request.PopWiredBoolean()));
				num5 = this.Request.PopWiredInt32();
				num6 = this.Request.PopWiredInt32();
				whoCanMute = this.Request.PopWiredInt32();
				whoCanKick = this.Request.PopWiredInt32();
				whoCanBan = this.Request.PopWiredInt32();
				chatType = this.Request.PopWiredInt32();
				chatBalloon = this.Request.PopWiredInt32();
				chatSpeed = this.Request.PopWiredInt32();
				num7 = this.Request.PopWiredInt32();
				chatFloodProtection = this.Request.PopWiredInt32();
				if (num5 < -2 || num5 > 1)
				{
					num5 = 0;
				}
				if (num6 < -2 || num6 > 1)
				{
					num6 = 0;
				}
				if (text.Length < 1)
				{
					return;
				}
				if (num < 0 || num > 2)
				{
					return;
				}
				if (num2 < 0)
				{
					return;
				}
				if (num7 > 99)
				{
					num7 = 99;
				}
				flatCat = CyberEnvironment.GetGame().GetNavigator().GetFlatCat(num3);
				if (flatCat == null)
				{
					return;
				}
			}
			if (flatCat.MinRank > this.Session.GetHabbo().Rank)
			{
                num3 = 0;
			}
			if (num4 > 2)
			{
				return;
			}
            room.TradeState = tradestate;
			room.AllowPets = allowPets;
			room.AllowPetsEating = allowPetsEating;
			room.AllowWalkthrough = allowWalkthrough;
			room.Hidewall = hidewall;
			room.RoomData.AllowPets = allowPets;
			room.RoomData.AllowPetsEating = allowPetsEating;
			room.RoomData.AllowWalkthrough = allowWalkthrough;
			room.RoomData.Hidewall = hidewall;
			room.Name = text;
			room.State = num;
			room.Description = description;
			room.Category = num3;
			room.Password = password;
			room.RoomData.Name = text;
			room.RoomData.State = num;
			room.RoomData.Description = description;
			room.RoomData.Category = num3;
			room.RoomData.Password = password;
			room.WhoCanBan = whoCanBan;
			room.WhoCanKick = whoCanKick;
			room.WhoCanMute = whoCanMute;
			room.RoomData.WhoCanBan = whoCanBan;
			room.RoomData.WhoCanKick = whoCanKick;
			room.RoomData.WhoCanMute = whoCanMute;
			room.ClearTags();
			room.AddTagRange(list);
			room.UsersMax = num2;
			room.RoomData.Tags.Clear();
			room.RoomData.Tags.AddRange(list);
			room.RoomData.UsersMax = num2;
			room.WallThickness = num5;
			room.FloorThickness = num6;
			room.RoomData.WallThickness = num5;
			room.RoomData.FloorThickness = num6;
			room.ChatType = chatType;
			room.ChatBalloon = chatBalloon;
			room.ChatSpeed = chatSpeed;
			room.ChatMaxDistance = num7;
			room.ChatFloodProtection = chatFloodProtection;
			room.RoomData.ChatType = chatType;
			room.RoomData.ChatBalloon = chatBalloon;
			room.RoomData.ChatSpeed = chatSpeed;
			room.RoomData.ChatMaxDistance = num7;
			room.RoomData.ChatFloodProtection = chatFloodProtection;
			this.RoomSettingsOkComposer(room.RoomId);
			this.RoomUpdatedOkComposer(room.RoomId);
			this.Session.GetHabbo().CurrentRoom.SendMessage(this.RoomFloorAndWallComposer(room));
			this.Session.GetHabbo().CurrentRoom.SendMessage(this.SerializeRoomChatOption(room));
			room.RoomData.SerializeRoomData(this.Response, false, this.Session, true);
		}
		internal void GetBannedUsers()
		{
			uint num = this.Request.PopWiredUInt();
			Room room = CyberEnvironment.GetGame().GetRoomManager().GetRoom(num);
			if (room == null)
			{
				return;
			}
			List<uint> list = room.BannedUsers();
			this.Response.Init(Outgoing.RoomBannedListMessageComposer);
			this.Response.AppendUInt(num);
			this.Response.AppendInt32(list.Count);
			foreach (uint current in list)
			{
				this.Response.AppendUInt(current);
				if (CyberEnvironment.getHabboForId(current) != null)
				{
					this.Response.AppendString(CyberEnvironment.getHabboForId(current).Username);
				}
				else
				{
					this.Response.AppendString("Undefined");
				}
			}
			this.SendResponse();
		}
		internal void UsersWithRights()
		{
			this.Response.Init(Outgoing.LoadRoomRightsListMessageComposer);
			this.Response.AppendUInt(this.Session.GetHabbo().CurrentRoom.RoomId);
			this.Response.AppendInt32(this.Session.GetHabbo().CurrentRoom.UsersWithRights.Count);
			foreach (uint current in this.Session.GetHabbo().CurrentRoom.UsersWithRights)
			{
				Habbo habboForId = CyberEnvironment.getHabboForId(current);
				this.Response.AppendUInt(current);
				this.Response.AppendString((habboForId == null) ? "Undefined" : habboForId.Username);
			}
			this.SendResponse();
		}
		internal void UnbanUser()
		{
			uint num = this.Request.PopWiredUInt();
			uint num2 = this.Request.PopWiredUInt();
			Room room = CyberEnvironment.GetGame().GetRoomManager().GetRoom(num2);
			if (room == null)
			{
				return;
			}
			room.Unban(num);
			this.Response.Init(Outgoing.RoomUnbanUserMessageComposer);
			this.Response.AppendUInt(num2);
			this.Response.AppendUInt(num);
			this.SendResponse();
		}
		internal void GiveRights()
		{
			uint num = this.Request.PopWiredUInt();
			Room room = CyberEnvironment.GetGame().GetRoomManager().GetRoom(this.Session.GetHabbo().CurrentRoomId);
			if (room == null)
			{
				return;
			}
			RoomUser roomUserByHabbo = room.GetRoomUserManager().GetRoomUserByHabbo(num);
			if (room == null || !room.CheckRights(this.Session, true, false))
			{
				return;
			}
			if (room.UsersWithRights.Contains(num))
			{
				this.Session.SendNotif("No puedes darle permisos.");
				return;
			}
			room.UsersWithRights.Add(num);
			using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
			{
				queryreactor.runFastQuery(string.Concat(new object[]
				{
					"INSERT INTO room_rights (room_id,user_id) VALUES (",
					room.RoomId,
					",",
					num,
					")"
				}));
			}
			if (roomUserByHabbo != null && !roomUserByHabbo.IsBot)
			{
				this.Response.Init(Outgoing.GiveRoomRightsMessageComposer);
				this.Response.AppendUInt(room.RoomId);
				this.Response.AppendUInt(roomUserByHabbo.GetClient().GetHabbo().Id);
				this.Response.AppendString(roomUserByHabbo.GetClient().GetHabbo().Username);
				this.SendResponse();
				roomUserByHabbo.UpdateNeeded = true;
				if (roomUserByHabbo != null && !roomUserByHabbo.IsBot)
				{
					roomUserByHabbo.AddStatus("flatctrl 1", "");
					this.Response.Init(Outgoing.RoomRightsLevelMessageComposer);
					this.Response.AppendInt32(1);
					roomUserByHabbo.GetClient().SendMessage(this.GetResponse());
				}
			}
			this.UsersWithRights();
		}
		internal void TakeRights()
		{
			Room room = CyberEnvironment.GetGame().GetRoomManager().GetRoom(this.Session.GetHabbo().CurrentRoomId);
			if (room == null || !room.CheckRights(this.Session, true, false))
			{
				return;
			}
			StringBuilder stringBuilder = new StringBuilder();
			int num = this.Request.PopWiredInt32();
			checked
			{
				for (int i = 0; i < num; i++)
				{
					if (i > 0)
					{
						stringBuilder.Append(" OR ");
					}
					uint num2 = this.Request.PopWiredUInt();
					if (room.UsersWithRights.Contains(num2))
					{
						room.UsersWithRights.Remove(num2);
					}
					stringBuilder.Append(string.Concat(new object[]
					{
						"room_id = '",
						room.RoomId,
						"' AND user_id = '",
						num2,
						"'"
					}));
					RoomUser roomUserByHabbo = room.GetRoomUserManager().GetRoomUserByHabbo(num2);
					if (roomUserByHabbo != null && !roomUserByHabbo.IsBot)
					{
						this.Response.Init(Outgoing.RoomRightsLevelMessageComposer);
						this.Response.AppendInt32(0);
						roomUserByHabbo.GetClient().SendMessage(this.GetResponse());
						roomUserByHabbo.RemoveStatus("flatctrl 1");
						roomUserByHabbo.UpdateNeeded = true;
					}
					this.Response.Init(Outgoing.RemoveRightsMessageComposer);
					this.Response.AppendUInt(room.RoomId);
					this.Response.AppendUInt(num2);
					this.SendResponse();
				}
				this.UsersWithRights();
				using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
				{
					queryreactor.runFastQuery("DELETE FROM room_rights WHERE " + stringBuilder.ToString());
				}
			}
		}
		internal void TakeAllRights()
		{
			Room room = CyberEnvironment.GetGame().GetRoomManager().GetRoom(this.Session.GetHabbo().CurrentRoomId);
			if (room == null || !room.CheckRights(this.Session, true, false))
			{
				return;
			}
			DataTable table;
			using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
			{
				queryreactor.setQuery("SELECT user_id FROM room_rights WHERE room_id=" + room.RoomId);
				table = queryreactor.getTable();
			}
			foreach (DataRow dataRow in table.Rows)
			{
				uint num = (uint)dataRow[0];
				RoomUser roomUserByHabbo = room.GetRoomUserManager().GetRoomUserByHabbo(num);
				this.Response.Init(Outgoing.RemoveRightsMessageComposer);
				this.Response.AppendUInt(room.RoomId);
				this.Response.AppendUInt(num);
				this.SendResponse();
				if (roomUserByHabbo != null && !roomUserByHabbo.IsBot)
				{
					this.Response.Init(Outgoing.RoomRightsLevelMessageComposer);
					this.Response.AppendInt32(0);
					roomUserByHabbo.GetClient().SendMessage(this.GetResponse());
					roomUserByHabbo.RemoveStatus("flatctrl 1");
					roomUserByHabbo.UpdateNeeded = true;
				}
			}
			using (IQueryAdapter queryreactor2 = CyberEnvironment.GetDatabaseManager().getQueryReactor())
			{
				queryreactor2.runFastQuery("DELETE FROM room_rights WHERE room_id = " + room.RoomId);
			}
			room.UsersWithRights.Clear();
			this.UsersWithRights();
		}
		internal void KickUser()
		{
			Room room = CyberEnvironment.GetGame().GetRoomManager().GetRoom(this.Session.GetHabbo().CurrentRoomId);
			if (room == null)
			{
				return;
			}
			if (!room.CheckRights(this.Session) && room.WhoCanKick != 2)
			{
				return;
			}
			uint pId = this.Request.PopWiredUInt();
			RoomUser roomUserByHabbo = room.GetRoomUserManager().GetRoomUserByHabbo(pId);
			if (roomUserByHabbo == null || roomUserByHabbo.IsBot)
			{
				return;
			}
			if (room.CheckRights(roomUserByHabbo.GetClient(), true, false) || roomUserByHabbo.GetClient().GetHabbo().HasFuse("fuse_mod") || roomUserByHabbo.GetClient().GetHabbo().HasFuse("fuse_no_kick"))
			{
				return;
			}
			room.GetRoomUserManager().RemoveUserFromRoom(roomUserByHabbo.GetClient(), true, true);
			roomUserByHabbo.GetClient().CurrentRoomUserID = -1;
		}
		internal void BanUser()
		{
			Room room = CyberEnvironment.GetGame().GetRoomManager().GetRoom(this.Session.GetHabbo().CurrentRoomId);
			if (room == null || (room.WhoCanBan == 0 && !room.CheckRights(this.Session, true, false)) || (room.WhoCanBan == 1 && !room.CheckRights(this.Session)))
			{
				return;
			}
			int num = this.Request.PopWiredInt32();
			this.Request.PopWiredUInt();
			string text = this.Request.PopFixedString();
			RoomUser roomUserByHabbo = room.GetRoomUserManager().GetRoomUserByHabbo(Convert.ToUInt32(num));
			if (roomUserByHabbo == null || roomUserByHabbo.IsBot)
			{
				return;
			}
			if (roomUserByHabbo.GetClient().GetHabbo().HasFuse("fuse_mod") || roomUserByHabbo.GetClient().GetHabbo().HasFuse("fuse_no_kick"))
			{
				return;
			}
			long time = 0L;
			if (text.ToLower().Contains("hour"))
			{
				time = 3600L;
			}
			else
			{
				if (text.ToLower().Contains("day"))
				{
					time = 86400L;
				}
				else
				{
					if (text.ToLower().Contains("perm"))
					{
						time = 788922000L;
					}
				}
			}
			room.AddBan(num, time);
			room.GetRoomUserManager().RemoveUserFromRoom(roomUserByHabbo.GetClient(), true, true);
			this.Session.CurrentRoomUserID = -1;
		}
		internal void SetHomeRoom()
		{
			uint num = this.Request.PopWiredUInt();
			RoomData roomData = CyberEnvironment.GetGame().GetRoomManager().GenerateRoomData(num);
			if (num != 0u && roomData == null)
			{
				return;
			}
			this.Session.GetHabbo().HomeRoom = num;
			using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
			{
				queryreactor.runFastQuery(string.Concat(new object[]
				{
					"UPDATE users SET home_room = ",
					num,
					" WHERE id = ",
					this.Session.GetHabbo().Id
				}));
			}
			this.Response.Init(Outgoing.HomeRoomMessageComposer);
			this.Response.AppendUInt(num);
			this.Response.AppendUInt(num);
			this.SendResponse();
		}
		internal void DeleteRoom()
		{
			uint RoomId = this.Request.PopWiredUInt();
			if (this.Session == null || this.Session.GetHabbo() == null || this.Session.GetHabbo().UsersRooms == null)
			{
				return;
			}
			Room room = CyberEnvironment.GetGame().GetRoomManager().GetRoom(RoomId);
			if (room == null)
			{
				return;
			}
			if (room.Owner == this.Session.GetHabbo().Username || this.Session.GetHabbo().Rank > 6u)
			{
				if (this.Session.GetHabbo().GetInventoryComponent() != null)
				{
					this.Session.GetHabbo().GetInventoryComponent().AddItemArray(room.GetRoomItemHandler().RemoveAllFurniture(this.Session));
				}
				RoomData roomData = room.RoomData;
				CyberEnvironment.GetGame().GetRoomManager().UnloadRoom(room);
				CyberEnvironment.GetGame().GetRoomManager().QueueVoteRemove(roomData);
				if (roomData == null || this.Session == null)
				{
					return;
				}
				using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
				{
					queryreactor.runFastQuery("DELETE FROM rooms WHERE id = " + RoomId);
					queryreactor.runFastQuery("DELETE FROM user_favorites WHERE room_id = " + RoomId);
					queryreactor.runFastQuery("DELETE FROM items WHERE room_id = " + RoomId);
					queryreactor.runFastQuery("DELETE FROM room_rights WHERE room_id = " + RoomId);
					queryreactor.runFastQuery("UPDATE users SET home_room = '0' WHERE home_room = " + RoomId);
				}
				if (this.Session.GetHabbo().Rank > 5u && this.Session.GetHabbo().Username != roomData.Owner)
				{
					CyberEnvironment.GetGame().GetModerationTool().LogStaffEntry(this.Session.GetHabbo().Username, roomData.Name, "Room deletion", string.Format("Deleted room ID {0}", roomData.Id));
				}
				RoomData roomData2 = (
					from p in this.Session.GetHabbo().UsersRooms
					where p.Id == RoomId
					select p).SingleOrDefault<RoomData>();
				if (roomData2 != null)
				{
					this.Session.GetHabbo().UsersRooms.Remove(roomData2);
				}
			}
		}
		internal void LookAt()
		{
			Room room = CyberEnvironment.GetGame().GetRoomManager().GetRoom(this.Session.GetHabbo().CurrentRoomId);
			if (room == null)
			{
				return;
			}
			RoomUser roomUserByHabbo = room.GetRoomUserManager().GetRoomUserByHabbo(this.Session.GetHabbo().Id);
			if (roomUserByHabbo == null)
			{
				return;
			}
			roomUserByHabbo.UnIdle();
			int num = this.Request.PopWiredInt32();
			int num2 = this.Request.PopWiredInt32();
			if (num == roomUserByHabbo.X && num2 == roomUserByHabbo.Y)
			{
				return;
			}
            int rotation = PathFinder.CalculateRotation(roomUserByHabbo.X, roomUserByHabbo.Y, num, num2);
			roomUserByHabbo.SetRot(rotation, false);
			roomUserByHabbo.UpdateNeeded = true;
			if (roomUserByHabbo.RidingHorse)
			{
				RoomUser roomUserByVirtualId = this.Session.GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUserByVirtualId(Convert.ToInt32(roomUserByHabbo.HorseID));
				roomUserByVirtualId.SetRot(rotation, false);
				roomUserByVirtualId.UpdateNeeded = true;
			}
		}
		internal void StartTyping()
		{
			Room room = CyberEnvironment.GetGame().GetRoomManager().GetRoom(this.Session.GetHabbo().CurrentRoomId);
			if (room == null)
			{
				return;
			}
			RoomUser roomUserByHabbo = room.GetRoomUserManager().GetRoomUserByHabbo(this.Session.GetHabbo().Id);
			if (roomUserByHabbo == null)
			{
				return;
			}
			ServerMessage serverMessage = new ServerMessage(Outgoing.TypingStatusMessageComposer);
			serverMessage.AppendInt32(roomUserByHabbo.VirtualId);
			serverMessage.AppendInt32(1);
			room.SendMessage(serverMessage);
		}
		internal void StopTyping()
		{
			Room room = CyberEnvironment.GetGame().GetRoomManager().GetRoom(this.Session.GetHabbo().CurrentRoomId);
			if (room == null)
			{
				return;
			}
			RoomUser roomUserByHabbo = room.GetRoomUserManager().GetRoomUserByHabbo(this.Session.GetHabbo().Id);
			if (roomUserByHabbo == null)
			{
				return;
			}
			ServerMessage serverMessage = new ServerMessage(Outgoing.TypingStatusMessageComposer);
			serverMessage.AppendInt32(roomUserByHabbo.VirtualId);
			serverMessage.AppendInt32(0);
			room.SendMessage(serverMessage);
		}
		internal void IgnoreUser()
		{
			if (this.Session.GetHabbo().CurrentRoom == null)
			{
				return;
			}
			string text = this.Request.PopFixedString();
			Habbo habbo = CyberEnvironment.GetGame().GetClientManager().GetClientByUsername(text).GetHabbo();
			if (habbo == null)
			{
				return;
			}
			if (this.Session.GetHabbo().MutedUsers.Contains(habbo.Id) || habbo.Rank > 4u)
			{
				return;
			}
			this.Session.GetHabbo().MutedUsers.Add(habbo.Id);
			this.Response.Init(Outgoing.UpdateIgnoreStatusMessageComposer);
			this.Response.AppendInt32(1);
			this.Response.AppendString(text);
			this.SendResponse();
		}
		internal void UnignoreUser()
		{
			if (this.Session.GetHabbo().CurrentRoom == null)
			{
				return;
			}
			string text = this.Request.PopFixedString();
			Habbo habbo = CyberEnvironment.GetGame().GetClientManager().GetClientByUsername(text).GetHabbo();
			if (habbo == null)
			{
				return;
			}
			if (!this.Session.GetHabbo().MutedUsers.Contains(habbo.Id))
			{
				return;
			}
			this.Session.GetHabbo().MutedUsers.Remove(habbo.Id);
			this.Response.Init(Outgoing.UpdateIgnoreStatusMessageComposer);
			this.Response.AppendInt32(3);
			this.Response.AppendString(text);
			this.SendResponse();
		}
		internal void CanCreateRoomEvent()
		{
			Room room = CyberEnvironment.GetGame().GetRoomManager().GetRoom(this.Session.GetHabbo().CurrentRoomId);
			if (room == null || !room.CheckRights(this.Session, true, false))
			{
				return;
			}
			bool b = true;
			int i = 0;
			if (room.State != 0)
			{
				b = false;
				i = 3;
			}
			this.Response.AppendBoolean(b);
			this.Response.AppendInt32(i);
		}
		internal void Sign()
		{
			Room room = CyberEnvironment.GetGame().GetRoomManager().GetRoom(this.Session.GetHabbo().CurrentRoomId);
			if (room == null)
			{
				return;
			}
			RoomUser roomUserByHabbo = room.GetRoomUserManager().GetRoomUserByHabbo(this.Session.GetHabbo().Id);
			if (roomUserByHabbo == null)
			{
				return;
			}
			roomUserByHabbo.UnIdle();
			int value = this.Request.PopWiredInt32();
			roomUserByHabbo.AddStatus("sign", Convert.ToString(value));
			roomUserByHabbo.UpdateNeeded = true;
			roomUserByHabbo.SignTime = checked(CyberEnvironment.GetUnixTimestamp() + 5);
		}
		internal void GetUserTags()
		{
			Room room = CyberEnvironment.GetGame().GetRoomManager().GetRoom(this.Session.GetHabbo().CurrentRoomId);
			if (room == null)
			{
				return;
			}
			RoomUser roomUserByHabbo = room.GetRoomUserManager().GetRoomUserByHabbo(this.Request.PopWiredUInt());
			if (roomUserByHabbo == null || roomUserByHabbo.IsBot)
			{
				return;
			}
			this.Response.Init(Outgoing.UserTagsMessageComposer);
			this.Response.AppendUInt(roomUserByHabbo.GetClient().GetHabbo().Id);
			this.Response.AppendInt32(roomUserByHabbo.GetClient().GetHabbo().Tags.Count);
			foreach (string current in roomUserByHabbo.GetClient().GetHabbo().Tags)
			{
				this.Response.AppendString(current);
			}
			this.SendResponse();

            if (Session == roomUserByHabbo.GetClient())
            {
                if (Session.GetHabbo().Tags.Count >= 5)
                {
                    CyberEnvironment.GetGame().GetAchievementManager().ProgressUserAchievement(roomUserByHabbo.GetClient(), "ACH_UserTags", 5, false);
                }
            }
		}
		internal void GetUserBadges()
		{
			Room room = CyberEnvironment.GetGame().GetRoomManager().GetRoom(this.Session.GetHabbo().CurrentRoomId);
			if (room == null)
			{
				return;
			}
			RoomUser roomUserByHabbo = room.GetRoomUserManager().GetRoomUserByHabbo(this.Request.PopWiredUInt());
			if (roomUserByHabbo == null || roomUserByHabbo.IsBot)
			{
				return;
			}
			if (roomUserByHabbo.GetClient() == null)
			{
				return;
			}
			this.Response.Init(Outgoing.UserBadgesMessageComposer);
			this.Response.AppendUInt(roomUserByHabbo.GetClient().GetHabbo().Id);
			this.Response.AppendInt32(roomUserByHabbo.GetClient().GetHabbo().GetBadgeComponent().EquippedCount);
			foreach (Badge badge in roomUserByHabbo.GetClient().GetHabbo().GetBadgeComponent().BadgeList.Values)
			{
				if (badge.Slot > 0)
				{
					this.Response.AppendInt32(badge.Slot);
					this.Response.AppendString(badge.Code);
				}
			}
			this.SendResponse();
		}
		internal void RateRoom()
		{
			Room room = CyberEnvironment.GetGame().GetRoomManager().GetRoom(this.Session.GetHabbo().CurrentRoomId);
			if (room == null || this.Session.GetHabbo().RatedRooms.Contains(room.RoomId) || room.CheckRights(this.Session, true, false))
			{
				return;
			}
			checked
			{
				switch (this.Request.PopWiredInt32())
				{
				case -1:
					room.Score--;
					room.RoomData.Score--;
					break;
				case 0:
					return;
				case 1:
					room.Score++;
					room.RoomData.Score++;
					break;
				default:
					return;
				}
				CyberEnvironment.GetGame().GetRoomManager().QueueVoteAdd(room.RoomData);
				using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
				{
					queryreactor.runFastQuery(string.Concat(new object[]
					{
						"UPDATE rooms SET score = ",
						room.Score,
						" WHERE id = ",
						room.RoomId
					}));
				}
				this.Session.GetHabbo().RatedRooms.Add(room.RoomId);
				this.Response.Init(Outgoing.RoomRatingMessageComposer);
				this.Response.AppendInt32(room.Score);
				this.Response.AppendBoolean(room.CheckRights(this.Session, true, false));
				this.SendResponse();
			}
		}
		internal void Dance()
		{
			Room room = CyberEnvironment.GetGame().GetRoomManager().GetRoom(this.Session.GetHabbo().CurrentRoomId);
			if (room == null)
			{
				return;
			}
			RoomUser roomUserByHabbo = room.GetRoomUserManager().GetRoomUserByHabbo(this.Session.GetHabbo().Id);
			if (roomUserByHabbo == null)
			{
				return;
			}
			roomUserByHabbo.UnIdle();
			int num = this.Request.PopWiredInt32();
			if (num < 0 || num > 4)
			{
				num = 0;
			}
			if (num > 0 && roomUserByHabbo.CarryItemID > 0)
			{
				roomUserByHabbo.CarryItem(0);
			}
			roomUserByHabbo.DanceId = num;
			ServerMessage serverMessage = new ServerMessage(Outgoing.DanceStatusMessageComposer);
			serverMessage.AppendInt32(roomUserByHabbo.VirtualId);
			serverMessage.AppendInt32(num);
			room.SendMessage(serverMessage);
			CyberEnvironment.GetGame().GetQuestManager().ProgressUserQuest(this.Session, QuestType.SOCIAL_DANCE, 0u);
			if (room.GetRoomUserManager().GetRoomUsers().Count > 19)
			{
				CyberEnvironment.GetGame().GetQuestManager().ProgressUserQuest(this.Session, QuestType.MASS_DANCE, 0u);
			}
		}
		internal void AnswerDoorbell()
		{
			Room room = CyberEnvironment.GetGame().GetRoomManager().GetRoom(this.Session.GetHabbo().CurrentRoomId);
			if (room == null || !room.CheckRights(this.Session))
			{
				return;
			}
			string username = this.Request.PopFixedString();
			bool flag = this.Request.PopWiredBoolean();
			GameClient clientByUsername = CyberEnvironment.GetGame().GetClientManager().GetClientByUsername(username);
			if (clientByUsername == null)
			{
				return;
			}
			if (flag)
			{
				clientByUsername.GetHabbo().LoadingChecksPassed = true;
				clientByUsername.GetMessageHandler().Response.Init(Outgoing.DoorbellOpenedMessageComposer);
				clientByUsername.GetMessageHandler().Response.AppendString("");
				clientByUsername.GetMessageHandler().SendResponse();
				return;
			}
			if (clientByUsername.GetHabbo().CurrentRoomId != this.Session.GetHabbo().CurrentRoomId)
			{
				clientByUsername.GetMessageHandler().Response.Init(Outgoing.DoorbellNoOneMessageComposer);
				clientByUsername.GetMessageHandler().Response.AppendString("");
				clientByUsername.GetMessageHandler().SendResponse();
			}
		}
		internal void AlterRoomFilter()
		{
			uint num = this.Request.PopWiredUInt();
			bool flag = this.Request.PopWiredBoolean();
			string text = this.Request.PopFixedString();
			Room room = CyberEnvironment.GetGame().GetRoomManager().GetRoom(this.Session.GetHabbo().CurrentRoomId);
			if (room == null || !room.CheckRights(this.Session, true, false))
			{
				return;
			}
			if (!flag)
			{
				if (!room.WordFilter.Contains(text))
				{
					return;
				}
				room.WordFilter.Remove(text);
				using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
				{
					queryreactor.setQuery("DELETE FROM room_wordfilter WHERE room_id = @id AND word = @word");
					queryreactor.addParameter("id", num);
					queryreactor.addParameter("word", text);
					queryreactor.runQuery();
					return;
				}
			}
			if (room.WordFilter.Contains(text))
			{
				return;
			}
            else if (text.Contains("+"))
            {
                Session.SendNotif("No puedes colocar ninguna palabra que tenga un ' + ' por asuntos técnicos.");
                return;
            }
			room.WordFilter.Add(text);
			using (IQueryAdapter queryreactor2 = CyberEnvironment.GetDatabaseManager().getQueryReactor())
			{
				queryreactor2.setQuery("INSERT INTO room_wordfilter (room_id, word) VALUES (@id, @word);");
				queryreactor2.addParameter("id", num);
				queryreactor2.addParameter("word", text);
				queryreactor2.runQuery();
			}
		}
		internal void GetRoomFilter()
		{
			uint roomID = this.Request.PopWiredUInt();
			Room room = CyberEnvironment.GetGame().GetRoomManager().GetRoom(roomID);
			if (room == null || !room.CheckRights(this.Session, true, false))
			{
				return;
			}
			ServerMessage serverMessage = new ServerMessage();
			serverMessage.Init(Outgoing.RoomLoadFilterMessageComposer);
			serverMessage.AppendInt32(room.WordFilter.Count);
			foreach (string current in room.WordFilter)
			{
				serverMessage.AppendString(current);
			}
			this.Response = serverMessage;
			this.SendResponse();
		}
		internal void ApplyRoomEffect()
		{
			Room room = CyberEnvironment.GetGame().GetRoomManager().GetRoom(this.Session.GetHabbo().CurrentRoomId);
			if (room == null || !room.CheckRights(this.Session, true, false))
			{
				return;
			}
			UserItem item = this.Session.GetHabbo().GetInventoryComponent().GetItem(this.Request.PopWiredUInt());
			if (item == null)
			{
				return;
			}
			string text = "floor";
			if (item.GetBaseItem().Name.ToLower().Contains("wallpaper"))
			{
				text = "wallpaper";
			}
			else
			{
				if (item.GetBaseItem().Name.ToLower().Contains("landscape"))
				{
					text = "landscape";
				}
			}
			string a;
			if ((a = text) != null)
			{
				if (!(a == "floor"))
				{
					if (!(a == "wallpaper"))
					{
						if (a == "landscape")
						{
							room.Landscape = item.ExtraData;
							room.RoomData.Landscape = item.ExtraData;
						}
					}
					else
					{
						room.Wallpaper = item.ExtraData;
						room.RoomData.Wallpaper = item.ExtraData;
						CyberEnvironment.GetGame().GetQuestManager().ProgressUserQuest(this.Session, QuestType.FURNI_DECORATION_WALL, 0u);
					}
				}
				else
				{
					room.Floor = item.ExtraData;
					room.RoomData.Floor = item.ExtraData;
					CyberEnvironment.GetGame().GetQuestManager().ProgressUserQuest(this.Session, QuestType.FURNI_DECORATION_FLOOR, 0u);
				}
			}
			using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
			{
				queryreactor.setQuery(string.Concat(new object[]
				{
					"UPDATE rooms SET ",
					text,
					" = @extradata WHERE id = ",
					room.RoomId
				}));
				queryreactor.addParameter("extradata", item.ExtraData);
				queryreactor.runQuery();
				queryreactor.runFastQuery("DELETE FROM items WHERE id=" + item.Id + " LIMIT 1");
			}
			this.Session.GetHabbo().GetInventoryComponent().RemoveItem(item.Id, false);
			ServerMessage serverMessage = new ServerMessage(Outgoing.RoomSpacesMessageComposer);
			serverMessage.AppendString(text);
			serverMessage.AppendString(item.ExtraData);
			room.SendMessage(serverMessage);
		}
		internal void PlacePostIt()
		{
			Room room = CyberEnvironment.GetGame().GetRoomManager().GetRoom(this.Session.GetHabbo().CurrentRoomId);
			if (room == null || !room.CheckRights(this.Session))
			{
				return;
			}
			uint id = this.Request.PopWiredUInt();
			string text = this.Request.PopFixedString();
			UserItem item = this.Session.GetHabbo().GetInventoryComponent().GetItem(id);
			if (item == null || room == null)
			{
				return;
			}
			try
			{
				string wallCoord = this.WallPositionCheck(":" + text.Split(new char[]
				{
					':'
				})[1]);
				RoomItem item2 = new RoomItem(item.Id, room.RoomId, item.BaseItem, item.ExtraData, wallCoord, room, this.Session.GetHabbo().Id, item.GroupId, item.GetBaseItem().FlatId);
				if (room.GetRoomItemHandler().SetWallItem(this.Session, item2))
				{
					this.Session.GetHabbo().GetInventoryComponent().RemoveItem(id, true);
				}
			}
			catch
			{
			}
		}
		internal void PlaceItem()
		{
			Room room = CyberEnvironment.GetGame().GetRoomManager().GetRoom(this.Session.GetHabbo().CurrentRoomId);
			if (room == null)
			{
				return;
			}
			if (CyberEnvironment.GetDBConfig().DBData["placing_enabled"] != "1")
			{
				return;
			}
			string text = this.Request.PopFixedString();
			string[] array = text.Split(new char[]
			{
				' '
			});
			int value = int.Parse(array[0]);
			uint id;
			UserItem item;
			checked
			{
				id = (uint)Math.Abs(value);
				item = this.Session.GetHabbo().GetInventoryComponent().GetItem(id);
				if (item == null)
				{
					return;
				}
				if (room.GetRoomItemHandler().mFloorItems.Values.Count + room.GetRoomItemHandler().mWallItems.Values.Count > 3000)
				{
					this.Session.SendNotif("You can't have more than 3000 items in a room!");
					return;
				}
			}
			InteractionType interactionType = item.GetBaseItem().InteractionType;
			if (interactionType <= InteractionType.onewaygate)
			{
				if (interactionType != InteractionType.dimmer)
				{
					if (interactionType != InteractionType.onewaygate)
					{
						goto IL_2AD;
					}
				}
				else
				{
					MoodlightData moodlightData = room.MoodlightData;
					if (moodlightData != null && room.GetRoomItemHandler().GetItem(moodlightData.ItemId) != null)
					{
						this.Session.SendNotif("Sólo puedes tener un regu por sala!");
						return;
					}
					goto IL_2AD;
				}
			}
			else
			{
				switch (interactionType)
				{
				case InteractionType.hopper:
					if (room.GetRoomItemHandler().HopperCount > 0)
					{
						return;
					}
					goto IL_2AD;
				case InteractionType.teleport:
					break;
				default:
                    switch (interactionType)
                    {
                        case InteractionType.freezetileblock:
                        case InteractionType.freezetile:
                            {
                                List<RoomItem> roomItemForSquare = room.GetGameMap().GetRoomItemForSquare(int.Parse(array[1]), int.Parse(array[2]));
                                bool flag = false;
                                foreach (RoomItem current in roomItemForSquare)
                                {
                                    if (current.GetBaseItem().InteractionType == InteractionType.freezetile)
                                    {
                                        flag = true;
                                    }
                                }
                                if (!flag)
                                {
                                    return;
                                }
                                goto IL_2AD;
                            }
                        default:
                            {
                                if (interactionType != InteractionType.roombg)
                                {
                                    goto IL_2AD;
                                }
                                TonerData tonerData = room.TonerData;
                                if (tonerData != null && room.GetRoomItemHandler().GetItem(tonerData.ItemId) != null)
                                {
                                    this.Session.SendNotif("Sólo puedes tener un tóner por sala!");
                                    return;
                                }
                                goto IL_2AD;
                            }
                    }
				}
			}
			
			IL_2AD:
			if (room.CheckRights(this.Session))
			{
				using (IQueryAdapter queryreactor2 = CyberEnvironment.GetDatabaseManager().getQueryReactor())
				{
					queryreactor2.runFastQuery(string.Concat(new object[]
					{
						"UPDATE items SET user_id=",
						room.OwnerId,
						" WHERE id=",
						item.Id,
						" LIMIT 1"
					}));
				}
				if (array[1].StartsWith(":"))
				{
					try
					{
						string wallCoord = this.WallPositionCheck(":" + text.Split(new char[]
						{
							':'
						})[1]);
						RoomItem item2 = new RoomItem(item.Id, room.RoomId, item.BaseItem, item.ExtraData, wallCoord, room, Convert.ToUInt32(room.OwnerId), item.GroupId, item.GetBaseItem().FlatId);
						if (room.GetRoomItemHandler().SetWallItem(this.Session, item2))
						{
							this.Session.GetHabbo().GetInventoryComponent().RemoveItem(id, true);
						}
						return;
					}
					catch
					{
						return;
					}
				}
				int num = int.Parse(array[1]);
				int num2 = int.Parse(array[2]);
				int num3 = int.Parse(array[3]);
				RoomItem roomItem = new RoomItem(item.Id, room.RoomId, item.BaseItem, item.ExtraData, num, num2, 0.0, num3, room, Convert.ToUInt32(room.OwnerId), item.GroupId, item.GetBaseItem().FlatId, item.SongCode);
				if (room.GetRoomItemHandler().SetFloorItem(this.Session, roomItem, num, num2, num3, true, false, true))
				{
					this.Session.GetHabbo().GetInventoryComponent().RemoveItem(id, true);
					if (roomItem.IsWired)
					{
						WiredItem item3 = room.GetWiredHandler().GenerateNewItem(roomItem);
                        if (item3 != null)
                        {
                            room.GetWiredHandler().SaveWired(item3);
                            room.GetWiredHandler().AddWired(item3);
                        }
					}
				}
				CyberEnvironment.GetGame().GetQuestManager().ProgressUserQuest(this.Session, QuestType.FURNI_PLACE, 0u);
				return;
			}
			if (room.CheckRights(this.Session, true, true))
			{
				if (array[1].StartsWith(":"))
				{
					try
					{
						string wallCoord2 = this.WallPositionCheck(":" + text.Split(new char[]
						{
							':'
						})[1]);
						RoomItem item4 = new RoomItem(item.Id, room.RoomId, item.BaseItem, item.ExtraData, wallCoord2, room, this.Session.GetHabbo().Id, item.GroupId, item.GetBaseItem().FlatId);
						if (room.GetRoomItemHandler().SetWallItem(this.Session, item4))
						{
							this.Session.GetHabbo().GetInventoryComponent().RemoveItem(id, true);
						}
						return;
					}
					catch
					{
						return;
					}
				}
				int num4 = int.Parse(array[1]);
				int num5 = int.Parse(array[2]);
				int num6 = int.Parse(array[3]);
				RoomItem roomItem2 = new RoomItem(item.Id, room.RoomId, item.BaseItem, item.ExtraData, num4, num5, 0.0, num6, room, this.Session.GetHabbo().Id, item.GroupId, item.GetBaseItem().FlatId, item.SongCode);
				if (room.GetRoomItemHandler().SetFloorItem(this.Session, roomItem2, num4, num5, num6, true, false, true))
				{
					this.Session.GetHabbo().GetInventoryComponent().RemoveItem(id, true);
					if (roomItem2.IsWired)
					{
						WiredItem item5 = room.GetWiredHandler().GenerateNewItem(roomItem2);
                        if (item5 != null)
                        {
                            room.GetWiredHandler().AddWired(item5);
                            room.GetWiredHandler().SaveWired(item5);
                        }
					}

				}
				CyberEnvironment.GetGame().GetQuestManager().ProgressUserQuest(this.Session, QuestType.FURNI_PLACE, 0u);
			}
		}
		internal void TakeItem()
		{
            // New Method
            this.Request.PopWiredInt32();
            Room Room = Session.GetHabbo().CurrentRoom;
            RoomItem Item = Room.GetRoomItemHandler().GetItem(this.Request.PopWiredUInt());

            if (Room != null && Room.CheckRights(this.Session, true))
            {
                if (Item != null && Item.GetBaseItem().InteractionType != InteractionType.postit)
                {
                    Room.GetRoomItemHandler().RemoveFurniture(this.Session, Item.Id, true);
                    this.Session.GetHabbo().GetInventoryComponent().AddNewItem(Item.Id, Item.BaseItem, Item.ExtraData, Item.GroupId, true, true, 0, 0, "");
                    Session.GetHabbo().GetInventoryComponent().UpdateItems(false);
                }
            }
		}

		internal void MoveItem()
		{

            uint pId = Convert.ToUInt32(Math.Abs(Request.PopWiredInt32()));
			Room room = CyberEnvironment.GetGame().GetRoomManager().GetRoom(this.Session.GetHabbo().CurrentRoomId);
			if (room == null)
			{
				return;
			}
			RoomItem item;
			if (room.Group != null)
			{
				if (room == null)
				{
					return;
				}
				if (!room.CheckRights(this.Session, false, true))
				{
					item = room.GetRoomItemHandler().GetItem(pId);
					if (item == null)
					{
						return;
					}
					ServerMessage message = new ServerMessage(Outgoing.UpdateRoomItemMessageComposer);
					item.Serialize(message);
					this.Session.SendMessage(message);
					return;
				}
			}
			else
			{
				if (room == null || !room.CheckRights(this.Session))
				{
					return;
				}
			}
			item = room.GetRoomItemHandler().GetItem(pId);
			if (item == null)
			{
				return;
			}
			int num = this.Request.PopWiredInt32();
			int num2 = this.Request.PopWiredInt32();
			int num3 = this.Request.PopWiredInt32();
			this.Request.PopWiredInt32();
			bool flag = false;
			if (item.GetBaseItem().InteractionType == InteractionType.teleport || item.GetBaseItem().InteractionType == InteractionType.hopper)
			{
				flag = true;
			}
			if (num != item.GetX || num2 != item.GetY)
			{
				CyberEnvironment.GetGame().GetQuestManager().ProgressUserQuest(this.Session, QuestType.FURNI_MOVE, 0u);
			}
			if (num3 != item.Rot)
			{
				CyberEnvironment.GetGame().GetQuestManager().ProgressUserQuest(this.Session, QuestType.FURNI_ROTATE, 0u);
			}
            var OldCoords = item.GetCoords;
			if (!room.GetRoomItemHandler().SetFloorItem(this.Session, item, num, num2, num3, false, false, true))
			{
				ServerMessage message3 = new ServerMessage(Outgoing.UpdateRoomItemMessageComposer);
				item.Serialize(message3);
				room.SendMessage(message3);
				return;
			}
            
			if (item.GetZ >= 0.1)
			{
				CyberEnvironment.GetGame().GetQuestManager().ProgressUserQuest(this.Session, QuestType.FURNI_STACK, 0u);
			}
            var newcoords = item.GetCoords;
            room.GetRoomItemHandler().OnHeightmapUpdate(OldCoords, newcoords);
			if (flag)
			{
				using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
				{
					room.GetRoomItemHandler().SaveFurniture(queryreactor, null);
				}
			}
		}
		public string WallPositionCheck(string wallPosition)
		{
			string result;
			try
			{
				if (wallPosition.Contains(Convert.ToChar(13)))
				{
					result = null;
				}
				else
				{
					if (wallPosition.Contains(Convert.ToChar(9)))
					{
						result = null;
					}
					else
					{
						string[] array = wallPosition.Split(new char[]
						{
							' '
						});
						if (array[2] != "l" && array[2] != "r")
						{
							result = null;
						}
						else
						{
							string[] array2 = array[0].Substring(3).Split(new char[]
							{
								','
							});
							int num = int.Parse(array2[0]);
							int num2 = int.Parse(array2[1]);
							if (num < 0 || num2 < 0 || num > 200 || num2 > 200)
							{
								result = null;
							}
							else
							{
								string[] array3 = array[1].Substring(2).Split(new char[]
								{
									','
								});
								int num3 = int.Parse(array3[0]);
								int num4 = int.Parse(array3[1]);
								if (num3 < 0 || num4 < 0 || num3 > 200 || num4 > 200)
								{
									result = null;
								}
								else
								{
									result = string.Concat(new object[]
									{
										":w=",
										num,
										",",
										num2,
										" l=",
										num3,
										",",
										num4,
										" ",
										array[2]
									});
								}
							}
						}
					}
				}
			}
			catch
			{
				result = null;
			}
			return result;
		}
		internal void MoveWallItem()
		{
			Room room = CyberEnvironment.GetGame().GetRoomManager().GetRoom(this.Session.GetHabbo().CurrentRoomId);
			if (room == null || !room.CheckRights(this.Session))
			{
				return;
			}
			uint pId = this.Request.PopWiredUInt();
			string text = this.Request.PopFixedString();
			RoomItem item = room.GetRoomItemHandler().GetItem(pId);
			if (item == null)
			{
				return;
			}
			try
			{
				string wallCoord = this.WallPositionCheck(":" + text.Split(new char[]
				{
					':'
				})[1]);
				item.wallCoord = wallCoord;
			}
			catch
			{
				return;
			}
			room.GetRoomItemHandler().UpdateItem(item);
			ServerMessage message = new ServerMessage(Outgoing.UpdateRoomWallItemMessageComposer);
			item.Serialize(message);
			room.SendMessage(message);
		}
		internal void TriggerItem()
		{
			Room room = CyberEnvironment.GetGame().GetRoomManager().GetRoom(this.Session.GetHabbo().CurrentRoomId);
			if (room == null)
			{
				return;
			}
			int num = this.Request.PopWiredInt32();
			if (num < 0)
			{
				return;
			}
			uint pId = Convert.ToUInt32(num);
			RoomItem item = room.GetRoomItemHandler().GetItem(pId);
			if (item == null)
			{
				return;
			}
			bool hasRights = false;
			if (room.CheckRights(this.Session, false, true))
			{
				hasRights = true;
			}
			if (item.GetBaseItem().InteractionType == InteractionType.roombg)
			{
				if (!room.CheckRights(this.Session, true, false))
				{
					return;
				}
				if (room.TonerData.Enabled == 0)
				{
					room.TonerData.Enabled = 1;
				}
				else
				{
					room.TonerData.Enabled = 0;
				}
				ServerMessage message = new ServerMessage(Outgoing.UpdateRoomItemMessageComposer);
				item.Serialize(message);
				room.SendMessage(message);
				item.UpdateState();
				using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
				{
					queryreactor.runFastQuery("UPDATE room_items_toner SET enabled = '" + room.TonerData.Enabled + "' LIMIT 1");
				}
			}
			else
			{
                if (item.GetBaseItem().InteractionType == InteractionType.moplaseed)
                {
                    this.PlantMonsterplant(item, room);
                    return;
                }
				string arg_15A_0 = item.ExtraData;
				int request = this.Request.PopWiredInt32();
				item.Interactor.OnTrigger(this.Session, item, request, hasRights);
				item.OnTrigger(room.GetRoomUserManager().GetRoomUserByHabbo(this.Session.GetHabbo().Id));
				CyberEnvironment.GetGame().GetQuestManager().ProgressUserQuest(this.Session, QuestType.EXPLORE_FIND_ITEM, item.GetBaseItem().ItemId);
				foreach (RoomUser current in room.GetRoomUserManager().UserList.Values)
				{
					if (current != null)
					{
						room.GetRoomUserManager().UpdateUserStatus(current, true);
					}
				}
			}
		}
		internal void TriggerItemDiceSpecial()
		{
			Room room = CyberEnvironment.GetGame().GetRoomManager().GetRoom(this.Session.GetHabbo().CurrentRoomId);
			if (room == null)
			{
				return;
			}
			RoomItem item = room.GetRoomItemHandler().GetItem(this.Request.PopWiredUInt());
			if (item == null)
			{
				return;
			}
			bool hasRights = false;
			if (room.CheckRights(this.Session))
			{
				hasRights = true;
			}
			item.Interactor.OnTrigger(this.Session, item, -1, hasRights);
			item.OnTrigger(room.GetRoomUserManager().GetRoomUserByHabbo(this.Session.GetHabbo().Id));
		}
		internal void OpenPostit()
		{
			Room room = CyberEnvironment.GetGame().GetRoomManager().GetRoom(this.Session.GetHabbo().CurrentRoomId);
			if (room == null)
			{
				return;
			}
			RoomItem item = room.GetRoomItemHandler().GetItem(this.Request.PopWiredUInt());
			if (item == null || item.GetBaseItem().InteractionType != InteractionType.postit)
			{
				return;
			}
			this.Response.Init(Outgoing.LoadPostItMessageComposer);
			this.Response.AppendString(item.Id.ToString());
			this.Response.AppendString(item.ExtraData);
			this.SendResponse();
		}
		internal void SavePostit()
		{
			Room room = CyberEnvironment.GetGame().GetRoomManager().GetRoom(this.Session.GetHabbo().CurrentRoomId);
			if (room == null)
			{
				return;
			}
			RoomItem item = room.GetRoomItemHandler().GetItem(this.Request.PopWiredUInt());
			if (item == null || item.GetBaseItem().InteractionType != InteractionType.postit)
			{
				return;
			}
			string text = this.Request.PopFixedString();
			string text2 = this.Request.PopFixedString();
			if (!room.CheckRights(this.Session) && !text2.StartsWith(item.ExtraData))
			{
				return;
			}
			string a;
			if ((a = text) == null || (!(a == "FFFF33") && !(a == "FF9CFF") && !(a == "9CCEFF") && !(a == "9CFF9C")))
			{
				return;
			}
			item.ExtraData = text + " " + text2;
			item.UpdateState(true, true);
		}
		internal void DeletePostit()
		{
			Room room = CyberEnvironment.GetGame().GetRoomManager().GetRoom(this.Session.GetHabbo().CurrentRoomId);
			if (room == null || !room.CheckRights(this.Session, true, false))
			{
				return;
			}
			RoomItem item = room.GetRoomItemHandler().GetItem(this.Request.PopWiredUInt());
			if (item == null || item.GetBaseItem().InteractionType != InteractionType.postit)
			{
				return;
			}
			room.GetRoomItemHandler().RemoveFurniture(this.Session, item.Id, true);
		}
		internal void OpenGift()
		{
			if ((DateTime.Now - this.Session.GetHabbo().LastGiftOpenTime).TotalSeconds <= 15.0)
			{
				this.Session.SendNotif("¡Estás abriendo regalos demasiado rápido! Por favor, espera un poco antes de abrir otro. Esto se hace por seguridad.");
				return;
			}
			Room currentRoom = this.Session.GetHabbo().CurrentRoom;
			if (currentRoom == null)
			{
				this.Session.SendWhisper("Ha ocurrido un error, por favor infórmaselo al técnico. El error es: Room is null.");
				return;
			}
			if (!currentRoom.CheckRights(this.Session, true, false))
			{
				this.Session.SendWhisper("Ha ocurrido un error, por favor infórmaselo al técnico. El error es: You can't open gifts.");
				return;
			}
			uint pId = this.Request.PopWiredUInt();
			RoomItem item = currentRoom.GetRoomItemHandler().GetItem(pId);
			if (item == null)
			{
				this.Session.SendWhisper("Ha ocurrido un error, por favor infórmaselo al técnico. El error es: RoomGift is null.");
				return;
			}
			item.MagicRemove = true;
			ServerMessage message = new ServerMessage(Outgoing.UpdateRoomItemMessageComposer);
			item.Serialize(message);
			currentRoom.SendMessage(message);
			this.Session.GetHabbo().LastGiftOpenTime = DateTime.Now;

			IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor();
			queryreactor.setQuery("SELECT * FROM user_gifts WHERE gift_id = " + item.Id);
			DataRow row = queryreactor.getRow();
			if (row == null)
			{
				currentRoom.GetRoomItemHandler().RemoveFurniture(this.Session, item.Id, true);
				return;
			}
			Item item2 = CyberEnvironment.GetGame().GetItemManager().GetItem(Convert.ToUInt32(row["item_id"]));
			if (item2 == null)
			{
				currentRoom.GetRoomItemHandler().RemoveFurniture(this.Session, item.Id, true);
				return;
			}
			if (item2.Type.Equals('s'))
			{
				currentRoom.GetRoomItemHandler().RemoveFurniture(this.Session, item.Id, false);
				string text = row["extradata"].ToString();
				uint num = uint.Parse(row["item_id"].ToString());
				queryreactor.runFastQuery(string.Concat(new object[]
				{
					"UPDATE items SET base_item = '",
					num,
					"' WHERE id = ",
					item.Id
				}));
				queryreactor.setQuery("UPDATE items SET extra_data = @extradata WHERE id = " + item.Id);
				queryreactor.addParameter("extradata", text);
				queryreactor.runQuery();
				queryreactor.runFastQuery("DELETE FROM user_gifts WHERE gift_id = " + item.Id);
				item.BaseItem = num;
				item.refreshItem();
				item.ExtraData = text;
				if (!currentRoom.GetRoomItemHandler().SetFloorItem(item, item.GetX, item.GetY, item.GetZ, item.Rot, true))
				{
					this.Session.SendNotif("¡No se pudo crear tu regalo!");
				}
				else
				{
					this.Response.Init(Outgoing.OpenGiftMessageComposer);
					this.Response.AppendString(item2.Type.ToString());
					this.Response.AppendInt32(item2.SpriteId);
					this.Response.AppendString(item2.Name);
					this.Response.AppendUInt(item2.ItemId);
					this.Response.AppendString(item2.Type.ToString());
					this.Response.AppendBoolean(true);
					this.Response.AppendString(text);
					this.SendResponse();
					ServerMessage serverMessage = new ServerMessage(Outgoing.AddFloorItemMessageComposer);
					item.Serialize(serverMessage);
					serverMessage.AppendString(currentRoom.Owner);
					currentRoom.SendMessage(serverMessage);
					currentRoom.GetRoomItemHandler().SetFloorItem(this.Session, item, item.GetX, item.GetY, 0, true, false, true);
				}
			}
			else
			{
				currentRoom.GetRoomItemHandler().RemoveFurniture(this.Session, item.Id, true);
				queryreactor.runFastQuery("DELETE FROM user_gifts WHERE gift_id = " + item.Id);
				this.Response.Init(Outgoing.NewInventoryObjectMessageComposer);
				this.Response.AppendInt32(1);
				int i = 2;
				if (item2.Type.ToString().ToLower().Equals("s"))
				{
					if (item2.InteractionType == InteractionType.pet)
					{
						i = 3;
					}
					else
					{
						i = 1;
					}
				}
				this.Response.AppendInt32(i);
				List<UserItem> list = CyberEnvironment.GetGame().GetCatalog().DeliverItems(this.Session, item2, 1, (string)row["extradata"], 0, 0, "");
				this.Response.AppendInt32(list.Count);
				foreach (UserItem current in list)
				{
					this.Response.AppendUInt(current.Id);
				}
				this.SendResponse();
				this.Session.GetHabbo().GetInventoryComponent().UpdateItems(true);
			}
			this.Response.Init(Outgoing.UpdateInventoryMessageComposer);
			this.SendResponse();
		}
		internal void GetMoodlight()
		{
			Room room = CyberEnvironment.GetGame().GetRoomManager().GetRoom(this.Session.GetHabbo().CurrentRoomId);
			if (room == null || !room.CheckRights(this.Session, true, false))
			{
				return;
			}
			if (room.MoodlightData == null)
			{
				foreach (RoomItem current in room.GetRoomItemHandler().mWallItems.Values)
				{
					if (current.GetBaseItem().InteractionType == InteractionType.dimmer)
					{
						room.MoodlightData = new MoodlightData(current.Id);
					}
				}
			}
			if (room.MoodlightData == null)
			{
				return;
			}
			this.Response.Init(Outgoing.DimmerDataMessageComposer);
			this.Response.AppendInt32(room.MoodlightData.Presets.Count);
			this.Response.AppendInt32(room.MoodlightData.CurrentPreset);
			int num = 0;
			checked
			{
				foreach (MoodlightPreset current2 in room.MoodlightData.Presets)
				{
					num++;
					this.Response.AppendInt32(num);
					this.Response.AppendInt32(int.Parse(CyberEnvironment.BoolToEnum(current2.BackgroundOnly)) + 1);
					this.Response.AppendString(current2.ColorCode);
					this.Response.AppendInt32(current2.ColorIntensity);
				}
				this.SendResponse();
			}
		}
		internal void UpdateMoodlight()
		{
			Room room = CyberEnvironment.GetGame().GetRoomManager().GetRoom(this.Session.GetHabbo().CurrentRoomId);
			if (room == null || !room.CheckRights(this.Session, true, false) || room.MoodlightData == null)
			{
				return;
			}
			RoomItem item = room.GetRoomItemHandler().GetItem(room.MoodlightData.ItemId);
			if (item == null || item.GetBaseItem().InteractionType != InteractionType.dimmer)
			{
				return;
			}
			int num = this.Request.PopWiredInt32();
			int num2 = this.Request.PopWiredInt32();
			string color = this.Request.PopFixedString();
			int intensity = this.Request.PopWiredInt32();
			bool bgOnly = false;
			if (num2 >= 2)
			{
				bgOnly = true;
			}
			room.MoodlightData.Enabled = true;
			room.MoodlightData.CurrentPreset = num;
			room.MoodlightData.UpdatePreset(num, color, intensity, bgOnly, false);
			item.ExtraData = room.MoodlightData.GenerateExtraData();
			item.UpdateState();
		}
		internal void SwitchMoodlightStatus()
		{
			Room room = CyberEnvironment.GetGame().GetRoomManager().GetRoom(this.Session.GetHabbo().CurrentRoomId);
			if (room == null || !room.CheckRights(this.Session, true, false) || room.MoodlightData == null)
			{
				return;
			}
			RoomItem item = room.GetRoomItemHandler().GetItem(room.MoodlightData.ItemId);
			if (item == null || item.GetBaseItem().InteractionType != InteractionType.dimmer)
			{
				return;
			}
			if (room.MoodlightData.Enabled)
			{
				room.MoodlightData.Disable();
			}
			else
			{
				room.MoodlightData.Enable();
			}
			item.ExtraData = room.MoodlightData.GenerateExtraData();
			item.UpdateState();
		}
		internal void SaveRoomBg()
		{
			Room room = CyberEnvironment.GetGame().GetRoomManager().GetRoom(this.Session.GetHabbo().CurrentRoomId);
			if (room == null || !room.CheckRights(this.Session, true, false))
			{
				return;
			}
			RoomItem item = room.GetRoomItemHandler().GetItem(room.TonerData.ItemId);
			if (item == null || item.GetBaseItem().InteractionType != InteractionType.roombg)
			{
				return;
			}
			this.Request.PopWiredInt32();
			int num = this.Request.PopWiredInt32();
			int num2 = this.Request.PopWiredInt32();
			int num3 = this.Request.PopWiredInt32();
			if (num > 255 || num2 > 255 || num3 > 255)
			{
				return;
			}
			using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
			{
				queryreactor.runFastQuery(string.Concat(new object[]
				{
					"UPDATE room_items_toner SET enabled = '1', data1=",
					num,
					" ,data2=",
					num2,
					",data3=",
					num3,
					" WHERE id=",
					item.Id,
					" LIMIT 1"
				}));
			}
			room.TonerData.Data1 = num;
			room.TonerData.Data2 = num2;
			room.TonerData.Data3 = num3;
			room.TonerData.Enabled = 1;
			ServerMessage message = new ServerMessage(Outgoing.UpdateRoomItemMessageComposer);
			item.Serialize(message);
			room.SendMessage(message);
			item.UpdateState();
		}
		internal void InitTrade()
		{
			Room room = CyberEnvironment.GetGame().GetRoomManager().GetRoom(this.Session.GetHabbo().CurrentRoomId);
			if (room == null)
			{
				return;
			}
			if (room.TradeState == 0)
			{
				this.Session.SendNotif("Los tradeos están desactivados para esta Sala.");
				return;
			}
			else if (room.TradeState == 1 && !room.CheckRights(Session))
			{
				this.Session.SendNotif("Necesitas permisos para poder tradear en esta Sala.");
				return;
			}
			if (CyberEnvironment.GetDBConfig().DBData["trading_enabled"] != "1")
			{
				this.Session.SendNotif("El tradeo ha sido desactivado.");
				return;
			}
			if (!this.Session.GetHabbo().CheckTrading())
			{
				this.Session.SendNotif("No puedes tradear porque un Moderador ha desactivado tus tradeos.<br>Podrás volver a tradear en: " + DateTime.Now.AddSeconds((double)checked(CyberEnvironment.GetUnixTimestamp() - this.Session.GetHabbo().TradeLockExpire)).ToLongDateString());
			}
			RoomUser roomUserByHabbo = room.GetRoomUserManager().GetRoomUserByHabbo(this.Session.GetHabbo().Id);
			RoomUser roomUserByVirtualId = room.GetRoomUserManager().GetRoomUserByVirtualId(this.Request.PopWiredInt32());
			if (roomUserByVirtualId == null || roomUserByVirtualId.GetClient() == null || roomUserByVirtualId.GetClient().GetHabbo() == null)
			{
				return;
			}
			room.TryStartTrade(roomUserByHabbo, roomUserByVirtualId);
		}
		internal void OfferTradeItem()
		{
			Room room = CyberEnvironment.GetGame().GetRoomManager().GetRoom(this.Session.GetHabbo().CurrentRoomId);
			if (room == null || !room.CanTradeInRoom)
			{
				return;
			}
			Trade userTrade = room.GetUserTrade(this.Session.GetHabbo().Id);
			UserItem item = this.Session.GetHabbo().GetInventoryComponent().GetItem(this.Request.PopWiredUInt());
			if (userTrade == null || item == null)
			{
				return;
			}
			userTrade.OfferItem(this.Session.GetHabbo().Id, item);
		}
		internal void TakeBackTradeItem()
		{
			Room room = CyberEnvironment.GetGame().GetRoomManager().GetRoom(this.Session.GetHabbo().CurrentRoomId);
			if (room == null || !room.CanTradeInRoom)
			{
				return;
			}
			Trade userTrade = room.GetUserTrade(this.Session.GetHabbo().Id);
			UserItem item = this.Session.GetHabbo().GetInventoryComponent().GetItem(this.Request.PopWiredUInt());
			if (userTrade == null || item == null)
			{
				return;
			}
			userTrade.TakeBackItem(this.Session.GetHabbo().Id, item);
		}
		internal void StopTrade()
		{
			Room room = CyberEnvironment.GetGame().GetRoomManager().GetRoom(this.Session.GetHabbo().CurrentRoomId);
			if (room == null || !room.CanTradeInRoom)
			{
				return;
			}
			room.TryStopTrade(this.Session.GetHabbo().Id);
		}
		internal void AcceptTrade()
		{
			Room room = CyberEnvironment.GetGame().GetRoomManager().GetRoom(this.Session.GetHabbo().CurrentRoomId);
			if (room == null || !room.CanTradeInRoom)
			{
				return;
			}
			Trade userTrade = room.GetUserTrade(this.Session.GetHabbo().Id);
			if (userTrade == null)
			{
				return;
			}
			userTrade.Accept(this.Session.GetHabbo().Id);
		}
		internal void UnacceptTrade()
		{
			Room room = CyberEnvironment.GetGame().GetRoomManager().GetRoom(this.Session.GetHabbo().CurrentRoomId);
			if (room == null || !room.CanTradeInRoom)
			{
				return;
			}
			Trade userTrade = room.GetUserTrade(this.Session.GetHabbo().Id);
			if (userTrade == null)
			{
				return;
			}
			userTrade.Unaccept(this.Session.GetHabbo().Id);
		}
		internal void CompleteTrade()
		{
			Room room = CyberEnvironment.GetGame().GetRoomManager().GetRoom(this.Session.GetHabbo().CurrentRoomId);
			if (room == null || !room.CanTradeInRoom)
			{
				return;
			}
			Trade userTrade = room.GetUserTrade(this.Session.GetHabbo().Id);
			if (userTrade == null)
			{
				return;
			}
			userTrade.CompleteTrade(this.Session.GetHabbo().Id);
		}
		internal void GiveRespect()
		{
			Room room = CyberEnvironment.GetGame().GetRoomManager().GetRoom(this.Session.GetHabbo().CurrentRoomId);
			if (room == null || this.Session.GetHabbo().DailyRespectPoints <= 0)
			{
				return;
			}
			RoomUser roomUserByHabbo = room.GetRoomUserManager().GetRoomUserByHabbo(this.Request.PopWiredUInt());
			if (roomUserByHabbo == null || roomUserByHabbo.GetClient().GetHabbo().Id == this.Session.GetHabbo().Id || roomUserByHabbo.IsBot)
			{
				return;
			}
			CyberEnvironment.GetGame().GetQuestManager().ProgressUserQuest(this.Session, QuestType.SOCIAL_RESPECT, 0u);
			CyberEnvironment.GetGame().GetAchievementManager().ProgressUserAchievement(this.Session, "ACH_RespectGiven", 1, false);
			CyberEnvironment.GetGame().GetAchievementManager().ProgressUserAchievement(roomUserByHabbo.GetClient(), "ACH_RespectEarned", 1, false);
			checked
			{
				this.Session.GetHabbo().DailyRespectPoints--;
				roomUserByHabbo.GetClient().GetHabbo().Respect++;
				using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
				{
					queryreactor.runFastQuery(string.Concat(new object[]
					{
						"UPDATE user_stats SET respect = respect + 1 WHERE id = ",
						roomUserByHabbo.GetClient().GetHabbo().Id,
						" LIMIT 1;UPDATE user_stats SET daily_respect_points = daily_respect_points - 1 WHERE id= ",
						this.Session.GetHabbo().Id,
						" LIMIT 1"
					}));
				}
				ServerMessage serverMessage = new ServerMessage(Outgoing.GiveRespectsMessageComposer);
				serverMessage.AppendUInt(roomUserByHabbo.GetClient().GetHabbo().Id);
				serverMessage.AppendInt32(roomUserByHabbo.GetClient().GetHabbo().Respect);
				room.SendMessage(serverMessage);

                ServerMessage thumbsUp = new ServerMessage();
                thumbsUp.Init(Outgoing.RoomUserActionMessageComposer);
                thumbsUp.AppendInt32(room.GetRoomUserManager().GetRoomUserByHabbo(this.Session.GetHabbo().Username).VirtualId);
                thumbsUp.AppendInt32(7);
                room.SendMessage(thumbsUp);
            }
		}
		internal void ApplyEffect()
		{
			int effectId = this.Request.PopWiredInt32();
			RoomUser roomUserByHabbo = CyberEnvironment.GetGame().GetRoomManager().GetRoom(this.Session.GetHabbo().CurrentRoomId).GetRoomUserManager().GetRoomUserByHabbo(this.Session.GetHabbo().Username);
			if (!roomUserByHabbo.RidingHorse)
			{
				this.Session.GetHabbo().GetAvatarEffectsInventoryComponent().ActivateCustomEffect(effectId);
			}
		}
		internal void EnableEffect()
		{
			Room currentRoom = this.Session.GetHabbo().CurrentRoom;
			if (currentRoom == null)
			{
				return;
			}
			RoomUser roomUserByHabbo = currentRoom.GetRoomUserManager().GetRoomUserByHabbo(this.Session.GetHabbo().Id);
			if (roomUserByHabbo == null)
			{
				return;
			}
			int num = this.Request.PopWiredInt32();
			if (roomUserByHabbo == null)
			{
				return;
			}
			if (!roomUserByHabbo.RidingHorse)
			{
				if (num == 0)
				{
					this.Session.GetHabbo().GetAvatarEffectsInventoryComponent().StopEffect(this.Session.GetHabbo().GetAvatarEffectsInventoryComponent().CurrentEffect);
					return;
				}
				this.Session.GetHabbo().GetAvatarEffectsInventoryComponent().ActivateEffect(num);
			}
		}
		internal void RecycleItems()
		{
			if (!this.Session.GetHabbo().InRoom)
			{
				return;
			}
			int num = this.Request.PopWiredInt32();
			if (num != 3)
			{
				return;
			}
			int i = 0;
			checked
			{
				while (i < num)
				{
					UserItem item = this.Session.GetHabbo().GetInventoryComponent().GetItem(this.Request.PopWiredUInt());
					if (item != null && item.GetBaseItem().AllowRecycle)
					{
						this.Session.GetHabbo().GetInventoryComponent().RemoveItem(item.Id, false);
						using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
						{
							queryreactor.runFastQuery("DELETE FROM items WHERE id=" + item.Id + " LIMIT 1");
							goto IL_B0;
						}
						IL_B0:
						i++;
						continue;
					}
					return;
				}
				EcotronReward randomEcotronReward = CyberEnvironment.GetGame().GetCatalog().GetRandomEcotronReward();
				uint num2;
				using (IQueryAdapter queryreactor2 = CyberEnvironment.GetDatabaseManager().getQueryReactor())
				{
					queryreactor2.setQuery("INSERT INTO items (user_id,base_item,extra_data) VALUES ( @userid , 999888416, @timestamp)");
					queryreactor2.addParameter("userid", (int)this.Session.GetHabbo().Id);
					queryreactor2.addParameter("timestamp", DateTime.Now.ToLongDateString());
					num2 = (uint)queryreactor2.insertQuery();
					queryreactor2.runFastQuery(string.Concat(new object[]
					{
						"INSERT INTO user_gifts (gift_id,item_id,gift_sprite,extra_data) VALUES (",
						num2,
						",",
						randomEcotronReward.BaseId,
						", ",
						randomEcotronReward.DisplayId,
						",'')"
					}));
				}
				this.Session.GetHabbo().GetInventoryComponent().UpdateItems(true);
				this.Response.Init(Outgoing.RecyclingStateMessageComposer);
				this.Response.AppendInt32(1);
				this.Response.AppendUInt(num2);
				this.SendResponse();
			}
		}
		internal void RedeemExchangeFurni()
		{
			Room room = CyberEnvironment.GetGame().GetRoomManager().GetRoom(this.Session.GetHabbo().CurrentRoomId);
			if (room == null || !room.CheckRights(this.Session, true, false))
			{
				return;
			}
			if (CyberEnvironment.GetDBConfig().DBData["exchange_enabled"] != "1")
			{
				this.Session.SendNotif("Se ha desactivado temporalmente el intercambio de monedas!");
				return;
			}
			RoomItem item = room.GetRoomItemHandler().GetItem(this.Request.PopWiredUInt());
			if (item == null)
			{
				return;
			}
			if (!item.GetBaseItem().Name.StartsWith("CF_") && !item.GetBaseItem().Name.StartsWith("CFC_"))
			{
				return;
			}
			string[] array = item.GetBaseItem().Name.Split(new char[]
			{
				'_'
			});
			int num = int.Parse(array[1]);
			checked
			{
				if (num > 0)
				{
					this.Session.GetHabbo().Credits += num;
					this.Session.GetHabbo().UpdateCreditsBalance();
				}
				using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
				{
					queryreactor.runFastQuery("DELETE FROM items WHERE id=" + item.Id + " LIMIT 1;");
				}
				room.GetRoomItemHandler().RemoveFurniture(null, item.Id, false);
				this.Session.GetHabbo().GetInventoryComponent().RemoveItem(item.Id, false);
				this.Response.Init(Outgoing.UpdateInventoryMessageComposer);
				this.SendResponse();
			}
		}
		internal void KickBot()
		{
			Room room = CyberEnvironment.GetGame().GetRoomManager().GetRoom(this.Session.GetHabbo().CurrentRoomId);
			if (room == null || !room.CheckRights(this.Session, true, false))
			{
				return;
			}
			RoomUser roomUserByVirtualId = room.GetRoomUserManager().GetRoomUserByVirtualId(this.Request.PopWiredInt32());
			if (roomUserByVirtualId == null || !roomUserByVirtualId.IsBot)
			{
				return;
			}
			room.GetRoomUserManager().RemoveBot(roomUserByVirtualId.VirtualId, true);
		}
		internal void PlacePet()
		{
			Room room = CyberEnvironment.GetGame().GetRoomManager().GetRoom(this.Session.GetHabbo().CurrentRoomId);
			if (room == null || (room.AllowPets == 0 && !room.CheckRights(this.Session, true, false)) || !room.CheckRights(this.Session, true, false))
			{
				return;
			}
			uint num = this.Request.PopWiredUInt();
			Pet pet = this.Session.GetHabbo().GetInventoryComponent().GetPet(num);
			if (pet == null || pet.PlacedInRoom)
			{
				return;
			}
			int num2 = this.Request.PopWiredInt32();
			int num3 = this.Request.PopWiredInt32();
			if (!room.GetGameMap().CanWalk(num2, num3, false, 0u))
			{
				return;
			}
			using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
			{
				queryreactor.runFastQuery(string.Concat(new object[]
				{
					"UPDATE bots SET room_id = '",
					room.RoomId,
					"', x = '",
					num2,
					"', y = '",
					num3,
					"' WHERE id = '",
					num,
					"'"
				}));
			}
			pet.PlacedInRoom = true;
			pet.RoomId = room.RoomId;
			List<RandomSpeech> list = new List<RandomSpeech>();
			List<BotResponse> list2 = new List<BotResponse>();
			room.GetRoomUserManager().DeployBot(new RoomBot(pet.PetId, Convert.ToUInt32(pet.OwnerId), pet.RoomId, AIType.Pet, "freeroam", pet.Name, "", pet.Look, num2, num3, 0.0, 4, 0, 0, 0, 0, ref list, ref list2, "", 0, false), pet);
			this.Session.GetHabbo().GetInventoryComponent().MovePetToRoom(pet.PetId);
			if (pet.DBState != DatabaseUpdateState.NeedsInsert)
			{
				pet.DBState = DatabaseUpdateState.NeedsUpdate;
			}
			using (IQueryAdapter queryreactor2 = CyberEnvironment.GetDatabaseManager().getQueryReactor())
			{
				room.GetRoomUserManager().SavePets(queryreactor2);
			}
			this.Session.SendMessage(this.Session.GetHabbo().GetInventoryComponent().SerializePetInventory());
		}
		internal void GetPetInfo()
		{
			if (this.Session.GetHabbo() == null || this.Session.GetHabbo().CurrentRoom == null)
			{
				return;
			}
			uint petId = this.Request.PopWiredUInt();
			RoomUser pet = this.Session.GetHabbo().CurrentRoom.GetRoomUserManager().GetPet(petId);
			if (pet == null || pet.PetData == null)
			{
				return;
			}
			this.Session.SendMessage(pet.PetData.SerializeInfo());
		}
		internal void PickUpPet()
		{
			Room room = CyberEnvironment.GetGame().GetRoomManager().GetRoom(this.Session.GetHabbo().CurrentRoomId);
			if (this.Session == null || this.Session.GetHabbo() == null || this.Session.GetHabbo().GetInventoryComponent() == null)
			{
				return;
			}
			if (room == null || (room.AllowPets == 0 && !room.CheckRights(this.Session, true, false)))
			{
				return;
			}
			uint petId = this.Request.PopWiredUInt();
			RoomUser pet = room.GetRoomUserManager().GetPet(petId);
			if (pet == null)
			{
				return;
			}
			if (pet.RidingHorse)
			{
				RoomUser roomUserByVirtualId = room.GetRoomUserManager().GetRoomUserByVirtualId(Convert.ToInt32(pet.HorseID));
				if (roomUserByVirtualId != null)
				{
					roomUserByVirtualId.RidingHorse = false;
					roomUserByVirtualId.ApplyEffect(-1);
					roomUserByVirtualId.MoveTo(checked(new Point(roomUserByVirtualId.X + 1, roomUserByVirtualId.Y + 1)));
				}
			}
			if (pet.PetData.DBState != DatabaseUpdateState.NeedsInsert)
			{
				pet.PetData.DBState = DatabaseUpdateState.NeedsUpdate;
			}
			pet.PetData.RoomId = 0u;
			this.Session.GetHabbo().GetInventoryComponent().AddPet(pet.PetData);
			using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
			{
				room.GetRoomUserManager().SavePets(queryreactor);
			}
			room.GetRoomUserManager().RemoveBot(pet.VirtualId, false);
			this.Session.SendMessage(this.Session.GetHabbo().GetInventoryComponent().SerializePetInventory());
		}

		internal void RespectPet()
		{
			Room room = CyberEnvironment.GetGame().GetRoomManager().GetRoom(this.Session.GetHabbo().CurrentRoomId);
			if (room == null)
			{
				return;
			}
			uint petId = this.Request.PopWiredUInt();
			RoomUser pet = room.GetRoomUserManager().GetPet(petId);
			if (pet == null || pet.PetData == null)
			{
				return;
			}
			pet.PetData.OnRespect();
			checked
			{
                if (pet.PetData.Type == 16)
                {
                    CyberEnvironment.GetGame().GetAchievementManager().ProgressUserAchievement(this.Session, "ACH_MonsterPlantTreater", 1, false);
                }
                else
                {
                    this.Session.GetHabbo().DailyPetRespectPoints--;
                    CyberEnvironment.GetGame().GetAchievementManager().ProgressUserAchievement(this.Session, "ACH_PetRespectGiver", 1, false);
                    using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
                    {
                        queryreactor.runFastQuery("UPDATE user_stats SET daily_pet_respect_points = daily_pet_respect_points - 1 WHERE id = " + this.Session.GetHabbo().Id + " LIMIT 1");
                    }
                }
			}
		}
		internal void AllowAllRide()
		{
			Room room = CyberEnvironment.GetGame().GetRoomManager().GetRoom(this.Session.GetHabbo().CurrentRoomId);
			uint num = this.Request.PopWiredUInt();
			RoomUser pet = room.GetRoomUserManager().GetPet(num);
			if (pet.PetData.AnyoneCanRide == 1)
			{
				using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
				{
					queryreactor.runFastQuery("UPDATE bots_petdata SET anyone_ride=0 WHERE id=" + num + " LIMIT 1");
				}
				pet.PetData.AnyoneCanRide = 0;
			}
			else
			{
				using (IQueryAdapter queryreactor2 = CyberEnvironment.GetDatabaseManager().getQueryReactor())
				{
					queryreactor2.runFastQuery("UPDATE bots_petdata SET anyone_ride=1 WHERE id=" + num + " LIMIT 1");
				}
				pet.PetData.AnyoneCanRide = 1;
			}
			ServerMessage serverMessage = new ServerMessage(Outgoing.PetInfoMessageComposer);
			serverMessage.AppendUInt(pet.PetData.PetId);
			serverMessage.AppendString(pet.PetData.Name);
			serverMessage.AppendInt32(pet.PetData.Level);
			serverMessage.AppendInt32(20);
			serverMessage.AppendInt32(pet.PetData.Experience);
			serverMessage.AppendInt32(pet.PetData.experienceGoal);
			serverMessage.AppendInt32(pet.PetData.Energy);
			serverMessage.AppendInt32(100);
			serverMessage.AppendInt32(pet.PetData.Nutrition);
			serverMessage.AppendInt32(150);
			serverMessage.AppendInt32(pet.PetData.Respect);
			serverMessage.AppendUInt(pet.PetData.OwnerId);
			serverMessage.AppendInt32(pet.PetData.Age);
			serverMessage.AppendString(pet.PetData.OwnerName);
			serverMessage.AppendInt32(1);
			serverMessage.AppendBoolean(pet.PetData.HaveSaddle);
			serverMessage.AppendBoolean(CyberEnvironment.GetGame().GetRoomManager().GetRoom(pet.PetData.RoomId).GetRoomUserManager().GetRoomUserByVirtualId(pet.PetData.VirtualId).RidingHorse);
			serverMessage.AppendInt32(0);
			serverMessage.AppendInt32(pet.PetData.AnyoneCanRide);
			serverMessage.AppendInt32(0);
			serverMessage.AppendInt32(0);
			serverMessage.AppendInt32(0);
			serverMessage.AppendInt32(0);
			serverMessage.AppendInt32(0);
			serverMessage.AppendInt32(0);
			serverMessage.AppendString("");
			serverMessage.AppendBoolean(false);
			serverMessage.AppendInt32(-1);
			serverMessage.AppendInt32(-1);
			serverMessage.AppendInt32(-1);
			serverMessage.AppendBoolean(false);
			room.SendMessage(serverMessage);
		}
		internal void AddSaddle()
		{
			Room room = CyberEnvironment.GetGame().GetRoomManager().GetRoom(this.Session.GetHabbo().CurrentRoomId);
			if (room == null || (room.AllowPets == 0 && !room.CheckRights(this.Session, true, false)))
			{
				return;
			}
			uint pId = this.Request.PopWiredUInt();
			RoomItem item = room.GetRoomItemHandler().GetItem(pId);
			if (item == null)
			{
				return;
			}
			uint petId = this.Request.PopWiredUInt();
			RoomUser pet = room.GetRoomUserManager().GetPet(petId);
			if (pet == null || pet.PetData == null || pet.PetData.OwnerId != this.Session.GetHabbo().Id)
			{
				return;
			}
			room.GetRoomItemHandler().RemoveFurniture(this.Session, item.Id, false);
			checked
			{
				if (item.GetBaseItem().Name.Contains("horse_hairdye"))
				{
					string s = item.GetBaseItem().Name.Split(new char[]
					{
						'_'
					})[2];
					int num = 48;
					num += int.Parse(s);
					pet.PetData.HairDye = num;
					using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
					{
						queryreactor.runFastQuery(string.Concat(new object[]
						{
							"UPDATE bots_petdata SET hairdye = '",
							pet.PetData.HairDye,
							"' WHERE id = ",
							pet.PetData.PetId
						}));
						goto IL_40C;
					}
				}
				if (item.GetBaseItem().Name.Contains("horse_dye"))
				{
					string s2 = item.GetBaseItem().Name.Split(new char[]
					{
						'_'
					})[2];
					int num2 = int.Parse(s2);
					int num3 = 2 + num2 * 4 - 4;
					if (num2 == 13)
					{
						num3 = 61;
					}
					else
					{
						if (num2 == 14)
						{
							num3 = 65;
						}
						else
						{
							if (num2 == 15)
							{
								num3 = 69;
							}
							else
							{
								if (num2 == 16)
								{
									num3 = 73;
								}
							}
						}
					}
					pet.PetData.Race = num3.ToString();
					using (IQueryAdapter queryreactor2 = CyberEnvironment.GetDatabaseManager().getQueryReactor())
					{
						queryreactor2.runFastQuery(string.Concat(new object[]
						{
							"UPDATE bots_petdata SET race = '",
							pet.PetData.Race,
							"' WHERE id = ",
							pet.PetData.PetId
						}));
						queryreactor2.runFastQuery("DELETE FROM items WHERE id=" + item.Id + " LIMIT 1");
						goto IL_40C;
					}
				}
				if (item.GetBaseItem().Name.Contains("horse_hairstyle"))
				{
					string s3 = item.GetBaseItem().Name.Split(new char[]
					{
						'_'
					})[2];
					int num4 = 100;
					num4 += int.Parse(s3);
					pet.PetData.PetHair = num4;
					using (IQueryAdapter queryreactor3 = CyberEnvironment.GetDatabaseManager().getQueryReactor())
					{
						queryreactor3.runFastQuery(string.Concat(new object[]
						{
							"UPDATE bots_petdata SET pethair = '",
							pet.PetData.PetHair,
							"' WHERE id = ",
							pet.PetData.PetId
						}));
						queryreactor3.runFastQuery("DELETE FROM items WHERE id=" + item.Id + " LIMIT 1");
						goto IL_40C;
					}
				}
				if (item.GetBaseItem().Name.Contains("saddle"))
				{
					pet.PetData.HaveSaddle = true;
					using (IQueryAdapter queryreactor4 = CyberEnvironment.GetDatabaseManager().getQueryReactor())
					{
						queryreactor4.runFastQuery("UPDATE bots_petdata SET have_saddle = 1 WHERE id = " + pet.PetData.PetId);
						queryreactor4.runFastQuery("DELETE FROM items WHERE id=" + item.Id + " LIMIT 1");
					}
				}
				IL_40C:
				ServerMessage serverMessage = new ServerMessage(Outgoing.SetRoomUserMessageComposer);
				serverMessage.AppendInt32(1);
				pet.Serialize(serverMessage, false);
				room.SendMessage(serverMessage);
				ServerMessage serverMessage2 = new ServerMessage(Outgoing.SerializePetMessageComposer);
				serverMessage2.AppendInt32(pet.PetData.VirtualId);
				serverMessage2.AppendUInt(pet.PetData.PetId);
				serverMessage2.AppendUInt(pet.PetData.Type);
				serverMessage2.AppendInt32(int.Parse(pet.PetData.Race));
				serverMessage2.AppendString(pet.PetData.Color.ToLower());
				if (pet.PetData.HaveSaddle)
				{
					serverMessage2.AppendInt32(2);
					serverMessage2.AppendInt32(3);
					serverMessage2.AppendInt32(4);
					serverMessage2.AppendInt32(9);
					serverMessage2.AppendInt32(0);
					serverMessage2.AppendInt32(3);
					serverMessage2.AppendInt32(pet.PetData.PetHair);
					serverMessage2.AppendInt32(pet.PetData.HairDye);
					serverMessage2.AppendInt32(3);
					serverMessage2.AppendInt32(pet.PetData.PetHair);
					serverMessage2.AppendInt32(pet.PetData.HairDye);
				}
				else
				{
					serverMessage2.AppendInt32(1);
					serverMessage2.AppendInt32(2);
					serverMessage2.AppendInt32(2);
					serverMessage2.AppendInt32(pet.PetData.PetHair);
					serverMessage2.AppendInt32(pet.PetData.HairDye);
					serverMessage2.AppendInt32(3);
					serverMessage2.AppendInt32(pet.PetData.PetHair);
					serverMessage2.AppendInt32(pet.PetData.HairDye);
				}
				serverMessage2.AppendBoolean(pet.PetData.HaveSaddle);
				serverMessage2.AppendBoolean(pet.RidingHorse);
				room.SendMessage(serverMessage2);
			}
		}
		internal void RemoveSaddle()
		{
			Room room = CyberEnvironment.GetGame().GetRoomManager().GetRoom(this.Session.GetHabbo().CurrentRoomId);
			if (room == null || (room.AllowPets == 0 && !room.CheckRights(this.Session, true, false)))
			{
				return;
			}
			uint petId = this.Request.PopWiredUInt();
			RoomUser pet = room.GetRoomUserManager().GetPet(petId);
			if (pet == null || pet.PetData == null || pet.PetData.OwnerId != this.Session.GetHabbo().Id)
			{
				return;
			}
			pet.PetData.HaveSaddle = false;
			using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
			{
				queryreactor.runFastQuery("UPDATE bots_petdata SET have_saddle = 0 WHERE id = " + pet.PetData.PetId);
				queryreactor.runFastQuery("INSERT INTO items (user_id, base_item) VALUES (" + this.Session.GetHabbo().Id + ", 4221);");
			}
			this.Session.GetHabbo().GetInventoryComponent().UpdateItems(true);
			ServerMessage serverMessage = new ServerMessage(Outgoing.SetRoomUserMessageComposer);
			serverMessage.AppendInt32(1);
			pet.Serialize(serverMessage, false);
			room.SendMessage(serverMessage);
			ServerMessage serverMessage2 = new ServerMessage(Outgoing.SerializePetMessageComposer);
			serverMessage2.AppendInt32(pet.PetData.VirtualId);
			serverMessage2.AppendUInt(pet.PetData.PetId);
			serverMessage2.AppendUInt(pet.PetData.Type);
			serverMessage2.AppendInt32(int.Parse(pet.PetData.Race));
			serverMessage2.AppendString(pet.PetData.Color.ToLower());
			serverMessage2.AppendInt32(1);
			serverMessage2.AppendInt32(2);
			serverMessage2.AppendInt32(2);
			serverMessage2.AppendInt32(pet.PetData.PetHair);
			serverMessage2.AppendInt32(pet.PetData.HairDye);
			serverMessage2.AppendInt32(3);
			serverMessage2.AppendInt32(pet.PetData.PetHair);
			serverMessage2.AppendInt32(pet.PetData.HairDye);
			serverMessage2.AppendBoolean(pet.PetData.HaveSaddle);
			serverMessage2.AppendBoolean(pet.RidingHorse);
			room.SendMessage(serverMessage2);
		}
		internal void CancelMountOnPet()
		{
			Room room = CyberEnvironment.GetGame().GetRoomManager().GetRoom(this.Session.GetHabbo().CurrentRoomId);
			if (room == null)
			{
				return;
			}
			RoomUser roomUserByHabbo = room.GetRoomUserManager().GetRoomUserByHabbo(this.Session.GetHabbo().Id);
			if (roomUserByHabbo == null)
			{
				return;
			}
			uint petId = this.Request.PopWiredUInt();
			RoomUser pet = room.GetRoomUserManager().GetPet(petId);
			if (pet == null || pet.PetData == null)
			{
				return;
			}
			roomUserByHabbo.RidingHorse = false;
			roomUserByHabbo.HorseID = 0u;
			pet.RidingHorse = false;
			pet.HorseID = 0u;
			checked
			{
				roomUserByHabbo.MoveTo(roomUserByHabbo.X + 1, roomUserByHabbo.Y + 1);
				roomUserByHabbo.ApplyEffect(-1);
			}
		}
		internal void GiveHanditem()
		{
			Room room = CyberEnvironment.GetGame().GetRoomManager().GetRoom(this.Session.GetHabbo().CurrentRoomId);
			if (room == null)
			{
				return;
			}
			RoomUser roomUserByHabbo = room.GetRoomUserManager().GetRoomUserByHabbo(this.Session.GetHabbo().Id);
			if (roomUserByHabbo == null)
			{
				return;
			}
			RoomUser roomUserByHabbo2 = room.GetRoomUserManager().GetRoomUserByHabbo(this.Request.PopWiredUInt());
			if (roomUserByHabbo2 == null)
			{
				return;
			}
			if ((checked(Math.Abs(roomUserByHabbo.X - roomUserByHabbo2.X) < 3 && Math.Abs(roomUserByHabbo.Y - roomUserByHabbo2.Y) < 3) || roomUserByHabbo.GetClient().GetHabbo().Rank > 4u) && roomUserByHabbo.CarryItemID > 0 && roomUserByHabbo.CarryTimer > 0)
			{
				if (roomUserByHabbo.CarryItemID == 8)
				{
					CyberEnvironment.GetGame().GetQuestManager().ProgressUserQuest(this.Session, QuestType.GIVE_COFFEE, 0u);
				}
				roomUserByHabbo2.CarryItem(roomUserByHabbo.CarryItemID);
				roomUserByHabbo.CarryItem(0);
				roomUserByHabbo2.DanceId = 0;
			}
		}
		internal void RedeemVoucher()
		{
			string query = this.Request.PopFixedString();
			string productName = "";
			string productDescription = "";
			bool isValid = false;
			DataRow row;
			using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
			{
				queryreactor.setQuery("SELECT * FROM vouchers WHERE voucher = @vo LIMIT 1");
				queryreactor.addParameter("vo", query);
				row = queryreactor.getRow();
			}
			checked
			{
				if (row != null)
				{
					isValid = true;
					using (IQueryAdapter queryreactor2 = CyberEnvironment.GetDatabaseManager().getQueryReactor())
					{
						queryreactor2.setQuery("DELETE * FROM vouchers WHERE voucher = @vou LIMIT 1");
						queryreactor2.addParameter("vou", query);
						queryreactor2.runQuery();
					}
					this.Session.GetHabbo().Credits += (int)row["value"];
					this.Session.GetHabbo().UpdateCreditsBalance();
					this.Session.GetHabbo().NotifyNewPixels((int)row["extra_duckets"]);
				}
				this.Session.GetHabbo().NotifyVoucher(isValid, productName, productDescription);
			}
		}
		internal void RemoveHanditem()
		{
			Room room = CyberEnvironment.GetGame().GetRoomManager().GetRoom(this.Session.GetHabbo().CurrentRoomId);
			if (room == null)
			{
				return;
			}
			RoomUser roomUserByHabbo = room.GetRoomUserManager().GetRoomUserByHabbo(this.Session.GetHabbo().Id);
			if (roomUserByHabbo == null)
			{
				return;
			}
			if (roomUserByHabbo.CarryItemID > 0 && roomUserByHabbo.CarryTimer > 0)
			{
				roomUserByHabbo.CarryItem(0);
			}
		}
		internal void MountOnPet()
		{
			Room room = CyberEnvironment.GetGame().GetRoomManager().GetRoom(this.Session.GetHabbo().CurrentRoomId);
			if (room == null)
			{
				return;
			}
			RoomUser roomUserByHabbo = room.GetRoomUserManager().GetRoomUserByHabbo(this.Session.GetHabbo().Id);
			if (roomUserByHabbo == null)
			{
				return;
			}
			uint petId = this.Request.PopWiredUInt();
			bool flag = this.Request.PopWiredBoolean();
			RoomUser pet = room.GetRoomUserManager().GetPet(petId);
			if (pet == null || pet.PetData == null)
			{
				return;
			}
			if (pet.PetData.AnyoneCanRide == 0 && pet.PetData.OwnerId != roomUserByHabbo.UserID)
			{
				this.Session.SendNotif("You are unable to ride this horse.\nThe owner of the pet has not selected for anyone to ride it.");
				return;
			}
			if (flag)
			{
				if (pet.RidingHorse)
				{
					string[] value = PetLocale.GetValue("pet.alreadymounted");
					Random random = new Random();
					pet.Chat(null, value[random.Next(0, checked(value.Length - 1))], false, 0, 0);
				}
				else
				{
					if (!roomUserByHabbo.RidingHorse)
					{
						pet.Statusses.Remove("sit");
						pet.Statusses.Remove("lay");
						pet.Statusses.Remove("snf");
						pet.Statusses.Remove("eat");
						pet.Statusses.Remove("ded");
						pet.Statusses.Remove("jmp");
						int x = roomUserByHabbo.X;
						int y = roomUserByHabbo.Y;
						room.SendMessage(room.GetRoomItemHandler().UpdateUserOnRoller(pet, new Point(x, y), 0u, room.GetGameMap().SqAbsoluteHeight(x, y)));
						room.GetRoomUserManager().UpdateUserStatus(pet, false);
						room.SendMessage(room.GetRoomItemHandler().UpdateUserOnRoller(roomUserByHabbo, new Point(x, y), 0u, room.GetGameMap().SqAbsoluteHeight(x, y) + 1.0));
						room.GetRoomUserManager().UpdateUserStatus(roomUserByHabbo, false);
						pet.ClearMovement(true);
						roomUserByHabbo.RidingHorse = true;
						pet.RidingHorse = true;
						pet.HorseID = checked((uint)roomUserByHabbo.VirtualId);
						roomUserByHabbo.HorseID = Convert.ToUInt32(pet.VirtualId);
						roomUserByHabbo.ApplyEffect(77);
						roomUserByHabbo.Z += 1.0;
						roomUserByHabbo.UpdateNeeded = true;
						pet.UpdateNeeded = true;
					}
				}
			}
			else
			{
				if ((long)roomUserByHabbo.VirtualId == (long)((ulong)pet.HorseID))
				{
					pet.Statusses.Remove("sit");
					pet.Statusses.Remove("lay");
					pet.Statusses.Remove("snf");
					pet.Statusses.Remove("eat");
					pet.Statusses.Remove("ded");
					pet.Statusses.Remove("jmp");
					roomUserByHabbo.RidingHorse = false;
					roomUserByHabbo.HorseID = 0u;
					pet.RidingHorse = false;
					pet.HorseID = 0u;
					roomUserByHabbo.MoveTo(checked(new Point(roomUserByHabbo.X + 2, roomUserByHabbo.Y + 2)));
					roomUserByHabbo.ApplyEffect(-1);
					roomUserByHabbo.UpdateNeeded = true;
					pet.UpdateNeeded = true;
				}
				else
				{
					this.Session.SendNotif("Could not dismount this horse - You are not riding it!");
				}
			}
			ServerMessage serverMessage = new ServerMessage(Outgoing.SerializePetMessageComposer);
			serverMessage.AppendInt32(pet.PetData.VirtualId);
			serverMessage.AppendUInt(pet.PetData.PetId);
			serverMessage.AppendUInt(pet.PetData.Type);
			serverMessage.AppendInt32(int.Parse(pet.PetData.Race));
			serverMessage.AppendString(pet.PetData.Color.ToLower());
			serverMessage.AppendInt32(2);
			serverMessage.AppendInt32(3);
			serverMessage.AppendInt32(4);
			serverMessage.AppendInt32(9);
			serverMessage.AppendInt32(0);
			serverMessage.AppendInt32(3);
			serverMessage.AppendInt32(pet.PetData.PetHair);
			serverMessage.AppendInt32(pet.PetData.HairDye);
			serverMessage.AppendInt32(3);
			serverMessage.AppendInt32(pet.PetData.PetHair);
			serverMessage.AppendInt32(pet.PetData.HairDye);
			serverMessage.AppendBoolean(pet.PetData.HaveSaddle);
			serverMessage.AppendBoolean(pet.RidingHorse);
			room.SendMessage(serverMessage);
		}

		
		internal void SaveWired()
		{
			uint pId = this.Request.PopWiredUInt();
			RoomItem item = CyberEnvironment.GetGame().GetRoomManager().GetRoom(this.Session.GetHabbo().CurrentRoomId).GetRoomItemHandler().GetItem(pId);
			WiredSaver.SaveWired(this.Session, item, this.Request);
		}
        
		internal void SaveWiredConditions()
		{
			uint pId = this.Request.PopWiredUInt();
			RoomItem item = CyberEnvironment.GetGame().GetRoomManager().GetRoom(this.Session.GetHabbo().CurrentRoomId).GetRoomItemHandler().GetItem(pId);
			WiredSaver.SaveWired(this.Session, item, this.Request);
		}
		internal void ChooseTVPlaylist()
		{
			uint num = this.Request.PopWiredUInt();
			string playlist = this.Request.PopFixedString();
			
			RoomItem item = this.Session.GetHabbo().CurrentRoom.GetRoomItemHandler().GetItem(num);

			if (item.GetBaseItem().InteractionType == InteractionType.youtubetv)
			{
				if (!CyberEnvironment.GetGame().GetVideoManager().PlaylistExists(playlist))
				{
					return;
				}
                PlayerTV playerTV = CyberEnvironment.GetGame().GetVideoManager().AddOrGetTV(num, this.Session.GetHabbo().CurrentRoomId);

				playerTV.SetPlaylist(CyberEnvironment.GetGame().GetVideoManager().GetPlaylist(playlist));
				ServerMessage serverMessage = new ServerMessage();
				serverMessage.Init(Outgoing.YouTubeLoadVideoMessageComposer);
				serverMessage.AppendUInt(num);
				serverMessage.AppendString(playerTV.CurrentVideo);
				serverMessage.AppendInt32(0);
				serverMessage.AppendInt32(playerTV.Playlist.GetDuration(playerTV.CurrentVideo));
				this.Response = serverMessage;
				this.SendResponse();
				item.ExtraData = playerTV.CurrentVideo;
				ServerMessage serverMessage2 = new ServerMessage(Outgoing.UpdateRoomItemMessageComposer);
				item.Serialize(serverMessage2);
				this.Response = serverMessage2;
				this.SendResponse();
				this.Session.GetHabbo().CurrentRoom.SendMessage(serverMessage2);
			}
		}
		internal void ChooseTVPlayerVideo()
		{
			uint num = this.Request.PopWiredUInt();
			int num2 = this.Request.PopWiredInt32();
			PlayerTV playerTV = CyberEnvironment.GetGame().GetVideoManager().AddOrGetTV(num, this.Session.GetHabbo().CurrentRoomId);
			RoomItem item = this.Session.GetHabbo().CurrentRoom.GetRoomItemHandler().GetItem(num);
			if (item.GetBaseItem().InteractionType == InteractionType.youtubetv)
			{
				if (num2 >= 1)
				{
					playerTV.SetNextVideo();
				}
				else
				{
					playerTV.SetPreviousVideo();
				}
				ServerMessage serverMessage = new ServerMessage();
				serverMessage.Init(Outgoing.YouTubeLoadVideoMessageComposer);
				serverMessage.AppendUInt(num);
				serverMessage.AppendString(playerTV.CurrentVideo);
				serverMessage.AppendInt32(0);
				serverMessage.AppendInt32(playerTV.Playlist.GetDuration(playerTV.CurrentVideo));
				this.Response = serverMessage;
				this.SendResponse();
				item.ExtraData = playerTV.CurrentVideo;
				ServerMessage message = new ServerMessage(Outgoing.UpdateRoomItemMessageComposer);
				item.Serialize(message);
				this.Session.GetHabbo().CurrentRoom.SendMessage(message);
			}
		}
		internal void GetTVPlayer()
		{
			uint num = this.Request.PopWiredUInt();
            PlayerTV playerTV = CyberEnvironment.GetGame().GetVideoManager().AddOrGetTV(num, this.Session.GetHabbo().CurrentRoomId);
			ServerMessage serverMessage = new ServerMessage();
			serverMessage.Init(Outgoing.YouTubeLoadVideoMessageComposer);
			serverMessage.AppendUInt(num);
			serverMessage.AppendString(playerTV.CurrentVideo);
			serverMessage.AppendInt32(0);
			serverMessage.AppendInt32((playerTV.Playlist == null) ? 0 : playerTV.Playlist.GetDuration(playerTV.CurrentVideo));
			this.Response = serverMessage;
			this.SendResponse();
			ServerMessage serverMessage2 = new ServerMessage();
			serverMessage2.Init(Outgoing.YouTubeLoadPlaylistsMessageComposer);
			serverMessage2.AppendUInt(num);
			serverMessage2.AppendInt32(CyberEnvironment.GetGame().GetVideoManager().GetPlaylists().Count);
			foreach (Playlist current in CyberEnvironment.GetGame().GetVideoManager().GetPlaylists().Values)
			{
				current.Serialize(serverMessage2);
			}
			serverMessage2.AppendString("");
			this.Response = serverMessage2;
			this.SendResponse();
		}
		internal void PlaceBot()
		{
			Room room = CyberEnvironment.GetGame().GetRoomManager().GetRoom(this.Session.GetHabbo().CurrentRoomId);
			if (room == null || !room.CheckRights(this.Session, true, false))
			{
				return;
			}
			uint num = this.Request.PopWiredUInt();
			RoomBot bot = this.Session.GetHabbo().GetInventoryComponent().GetBot(num);
			if (bot == null)
			{
				Logging.WriteLine("Null bot found" + num, ConsoleColor.Gray);
				return;
			}
			int num2 = this.Request.PopWiredInt32();
			int num3 = this.Request.PopWiredInt32();
			if (!room.GetGameMap().CanWalk(num2, num3, false, 0u) || !room.GetGameMap().validTile(num2, num3))
			{
				this.Session.SendNotif("You can't place your bot here.");
				return;
			}
			using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
			{
				queryreactor.runFastQuery(string.Concat(new object[]
				{
					"UPDATE bots SET room_id = '",
					room.RoomId,
					"', x = '",
					num2,
					"', y = '",
					num3,
					"' WHERE id = '",
					num,
					"'"
				}));
			}
			bot.RoomId = room.RoomId;
			List<RandomSpeech> list = new List<RandomSpeech>();
			List<BotResponse> list2 = new List<BotResponse>();
            RoomBot botData = new RoomBot(num, bot.OwnerId, bot.RoomId, AIType.Generic, "freeroam", bot.Name, bot.Motto, bot.Look, num2, num3, 0.0, 4, 0, 0, 0, 0, ref list, ref list2, bot.Gender, bot.DanceId, bot.IsBartender);
			room.GetRoomUserManager().DeployBot(botData, null);
            botData.WasPicked = false;
			this.Session.GetHabbo().GetInventoryComponent().MoveBotToRoom(num);
			this.Session.SendMessage(this.Session.GetHabbo().GetInventoryComponent().SerializeBotInventory());
		}
		internal void Sit()
		{
			RoomUser roomUserByHabbo = this.Session.GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(this.Session.GetHabbo().Id);
			if (roomUserByHabbo.Statusses.ContainsKey("lay") || roomUserByHabbo.IsLyingDown || roomUserByHabbo.RidingHorse || roomUserByHabbo.IsWalking)
			{
				return;
			}
			checked
			{
				if (!roomUserByHabbo.Statusses.ContainsKey("sit"))
				{
					if (roomUserByHabbo.RotBody % 2 != 0)
					{
						roomUserByHabbo.RotBody--;
					}
					roomUserByHabbo.Statusses.Add("sit", "0.55");
					roomUserByHabbo.IsSitting = true;
					roomUserByHabbo.UpdateNeeded = true;
					return;
				}
				if (roomUserByHabbo.IsSitting)
				{
					roomUserByHabbo.Statusses.Remove("sit");
					roomUserByHabbo.IsSitting = false;
					roomUserByHabbo.UpdateNeeded = true;
				}
			}
		}
		internal void PickUpBot()
		{
			uint num = this.Request.PopWiredUInt();
			Room room = CyberEnvironment.GetGame().GetRoomManager().GetRoom(this.Session.GetHabbo().CurrentRoomId);
			RoomUser bot = room.GetRoomUserManager().GetBot(num);

			if (this.Session == null || this.Session.GetHabbo() == null || this.Session.GetHabbo().GetInventoryComponent() == null || bot == null || !room.CheckRights(Session, true))
			{
				return;
			}
			this.Session.GetHabbo().GetInventoryComponent().AddBot(bot.BotData);
			using (IQueryAdapter queryreactor2 = CyberEnvironment.GetDatabaseManager().getQueryReactor())
			{
				queryreactor2.runFastQuery("UPDATE bots SET room_id = '0' WHERE id = '" + num + "' LIMIT 1");
			}
			room.GetRoomUserManager().RemoveBot(bot.VirtualId, false);
			this.Session.SendMessage(this.Session.GetHabbo().GetInventoryComponent().SerializeBotInventory());
		}
		internal void CancelMysteryBox()
		{
			this.Request.PopWiredUInt();
			RoomUser roomUserByHabbo = this.Session.GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(this.Session.GetHabbo().Id);
			RoomItem item = this.Session.GetHabbo().CurrentRoom.GetRoomItemHandler().GetItem(roomUserByHabbo.GateId);
			if (item == null)
			{
				return;
			}
			if (item.InteractingUser == this.Session.GetHabbo().Id)
			{
				item.InteractingUser = 0u;
			}
			else
			{
				if (item.InteractingUser2 == this.Session.GetHabbo().Id)
				{
					item.InteractingUser2 = 0u;
				}
			}
			roomUserByHabbo.GateId = 0u;
			string text = item.ExtraData.Split(new char[]
			{
				Convert.ToChar(5)
			})[0];
			using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
			{
				queryreactor.setQuery(string.Concat(new object[]
				{
					"UPDATE items SET extra_data = '",
					text,
					Convert.ToChar(5),
					"2' WHERE id=",
					item.Id,
					" LIMIT 1;"
				}));
			}
			item.ExtraData = text + Convert.ToChar(5) + "2";
			item.UpdateNeeded = true;
			item.UpdateState(true, true);
		}

        internal void SaveFootballOutfit()
        {
            uint pId = this.Request.PopWiredUInt();
            string gender = this.Request.PopFixedString();
            string look = this.Request.PopFixedString();

            Room room = Session.GetHabbo().CurrentRoom;
            if (room == null)
            { return; }
            RoomItem item = room.GetRoomItemHandler().GetItem(pId);
            if (item == null || item.GetBaseItem().InteractionType != InteractionType.fbgate)
            {
                return;
            }

            if (gender.ToUpper() == "M")
            {
                string[] Figures = item.ExtraData.Split(';');
                string[] newFigures = new string[2];

                newFigures[0] = look;
                if (Figures.Length > 1)
                {
                    newFigures[1] = Figures[1];
                }
                else
                {
                    newFigures[1] = "hd-99999-99999.ch-630-62.lg-695-62";
                }

                item.ExtraData = String.Join(";", newFigures);
                item.UpdateState();
                return;
            }
            else if (gender.ToUpper() == "F")
            {
                string[] Figures = item.ExtraData.Split(';');
                string[] newFigures = new string[2];

                if (!string.IsNullOrWhiteSpace(Figures[0]))
                {
                    newFigures[0] = Figures[0];
                }
                else
                {
                    newFigures[0] = "hd-99999-99999.lg-270-62";
                }
                newFigures[1] = look;
                
                item.ExtraData = String.Join(";", newFigures);
                item.UpdateState();
            }
            return;
        }

		internal void SaveMannequin()
		{
			uint pId = this.Request.PopWiredUInt();
			string text = this.Request.PopFixedString();
			RoomItem item = this.Session.GetHabbo().CurrentRoom.GetRoomItemHandler().GetItem(pId);
			if (item == null)
			{
				return;
			}
			if (!item.ExtraData.Contains(Convert.ToChar(5)))
			{
				return;
			}
			if (!this.Session.GetHabbo().CurrentRoom.CheckRights(this.Session, true, false))
			{
				return;
			}
			string[] array = item.ExtraData.Split(new char[]
			{
				Convert.ToChar(5)
			});
			array[2] = text;
			item.ExtraData = string.Concat(new object[]
			{
				array[0],
				Convert.ToChar(5),
				array[1],
				Convert.ToChar(5),
				array[2]
			});
			item.Serialize(this.Response);
			item.UpdateState(true, true);
			using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
			{
				queryreactor.setQuery("UPDATE items SET extra_data=@Ed WHERE id=" + item.Id + " LIMIT 1");
				queryreactor.addParameter("Ed", item.ExtraData);
				queryreactor.runQuery();
			}
		}
		internal void SaveMannequin2()
		{
			uint pId = this.Request.PopWiredUInt();
			RoomItem item = this.Session.GetHabbo().CurrentRoom.GetRoomItemHandler().GetItem(pId);
			if (item == null)
			{
				return;
			}
			if (!item.ExtraData.Contains(Convert.ToChar(5)))
			{
				return;
			}
			if (!this.Session.GetHabbo().CurrentRoom.CheckRights(this.Session, true, false))
			{
				return;
			}
			string[] array = item.ExtraData.Split(new char[]
			{
				Convert.ToChar(5)
			});
			array[0] = this.Session.GetHabbo().Gender.ToLower();
			array[1] = "";
			string[] array2 = this.Session.GetHabbo().Look.Split(new char[]
			{
				'.'
			});
			for (int i = 0; i < array2.Length; i++)
			{
				string text = array2[i];
				if (!text.Contains("hr") && !text.Contains("hd") && !text.Contains("he") && !text.Contains("ea") && !text.Contains("ha"))
				{
					string[] array3;
					(array3 = array)[1] = array3[1] + text + ".";
				}
			}
			array[1] = array[1].TrimEnd(new char[]
			{
				'.'
			});
			item.ExtraData = string.Concat(new object[]
			{
				array[0],
				Convert.ToChar(5),
				array[1],
				Convert.ToChar(5),
				array[2]
			});
			item.UpdateNeeded = true;
			item.UpdateState(true, true);
			using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
			{
				queryreactor.setQuery("UPDATE items SET extra_data=@Ed WHERE id=" + item.Id + " LIMIT 1");
				queryreactor.addParameter("Ed", item.ExtraData);
				queryreactor.runQuery();
			}
		}
		internal void EjectFurni()
		{
			this.Request.PopWiredInt32();
			Room room = CyberEnvironment.GetGame().GetRoomManager().GetRoom(this.Session.GetHabbo().CurrentRoomId);
			if (room == null || !room.CheckRights(this.Session))
			{
				return;
			}
			uint pId = this.Request.PopWiredUInt();
			RoomItem item = room.GetRoomItemHandler().GetItem(pId);
			if (item == null)
			{
				return;
			}
			GameClient clientByUserID = CyberEnvironment.GetGame().GetClientManager().GetClientByUserID(item.UserID);
			if (item.GetBaseItem().InteractionType == InteractionType.postit)
			{
				return;
			}
			if (clientByUserID != null)
			{
				room.GetRoomItemHandler().RemoveFurniture(this.Session, item.Id, true);
				clientByUserID.GetHabbo().GetInventoryComponent().AddNewItem(item.Id, item.BaseItem, item.ExtraData, item.GroupId, true, true, 0, 0, "");
				clientByUserID.GetHabbo().GetInventoryComponent().UpdateItems(true);
				return;
			}
			room.GetRoomItemHandler().RemoveFurniture(this.Session, item.Id, true);
			using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
			{
				queryreactor.runFastQuery("UPDATE items SET room_id='0' WHERE id=" + item.Id + " LIMIT 1");
			}
		}
		internal void MuteUser()
		{
			uint num = this.Request.PopWiredUInt();
			this.Request.PopWiredUInt();
			uint num2 = this.Request.PopWiredUInt();
			Room currentRoom = this.Session.GetHabbo().CurrentRoom;
			if (currentRoom == null || (currentRoom.WhoCanBan == 0 && !currentRoom.CheckRights(this.Session, true, false)) || (currentRoom.WhoCanBan == 1 && !currentRoom.CheckRights(this.Session)))
			{
				return;
			}
			RoomUser roomUserByHabbo = currentRoom.GetRoomUserManager().GetRoomUserByHabbo(CyberEnvironment.getHabboForId(num).Username);
			if (roomUserByHabbo == null)
			{
				return;
			}
			if (roomUserByHabbo.GetClient().GetHabbo().Rank > 3u)
			{
				return;
			}
			if (currentRoom.MutedUsers.ContainsKey(num))
			{
				if ((ulong)currentRoom.MutedUsers[num] >= (ulong)((long)CyberEnvironment.GetUnixTimestamp()))
				{
					return;
				}
				currentRoom.MutedUsers.Remove(num);
			}
			currentRoom.MutedUsers.Add(num, uint.Parse(checked(unchecked((long)CyberEnvironment.GetUnixTimestamp()) + (long)unchecked((ulong)checked(num2 * 60u))).ToString()));
			roomUserByHabbo.GetClient().SendNotif("¡El dueño de la Sala te ha muteado por " + num2 + " minutos!");
		}
		internal void UpdateEventInfo()
		{
			this.Request.PopWiredInt32();
			string original = this.Request.PopFixedString();
			string original2 = this.Request.PopFixedString();
			Room room = CyberEnvironment.GetGame().GetRoomManager().GetRoom(this.Session.GetHabbo().CurrentRoomId);
			if (room == null || !room.CheckRights(this.Session, true, false) || room.Event == null)
			{
				return;
			}
			room.Event.Name = original;
            room.Event.Description = original2;
			CyberEnvironment.GetGame().GetRoomEvents().UpdateEvent(room.Event);
		}
		internal void HandleBotSpeechList()
		{
			int num = this.Request.PopWiredInt32();
			int num2 = this.Request.PopWiredInt32();
			int num3 = num2;
			if (num3 == 2)
			{
				string text = "";
				using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
				{
					queryreactor.setQuery("SELECT * FROM bots_speech WHERE bot_id = " + num);
					DataTable table = queryreactor.getTable();
					if (table != null)
					{
						foreach (DataRow dataRow in table.Rows)
						{
							text = text + dataRow["text"] + "\n";
						}
						queryreactor.setQuery("SELECT * FROM bots WHERE id = " + num);
						DataRow row = queryreactor.getRow();
						text += ";#;";
						text += Convert.ToString(row["automatic_chat"]);
						text += ";#;";
						text += Convert.ToString(row["speaking_interval"]);
						text += ";#;";
						text += Convert.ToString(row["mix_phrases"]);
					}
				}
				ServerMessage serverMessage = new ServerMessage(Outgoing.BotSpeechListMessageComposer);
				serverMessage.AppendInt32(num);
				serverMessage.AppendInt32(num2);
				serverMessage.AppendString(text);
				this.Response = serverMessage;
				this.SendResponse();
				return;
			}
			if (num3 != 5)
			{
				return;
			}
			ServerMessage serverMessage2 = new ServerMessage(Outgoing.BotSpeechListMessageComposer);
			serverMessage2.AppendInt32(num);
			serverMessage2.AppendInt32(num2);
			using (IQueryAdapter queryreactor2 = CyberEnvironment.GetDatabaseManager().getQueryReactor())
			{
				queryreactor2.setQuery("SELECT name FROM bots WHERE id = " + num);
				serverMessage2.AppendString(queryreactor2.getString());
			}
			this.Response = serverMessage2;
			this.SendResponse();
		}
		internal void ManageBotActions()
		{
			Room room = CyberEnvironment.GetGame().GetRoomManager().GetRoom(this.Session.GetHabbo().CurrentRoomId);
			uint num = this.Request.PopWiredUInt();
			int num2 = this.Request.PopWiredInt32();
			string text = CyberEnvironment.FilterInjectionChars(this.Request.PopFixedString());
			RoomUser bot = room.GetRoomUserManager().GetBot(num);
			bool flag = false;
			checked
			{
				switch (num2)
				{
				case 1:
					bot.BotData.Look = this.Session.GetHabbo().Look;
					goto IL_439;
				case 2:
					try
					{
						string[] array = text.Split(new string[]
						{
							";#;"
						}, StringSplitOptions.None);
						string[] array2 = array[0].Split(new char[]
						{
							'\r',
							'\n'
						}, StringSplitOptions.RemoveEmptyEntries);
						string text2 = array[1];
						string value = array[2];
						string text3 = array[3];
						if (string.IsNullOrEmpty(value) || Convert.ToInt32(value) <= 0)
						{
							value = "7";
						}
						using (IQueryAdapter queryreactor2 = CyberEnvironment.GetDatabaseManager().getQueryReactor())
						{
							queryreactor2.runFastQuery("DELETE FROM bots_speech WHERE bot_id = '" + num + "'");
						}
						for (int i = 0; i <= array2.Length - 1; i++)
						{
							using (IQueryAdapter queryreactor3 = CyberEnvironment.GetDatabaseManager().getQueryReactor())
							{
								queryreactor3.setQuery("INSERT INTO `bots_speech` (`bot_id`, `text`) VALUES ('" + num + "', @data)");
								queryreactor3.addParameter("data", array2[i]);
								queryreactor3.runQuery();
								queryreactor3.runFastQuery(string.Concat(new object[]
								{
									"UPDATE bots SET automatic_chat='",
									text2,
									"', speaking_interval=",
									Convert.ToInt32(value),
									" , mix_phrases = '",
									text3,
									"' WHERE id = ",
									num
								}));
							}
						}
                        DataTable table;
                        using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
                        {
                            queryreactor.setQuery("SELECT text, shout FROM bots_speech WHERE bot_id = @id;");
                            queryreactor.addParameter("id", num);
                            table = queryreactor.getTable();
                        }
						List<RandomSpeech> list = new List<RandomSpeech>();
						foreach (DataRow dataRow in table.Rows)
						{
							list.Add(new RandomSpeech((string)dataRow["text"], CyberEnvironment.EnumToBool(dataRow["shout"].ToString())));
						}
						List<BotResponse> list2 = new List<BotResponse>();
						room.GetRoomUserManager().RemoveBot(bot.VirtualId, false);
						room.GetRoomUserManager().DeployBot(new RoomBot(bot.BotData.BotId, bot.BotData.OwnerId, bot.BotData.RoomId, AIType.Generic, bot.BotData.WalkingMode, bot.BotData.Name, bot.BotData.Motto, bot.BotData.Look, bot.X, bot.Y, bot.Z, bot.BotData.Rot, 4, 0, 0, 0, ref list, ref list2, bot.BotData.Gender, bot.BotData.DanceId, bot.BotData.IsBartender), null);
						flag = true;
						goto IL_439;
					}
					catch
					{
						return;
					}
				case 3:
					goto IL_439;
				case 4:
					break;
				case 5:
					bot.BotData.Name = text;
					goto IL_439;
				default:
					goto IL_439;
				}
				if (bot.BotData.DanceId > 0)
				{
					bot.BotData.DanceId = 0;
				}
				else
				{
					Random random = new Random();
					bot.DanceId = random.Next(1, 4);
					bot.BotData.DanceId = bot.DanceId;
				}
				ServerMessage serverMessage = new ServerMessage(Outgoing.DanceStatusMessageComposer);
				serverMessage.AppendInt32(bot.VirtualId);
				serverMessage.AppendInt32(bot.BotData.DanceId);
				this.Session.GetHabbo().CurrentRoom.SendMessage(serverMessage);
				IL_439:
				if (!flag)
				{
					ServerMessage serverMessage2 = new ServerMessage(Outgoing.SetRoomUserMessageComposer);
					serverMessage2.AppendInt32(1);
					bot.Serialize(serverMessage2, room.GetGameMap().gotPublicPool);
					room.SendMessage(serverMessage2);
				}
			}
		}
		internal void RoomOnLoad()
		{
            // TODO!
			this.Response.Init(Outgoing.SendRoomCampaignFurnitureMessageComposer);
			this.Response.AppendInt32(0);
			this.SendResponse();
		}
		internal void MuteAll()
		{
			Room currentRoom = this.Session.GetHabbo().CurrentRoom;
			if (currentRoom == null || !currentRoom.CheckRights(this.Session, true, false))
			{
				return;
			}
			currentRoom.RoomMuted = !currentRoom.RoomMuted;

			this.Response.Init(Outgoing.RoomMuteStatusMessageComposer);
			this.Response.AppendBoolean(currentRoom.RoomMuted);
			this.Session.SendMessage(this.Response);
		}
		internal void RetrieveSongID()
		{
			string text = this.Request.PopFixedString();
			uint songId = SongManager.GetSongId(text);
			if (songId != 0)
			{
				ServerMessage serverMessage = new ServerMessage(Outgoing.RetrieveSongIDMessageComposer);
				serverMessage.AppendString(text);
				serverMessage.AppendUInt(songId);
				this.Session.SendMessage(serverMessage);
			}
		}
		internal void GetMusicData()
		{
			int num = this.Request.PopWiredInt32();
			List<SongData> list = new List<SongData>();
			checked
			{
				for (int i = 0; i < num; i++)
				{
					SongData song = SongManager.GetSong(this.Request.PopWiredUInt());
					if (song != null)
					{
						list.Add(song);
					}
				}
				this.Session.SendMessage(JukeboxComposer.Compose(list));
				list.Clear();
			}
		}
		internal void AddPlaylistItem()
		{
			if (this.Session == null || this.Session.GetHabbo() == null || this.Session.GetHabbo().CurrentRoom == null)
			{
				return;
			}
			Room currentRoom = this.Session.GetHabbo().CurrentRoom;
			if (!currentRoom.CheckRights(this.Session, true, false))
			{
				return;
			}
			RoomMusicController roomMusicController = currentRoom.GetRoomMusicController();
			if (roomMusicController.PlaylistSize >= roomMusicController.PlaylistCapacity)
			{
				return;
			}
			uint num = this.Request.PopWiredUInt();
			UserItem item = this.Session.GetHabbo().GetInventoryComponent().GetItem(num);
			if (item == null || item.GetBaseItem().InteractionType != InteractionType.musicdisc)
			{
				return;
			}
			SongItem songItem = new SongItem(item);
			int num2 = roomMusicController.AddDisk(songItem);
			if (num2 < 0)
			{
				return;
			}
			songItem.SaveToDatabase(currentRoom.RoomId);
			this.Session.GetHabbo().GetInventoryComponent().RemoveItem(num, true);
			using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
			{
				queryreactor.runFastQuery("UPDATE items SET user_id='0' WHERE id=" + num + " LIMIT 1");
			}
			this.Session.SendMessage(JukeboxComposer.Compose(roomMusicController.PlaylistCapacity, roomMusicController.Playlist.Values.ToList<SongInstance>()));
		}
		internal void RemovePlaylistItem()
		{
			if (this.Session == null || this.Session.GetHabbo() == null || this.Session.GetHabbo().CurrentRoom == null)
			{
				return;
			}
			Room currentRoom = this.Session.GetHabbo().CurrentRoom;
			if (!currentRoom.GotMusicController())
			{
				return;
			}
			RoomMusicController roomMusicController = currentRoom.GetRoomMusicController();
			SongItem songItem = roomMusicController.RemoveDisk(this.Request.PopWiredInt32());
			if (songItem == null)
			{
				return;
			}
			songItem.RemoveFromDatabase();
			this.Session.GetHabbo().GetInventoryComponent().AddNewItem(songItem.itemID, songItem.baseItem.ItemId, songItem.extraData, 0u, false, true, 0, 0, songItem.songCode);
			this.Session.GetHabbo().GetInventoryComponent().UpdateItems(false);
			using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
			{
				queryreactor.runFastQuery(string.Concat(new object[]
				{
					"UPDATE items SET user_id='",
					this.Session.GetHabbo().Id,
					"' WHERE id=",
					songItem.itemID,
					" LIMIT 1"
				}));
			}
			this.Session.SendMessage(JukeboxComposer.SerializeSongInventory(this.Session.GetHabbo().GetInventoryComponent().songDisks));
			this.Session.SendMessage(JukeboxComposer.Compose(roomMusicController.PlaylistCapacity, roomMusicController.Playlist.Values.ToList<SongInstance>()));
		}
		internal void GetDisks()
		{
			if (this.Session == null || this.Session.GetHabbo() == null || this.Session.GetHabbo().GetInventoryComponent() == null)
			{
				return;
			}
			this.Session.SendMessage(JukeboxComposer.SerializeSongInventory(this.Session.GetHabbo().GetInventoryComponent().songDisks));
		}
		internal void GetPlaylists()
		{
			if (this.Session == null || this.Session.GetHabbo() == null || this.Session.GetHabbo().CurrentRoom == null)
			{
				return;
			}
			Room currentRoom = this.Session.GetHabbo().CurrentRoom;
			if (!currentRoom.GotMusicController())
			{
				return;
			}
			RoomMusicController roomMusicController = currentRoom.GetRoomMusicController();
			this.Session.SendMessage(JukeboxComposer.Compose(roomMusicController.PlaylistCapacity, roomMusicController.Playlist.Values.ToList<SongInstance>()));
		}
		internal void GetUserInfo()
		{
			this.GetResponse().Init(Outgoing.UpdateUserDataMessageComposer);
			this.GetResponse().AppendInt32(-1);
			this.GetResponse().AppendString(this.Session.GetHabbo().Look);
			this.GetResponse().AppendString(this.Session.GetHabbo().Gender.ToLower());
			this.GetResponse().AppendString(this.Session.GetHabbo().Motto);
			this.GetResponse().AppendInt32(this.Session.GetHabbo().AchievementPoints);
			this.SendResponse();
			this.GetResponse().Init(Outgoing.AchievementPointsMessageComposer);
			this.GetResponse().AppendInt32(this.Session.GetHabbo().AchievementPoints);
			this.SendResponse();
			this.InitMessenger();
			using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
			{
				queryreactor.setQuery("SELECT ip_last FROM users WHERE id=" + this.Session.GetHabbo().Id + " LIMIT 1");
				string @string = queryreactor.getString();
				queryreactor.runFastQuery(string.Concat(new object[]
				{
					"REPLACE INTO user_access (user_id, ip, machineid, last_login) VALUES (",
					this.Session.GetHabbo().Id,
					", '",
					@string,
					"', '",
					this.Session.MachineId,
					"', UNIX_TIMESTAMP())"
				}));
			}
		}
		internal void Pong()
		{
			this.Session.TimePingedReceived = DateTime.Now;
		}
		internal void DisconnectEvent()
		{
			this.Session.Disconnect();
		}
		internal void LatencyTest()
		{
			if (this.Session == null)
			{
				return;
			}
			this.Session.TimePingedReceived = DateTime.Now;
			this.GetResponse().Init(Outgoing.LatencyTestResponseMessageComposer);
			this.GetResponse().AppendInt32(this.Request.PopFixedInt32());
			this.SendResponse();
		}
		internal void HomeRoom()
		{
			this.GetResponse().Init(Outgoing.HomeRoomMessageComposer);
			this.GetResponse().AppendUInt(this.Session.GetHabbo().HomeRoom);
			this.GetResponse().AppendUInt(this.Session.GetHabbo().HomeRoom);
			this.SendResponse();
		}
		internal void GetBalance()
		{
			this.Session.GetHabbo().UpdateCreditsBalance();
			this.Session.GetHabbo().UpdateSeasonalCurrencyBalance();
		}
		internal void GetSubscriptionData()
		{
			this.Session.GetHabbo().SerializeClub();
		}
		internal void LoadSettings()
		{
			using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
			{
				queryreactor.setQuery("SELECT client_volume FROM users WHERE id = '" + this.Session.GetHabbo().Id + "' LIMIT 1");
				string[] array = queryreactor.getString().Split(new char[]
				{
					','
				});
				ServerMessage serverMessage = new ServerMessage(Outgoing.LoadVolumeMessageComposer);
				string[] array2 = array;
				for (int i = 0; i < array2.Length; i++)
				{
					string value = array2[i];
					int num = Convert.ToInt32(value);
					if (num > 100 || num < 0)
					{
						num = 100;
					}
					serverMessage.AppendInt32(num);
				}
				serverMessage.AppendBoolean(false);
                serverMessage.AppendBoolean(false);
				this.Session.SendMessage(serverMessage);
			}
		}
		internal void SaveSettings()
		{
			int num = this.Request.PopWiredInt32();
			int num2 = this.Request.PopWiredInt32();
			int num3 = this.Request.PopWiredInt32();
			using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
			{
				queryreactor.runFastQuery(string.Concat(new object[]
				{
					"UPDATE users SET client_volume = '",
					num,
					",",
					num2,
					",",
					num3,
					"' WHERE id = '",
					this.Session.GetHabbo().Id,
					"' LIMIT 1"
				}));
			}
		}
		internal void GetBadges()
		{
			this.Session.SendMessage(this.Session.GetHabbo().GetBadgeComponent().Serialize());
		}
		internal void UpdateBadges()
		{
			this.Session.GetHabbo().GetBadgeComponent().ResetSlots();
			using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
			{
				queryreactor.runFastQuery("UPDATE user_badges SET badge_slot = 0 WHERE user_id = " + this.Session.GetHabbo().Id);
			}
			checked
			{
				for (int i = 0; i < 5; i++)
				{
					int num = this.Request.PopWiredInt32();
					string text = this.Request.PopFixedString();
					if (text.Length != 0)
					{
						if (!this.Session.GetHabbo().GetBadgeComponent().HasBadge(text) || num < 1 || num > 5)
						{
							return;
						}
						this.Session.GetHabbo().GetBadgeComponent().GetBadge(text).Slot = num;
						using (IQueryAdapter queryreactor2 = CyberEnvironment.GetDatabaseManager().getQueryReactor())
						{
							queryreactor2.setQuery(string.Concat(new object[]
							{
								"UPDATE user_badges SET badge_slot = ",
								num,
								" WHERE badge_id = @badge AND user_id = ",
								this.Session.GetHabbo().Id
							}));
							queryreactor2.addParameter("badge", text);
							queryreactor2.runQuery();
						}
					}
				}
				CyberEnvironment.GetGame().GetQuestManager().ProgressUserQuest(this.Session, QuestType.PROFILE_BADGE, 0u);
				ServerMessage serverMessage = new ServerMessage(Outgoing.UserBadgesMessageComposer);
				serverMessage.AppendUInt(this.Session.GetHabbo().Id);
				serverMessage.AppendInt32(this.Session.GetHabbo().GetBadgeComponent().EquippedCount);
				foreach (Badge badge in this.Session.GetHabbo().GetBadgeComponent().BadgeList.Values)
				{
					if (badge.Slot > 0)
					{
						serverMessage.AppendInt32(badge.Slot);
						serverMessage.AppendString(badge.Code);
					}
				}
				if (this.Session.GetHabbo().InRoom && CyberEnvironment.GetGame().GetRoomManager().GetRoom(this.Session.GetHabbo().CurrentRoomId) != null)
				{
					CyberEnvironment.GetGame().GetRoomManager().GetRoom(this.Session.GetHabbo().CurrentRoomId).SendMessage(serverMessage);
					return;
				}
				this.Session.SendMessage(serverMessage);
			}
		}
		internal void GetAchievements()
		{
			CyberEnvironment.GetGame().GetAchievementManager().GetList(this.Session, this.Request);
		}
        internal void PrepareCampaing()
        {
            string text = this.Request.PopFixedString();
            this.Response.Init(Outgoing.BadgeCampaignMessageComposer);
            this.Response.AppendString(text);
            this.Response.AppendBoolean(this.Session.GetHabbo().GetBadgeComponent().HasBadge(text));
            this.SendResponse();
        }
		private int getFriendsCount(uint userId)
		{
			int result = 0;
			using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
			{
				queryreactor.setQuery("SELECT COUNT(*) FROM messenger_friendships WHERE user_one_id = @id OR user_two_id = @id;");
				queryreactor.addParameter("id", userId);
				result = queryreactor.getInteger();
			}
			return result;
		}

		internal void LoadProfile()
		{
			uint userId = this.Request.PopWiredUInt();
			this.Request.PopWiredBoolean();

			Habbo Habbo = CyberEnvironment.getHabboForId(userId);
			if (Habbo == null)
			{
				this.Session.SendNotif("This user may not exist.");
				return;
			}
			DateTime CreateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(Habbo.CreateDate);

			this.Response.Init(Outgoing.UserProfileMessageComposer);
			this.Response.AppendUInt(Habbo.Id);
			this.Response.AppendString(Habbo.Username);
			this.Response.AppendString(Habbo.Look);
			this.Response.AppendString(Habbo.Motto);
			this.Response.AppendString(CreateTime.ToString("dd/MM/yyyy"));
			this.Response.AppendInt32(Habbo.AchievementPoints);
			this.Response.AppendInt32(this.getFriendsCount(userId));
			this.Response.AppendBoolean(Habbo.Id != this.Session.GetHabbo().Id && this.Session.GetHabbo().GetMessenger().FriendshipExists(Habbo.Id));
			this.Response.AppendBoolean(Habbo.Id != this.Session.GetHabbo().Id && !this.Session.GetHabbo().GetMessenger().FriendshipExists(Habbo.Id) && this.Session.GetHabbo().GetMessenger().RequestExists(Habbo.Id));
			this.Response.AppendBoolean(CyberEnvironment.GetGame().GetClientManager().GetClientByUserID(Habbo.Id) != null);
            HashSet<GroupUser> Groups = CyberEnvironment.GetGame().GetGroupManager().GetUserGroups(Habbo.Id);
			this.Response.AppendInt32(Groups.Count);
            foreach (GroupUser GroupUs in Groups)
            {
                Guild Group = CyberEnvironment.GetGame().GetGroupManager().GetGroup(GroupUs.GroupId);
                if (Group != null)
                {
                    this.Response.AppendUInt(Group.Id);
                    this.Response.AppendString(Group.Name);
                    this.Response.AppendString(Group.Badge);
                    this.Response.AppendString(CyberEnvironment.GetGame().GetGroupManager().GetGroupColour(Group.Colour1, true));
                    this.Response.AppendString(CyberEnvironment.GetGame().GetGroupManager().GetGroupColour(Group.Colour2, false));
                    this.Response.AppendBoolean(Group.Id == Habbo.FavouriteGroup);
                    this.Response.AppendInt32(-1);
                    this.Response.AppendBoolean(Group.HasForum);
                }
                else
                {
                    this.Response.AppendInt32(1);
                    this.Response.AppendString("THIS GROUP IS INVALID");
                    this.Response.AppendString("");
                    this.Response.AppendString("");
                    this.Response.AppendString("");
                    this.Response.AppendBoolean(false);
                    this.Response.AppendInt32(-1);
                    this.Response.AppendBoolean(false);
                }
            }
			this.Response.AppendInt32(checked(CyberEnvironment.GetUnixTimestamp() - Habbo.LastOnline));
			this.Response.AppendBoolean(true);
			this.SendResponse();
			this.Response.Init(Outgoing.UserBadgesMessageComposer);
			this.Response.AppendUInt(Habbo.Id);
			this.Response.AppendInt32(Habbo.GetBadgeComponent().EquippedCount);

			foreach (Badge Badge in Habbo.GetBadgeComponent().BadgeList.Values)
			{
				if (Badge.Slot > 0)
				{
					this.Response.AppendInt32(Badge.Slot);
					this.Response.AppendString(Badge.Code);
				}
			}
			this.SendResponse();
		}

		internal void ChangeLook()
		{
			string text = this.Request.PopFixedString().ToUpper();
			string text2 = this.Request.PopFixedString();
            text2 = CyberEnvironment.FilterFigure(text2);
            text2 = CyberEnvironment.GetGame().GetAntiMutant().RunLook(text2);

			CyberEnvironment.GetGame().GetQuestManager().ProgressUserQuest(this.Session, QuestType.PROFILE_CHANGE_LOOK, 0u);
            this.Session.GetHabbo().Look = text2;
			this.Session.GetHabbo().Gender = text.ToLower();
			using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
			{
				queryreactor.setQuery("UPDATE users SET look =  @Look, gender =  @gender WHERE id = " + this.Session.GetHabbo().Id);
				queryreactor.addParameter("look", text2);
				queryreactor.addParameter("gender", text);
				queryreactor.runQuery();
			}
			CyberEnvironment.GetGame().GetAchievementManager().ProgressUserAchievement(this.Session, "ACH_AvatarLooks", 1, false);
			if (this.Session.GetHabbo().Look.Contains("ha-1006"))
			{
				CyberEnvironment.GetGame().GetQuestManager().ProgressUserQuest(this.Session, QuestType.WEAR_HAT, 0u);
			}
			this.Session.GetMessageHandler().GetResponse().Init(Outgoing.UpdateAvatarAspectMessageComposer);
			this.Session.GetMessageHandler().GetResponse().AppendString(this.Session.GetHabbo().Look);
			this.Session.GetMessageHandler().GetResponse().AppendString(this.Session.GetHabbo().Gender.ToUpper());
			this.Session.GetMessageHandler().SendResponse();
			this.Session.GetMessageHandler().GetResponse().Init(Outgoing.UpdateUserDataMessageComposer);
			this.Session.GetMessageHandler().GetResponse().AppendInt32(-1);
			this.Session.GetMessageHandler().GetResponse().AppendString(this.Session.GetHabbo().Look);
			this.Session.GetMessageHandler().GetResponse().AppendString(this.Session.GetHabbo().Gender.ToLower());
			this.Session.GetMessageHandler().GetResponse().AppendString(this.Session.GetHabbo().Motto);
			this.Session.GetMessageHandler().GetResponse().AppendInt32(this.Session.GetHabbo().AchievementPoints);
			this.Session.GetMessageHandler().SendResponse();
			if (this.Session.GetHabbo().InRoom)
			{
				Room currentRoom = this.Session.GetHabbo().CurrentRoom;
				if (currentRoom == null)
				{
					return;
				}
				RoomUser roomUserByHabbo = currentRoom.GetRoomUserManager().GetRoomUserByHabbo(this.Session.GetHabbo().Id);
				if (roomUserByHabbo == null)
				{
					return;
				}
				ServerMessage serverMessage = new ServerMessage(Outgoing.UpdateUserDataMessageComposer);
				serverMessage.AppendInt32(roomUserByHabbo.VirtualId);
				serverMessage.AppendString(this.Session.GetHabbo().Look);
				serverMessage.AppendString(this.Session.GetHabbo().Gender.ToLower());
				serverMessage.AppendString(this.Session.GetHabbo().Motto);
				serverMessage.AppendInt32(this.Session.GetHabbo().AchievementPoints);
				currentRoom.SendMessage(serverMessage);
			}
		}
		internal void ChangeMotto()
		{
			string text = this.Request.PopFixedString();
			if (text == this.Session.GetHabbo().Motto)
			{
				return;
			}
			this.Session.GetHabbo().Motto = text;
			using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
			{
				queryreactor.setQuery("UPDATE users SET motto = @motto WHERE id = '" + this.Session.GetHabbo().Id + "'");
				queryreactor.addParameter("motto", text);
				queryreactor.runQuery();
			}
			CyberEnvironment.GetGame().GetQuestManager().ProgressUserQuest(this.Session, QuestType.PROFILE_CHANGE_MOTTO, 0u);
			if (this.Session.GetHabbo().InRoom)
			{
				Room currentRoom = this.Session.GetHabbo().CurrentRoom;
				if (currentRoom == null)
				{
					return;
				}
				RoomUser roomUserByHabbo = currentRoom.GetRoomUserManager().GetRoomUserByHabbo(this.Session.GetHabbo().Id);
				if (roomUserByHabbo == null)
				{
					return;
				}
				ServerMessage serverMessage = new ServerMessage(Outgoing.UpdateUserDataMessageComposer);
				serverMessage.AppendInt32(roomUserByHabbo.VirtualId);
				serverMessage.AppendString(this.Session.GetHabbo().Look);
				serverMessage.AppendString(this.Session.GetHabbo().Gender.ToLower());
				serverMessage.AppendString(this.Session.GetHabbo().Motto);
				serverMessage.AppendInt32(this.Session.GetHabbo().AchievementPoints);
				currentRoom.SendMessage(serverMessage);
			}
			CyberEnvironment.GetGame().GetAchievementManager().ProgressUserAchievement(this.Session, "ACH_Motto", 1, false);
		}
		internal void GetWardrobe()
		{
			this.GetResponse().Init(Outgoing.LoadWardrobeMessageComposer);
			this.GetResponse().AppendInt32(0);
			using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
			{
				queryreactor.setQuery("SELECT slot_id, look, gender FROM user_wardrobe WHERE user_id = " + this.Session.GetHabbo().Id);
				DataTable table = queryreactor.getTable();
				if (table == null)
				{
					this.GetResponse().AppendInt32(0);
				}
				else
				{
					this.GetResponse().AppendInt32(table.Rows.Count);
					foreach (DataRow dataRow in table.Rows)
					{
						this.GetResponse().AppendUInt(Convert.ToUInt32(dataRow["slot_id"]));
						this.GetResponse().AppendString((string)dataRow["look"]);
						this.GetResponse().AppendString(dataRow["gender"].ToString().ToUpper());
					}
				}
				this.SendResponse();
			}
		}
		internal void SaveWardrobe()
		{
			uint num = this.Request.PopWiredUInt();
			string text = this.Request.PopFixedString();
			string text2 = this.Request.PopFixedString();

            text = CyberEnvironment.FilterFigure(text);
            text = CyberEnvironment.GetGame().GetAntiMutant().RunLook(text);

			using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
			{
				queryreactor.setQuery(string.Concat(new object[]
				{
					"SELECT null FROM user_wardrobe WHERE user_id = ",
					this.Session.GetHabbo().Id,
					" AND slot_id = ",
					num
				}));
				queryreactor.addParameter("look", text);
				queryreactor.addParameter("gender", text2.ToUpper());
				if (queryreactor.getRow() != null)
				{
					queryreactor.setQuery(string.Concat(new object[]
					{
						"UPDATE user_wardrobe SET look = @look, gender = @gender WHERE user_id = ",
						this.Session.GetHabbo().Id,
						" AND slot_id = ",
						num,
						";"
					}));
					queryreactor.addParameter("look", text);
					queryreactor.addParameter("gender", text2.ToUpper());
					queryreactor.runQuery();
				}
				else
				{
					queryreactor.setQuery(string.Concat(new object[]
					{
						"INSERT INTO user_wardrobe (user_id,slot_id,look,gender) VALUES (",
						this.Session.GetHabbo().Id,
						",",
						num,
						",@look,@gender)"
					}));
					queryreactor.addParameter("look", text);
					queryreactor.addParameter("gender", text2.ToUpper());
					queryreactor.runQuery();
				}
			}
		}
		internal void GetPetsInventory()
		{
			if (this.Session.GetHabbo().GetInventoryComponent() == null)
			{
				return;
			}
			this.Session.SendMessage(this.Session.GetHabbo().GetInventoryComponent().SerializePetInventory());
		}
		internal void GetBotsInventory()
		{
			this.Session.SendMessage(this.Session.GetHabbo().GetInventoryComponent().SerializeBotInventory());
			this.SendResponse();
		}
		internal void CheckName()
		{
			string text = this.Request.PopFixedString();
            if (text.ToLower() == Session.GetHabbo().Username.ToLower())
            {
                this.Response.Init(Outgoing.NameChangedUpdatesMessageComposer);
                this.Response.AppendInt32(0);
                this.Response.AppendString(text);
                this.Response.AppendInt32(0);
                this.SendResponse();
                return;
            }
			using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
			{
				queryreactor.setQuery("SELECT username FROM users WHERE username=@name LIMIT 1");
				queryreactor.addParameter("name", text);
				string @string = queryreactor.getString();
				char[] array = text.ToLower().ToCharArray();
				string source = "abcdefghijklmnopqrstuvwxyz1234567890.,_-;:?!@áéíóúÁÉÍÓÚñÑÜüÝý ";
				char[] array2 = array;
				for (int i = 0; i < array2.Length; i++)
				{
					char c = array2[i];
					if (!source.Contains(char.ToLower(c)))
					{
						this.Response.Init(Outgoing.NameChangedUpdatesMessageComposer);
						this.Response.AppendInt32(4);
						this.Response.AppendString(text);
						this.Response.AppendInt32(0);
						this.SendResponse();
						return;
					}
				}
				if (text.ToLower().Contains("mod") || text.ToLower().Contains("m0d") || text.Contains(" ") || text.ToLower().Contains("admin"))
				{
					this.Response.Init(Outgoing.NameChangedUpdatesMessageComposer);
					this.Response.AppendInt32(4);
					this.Response.AppendString(text);
					this.Response.AppendInt32(0);
					this.SendResponse();
				}
				else
				{
					if (text.Length > 15)
					{
						this.Response.Init(Outgoing.NameChangedUpdatesMessageComposer);
						this.Response.AppendInt32(3);
						this.Response.AppendString(text);
						this.Response.AppendInt32(0);
						this.SendResponse();
					}
					else
					{
						if (text.Length < 3)
						{
							this.Response.Init(Outgoing.NameChangedUpdatesMessageComposer);
							this.Response.AppendInt32(2);
							this.Response.AppendString(text);
							this.Response.AppendInt32(0);
							this.SendResponse();
						}
						else
						{
							if (string.IsNullOrWhiteSpace(@string))
							{
								this.Response.Init(Outgoing.NameChangedUpdatesMessageComposer);
								this.Response.AppendInt32(0);
								this.Response.AppendString(text);
								this.Response.AppendInt32(0);
								this.SendResponse();
							}
							else
							{
								queryreactor.setQuery("SELECT tag FROM user_tags ORDER BY RAND() LIMIT 3");
								DataTable table = queryreactor.getTable();
								this.Response.Init(Outgoing.NameChangedUpdatesMessageComposer);
								this.Response.AppendInt32(5);
								this.Response.AppendString(text);
								this.Response.AppendInt32(table.Rows.Count);
								foreach (DataRow dataRow in table.Rows)
								{
									this.Response.AppendString(text + dataRow[0].ToString());
								}
								this.SendResponse();
							}
						}
					}
				}
			}
		}
		internal void ChangeName()
		{
			string text = this.Request.PopFixedString();
			string username = this.Session.GetHabbo().Username;
			checked
			{
				using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
				{
					queryreactor.setQuery("SELECT username FROM users WHERE username=@name LIMIT 1");
					queryreactor.addParameter("name", text);
					string @string = queryreactor.getString();
					if (string.IsNullOrWhiteSpace(@string) || username.ToLower() == text.ToLower())
					{
						queryreactor.setQuery(string.Concat(new object[]
						{
							"UPDATE users SET username=@name, last_name_change=",
							CyberEnvironment.GetUnixTimestamp() + 43200,
							" WHERE id=",
							this.Session.GetHabbo().Id,
							" LIMIT 1;UPDATE rooms SET owner=@name WHERE owner='",
							this.Session.GetHabbo().Username,
							"';"
						}));
						queryreactor.addParameter("name", text);
						queryreactor.runQuery();
						this.Session.GetHabbo().LastChange = CyberEnvironment.GetUnixTimestamp() + 43200;
						this.Session.GetHabbo().Username = text;
						this.Response.Init(Outgoing.UpdateUserNameMessageComposer);
						this.Response.AppendInt32(0);
						this.Response.AppendString(text);
						this.Response.AppendInt32(0);
						this.SendResponse();
						this.Response.Init(Outgoing.UpdateUserDataMessageComposer);
						this.Response.AppendInt32(-1);
						this.Response.AppendString(this.Session.GetHabbo().Look);
						this.Response.AppendString(this.Session.GetHabbo().Gender.ToLower());
						this.Response.AppendString(this.Session.GetHabbo().Motto);
						this.Response.AppendInt32(this.Session.GetHabbo().AchievementPoints);
						this.SendResponse();
						this.Session.GetHabbo().CurrentRoom.GetRoomUserManager().UpdateUser(username, text);
						if (this.Session.GetHabbo().CurrentRoom != null)
						{
							this.Response.Init(Outgoing.UserUpdateNameInRoomMessageComposer);
							this.Response.AppendUInt(this.Session.GetHabbo().Id);
							this.Response.AppendUInt(this.Session.GetHabbo().CurrentRoom.RoomId);
							this.Response.AppendString(text);
						}
						foreach (RoomData current in this.Session.GetHabbo().UsersRooms)
						{
							current.Owner = text;
							current.SerializeRoomData(this.Response, false, this.Session, true);
							Room room = CyberEnvironment.GetGame().GetRoomManager().GetRoom(current.Id);
							if (room != null)
							{
								room.Owner = text;
							}
						}
						foreach (MessengerBuddy current2 in this.Session.GetHabbo().GetMessenger().friends.Values)
						{
							if (current2.client != null)
							{
								foreach (MessengerBuddy current3 in current2.client.GetHabbo().GetMessenger().friends.Values)
								{
									if (current3.mUsername == username)
									{
										current3.mUsername = text;
										current3.Serialize(this.Response, current2.client);
									}
								}
							}
						}
					}
				}
			}
		}
		internal void GetRelationships()
		{
			uint userId = this.Request.PopWiredUInt();
			Habbo habboForId = CyberEnvironment.getHabboForId(userId);
			if (habboForId == null)
			{
				return;
			}
			Random rand = new Random();
			habboForId.Relationships = (
				from x in habboForId.Relationships
				orderby rand.Next()
				select x).ToDictionary((KeyValuePair<int, Relationship> item) => item.Key, (KeyValuePair<int, Relationship> item) => item.Value);
			int num = habboForId.Relationships.Count((KeyValuePair<int, Relationship> x) => x.Value.Type == 1);
			int num2 = habboForId.Relationships.Count((KeyValuePair<int, Relationship> x) => x.Value.Type == 2);
			int num3 = habboForId.Relationships.Count((KeyValuePair<int, Relationship> x) => x.Value.Type == 3);
			this.Response.Init(Outgoing.RelationshipMessageComposer);
			this.Response.AppendUInt(habboForId.Id);
			this.Response.AppendInt32(habboForId.Relationships.Count);
			foreach (Relationship current in habboForId.Relationships.Values)
			{
				Habbo habboForId2 = CyberEnvironment.getHabboForId(Convert.ToUInt32(current.UserId));
				if (habboForId2 == null)
				{
					this.Response.AppendInt32(0);
					this.Response.AppendInt32(0);
					this.Response.AppendInt32(0);
					this.Response.AppendString("Placeholder");
					this.Response.AppendString("hr-115-42.hd-190-1.ch-215-62.lg-285-91.sh-290-62");
				}
				else
				{
					this.Response.AppendInt32(current.Type);
					this.Response.AppendInt32((current.Type == 1) ? num : ((current.Type == 2) ? num2 : num3));
					this.Response.AppendInt32(current.UserId);
					this.Response.AppendString(habboForId2.Username);
					this.Response.AppendString(habboForId2.Look);
				}
			}
			this.SendResponse();
		}
		internal void SetRelationship()
		{
			uint num = this.Request.PopWiredUInt();
			int num2 = this.Request.PopWiredInt32();
			checked
			{
				using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
				{
					if (num2 == 0)
					{
						queryreactor.setQuery("SELECT id FROM user_relationships WHERE user_id=@id AND target=@target LIMIT 1");
						queryreactor.addParameter("id", this.Session.GetHabbo().Id);
						queryreactor.addParameter("target", num);
						int integer = queryreactor.getInteger();
						queryreactor.setQuery("DELETE FROM user_relationships WHERE user_id=@id AND target=@target LIMIT 1");
						queryreactor.addParameter("id", this.Session.GetHabbo().Id);
						queryreactor.addParameter("target", num);
						queryreactor.runQuery();
						if (this.Session.GetHabbo().Relationships.ContainsKey(integer))
						{
							this.Session.GetHabbo().Relationships.Remove(integer);
						}
					}
					else
					{
						queryreactor.setQuery("SELECT id FROM user_relationships WHERE user_id=@id AND target=@target LIMIT 1");
						queryreactor.addParameter("id", this.Session.GetHabbo().Id);
						queryreactor.addParameter("target", num);
						int integer2 = queryreactor.getInteger();
						if (integer2 > 0)
						{
							queryreactor.setQuery("DELETE FROM user_relationships WHERE user_id=@id AND target=@target LIMIT 1");
							queryreactor.addParameter("id", this.Session.GetHabbo().Id);
							queryreactor.addParameter("target", num);
							queryreactor.runQuery();
							if (this.Session.GetHabbo().Relationships.ContainsKey(integer2))
							{
								this.Session.GetHabbo().Relationships.Remove(integer2);
							}
						}
						queryreactor.setQuery("INSERT INTO user_relationships (user_id, target, type) VALUES (@id, @target, @type)");
						queryreactor.addParameter("id", this.Session.GetHabbo().Id);
						queryreactor.addParameter("target", num);
						queryreactor.addParameter("type", num2);
						int num3 = (int)queryreactor.insertQuery();
						this.Session.GetHabbo().Relationships.Add(num3, new Relationship(num3, (int)num, num2));
					}
					GameClient clientByUserID = CyberEnvironment.GetGame().GetClientManager().GetClientByUserID(num);
					this.Session.GetHabbo().GetMessenger().UpdateFriend(num, clientByUserID, true);
				}
			}
		}

        internal void InitCrypto()
        {
            this.Response.Init(Outgoing.InitCryptoMessageComposer);
            this.Response.AppendString(HabboCrypto.GetDHPrimeKey());
            this.Response.AppendString(HabboCrypto.GetDHGeneratorKey());
            this.SendResponse();
        }

        internal void SecretKey()
        {
            string CipherKey = Request.PopFixedString();
            BigInteger SharedKey = HabboCrypto.CalculateDHSharedKey(CipherKey);

            if (SharedKey != 0)
            {
                this.Response.Init(Outgoing.SecretKeyMessageComposer);
                this.Response.AppendString(HabboCrypto.GetDHPublicKey());
                this.SendResponse();


                Session.ARC4 = new ARC4(SharedKey.getBytes());
            }
            else
            {
                Session.Disconnect();
            }
        }

        internal void MachineId()
        {
            Request.PopFixedString();
            string machineId = Request.PopFixedString();
            Session.MachineId = machineId;
        }

        internal void LoginWithTicket()
        {
            if (Session.GetHabbo() == null)
            {
                Session.tryLogin(Request.PopFixedString());
                if (Session != null)
                {
                    Session.TimePingedReceived = DateTime.Now;
                    return;
                }
            }
        }

        internal void InfoRetrieve()
        {
            if (Session == null || Session.GetHabbo() == null)
            {
                return;
            }
            Habbo Habbo = Session.GetHabbo();

            this.Response.Init(Outgoing.UserObjectMessageComposer);
            this.Response.AppendUInt(Habbo.Id);
            this.Response.AppendString(Habbo.Username);
            this.Response.AppendString(Habbo.Look);
            this.Response.AppendString(Habbo.Gender.ToUpper());
            this.Response.AppendString(Habbo.Motto);
            this.Response.AppendString("");
            this.Response.AppendBoolean(false);
            this.Response.AppendInt32(Habbo.Respect);
            this.Response.AppendInt32(Habbo.DailyRespectPoints);
            this.Response.AppendInt32(Habbo.DailyPetRespectPoints);
            this.Response.AppendBoolean(true);
            this.Response.AppendString(Habbo.LastOnline.ToString());
            this.Response.AppendBoolean(Habbo.CanChangeName);
            this.Response.AppendBoolean(false);
            this.SendResponse();

            this.Response.Init(Outgoing.BuildersClubMembershipMessageComposer);
            this.Response.AppendInt32(int.MaxValue);//expiry
            this.Response.AppendInt32(100);//Furniture limit
            this.Response.AppendInt32(0);
            this.SendResponse();

            bool tradeLocked = Session.GetHabbo().CheckTrading();
            bool canUseFloorEditor = (ExtraSettings.EVERYONE_USE_FLOOR || Session.GetHabbo().VIP || Session.GetHabbo().Rank >= 4);
            this.Response.Init(Outgoing.SendPerkAllowancesMessageComposer);
            this.Response.AppendInt32(13);
            this.Response.AppendString("NAVIGATOR_PHASE_ONE_2014");
            this.Response.AppendString("");
            this.Response.AppendBoolean(ExtraSettings.NAVIGATOR_NEW_ENABLED);
            this.Response.AppendString("CAMERA");
            this.Response.AppendString("");
            this.Response.AppendBoolean(ExtraSettings.ENABLE_BETA_CAMERA);
            this.Response.AppendString("EXPERIMENTAL_CHAT_BETA");
            this.Response.AppendString("");
            this.Response.AppendBoolean(true);
            this.Response.AppendString("CITIZEN");
            this.Response.AppendString("");
            this.Response.AppendBoolean(false);
            this.Response.AppendString("VOTE_IN_COMPETITIONS");
            this.Response.AppendString("requirement.unfulfilled.helper_level_2");
            this.Response.AppendBoolean(false);
            this.Response.AppendString("NEW_UI");
            this.Response.AppendString("");
            this.Response.AppendBoolean(true);
            this.Response.AppendString("USE_GUIDE_TOOL");
            this.Response.AppendString(Session.GetHabbo().Rank >= 4 ? "" : "requirement.unfulfilled.helper_level_4");
            this.Response.AppendBoolean(Session.GetHabbo().Rank >= 4);
            this.Response.AppendString("BUILDER_AT_WORK");
            this.Response.AppendString("");
            this.Response.AppendBoolean(canUseFloorEditor);
            this.Response.AppendString("HEIGHTMAP_EDITOR_BETA");
            this.Response.AppendString("");
            this.Response.AppendBoolean(true);
            this.Response.AppendString("JUDGE_CHAT_REVIEWS");
            this.Response.AppendString("requirement.unfulfilled.helper_level_6");
            this.Response.AppendBoolean(false);
            this.Response.AppendString("EXPERIMENTAL_TOOLBAR");
            this.Response.AppendString("requirement.unfulfilled.group_membership");
            this.Response.AppendBoolean(false);
            this.Response.AppendString("CALL_ON_HELPERS");
            this.Response.AppendString("");
            this.Response.AppendBoolean(true);
            this.Response.AppendString("TRADE");
            this.Response.AppendString(tradeLocked ? "" : "requirement.unfulfilled.no_trade_lock");
            this.Response.AppendBoolean(tradeLocked);
            this.SendResponse();

            Session.GetHabbo().InitMessenger();
        }

        internal void RemoveFavouriteRoom()
        {
            if (Session.GetHabbo() == null)
            {
                return;
            }
            uint num = Request.PopWiredUInt();
            Session.GetHabbo().FavoriteRooms.Remove(num);
            this.Response.Init(Outgoing.FavouriteRoomsUpdateMessageComposer);
            this.Response.AppendUInt(num);
            this.Response.AppendBoolean(false);
            this.SendResponse();

            using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
            {
                queryreactor.runFastQuery(string.Concat(new object[]
				{
					"DELETE FROM user_favorites WHERE user_id = ",
					Session.GetHabbo().Id,
					" AND room_id = ",
					num
				}));
            }
        }

        internal void RoomUserAction()
        {
            Room room = CyberEnvironment.GetGame().GetRoomManager().GetRoom(Session.GetHabbo().CurrentRoomId);
            if (room == null)
            {
                return;
            }
            RoomUser roomUserByHabbo = room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            if (roomUserByHabbo == null)
            {
                return;
            }
            roomUserByHabbo.UnIdle();
            int num = Request.PopWiredInt32();
            roomUserByHabbo.DanceId = 0;

            ServerMessage Action = new ServerMessage(Outgoing.RoomUserActionMessageComposer);
            Action.AppendInt32(roomUserByHabbo.VirtualId);
            Action.AppendInt32(num);
            room.SendMessage(Action);

            if (num == 5)
            {
                roomUserByHabbo.IsAsleep = true;
                ServerMessage Sleep = new ServerMessage(Outgoing.RoomUserIdleMessageComposer);
                Sleep.AppendInt32(roomUserByHabbo.VirtualId);
                Sleep.AppendBoolean(roomUserByHabbo.IsAsleep);
                room.SendMessage(Sleep);
            }
            CyberEnvironment.GetGame().GetQuestManager().ProgressUserQuest(Session, QuestType.SOCIAL_WAVE, 0u);
        }

        internal void RequestLeaveGroup()
        {
            uint GroupId = Request.PopWiredUInt();
            uint UserId = Request.PopWiredUInt();

            Guild Guild = CyberEnvironment.GetGame().GetGroupManager().GetGroup(GroupId);
            if (Guild == null || Guild.CreatorId == UserId)
                return;

            if (UserId == Session.GetHabbo().Id || Guild.Admins.ContainsKey(Session.GetHabbo().Id))
            {
                this.Response.Init(Outgoing.GroupAreYouSureMessageComposer);
                this.Response.AppendUInt(UserId);
                this.Response.AppendInt32(0);
                this.SendResponse();
            }
        }

        internal void ConfirmLeaveGroup()
        {
            uint Guild = Request.PopWiredUInt();
            uint UserId = Request.PopWiredUInt();

            Guild byeGuild = CyberEnvironment.GetGame().GetGroupManager().GetGroup(Guild);

            if (byeGuild == null)
            {
                return;
            }
            else if (byeGuild.CreatorId == UserId)
            {
                Session.SendNotif("You can't leave a group if you are the creator!");
                return;
            }

            if (UserId == Session.GetHabbo().Id || byeGuild.Admins.ContainsKey(Session.GetHabbo().Id))
            {

                GroupUser memberShip;

                if (byeGuild.Members.ContainsKey(UserId))
                {
                    memberShip = byeGuild.Members[UserId];

                    Session.GetHabbo().UserGroups.Remove(memberShip);
                    byeGuild.Members.Remove(UserId);
                }
                else if (byeGuild.Admins.ContainsKey(UserId))
                {
                    memberShip = byeGuild.Admins[UserId];

                    Session.GetHabbo().UserGroups.Remove(memberShip);
                    byeGuild.Admins.Remove(UserId);
                }
                else
                {
                    return;
                }


                this.Response.Init(Outgoing.GroupConfirmLeaveMessageComposer);
                this.Response.AppendUInt(Guild);
                this.Response.AppendUInt(UserId);
                this.SendResponse();

                using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
                {
                    queryreactor.runFastQuery(string.Concat(new object[]
					{
						"DELETE FROM group_memberships WHERE user_id=",
						UserId,
						" AND group_id=",
						Guild,
						" LIMIT 1"
					}));
                }

                Habbo byeUser = CyberEnvironment.getHabboForId(UserId);

                if (byeUser != null && byeUser.FavouriteGroup == Guild)
                {
                    byeUser.FavouriteGroup = 0;
                    using (IQueryAdapter queryreactor2 = CyberEnvironment.GetDatabaseManager().getQueryReactor())
                    {
                        queryreactor2.runFastQuery("UPDATE user_stats SET favourite_group=0 WHERE id=" + UserId + " LIMIT 1");
                    }
                    Room Room = Session.GetHabbo().CurrentRoom;

                    this.Response.Init(Outgoing.FavouriteGroupMessageComposer);
                    this.Response.AppendUInt(byeUser.Id);
                    if (Room != null)
                    {
                        Room.SendMessage(this.Response);
                    }
                    else
                    {
                        this.SendResponse();
                    }
                }

                this.Response.Init(Outgoing.GroupRequestReloadMessageComposer);
                this.Response.AppendUInt(Guild);
                this.SendResponse();

            }



            
        }
    }
}
