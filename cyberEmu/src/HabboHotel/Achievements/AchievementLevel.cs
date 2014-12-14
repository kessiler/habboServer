using System;
namespace Cyber.HabboHotel.Achievements
{
	internal struct AchievementLevel
	{
		internal readonly int Level;
		internal readonly int RewardPixels;
		internal readonly int RewardPoints;
		internal readonly int Requirement;
		public AchievementLevel(int level, int rewardPixels, int rewardPoints, int requirement)
		{
			this.Level = level;
			this.RewardPixels = rewardPixels;
			this.RewardPoints = rewardPoints;
			this.Requirement = requirement;
		}
	}
}
