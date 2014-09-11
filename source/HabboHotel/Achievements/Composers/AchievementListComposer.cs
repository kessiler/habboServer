using Cyber.HabboHotel.GameClients;
using Cyber.Messages;
using Cyber.Messages.Headers;
using System;
using System.Collections.Generic;
namespace Cyber.HabboHotel.Achievements.Composer
{
    internal class AchievementListComposer
    {
        internal static ServerMessage Compose(GameClient Session, List<Achievement> Achievements)
        {
            ServerMessage serverMessage = new ServerMessage(Outgoing.AchievementListMessageComposer);
            serverMessage.AppendInt32(Achievements.Count);
            foreach (Achievement achievement in Achievements)
            {
                UserAchievement achievementData = Session.GetHabbo().GetAchievementData(achievement.GroupName);
                int i = achievementData != null ? checked(achievementData.Level + 1) : 1;
                int count = achievement.Levels.Count;
                if (i > count)
                {
                    i = count;
                }
                AchievementLevel achievementLevel = achievement.Levels[i];
                AchievementLevel oldLevel = (achievement.Levels.ContainsKey(i - 1)) ? achievement.Levels[i - 1] : achievementLevel;

                serverMessage.AppendUInt(achievement.Id);
                serverMessage.AppendInt32(i);
                serverMessage.AppendString(achievement.GroupName + i);
                serverMessage.AppendInt32(oldLevel.Requirement); // Requisito Anterior
                serverMessage.AppendInt32(achievementLevel.Requirement); // Requisito Nuevo
                serverMessage.AppendInt32(achievementLevel.RewardPoints);
                serverMessage.AppendInt32(0);
                serverMessage.AppendInt32(achievementData != null ? achievementData.Progress : 0); // Progreso Total
                if (achievementData == null)
                {
                    serverMessage.AppendBoolean(false);
                }
                else if (achievementData.Level >= count)
                {
                    serverMessage.AppendBoolean(true);
                }
                else
                {
                    serverMessage.AppendBoolean(false); // Terminado
                }
                serverMessage.AppendString(achievement.Category);
                serverMessage.AppendString(string.Empty);
                serverMessage.AppendInt32(count); // Número de niveles
                serverMessage.AppendInt32(0);
            }
            serverMessage.AppendString("");
            return serverMessage;
        }
    }

}
