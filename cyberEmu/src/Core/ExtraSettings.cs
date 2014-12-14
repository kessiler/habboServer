using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Cyber.Core
{
    class ExtraSettings
    {
        internal static bool CURRENCY_LOOP_ENABLED = true;
        internal static int CURRENTY_LOOP_TIME_IN_MINUTES = 15;
        internal static int CREDITS_TO_GIVE = 3000;
        internal static int PIXELS_TO_GIVE = 100;

        internal static bool DIAMONDS_LOOP_ENABLED = true;
        internal static bool DIAMONDS_VIP_ONLY = true;
        internal static int DIAMONDS_TO_GIVE = 1;

        internal static bool CHANGE_NAME_STAFF = true;
        internal static bool CHANGE_NAME_VIP = true;
        internal static bool CHANGE_NAME_EVERYONE = true;

        internal static bool NEW_USER_GIFTS_ENABLED = true;
        internal static bool NAVIGATOR_NEW_ENABLED = true;
        internal static bool ENABLE_BETA_CAMERA = true;
        internal static uint NEW_USER_GIFT_YTTV2_ID = 4930;

        internal static bool EVERYONE_USE_FLOOR = true;
        internal static string FIGUREDATA_URL = "http://www.holobox.com.es/lcnchk/figuredata.xml";
        internal static string YOUTUBE_GENERATOR_SUBURL = "youtubethumbnail.php?Video";


        internal static bool RunExtraSettings()
        {
            if (!File.Exists("extra_settings.txt"))
            {
                return false;
            }
            else
            {
                foreach (string Line in File.ReadAllLines("extra_settings.txt", Encoding.Default))
                {
                    if (String.IsNullOrWhiteSpace(Line) || !Line.Contains("="))
                    {
                        continue;
                    }

                    string[] PARAMS = Line.Split('=');

                    switch (PARAMS[0])
                    {
                        case "currency.loop.enabled":
                            if (PARAMS[1] != "true")
                            {
                                CURRENCY_LOOP_ENABLED = false;
                            }
                            break;

                        case "currency.loop.time.in.minutes":
                            int i = 15;
                            if (int.TryParse(PARAMS[1], out i))
                            {
                                CURRENTY_LOOP_TIME_IN_MINUTES = i;
                            }
                            break;

                        case "credits.to.give":
                            int j = 3000;
                            if (int.TryParse(PARAMS[1], out j))
                            {
                                CREDITS_TO_GIVE = j;
                            }
                            break;

                        case "pixels.to.give":
                            int k = 100;
                            if (int.TryParse(PARAMS[1], out k))
                            {
                                PIXELS_TO_GIVE = k;
                            }
                            break;

                        case "diamonds.loop.enabled":
                            if (PARAMS[1] != "true")
                            {
                                DIAMONDS_LOOP_ENABLED = false;
                            }
                            break;

                        case "diamonds.to.give":
                            int l = 100;
                            if (int.TryParse(PARAMS[1], out l))
                            {
                                DIAMONDS_TO_GIVE = l;
                            }
                            break;

                        case "diamonds.vip.only":
                            if (PARAMS[1] != "true")
                            {
                                DIAMONDS_VIP_ONLY = false;
                            }
                            break;

                        case "navigator.newstyle.enabled":
                            if (PARAMS[1] != "true")
                            {
                                NAVIGATOR_NEW_ENABLED = false;
                            }
                            break;

                        case "change.name.staff":
                            if (PARAMS[1] != "true")
                            {
                                CHANGE_NAME_STAFF = false;
                            }
                            break;

                        case "change.name.vip":
                            if (PARAMS[1] != "true")
                            {
                                CHANGE_NAME_VIP = false;
                            }
                            break;

                        case "change.name.everyone":
                            if (PARAMS[1] != "true")
                            {
                                CHANGE_NAME_EVERYONE = false;
                            }
                            break;

                        case "enable.beta.camera":
                            if (PARAMS[1] != "true")
                            {
                                ENABLE_BETA_CAMERA = false;
                            }
                            break;
                        case "newuser.gifts.enabled":
                            if (PARAMS[1] != "true")
                            {
                                NEW_USER_GIFTS_ENABLED = false;
                            }
                            break;

                        case "newuser.gift.yttv2.id":
                            uint u;
                            if (uint.TryParse(PARAMS[1], out u))
                            {
                                NEW_USER_GIFT_YTTV2_ID = u;
                            }
                            break;

                        case "everyone.use.floor":
                            if (PARAMS[1] != "true")
                            {
                                EVERYONE_USE_FLOOR = false;
                            }
                            break;

                        case "figuredata.url":
                            FIGUREDATA_URL = PARAMS[1];
                            break;

                        case "youtube.thumbnail.suburl":
                            YOUTUBE_GENERATOR_SUBURL = PARAMS[1];
                            break;

                        default: break;
                    }
                }
            }

            return true;

        }
    }
}
