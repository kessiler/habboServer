using Database_Manager.Database.Session_Details.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
namespace Cyber.HabboHotel.Polls
{
	internal class PollManager
	{
		internal Dictionary<uint, Poll> Polls;
		internal PollManager()
		{
			this.Polls = new Dictionary<uint, Poll>();
		}
        internal void Init(IQueryAdapter DBClient, out uint pollLoaded)
        {
            Init(DBClient);
            pollLoaded = (uint)this.Polls.Count;
        }
		internal void Init(IQueryAdapter DBClient)
		{
			this.Polls.Clear();
			DBClient.setQuery("SELECT * FROM polls WHERE enabled = '1'");
			DataTable table = DBClient.getTable();
			if (table != null)
			{
				foreach (DataRow dataRow in table.Rows)
				{
					uint num = uint.Parse(dataRow["id"].ToString());
					DBClient.setQuery("SELECT * FROM poll_questions WHERE poll_id = " + num);
					DataTable table2 = DBClient.getTable();
					List<PollQuestion> list = new List<PollQuestion>();
					foreach (DataRow dataRow2 in table2.Rows)
					{
						list.Add(new PollQuestion(uint.Parse(dataRow2["id"].ToString()), (string)dataRow2["question"], int.Parse(dataRow2["answertype"].ToString()), dataRow2["answers"].ToString().Split(new char[]
						{
							'|'
						}), (string)dataRow2["correct_answer"]));
					}
					Poll value = new Poll(num, uint.Parse(dataRow["room_id"].ToString()), (string)dataRow["caption"], (string)dataRow["invitation"], (string)dataRow["greetings"], (string)dataRow["prize"], int.Parse(dataRow["type"].ToString()), list);
					this.Polls.Add(num, value);
				}
			}
		}
		internal bool TryGetPoll(uint RoomId, out Poll Poll)
		{
			foreach (Poll current in this.Polls.Values)
			{
				if (current.RoomId == RoomId)
				{
					Poll = current;
					return true;
				}
			}
			Poll = null;
			return false;
		}
	}
}
