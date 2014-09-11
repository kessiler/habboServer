using System;
namespace Cyber.HabboHotel.Achievements
{
	internal class UserAchievement
	{
		internal readonly string AchievementGroup;
		internal int Level;
		internal int Progress;
		public UserAchievement(string achievementGroup, int level, int progress)
		{
			this.AchievementGroup = achievementGroup;
			this.Level = level;
			this.Progress = progress;
		}
	}
}
