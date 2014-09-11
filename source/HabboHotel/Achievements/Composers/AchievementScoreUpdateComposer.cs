using Cyber.Messages;
using Cyber.Messages.Headers;
using System;
namespace Cyber.HabboHotel.Achievements.Composer
{
	internal class AchievementScoreUpdateComposer
	{
		internal static ServerMessage Compose(int Score)
		{
			ServerMessage serverMessage = new ServerMessage(Outgoing.AchievementPointsMessageComposer);
			serverMessage.AppendInt32(Score);
			return serverMessage;
		}
	}
}
