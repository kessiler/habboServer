using Database_Manager.Database.Session_Details.Interfaces;
using Cyber.HabboHotel.GameClients;
using Cyber.HabboHotel.Users.UserDataManagement;
using System;
using System.Data;
namespace Cyber.HabboHotel.Users.Subscriptions
{
	internal class SubscriptionManager
	{
		private uint UserId;
		private Subscription Subscription;
		internal bool HasSubscription
		{
			get
			{
				return this.Subscription != null && this.Subscription.IsValid;
			}
		}
		internal SubscriptionManager(uint userID, UserData userData)
		{
			this.UserId = userID;
			this.Subscription = userData.subscriptions;
		}
		internal Subscription GetSubscription()
		{
			return this.Subscription;
		}
		internal void AddSubscription(double DayLength)
		{
			int num = checked((int)Math.Round(DayLength));
			GameClient clientByUserID = CyberEnvironment.GetGame().GetClientManager().GetClientByUserID(this.UserId);
			DateTime target;
			int num2;
			int num3;
			if (this.Subscription != null)
			{
				target = CyberEnvironment.UnixToDateTime((double)this.Subscription.ExpireTime).AddDays((double)num);
				num2 = this.Subscription.ActivateTime;
				num3 = this.Subscription.LastGiftTime;
			}
			else
			{
				target = DateTime.Now.AddDays((double)num);
				num2 = CyberEnvironment.GetUnixTimestamp();
				num3 = CyberEnvironment.GetUnixTimestamp();
			}
			int num4 = CyberEnvironment.DateTimeToUnix(target);
			this.Subscription = new Subscription(2, num2, num4, num3);
			using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
			{
				queryreactor.runFastQuery(string.Concat(new object[]
				{
					"REPLACE INTO user_subscriptions VALUES (",
					this.UserId,
					", 2, ",
					num2,
					", ",
					num4,
					", ",
					num3,
					");"
				}));
			}
			clientByUserID.GetHabbo().SerializeClub();
		}
		internal void ReloadSubscription()
		{
			using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
			{
				queryreactor.setQuery("SELECT * FROM user_subscriptions WHERE user_id=@id AND timestamp_expire > UNIX_TIMESTAMP() ORDER BY subscription_id DESC LIMIT 1");
				queryreactor.addParameter("id", this.UserId);
				DataRow row = queryreactor.getRow();
				if (row == null)
				{
					this.Subscription = null;
				}
				else
				{
					this.Subscription = new Subscription((int)row[1], (int)row[2], (int)row[3], (int)row[4]);
				}
			}
		}
	}
}
