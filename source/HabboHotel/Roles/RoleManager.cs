using Database_Manager.Database.Session_Details.Interfaces;
using Cyber.Core;
using System;
using System.Collections.Generic;
using System.Data;
namespace Cyber.HabboHotel.Roles
{
	internal class RoleManager
	{
		private Dictionary<string, uint> Rights;
		private Dictionary<string, int> SubRights;
		private Dictionary<string, uint> CMDRights;
		internal RoleManager()
		{
			this.Rights = new Dictionary<string, uint>();
			this.SubRights = new Dictionary<string, int>();
			this.CMDRights = new Dictionary<string, uint>();
		}
		internal void LoadRights(IQueryAdapter dbClient)
		{
			this.ClearRights();
			dbClient.setQuery("SELECT command,rank FROM fuse_cmds;");
			DataTable table = dbClient.getTable();
			if (table != null)
			{
				foreach (DataRow dataRow in table.Rows)
				{
					if (!this.CMDRights.ContainsKey((string)dataRow[0]))
					{
						this.CMDRights.Add((string)dataRow[0], Convert.ToUInt32(dataRow[1]));
					}
					else
					{
						Logging.LogException(string.Format("Duplicate Fuse Command \"{0}\" found", dataRow[0]));
					}
				}
			}
			dbClient.setQuery("SELECT * FROM server_fuserights");
			DataTable table2 = dbClient.getTable();
			if (table2 != null)
			{
				foreach (DataRow dataRow2 in table2.Rows)
				{
					if ((int)dataRow2[3] == 0)
					{
						if (!this.Rights.ContainsKey((string)dataRow2[0]))
						{
							this.Rights.Add((string)dataRow2[0], Convert.ToUInt32(dataRow2[1]));
						}
						else
						{
							Logging.LogException(string.Format("Unknown Subscription Fuse \"{0}\" found", dataRow2[0]));
						}
					}
					else
					{
						if ((int)dataRow2[3] > 0)
						{
							this.SubRights.Add((string)dataRow2[0], (int)dataRow2[3]);
						}
						else
						{
							Logging.LogException(string.Format("Unknown fuse type \"{0}\" found", dataRow2[3]));
						}
					}
				}
			}
		}
		internal bool RankGotCommand(uint RankId, string CMD)
		{
			return (this.CMDRights.ContainsKey(CMD) && RankId >= this.CMDRights[CMD]);
		}
		internal bool RankHasRight(uint RankId, string Fuse)
		{
			return this.ContainsRight(Fuse) && RankId >= this.Rights[Fuse];
		}
		internal bool HasVIP(int Sub, string Fuse)
		{
			return this.SubRights.ContainsKey(Fuse) && this.SubRights[Fuse] == Sub;
		}
		internal List<string> GetRightsForRank(uint RankId)
		{
			List<string> list = new List<string>();
			foreach (KeyValuePair<string, uint> current in this.Rights)
			{
				if (RankId >= current.Value && !list.Contains(current.Key))
				{
					list.Add(current.Key);
				}
			}
			return list;
		}
		internal bool ContainsRight(string Right)
		{
			return this.Rights.ContainsKey(Right);
		}
		internal void ClearRights()
		{
			this.Rights.Clear();
			this.CMDRights.Clear();
			this.SubRights.Clear();
		}
	}
}
