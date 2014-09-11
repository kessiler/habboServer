namespace Cyber.Messages.Headers
{
    internal static class Incoming
    {
        #region Handshake, Login & Misc.
        internal static int GetClientVersionMessageEvent = 4000;
        internal static int InitCryptoMessageEvent = 2478;//Finn
        internal static int GenerateSecretKeyMessageEvent = 901;//Finn
        internal static int UniqueIDMessageEvent = 2248;//Finn
        internal static int SSOTicketMessageEvent = 227;//Finn
        internal static int InfoRetrieveMessageEvent = 543;//Finn
        internal static int GetCurrencyBalanceMessageEvent = 1047;//Finn
        internal static int GetSubscriptionDataMessageEvent = 679;//Finn
        internal static int LandingLoadWidgetMessageEvent = 2402;//Finn
        internal static int LandingRefreshPromosMessageEvent = 1548;//Finn
        internal static int LandingRefreshRewardMessageEvent = 0x0D0D;//Finn
        internal static int OnlineConfirmationMessageEvent = 2695;//Finn
        internal static int RequestLatencyTestMessageEvent = 167;//Finn
        internal static int UserGetVolumeSettingsMessageEvent = 3168;//Finn
        internal static int SaveClientSettingsMessageEvent = 305;//Finn
        internal static int SendBadgeCampaignMessageEvent = 2174;//Finn
        internal static int OnDisconnectMessageEvent = 2990;//Finn
        internal static int GetTalentsTrackMessageEvent = 3369;//Finn
        internal static int PongMessageEvent = 348;//Finn
        internal static int RetrieveCitizenshipStatus = 1298;//Finn
        internal static int AcceptPollMessageEvent = 287;//Finn
        internal static int RefusePollMessageEvent = 3411;//Finn
        internal static int AnswerPollQuestionMessageEvent = 1283;//Finn
        internal static int NuxAcceptGiftsMessageEvent = 3612;//Finn
        internal static int NuxReceiveGiftsMessageEvent = 2621;//Finn
        internal static int LandingCommunityGoalMessageEvent = 3983;//Finn
        #endregion
        #region Navigator
        internal static int NavigatorGetEventsMessageEvent = 3883;//Finn
        internal static int NavigatorGetFavouriteRoomsMessageEvent = 1619;//Finn
        internal static int NavigatorGetFeaturedRoomsMessageEvent = 1961;//Finn
        internal static int NavigatorGetFriendsRoomsMessageEvent = 2845;//Finn
        internal static int NavigatorGetHighRatedRoomsMessageEvent = 3792;//Finn
        internal static int NavigatorGetMyRoomsMessageEvent = 138;//Finn
        internal static int NavigatorGetPopularRoomsMessageEvent = 3167;//Finn
        internal static int NavigatorGetPopularTagsMessageEvent = 1576;//Finn
        internal static int NavigatorGetRecentRoomsMessageEvent = 3420;//Finn
        internal static int NavigatorSearchRoomByNameMessageEvent = 3910;//Finn
        internal static int NavigatorGetRecommendedRoomsMessageEvent = 1027;//Finn
        internal static int NavigatorGetFlatCategoriesMessageEvent = 3417;//Finn
        internal static int CanCreateRoomMessageEvent = 2373;//Finn
        internal static int CreateRoomMessageEvent = 3669;//Finn
        internal static int AddFavouriteRoomMessageEvent = 3034;//Finn
        internal static int RemoveFavouriteRoomMessageEvent = 324;//Finn
        internal static int NavigatorGetPopularGroupsMessageEvent = 1558;//Finn
        internal static int NavigatorGetRoomsWithFriendsMessageEvent = 1079;//Finn
        internal static int SetHomeRoomMessageEvent = 1888;//Finn
        internal static int GoToHotelViewMessageEvent = 754;//Finn
        #endregion
        #region Catalogue
        internal static int GetCatalogIndexMessageEvent = 2911;//Finn
        internal static int GetCatalogPageMessageEvent = 1569;//Finn
        internal static int GetCatalogClubPageMessageEvent = 64;//Finn
        internal static int GetCatalogOfferMessageEvent = 2055;//Finn
        internal static int CatalogueOfferConfigMessageEvent = 978;//Finn
        internal static int PurchaseFromCatalogMessageEvent = 2134;//Finn
        internal static int PurchaseFromCatalogAsGiftMessageEvent = 3391;//Finn
        internal static int GetSellablePetBreedsMessageEvent = 840;//Finn
        internal static int ReloadRecyclerMessageEvent = 2013;//Finn
        internal static int GetGiftWrappingConfigurationMessageEvent = 2609;//Finn
        internal static int GetRecyclerRewardsMessageEvent = 94;//Finn
        internal static int CatalogPromotionGetRoomsMessageEvent = 3118;//Finn
        internal static int GetCatalogClubGiftsMessageEvent = 446;//Finn
        internal static int ChooseClubGiftMessageEvent = 2149;//Finn
        internal static int PromoteRoomMessageEvent = 60;//Finn
        internal static int RetrieveSongIDMessageEvent = 773;//Finn
        internal static int CheckPetnameMessageEvent = 2616;//Finn
        internal static int EcotronRecycleMessageEvent = 540;//Finn
        internal static int RedeemVoucherMessageEvent = 2517;//Finn
        #endregion
        #region Rooms
        internal static int EnterPrivateRoomMessageEvent = 2383;//Finn
        internal static int RoomGetHeightmapMessageEvent = 1319;//Finn
        internal static int RoomGetInfoMessageEvent = 902;//Finn
        internal static int RoomUserActionMessageEvent = 3301;//Finn
        internal static int RoomOnLoadMessageEvent = 1286;//Finn
        internal static int ChatMessageEvent = 769;//Finn
        internal static int ShoutMessageEvent = 2899;//Finn
        internal static int UserWhisperMessageEvent = 3099;//Finn
        internal static int UserWalkMessageEvent = 3194;//Finn
        internal static int UserDanceMessageEvent = 185;//Finn
        internal static int UserSignMessageEvent = 101;//Finn
        internal static int RoomBanUserMessageEvent = 986;//Finn
        internal static int RoomDeleteMessageEvent = 3489;//Finn
        internal static int RoomEventUpdateMessageEvent = 0x0A0A;//Finn
        internal static int RoomGetSettingsInfoMessageEvent = 2767;//Finn
        internal static int RoomKickUserMessageEvent = 649;//Finn
        internal static int RateRoomMessageEvent = 1444;//Finn
        internal static int RoomLoadByDoorbellMessageEvent = 2709;//Finn
        internal static int DoorbellAnswerMessageEvent = 3625;//Finn
        internal static int DropHanditemMessageEvent = 633;//Finn
        internal static int GiveHanditemMessageEvent = 735;//Finn
        internal static int GiveRespectMessageEvent = 123;//Finn
        internal static int GiveRightsMessageEvent = 3342;//Finn
        internal static int RoomRemoveAllRightsMessageEvent = 3576;//Finn
        internal static int RoomRemoveUserRightsMessageEvent = 541;//Finn
        internal static int RoomSaveSettingsMessageEvent = 1877;//Finn
        internal static int RoomSettingsMuteAllMessageEvent = 1809;//Finn
        internal static int RoomSettingsMuteUserMessageEvent = 2870;//Finn
        internal static int RoomUnbanUserMessageEvent = 3031;//Finn
        internal static int StartTypingMessageEvent = 2196;//Finn
        internal static int StopTypingMessageEvent = 1950;//Finn
        internal static int IgnoreUserMessageEvent = 691;//Finn
        internal static int UnignoreUserMessageEvent = 2249;//Finn
        internal static int RoomGetFilterMessageEvent = 3452;//Finn
        internal static int RoomAlterFilterMessageEvent = 1000;//Finn
        internal static int GetRoomBannedUsersMessageEvent = 2593;//Finn
        internal static int GetRoomRightsListMessageEvent = 261;//Finn
        internal static int ToggleSittingMessageEvent = 2576;//Finn
        internal static int LookAtUserMessageEvent = 44;//Finn
        internal static int BotActionsMessageEvent = 3475;//Finn
        internal static int BotSpeechListMessageEvent = 967;//Finn
        internal static int HorseAddSaddleMessageEvent = 3534;//Finn
        internal static int HorseAllowAllRideMessageEvent = 2555;//Finn
        internal static int HorseMountOnMessageEvent = 709;//Finn
        internal static int HorseRemoveSaddleMessageEvent = 3931;//Finn
        internal static int GetPetTrainerPanelMessageEvent = 2353;//Finn
        internal static int PetGetInformationMessageEvent = 2848;//Finn
        internal static int RespectPetMessageEvent = 3857;//Finn
        internal static int SaveFloorPlanEditorMessageEvent = 3392;//Finn
        internal static int GetFloorPlanFurnitureMessageEvent = 1662;//Finn
        internal static int GetFloorPlanDoorMessageEvent = 0x0B0B;//Finn
        #endregion
        #region Users
        internal static int LoadUserProfileMessageEvent = 3305;//Finn
        internal static int GetUserBadgesMessageEvent = 935;//Finn
        internal static int GetUserTagsMessageEvent = 3428;//Finn
        internal static int RelationshipsGetMessageEvent = 3943;//Finn
        internal static int SetRelationshipMessageEvent = 2703;//Finn
        internal static int UserUpdateLookMessageEvent = 3172;//Finn
        internal static int UserUpdateMottoMessageEvent = 1512;//Finn
        internal static int CheckUsernameMessageEvent = 208;//Finn
        internal static int ChangeUsernameMessageEvent = 2647;//Finn
        internal static int CompleteSafetyQuizMessageEvent = 588;//Finn
        internal static int WardrobeMessageEvent = 2889;//Finn
        internal static int WardrobeUpdateMessageEvent = 3134;//Finn
        #endregion
        #region Items, inventory, pets
        internal static int RoomAddFloorItemMessageEvent = 581;//Finn
        internal static int FloorItemMoveMessageEvent = 3813;//Finn
        internal static int TriggerDiceCloseMessageEvent = 2501;//Finn
        internal static int TriggerDiceRollMessageEvent = 2496;//Finn
        internal static int TriggerItemMessageEvent = 2926;//Finn
        internal static int TriggerMoodlightMessageEvent = 1826;//Finn
        internal static int TriggerWallItemMessageEvent = 0x0707;//Finn
        internal static int EnterOneWayDoorMessageEvent = 3390;//Finn
        internal static int UpdateMoodlightMessageEvent = 3892;//Finn
        internal static int OpenPostItMessageEvent = 2145;//Finn
        internal static int UseHabboWheelMessageEvent = 829;//Finn
        internal static int ActivateMoodlightMessageEvent = 134;//Finn
        internal static int RoomAddPostItMessageEvent = 1551;//Finn
        internal static int RoomApplySpaceMessageEvent = 3449;//Finn
        internal static int SaveFootballGateOutfitMessageEvent = 3928;//Finn
        internal static int LoadPetInventoryMessageEvent = 2304;//Finn
        internal static int LoadBotInventoryMessageEvent = 2529;//Finn
        internal static int LoadBadgeInventoryMessageEvent = 1125;//Finn
        internal static int LoadItemsInventoryMessageEvent = 591;//Finn
        internal static int PlaceBotMessageEvent = 2466;//Finn
        internal static int PlacePetMessageEvent = 664;//Finn
        internal static int PickUpBotMessageEvent = 925;//Finn
        internal static int PickUpItemMessageEvent = 3282;//Finn
        internal static int PickUpPetMessageEvent = 3377;//Finn
        internal static int SetActivatedBadgesMessageEvent = 1379;//Finn
        internal static int WiredSaveConditionMessageEvent = 2589;//Finn
        internal static int WiredSaveEffectMessageEvent = 2194;//Finn
        internal static int WiredSaveMatchingMessageEvent = 3871;//Finn
        internal static int WiredSaveTriggerMessageEvent = 2018;//Finn
        internal static int YouTubeChoosePlaylistVideoMessageEvent = 3241;//Finn
        internal static int YouTubeGetPlayerMessageEvent = 2418;//Finn
        internal static int YouTubeGetPlaylistGetMessageEvent = 3863;//Finn
        internal static int TileStackMagicSetHeightMessageEvent = 3261;//Finn
        internal static int WallItemMoveMessageEvent = 1289;//Finn
        internal static int SavePostItMessageEvent = 3113;//Finn
        internal static int SaveRoomBackgroundTonerMessageEvent = 2005;//Finn
        internal static int SaveRoomBrandingMessageEvent = 1565;//Finn
        internal static int MannequinSaveDataMessageEvent = 1157;//Finn
        internal static int MannequinUpdateDataMessageEvent = 241;//Finn
        internal static int LoadJukeboxDiscsMessageEvent = 3482;//Finn
        internal static int JukeboxAddPlaylistItemMessageEvent = 2074;//Finn
        internal static int JukeboxRemoveSongMessageEvent = 1389;//Finn
        internal static int GetJukeboxPlaylistsMessageEvent = 965;//Finn
        internal static int GetMusicDataMessageEvent = 3338;//Finn
        internal static int OpenGiftMessageEvent = 477;//Finn
        internal static int ReedemExchangeItemMessageEvent = 1904;//Finn
        internal static int RemovePostItMessageEvent = 2028;//Finn
        internal static int EnableInventoryEffectMessageEvent = 1061;//Finn
        internal static int EffectEnableMessageEvent = 1075;//Finn
        #endregion
        #region Quests & Achievements
        internal static int OpenQuestsMessageEvent = 1422;//Finn
        internal static int LoadNextQuestMessageEvent = 3999;//Finn
        internal static int QuestCancelMessageEvent = 3037;//Finn
        internal static int QuestSeasonalStartMessageEvent = 3904;//Finn
        internal static int QuestStartMessageEvent = 3092;//Finn
        internal static int OpenAchievementsBoxMessageEvent = 2011;//Finn
        #endregion
        #region Moderation Tool
        internal static int ModerationToolUserToolMessageEvent = 19;//Finn
        internal static int ModerationToolRoomChatlogMessageEvent = 3667;//Finn
        internal static int ModerationToolRoomToolMessageEvent = 1226;//Finn
        internal static int ModerationBanUserMessageEvent = 3722;//Finn
        internal static int ModerationKickUserMessageEvent = 412;//Finn
        internal static int ModerationMuteUserMessageEvent = 3930;//Finn
        internal static int ModerationLockTradeMessageEvent = 2839;//Finn
        internal static int ModerationToolCloseIssueMessageEvent = 1313;//Finn
        internal static int ModerationToolGetRoomVisitsMessageEvent = 2688;//Finn
        internal static int ModerationToolPerformRoomActionMessageEvent = 1772;//Finn
        internal static int ModerationToolPickIssueMessageEvent = 3756;//Finn
        internal static int ModerationToolReleaseIssueMessageEvent = 1091;//Finn
        internal static int ModerationToolSendRoomAlertMessageEvent = 1941;//Finn
        internal static int ModerationToolSendUserAlertMessageEvent = 1779;//Finn
        internal static int ModerationToolSendUserCautionMessageEvent = 2321;//Finn
        internal static int ModerationToolUserChatlogMessageEvent = 2615;//Finn
        internal static int SubmitHelpTicketMessageEvent = 1715;//Finn
        internal static int OpenBullyReportingMessageEvent = 194;//Finn
        internal static int SendBullyReportMessageEvent = 3028;//Finn
        internal static int OpenHelpToolMessageEvent = 2618;//Finn
        #endregion
        #region Console
        internal static int AcceptFriendMessageEvent = 7;//Finn
        internal static int ConsoleInstantChatMessageEvent = 2197;//Finn
        internal static int ConsoleInviteFriendsMessageEvent = 3686;//Finn
        internal static int ConsoleSearchFriendsMessageEvent = 0;//Finn
        internal static int FollowFriendMessageEvent = 2854;//Finn
        internal static int FriendListUpdateMessageEvent = 1934;//Finn
        internal static int DeclineFriendMessageEvent = 1699;//Finn
        internal static int DeleteFriendMessageEvent = 391;//Finn   
        internal static int RequestFriendMessageEvent = 2233;//Finn
        internal static int GetMyFriendsMessageEvent = 812;//Finn
        #endregion
        #region Trade
        internal static int TradeAcceptMessageEvent = 3681;//Finn
        internal static int TradeAddItemOfferMessageEvent = 2752;//Finn
        internal static int TradeCancelMessageEvent = 3527;//Finn
        internal static int TradeConfirmMessageEvent = 833;//Finn
        internal static int TradeDiscardMessageEvent = 1932;//Finn
        internal static int TradeRemoveItemMessageEvent = 1560;//Finn
        internal static int TradeStartMessageEvent = 509;//Finn
        internal static int TradeUnacceptMessageEvent = 3484;//Finn
        #endregion
        #region Guilds
        internal static int GetGroupForumsMessageEvent = 2274;//Finn
        internal static int GetGroupForumDataMessageEvent = 1630;//Finn
        internal static int GetGroupForumThreadRootMessageEvent = 1093;//Finn
        internal static int UpdateThreadMessageEvent = 2290;//Finn
        internal static int AlterForumThreadStateMessageEvent = 3421;//Finn
        internal static int PublishForumThreadMessageEvent = 1423;//Finn
        internal static int ReadForumThreadMessageEvent = 490;//Finn
        internal static int RequestLeaveGroupMessageEvent = 502;//Finn
        internal static int ConfirmLeaveGroupMessageEvent = 400;//Finn
        internal static int AcceptGroupRequestMessageEvent = 211;//Finn
        internal static int CreateGuildMessageEvent = 1287;//Finn
        internal static int GetGroupFurnitureMessageEvent = 3467;//Finn
        internal static int GetGroupInfoMessageEvent = 3908;//Finn
        internal static int GetGroupMembersMessageEvent = 247;//Finn
        internal static int GetGroupPurchaseBoxMessageEvent = 246;//Finn
        internal static int GetGroupPurchasingInfoMessageEvent = 1640;//Finn
        internal static int GroupDeclineMembershipRequestMessageEvent = 181;//Finn
        internal static int GroupMakeAdministratorMessageEvent = 3453;//Finn
        internal static int GroupManageMessageEvent = 610;//Finn
        internal static int GroupUpdateBadgeMessageEvent = 3334;//Finn
        internal static int GroupUpdateColoursMessageEvent = 1873;//Finn
        internal static int GroupUpdateNameMessageEvent = 646;//Finn
        internal static int GroupUpdateSettingsMessageEvent = 3548;//Finn
        internal static int GroupUserJoinMessageEvent = 1420;//Finn
        internal static int SetFavoriteGroupMessageEvent = 1224;//Finn
        internal static int RemoveFavouriteGroupMessageEvent = 85;//Finn
        internal static int RemoveGroupAdminMessageEvent = 188;//Finn
        #endregion
  
      

      
    }
}
