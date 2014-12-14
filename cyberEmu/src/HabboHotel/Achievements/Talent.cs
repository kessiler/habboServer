using System;
namespace Cyber.HabboHotel.Achievements
{
	internal class Talent
	{
		internal int Id;
		internal string Type;
		internal int ParentCategory;
		internal int Level;
		internal string AchievementGroup;
		internal int AchievementLevel;
		internal string Prize;
		internal uint PrizeBaseItem;
		internal Achievement GetAchievement()
		{
			if (this.AchievementGroup == "" || this.ParentCategory == -1)
			{
				return null;
			}
			return CyberEnvironment.GetGame().GetAchievementManager().GetAchievement(this.AchievementGroup);
		}
		internal Talent(int Id, string Type, int ParentCategory, int Level, string AchId, int AchLevel, string Prize, uint PrizeBaseItem)
		{
			this.Id = Id;
			this.Type = Type;
			this.ParentCategory = ParentCategory;
			this.Level = Level;
			this.AchievementGroup = AchId;
			this.AchievementLevel = AchLevel;
			this.Prize = Prize;
			this.PrizeBaseItem = PrizeBaseItem;
		}
	}
}
