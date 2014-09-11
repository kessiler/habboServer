using Database_Manager.Database.Session_Details.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
namespace Cyber.HabboHotel.Achievements
{
	internal class AchievementLevelFactory
	{
		internal static void GetAchievementLevels(out Dictionary<string, Achievement> achievements, IQueryAdapter dbClient)
		{
			achievements = new Dictionary<string, Achievement>();
			dbClient.setQuery("SELECT * FROM achievements");
			DataTable table = dbClient.getTable();
			foreach (DataRow dataRow in table.Rows)
			{
				uint id = Convert.ToUInt32(dataRow["id"]);
				string category = (string)dataRow["category"];
				string text = (string)dataRow["group_name"];
				int level = (int)dataRow["level"];
				int rewardPixels = (int)dataRow["reward_pixels"];
				int rewardPoints = (int)dataRow["reward_points"];
				int requirement = (int)dataRow["progress_needed"];
				AchievementLevel level2 = new AchievementLevel(level, rewardPixels, rewardPoints, requirement);
				if (!achievements.ContainsKey(text))
				{
					Achievement achievement = new Achievement(id, text, category);
					achievement.AddLevel(level2);
					achievements.Add(text, achievement);
				}
				else
				{
					achievements[text].AddLevel(level2);
				}
			}
		}
	}
}
