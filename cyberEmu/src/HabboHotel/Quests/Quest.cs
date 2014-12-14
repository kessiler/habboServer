using System;
namespace Cyber.HabboHotel.Quests
{
	public class Quest
	{
		internal readonly uint Id;
		internal readonly string Category;
		internal readonly int Number;
		internal readonly QuestType GoalType;
		internal readonly uint GoalData;
		internal readonly string Name;
		internal readonly int Reward;
		internal readonly string DataBit;
		internal readonly int RewardType;
		internal readonly int TimeUnlock;
		internal readonly bool HasEnded;
		public string ActionName
		{
			get
			{
				return QuestTypeUtillity.GetString(this.GoalType);
			}
		}
		public Quest(uint Id, string Category, int Number, QuestType GoalType, uint GoalData, string Name, int Reward, string DataBit, int RewardType, int TimeUnlock, int TimeLock)
		{
			this.Id = Id;
			this.Category = Category;
			this.Number = Number;
			this.GoalType = GoalType;
			this.GoalData = GoalData;
			this.Name = Name;
			this.Reward = Reward;
			this.DataBit = DataBit;
			this.RewardType = RewardType;
			this.TimeUnlock = TimeUnlock;
			this.HasEnded = (TimeLock >= CyberEnvironment.GetUnixTimestamp() && TimeLock > 0);
		}
		public bool IsCompleted(int UserProgress)
		{
			QuestType goalType = this.GoalType;
			if (goalType != QuestType.EXPLORE_FIND_ITEM)
			{
				return (long)UserProgress >= (long)((ulong)this.GoalData);
			}
			return UserProgress >= 1;
		}
	}
}
