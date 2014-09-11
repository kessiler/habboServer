using Database_Manager.Database.Session_Details.Interfaces;
using Cyber.HabboHotel.GameClients;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Collections.Specialized;

namespace Cyber.HabboHotel.Support
{
	internal class ModerationBanManager
	{
		private HybridDictionary bannedUsernames;
		private HybridDictionary bannedIPs;
		private Dictionary<string, ModerationBan> bannedMachines;
		internal ModerationBanManager()
		{
			this.bannedUsernames = new HybridDictionary();
			this.bannedIPs = new HybridDictionary();
			this.bannedMachines = new Dictionary<string, ModerationBan>();
		}
		internal void LoadBans(IQueryAdapter dbClient)
		{
			this.bannedUsernames.Clear();
			this.bannedIPs.Clear();
			this.bannedMachines.Clear();
			dbClient.setQuery("SELECT bantype,value,reason,expire FROM bans");
			DataTable table = dbClient.getTable();
			double num = (double)CyberEnvironment.GetUnixTimestamp();
			foreach (DataRow dataRow in table.Rows)
			{
				string text = (string)dataRow["value"];
				string reasonMessage = (string)dataRow["reason"];
				double num2 = (double)dataRow["expire"];
				string a = (string)dataRow["bantype"];
				ModerationBanType type;
				if (a == "user")
				{
					type = ModerationBanType.USERNAME;
				}
				else
				{
					if (a == "ip")
					{
						type = ModerationBanType.IP;
					}
					else
					{
						type = ModerationBanType.MACHINE;
					}
				}
				ModerationBan moderationBan = new ModerationBan(type, text, reasonMessage, num2);
				if (num2 > num)
				{
					if (moderationBan.Type == ModerationBanType.USERNAME)
					{
                        if (!this.bannedUsernames.Contains(text))
						{
							this.bannedUsernames.Add(text, moderationBan);
						}
					}
					else
					{
						if (moderationBan.Type == ModerationBanType.IP)
						{
                            if (!this.bannedIPs.Contains(text))
							{
								this.bannedIPs.Add(text, moderationBan);
							}
						}
						else
						{
							if (!this.bannedMachines.ContainsKey(text))
							{
								this.bannedMachines.Add(text, moderationBan);
							}
						}
					}
				}
			}
		}
		internal string GetBanReason(string username, string ip, string machineid)
		{
            if (this.bannedUsernames.Contains(username))
			{
				ModerationBan moderationBan = (ModerationBan)this.bannedUsernames[username];
				if (!moderationBan.Expired)
				{
					return moderationBan.ReasonMessage;
				}
			}
			else
			{
                if (this.bannedIPs.Contains(ip))
				{
					ModerationBan moderationBan2 = (ModerationBan)this.bannedIPs[username];
					if (!moderationBan2.Expired)
					{
						return moderationBan2.ReasonMessage;
					}
				}
				else
				{
					if (this.bannedMachines.ContainsKey(machineid))
					{
						ModerationBan moderationBan3 = this.bannedMachines[username];
						if (!moderationBan3.Expired)
						{
							return moderationBan3.ReasonMessage;
						}
					}
				}
			}
			return string.Empty;
		}
		internal string CheckMachineBan(string MachineId)
		{
			if (this.bannedMachines.ContainsKey(MachineId))
			{
				return this.bannedMachines[MachineId].ReasonMessage;
			}
			return string.Empty;
		}
		internal void BanUser(GameClient Client, string Moderator, double LengthSeconds, string Reason, bool IpBan, bool Machine)
		{
			ModerationBanType type = ModerationBanType.USERNAME;
			string text = Client.GetHabbo().Username;
			string query = "user";
			double num = (double)CyberEnvironment.GetUnixTimestamp() + LengthSeconds;
			if (IpBan)
			{
				type = ModerationBanType.IP;
				using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
				{
					queryreactor.setQuery("SELECT ip_last FROM users WHERE username='" + text + "' LIMIT 1");
					text = queryreactor.getString();
				}
				query = "ip";
			}
			if (Machine)
			{
				type = ModerationBanType.MACHINE;
				query = "machine";
				text = Client.MachineId;
			}
			ModerationBan moderationBan = new ModerationBan(type, text, Reason, num);
			if (moderationBan.Type == ModerationBanType.IP)
			{
                if (this.bannedIPs.Contains(text))
				{
					this.bannedIPs[text] = moderationBan;
				}
				else
				{
					this.bannedIPs.Add(text, moderationBan);
				}
			}
			else
			{
				if (moderationBan.Type == ModerationBanType.MACHINE)
				{
					if (this.bannedMachines.ContainsKey(text))
					{
						this.bannedMachines[text] = moderationBan;
					}
					else
					{
						this.bannedMachines.Add(text, moderationBan);
					}
				}
				else
				{
                    if (this.bannedUsernames.Contains(text))
					{
						this.bannedUsernames[text] = moderationBan;
					}
					else
					{
						this.bannedUsernames.Add(text, moderationBan);
					}
				}
			}
			using (IQueryAdapter queryreactor2 = CyberEnvironment.GetDatabaseManager().getQueryReactor())
			{
				queryreactor2.setQuery(string.Concat(new object[]
				{
					"INSERT INTO bans (bantype,value,reason,expire,added_by,added_date) VALUES (@rawvar,@var,@reason,'",
					num,
					"',@mod,'",
					DateTime.Now.ToLongDateString(),
					"')"
				}));
				queryreactor2.addParameter("rawvar", query);
				queryreactor2.addParameter("var", text);
				queryreactor2.addParameter("reason", Reason);
				queryreactor2.addParameter("mod", Moderator);
				queryreactor2.runQuery();
			}
			if (IpBan)
			{
				DataTable dataTable = null;
				using (IQueryAdapter queryreactor3 = CyberEnvironment.GetDatabaseManager().getQueryReactor())
				{
					queryreactor3.setQuery("SELECT id FROM users WHERE ip_last = @var");
					queryreactor3.addParameter("var", text);
					dataTable = queryreactor3.getTable();
				}
				if (dataTable != null)
				{
					using (IQueryAdapter queryreactor4 = CyberEnvironment.GetDatabaseManager().getQueryReactor())
					{
						foreach (DataRow dataRow in dataTable.Rows)
						{
							queryreactor4.runFastQuery("UPDATE user_info SET bans = bans + 1 WHERE user_id = " + Convert.ToUInt32(dataRow["id"]));
						}
					}
				}
				this.BanUser(Client, Moderator, LengthSeconds, Reason, false, false);
				return;
			}
			using (IQueryAdapter queryreactor5 = CyberEnvironment.GetDatabaseManager().getQueryReactor())
			{
				queryreactor5.runFastQuery("UPDATE user_info SET bans = bans + 1 WHERE user_id = " + Client.GetHabbo().Id);
			}
			Client.Disconnect();
		}
		internal void UnbanUser(string usernameOrIP)
		{
			new List<ModerationBan>();
			this.bannedUsernames.Remove(usernameOrIP);
			this.bannedIPs.Remove(usernameOrIP);
			using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
			{
				queryreactor.setQuery("DELETE FROM bans WHERE value = @userorip");
				queryreactor.addParameter("userorip", usernameOrIP);
				queryreactor.runQuery();
			}
		}
	}
}
