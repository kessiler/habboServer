using Database_Manager.Database.Session_Details.Interfaces;
using Cyber.Core;
using Cyber.HabboHotel.GameClients;
using Cyber.HabboHotel.Groups;
using Cyber.HabboHotel.Items;
using Cyber.HabboHotel.Pets;
using Cyber.HabboHotel.Quests;
using Cyber.HabboHotel.RoomBots;
using Cyber.HabboHotel.SoundMachine;
using Cyber.HabboHotel.Users.Inventory;
using Cyber.Util;
using Cyber.Messages;
using Cyber.Messages.Headers;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Linq;

namespace Cyber.HabboHotel.Catalogs
{
    internal class Catalog
    {
        internal HybridDictionary Categories;
        internal HybridDictionary Offers;
        internal Dictionary<int, uint> FlatOffers;
        internal List<CatalogItem> HabboClubItems;

        internal Dictionary<uint, ServerMessage> CachedIndexes;
        internal List<EcotronReward> EcotronRewards;
        internal List<int> EcotronLevels;
        internal static int LastSentOffer;

        internal CatalogItem GetItemFromOffer(int offerId)
        {
            CatalogItem Result = null;
            if (this.FlatOffers.ContainsKey(offerId))
            {
                Result =  (CatalogItem)this.Offers[FlatOffers[offerId]];
            }

            if (Result == null)
            {
                Result = CyberEnvironment.GetGame().GetCatalog().GetItem(Convert.ToUInt32(offerId));
            }

            return Result;
        }

        internal void Initialize(IQueryAdapter dbClient, out uint pageLoaded)
        {
            this.Initialize(dbClient);
            pageLoaded = (uint)Categories.Count;
        }

        internal void Initialize(IQueryAdapter dbClient)
        {
            this.Categories = new HybridDictionary();
            this.Offers = new HybridDictionary();
            this.FlatOffers = new Dictionary<int, uint>();
            this.EcotronRewards = new List<EcotronReward>();
            this.EcotronLevels = new List<int>();
            this.HabboClubItems = new List<CatalogItem>();

            dbClient.setQuery("SELECT * FROM catalog_pages ORDER BY order_num");
            DataTable table = dbClient.getTable();
            dbClient.setQuery("SELECT * FROM ecotron_rewards ORDER BY reward_level ASC");
            DataTable table2 = dbClient.getTable();
            dbClient.setQuery("SELECT * FROM catalog_items");
            DataTable table3 = dbClient.getTable();
            dbClient.setQuery("SELECT * FROM  `catalog_items` WHERE  `catalog_name` LIKE  '%HABBO_CLUB_VIP%'");
            DataTable table4 = dbClient.getTable();

            if (table3 != null)
            {
                foreach (DataRow dataRow in table3.Rows)
                {
                    if (!string.IsNullOrEmpty(dataRow["item_ids"].ToString()) && !string.IsNullOrEmpty(dataRow["amounts"].ToString()))
                    {
                        int value = Convert.ToInt32(dataRow["page_id"]);
                        string source = dataRow["item_ids"].ToString();
                        dataRow["catalog_name"].ToString();
                        uint id = uint.Parse(dataRow["item_ids"].ToString().Split(';')[0]);
                        Item item = CyberEnvironment.GetGame().GetItemManager().GetItem(id);

                        if (item != null)
                        {
                            int num;
                            if (!source.Contains(';'))
                            {
                                num = item.FlatId;
                            }
                            else
                            {
                                num = -1;
                            }

                            CatalogItem value2 = new CatalogItem(dataRow);
                            if (value2.GetFirstBaseItem() == null)
                                continue;

                            this.Offers.Add(value2.Id, value2);

                            if (num != -1 && !this.FlatOffers.ContainsKey(num))
                            {
                                this.FlatOffers.Add(num, value2.Id);
                            }
                        }
                    }
                }
            }
            if (table != null)
            {
                foreach (DataRow dataRow2 in table.Rows)
                {
                    bool visible = false;
                    bool enabled = false;
                    bool comingSoon = false;
                    if (dataRow2["visible"].ToString() == "1")
                    {
                        visible = true;
                    }
                    if (dataRow2["enabled"].ToString() == "1")
                    {
                        enabled = true;
                    }
                    this.Categories.Add((int)dataRow2["id"], new CatalogPage((int)dataRow2["id"], (int)dataRow2["parent_id"], (string)dataRow2["code_name"], (string)dataRow2["caption"], visible, enabled, comingSoon, Convert.ToUInt32(dataRow2["min_rank"]), (int)dataRow2["icon_image"], (string)dataRow2["page_layout"], (string)dataRow2["page_headline"], (string)dataRow2["page_teaser"], (string)dataRow2["page_special"], (string)dataRow2["page_text1"], (string)dataRow2["page_text2"], (string)dataRow2["page_text_details"], (string)dataRow2["page_text_teaser"], (string)dataRow2["page_link_description"], (string)dataRow2["page_link_pagename"], (int)dataRow2["order_num"], ref Offers));
                }
            }
            if (table2 != null)
            {
                foreach (DataRow dataRow3 in table2.Rows)
                {
                    this.EcotronRewards.Add(new EcotronReward(Convert.ToUInt32(dataRow3["display_id"]), Convert.ToUInt32(dataRow3["item_id"]), Convert.ToUInt32(dataRow3["reward_level"])));
                    if (!this.EcotronLevels.Contains((int)Convert.ToInt16(dataRow3["reward_level"])))
                    {
                        this.EcotronLevels.Add((int)Convert.ToInt16(dataRow3["reward_level"]));
                    }
                }
            }
            if (table4 != null)
            {
                foreach (DataRow row in table4.Rows)
                {
                    this.HabboClubItems.Add(new CatalogItem(row));
                }
            }

            CachedIndexes = new Dictionary<uint, ServerMessage>();
            for (uint i = 1; i < 9; i++)
            {
                CachedIndexes.Add(i, CatalogPacket.ComposeIndex(i));
            }

        }

        internal CatalogItem GetItem(uint ItemId)
        {
            if (this.Offers.Contains(ItemId))
            {
                return (CatalogItem)Offers[ItemId];
            }
            return null;
        }

        internal CatalogPage GetPage(int Page)
        {
            if (!this.Categories.Contains(Page))
            {
                return null;
            }
            return (CatalogPage)this.Categories[Page];
        }

        internal void HandlePurchase(GameClient Session, int PageId, int ItemId, string ExtraData, int priceAmount, bool IsGift, string GiftUser, string GiftMessage, int GiftSpriteId, int GiftLazo, int GiftColor, bool undef, uint Group)
        {
            if (priceAmount < 1 || priceAmount > 100)
            {
                priceAmount = 1;
            }
            int num = priceAmount;
            int num2 = 0;
            int limtot = 0;
            CatalogPage catalogPage;
            CatalogItem item;
            uint num3;
            checked
            {
                if (priceAmount >= 6)
                {
                    num--;
                }
                if (priceAmount >= 12)
                {
                    num -= 2;
                }
                if (priceAmount >= 18)
                {
                    num -= 2;
                }
                if (priceAmount >= 24)
                {
                    num -= 2;
                }
                if (priceAmount >= 30)
                {
                    num -= 2;
                }
                if (priceAmount >= 36)
                {
                    num -= 2;
                }
                if (priceAmount >= 42)
                {
                    num -= 2;
                }
                if (priceAmount >= 48)
                {
                    num -= 2;
                }
                if (priceAmount >= 54)
                {
                    num -= 2;
                }
                if (priceAmount >= 60)
                {
                    num -= 2;
                }
                if (priceAmount >= 66)
                {
                    num -= 2;
                }
                if (priceAmount >= 72)
                {
                    num -= 2;
                }
                if (priceAmount >= 78)
                {
                    num -= 2;
                }
                if (priceAmount >= 84)
                {
                    num -= 2;
                }
                if (priceAmount >= 90)
                {
                    num -= 2;
                }
                if (priceAmount >= 96)
                {
                    num -= 2;
                }
                if (priceAmount >= 99)
                {
                    num -= 2;
                }
                if (!this.Categories.Contains(PageId) && PageId != -12345678)
                {
                    return;
                }

                if (PageId == -12345678)
                {
                    item = GetItemFromOffer(ItemId);

                    if (item == null)
                    {
                        return;
                    }
                }
                else
                {
                    catalogPage = (CatalogPage)Categories[PageId];
                    if (catalogPage == null || !catalogPage.Enabled || !catalogPage.Visible || Session == null || Session.GetHabbo() == null)
                    {
                        return;
                    }
                    if (catalogPage.MinRank > Session.GetHabbo().Rank || catalogPage.Layout == "sold_ltd_items")
                    {
                        return;
                    }

                    item = catalogPage.GetItem(ItemId);
                    if (item == null)
                    {
                        return;
                    }

                    if (catalogPage.Layout == "vip_buy" || catalogPage.Layout == "club_buy" || HabboClubItems.Contains(item))
                    {
                        string[] array = item.Name.Split(new char[]
				{
					'_'
				});
                        double dayLength;
                        if (item.Name.Contains("DAY"))
                        {
                            dayLength = double.Parse(array[3]);
                        }
                        else
                        {
                            if (item.Name.Contains("MONTH"))
                            {
                                double num4 = double.Parse(array[3]);
                                dayLength = Math.Ceiling((num4 * 31) - 0.205);
                            }
                            else
                            {
                                if (item.Name.Contains("YEAR"))
                                {
                                    double num5 = double.Parse(array[3]);
                                    dayLength = (num5 * 31 * 12);
                                }
                                else
                                {
                                    dayLength = 31;
                                }
                            }
                        }
                        Session.GetHabbo().GetSubscriptionManager().AddSubscription(dayLength);
                        return;
                    }
                }

                if (item.Name == "room_ad_plus_badge")
                {
                    return;
                }

                if (item.ClubOnly && !Session.GetHabbo().GetSubscriptionManager().HasSubscription)
                {
                    ServerMessage serverMessage = new ServerMessage(Outgoing.CatalogPurchaseNotAllowedMessageComposer);
                    serverMessage.AppendInt32(1);
                    Session.SendMessage(serverMessage);
                    return;
                }
                else if (item.Name.Contains("guild_forum"))
                {
                    uint GroupId;
                    if (!uint.TryParse(ExtraData, out GroupId))
                    {
                        Session.SendNotif("Your group forum couldn't be created by an unknown error. Please report it.");
                        return;
                    }
                    else
                    {
                        Guild Grap = CyberEnvironment.GetGame().GetGroupManager().GetGroup(GroupId);
                        if (Grap == null)
                        {
                            Session.SendNotif("Your group forum couldn't be created by an unknown error. Please report it");
                            return;
                        }
                        else if (!Grap.HasForum && Grap.CreatorId == Session.GetHabbo().Id)
                        {
                            Grap.HasForum = true;
                            CyberEnvironment.GetGame().GetClientManager().SendSuperNotif("Congratulations!", "You successfully purchased a Forum for your group.", "admin", Session, "event:groupforum/" + Grap.Id, "Enter my new Group Forum", false, false);
                            Grap.UpdateForum();
                        }
                        else if (Grap.CreatorId != Session.GetHabbo().Id && !Grap.HasForum)
                        {
                            Session.SendNotif("Uhm, looks like you're not the owner of the group. Anyway, you received a Group Forum Terminal, which would work only when the owner of the group buys a forum.");
                        }
                    }
                }
                bool flag = false;
                foreach (uint current in item.Items.Keys)
                {
                    if (item.GetBaseItem(current).InteractionType == InteractionType.pet1 || item.GetBaseItem(current).InteractionType == InteractionType.pet2 || item.GetBaseItem(current).InteractionType == InteractionType.pet3 || item.GetBaseItem(current).InteractionType == InteractionType.pet4 || item.GetBaseItem(current).InteractionType == InteractionType.pet5 || item.GetBaseItem(current).InteractionType == InteractionType.pet6 || item.GetBaseItem(current).InteractionType == InteractionType.pet7 || item.GetBaseItem(current).InteractionType == InteractionType.pet8 || item.GetBaseItem(current).InteractionType == InteractionType.pet9 || item.GetBaseItem(current).InteractionType == InteractionType.pet10 || item.GetBaseItem(current).InteractionType == InteractionType.pet11 || item.GetBaseItem(current).InteractionType == InteractionType.pet12 || item.GetBaseItem(current).InteractionType == InteractionType.pet13 || item.GetBaseItem(current).InteractionType == InteractionType.pet14 || item.GetBaseItem(current).InteractionType == InteractionType.pet15 || item.GetBaseItem(current).InteractionType == InteractionType.pet16 || item.GetBaseItem(current).InteractionType == InteractionType.pet17 || item.GetBaseItem(current).InteractionType == InteractionType.pet18 || item.GetBaseItem(current).InteractionType == InteractionType.pet19 || item.GetBaseItem(current).InteractionType == InteractionType.pet20 || item.GetBaseItem(current).InteractionType == InteractionType.pet21 || item.GetBaseItem(current).InteractionType == InteractionType.pet22 || item.GetBaseItem(current).InteractionType == InteractionType.pet23 || item.GetBaseItem(current).InteractionType == InteractionType.pet24 || item.GetBaseItem(current).InteractionType == InteractionType.pet25 || item.GetBaseItem(current).InteractionType == InteractionType.pet26)
                    {
                        flag = true;
                    }
                }
                if (!flag && (item.CreditsCost * num < 0 || item.DucketsCost * num < 0 || item.BelCreditsCost * num < 0 || item.LoyaltyCost * num < 0))
                {
                    return;
                }
                if (item.IsLimited)
                {
                    num = 1;
                    priceAmount = 1;
                    if (item.LimitedSelled >= item.LimitedStack)
                    {
                        Session.SendMessage(new ServerMessage(Outgoing.CatalogLimitedItemSoldOutMessageComposer));
                        return;
                    }
                    item.LimitedSelled++;
                    using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
                    {
                        queryreactor.runFastQuery(string.Concat(new object[]
						{
							"UPDATE catalog_items SET limited_sells = ",
							item.LimitedSelled,
							" WHERE id = ",
							item.Id
						}));
                        num2 = item.LimitedSelled;
                        limtot = item.LimitedStack;
                    }
                }
                else
                {
                    if (IsGift & priceAmount > 1)
                    {
                        num = 1;
                        priceAmount = 1;
                    }
                }
                num3 = 0u;
                if (Session.GetHabbo().Credits < item.CreditsCost * num)
                {
                    return;
                }
                if (Session.GetHabbo().ActivityPoints < item.DucketsCost * num)
                {
                    return;
                }
                if (Session.GetHabbo().BelCredits < item.BelCreditsCost * num)
                {
                    return;
                }
                if (Session.GetHabbo().BelCredits < item.LoyaltyCost * num)
                {
                    return;
                }
                if (item.CreditsCost > 0 && !IsGift)
                {
                    Session.GetHabbo().Credits -= item.CreditsCost * num;
                    Session.GetHabbo().UpdateCreditsBalance();
                }
                if (item.DucketsCost > 0 && !IsGift)
                {
                    Session.GetHabbo().ActivityPoints -= item.DucketsCost * num;
                    Session.GetHabbo().UpdateActivityPointsBalance();
                }
                if (item.BelCreditsCost > 0 && !IsGift)
                {
                    Session.GetHabbo().BelCredits -= item.BelCreditsCost * num;
                    Session.GetHabbo().UpdateSeasonalCurrencyBalance();
                }
                if (item.LoyaltyCost > 0 && !IsGift)
                {
                    Session.GetHabbo().BelCredits -= item.LoyaltyCost * num;
                    Session.GetHabbo().UpdateSeasonalCurrencyBalance();
                }
            }

            

            checked
            {
                foreach (uint current2 in item.Items.Keys)
                {
                    if (IsGift)
                    {
                        if ((DateTime.Now - Session.GetHabbo().LastGiftPurchaseTime).TotalSeconds <= 15.0)
                        {
                            Session.SendNotif("You're purchasing gifts too fast! Please wait 15 seconds, then you purchase another gift.");
                            return;
                        }

                        if (!item.GetBaseItem(current2).AllowGift)
                        {
                            return;
                        }
                        DataRow row;
                        using (IQueryAdapter queryreactor3 = CyberEnvironment.GetDatabaseManager().getQueryReactor())
                        {
                            queryreactor3.setQuery("SELECT id FROM users WHERE username = @gift_user");
                            queryreactor3.addParameter("gift_user", GiftUser);
                            row = queryreactor3.getRow();
                        }
                        if (row == null)
                        {
                            Session.GetMessageHandler().GetResponse().Init(Outgoing.GiftErrorMessageComposer);
                            Session.GetMessageHandler().GetResponse().AppendString(GiftUser);
                            Session.GetMessageHandler().SendResponse();
                            return;
                        }
                        num3 = Convert.ToUInt32(row["id"]);
                        if (num3 == 0u)
                        {
                            Session.GetMessageHandler().GetResponse().Init(Outgoing.GiftErrorMessageComposer);
                            Session.GetMessageHandler().GetResponse().AppendString(GiftUser);
                            Session.GetMessageHandler().SendResponse();
                            return;
                        }
                        if (item.CreditsCost > 0 && IsGift)
                        {
                            Session.GetHabbo().Credits -= item.CreditsCost * num;
                            Session.GetHabbo().UpdateCreditsBalance();
                        }
                        if (item.DucketsCost > 0 && IsGift)
                        {
                            Session.GetHabbo().ActivityPoints -= item.DucketsCost * num;
                            Session.GetHabbo().UpdateActivityPointsBalance();
                        }
                        if (item.BelCreditsCost > 0 && IsGift)
                        {
                            Session.GetHabbo().BelCredits -= item.BelCreditsCost * num;
                            Session.GetHabbo().UpdateSeasonalCurrencyBalance();
                        }
                        if (item.LoyaltyCost > 0 && IsGift)
                        {
                            Session.GetHabbo().BelCredits -= item.LoyaltyCost * num;
                            Session.GetHabbo().UpdateSeasonalCurrencyBalance();
                        }
                    }
                    if (IsGift && item.GetBaseItem(current2).Type == 'e')
                    {
                        Session.SendNotif("You can't send effects as gifts.");
                        return;
                    }
                    string text = "";
                    InteractionType interactionType = item.GetBaseItem(current2).InteractionType;
                    switch (interactionType)
                    {
                        case InteractionType.none:
                            ExtraData = "";
                            break;
                        case InteractionType.gate:
                        case InteractionType.bed:
                        case InteractionType.scoreboard:
                        case InteractionType.vendingmachine:
                        case InteractionType.alert:
                        case InteractionType.onewaygate:
                        case InteractionType.loveshuffler:
                        case InteractionType.habbowheel:
                        case InteractionType.dice:
                        case InteractionType.bottle:
                        case InteractionType.hopper:
                        case InteractionType.teleport:
                        case InteractionType.pet:
                        case InteractionType.pool:
                        case InteractionType.roller:
                        case InteractionType.fbgate:
                            goto IL_DF5;
                        case InteractionType.postit:
                            ExtraData = "FFFF33";
                            break;
                        case InteractionType.roomeffect:
                            {
                                double num6 = 0.0;
                                try
                                {
                                    if (string.IsNullOrEmpty(ExtraData))
                                    {
                                        num6 = 0.0;
                                    }
                                    else
                                    {
                                        num6 = double.Parse(ExtraData, CyberEnvironment.cultureInfo);
                                    }
                                }
                                catch (Exception pException)
                                {
                                    Logging.HandleException(pException, "Catalog.HandlePurchase: " + ExtraData);
                                }
                                ExtraData = TextHandling.GetString(num6);
                                break;
                            }
                        case InteractionType.dimmer:
                            ExtraData = "1,1,1,#000000,255";
                            break;
                        case InteractionType.trophy:
                            ExtraData = string.Concat(new object[]
						{
							Session.GetHabbo().Username,
							Convert.ToChar(9),
							DateTime.Now.Day,
							"-",
							DateTime.Now.Month,
							"-",
							DateTime.Now.Year,
							Convert.ToChar(9),
							ExtraData
						});
                            break;
                        case InteractionType.rentals:
                            goto IL_C41;
                        case InteractionType.pet0:
                        case InteractionType.pet1:
                        case InteractionType.pet2:
                        case InteractionType.pet3:
                        case InteractionType.pet4:
                        case InteractionType.pet5:
                        case InteractionType.pet6:
                        case InteractionType.pet7:
                        case InteractionType.pet8:
                        case InteractionType.pet9:
                        case InteractionType.pet10:
                        case InteractionType.pet11:
                        case InteractionType.pet12:
                        case InteractionType.pet13:
                        case InteractionType.pet14:
                        case InteractionType.pet15:
                        case InteractionType.pet16:
                        case InteractionType.pet17:
                        case InteractionType.pet18:
                        case InteractionType.pet19:
                        case InteractionType.pet20:
                        case InteractionType.pet21:
                        case InteractionType.pet22:
                        case InteractionType.pet23:
                        case InteractionType.pet24:
                        case InteractionType.pet25:
                        case InteractionType.pet26:
                            try
                            {
                                string[] array2 = ExtraData.Split(new char[]
							{
								'\n'
							});
                                string petName = array2[0];
                                string text2 = array2[1];
                                string text3 = array2[2];
                                int.Parse(text2);
                                if (!Catalog.CheckPetName(petName))
                                {
                                    return;
                                }
                                if (text2.Length != 1)
                                {
                                    return;
                                }
                                if (text3.Length != 6)
                                {
                                    return;
                                }
                                CyberEnvironment.GetGame().GetAchievementManager().ProgressUserAchievement(Session, "ACH_PetLover", 1, false);
                                break;
                            }
                            catch (Exception ex)
                            {
                                Logging.WriteLine(ex.ToString(), ConsoleColor.Gray);
                                Logging.HandleException(ex, "Catalog.HandlePurchase");
                                return;
                            }
                        default:
                            switch (interactionType)
                            {
                                case InteractionType.mannequin:
                                    ExtraData = string.Concat(new object[]
								{
									"m",
									Convert.ToChar(5),
									"ch-215-92.lg-3202-1322-73",
									Convert.ToChar(5),
									"Mannequin"
								});
                                    break;
                                case InteractionType.vip_gate:
                                case InteractionType.mystery_box:
                                case InteractionType.youtubetv:
                                case InteractionType.tilestackmagic:
                                case InteractionType.tent:
                                case InteractionType.bedtent:
                                    goto IL_DF5;
                                case InteractionType.badge_display:
                                    if (!Session.GetHabbo().GetBadgeComponent().HasBadge(ExtraData))
                                        ExtraData = "UMAD";
                                    break;
                                case InteractionType.fbgate:
                                    ExtraData = "hd-99999-99999.lg-270-62;hd-99999-99999.ch-630-62.lg-695-62";
                                    break;


                                case InteractionType.pinata:
                                case InteractionType.runwaysage:
                                case InteractionType.shower:
                                    ExtraData = "0";
                                    break;
                                case InteractionType.groupforumterminal:
                                case InteractionType.gld_item:
                                case InteractionType.gld_gate:
                                case InteractionType.poster:
                                    break;
                                case InteractionType.moplaseed:
                                    ExtraData = new Random().Next(0, 12).ToString();
                                    break;
                                case InteractionType.musicdisc:
                                    text = SongManager.GetCodeById((uint)item.songID);
                                    SongData song = SongManager.GetSongById((uint)item.songID);
                                    ExtraData = string.Concat(new object[]
							    {
								    Session.GetHabbo().Username,
								    '\n',
								    DateTime.Now.Year,
								    '\n',
								    DateTime.Now.Month,
								    '\n',
								    DateTime.Now.Day,
								    '\n',
								    song.LengthSeconds,
								    '\n',
								    song.Name
							    });
                                    break;
                                default:
                                    goto IL_DF5;
                            }
                            break;
                    }
                IL_DFC:
                    Session.GetMessageHandler().GetResponse().Init(Outgoing.UpdateInventoryMessageComposer);
                    Session.GetMessageHandler().SendResponse();
                    Session.SendMessage(CatalogPacket.PurchaseOK());
                    if (IsGift)
                    {
                        Item itemBySprite = CyberEnvironment.GetGame().GetItemManager().GetItemBySprite(GiftSpriteId, 's');

                        if (itemBySprite == null)
                        {
                            return;
                        }

                        GameClient clientByUserID = CyberEnvironment.GetGame().GetClientManager().GetClientByUserID(num3);
                        uint num7;
                        using (IQueryAdapter queryreactor4 = CyberEnvironment.GetDatabaseManager().getQueryReactor())
                        {
                            queryreactor4.setQuery(string.Concat(new object[]
							{
								"INSERT INTO items (base_item,user_id) VALUES (",
								itemBySprite.ItemId,
								", ",
								num3,
								")"
							}));
                            num7 = (uint)queryreactor4.insertQuery();
                            queryreactor4.setQuery(string.Concat(new object[]
							{
								"INSERT INTO user_gifts (gift_id,item_id,extradata,giver_name,message,ribbon,color,gift_sprite,show_sender,rare_id) VALUES (",
								num7,
								", ",
								item.GetBaseItem(current2).ItemId,
								",@extradata, @name, @message,",
								GiftLazo,
								",",
								GiftColor,
								",",
								GiftSpriteId,
								",",
								undef ? 1 : 0,
								",",
								num2,
								")"
							}));
                            queryreactor4.addParameter("extradata", ExtraData);
                            queryreactor4.addParameter("name", GiftUser);
                            queryreactor4.addParameter("message", GiftMessage);
                            queryreactor4.runQuery();
                            if (Session.GetHabbo().Id != num3)
                            {
                                queryreactor4.runFastQuery(string.Concat(new object[]
								{
									"UPDATE user_stats SET gifts_given = gifts_given + 1 WHERE id = ",
									Session.GetHabbo().Id,
									" LIMIT 1;UPDATE user_stats SET gifts_received = gifts_received + 1 WHERE id = ",
									num3,
									" LIMIT 1"
								}));
                            }
                        }
                        if (clientByUserID.GetHabbo().Id != Session.GetHabbo().Id)
                        {
                            CyberEnvironment.GetGame().GetAchievementManager().ProgressUserAchievement(Session, "ACH_GiftGiver", 1, false);
                            CyberEnvironment.GetGame().GetQuestManager().ProgressUserQuest(Session, QuestType.GIFT_OTHERS, 0u);
                        }
                        if (clientByUserID != null)
                        {
                            UserItem userItem = clientByUserID.GetHabbo().GetInventoryComponent().AddNewItem(num7, itemBySprite.ItemId, string.Concat(new object[]
							{
								Session.GetHabbo().Id,
								(char)9,
								GiftMessage,
								(char)9,
								GiftLazo,
								(char)9,
								GiftColor,
                                (char)9,
                                ((undef) ? "1" : "0"),
                                (char)9,
                                Session.GetHabbo().Username,
                                (char)9,
                                Session.GetHabbo().Look,
                                (char)9,
                                item.Name
							}), 0u, false, false, 0, 0, "");
                            if (clientByUserID.GetHabbo().Id != Session.GetHabbo().Id)
                            {
                                CyberEnvironment.GetGame().GetAchievementManager().ProgressUserAchievement(clientByUserID, "ACH_GiftReceiver", 1, false);
                            }
                        }
                        Session.GetHabbo().LastGiftPurchaseTime = DateTime.Now;
                        continue;
                    }
                    Session.GetMessageHandler().GetResponse().Init(Outgoing.NewInventoryObjectMessageComposer);
                    Session.GetMessageHandler().GetResponse().AppendInt32(1);
                    int i = 1;
                    if (item.GetBaseItem(current2).Type.ToString().ToLower().Equals("s"))
                    {
                        if (item.GetBaseItem(current2).InteractionType == InteractionType.pet1 || item.GetBaseItem(current2).InteractionType == InteractionType.pet2 || item.GetBaseItem(current2).InteractionType == InteractionType.pet3 || item.GetBaseItem(current2).InteractionType == InteractionType.pet4 || item.GetBaseItem(current2).InteractionType == InteractionType.pet5 || item.GetBaseItem(current2).InteractionType == InteractionType.pet6 || item.GetBaseItem(current2).InteractionType == InteractionType.pet7 || item.GetBaseItem(current2).InteractionType == InteractionType.pet8 || item.GetBaseItem(current2).InteractionType == InteractionType.pet9 || item.GetBaseItem(current2).InteractionType == InteractionType.pet10 || item.GetBaseItem(current2).InteractionType == InteractionType.pet11 || item.GetBaseItem(current2).InteractionType == InteractionType.pet12 || item.GetBaseItem(current2).InteractionType == InteractionType.pet13 || item.GetBaseItem(current2).InteractionType == InteractionType.pet14 || item.GetBaseItem(current2).InteractionType == InteractionType.pet15 || item.GetBaseItem(current2).InteractionType == InteractionType.pet16 || item.GetBaseItem(current2).InteractionType == InteractionType.pet17 || item.GetBaseItem(current2).InteractionType == InteractionType.pet18)
                        {
                            i = 3;
                        }
                        else
                        {
                            i = 1;
                        }
                    }
                    Session.GetMessageHandler().GetResponse().AppendInt32(i);
                    List<UserItem> list = this.DeliverItems(Session, item.GetBaseItem(current2), priceAmount * item.Items[current2], ExtraData, num2, limtot, text);
                    Session.GetMessageHandler().GetResponse().AppendInt32(list.Count);
                    foreach (UserItem current3 in list)
                    {
                        Session.GetMessageHandler().GetResponse().AppendUInt(current3.Id);
                    }
                    Session.GetMessageHandler().SendResponse();
                    Session.GetHabbo().GetInventoryComponent().UpdateItems(false);
                    if (item.GetBaseItem(current2).InteractionType == InteractionType.pet1 || item.GetBaseItem(current2).InteractionType == InteractionType.pet2 || item.GetBaseItem(current2).InteractionType == InteractionType.pet3 || item.GetBaseItem(current2).InteractionType == InteractionType.pet4 || item.GetBaseItem(current2).InteractionType == InteractionType.pet5 || item.GetBaseItem(current2).InteractionType == InteractionType.pet6 || item.GetBaseItem(current2).InteractionType == InteractionType.pet7 || item.GetBaseItem(current2).InteractionType == InteractionType.pet8 || item.GetBaseItem(current2).InteractionType == InteractionType.pet9 || item.GetBaseItem(current2).InteractionType == InteractionType.pet10 || item.GetBaseItem(current2).InteractionType == InteractionType.pet11 || item.GetBaseItem(current2).InteractionType == InteractionType.pet12 || item.GetBaseItem(current2).InteractionType == InteractionType.pet13 || item.GetBaseItem(current2).InteractionType == InteractionType.pet14 || item.GetBaseItem(current2).InteractionType == InteractionType.pet15 || item.GetBaseItem(current2).InteractionType == InteractionType.pet16 || item.GetBaseItem(current2).InteractionType == InteractionType.pet17 || item.GetBaseItem(current2).InteractionType == InteractionType.pet18)
                    {
                        Session.SendMessage(Session.GetHabbo().GetInventoryComponent().SerializePetInventory());
                        continue;
                    }
                    continue;
                IL_C41:
                    ExtraData = item.ExtraData;
                    goto IL_DFC;
                IL_DF5:
                    ExtraData = "";
                    goto IL_DFC;
                }
                if (item.Badge.Length >= 1)
                {
                    Session.GetHabbo().GetBadgeComponent().GiveBadge(item.Badge, true, Session, false);
                }
            }
        }
        internal static bool CheckPetName(string PetName)
        {
            return PetName.Length >= 3 && PetName.Length <= 15 && CyberEnvironment.IsValidAlphaNumeric(PetName);
        }
        internal List<UserItem> DeliverItems(GameClient Session, Item Item, int Amount, string ExtraData, int limno, int limtot, string SongCode)
        {
            List<UserItem> list = new List<UserItem>();
            if (Item.InteractionType == InteractionType.postit)
            {
                Amount = Amount * 20;
            }
            checked
            {
                string a;
                if ((a = Item.Type.ToString()) != null)
                {
                    if (a == "i" || a == "s")
                    {
                        int i = 0;
                        while (i < Amount)
                        {
                            InteractionType interactionType = Item.InteractionType;
                            switch (interactionType)
                            {
                                case InteractionType.dimmer:
                                    goto IL_F87;
                                case InteractionType.trophy:
                                case InteractionType.bed:
                                case InteractionType.scoreboard:
                                case InteractionType.vendingmachine:
                                case InteractionType.alert:
                                case InteractionType.onewaygate:
                                case InteractionType.loveshuffler:
                                case InteractionType.habbowheel:
                                case InteractionType.dice:
                                case InteractionType.bottle:
                                case InteractionType.hopper:
                                case InteractionType.rentals:
                                case InteractionType.pet:
                                case InteractionType.pool:
                                case InteractionType.roller:
                                case InteractionType.fbgate:
                                    goto IL_10C3;
                                case InteractionType.teleport:
                                    {
                                        UserItem userItem = Session.GetHabbo().GetInventoryComponent().AddNewItem(0u, Item.ItemId, "0", 0u, true, false, 0, 0, "");
                                        uint id = userItem.Id;
                                        UserItem userItem2 = Session.GetHabbo().GetInventoryComponent().AddNewItem(0u, Item.ItemId, "0", 0u, true, false, 0, 0, "");
                                        uint id2 = userItem2.Id;
                                        list.Add(userItem);
                                        list.Add(userItem2);
                                        using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
                                        {
                                            queryreactor.runFastQuery(string.Concat(new object[]
									{
										"INSERT INTO tele_links (tele_one_id,tele_two_id) VALUES (",
										id,
										",",
										id2,
										")"
									}));
                                            queryreactor.runFastQuery(string.Concat(new object[]
									{
										"INSERT INTO tele_links (tele_one_id,tele_two_id) VALUES (",
										id2,
										",",
										id,
										")"
									}));
                                            break;
                                        }
                                    }
                                case InteractionType.pet0:
                                    {
                                        string[] array = ExtraData.Split(new char[]
								{
									'\n'
								});
                                        Pet pet = Catalog.CreatePet(Session.GetHabbo().Id, array[0], 0, array[1], array[2], 0);
                                        Session.GetHabbo().GetInventoryComponent().AddPet(pet);
                                        list.Add(Session.GetHabbo().GetInventoryComponent().AddNewItem(0u, 320u, "0", 0u, true, false, 0, 0, ""));
                                        break;
                                    }
                                case InteractionType.pet1:
                                    {
                                        string[] array2 = ExtraData.Split(new char[]
								{
									'\n'
								});
                                        Pet pet2 = Catalog.CreatePet(Session.GetHabbo().Id, array2[0], 1, array2[1], array2[2], 0);
                                        Session.GetHabbo().GetInventoryComponent().AddPet(pet2);
                                        list.Add(Session.GetHabbo().GetInventoryComponent().AddNewItem(0u, 320u, "0", 0u, true, false, 0, 0, ""));
                                        break;
                                    }
                                case InteractionType.pet2:
                                    {
                                        string[] array3 = ExtraData.Split(new char[]
								{
									'\n'
								});
                                        Pet pet3 = Catalog.CreatePet(Session.GetHabbo().Id, array3[0], 2, array3[1], array3[2], 0);
                                        Session.GetHabbo().GetInventoryComponent().AddPet(pet3);
                                        list.Add(Session.GetHabbo().GetInventoryComponent().AddNewItem(0u, 320u, "0", 0u, true, false, 0, 0, ""));
                                        break;
                                    }
                                case InteractionType.pet3:
                                    {
                                        string[] array4 = ExtraData.Split(new char[]
								{
									'\n'
								});
                                        Pet pet4 = Catalog.CreatePet(Session.GetHabbo().Id, array4[0], 3, array4[1], array4[2], 0);
                                        Session.GetHabbo().GetInventoryComponent().AddPet(pet4);
                                        list.Add(Session.GetHabbo().GetInventoryComponent().AddNewItem(0u, 320u, "0", 0u, true, false, 0, 0, ""));
                                        break;
                                    }
                                case InteractionType.pet4:
                                    {
                                        string[] array5 = ExtraData.Split(new char[]
								{
									'\n'
								});
                                        Pet pet5 = Catalog.CreatePet(Session.GetHabbo().Id, array5[0], 4, array5[1], array5[2], 0);
                                        Session.GetHabbo().GetInventoryComponent().AddPet(pet5);
                                        list.Add(Session.GetHabbo().GetInventoryComponent().AddNewItem(0u, 320u, "0", 0u, true, false, 0, 0, ""));
                                        break;
                                    }
                                case InteractionType.pet5:
                                    {
                                        string[] array6 = ExtraData.Split(new char[]
								{
									'\n'
								});
                                        Pet pet6 = Catalog.CreatePet(Session.GetHabbo().Id, array6[0], 5, array6[1], array6[2], 0);
                                        Session.GetHabbo().GetInventoryComponent().AddPet(pet6);
                                        list.Add(Session.GetHabbo().GetInventoryComponent().AddNewItem(0u, 320u, "0", 0u, true, false, 0, 0, ""));
                                        break;
                                    }
                                case InteractionType.pet6:
                                    {
                                        string[] array7 = ExtraData.Split(new char[]
								{
									'\n'
								});
                                        Pet pet7 = Catalog.CreatePet(Session.GetHabbo().Id, array7[0], 6, array7[1], array7[2], 0);
                                        Session.GetHabbo().GetInventoryComponent().AddPet(pet7);
                                        list.Add(Session.GetHabbo().GetInventoryComponent().AddNewItem(0u, 320u, "0", 0u, true, false, 0, 0, ""));
                                        break;
                                    }
                                case InteractionType.pet7:
                                    {
                                        string[] array8 = ExtraData.Split(new char[]
								{
									'\n'
								});
                                        Pet pet8 = Catalog.CreatePet(Session.GetHabbo().Id, array8[0], 7, array8[1], array8[2], 0);
                                        Session.GetHabbo().GetInventoryComponent().AddPet(pet8);
                                        list.Add(Session.GetHabbo().GetInventoryComponent().AddNewItem(0u, 320u, "0", 0u, true, false, 0, 0, ""));
                                        break;
                                    }
                                case InteractionType.pet8:
                                    {
                                        string[] array9 = ExtraData.Split(new char[]
								{
									'\n'
								});
                                        Pet pet9 = Catalog.CreatePet(Session.GetHabbo().Id, array9[0], 8, array9[1], array9[2], 0);
                                        Session.GetHabbo().GetInventoryComponent().AddPet(pet9);
                                        list.Add(Session.GetHabbo().GetInventoryComponent().AddNewItem(0u, 320u, "0", 0u, true, false, 0, 0, ""));
                                        break;
                                    }
                                case InteractionType.pet9:
                                    {
                                        string[] array10 = ExtraData.Split(new char[]
								{
									'\n'
								});
                                        Pet pet10 = Catalog.CreatePet(Session.GetHabbo().Id, array10[0], 9, array10[1], array10[2], 0);
                                        Session.GetHabbo().GetInventoryComponent().AddPet(pet10);
                                        list.Add(Session.GetHabbo().GetInventoryComponent().AddNewItem(0u, 320u, "0", 0u, true, false, 0, 0, ""));
                                        break;
                                    }
                                case InteractionType.pet10:
                                    {
                                        string[] array11 = ExtraData.Split(new char[]
								{
									'\n'
								});
                                        Pet pet11 = Catalog.CreatePet(Session.GetHabbo().Id, array11[0], 10, array11[1], array11[2], 0);
                                        Session.GetHabbo().GetInventoryComponent().AddPet(pet11);
                                        list.Add(Session.GetHabbo().GetInventoryComponent().AddNewItem(0u, 320u, "0", 0u, true, false, 0, 0, ""));
                                        break;
                                    }
                                case InteractionType.pet11:
                                    {
                                        string[] array12 = ExtraData.Split(new char[]
								{
									'\n'
								});
                                        Pet pet12 = Catalog.CreatePet(Session.GetHabbo().Id, array12[0], 11, array12[1], array12[2], 0);
                                        Session.GetHabbo().GetInventoryComponent().AddPet(pet12);
                                        list.Add(Session.GetHabbo().GetInventoryComponent().AddNewItem(0u, 320u, "0", 0u, true, false, 0, 0, ""));
                                        break;
                                    }
                                case InteractionType.pet12:
                                    {
                                        string[] array13 = ExtraData.Split(new char[]
								{
									'\n'
								});
                                        Pet pet13 = Catalog.CreatePet(Session.GetHabbo().Id, array13[0], 12, array13[1], array13[2], 0);
                                        Session.GetHabbo().GetInventoryComponent().AddPet(pet13);
                                        list.Add(Session.GetHabbo().GetInventoryComponent().AddNewItem(0u, 320u, "0", 0u, true, false, 0, 0, ""));
                                        break;
                                    }
                                case InteractionType.pet13:
                                    {
                                        string[] array14 = ExtraData.Split(new char[]
								{
									'\n'
								});
                                        Pet pet14 = Catalog.CreatePet(Session.GetHabbo().Id, array14[0], 13, array14[1], array14[2], 0);
                                        Session.GetHabbo().GetInventoryComponent().AddPet(pet14);
                                        list.Add(Session.GetHabbo().GetInventoryComponent().AddNewItem(0u, 320u, "0", 0u, true, false, 0, 0, ""));
                                        break;
                                    }
                                case InteractionType.pet14:
                                    {
                                        string[] array15 = ExtraData.Split(new char[]
								{
									'\n'
								});
                                        Pet pet15 = Catalog.CreatePet(Session.GetHabbo().Id, array15[0], 14, array15[1], array15[2], 0);
                                        Session.GetHabbo().GetInventoryComponent().AddPet(pet15);
                                        list.Add(Session.GetHabbo().GetInventoryComponent().AddNewItem(0u, 320u, "0", 0u, true, false, 0, 0, ""));
                                        break;
                                    }
                                case InteractionType.pet15:
                                    {
                                        string[] array16 = ExtraData.Split(new char[]
								{
									'\n'
								});
                                        Pet pet16 = Catalog.CreatePet(Session.GetHabbo().Id, array16[0], 15, array16[1], array16[2], 0);
                                        Session.GetHabbo().GetInventoryComponent().AddPet(pet16);
                                        list.Add(Session.GetHabbo().GetInventoryComponent().AddNewItem(0u, 320u, "0", 0u, true, false, 0, 0, ""));
                                        break;
                                    }
                                case InteractionType.pet16:
                                    {
                                        string[] array17 = ExtraData.Split(new char[]
								{
									'\n'
								});
                                        Pet pet17 = Catalog.CreatePet(Session.GetHabbo().Id, array17[0], 16, array17[1], array17[2], 0);
                                        Session.GetHabbo().GetInventoryComponent().AddPet(pet17);
                                        list.Add(Session.GetHabbo().GetInventoryComponent().AddNewItem(0u, 320u, "0", 0u, true, false, 0, 0, ""));
                                        break;
                                    }
                                case InteractionType.pet17:
                                    {
                                        string[] array18 = ExtraData.Split(new char[]
								{
									'\n'
								});
                                        Pet pet18 = Catalog.CreatePet(Session.GetHabbo().Id, array18[0], 17, array18[1], array18[2], 0);
                                        Session.GetHabbo().GetInventoryComponent().AddPet(pet18);
                                        list.Add(Session.GetHabbo().GetInventoryComponent().AddNewItem(0u, 320u, "0", 0u, true, false, 0, 0, ""));
                                        break;
                                    }
                                case InteractionType.pet18:
                                    {
                                        string[] array19 = ExtraData.Split(new char[]
								{
									'\n'
								});
                                        Pet pet19 = Catalog.CreatePet(Session.GetHabbo().Id, array19[0], 18, array19[1], array19[2], 0);
                                        Session.GetHabbo().GetInventoryComponent().AddPet(pet19);
                                        list.Add(Session.GetHabbo().GetInventoryComponent().AddNewItem(0u, 320u, "0", 0u, true, false, 0, 0, ""));
                                        break;
                                    }
                                case InteractionType.pet19:
                                    {
                                        string[] array20 = ExtraData.Split(new char[]
								{
									'\n'
								});
                                        Pet pet20 = Catalog.CreatePet(Session.GetHabbo().Id, array20[0], 19, array20[1], array20[2], 0);
                                        Session.GetHabbo().GetInventoryComponent().AddPet(pet20);
                                        list.Add(Session.GetHabbo().GetInventoryComponent().AddNewItem(0u, 320u, "0", 0u, true, false, 0, 0, ""));
                                        break;
                                    }
                                case InteractionType.pet20:
                                    {
                                        string[] array21 = ExtraData.Split(new char[]
								{
									'\n'
								});
                                        Pet pet21 = Catalog.CreatePet(Session.GetHabbo().Id, array21[0], 20, array21[1], array21[2], 0);
                                        Session.GetHabbo().GetInventoryComponent().AddPet(pet21);
                                        list.Add(Session.GetHabbo().GetInventoryComponent().AddNewItem(0u, 320u, "0", 0u, true, false, 0, 0, ""));
                                        break;
                                    }
                                case InteractionType.pet21:
                                    {
                                        string[] array22 = ExtraData.Split(new char[]
								{
									'\n'
								});
                                        Pet pet22 = Catalog.CreatePet(Session.GetHabbo().Id, array22[0], 21, array22[1], array22[2], 0);
                                        Session.GetHabbo().GetInventoryComponent().AddPet(pet22);
                                        list.Add(Session.GetHabbo().GetInventoryComponent().AddNewItem(0u, 320u, "0", 0u, true, false, 0, 0, ""));
                                        break;
                                    }
                                case InteractionType.pet22:
                                    {
                                        string[] array23 = ExtraData.Split(new char[]
								{
									'\n'
								});
                                        Pet pet23 = Catalog.CreatePet(Session.GetHabbo().Id, array23[0], 22, array23[1], array23[2], 0);
                                        Session.GetHabbo().GetInventoryComponent().AddPet(pet23);
                                        list.Add(Session.GetHabbo().GetInventoryComponent().AddNewItem(0u, 320u, "0", 0u, true, false, 0, 0, ""));
                                        break;
                                    }
                                case InteractionType.pet23:
                                    {
                                        string[] array24 = ExtraData.Split(new char[]
								{
									'\n'
								});
                                        Pet pet24 = Catalog.CreatePet(Session.GetHabbo().Id, array24[0], 23, array24[1], array24[2], 0);
                                        Session.GetHabbo().GetInventoryComponent().AddPet(pet24);
                                        list.Add(Session.GetHabbo().GetInventoryComponent().AddNewItem(0u, 320u, "0", 0u, true, false, 0, 0, ""));
                                        break;
                                    }
                                case InteractionType.pet24:
                                    {
                                        string[] array25 = ExtraData.Split(new char[]
								{
									'\n'
								});
                                        Pet pet25 = Catalog.CreatePet(Session.GetHabbo().Id, array25[0], 24, array25[1], array25[2], 0);
                                        Session.GetHabbo().GetInventoryComponent().AddPet(pet25);
                                        list.Add(Session.GetHabbo().GetInventoryComponent().AddNewItem(0u, 320u, "0", 0u, true, false, 0, 0, ""));
                                        break;
                                    }
                                case InteractionType.pet25:
                                    {
                                        string[] array26 = ExtraData.Split(new char[]
								{
									'\n'
								});
                                        Pet pet26 = Catalog.CreatePet(Session.GetHabbo().Id, array26[0], 25, array26[1], array26[2], 0);
                                        Session.GetHabbo().GetInventoryComponent().AddPet(pet26);
                                        list.Add(Session.GetHabbo().GetInventoryComponent().AddNewItem(0u, 320u, "0", 0u, true, false, 0, 0, ""));
                                        break;
                                    }
                                case InteractionType.pet26:
                                    {
                                        string[] array27 = ExtraData.Split(new char[]
								{
									'\n'
								});
                                        Pet pet27 = Catalog.CreatePet(Session.GetHabbo().Id, array27[0], 26, array27[1], array27[2], 0);
                                        Session.GetHabbo().GetInventoryComponent().AddPet(pet27);
                                        list.Add(Session.GetHabbo().GetInventoryComponent().AddNewItem(0u, 320u, "0", 0u, true, false, 0, 0, ""));
                                        break;
                                    }
                                default:
                                    switch (interactionType)
                                    {
                                        case InteractionType.musicdisc:
                                            goto IL_1067;
                                        case InteractionType.puzzlebox:
                                            goto IL_10C3;
                                        case InteractionType.roombg:
                                            goto IL_FF7;
                                        default:
                                            switch (interactionType)
                                            {
                                                case InteractionType.gld_item:
                                                case InteractionType.gld_gate:
                                                case InteractionType.groupforumterminal:
                                                    list.Add(Session.GetHabbo().GetInventoryComponent().AddNewItem(0u, Item.ItemId, "0", Convert.ToUInt32(ExtraData), true, false, 0, 0, ""));
                                                    break;
                                                default:
                                                    goto IL_10C3;
                                            }
                                            break;
                                    }
                                    break;
                            }
                        IL_10EE:
                            i++;
                            continue;
                        IL_F87:
                            UserItem userItem3 = Session.GetHabbo().GetInventoryComponent().AddNewItem(0u, Item.ItemId, ExtraData, 0u, true, false, 0, 0, "");
                            uint id3 = userItem3.Id;
                            list.Add(userItem3);
                            using (IQueryAdapter queryreactor2 = CyberEnvironment.GetDatabaseManager().getQueryReactor())
                            {
                                queryreactor2.runFastQuery("INSERT INTO room_items_moodlight (item_id,enabled,current_preset,preset_one,preset_two,preset_three) VALUES (" + id3 + ",'0',1,'#000000,255,0','#000000,255,0','#000000,255,0')");
                                goto IL_10EE;
                            }
                        IL_FF7:
                            UserItem userItem4 = Session.GetHabbo().GetInventoryComponent().AddNewItem(0u, Item.ItemId, ExtraData, 0u, true, false, 0, 0, "");
                            uint id4 = userItem4.Id;
                            list.Add(userItem4);
                            using (IQueryAdapter queryreactor3 = CyberEnvironment.GetDatabaseManager().getQueryReactor())
                            {
                                queryreactor3.runFastQuery("INSERT INTO room_items_toner VALUES (" + id4 + ",'0',0,0,0)");
                                goto IL_10EE;
                            }
                        IL_1067:
                            list.Add(Session.GetHabbo().GetInventoryComponent().AddNewItem(0u, Item.ItemId, ExtraData, 0u, true, false, 0, 0, SongCode));
                            goto IL_10EE;
                        IL_10C3:
                            list.Add(Session.GetHabbo().GetInventoryComponent().AddNewItem(0u, Item.ItemId, ExtraData, 0u, true, false, limno, limtot, ""));
                            goto IL_10EE;
                        }
                        return list;
                    }
                    if (a == "e")
                    {
                        for (int j = 0; j < Amount; j++)
                        {
                            Session.GetHabbo().GetAvatarEffectsInventoryComponent().AddNewEffect(Item.SpriteId, 7200);
                        }
                        return list;
                    }
                    if (a == "r")
                    {
                        if (Item.Name == "bot_bartender")
                        {
                            RoomBot bot = Catalog.CreateBot(Session.GetHabbo().Id, "Camarera", "hr-9534-39.hd-600-1.ch-819-92.lg-3058-64.sh-3064-110.wa-2005", "¡Te calma la sed y sabe bailar!", "f", true);
                            Session.GetHabbo().GetInventoryComponent().AddBot(bot);
                            Session.SendMessage(Session.GetHabbo().GetInventoryComponent().SerializeBotInventory());
                        }
                        else
                        {
                            RoomBot bot2 = Catalog.CreateBot(Session.GetHabbo().Id, "Robbie", "hr-3020-34.hd-3091-2.ch-225-92.lg-3058-100.sh-3089-1338.ca-3084-78-108.wa-2005", "Habla, anda, baila y se viste", "m", false);
                            Session.GetHabbo().GetInventoryComponent().AddBot(bot2);
                            Session.SendMessage(Session.GetHabbo().GetInventoryComponent().SerializeBotInventory());
                        }
                        return list;
                    }
                }
                return list;
            }
        }
        internal static RoomBot CreateBot(uint UserId, string Name, string Look, string Motto, string Gender, bool Bartender)
        {
            List<RandomSpeech> list = new List<RandomSpeech>();
            List<BotResponse> list2 = new List<BotResponse>();
            uint botId;
            using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
            {
                queryreactor.setQuery("INSERT INTO bots (user_id,name,motto,look,gender,walk_mode,is_bartender) VALUES (@h_user,@b_name,@b_motto,@b_look,@b_gender,@b_walk,@b_bartender)");
                queryreactor.addParameter("h_user", UserId);
                queryreactor.addParameter("b_name", Name);
                queryreactor.addParameter("b_motto", Motto);
                queryreactor.addParameter("b_look", Look);
                queryreactor.addParameter("b_gender", Gender);
                queryreactor.addParameter("b_walk", "freeroam");
                queryreactor.addParameter("b_bartender", Bartender ? "1" : "0");
                botId = Convert.ToUInt32(queryreactor.insertQuery());
            }
            return new RoomBot(botId, UserId, 0u, AIType.Generic, "freeroam", Name, Motto, Look, 0, 0, 0.0, 0, 0, 0, 0, 0, ref list, ref list2, Gender, 0, Bartender);
        }
        internal static Pet CreatePet(uint UserId, string Name, int Type, string Race, string Color, int Rarity = 0)
        {
            checked
            {
                Pet pet = new Pet(404u, UserId, 0u, Name, (uint)Type, Race, Color, 0, 100, 100, 0, (double)CyberEnvironment.GetUnixTimestamp(), 0, 0, 0.0, false, 0, 0, -1, Rarity, DateTime.Now.AddHours(36.0), DateTime.Now.AddHours(48.0), null);
                pet.DBState = DatabaseUpdateState.NeedsUpdate;
                using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
                {
                    queryreactor.setQuery(string.Concat(new object[]
					{
						"INSERT INTO bots (user_id,name, ai_type) VALUES (",
						pet.OwnerId,
						",@",
						pet.PetId,
						"name, 'pet')"
					}));
                    queryreactor.addParameter(pet.PetId + "name", pet.Name);
                    pet.PetId = (uint)queryreactor.insertQuery();
                    queryreactor.setQuery(string.Concat(new object[]
					{
						"INSERT INTO bots_petdata (id,type,race,color,experience,energy,createstamp,rarity,lasthealth_stamp,untilgrown_stamp) VALUES (",
						pet.PetId,
						", ",
						pet.Type,
						",@",
						pet.PetId,
						"race,@",
						pet.PetId,
						"color,0,100,UNIX_TIMESTAMP(), ",
						Rarity,
						", UNIX_TIMESTAMP(now() + INTERVAL 36 HOUR), UNIX_TIMESTAMP(now() + INTERVAL 48 HOUR))"
					}));
                    queryreactor.addParameter(pet.PetId + "race", pet.Race);
                    queryreactor.addParameter(pet.PetId + "color", pet.Color);
                    queryreactor.runQuery();
                }
                if (pet.Type == 16u)
                {
                    pet.MoplaBreed = MoplaBreed.CreateMonsterplantBreed(pet);
                    pet.Name = pet.MoplaBreed.Name;
                    pet.DBState = DatabaseUpdateState.NeedsUpdate;
                }
                return pet;
            }
        }
        internal static Pet GeneratePetFromRow(DataRow Row, DataRow mRow)
        {
            if (Row == null)
            {
                return null;
            }
            MoplaBreed moplaBreed = null;
            if (Convert.ToUInt32(mRow["type"]) == 16u)
            {
                using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
                {
                    queryreactor.setQuery("SELECT * FROM bots_monsterplants WHERE pet_id = " + Convert.ToUInt32(Row["id"]));
                    DataRow row = queryreactor.getRow();
                    moplaBreed = new MoplaBreed(row);
                }
            }
            return new Pet(Convert.ToUInt32(Row["id"]), Convert.ToUInt32(Row["user_id"]), Convert.ToUInt32(Row["room_id"]), (string)Row["name"], Convert.ToUInt32(mRow["type"]), (string)mRow["race"], (string)mRow["color"], (int)mRow["experience"], (int)mRow["energy"], (int)mRow["nutrition"], (int)mRow["respect"], Convert.ToDouble(mRow["createstamp"]), (int)Row["x"], (int)Row["y"], Convert.ToDouble(Row["z"]), (int)mRow["have_saddle"] == 1, (int)mRow["anyone_ride"], (int)mRow["hairdye"], (int)mRow["pethair"], (int)mRow["rarity"], CyberEnvironment.UnixToDateTime((double)((int)mRow["lasthealth_stamp"])), CyberEnvironment.UnixToDateTime((double)((int)mRow["untilgrown_stamp"])), moplaBreed);
        }
        internal EcotronReward GetRandomEcotronReward()
        {
            uint level = 1u;
            if (CyberEnvironment.GetRandomNumber(1, 2000) == 2000)
            {
                level = 5u;
            }
            else
            {
                if (CyberEnvironment.GetRandomNumber(1, 200) == 200)
                {
                    level = 4u;
                }
                else
                {
                    if (CyberEnvironment.GetRandomNumber(1, 40) == 40)
                    {
                        level = 3u;
                    }
                    else
                    {
                        if (CyberEnvironment.GetRandomNumber(1, 4) == 4)
                        {
                            level = 2u;
                        }
                    }
                }
            }
            List<EcotronReward> ecotronRewardsForLevel = this.GetEcotronRewardsForLevel(level);
            return ecotronRewardsForLevel[CyberEnvironment.GetRandomNumber(0, checked(ecotronRewardsForLevel.Count - 1))];
        }
        internal List<EcotronReward> GetEcotronRewards()
        {
            return this.EcotronRewards;
        }
        internal List<int> GetEcotronRewardsLevels()
        {
            return this.EcotronLevels;
        }
        internal List<EcotronReward> GetEcotronRewardsForLevel(uint Level)
        {
            List<EcotronReward> list = new List<EcotronReward>();
            foreach (EcotronReward current in this.EcotronRewards)
            {
                if (current.RewardLevel == Level)
                {
                    list.Add(current);
                }
            }
            return list;
        }
    }
}
