using System;
namespace Cyber.HabboHotel.Support
{
	internal struct ModerationBan
	{
		internal ModerationBanType Type;
		internal string Variable;
		internal string ReasonMessage;
		internal double Expire;
		internal bool Expired
		{
			get
			{
				return (double)CyberEnvironment.GetUnixTimestamp() >= this.Expire;
			}
		}
		internal ModerationBan(ModerationBanType Type, string Variable, string ReasonMessage, double Expire)
		{
			this.Type = Type;
			this.Variable = Variable;
			this.ReasonMessage = ReasonMessage;
			this.Expire = Expire;
		}
	}
}
