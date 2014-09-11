using Cyber.Messages;
using Cyber.Messages.Headers;
using System;
namespace Cyber.HabboHotel.Achievements.Composer
{
	internal class AchievementUnlockedComposer
	{
        internal static ServerMessage Compose(Achievement Achievement, int Level, int PointReward, int PixelReward)
        {
            ServerMessage serverMessage = new ServerMessage(Outgoing.UnlockAchievementMessageComposer);
            serverMessage.AppendUInt(Achievement.Id);
            serverMessage.AppendInt32(Level);
            serverMessage.AppendInt32(144);
            serverMessage.AppendString(Achievement.GroupName + Level);
            serverMessage.AppendInt32(PointReward);
            serverMessage.AppendInt32(PixelReward);
            serverMessage.AppendInt32(0);
            serverMessage.AppendInt32(10);
            serverMessage.AppendInt32(21);
            serverMessage.AppendString(Level > 1 ? Achievement.GroupName + checked(Level - 1) : string.Empty);
            serverMessage.AppendString(Achievement.Category);
            serverMessage.AppendBoolean(true);
            return serverMessage;
        }

	}
}
