using Cyber.HabboHotel.Items;
using System;
namespace Cyber.HabboHotel.Catalogs
{
	internal class EcotronReward
	{
		internal uint DisplayId;
		internal uint BaseId;
		internal uint RewardLevel;
		internal EcotronReward(uint DisplayId, uint BaseId, uint RewardLevel)
		{
			this.DisplayId = DisplayId;
			this.BaseId = BaseId;
			this.RewardLevel = RewardLevel;
		}
		internal Item GetBaseItem()
		{
			return CyberEnvironment.GetGame().GetItemManager().GetItem(this.BaseId);
		}
	}
}
