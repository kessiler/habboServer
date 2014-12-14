using Cyber.HabboHotel.GameClients;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Cyber.Core;

namespace Cyber.HabboHotel.Misc
{
	internal class PixelManager
	{
        internal static Timer mTimer;

        internal void StartPixelTimer()
        {
            if (ExtraSettings.CURRENCY_LOOP_ENABLED)
            {
                mTimer = new Timer(new TimerCallback(GivePixels), null, 0, ExtraSettings.CURRENTY_LOOP_TIME_IN_MINUTES * 60000);
            }
        }

        internal void GivePixels(object Caller)
        {
            var tClients = CyberEnvironment.GetGame().GetClientManager().clients.Values;

            foreach (GameClient Client in tClients)
            {
                if (Client == null || Client.GetHabbo() == null)
                {
                    continue;
                }

                Client.GetHabbo().Credits += ExtraSettings.CREDITS_TO_GIVE;
                Client.GetHabbo().UpdateCreditsBalance();

                Client.GetHabbo().ActivityPoints += ExtraSettings.PIXELS_TO_GIVE;

                if (ExtraSettings.DIAMONDS_LOOP_ENABLED)
                {
                    if (ExtraSettings.DIAMONDS_VIP_ONLY)
                    {
                        if (Client.GetHabbo().VIP || Client.GetHabbo().Rank >= 6)
                        {
                            Client.GetHabbo().BelCredits += ExtraSettings.DIAMONDS_TO_GIVE;
                        }
                    }
                    else
                    {;
                        Client.GetHabbo().BelCredits += ExtraSettings.DIAMONDS_TO_GIVE;
                    }
                }
                Client.GetHabbo().UpdateSeasonalCurrencyBalance();
            }
        }


        internal void Destroy()
        {
            mTimer.Dispose();
            mTimer = null;
        }
	}
}
