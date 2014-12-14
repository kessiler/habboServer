using System;
using System.Collections.Generic;
namespace Cyber.HabboHotel.RoomBots
{
	internal class BotResponse
	{
		internal uint BotId;
		internal List<string> Keywords;
		internal string ResponseText;
		internal string ResponseType;
		internal int ServeId;
		internal BotResponse(uint BotId, string Keywords, string ResponseText, string ResponseType, int ServeId)
		{
			this.BotId = BotId;
			this.Keywords = new List<string>();
			this.ResponseText = ResponseText;
			this.ResponseType = ResponseType;
			this.ServeId = ServeId;
			string[] array = Keywords.Split(new char[]
			{
				';'
			});
			for (int i = 0; i < array.Length; i++)
			{
				string text = array[i];
				this.Keywords.Add(text.ToLower());
			}
		}
		internal bool KeywordMatched(string Message)
		{
			foreach (string current in this.Keywords)
			{
				if (Message.ToLower().Contains(current.ToLower()))
				{
					return true;
				}
			}
			return false;
		}
	}
}
