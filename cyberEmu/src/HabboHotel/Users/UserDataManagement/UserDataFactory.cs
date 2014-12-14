using Database_Manager.Database.Session_Details.Interfaces;
using Cyber.HabboHotel.Achievements;
using Cyber.HabboHotel.Catalogs;
using Cyber.HabboHotel.Groups;
using Cyber.HabboHotel.Items;
using Cyber.HabboHotel.Pets;
using Cyber.HabboHotel.Polls;
using Cyber.HabboHotel.RoomBots;
using Cyber.HabboHotel.Rooms;
using Cyber.HabboHotel.Users.Authenticator;
using Cyber.HabboHotel.Users.Badges;
using Cyber.HabboHotel.Users.Inventory;
using Cyber.HabboHotel.Users.Messenger;
using Cyber.HabboHotel.Users.Relationships;
using Cyber.HabboHotel.Users.Subscriptions;
using System;
using System.Collections.Generic;
using System.Data;
namespace Cyber.HabboHotel.Users.UserDataManagement
{
	internal class UserDataFactory
	{
		internal static UserData GetUserData(string sessionTicket, string ip, out byte errorCode)
		{
			DataTable dataTable = null;
			DataRow dataRow;
			uint Userid;
			DataTable dataTable2;
			DataTable table;
			DataRow row;
			DataTable dataTable3;
			DataTable dataTable4;
			DataTable dataTable5;
			DataRow dataRow2;
			DataTable dataTable6;
			DataTable dataTable7;
			DataTable dataTable8;
			DataTable dataTable9;
			DataTable dataTable10;
			DataTable dataTable11;
			DataTable dataTable12;
			DataTable dataTable13;
			DataTable table2;
			DataTable dataTable14;
			DataTable dataTable15;
           
			using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
			{
				
				queryreactor.setQuery("SELECT * FROM users WHERE auth_ticket = @sso ");
				queryreactor.addParameter("sso", sessionTicket);
				queryreactor.addParameter("ipaddress", ip);
				dataRow = queryreactor.getRow();
				if (dataRow == null)
				{
					errorCode = 1;
					throw new UserDataNotFoundException(string.Format("No user found with ip {0} and sso {1}.", ip, sessionTicket));
				}
				Userid = Convert.ToUInt32(dataRow["id"]);
				queryreactor.runFastQuery("UPDATE users SET online='1' WHERE id=" + Userid + " LIMIT 1");
				if (CyberEnvironment.GetGame().GetClientManager().GetClientByUserID(Userid) != null)
				{
					errorCode = 2;
					CyberEnvironment.GetGame().GetClientManager().GetClientByUserID(Userid).Disconnect();
					return null;
				}
				queryreactor.setQuery("SELECT * FROM user_achievement WHERE userid = " + Userid);
				dataTable2 = queryreactor.getTable();
				queryreactor.setQuery("SELECT * FROM user_talents WHERE userid = " + Userid);
				table = queryreactor.getTable();
				queryreactor.setQuery("SELECT COUNT(*) FROM user_stats WHERE id=" + Userid);
				if (int.Parse(queryreactor.getString()) == 0)
				{
					queryreactor.runFastQuery("INSERT INTO user_stats (id) VALUES (" + Userid + ");");
				}
				queryreactor.setQuery("SELECT * FROM user_stats WHERE id=" + Userid + " LIMIT 1");
				row = queryreactor.getRow();
				queryreactor.setQuery("SELECT room_id FROM user_favorites WHERE user_id = " + Userid);
				dataTable3 = queryreactor.getTable();
				queryreactor.setQuery("SELECT ignore_id FROM user_ignores WHERE user_id = " + Userid);
				dataTable4 = queryreactor.getTable();
				queryreactor.setQuery("SELECT tag FROM user_tags WHERE user_id = " + Userid);
				dataTable5 = queryreactor.getTable();
				queryreactor.setQuery("SELECT * FROM user_subscriptions WHERE user_id = " + Userid + " AND timestamp_expire > UNIX_TIMESTAMP() ORDER BY subscription_id DESC LIMIT 1");
				dataRow2 = queryreactor.getRow();
				queryreactor.setQuery("SELECT * FROM user_badges WHERE user_id = " + Userid);
				dataTable6 = queryreactor.getTable();
				queryreactor.setQuery("SELECT `items`.* , COALESCE(`items_groups`.`group_id`, 0) AS group_id FROM `items` LEFT OUTER JOIN `items_groups` ON `items`.`id` = `items_groups`.`id` WHERE room_id='0' AND user_id=" + Userid + " LIMIT 8000");
				dataTable7 = queryreactor.getTable();
				queryreactor.setQuery("SELECT * FROM user_effects WHERE user_id =  " + Userid);
				dataTable8 = queryreactor.getTable();
				queryreactor.setQuery("SELECT poll_id FROM user_polldata WHERE user_id = " + Userid + " GROUP BY poll_id;");
				dataTable9 = queryreactor.getTable();
				queryreactor.setQuery(string.Concat(new object[]
				{
					"SELECT users.id,users.username,users.motto,users.look,users.last_online,users.hide_inroom,users.hide_online FROM users JOIN messenger_friendships ON users.id = messenger_friendships.user_one_id WHERE messenger_friendships.user_two_id = ",
					Userid,
					" UNION ALL SELECT users.id,users.username,users.motto,users.look,users.last_online,users.hide_inroom,users.hide_online FROM users JOIN messenger_friendships ON users.id = messenger_friendships.user_two_id WHERE messenger_friendships.user_one_id = ",
					Userid
				}));
				dataTable10 = queryreactor.getTable();
				queryreactor.setQuery("SELECT * FROM user_stats WHERE id=" + Userid + " LIMIT 1");
				row = queryreactor.getRow();
				queryreactor.setQuery("SELECT messenger_requests.from_id,messenger_requests.to_id,users.username FROM users JOIN messenger_requests ON users.id = messenger_requests.from_id WHERE messenger_requests.to_id = " + Userid);
				dataTable11 = queryreactor.getTable();
				queryreactor.setQuery("SELECT * FROM rooms WHERE owner = @name LIMIT 150");
				queryreactor.addParameter("name", (string)dataRow["username"]);
				dataTable12 = queryreactor.getTable();
				queryreactor.setQuery("SELECT * FROM bots WHERE user_id = " + Userid + " AND room_id = 0 AND ai_type='pet'");
				dataTable13 = queryreactor.getTable();
				queryreactor.setQuery("SELECT * FROM user_quests WHERE user_id = " + Userid);
				table2 = queryreactor.getTable();
				queryreactor.setQuery("SELECT * FROM bots WHERE user_id=" + Userid + " AND room_id=0 AND ai_type='generic'");
				dataTable14 = queryreactor.getTable();
				queryreactor.setQuery("SELECT group_id,rank FROM group_memberships WHERE user_id=" + Userid);
				dataTable = queryreactor.getTable();
				queryreactor.setQuery(string.Concat(new object[]
				{
					"UPDATE user_info SET login_timestamp = '",
					CyberEnvironment.GetUnixTimestamp(),
					"' WHERE user_id = ",
					Userid,
					" ; "
				}));
				queryreactor.addParameter("ip", ip);
				queryreactor.runQuery();
				queryreactor.setQuery("SELECT * FROM user_relationships WHERE user_id=@id");
				queryreactor.addParameter("id", Userid);
				dataTable15 = queryreactor.getTable();
				queryreactor.runFastQuery("UPDATE users SET online='1' WHERE id=" + Userid + " LIMIT 1");
			}
			Dictionary<string, UserAchievement> dictionary = new Dictionary<string, UserAchievement>();
			foreach (DataRow dataRow3 in dataTable2.Rows)
			{
				string text = (string)dataRow3["group"];
				int level = (int)dataRow3["level"];
				int progress = (int)dataRow3["progress"];
				UserAchievement value = new UserAchievement(text, level, progress);
				dictionary.Add(text, value);
			}
			Dictionary<int, UserTalent> dictionary2 = new Dictionary<int, UserTalent>();
			foreach (DataRow dataRow4 in table.Rows)
			{
				int num2 = (int)dataRow4["talent_id"];
				int state = (int)dataRow4["talent_state"];
				UserTalent value2 = new UserTalent(num2, state);
				dictionary2.Add(num2, value2);
			}
			List<uint> list = new List<uint>();
			foreach (DataRow dataRow5 in dataTable3.Rows)
			{
				uint item = Convert.ToUInt32(dataRow5["room_id"]);
				list.Add(item);
			}
			List<uint> list2 = new List<uint>();
			foreach (DataRow dataRow6 in dataTable4.Rows)
			{
				uint item2 = Convert.ToUInt32(dataRow6["ignore_id"]);
				list2.Add(item2);
			}
			List<string> list3 = new List<string>();
			foreach (DataRow dataRow7 in dataTable5.Rows)
			{
				string item3 = dataRow7["tag"].ToString().Replace(" ", "");
				list3.Add(item3);
			}
			Subscription sub = null;
			if (dataRow2 != null)
			{
				sub = new Subscription((int)dataRow2["subscription_id"], (int)dataRow2["timestamp_activated"], (int)dataRow2["timestamp_expire"], (int)dataRow2["timestamp_lastgift"]);
			}
			Dictionary<uint, RoomBot> dictionary3 = new Dictionary<uint, RoomBot>();
			foreach (DataRow row2 in dataTable14.Rows)
			{
				RoomBot roomBot = BotManager.GenerateBotFromRow(row2);
				dictionary3.Add(roomBot.BotId, roomBot);
			}
			List<Badge> list4 = new List<Badge>();
			foreach (DataRow dataRow8 in dataTable6.Rows)
			{
				string code = (string)dataRow8["badge_id"];
				int slot = (int)dataRow8["badge_slot"];
				list4.Add(new Badge(code, slot));
			}

            int miniMailCount = 0;

            try
            {
                DataRow Rowi;
                using (IQueryAdapter dbClient = CyberEnvironment.GetDatabaseManager().getQueryReactor())
                {
                    dbClient.setQuery("SELECT Count( IsReaded ) FROM xdrcms_minimail WHERE InBin = 0 AND IsReaded = 0 AND SenderId != " + Userid + " AND OwnerId = " + Userid);
                    Rowi = dbClient.getRow();
                }
                if (Rowi != null)
                {
                    // We are using aXDR CMS with MiniMail!
                    miniMailCount = int.Parse(Rowi[0].ToString());
                }
            }
            catch { }

			List<UserItem> list5 = new List<UserItem>();
			foreach (DataRow dataRow9 in dataTable7.Rows)
			{
				uint id = Convert.ToUInt32(dataRow9[0]);
				uint baseItem = Convert.ToUInt32(dataRow9[3]);
				string extraData;
				if (!DBNull.Value.Equals(dataRow9[4]))
				{
					extraData = (string)dataRow9[4];
				}
				else
				{
					extraData = string.Empty;
				}
				uint group = Convert.ToUInt32(dataRow9["group_id"]);
				string songCode = (string)dataRow9["songcode"];
				list5.Add(new UserItem(id, baseItem, extraData, group, songCode));
			}
			List<AvatarEffect> list6 = new List<AvatarEffect>();
			foreach (DataRow dataRow10 in dataTable8.Rows)
			{
				int effectId = (int)dataRow10["effect_id"];
				int totalDuration = (int)dataRow10["total_duration"];
				bool activated = CyberEnvironment.EnumToBool((string)dataRow10["is_activated"]);
				double activateTimestamp = (double)dataRow10["activated_stamp"];
				list6.Add(new AvatarEffect(effectId, totalDuration, activated, activateTimestamp));
			}

			HashSet<uint> pollSuggested = new HashSet<uint>();
			foreach (DataRow Row in dataTable9.Rows)
			{
				uint pId = (uint)Row["poll_id"];
                pollSuggested.Add(pId);
			}

			Dictionary<uint, MessengerBuddy> dictionary4 = new Dictionary<uint, MessengerBuddy>();
			string arg_A3D_0 = (string)dataRow["username"];
			int num3 = checked(dataTable10.Rows.Count - 700);
			if (num3 > 0)
			{
				using (IQueryAdapter queryreactor2 = CyberEnvironment.GetDatabaseManager().getQueryReactor())
				{
					queryreactor2.runFastQuery(string.Concat(new object[]
					{
						"DELETE FROM messenger_friendships WHERE user_one_id=",
						Userid,
						" OR user_two_id=",
						Userid,
						" LIMIT ",
						num3
					}));
					queryreactor2.setQuery(string.Concat(new object[]
					{
						"SELECT users.id,users.username,users.motto,users.look,users.last_online,users.hide_inroom,users.hide_online FROM users JOIN messenger_friendships ON users.id = messenger_friendships.user_one_id WHERE messenger_friendships.user_two_id = ",
						Userid,
						" UNION ALL SELECT users.id,users.username,users.motto,users.look,users.last_online,users.hide_inroom,users.hide_online FROM users JOIN messenger_friendships ON users.id = messenger_friendships.user_two_id WHERE messenger_friendships.user_one_id = ",
						Userid
					}));
					dataTable10 = queryreactor2.getTable();
				}
			}
			foreach (DataRow dataRow12 in dataTable10.Rows)
			{
				uint num4 = Convert.ToUInt32(dataRow12["id"]);
				string pUsername = (string)dataRow12["username"];
				string pLook = (string)dataRow12["look"];
				string pMotto = (string)dataRow12["motto"];
				int pLastOnline = Convert.ToInt32(dataRow12["last_online"]);
				bool pAppearOffline = CyberEnvironment.EnumToBool(dataRow12["hide_online"].ToString());
				bool pHideInroom = CyberEnvironment.EnumToBool(dataRow12["hide_inroom"].ToString());
				if (num4 != Userid && !dictionary4.ContainsKey(num4))
				{
					dictionary4.Add(num4, new MessengerBuddy(num4, pUsername, pLook, pMotto, pLastOnline, pAppearOffline, pHideInroom));
				}
			}
			Dictionary<uint, MessengerRequest> dictionary5 = new Dictionary<uint, MessengerRequest>();
			foreach (DataRow dataRow13 in dataTable11.Rows)
			{
				uint num5 = Convert.ToUInt32(dataRow13["from_id"]);
				uint num6 = Convert.ToUInt32(dataRow13["to_id"]);
				string pUsername2 = (string)dataRow13["username"];
				if (num5 != Userid)
				{
					if (!dictionary5.ContainsKey(num5))
					{
						dictionary5.Add(num5, new MessengerRequest(Userid, num5, pUsername2));
					}
				}
				else
				{
					if (!dictionary5.ContainsKey(num6))
					{
						dictionary5.Add(num6, new MessengerRequest(Userid, num6, pUsername2));
					}
				}
			}
			HashSet<RoomData> list8 = new HashSet<RoomData>();
			foreach (DataRow dataRow14 in dataTable12.Rows)
			{
				uint roomId = Convert.ToUInt32(dataRow14["id"]);
				list8.Add(CyberEnvironment.GetGame().GetRoomManager().FetchRoomData(roomId, dataRow14));
			}
			Dictionary<uint, Pet> dictionary6 = new Dictionary<uint, Pet>();
			foreach (DataRow dataRow15 in dataTable13.Rows)
			{
				using (IQueryAdapter queryreactor3 = CyberEnvironment.GetDatabaseManager().getQueryReactor())
				{
					queryreactor3.setQuery("SELECT * FROM bots_petdata WHERE id=" + dataRow15[0] + " LIMIT 1");
					DataRow row3 = queryreactor3.getRow();
					if (row3 != null)
					{
						Pet pet = Catalog.GeneratePetFromRow(dataRow15, row3);
						dictionary6.Add(pet.PetId, pet);
					}
				}
			}
			Dictionary<uint, int> dictionary7 = new Dictionary<uint, int>();
			foreach (DataRow dataRow16 in table2.Rows)
			{
				uint key = Convert.ToUInt32(dataRow16["quest_id"]);
				int value3 = (int)dataRow16["progress"];
				if (dictionary7.ContainsKey(key))
				{
					dictionary7.Remove(key);
				}
				dictionary7.Add(key, value3);
			}
			HashSet<GroupUser> list9 = new HashSet<GroupUser>();
			foreach (DataRow dataRow17 in dataTable.Rows)
			{
				list9.Add(new GroupUser(Userid, (uint)dataRow17[0], (int)Convert.ToInt16(dataRow17[1])));
			}
			Dictionary<int, Relationship> dictionary8 = new Dictionary<int, Relationship>();
			foreach (DataRow dataRow18 in dataTable15.Rows)
			{
				dictionary8.Add((int)dataRow18[0], new Relationship((int)dataRow18[0], (int)dataRow18[2], Convert.ToInt32(dataRow18[3].ToString())));
			}
			Habbo user = HabboFactory.GenerateHabbo(dataRow, row, list9);
			dataRow = null;
			dataTable2 = null;
			dataTable3 = null;
			dataTable4 = null;
			dataTable5 = null;
			dataRow2 = null;
			dataTable6 = null;
			dataTable7 = null;
			dataTable8 = null;
			dataTable10 = null;
			dataTable11 = null;
			dataTable12 = null;
			dataTable13 = null;
			dataTable14 = null;
			dataTable15 = null;
			dataTable9 = null;
			errorCode = 0;
			return new UserData(Userid, dictionary, dictionary2, list, list2, list3, sub, list4, list5, list6, dictionary4, dictionary5, list8, dictionary6, dictionary7, user, dictionary3, dictionary8, pollSuggested, miniMailCount);
		}
		internal static UserData GetUserData(int UserId)
		{
			DataRow dataRow;
			uint num;
			DataRow row;
			DataTable table;
			using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
			{
				queryreactor.setQuery("SELECT users.* FROM users WHERE users.id = @id");
				queryreactor.addParameter("id", UserId);
				dataRow = queryreactor.getRow();
				CyberEnvironment.GetGame().GetClientManager().LogClonesOut(Convert.ToUInt32(UserId));
				if (dataRow == null)
				{
					UserData result = null;
					return result;
				}
				num = Convert.ToUInt32(dataRow["id"]);
				if (CyberEnvironment.GetGame().GetClientManager().GetClientByUserID(num) != null)
				{
					UserData result = null;
					return result;
				}
				queryreactor.setQuery("SELECT group_id,rank FROM group_memberships WHERE user_id=" + UserId);
				queryreactor.getTable();
				queryreactor.setQuery("SELECT * FROM user_stats WHERE id=" + num + " LIMIT 1");
				row = queryreactor.getRow();
				queryreactor.setQuery("SELECT * FROM user_relationships WHERE user_id=@id");
				queryreactor.addParameter("id", num);
				table = queryreactor.getTable();
			}
			Dictionary<string, UserAchievement> achievements = new Dictionary<string, UserAchievement>();
			Dictionary<int, UserTalent> talents = new Dictionary<int, UserTalent>();
			List<uint> favouritedRooms = new List<uint>();
			List<uint> ignores = new List<uint>();
			List<string> tags = new List<string>();
			List<Badge> badges = new List<Badge>();
			List<UserItem> inventory = new List<UserItem>();
			List<AvatarEffect> effects = new List<AvatarEffect>();
			Dictionary<uint, MessengerBuddy> friends = new Dictionary<uint, MessengerBuddy>();
			Dictionary<uint, MessengerRequest> requests = new Dictionary<uint, MessengerRequest>();
			HashSet<RoomData> rooms = new HashSet<RoomData>();
			Dictionary<uint, Pet> pets = new Dictionary<uint, Pet>();
			Dictionary<uint, int> quests = new Dictionary<uint, int>();
			Dictionary<uint, RoomBot> bots = new Dictionary<uint, RoomBot>();
			HashSet<GroupUser> group = new HashSet<GroupUser>();
			HashSet<uint> pollData = new HashSet<uint>();
			Dictionary<int, Relationship> dictionary = new Dictionary<int, Relationship>();
			foreach (DataRow dataRow2 in table.Rows)
			{
				dictionary.Add((int)dataRow2[0], new Relationship((int)dataRow2[0], (int)dataRow2[2], Convert.ToInt32(dataRow2[3].ToString())));
			}
			Habbo user = HabboFactory.GenerateHabbo(dataRow, row, group);
			dataRow = null;
            
			return new UserData(num, achievements, talents, favouritedRooms, ignores, tags, null, badges, inventory, effects, friends, requests, rooms, pets, quests, user, bots, dictionary, pollData, 0);
		}
	}
}
