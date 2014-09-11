using Cyber.HabboHotel.GameClients;
using Cyber.Messages;
using Cyber.Messages.Headers;
using System;
namespace Cyber.HabboHotel.Quests.Composer
{
	internal class QuestStartedComposer
	{
		internal static ServerMessage Compose(GameClient Session, Quest Quest)
		{
			ServerMessage serverMessage = new ServerMessage(Outgoing.QuestStartedMessageComposer);
			QuestListComposer.SerializeQuest(serverMessage, Session, Quest, Quest.Category);
			return serverMessage;
		}
	}
}
