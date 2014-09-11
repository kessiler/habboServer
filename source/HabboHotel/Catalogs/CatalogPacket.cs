using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cyber.Messages;
using Cyber.Messages.Headers;
using Cyber.HabboHotel.GameClients;
using Cyber.HabboHotel.Items;

namespace Cyber.HabboHotel.Catalogs
{
    static class CatalogPacket
    {
        internal static ServerMessage ComposeIndex(uint Rank)
        {
            var Pages = CyberEnvironment.GetGame().GetCatalog().Categories.Values.OfType<CatalogPage>();
            ServerMessage Message = new ServerMessage(Outgoing.CatalogueIndexMessageComposer);
            var SortedPages = Pages.Where(x => x.ParentId == -1 && x.MinRank <= Rank).OrderBy(x => x.OrderNum);

            Message.AppendBoolean(true);
            Message.AppendInt32(0);
            Message.AppendInt32(-1);
            Message.AppendString("root");
            Message.AppendString(string.Empty);
            Message.AppendInt32(0);
            Message.AppendInt32(SortedPages.Count());
            foreach (CatalogPage Cat in SortedPages)
            {
                Message.AppendBoolean(Cat.Visible);
                Message.AppendInt32(Cat.IconImage);
                Message.AppendInt32(Cat.PageId);
                Message.AppendString(Cat.CodeName);
                Message.AppendString(Cat.Caption);
                Message.AppendInt32(Cat.FlatOffers.Count);
                foreach (int i in Cat.FlatOffers.Keys)
                {
                    Message.AppendInt32(i);
                }

                var SortedSubPages = Pages.Where(x => x.ParentId == Cat.PageId && x.MinRank <= Rank).OrderBy(x => x.Caption);
                Message.AppendInt32(SortedSubPages.Count());

                foreach (CatalogPage SubCat in SortedSubPages)
                {
                    Message.AppendBoolean(SubCat.Visible);
                    Message.AppendInt32(SubCat.IconImage);
                    Message.AppendInt32(SubCat.PageId);
                    Message.AppendString(SubCat.CodeName);
                    Message.AppendString(SubCat.Caption);
                    Message.AppendInt32(SubCat.FlatOffers.Count);
                    foreach (int i2 in SubCat.FlatOffers.Keys)
                    {
                        Message.AppendInt32(i2);
                    }
                    Message.AppendInt32(0);
                }
            }
            Message.AppendBoolean(false);
            Message.AppendString("NORMAL");

            return Message;
        }
        
        internal static ServerMessage ComposePage(CatalogPage Page)
        {
            ServerMessage Message = new ServerMessage(Outgoing.CataloguePageMessageComposer);
            Message.AppendInt32(Page.PageId);
            Message.AppendString("NORMAL");

            switch (Page.Layout)
            {
                case "frontpage":
                    Message.AppendString("frontpage4");
                    Message.AppendInt32(2);
                    Message.AppendString(Page.LayoutHeadline);
                    Message.AppendString(Page.LayoutTeaser);
                    Message.AppendInt32(2);
                    Message.AppendString(Page.Text1);
                    Message.AppendString(Page.Text2);
                    Message.AppendInt32(0);
                    Message.AppendInt32(-1);
                    Message.AppendBoolean(false);
                    break;

                case "roomads":
                    Message.AppendString("roomads");
                    Message.AppendInt32(2);
                    Message.AppendString("events_header");
                    Message.AppendString("");
                    Message.AppendInt32(2);
                    Message.AppendString(Page.Text1);
                    Message.AppendString("");
                    break;

                case "bots":
                    Message.AppendString("bots");
                    Message.AppendInt32(2);
                    Message.AppendString(Page.LayoutHeadline);
                    Message.AppendString(Page.LayoutTeaser);
                    Message.AppendInt32(3);
                    Message.AppendString(Page.Text1);
                    Message.AppendString(Page.Text2);
                    Message.AppendString(Page.TextDetails);
                    break;

                case "badge_display":
                    Message.AppendString("badge_display");
                    Message.AppendInt32(2);
                    Message.AppendString(Page.LayoutHeadline);
                    Message.AppendString(Page.LayoutTeaser);
                    Message.AppendInt32(3);
                    Message.AppendString(Page.Text1);
                    Message.AppendString(Page.Text2);
                    Message.AppendString(Page.TextDetails);
                    break;

                case "info_loyalty":
                case "info_duckets":
                    Message.AppendString(Page.Layout);
                    Message.AppendInt32(1);
                    Message.AppendString(Page.LayoutHeadline);
                    Message.AppendInt32(1);
                    Message.AppendString(Page.Text1);
                    break;
                case "sold_ltd_items":
                    Message.AppendString("sold_ltd_items");
                    Message.AppendInt32(2);
                    Message.AppendString(Page.LayoutHeadline);
                    Message.AppendString(Page.LayoutTeaser);
                    Message.AppendInt32(3);
                    Message.AppendString(Page.Text1);
                    Message.AppendString(Page.Text2);
                    Message.AppendString(Page.TextDetails);
                    break;
                case "recycler_info":
                    Message.AppendString(Page.Layout);
                    Message.AppendInt32(2);
                    Message.AppendString(Page.LayoutHeadline);
                    Message.AppendString(Page.LayoutTeaser);
                    Message.AppendInt32(3);
                    Message.AppendString(Page.Text1);
                    Message.AppendString(Page.Text2);
                    Message.AppendString(Page.TextDetails);
                    break;
                case "recycler_prizes":
                    Message.AppendString("recycler_prizes");
                    Message.AppendInt32(1);
                    Message.AppendString("catalog_recycler_headline3");
                    Message.AppendInt32(1);
                    Message.AppendString(Page.Text1);
                    break;
                case "spaces_new":
                case "spaces":
                    Message.AppendString("spaces_new");
                    Message.AppendInt32(1);
                    Message.AppendString(Page.LayoutHeadline);
                    Message.AppendInt32(1);
                    Message.AppendString(Page.Text1);
                    break;
                case "recycler":
                    Message.AppendString(Page.Layout);
                    Message.AppendInt32(2);
                    Message.AppendString(Page.LayoutHeadline);
                    Message.AppendString(Page.LayoutTeaser);
                    Message.AppendInt32(1);
                    Message.AppendString(Page.Text1);
                    Message.AppendInt32(-1);
                    Message.AppendBoolean(false);
                    break;

                case "trophies":
                    Message.AppendString("trophies");
                    Message.AppendInt32(1);
                    Message.AppendString(Page.LayoutHeadline);
                    Message.AppendInt32(2);
                    Message.AppendString(Page.Text1);
                    Message.AppendString(Page.TextDetails);
                    break;

                case "pets":
                case "pets2":
                case "pets3":
                    Message.AppendString(Page.Layout);
                    Message.AppendInt32(2);
                    Message.AppendString(Page.LayoutHeadline);
                    Message.AppendString(Page.LayoutTeaser);
                    Message.AppendInt32(4);
                    Message.AppendString(Page.Text1);
                    Message.AppendString(Page.Text2);
                    Message.AppendString(Page.TextDetails);
                    Message.AppendString(Page.TextTeaser);
                    break;

                case "soundmachine":
                    Message.AppendString(Page.Layout);
                    Message.AppendInt32(2);
                    Message.AppendString(Page.LayoutHeadline);
                    Message.AppendString(Page.LayoutTeaser);
                    Message.AppendInt32(2);
                    Message.AppendString(Page.Text1);
                    Message.AppendString(Page.TextDetails);
                    break;
                case "vip_buy":
                    Message.AppendString(Page.Layout);
                    Message.AppendInt32(2);
                    Message.AppendString(Page.LayoutHeadline);
                    Message.AppendString(Page.LayoutTeaser);
                    Message.AppendInt32(0);
                    break;

                case "guild_custom_furni":
                    Message.AppendString("guild_custom_furni");
                    Message.AppendInt32(3);
                    Message.AppendString(Page.LayoutHeadline);
                    Message.AppendString("");
                    Message.AppendString("");
                    Message.AppendInt32(3);
                    Message.AppendString(Page.Text1);
                    Message.AppendString(Page.TextDetails);
                    Message.AppendString(Page.Text2);
                    break;

                case "guild_frontpage":
                    Message.AppendString("guild_frontpage");
                    Message.AppendInt32(2);
                    Message.AppendString(Page.LayoutHeadline);
                    Message.AppendString(Page.LayoutTeaser);
                    Message.AppendInt32(3);
                    Message.AppendString(Page.Text1);
                    Message.AppendString(Page.TextDetails);
                    Message.AppendString(Page.Text2);
                    break;

                case "guild_forum":
                    Message.AppendString("guild_forum");
                    Message.AppendInt32(0);
                    Message.AppendInt32(2);
                    Message.AppendString(Page.Text1);
                    Message.AppendString(Page.Text2);
                    break;

                case "club_gifts":
                    Message.AppendString("club_gifts");
                    Message.AppendInt32(1);
                    Message.AppendString(Page.LayoutHeadline);
                    Message.AppendInt32(1);
                    Message.AppendString(Page.Text1);
                    break;

                default:
                case "default_3x3":
                    Message.AppendString(Page.Layout);
                    Message.AppendInt32(3);
                    Message.AppendString(Page.LayoutHeadline);
                    Message.AppendString(Page.LayoutTeaser);
                    Message.AppendString(Page.LayoutSpecial);
                    Message.AppendInt32(3);
                    Message.AppendString(Page.Text1);
                    Message.AppendString(Page.TextDetails);
                    Message.AppendString(Page.TextTeaser);
                    break;
            }

            if (Page.Layout.StartsWith("frontpage") || Page.Layout == "vip_buy")
            {
                Message.AppendInt32(0);
            }
            else
            {
                Message.AppendInt32(Page.Items.Count);
                foreach (CatalogItem Item in Page.Items.Values)
                {
                    ComposeItem(Item, Message);
                }
            }
                

            Message.AppendInt32(-1);
            Message.AppendBoolean(false);

            return Message;
        }

        internal static ServerMessage ComposeClubPurchasePage(GameClient Session, int WindowId)
        {
            // Coded by Finn for Cyber Emulator
            ServerMessage Message = new ServerMessage(Outgoing.CatalogueClubPageMessageComposer);
           
            var habboClubItems = CyberEnvironment.GetGame().GetCatalog().HabboClubItems;
            Message.AppendInt32(habboClubItems.Count);

            foreach (CatalogItem Item in habboClubItems)
            {
                Message.AppendUInt(Item.Id);
                Message.AppendString(Item.Name);
                Message.AppendBoolean(false);
                Message.AppendInt32(Item.CreditsCost);
                Message.AppendInt32(Item.DucketsCost);
                Message.AppendInt32(0);
                Message.AppendBoolean(true);

                string[] fuckingArray = Item.Name.Split('_');
                double dayTime = 31;

                if (Item.Name.Contains("DAY"))
                {
                    dayTime = int.Parse(fuckingArray[3]);
                }
                else if (Item.Name.Contains("MONTH"))
                {
                    int monthTime = int.Parse(fuckingArray[3]);
                    dayTime = monthTime * 31;
                }
                else if (Item.Name.Contains("YEAR"))
                {
                    int yearTimeOmg = int.Parse(fuckingArray[3]);
                    dayTime = yearTimeOmg * 31 * 12;
                }

                DateTime newExpiryDate = DateTime.Now.AddDays(dayTime);
                if (Session.GetHabbo().GetSubscriptionManager().HasSubscription)
                {
                    newExpiryDate = CyberEnvironment.UnixToDateTime((double)Session.GetHabbo().GetSubscriptionManager().GetSubscription().ExpireTime).AddDays(dayTime);
                }
                Message.AppendInt32((int)dayTime / 31);
                Message.AppendInt32((int)dayTime);
                Message.AppendBoolean(false);
                Message.AppendInt32((int)dayTime);
                Message.AppendInt32(newExpiryDate.Year);
                Message.AppendInt32(newExpiryDate.Month);
                Message.AppendInt32(newExpiryDate.Day);
            }
            Message.AppendInt32(WindowId);
            return Message;
        }

        internal static ServerMessage PurchaseOK()
        {
            ServerMessage Message = new ServerMessage(Outgoing.PurchaseOKMessageComposer);
            Message.AppendInt32(0);
            Message.AppendString("");
            Message.AppendBoolean(false);
            Message.AppendInt32(0);
            Message.AppendInt32(0);
            Message.AppendInt32(0);
            Message.AppendBoolean(true);
            Message.AppendInt32(1);
            Message.AppendString("s");
            Message.AppendInt32(0);
            Message.AppendString("");
            Message.AppendInt32(1);
            Message.AppendInt32(0);
            Message.AppendString("");
            Message.AppendInt32(1);
            return Message;
        }

        internal static void ComposeItem(CatalogItem Item, ServerMessage Message)
        {
            Message.AppendUInt(Item.Id);
            Message.AppendString(Item.Name);
            Message.AppendBoolean(false);
            Message.AppendInt32(Item.CreditsCost);
            if (Item.BelCreditsCost > 0)
            {
                Message.AppendInt32(Item.BelCreditsCost);
                Message.AppendInt32(105);
            }
            else
            {
                if (Item.LoyaltyCost > 0)
                {
                    Message.AppendInt32(Item.LoyaltyCost);
                    Message.AppendInt32(105);
                }
                else
                {
                    Message.AppendInt32(Item.DucketsCost);
                    Message.AppendInt32(0);
                }
            }
            Message.AppendBoolean(Item.GetFirstBaseItem().AllowGift);
            checked
            {
                if (Item.Name == "g0 group_product")
                {
                    Message.AppendInt32(0);
                }
                else
                {
                    if (Item.Badge == "")
                    {
                        Message.AppendInt32(Item.Items.Count);
                    }
                    else
                    {
                        if (Item.Name == "room_ad_plus_badge")
                        {
                            Message.AppendInt32(1);
                        }
                        else
                        {
                            Message.AppendInt32(Item.Items.Count + 1);
                        }
                        Message.AppendString("b");
                        Message.AppendString(Item.Badge);
                    }
                }
                foreach (uint current in Item.Items.Keys)
                {
                    if (Item.Name == "g0 group_product")
                    {
                        break;
                    }
                    if (Item.Name != "room_ad_plus_badge")
                    {
                        Message.AppendString(Item.GetBaseItem(current).Type.ToString());
                        Message.AppendInt32(Item.GetBaseItem(current).SpriteId);

                        if (Item.Name.Contains("wallpaper_single") || Item.Name.Contains("floor_single") || Item.Name.Contains("landscape_single"))
                        {
                            string[] array = Item.Name.Split('_');
                            Message.AppendString(array[2]);
                        }
                        else
                        {
                            if (Item.Name.StartsWith("bot_") || Item.GetBaseItem(current).InteractionType == InteractionType.musicdisc)
                            {
                                Message.AppendString(Item.ExtraData);
                            }
                            else
                            {
                                if (Item.Name.StartsWith("poster_"))
                                {
                                    string[] array2 = Item.Name.Split('_');
                                    Message.AppendString(array2[1]);
                                }
                                else
                                {
                                    if (Item.Name.StartsWith("poster "))
                                    {
                                        string[] array3 = Item.Name.Split(' ');
                                        Message.AppendString(array3[1]);
                                    }
                                    else
                                    {
                                        if (Item.songID > 0u && Item.GetBaseItem(current).InteractionType == InteractionType.musicdisc)
                                        {
                                            Message.AppendString(Item.ExtraData);
                                        }
                                        else
                                        {
                                            Message.AppendString(string.Empty);
                                        }
                                    }
                                }
                            }
                        }
                        Message.AppendInt32(Item.Items[current]);
                        Message.AppendBoolean(Item.IsLimited);
                        if (Item.IsLimited)
                        {
                            Message.AppendInt32(Item.LimitedStack);
                            Message.AppendInt32(Item.LimitedStack - Item.LimitedSelled);
                        }
                    }
                    else
                    {
                        Message.AppendString("");
                        Message.AppendInt32(0);
                    }
                }
                Message.AppendInt32(Item.ClubOnly ? 1 : 0);
                if (Item.IsLimited || Item.FirstAmount != 1)
                {
                    Message.AppendBoolean(false);
                    return;
                }
                Message.AppendBoolean(Item.HaveOffer && !Item.IsLimited);
            }
        }

    }
}
