using System;
namespace Cyber.Messages.StaticMessageHandlers
{
	internal class SharedPacketLib
	{
        internal static void InitCrypto(GameClientMessageHandler handler)
        {
            handler.InitCrypto();
        }
        internal static void SecretKey(GameClientMessageHandler handler)
        {
            handler.SecretKey();
        }
        internal static void MachineId(GameClientMessageHandler handler)
        {
            handler.MachineId();
        }
        internal static void LoginWithTicket(GameClientMessageHandler handler)
        {
            handler.LoginWithTicket();
        }
        internal static void InfoRetrieve(GameClientMessageHandler handler)
        {
            handler.InfoRetrieve();
        }

        internal static void Chat(GameClientMessageHandler handler)
        {
            handler.Chat();
        }

        internal static void Shout(GameClientMessageHandler handler)
        {
            handler.Shout();
        }

        internal static void RequestFloorPlanUsedCoords(GameClientMessageHandler handler)
        {
            handler.GetFloorPlanUsedCoords();
        }
        internal static void RequestFloorPlanDoor(GameClientMessageHandler handler)
        {
            handler.GetFloorPlanDoor();
        }

        internal static void OpenBullyReporting(GameClientMessageHandler handler)
        {
            handler.OpenBullyReporting();
        }

        internal static void SendBullyReport(GameClientMessageHandler handler)
        {
            handler.SendBullyReport();
        }

		internal static void NavigatorGetPopularGroups(GameClientMessageHandler handler)
		{
			handler.GetPopularGroups();
		}
		internal static void LoadClubGifts(GameClientMessageHandler handler)
		{
			handler.LoadClubGifts();
		}
		internal static void SaveHeightmap(GameClientMessageHandler handler)
		{
			handler.SaveHeightmap();
		}
		internal static void AcceptPoll(GameClientMessageHandler handler)
		{
			handler.AcceptPoll();
		}
		internal static void RefusePoll(GameClientMessageHandler handler)
		{
			handler.RefusePoll();
		}
		internal static void AnswerPollQuestion(GameClientMessageHandler handler)
		{
			handler.AnswerPoll();
		}
		internal static void RetrieveSongID(GameClientMessageHandler handler)
		{
			handler.RetrieveSongID();
		}
		internal static void TileStackMagicSetHeight(GameClientMessageHandler handler)
		{
			handler.TileStackMagicSetHeight();
		}
		internal static void EnableInventoryEffect(GameClientMessageHandler handler)
		{
			handler.EnableEffect();
		}
		internal static void PromoteRoom(GameClientMessageHandler handler)
		{
			handler.PromoteRoom();
		}
		internal static void GetPromotionableRooms(GameClientMessageHandler handler)
		{
			handler.GetPromotionableRooms();
		}
		internal static void GetRoomFilter(GameClientMessageHandler handler)
		{
			handler.GetRoomFilter();
		}
		internal static void AlterRoomFilter(GameClientMessageHandler handler)
		{
			handler.AlterRoomFilter();
		}
		internal static void GetTVPlayer(GameClientMessageHandler handler)
		{
			handler.GetTVPlayer();
		}
		internal static void ChooseTVPlayerVideo(GameClientMessageHandler handler)
		{
			handler.ChooseTVPlayerVideo();
		}
		internal static void GetTVPlaylist(GameClientMessageHandler handler)
		{
			handler.ChooseTVPlaylist();
		}
		internal static void PlaceBot(GameClientMessageHandler handler)
		{
			handler.PlaceBot();
		}
		internal static void PickUpBot(GameClientMessageHandler handler)
		{
			handler.PickUpBot();
		}
		internal static void GetTalentsTrack(GameClientMessageHandler handler)
		{
			handler.Talents();
		}
		internal static void PrepareCampaing(GameClientMessageHandler handler)
		{
			handler.PrepareCampaing();
		}
		internal static void Pong(GameClientMessageHandler handler)
		{
			handler.Pong();
		}
		internal static void DisconnectEvent(GameClientMessageHandler handler)
		{
			handler.DisconnectEvent();
		}
		internal static void LatencyTest(GameClientMessageHandler handler)
		{
			handler.LatencyTest();
		}
		internal static void ReceptionView(GameClientMessageHandler handler)
		{
			handler.GoToHotelView();
		}
		internal static void OnlineConfirmationEvent(GameClientMessageHandler handler)
		{
			handler.OnlineConfirmationEvent();
		}
		internal static void RetriveCitizenShipStatus(GameClientMessageHandler handler)
		{
			handler.RetrieveCitizenship();
		}
		internal static void RefreshPromoEvent(GameClientMessageHandler handler)
		{
			handler.RefreshPromoEvent();
		}
		internal static void WidgetContainer(GameClientMessageHandler handler)
		{
			handler.WidgetContainers();
		}
        internal static void LandingCommunityGoal(GameClientMessageHandler handler)
        {
            handler.LandingCommunityGoal();
        }
		internal static void RemoveHanditem(GameClientMessageHandler handler)
		{
			handler.RemoveHanditem();
		}
		internal static void RedeemVoucher(GameClientMessageHandler handler)
		{
			handler.RedeemVoucher();
		}
		internal static void GiveHanditem(GameClientMessageHandler handler)
		{
			handler.GiveHanditem();
		}
		internal static void InitHelpTool(GameClientMessageHandler handler)
		{
			handler.InitHelpTool();
		}
		internal static void SubmitHelpTicket(GameClientMessageHandler handler)
		{
			handler.SubmitHelpTicket();
		}
		internal static void DeletePendingCFH(GameClientMessageHandler handler)
		{
			handler.DeletePendingCFH();
		}
		internal static void ModGetUserInfo(GameClientMessageHandler handler)
		{
			handler.ModGetUserInfo();
		}
		internal static void ModGetUserChatlog(GameClientMessageHandler handler)
		{
			handler.ModGetUserChatlog();
		}
		internal static void ModGetRoomChatlog(GameClientMessageHandler handler)
		{
			handler.ModGetRoomChatlog();
		}
		internal static void ModGetRoomTool(GameClientMessageHandler handler)
		{
			handler.ModGetRoomTool();
		}
		internal static void ModPickTicket(GameClientMessageHandler handler)
		{
			handler.ModPickTicket();
		}
		internal static void ModReleaseTicket(GameClientMessageHandler handler)
		{
			handler.ModReleaseTicket();
		}
		internal static void ModCloseTicket(GameClientMessageHandler handler)
		{
			handler.ModCloseTicket();
		}
		internal static void ModGetTicketChatlog(GameClientMessageHandler handler)
		{
			handler.ModGetTicketChatlog();
		}
		internal static void ModGetRoomVisits(GameClientMessageHandler handler)
		{
			handler.ModGetRoomVisits();
		}
		internal static void ModSendRoomAlert(GameClientMessageHandler handler)
		{
			handler.ModSendRoomAlert();
		}
		internal static void ModPerformRoomAction(GameClientMessageHandler handler)
		{
			handler.ModPerformRoomAction();
		}
		internal static void ModSendUserCaution(GameClientMessageHandler handler)
		{
			handler.ModSendUserCaution();
		}
		internal static void ModSendUserMessage(GameClientMessageHandler handler)
		{
			handler.ModSendUserMessage();
		}
		internal static void ModKickUser(GameClientMessageHandler handler)
		{
			handler.ModKickUser();
		}
		internal static void ModMuteUser(GameClientMessageHandler handler)
		{
			handler.ModMuteUser();
		}
		internal static void ModLockTrade(GameClientMessageHandler handler)
		{
			handler.ModLockTrade();
		}
		internal static void ModBanUser(GameClientMessageHandler handler)
		{
			handler.ModBanUser();
		}
		internal static void InitMessenger(GameClientMessageHandler handler)
		{
			handler.InitMessenger();
		}
		internal static void FriendsListUpdate(GameClientMessageHandler handler)
		{
			handler.FriendsListUpdate();
		}
		internal static void RemoveBuddy(GameClientMessageHandler handler)
		{
			handler.RemoveBuddy();
		}
		internal static void SearchHabbo(GameClientMessageHandler handler)
		{
			handler.SearchHabbo();
		}
		internal static void AcceptRequest(GameClientMessageHandler handler)
		{
			handler.AcceptRequest();
		}
		internal static void DeclineRequest(GameClientMessageHandler handler)
		{
			handler.DeclineRequest();
		}
		internal static void RequestBuddy(GameClientMessageHandler handler)
		{
			handler.RequestBuddy();
		}
		internal static void SendInstantMessenger(GameClientMessageHandler handler)
		{
			handler.SendInstantMessenger();
		}
		internal static void FollowBuddy(GameClientMessageHandler handler)
		{
			handler.FollowBuddy();
		}
		internal static void SendInstantInvite(GameClientMessageHandler handler)
		{
			handler.SendInstantInvite();
		}
		internal static void HomeRoomStuff(GameClientMessageHandler handler)
		{
			handler.HomeRoom();
		}
		internal static void AddFavorite(GameClientMessageHandler handler)
		{
			handler.AddFavorite();
		}
		internal static void RemoveFavorite(GameClientMessageHandler handler)
		{
			handler.RemoveFavorite();
		}
		internal static void GetFlatCats(GameClientMessageHandler handler)
		{
			handler.GetFlatCats();
		}
		internal static void EnterInquiredRoom(GameClientMessageHandler handler)
		{
			handler.EnterInquiredRoom();
		}
		internal static void GetPubs(GameClientMessageHandler handler)
		{
			handler.GetPubs();
		}
		internal static void SaveBranding(GameClientMessageHandler handler)
		{
			handler.SaveBranding();
		}
		internal static void GetRoomInfo(GameClientMessageHandler handler)
		{
			handler.GetRoomInfo();
		}
		internal static void GetPopularRooms(GameClientMessageHandler handler)
		{
			handler.GetPopularRooms();
		}
        internal static void GetRecommendedRooms(GameClientMessageHandler handler)
        {
            handler.GetRecommendedRooms();
        }
		internal static void GetHighRatedRooms(GameClientMessageHandler handler)
		{
			handler.GetHighRatedRooms();
		}
		internal static void GetFriendsRooms(GameClientMessageHandler handler)
		{
			handler.GetFriendsRooms();
		}
		internal static void GetRoomsWithFriends(GameClientMessageHandler handler)
		{
			handler.GetRoomsWithFriends();
		}
		internal static void GetOwnRooms(GameClientMessageHandler handler)
		{
			handler.GetOwnRooms();
		}
        internal static void NewNavigatorFlatCats(GameClientMessageHandler handler)
        {
            handler.NewNavigatorFlatCats();
        }
		internal static void GetFavoriteRooms(GameClientMessageHandler handler)
		{
			handler.GetFavoriteRooms();
		}
		internal static void GetRecentRooms(GameClientMessageHandler handler)
		{
			handler.GetRecentRooms();
		}
		internal static void GetPopularTags(GameClientMessageHandler handler)
		{
			handler.GetPopularTags();
		}
		internal static void PerformSearch(GameClientMessageHandler handler)
		{
			handler.PerformSearch();
		}
        internal static void SearchByTag(GameClientMessageHandler handler)
        {
            handler.SearchByTag();
        }
		internal static void PerformSearch2(GameClientMessageHandler handler)
		{
			handler.PerformSearch2();
		}
		internal static void OpenFlat(GameClientMessageHandler handler)
		{
			handler.OpenFlat();
		}
		internal static void GetVoume(GameClientMessageHandler handler)
		{
			handler.LoadSettings();
		}
		internal static void SaveVolume(GameClientMessageHandler handler)
		{
			handler.SaveSettings();
		}
		internal static void GetPub(GameClientMessageHandler handler)
		{
			handler.GetPub();
		}
		internal static void OpenPub(GameClientMessageHandler handler)
		{
			handler.OpenPub();
		}
		internal static void GetInventory(GameClientMessageHandler handler)
		{
			handler.GetInventory();
		}
		internal static void GetRoomData1(GameClientMessageHandler handler)
		{
			handler.GetRoomData1();
		}
		internal static void GetRoomData2(GameClientMessageHandler handler)
		{
			handler.GetRoomData2();
		}
		internal static void GetRoomData3(GameClientMessageHandler handler)
		{
			handler.GetRoomData3();
		}
		internal static void RequestFloorItems(GameClientMessageHandler handler)
		{
			handler.RequestFloorItems();
		}
		internal static void RequestWallItems(GameClientMessageHandler handler)
		{
			handler.RequestWallItems();
		}
		internal static void OnRoomUserAdd(GameClientMessageHandler handler)
		{
			handler.OnRoomUserAdd();
		}
		internal static void ReqLoadRoomForUser(GameClientMessageHandler handler)
		{
			handler.ReqLoadRoomForUser();
		}
		internal static void enterOnRoom(GameClientMessageHandler handler)
		{
			handler.enterOnRoom();
		}
		internal static void ClearRoomLoading(GameClientMessageHandler handler)
		{
			handler.ClearRoomLoading();
		}
		internal static void Move(GameClientMessageHandler handler)
		{
			handler.Move();
		}
		internal static void CanCreateRoom(GameClientMessageHandler handler)
		{
			handler.CanCreateRoom();
		}
		internal static void CreateRoom(GameClientMessageHandler handler)
		{
			handler.CreateRoom();
		}
		internal static void GetRoomInformation(GameClientMessageHandler handler)
		{
			handler.ParseRoomDataInformation();
		}
		internal static void GetRoomEditData(GameClientMessageHandler handler)
		{
			handler.GetRoomEditData();
		}
		internal static void SaveRoomData(GameClientMessageHandler handler)
		{
			handler.SaveRoomData();
		}
		internal static void GiveRights(GameClientMessageHandler handler)
		{
			handler.GiveRights();
		}
		internal static void TakeRights(GameClientMessageHandler handler)
		{
			handler.TakeRights();
		}
		internal static void TakeAllRights(GameClientMessageHandler handler)
		{
			handler.TakeAllRights();
		}
		internal static void KickUser(GameClientMessageHandler handler)
		{
			handler.KickUser();
		}
		internal static void BanUser(GameClientMessageHandler handler)
		{
			handler.BanUser();
		}
		internal static void SetHomeRoom(GameClientMessageHandler handler)
		{
			handler.SetHomeRoom();
		}
		internal static void DeleteRoom(GameClientMessageHandler handler)
		{
			handler.DeleteRoom();
		}
		internal static void LookAt(GameClientMessageHandler handler)
		{
			handler.LookAt();
		}
		internal static void StartTyping(GameClientMessageHandler handler)
		{
			handler.StartTyping();
		}
		internal static void StopTyping(GameClientMessageHandler handler)
		{
			handler.StopTyping();
		}
		internal static void IgnoreUser(GameClientMessageHandler handler)
		{
			handler.IgnoreUser();
		}
		internal static void UnignoreUser(GameClientMessageHandler handler)
		{
			handler.UnignoreUser();
		}
		internal static void CanCreateRoomEvent(GameClientMessageHandler handler)
		{
			handler.CanCreateRoomEvent();
		}
		internal static void Sign(GameClientMessageHandler handler)
		{
			handler.Sign();
		}
		internal static void GetUserTags(GameClientMessageHandler handler)
		{
			handler.GetUserTags();
		}
		internal static void GetUserBadges(GameClientMessageHandler handler)
		{
			handler.GetUserBadges();
		}
		internal static void RateRoom(GameClientMessageHandler handler)
		{
			handler.RateRoom();
		}
		internal static void Dance(GameClientMessageHandler handler)
		{
			handler.Dance();
		}
		internal static void AnswerDoorbell(GameClientMessageHandler handler)
		{
			handler.AnswerDoorbell();
		}
		internal static void ApplyRoomEffect(GameClientMessageHandler handler)
		{
			handler.ApplyRoomEffect();
		}
		internal static void PlacePostIt(GameClientMessageHandler handler)
		{
			handler.PlacePostIt();
		}
		internal static void PlaceItem(GameClientMessageHandler handler)
		{
			handler.PlaceItem();
		}
		internal static void TakeItem(GameClientMessageHandler handler)
		{
			handler.TakeItem();
		}
		internal static void MoveItem(GameClientMessageHandler handler)
		{
			handler.MoveItem();
		}
		internal static void MoveWallItem(GameClientMessageHandler handler)
		{
			handler.MoveWallItem();
		}
		internal static void TriggerItem(GameClientMessageHandler handler)
		{
			handler.TriggerItem();
		}
		internal static void TriggerItemDiceSpecial(GameClientMessageHandler handler)
		{
			handler.TriggerItemDiceSpecial();
		}
		internal static void OpenPostit(GameClientMessageHandler handler)
		{
			handler.OpenPostit();
		}
		internal static void SavePostit(GameClientMessageHandler handler)
		{
			handler.SavePostit();
		}
		internal static void DeletePostit(GameClientMessageHandler handler)
		{
			handler.DeletePostit();
		}
		internal static void OpenPresent(GameClientMessageHandler handler)
		{
            handler.OpenGift();
		}
		internal static void GetMoodlight(GameClientMessageHandler handler)
		{
			handler.GetMoodlight();
		}
		internal static void UpdateMoodlight(GameClientMessageHandler handler)
		{
			handler.UpdateMoodlight();
		}
		internal static void SwitchMoodlightStatus(GameClientMessageHandler handler)
		{
			handler.SwitchMoodlightStatus();
		}
		internal static void InitTrade(GameClientMessageHandler handler)
		{
			handler.InitTrade();
		}
		internal static void OfferTradeItem(GameClientMessageHandler handler)
		{
			handler.OfferTradeItem();
		}
		internal static void TakeBackTradeItem(GameClientMessageHandler handler)
		{
			handler.TakeBackTradeItem();
		}
		internal static void StopTrade(GameClientMessageHandler handler)
		{
			handler.StopTrade();
		}
		internal static void AcceptTrade(GameClientMessageHandler handler)
		{
			handler.AcceptTrade();
		}
		internal static void UnacceptTrade(GameClientMessageHandler handler)
		{
			handler.UnacceptTrade();
		}
		internal static void CompleteTrade(GameClientMessageHandler handler)
		{
			handler.CompleteTrade();
		}
		internal static void GiveRespect(GameClientMessageHandler handler)
		{
			handler.GiveRespect();
		}
		internal static void ApplyEffect(GameClientMessageHandler handler)
		{
			handler.ApplyEffect();
		}
		internal static void EnableEffect(GameClientMessageHandler handler)
		{
			handler.EnableEffect();
		}
		internal static void RecycleItems(GameClientMessageHandler handler)
		{
			handler.RecycleItems();
		}
		internal static void RedeemExchangeFurni(GameClientMessageHandler handler)
		{
			handler.RedeemExchangeFurni();
		}
		internal static void KickBot(GameClientMessageHandler handler)
		{
			handler.KickBot();
		}
		internal static void PlacePet(GameClientMessageHandler handler)
		{
			handler.PlacePet();
		}
		internal static void GetPetInfo(GameClientMessageHandler handler)
		{
			handler.GetPetInfo();
		}
		internal static void PickUpPet(GameClientMessageHandler handler)
		{
			handler.PickUpPet();
		}

		internal static void RespectPet(GameClientMessageHandler handler)
		{
			handler.RespectPet();
		}
		internal static void AddSaddle(GameClientMessageHandler handler)
		{
			handler.AddSaddle();
		}
		internal static void RemoveSaddle(GameClientMessageHandler handler)
		{
			handler.RemoveSaddle();
		}
		internal static void Ride(GameClientMessageHandler handler)
		{
			handler.MountOnPet();
		}
		internal static void Unride(GameClientMessageHandler handler)
		{
			handler.CancelMountOnPet();
		}
		internal static void SaveWired(GameClientMessageHandler handler)
		{
			handler.SaveWired();
		}
		internal static void SaveWiredCondition(GameClientMessageHandler handler)
		{
			handler.SaveWiredConditions();
		}
		internal static void GetMusicData(GameClientMessageHandler handler)
		{
			handler.GetMusicData();
		}
		internal static void AddPlaylistItem(GameClientMessageHandler handler)
		{
			handler.AddPlaylistItem();
		}
		internal static void RemovePlaylistItem(GameClientMessageHandler handler)
		{
			handler.RemovePlaylistItem();
		}
		internal static void GetDisks(GameClientMessageHandler handler)
		{
			handler.GetDisks();
		}
		internal static void GetPlaylists(GameClientMessageHandler handler)
		{
			handler.GetPlaylists();
		}
		internal static void GetUserInfo(GameClientMessageHandler handler)
		{
			handler.GetUserInfo();
		}
		internal static void LoadProfile(GameClientMessageHandler handler)
		{
			handler.LoadProfile();
		}
		internal static void GetBalance(GameClientMessageHandler handler)
		{
			handler.GetBalance();
		}
		internal static void GetSubscriptionData(GameClientMessageHandler handler)
		{
			handler.GetSubscriptionData();
		}
		internal static void GetBadges(GameClientMessageHandler handler)
		{
			handler.GetBadges();
		}
		internal static void UpdateBadges(GameClientMessageHandler handler)
		{
			handler.UpdateBadges();
		}
		internal static void GetAchievements(GameClientMessageHandler handler)
		{
			handler.GetAchievements();
		}
		internal static void ChangeLook(GameClientMessageHandler handler)
		{
			handler.ChangeLook();
		}
		internal static void ChangeMotto(GameClientMessageHandler handler)
		{
			handler.ChangeMotto();
		}
		internal static void GetWardrobe(GameClientMessageHandler handler)
		{
			handler.GetWardrobe();
		}
		internal static void AllowAllRide(GameClientMessageHandler handler)
		{
			handler.AllowAllRide();
		}
		internal static void SaveWardrobe(GameClientMessageHandler handler)
		{
			handler.SaveWardrobe();
		}
		internal static void GetPetsInventory(GameClientMessageHandler handler)
		{
			handler.GetPetsInventory();
		}
		internal static void OpenQuests(GameClientMessageHandler handler)
		{
			handler.OpenQuests();
		}
		internal static void StartQuest(GameClientMessageHandler handler)
		{
			handler.StartQuest();
		}
		internal static void StopQuest(GameClientMessageHandler handler)
		{
			handler.StopQuest();
		}
		internal static void GetCurrentQuest(GameClientMessageHandler handler)
		{
			handler.GetCurrentQuest();
		}
		internal static void GetGroupBadges(GameClientMessageHandler handler)
		{
			handler.InitRoomGroupBadges();
		}
		internal static void GetBotInv(GameClientMessageHandler handler)
		{
			handler.GetBotsInventory();
		}
		internal static void SaveRoomBg(GameClientMessageHandler handler)
		{
			handler.SaveRoomBg();
		}
		internal static void GoRoom(GameClientMessageHandler handler)
		{
			handler.GoRoom();
		}
		internal static void Sit(GameClientMessageHandler handler)
		{
			handler.Sit();
		}
		internal static void GetEventRooms(GameClientMessageHandler handler)
		{
			handler.GetEventRooms();
		}
		internal static void StartSeasonalQuest(GameClientMessageHandler handler)
		{
			handler.StartSeasonalQuest();
		}
		internal static void SaveMannequin(GameClientMessageHandler handler)
		{
			handler.SaveMannequin();
		}
		internal static void SaveMannequin2(GameClientMessageHandler handler)
		{
			handler.SaveMannequin2();
		}
		internal static void SerializeGroupPurchasePage(GameClientMessageHandler handler)
		{
			handler.SerializeGroupPurchasePage();
		}
		internal static void SerializeGroupPurchaseParts(GameClientMessageHandler handler)
		{
			handler.SerializeGroupPurchaseParts();
		}
		internal static void PurchaseGroup(GameClientMessageHandler handler)
		{
			handler.PurchaseGroup();
		}
		internal static void SerializeGroupInfo(GameClientMessageHandler handler)
		{
			handler.SerializeGroupInfo();
		}
		internal static void SerializeGroupMembers(GameClientMessageHandler handler)
		{
			handler.SerializeGroupMembers();
		}
		internal static void MakeGroupAdmin(GameClientMessageHandler handler)
		{
			handler.MakeGroupAdmin();
		}
		internal static void RemoveGroupAdmin(GameClientMessageHandler handler)
		{
			handler.RemoveGroupAdmin();
		}
		internal static void AcceptMembership(GameClientMessageHandler handler)
		{
			handler.AcceptMembership();
		}
		internal static void DeclineMembership(GameClientMessageHandler handler)
		{
			handler.DeclineMembership();
		}
		internal static void RemoveMember(GameClientMessageHandler handler)
		{
			handler.RemoveMember();
		}
		internal static void JoinGroup(GameClientMessageHandler handler)
		{
			handler.JoinGroup();
		}
		internal static void MakeFav(GameClientMessageHandler handler)
		{
			handler.MakeFav();
		}
		internal static void RemoveFav(GameClientMessageHandler handler)
		{
			handler.RemoveFav();
		}
        internal static void ReceiveNuxGifts(GameClientMessageHandler handler)
        {
            handler.ReceiveNuxGifts();
        }
        internal static void AcceptNuxGifts(GameClientMessageHandler handler)
        {
            handler.AcceptNuxGifts();
        }
        internal static void ReadForumThread(GameClientMessageHandler handler)
        {
            handler.ReadForumThread();
        }
        internal static void PublishForumThread(GameClientMessageHandler handler)
        {
            handler.PublishForumThread();
        }
        internal static void UpdateForumThread(GameClientMessageHandler handler)
        {
            handler.UpdateThreadState();
        }
        internal static void AlterForumThreadState(GameClientMessageHandler handler)
        {
            handler.AlterForumThreadState();
        }
        internal static void GetForumThreadRoot(GameClientMessageHandler handler)
        {
            handler.GetGroupForumThreadRoot();
        }
        internal static void GetGroupForumData(GameClientMessageHandler handler)
        {
            handler.GetGroupForumData();
        }
        internal static void GetGroupForums(GameClientMessageHandler handler)
        {
            handler.GetGroupForums();
        }
		internal static void ManageGroup(GameClientMessageHandler handler)
		{
			handler.ManageGroup();
		}
		internal static void UpdateGroupName(GameClientMessageHandler handler)
		{
			handler.UpdateGroupName();
		}
		internal static void UpdateGroupBadge(GameClientMessageHandler handler)
		{
			handler.UpdateGroupBadge();
		}
		internal static void UpdateGroupColours(GameClientMessageHandler handler)
		{
			handler.UpdateGroupColours();
		}
		internal static void UpdateGroupSettings(GameClientMessageHandler handler)
		{
			handler.UpdateGroupSettings();
		}
		internal static void SerializeGroupFurniPage(GameClientMessageHandler handler)
		{
			handler.SerializeGroupFurniPage();
		}
		internal static void EjectFurni(GameClientMessageHandler handler)
		{
			handler.EjectFurni();
		}
		internal static void MuteUser(GameClientMessageHandler handler)
		{
			handler.MuteUser();
		}
		internal static void CheckName(GameClientMessageHandler handler)
		{
			handler.CheckName();
		}
		internal static void ChangeName(GameClientMessageHandler handler)
		{
			handler.ChangeName();
		}
		internal static void GetTrainerPanel(GameClientMessageHandler handler)
		{
			handler.GetTrainerPanel();
		}
		internal static void UpdateEventInfo(GameClientMessageHandler handler)
		{
			handler.UpdateEventInfo();
		}
		internal static void GetRoomBannedUsers(GameClientMessageHandler handler)
		{
			handler.GetBannedUsers();
		}
		internal static void UsersWithRights(GameClientMessageHandler handler)
		{
			handler.UsersWithRights();
		}
		internal static void UnbanUser(GameClientMessageHandler handler)
		{
			handler.UnbanUser();
		}
		internal static void ManageBotActions(GameClientMessageHandler handler)
		{
			handler.ManageBotActions();
		}
		internal static void HandleBotSpeechList(GameClientMessageHandler handler)
		{
			handler.HandleBotSpeechList();
		}
		internal static void GetRelationships(GameClientMessageHandler handler)
		{
			handler.GetRelationships();
		}
		internal static void SetRelationship(GameClientMessageHandler handler)
		{
			handler.SetRelationship();
		}
		internal static void AutoRoom(GameClientMessageHandler handler)
		{
			handler.RoomOnLoad();
		}
		internal static void MuteAll(GameClientMessageHandler handler)
		{
			handler.MuteAll();
		}
		internal static void CompleteSafteyQuiz(GameClientMessageHandler handler)
		{
			handler.CompleteSafetyQuiz();
		}

        internal static void RemoveFavouriteRoom(GameClientMessageHandler handler)
        {
            handler.RemoveFavouriteRoom();
        }

        internal static void RoomUserAction(GameClientMessageHandler handler)
        {
            handler.RoomUserAction();
        }

        internal static void SaveFootballOutfit(GameClientMessageHandler handler)
        {
            handler.SaveFootballOutfit();
        }


        internal static void Whisper(GameClientMessageHandler handler)
        {
            handler.Whisper();
        }

        internal static void CatalogueIndex(GameClientMessageHandler handler)
        {
            handler.CatalogueIndex();
        }

        internal static void CataloguePage(GameClientMessageHandler handler)
        {
            handler.CataloguePage();
        }

        internal static void CatalogueClubPage(GameClientMessageHandler handler)
        {
            handler.CatalogueClubPage();
        }

        internal static void CatalogueOffersConfig(GameClientMessageHandler handler)
        {
            handler.CatalogueOfferConfig();
        }

        internal static void CatalogueSingleOffer(GameClientMessageHandler handler)
        {
            handler.CatalogueOffer();
        }

        internal static void CheckPetName(GameClientMessageHandler handler)
        {
            handler.CheckPetName();
        }

        internal static void PurchaseItem(GameClientMessageHandler handler)
        {
            handler.PurchaseItem();
        }

        internal static void PurchaseGift(GameClientMessageHandler handler)
        {
            handler.PurchaseGift();
        }

        internal static void GetPetBreeds(GameClientMessageHandler handler)
        {
            handler.GetPetBreeds();
        }

        internal static void ReloadEcotron(GameClientMessageHandler handler)
        {
            handler.ReloadEcotron();
        }

        internal static void GiftWrappingConfig(GameClientMessageHandler handler)
        {
            handler.GiftWrappingConfig();
        }

        internal static void RecyclerRewards(GameClientMessageHandler handler)
        {
            handler.GetRecyclerRewards();
        }

        internal static void RequestLeaveGroup(GameClientMessageHandler handler)
        {
            handler.RequestLeaveGroup();
        }
        internal static void ConfirmLeaveGroup(GameClientMessageHandler handler)
        {
            handler.ConfirmLeaveGroup();
        }
    }
}
