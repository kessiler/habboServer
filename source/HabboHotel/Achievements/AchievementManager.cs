using Database_Manager.Database.Session_Details.Interfaces;
using Cyber.HabboHotel.Achievements.Composer;
using Cyber.HabboHotel.GameClients;
using Cyber.Messages;
using Cyber.Messages.Headers;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Cyber.HabboHotel.Achievements
{
	public class AchievementManager
	{
		internal Dictionary<string, Achievement> Achievements;


		internal AchievementManager(IQueryAdapter dbClient, out uint LoadedAchs)
		{
			this.Achievements = new Dictionary<string, Achievement>();
			this.LoadAchievements(dbClient);
            LoadedAchs = (uint)Achievements.Count;
		}
		internal void LoadAchievements(IQueryAdapter dbClient)
		{
            Achievements.Clear();
			AchievementLevelFactory.GetAchievementLevels(out this.Achievements, dbClient);
		}
		internal void GetList(GameClient Session, ClientMessage Message)
		{
			Session.SendMessage(AchievementListComposer.Compose(Session, this.Achievements.Values.ToList<Achievement>()));
		}
		internal List<Achievement> GetSnowAchievements()
		{
			List<Achievement> list = new List<Achievement>();
			foreach (Achievement current in this.Achievements.Values)
			{
				if (current.Category == "games")
				{
					list.Add(current);
				}
			}
			return list;
		}
		internal void TryProgressAchievement(GameClient Session, string ACHGroup, int MinimumRequirement, int Requirement)
		{
			if (Requirement == 0 || Session == null)
			{
				return;
			}
			Achievement achievement = null;
			this.Achievements.TryGetValue(ACHGroup, out achievement);
			if (achievement == null)
			{
				return;
			}
			UserAchievement achievementData = Session.GetHabbo().GetAchievementData(ACHGroup);
			if (achievementData == null)
			{
				this.ProgressUserAchievement(Session, ACHGroup, MinimumRequirement, false);
				return;
			}
			checked
			{
				int requirement = achievement.Levels[achievementData.Level + 1].Requirement;
				if (requirement < Requirement)
				{
					return;
				}
				this.ProgressUserAchievement(Session, ACHGroup, requirement - Requirement, false);
			}
		}
		internal bool ProgressUserAchievement(GameClient Session, string AchievementGroup, int ProgressAmount, bool FromZero = false)
		{
			if (!this.Achievements.ContainsKey(AchievementGroup) || Session == null)
			{
				return false;
			}
			Achievement achievement = null;
			achievement = this.Achievements[AchievementGroup];
			UserAchievement userAchievement = Session.GetHabbo().GetAchievementData(AchievementGroup);
			if (userAchievement == null)
			{
				userAchievement = new UserAchievement(AchievementGroup, 0, 0);
				Session.GetHabbo().Achievements.Add(AchievementGroup, userAchievement);
			}
			int count = achievement.Levels.Count;
			if (userAchievement != null && userAchievement.Level == count)
			{
				return false;
			}
			checked
			{
				int num = (userAchievement != null) ? (userAchievement.Level + 1) : 1;
				if (num > count)
				{
					num = count;
				}
				AchievementLevel targetLevelData = achievement.Levels[num];
				int num2 = 0;
				if (FromZero)
				{
					num2 = ProgressAmount;
				}
				else
				{
					num2 = ((userAchievement != null) ? (userAchievement.Progress + ProgressAmount) : ProgressAmount);
				}
				int num3 = (userAchievement != null) ? userAchievement.Level : 0;
				int num4 = num3 + 1;
				if (num4 > count)
				{
					num4 = count;
				}
				if (num2 >= targetLevelData.Requirement)
				{
					num3++;
					num4++;
					int arg_E6_0 = num2 - targetLevelData.Requirement;
					num2 = 0;
					if (num == 1)
					{
						Session.GetHabbo().GetBadgeComponent().GiveBadge(AchievementGroup + num, true, Session, false);
					}
					else
					{
						Session.GetHabbo().GetBadgeComponent().RemoveBadge(Convert.ToString(AchievementGroup + (num - 1)), Session);
						Session.GetHabbo().GetBadgeComponent().GiveBadge(AchievementGroup + num, true, Session, false);
					}
					if (num4 > count)
					{
						num4 = count;
					}
					Session.GetHabbo().ActivityPoints += targetLevelData.RewardPixels;
					Session.GetHabbo().NotifyNewPixels(targetLevelData.RewardPixels);
					Session.GetHabbo().UpdateActivityPointsBalance();
					Session.SendMessage(AchievementUnlockedComposer.Compose(achievement, num, targetLevelData.RewardPoints, targetLevelData.RewardPixels));
					using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
					{
						queryreactor.setQuery(string.Concat(new object[]
						{
							"REPLACE INTO user_achievement VALUES (",
							Session.GetHabbo().Id,
							", @group, ",
							num3,
							", ",
							num2,
							")"
						}));
						queryreactor.addParameter("group", AchievementGroup);
						queryreactor.runQuery();
					}
					userAchievement.Level = num3;
					userAchievement.Progress = num2;
					Session.GetHabbo().AchievementPoints += targetLevelData.RewardPoints;
					Session.GetHabbo().NotifyNewPixels(targetLevelData.RewardPixels);
					Session.GetHabbo().ActivityPoints += targetLevelData.RewardPixels;
					Session.GetHabbo().UpdateActivityPointsBalance();
					Session.SendMessage(AchievementScoreUpdateComposer.Compose(Session.GetHabbo().AchievementPoints));
					AchievementLevel targetLevelData2 = achievement.Levels[num4];
					Session.SendMessage(AchievementProgressComposer.Compose(achievement, num4, targetLevelData2, count, Session.GetHabbo().GetAchievementData(AchievementGroup)));
					Talent talent = null;
					if (CyberEnvironment.GetGame().GetTalentManager().TryGetTalent(AchievementGroup, out talent))
					{
						CyberEnvironment.GetGame().GetTalentManager().CompleteUserTalent(Session, talent);
					}
					return true;
				}
				userAchievement.Level = num3;
				userAchievement.Progress = num2;
				using (IQueryAdapter queryreactor2 = CyberEnvironment.GetDatabaseManager().getQueryReactor())
				{
					queryreactor2.setQuery(string.Concat(new object[]
					{
						"REPLACE INTO user_achievement VALUES (",
						Session.GetHabbo().Id,
						", @group, ",
						num3,
						", ",
						num2,
						")"
					}));
					queryreactor2.addParameter("group", AchievementGroup);
					queryreactor2.runQuery();
				}
				Session.SendMessage(AchievementProgressComposer.Compose(achievement, num, targetLevelData, count, Session.GetHabbo().GetAchievementData(AchievementGroup)));

                Session.GetMessageHandler().GetResponse().Init(Outgoing.UpdateUserDataMessageComposer);
                Session.GetMessageHandler().GetResponse().AppendInt32(-1);
                Session.GetMessageHandler().GetResponse().AppendString(Session.GetHabbo().Look);
                Session.GetMessageHandler().GetResponse().AppendString(Session.GetHabbo().Gender.ToLower());
                Session.GetMessageHandler().GetResponse().AppendString(Session.GetHabbo().Motto);
                Session.GetMessageHandler().GetResponse().AppendInt32(Session.GetHabbo().AchievementPoints);
                Session.GetMessageHandler().SendResponse();
                return false;
			}
		}
		internal Achievement GetAchievement(string AchievementGroup)
		{
			if (this.Achievements.ContainsKey(AchievementGroup))
			{
				return this.Achievements[AchievementGroup];
			}
			return null;
		}
	}
}
