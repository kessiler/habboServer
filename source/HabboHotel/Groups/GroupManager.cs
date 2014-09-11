using Database_Manager.Database.Session_Details.Interfaces;
using Cyber.HabboHotel.GameClients;
using Cyber.HabboHotel.Rooms;
using Cyber.HabboHotel.Users;
using Cyber.Messages;
using Cyber.Messages.Headers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Collections;
using System.Linq;
using System.Collections.Specialized;

namespace Cyber.HabboHotel.Groups
{
	internal class GroupManager
	{
		internal HashSet<GroupBases> Bases;
        internal HashSet<GroupSymbols> Symbols;
        internal HashSet<GroupBaseColours> BaseColours;
		internal HybridDictionary SymbolColours;
		internal HybridDictionary BackGroundColours;
		internal HybridDictionary Groups;
        internal void InitGroups(IQueryAdapter dbClient)
		{
			this.Bases = new HashSet<GroupBases>();
			this.Symbols = new HashSet<GroupSymbols>();
			this.BaseColours = new HashSet<GroupBaseColours>();
            this.SymbolColours = new HybridDictionary();
            this.BackGroundColours = new HybridDictionary();
            this.Groups = new HybridDictionary();
			this.ClearInfo();

            dbClient.setQuery("SELECT * FROM group_badgeparts");
            DataTable table = dbClient.getTable();

            if (table == null) { return; }
            foreach (DataRow row in table.Rows)
            {
                switch (row["type"].ToString().ToLower())
                {
                    case "base":
                        this.Bases.Add(new GroupBases(int.Parse(row["id"].ToString()), row["code"].ToString(), row["code2"].ToString()));
                        break;

                    case "symbol":
                        this.Symbols.Add(new GroupSymbols(int.Parse(row["id"].ToString()), row["code"].ToString(), row["code2"].ToString()));
                        break;

                    case "base_color":
                        this.BaseColours.Add(new GroupBaseColours(int.Parse(row["id"].ToString()), row["code"].ToString()));
                        break;

                    case "symbol_color":
                        this.SymbolColours.Add(int.Parse(row["id"].ToString()), new GroupSymbolColours(int.Parse(row["id"].ToString()), row["code"].ToString()));
                        break;

                    case "other_color":
                        this.BackGroundColours.Add(int.Parse(row["id"].ToString()), new GroupBackGroundColours(int.Parse(row["id"].ToString()), row["code"].ToString()));
                        break;
                }
            }
		}
		internal void ClearInfo()
		{
			this.Bases.Clear();
			this.Symbols.Clear();
			this.BaseColours.Clear();
			this.SymbolColours.Clear();
			this.BackGroundColours.Clear();
		}
		internal void CreateGroup(string Name, string Desc, uint RoomId, string Badge, GameClient Session, int Colour1, int Colour2, out Guild Group)
		{
			using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
			{
				queryreactor.setQuery(string.Concat(new object[]
				{
					"INSERT INTO groups (`name`, `desc`,`badge`,`owner_id`,`created`,`room_id`,`colour1`,`colour2`) VALUES(@name, @desc, @badge, ",
					Session.GetHabbo().Id,
					", UNIX_TIMESTAMP(), ",
					RoomId,
					",'",
					Colour1,
					"','",
					Colour2,
					"')"
				}));
				queryreactor.addParameter("name", Name);
				queryreactor.addParameter("desc", Desc);
				queryreactor.addParameter("badge", Badge);
				uint num = checked((uint)queryreactor.insertQuery());
				queryreactor.runFastQuery(string.Concat(new object[]
				{
					"UPDATE rooms SET group_id=",
					num,
					" WHERE id=",
					RoomId,
					" LIMIT 1"
				}));
				Dictionary<uint, GroupUser> dictionary = new Dictionary<uint, GroupUser>();
				dictionary.Add(Session.GetHabbo().Id, new GroupUser(Session.GetHabbo().Id, num, 2));
				this.Groups.Add(num, new Guild(num, Name, Desc, RoomId, Badge, CyberEnvironment.GetUnixTimestamp(), Session.GetHabbo().Id, Colour1, Colour2, dictionary, new List<uint>(), new Dictionary<uint, GroupUser>(), 0u, 1u, false, Name, Desc, 0, 0.0, 0, "", 0));

				queryreactor.runFastQuery(string.Concat(new object[]
				{
					"INSERT INTO group_memberships (group_id, user_id, rank) VALUES (",
					num,
					", ",
					Session.GetHabbo().Id,
					", '2')"
				}));
				Group = this.GetGroup(num);
                GroupUser User = new GroupUser(Session.GetHabbo().Id, num, 2);
                Session.GetHabbo().UserGroups.Add(User);
                Group.Admins.Add(Session.GetHabbo().Id, User);
				queryreactor.runFastQuery(string.Concat(new object[]
				{
					"UPDATE user_stats SET favourite_group=",
					num,
					" WHERE id= ",
					Session.GetHabbo().Id,
					" LIMIT 1"
				}));
				queryreactor.runFastQuery("DELETE FROM room_rights WHERE room_id=" + RoomId);
			}
		}
		internal Guild GetGroup(uint GroupId)
		{
			if (this.Groups != null)
			{
                if (this.Groups.Contains(GroupId))
				{
					return (Guild)this.Groups[GroupId];
				}
				using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
				{
					queryreactor.setQuery("SELECT * FROM groups WHERE id=" + GroupId + " LIMIT 1");
					DataRow row = queryreactor.getRow();
					Guild result;
					if (row == null)
					{
						result = null;
						return result;
					}
					queryreactor.setQuery("SELECT user_id, rank FROM group_memberships WHERE group_id=" + GroupId);
					DataTable table = queryreactor.getTable();
					queryreactor.setQuery("SELECT user_id FROM group_requests WHERE group_id=" + GroupId);
					DataTable table2 = queryreactor.getTable();
					Dictionary<uint, GroupUser> dictionary = new Dictionary<uint, GroupUser>();
					Dictionary<uint, GroupUser> dictionary2 = new Dictionary<uint, GroupUser>();
					List<uint> list = new List<uint>();
					foreach (DataRow dataRow in table.Rows)
					{
						dictionary.Add((uint)dataRow[0], new GroupUser((uint)dataRow[0], GroupId, int.Parse(dataRow[1].ToString())));
						if (int.Parse(dataRow[1].ToString()) >= 1)
						{
							dictionary2.Add((uint)dataRow[0], new GroupUser((uint)dataRow[0], GroupId, int.Parse(dataRow[1].ToString())));
						}
					}
					foreach (DataRow dataRow2 in table2.Rows)
					{
						list.Add((uint)dataRow2[0]);
					}
					Guild group = new Guild((uint)row[0], row[1].ToString(), row[2].ToString(), (uint)row[6], row[3].ToString(), (int)row[5], (uint)row[4], (int)row[8], (int)row[9], dictionary, list, dictionary2, (uint)Convert.ToUInt16(row[7]), (uint)Convert.ToUInt16(row[10]), row["has_forum"].ToString() == "1", row["forum_name"].ToString(), row["forum_description"].ToString(), uint.Parse(row["forum_messages_count"].ToString()), double.Parse(row["forum_score"].ToString()), uint.Parse(row["forum_lastposter_id"].ToString()), row["forum_lastposter_name"].ToString(), int.Parse(row["forum_lastposter_timestamp"].ToString()));
					this.Groups.Add((uint)row[0], group);
					result = group;
					return result;
				}
			}
			return null;
		}
		internal HashSet<GroupUser> GetUserGroups(uint UserId)
		{
            HashSet<GroupUser> list = new HashSet<GroupUser>();
			using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
			{
				queryreactor.setQuery("SELECT group_id, rank FROM group_memberships WHERE user_id=" + UserId);
				DataTable table = queryreactor.getTable();
				foreach (DataRow dataRow in table.Rows)
				{
					list.Add(new GroupUser(UserId, (uint)dataRow[0], (int)Convert.ToInt16(dataRow[1])));
				}
			}
			return list;
		}
		internal ServerMessage SerializeGroupMembers(ServerMessage Response, Guild Group, uint ReqType, GameClient Session, string SearchVal = "", int Page = 0)
		{
			if (Group == null || Session == null)
			{
				return null;
			}
			if (Page < 1)
			{
				Page = 0;
			}
			List<List<GroupUser>> list = GroupManager.Split(this.GetGroupUsersByString(Group, SearchVal, ReqType));
			Response.AppendUInt(Group.Id);
			Response.AppendString(Group.Name);
			Response.AppendUInt(Group.RoomId);
			Response.AppendString(Group.Badge);
			switch (ReqType)
			{
			case 0u:
				Response.AppendInt32(Group.Members.Count);
				Response.AppendInt32(list[Page].Count);
				using (List<GroupUser>.Enumerator enumerator = list[Page].GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						GroupUser current = enumerator.Current;
						Habbo habboForId = CyberEnvironment.getHabboForId(current.Id);
						if (habboForId == null)
						{
							Response.AppendInt32(0);
							Response.AppendInt32(0);
							Response.AppendString("");
							Response.AppendString("");
							Response.AppendString("");
						}
						else
						{
							Response.AppendInt32((current.Rank == 2) ? 0 : ((current.Rank == 1) ? 1 : 2));
							Response.AppendUInt(habboForId.Id);
							Response.AppendString(habboForId.Username);
							Response.AppendString(habboForId.Look);
							Response.AppendString("");
						}
					}
					goto IL_367;
				}
			case 1u:
				break;
			case 2u:
			{
				List<List<uint>> list2 = GroupManager.Split(this.GetGroupRequestsByString(Group, SearchVal, ReqType));
				Response.AppendInt32(Group.Requests.Count);
				if (Group.Requests.Count > 0)
				{
					Response.AppendInt32(list2[Page].Count);
					using (List<uint>.Enumerator enumerator2 = list2[Page].GetEnumerator())
					{
						while (enumerator2.MoveNext())
						{
							uint current2 = enumerator2.Current;
							Habbo habboForId2 = CyberEnvironment.getHabboForId(current2);
							if (habboForId2 == null)
							{
								Response.AppendInt32(0);
								Response.AppendInt32(0);
								Response.AppendString("");
								Response.AppendString("");
								Response.AppendString("");
							}
							else
							{
								Response.AppendInt32(3);
								Response.AppendUInt(habboForId2.Id);
								Response.AppendString(habboForId2.Username);
								Response.AppendString(habboForId2.Look);
								Response.AppendString("");
							}
						}
						goto IL_367;
					}
				}
				Response.AppendInt32(0);
				goto IL_367;
			}
			default:
				goto IL_367;
			}
			Response.AppendInt32(Group.Admins.Count);
			if (Group.Admins.Count > 0)
			{
				Response.AppendInt32(list[Page].Count);
				using (List<GroupUser>.Enumerator enumerator3 = list[Page].GetEnumerator())
				{
					while (enumerator3.MoveNext())
					{
						GroupUser current3 = enumerator3.Current;
						Habbo habboForId3 = CyberEnvironment.getHabboForId(current3.Id);
						if (habboForId3 == null)
						{
							Response.AppendInt32(0);
							Response.AppendInt32(0);
							Response.AppendString("");
							Response.AppendString("");
							Response.AppendString("");
						}
						else
						{
							Response.AppendInt32((current3.Rank == 2) ? 0 : ((current3.Rank == 1) ? 1 : 2));
							Response.AppendUInt(habboForId3.Id);
							Response.AppendString(habboForId3.Username);
							Response.AppendString(habboForId3.Look);
							Response.AppendString("");
						}
					}
					goto IL_367;
				}
			}
			Response.AppendInt32(0);
			IL_367:
			Response.AppendBoolean(Session.GetHabbo().Id == Group.CreatorId);
			Response.AppendInt32(14);
			Response.AppendInt32(Page);
			Response.AppendUInt(ReqType);
			Response.AppendString(SearchVal);
			return Response;
		}
		internal List<GroupUser> GetGroupUsersByString(Guild Group, string SearchVal, uint Req)
		{
			List<GroupUser> list = new List<GroupUser>();
			if (string.IsNullOrWhiteSpace(SearchVal))
			{
				if (Req == 0u)
				{
					using (Dictionary<uint, GroupUser>.ValueCollection.Enumerator enumerator = Group.Members.Values.GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							GroupUser current = enumerator.Current;
							list.Add(current);
						}
						return list;
					}
				}
				using (Dictionary<uint, GroupUser>.ValueCollection.Enumerator enumerator2 = Group.Admins.Values.GetEnumerator())
				{
					while (enumerator2.MoveNext())
					{
						GroupUser current2 = enumerator2.Current;
						list.Add(current2);
					}
					return list;
				}
			}
			using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
			{
				queryreactor.setQuery("SELECT id FROM users WHERE username LIKE @query");
				queryreactor.addParameter("query", "%" + SearchVal + "%");
				DataTable table = queryreactor.getTable();
				if (table == null)
				{
					if (Req == 0u)
					{
						using (Dictionary<uint, GroupUser>.ValueCollection.Enumerator enumerator3 = Group.Members.Values.GetEnumerator())
						{
							while (enumerator3.MoveNext())
							{
								GroupUser current3 = enumerator3.Current;
								list.Add(current3);
							}
							goto IL_1CD;
						}
					}
					using (Dictionary<uint, GroupUser>.ValueCollection.Enumerator enumerator4 = Group.Admins.Values.GetEnumerator())
					{
						while (enumerator4.MoveNext())
						{
							GroupUser current4 = enumerator4.Current;
							list.Add(current4);
						}
						goto IL_1CD;
					}
				}
				foreach (DataRow dataRow in table.Rows)
				{
					if (Group.Members.ContainsKey((uint)dataRow[0]))
					{
						list.Add(Group.Members[(uint)dataRow[0]]);
					}
				}
				IL_1CD:;
			}
			return list;
		}
		internal List<uint> GetGroupRequestsByString(Guild Group, string SearchVal, uint Req)
		{
			if (string.IsNullOrWhiteSpace(SearchVal))
			{
				return Group.Requests;
			}
			List<uint> list = new List<uint>();
			using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
			{
				queryreactor.setQuery("SELECT id FROM users WHERE username LIKE @query");
				queryreactor.addParameter("query", "%" + SearchVal + "%");
				DataTable table = queryreactor.getTable();
				if (table != null)
				{
					foreach (DataRow dataRow in table.Rows)
					{
						if (Group.Requests.Contains((uint)dataRow[0]))
						{
							list.Add((uint)dataRow[0]);
						}
					}
				}
			}
			return list;
		}
		internal void SerializeGroupInfo(Guild Group, ServerMessage Response, GameClient Session, bool NewWindow = false)
		{
			DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);
			DateTime dateTime2 = dateTime.AddSeconds((double)Group.CreateTime);
			if (Group == null || Session == null)
			{
				return;
			}
			Response.Init(Outgoing.GroupDataMessageComposer);
			Response.AppendUInt(Group.Id);
			Response.AppendBoolean(true);
			Response.AppendUInt(Group.State);
			Response.AppendString(Group.Name);
			Response.AppendString(Group.Description);
			Response.AppendString(Group.Badge);
			Response.AppendUInt(Group.RoomId);
			Response.AppendString((CyberEnvironment.GetGame().GetRoomManager().GenerateRoomData(Group.RoomId) == null) ? "No room found.." : CyberEnvironment.GetGame().GetRoomManager().GenerateRoomData(Group.RoomId).Name);
			Response.AppendInt32((Group.CreatorId == Session.GetHabbo().Id) ? 3 : (Group.Requests.Contains(Session.GetHabbo().Id) ? 2 : (Group.Members.ContainsKey(Session.GetHabbo().Id) ? 1 : 0)));
			Response.AppendInt32(Group.Members.Count);
			Response.AppendBoolean(Session.GetHabbo().FavouriteGroup == Group.Id);
			Response.AppendString(string.Concat(new object[]
			{
				dateTime2.Day,
				"-",
				dateTime2.Month,
				"-",
				dateTime2.Year
			}));
			Response.AppendBoolean(Group.CreatorId == Session.GetHabbo().Id);
			Response.AppendBoolean(Group.Admins.ContainsKey(Session.GetHabbo().Id));
			Response.AppendString((CyberEnvironment.getHabboForId(Group.CreatorId) == null) ? "" : CyberEnvironment.getHabboForId(Group.CreatorId).Username);
			Response.AppendBoolean(NewWindow);
			Response.AppendBoolean(Group.AdminOnlyDeco == 0u);
			Response.AppendInt32(Group.Requests.Count);
			Response.AppendBoolean(Group.HasForum);
			Session.SendMessage(Response);
		}
		internal void SerializeGroupInfo(Guild Group, ServerMessage Response, GameClient Session, Room Room, bool NewWindow = false)
		{
            if (Room == null) { return; }
			DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);
			DateTime dateTime2 = dateTime.AddSeconds((double)Group.CreateTime);
			Response.Init(Outgoing.GroupDataMessageComposer);
			Response.AppendUInt(Group.Id);
			Response.AppendBoolean(true);
			Response.AppendUInt(Group.State);
			Response.AppendString(Group.Name);
			Response.AppendString(Group.Description);
			Response.AppendString(Group.Badge);
			Response.AppendUInt(Group.RoomId);
			Response.AppendString((CyberEnvironment.GetGame().GetRoomManager().GenerateRoomData(Group.RoomId) == null) ? "No room found.." : CyberEnvironment.GetGame().GetRoomManager().GenerateRoomData(Group.RoomId).Name);
			Response.AppendInt32((Group.CreatorId == Session.GetHabbo().Id) ? 3 : (Group.Requests.Contains(Session.GetHabbo().Id) ? 2 : (Group.Members.ContainsKey(Session.GetHabbo().Id) ? 1 : 0)));
			Response.AppendInt32(Group.Members.Count);
			Response.AppendBoolean(Session.GetHabbo().FavouriteGroup == Group.Id);
			Response.AppendString(string.Concat(new object[]
			{
				dateTime2.Day,
				"-",
				dateTime2.Month,
				"-",
				dateTime2.Year
			}));
			Response.AppendBoolean(Group.CreatorId == Session.GetHabbo().Id);
			Response.AppendBoolean(Group.Admins.ContainsKey(Session.GetHabbo().Id));
			Response.AppendString((CyberEnvironment.getHabboForId(Group.CreatorId) == null) ? "" : CyberEnvironment.getHabboForId(Group.CreatorId).Username);
			Response.AppendBoolean(NewWindow);
			Response.AppendBoolean(Group.AdminOnlyDeco == 0u);
			Response.AppendInt32(Group.Requests.Count);
			Response.AppendBoolean(Group.HasForum);
			if (Room != null)
			{
				Room.SendMessage(Response);
				return;
			}
			if (Session != null)
			{
				Session.SendMessage(Response);
			}
		}
		private static List<List<GroupUser>> Split(List<GroupUser> source)
		{
			return (
				from x in source.Select((GroupUser x, int i) => new
				{
					Index = i,
					Value = x
				})
				group x by x.Index / 14 into x
				select (
					from v in x
					select v.Value).ToList<GroupUser>()).ToList<List<GroupUser>>();
		}
		private static List<List<uint>> Split(List<uint> source)
		{
			return (
				from x in source.Select((uint x, int i) => new
				{
					Index = i,
					Value = x
				})
				group x by x.Index / 14 into x
				select (
					from v in x
					select v.Value).ToList<uint>()).ToList<List<uint>>();
		}
        internal string GenerateGuildImage(int GuildBase, int GuildBaseColor, List<int> states)
        {
            StringBuilder image = new StringBuilder(String.Format("b{0:00}{1:00}", GuildBase, GuildBaseColor));

            for (int i = 0; i < 3 * 4; i += 3)
            {
                if (i >= states.Count)
                    image.Append("s");
                else
                    image.Append(String.Format("s{0:00}{1:00}{2}", states[i], states[i + 1], states[i + 2]));
            }

            return image.ToString();
        }

		internal string GetGroupColour(int Index, bool Colour1)
		{
			if (Colour1)
			{
                if (this.SymbolColours.Contains(Index))
				{
					return ((GroupSymbolColours)this.SymbolColours[Index]).Colour;
				}
			}
			else
			{
                if (this.BackGroundColours.Contains(Index))
				{
                    return ((GroupBackGroundColours)this.BackGroundColours[Index]).Colour;
				}
			}
			return "4f8a00";
		}
		internal void DeleteGroup(uint Id)
		{
			using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
			{
				queryreactor.setQuery("DELETE FROM group_memberships WHERE group_id=@id;DELETE FROM group_requests WHERE group_id=@id;DELETE FROM groups WHERE id=@id LIMIT 1;");
				queryreactor.addParameter("id", Id);
				queryreactor.runQuery();
				this.Groups.Remove(Id);
			}
		}

        internal int GetMessageCountForThread(uint Id)
        {
            using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
            {
                queryreactor.setQuery("SELECT COUNT(*) FROM group_forum_posts WHERE parent_id = @id");
                queryreactor.addParameter("id", Id);
                return int.Parse(queryreactor.getString());
            }
        }
    }
}
