using System;
namespace Cyber.HabboHotel.RoomBots
{
	internal class RandomSpeech
	{
		internal string Message;
		internal bool Shout;
		internal RandomSpeech(string Message, bool Shout)
		{
			this.Message = Message;
			this.Shout = Shout;
		}
	}
}
