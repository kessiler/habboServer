using System;
using System.Collections.Generic;
namespace Cyber.HabboHotel.Achievements
{
	internal class Achievement
	{
		internal readonly uint Id;
		internal readonly string GroupName;
		internal readonly string Category;
		internal readonly Dictionary<int, AchievementLevel> Levels;
		public Achievement(uint Id, string GroupName, string Category)
		{
			this.Id = Id;
			this.GroupName = GroupName;
			this.Category = Category;
			this.Levels = new Dictionary<int, AchievementLevel>();
		}
		public void AddLevel(AchievementLevel Level)
		{
			this.Levels.Add(Level.Level, Level);
		}
	}
}
