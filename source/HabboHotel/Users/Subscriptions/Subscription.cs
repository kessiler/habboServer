using System;
namespace Cyber.HabboHotel.Users.Subscriptions
{
	internal class Subscription
	{
		private int Id;
		private int TimeActivated;
		private int TimeExpire;
		private int TimeLastGift;
		internal int SubscriptionId
		{
			get
			{
				return this.Id;
			}
		}
		internal int ExpireTime
		{
			get
			{
				return this.TimeExpire;
			}
		}
		internal int ActivateTime
		{
			get
			{
				return this.TimeActivated;
			}
		}
		internal int LastGiftTime
		{
			get
			{
				return this.TimeLastGift;
			}
		}
		internal bool IsValid
		{
			get
			{
				return this.TimeExpire > CyberEnvironment.GetUnixTimestamp();
			}
		}
		internal Subscription(int Id, int Activated, int TimeExpire, int TimeLastGift)
		{
			this.Id = Id;
			this.TimeActivated = Activated;
			this.TimeExpire = TimeExpire;
			this.TimeLastGift = TimeLastGift;
		}
	}
}
