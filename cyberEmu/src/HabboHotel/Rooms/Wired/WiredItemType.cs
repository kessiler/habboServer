using System;
namespace Cyber.HabboHotel.Rooms.Wired
{
	public enum WiredItemType
	{
		TriggerUserEntersRoom,
		TriggerUserSaysKeyword,
		TriggerRepeatEffect,
		TriggerGameStarts,
		TriggerGameEnds,
		TriggerToggleFurni,
		TriggerWalksOnFurni,
		TriggerWalksOffFurni,
        TriggerLongRepeater,
        TriggerCollision, // Colisión
        EffectMuteUser, // Mutear Usuario
		EffectShowMessage,
		EffectTeleportToFurni,
		EffectToggleFurniState,
		EffectMoveRotateFurni,
		EffectKickUser,
		ConditionFurniHasUsers,
		ConditionFurniHasFurni,
		ConditionTriggererOnFurni,
        ConditionFurniCoincides,
		TriggerTimer,
		TriggerScoreAchieved,
		EffectGiveReward,
		EffectResetPosition,
        EffectGiveScore,
        EffectResetTimers,
		ConditionTimeMoreThan,
		ConditionTimeLessThan,
        ConditionIsGroupMember,
        ConditionFurniTypeMatches,
        ConditionHowManyUsers,
        ConditionIsWearingEffect,
        ConditionIsWearingBadge,
        ConditionDateRangeActive,

        // Negativos:
        ConditionTriggererNotOnFurni,
        ConditionFurniHasNotFurni,
        ConditionFurniHaveNotUsers,
        ConditionItemsDontMatch,
        ConditionFurniTypeDontMatch,
        ConditionIsNotGroupMember,
        ConditionNotHowManyUsers,
        ConditionIsNotWearingEffect,
        ConditionIsNotWearingBadge
	}
}
