using Cyber.Messages;
using Cyber.Messages.Headers;
using System;
namespace Cyber.HabboHotel.Quests.Composer
{
	internal class QuestAbortedComposer
	{
		internal static ServerMessage Compose()
		{
			ServerMessage serverMessage = new ServerMessage(Outgoing.QuestAbortedMessageComposer);
			serverMessage.AppendBoolean(false);
			return serverMessage;
		}
	}
}
