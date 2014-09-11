using Cyber.HabboHotel.Groups;
using System;
using System.Collections.Generic;
using System.Data;
using Cyber.HabboHotel.Users.UserDataManagement;

namespace Cyber.HabboHotel.Users.Authenticator
{
	internal static class HabboFactory
	{
		internal static Habbo GenerateHabbo(DataRow dRow, DataRow mRow, HashSet<GroupUser> group)
		{
			uint id = uint.Parse(dRow["id"].ToString());
			string username = (string)dRow["username"];
			string realName = (string)dRow["real_name"];
			uint ras = uint.Parse(dRow["rank"].ToString());
			string motto = (string)dRow["motto"];
			string look = (string)dRow["look"];
			string gender = (string)dRow["gender"];
			int lastOnline = int.Parse(dRow["last_online"].ToString());
			int credits = (int)dRow["credits"];
			int activityPoints = (int)dRow["activity_points"];
			double lastActivityPointsUpdate = Convert.ToDouble(dRow["activity_points_lastupdate"]);
			bool muted = CyberEnvironment.EnumToBool(dRow["is_muted"].ToString());
			uint homeRoom = Convert.ToUInt32(dRow["home_room"]);
			int respect = (int)mRow["respect"];
			int dailyRespectPoints = (int)mRow["daily_respect_points"];
			int dailyPetRespectPoints = (int)mRow["daily_pet_respect_points"];
			bool hasFriendRequestsDisabled = CyberEnvironment.EnumToBool(dRow["block_newfriends"].ToString());
			bool appearOffline = CyberEnvironment.EnumToBool(dRow["hide_online"].ToString());
			bool hideInRoom = CyberEnvironment.EnumToBool(dRow["hide_inroom"].ToString());
			uint currentQuestID = Convert.ToUInt32(mRow["quest_id"]);
			int currentQuestProgress = (int)mRow["quest_progress"];
			int achievementPoints = (int)mRow["achievement_score"];
			bool vIP = CyberEnvironment.EnumToBool(dRow["vip"].ToString());
			double createDate = Convert.ToDouble(dRow["account_created"]);
			bool online = CyberEnvironment.EnumToBool(dRow["online"].ToString());
			string citizenship = dRow["talent_status"].ToString();
			int belCredits = int.Parse(dRow["seasonal_currency"].ToString());
			uint favId = uint.Parse(mRow["favourite_group"].ToString());
			int lastChange = (int)dRow["last_name_change"];
			int regTimestamp = int.Parse(dRow["account_created"].ToString());
			bool tradeLocked = CyberEnvironment.EnumToBool(dRow["trade_lock"].ToString());
			int tradeLockExpire = int.Parse(dRow["trade_lock_expire"].ToString());
            bool NuxPassed = CyberEnvironment.EnumToBool(dRow["nux_passed"].ToString());

			return new Habbo(id, username, realName, ras, motto, look, gender, credits, activityPoints, lastActivityPointsUpdate, muted, homeRoom, respect, dailyRespectPoints, dailyPetRespectPoints, hasFriendRequestsDisabled, currentQuestID, currentQuestProgress, achievementPoints, regTimestamp, lastOnline, appearOffline, hideInRoom, vIP, createDate, online, citizenship, belCredits, group, favId, lastChange, tradeLocked, tradeLockExpire, NuxPassed);
		
            
        }
	}
}
