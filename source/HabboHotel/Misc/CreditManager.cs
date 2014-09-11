using Cyber.HabboHotel.GameClients;
using System;
namespace Cyber.HabboHotel.Misc
{
	internal class CreditManager
	{
		internal static void GiveCredits(GameClient Client, int amount)
		{
			if (Client == null || Client.GetHabbo() == null)
			{
				return;
			}
			double arg_12_0 = (double)CyberEnvironment.GetUnixTimestamp();
			checked
			{
				Client.GetHabbo().Credits += amount;
				Client.GetHabbo().UpdateCreditsBalance();
			}
		}
	}
}
