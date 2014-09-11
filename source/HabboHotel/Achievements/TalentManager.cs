using Database_Manager.Database.Session_Details.Interfaces;
using Cyber.HabboHotel.GameClients;
using Cyber.HabboHotel.Items;
using Cyber.Messages;
using Cyber.Messages.Headers;
using System;
using System.Collections.Generic;
using System.Data;
namespace Cyber.HabboHotel.Achievements
{
	internal class TalentManager
	{
		private Dictionary<int, Talent> Talents;
		internal TalentManager()
		{
			this.Talents = new Dictionary<int, Talent>();
		}
		internal void Initialize(IQueryAdapter dbClient)
		{
			dbClient.setQuery("SELECT * FROM achievements_talenttrack ORDER BY `order_num` ASC");
			DataTable table = dbClient.getTable();
			foreach (DataRow dataRow in table.Rows)
			{
				Talent talent = new Talent((int)dataRow["id"], (string)dataRow["type"], (int)dataRow["parent_category"], (int)dataRow["level"], (string)dataRow["achievement_group"], (int)dataRow["achievement_level"], (string)dataRow["prize"], (uint)dataRow["prize_baseitem"]);
				this.Talents.Add(talent.Id, talent);
			}
		}
		internal Talent GetTalent(int TalentId)
		{
			return this.Talents[TalentId];
		}
		internal bool LevelIsCompleted(GameClient Session, int TalentLevel)
		{
			foreach (Talent current in this.GetTalents(TalentLevel))
			{
				if (Session.GetHabbo().GetAchievementData(current.AchievementGroup) == null)
				{
					return false;
				}
			}
			return true;
		}
		internal void CompleteUserTalent(GameClient Session, Talent Talent)
		{
			if (Session == null || Session.GetHabbo() == null || Session.GetHabbo().CurrentTalentLevel < Talent.Level)
			{
				return;
			}
			if (!this.LevelIsCompleted(Session, Talent.Level))
			{
				return;
			}
			if (Talent.Prize != "" && Talent.PrizeBaseItem > 0u)
			{
				Item item = CyberEnvironment.GetGame().GetItemManager().GetItem(Talent.PrizeBaseItem);
				CyberEnvironment.GetGame().GetCatalog().DeliverItems(Session, item, 1, "", 0, 0, "");
			}
			UserTalent value = new UserTalent(Talent.Id, 1);
			Session.GetHabbo().Talents.Add(Talent.Id, value);
			using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
			{
				queryreactor.runFastQuery(string.Concat(new object[]
				{
					"REPLACE INTO user_talents VALUES (",
					Session.GetHabbo().Id,
					", ",
					Talent.Id,
					", ",
					1,
					");"
				}));
				queryreactor.runFastQuery(string.Concat(new object[]
				{
					"UPDATE users SET talent_status = '",
					Talent.Type,
					"' WHERE id = ",
					Session.GetHabbo().Id,
					";"
				}));
			}
			ServerMessage serverMessage = new ServerMessage(Outgoing.TalentLevelUpMessageComposer);
			serverMessage.AppendString(Session.GetHabbo().TalentStatus);
			serverMessage.AppendInt32(Talent.Level);
			serverMessage.AppendInt32(0);
			serverMessage.AppendInt32(1);
			serverMessage.AppendString(Talent.Prize);
			serverMessage.AppendInt32(0);
			Session.SendMessage(serverMessage);
		}
		internal bool TryGetTalent(string AchGroup, out Talent talent)
		{
			foreach (Talent current in this.Talents.Values)
			{
				if (current.AchievementGroup == AchGroup)
				{
					talent = current;
					return true;
				}
			}
			talent = null;
			return false;
		}
		internal Dictionary<int, Talent> GetAllTalents()
		{
			return this.Talents;
		}
		internal List<Talent> GetTalents(int ParentCategory)
		{
			List<Talent> list = new List<Talent>();
			foreach (Talent current in this.Talents.Values)
			{
				if (current.ParentCategory == ParentCategory && current.GetAchievement() != null)
				{
					list.Add(current);
				}
			}
			return list;
		}
	}
}
