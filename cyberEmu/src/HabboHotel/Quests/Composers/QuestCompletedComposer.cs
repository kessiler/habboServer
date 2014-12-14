using Cyber.HabboHotel.GameClients;
using Cyber.Messages;
using Cyber.Messages.Headers;
using System;
namespace Cyber.HabboHotel.Quests.Composer
{
	internal class QuestCompletedComposer
	{
		internal static ServerMessage Compose(GameClient Session, Quest Quest)
		{
			int amountOfQuestsInCategory = CyberEnvironment.GetGame().GetQuestManager().GetAmountOfQuestsInCategory(Quest.Category);
			int i = (Quest == null) ? amountOfQuestsInCategory : Quest.Number;
			int i2 = (Quest == null) ? 0 : Session.GetHabbo().GetQuestProgress(Quest.Id);
			ServerMessage serverMessage = new ServerMessage(Outgoing.QuestCompletedMessageComposer);
			serverMessage.AppendString(Quest.Category);
			serverMessage.AppendInt32(i);
			serverMessage.AppendInt32(Quest.Name.Contains("xmas2012") ? 1 : amountOfQuestsInCategory);
			serverMessage.AppendInt32((Quest == null) ? 3 : Quest.RewardType);
			serverMessage.AppendUInt((Quest == null) ? 0u : Quest.Id);
			serverMessage.AppendBoolean(Quest != null && Session.GetHabbo().CurrentQuestId == Quest.Id);
			serverMessage.AppendString((Quest == null) ? string.Empty : Quest.ActionName);
			serverMessage.AppendString((Quest == null) ? string.Empty : Quest.DataBit);
			serverMessage.AppendInt32((Quest == null) ? 0 : Quest.Reward);
			serverMessage.AppendString((Quest == null) ? string.Empty : Quest.Name);
			serverMessage.AppendInt32(i2);
			serverMessage.AppendUInt((Quest == null) ? 0u : Quest.GoalData);
			serverMessage.AppendInt32((Quest == null) ? 0 : Quest.TimeUnlock);
			serverMessage.AppendString("");
			serverMessage.AppendString("");
			serverMessage.AppendBoolean(true);
			serverMessage.AppendBoolean(true);
			return serverMessage;
		}
	}
}
