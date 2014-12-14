using Database_Manager.Database.Session_Details.Interfaces;
using Cyber.HabboHotel.GameClients;
using Cyber.HabboHotel.Quests.Composer;
using Cyber.Messages;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
namespace Cyber.HabboHotel.Quests
{
	internal class QuestManager
	{
		private Dictionary<uint, Quest> quests;
		private Dictionary<string, int> questCount;
		public void Initialize(IQueryAdapter dbClient)
		{
			this.quests = new Dictionary<uint, Quest>();
			this.questCount = new Dictionary<string, int>();
			this.ReloadQuests(dbClient);
		}
		public void ReloadQuests(IQueryAdapter dbClient)
		{
			this.quests.Clear();
			dbClient.setQuery("SELECT * FROM quests");
			DataTable table = dbClient.getTable();
			foreach (DataRow dataRow in table.Rows)
			{
				uint num = Convert.ToUInt32(dataRow["id"]);
				string category = (string)dataRow["type"];
				int number = (int)dataRow["level_num"];
				int goalType = (int)dataRow["goal_type"];
				uint goalData = Convert.ToUInt32(dataRow["goal_data"]);
				string name = (string)dataRow["action"];
				int reward = (int)dataRow["pixel_reward"];
				string dataBit = (string)dataRow["data_bit"];
				int rewardType = Convert.ToInt32(dataRow["reward_type"].ToString());
				int timeUnlock = (int)dataRow["timestamp_unlock"];
				int timeLock = (int)dataRow["timestamp_lock"];
				Quest value = new Quest(num, category, number, (QuestType)goalType, goalData, name, reward, dataBit, rewardType, timeUnlock, timeLock);
				this.quests.Add(num, value);
				this.AddToCounter(category);
			}
		}
		private void AddToCounter(string category)
		{
			int num = 0;
			if (this.questCount.TryGetValue(category, out num))
			{
				this.questCount[category] = checked(num + 1);
				return;
			}
			this.questCount.Add(category, 1);
		}
		internal Quest GetQuest(uint Id)
		{
			Quest result = null;
			this.quests.TryGetValue(Id, out result);
			return result;
		}
		internal int GetAmountOfQuestsInCategory(string Category)
		{
			int result = 0;
			this.questCount.TryGetValue(Category, out result);
			return result;
		}
		internal void ProgressUserQuest(GameClient Session, QuestType QuestType, uint EventData = 0u)
		{
			if (Session == null || Session.GetHabbo() == null || Session.GetHabbo().CurrentQuestId <= 0u)
			{
				return;
			}
			Quest quest = this.GetQuest(Session.GetHabbo().CurrentQuestId);
			if (quest == null || quest.GoalType != QuestType)
			{
				return;
			}
			int questProgress = Session.GetHabbo().GetQuestProgress(quest.Id);
			int num = questProgress;
			bool flag = false;
			checked
			{
				if (QuestType != QuestType.EXPLORE_FIND_ITEM)
				{
					switch (QuestType)
					{
					case QuestType.STAND_ON:
						if (EventData != quest.GoalData)
						{
							return;
						}
						num = (int)quest.GoalData;
						flag = true;
						goto IL_DC;
					case QuestType.GIVE_ITEM:
						if (EventData != quest.GoalData)
						{
							return;
						}
						num = (int)quest.GoalData;
						flag = true;
						goto IL_DC;
					case QuestType.GIVE_COFFEE:
					case QuestType.WAVE_REINDEER:
						num++;
						if (unchecked((long)num >= (long)((ulong)quest.GoalData)))
						{
							flag = true;
							goto IL_DC;
						}
						goto IL_DC;
					case QuestType.XMAS_PARTY:
						num++;
						if (unchecked((long)num == (long)((ulong)quest.GoalData)))
						{
							flag = true;
							goto IL_DC;
						}
						goto IL_DC;
					}
				}
				if (EventData != quest.GoalData)
				{
					return;
				}
				num = (int)quest.GoalData;
				flag = true;
				IL_DC:
				using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
				{
					queryreactor.runFastQuery(string.Concat(new object[]
					{
						"UPDATE user_quests SET progress = ",
						num,
						" WHERE user_id = ",
						Session.GetHabbo().Id,
						" AND quest_id =  ",
						quest.Id
					}));
					if (flag)
					{
						queryreactor.runFastQuery("UPDATE user_stats SET quest_id = 0 WHERE id = " + Session.GetHabbo().Id);
					}
				}
				Session.GetHabbo().quests[Session.GetHabbo().CurrentQuestId] = num;
				Session.SendMessage(QuestStartedComposer.Compose(Session, quest));
				if (flag)
				{
					Session.GetHabbo().CurrentQuestId = 0u;
					Session.GetHabbo().LastQuestCompleted = quest.Id;
					Session.SendMessage(QuestCompletedComposer.Compose(Session, quest));
					Session.GetHabbo().ActivityPoints += quest.Reward;
					Session.GetHabbo().NotifyNewPixels(quest.Reward);
					Session.GetHabbo().UpdateSeasonalCurrencyBalance();
					this.GetList(Session, null);
				}
			}
		}
		internal Quest GetNextQuestInSeries(string Category, int Number)
		{
			foreach (Quest current in this.quests.Values)
			{
				if (current.Category == Category && current.Number == Number)
				{
					return current;
				}
			}
			return null;
		}
		internal List<Quest> GetSeasonalQuests(string Season)
		{
			List<Quest> list = new List<Quest>();
			foreach (Quest current in this.quests.Values)
			{
				if (current.Category.Contains(Season) && checked(current.TimeUnlock - CyberEnvironment.GetUnixTimestamp()) < 0)
				{
					list.Add(current);
				}
			}
			return list;
		}
		public ICollection<Quest> GetQuests()
		{
			return this.quests.Values;
		}
		internal void GetList(GameClient Session, ClientMessage Message)
		{
			Session.SendMessage(QuestListComposer.Compose(Session, this.quests.Values.ToList<Quest>(), Message != null));
		}
		internal void ActivateQuest(GameClient Session, ClientMessage Message)
		{
			Quest quest = this.GetQuest(Message.PopWiredUInt());
			if (quest == null)
			{
				return;
			}
			using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
			{
				queryreactor.runFastQuery(string.Concat(new object[]
				{
					"REPLACE INTO user_quests(user_id,quest_id) VALUES (",
					Session.GetHabbo().Id,
					", ",
					quest.Id,
					")"
				}));
				queryreactor.runFastQuery(string.Concat(new object[]
				{
					"UPDATE user_stats SET quest_id = ",
					quest.Id,
					" WHERE id = ",
					Session.GetHabbo().Id
				}));
			}
			Session.GetHabbo().CurrentQuestId = quest.Id;
			this.GetList(Session, null);
			Session.SendMessage(QuestStartedComposer.Compose(Session, quest));
		}
		internal void GetCurrentQuest(GameClient Session, ClientMessage Message)
		{
			if (!Session.GetHabbo().InRoom)
			{
				return;
			}
			Quest quest = this.GetQuest(Session.GetHabbo().LastQuestCompleted);
			Quest nextQuestInSeries = this.GetNextQuestInSeries(quest.Category, checked(quest.Number + 1));
			if (nextQuestInSeries == null)
			{
				return;
			}
			using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
			{
				queryreactor.runFastQuery(string.Concat(new object[]
				{
					"REPLACE INTO user_quests(user_id,quest_id) VALUES (",
					Session.GetHabbo().Id,
					", ",
					nextQuestInSeries.Id,
					")"
				}));
				queryreactor.runFastQuery(string.Concat(new object[]
				{
					"UPDATE user_stats SET quest_id = ",
					nextQuestInSeries.Id,
					" WHERE id = ",
					Session.GetHabbo().Id
				}));
			}
			Session.GetHabbo().CurrentQuestId = nextQuestInSeries.Id;
			this.GetList(Session, null);
			Session.SendMessage(QuestStartedComposer.Compose(Session, nextQuestInSeries));
		}
		internal void CancelQuest(GameClient Session, ClientMessage Message)
		{
			Quest quest = this.GetQuest(Session.GetHabbo().CurrentQuestId);
			if (quest == null)
			{
				return;
			}
			using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
			{
				queryreactor.runFastQuery(string.Concat(new object[]
				{
					"DELETE FROM user_quests WHERE user_id = ",
					Session.GetHabbo().Id,
					" AND quest_id = ",
					quest.Id,
					";UPDATE user_stats SET quest_id=0 WHERE id=",
					Session.GetHabbo().Id
				}));
			}
			Session.GetHabbo().CurrentQuestId = 0u;
			Session.SendMessage(QuestAbortedComposer.Compose());
			this.GetList(Session, null);
		}
	}
}
