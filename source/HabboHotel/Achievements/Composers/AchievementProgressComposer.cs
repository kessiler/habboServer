using Cyber.Messages;
using Cyber.Messages.Headers;
using System;
namespace Cyber.HabboHotel.Achievements.Composer
{
	internal class AchievementProgressComposer
	{
        internal static ServerMessage Compose(Achievement Achievement, int TargetLevel, AchievementLevel TargetLevelData, int TotalLevels, UserAchievement UserData)
        {
            ServerMessage serverMessage = new ServerMessage(Outgoing.AchievementProgressMessageComposer);
            serverMessage.AppendUInt(Achievement.Id);
            serverMessage.AppendInt32(TargetLevel);
            serverMessage.AppendString(Achievement.GroupName + TargetLevel);
            serverMessage.AppendInt32(TargetLevelData.Requirement);
            serverMessage.AppendInt32(TargetLevelData.Requirement);
            serverMessage.AppendInt32(TargetLevelData.RewardPixels);
            serverMessage.AppendInt32(0);
            serverMessage.AppendInt32(UserData != null ? UserData.Progress : 0);
            serverMessage.AppendBoolean(UserData != null && UserData.Level >= TotalLevels);
            serverMessage.AppendString(Achievement.Category);
            serverMessage.AppendString(string.Empty);
            serverMessage.AppendInt32(TotalLevels);
            serverMessage.AppendInt32(0);
            return serverMessage;
        }


	}
}
