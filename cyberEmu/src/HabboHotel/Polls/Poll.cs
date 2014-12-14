using Cyber.Messages;
using System;
using System.Collections.Generic;
namespace Cyber.HabboHotel.Polls
{
	internal class Poll
	{
		internal enum PollType
		{
			Opinion,
			Prize_Badge,
			Prize_Furni
		}

		internal uint Id;
		internal uint RoomId;
		internal string PollName;
		internal string PollInvitation;
		internal string Thanks;
		internal string Prize;
		internal Poll.PollType Type;
		internal List<PollQuestion> Questions;
		internal Poll(uint Id, uint RoomId, string PollName, string PollInvitation, string Thanks, string Prize, int Type, List<PollQuestion> Questions)
		{
			this.Id = Id;
			this.RoomId = RoomId;
			this.PollName = PollName;
			this.PollInvitation = PollInvitation;
			this.Thanks = Thanks;
			this.Type = (Poll.PollType)Type;
			this.Prize = Prize;
			this.Questions = Questions;
		}
		internal void Serialize(ServerMessage Message)
		{
			Message.AppendUInt(this.Id);
            Message.AppendString("");//?
			Message.AppendString(this.PollInvitation);
		}
	}
}
