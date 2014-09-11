using Cyber.HabboHotel.Achievements;
using Cyber.HabboHotel.Items;
using Cyber.HabboHotel.Pets;
using Cyber.HabboHotel.Polls;
using Cyber.HabboHotel.RoomBots;
using Cyber.HabboHotel.Rooms;
using Cyber.HabboHotel.Users.Badges;
using Cyber.HabboHotel.Users.Inventory;
using Cyber.HabboHotel.Users.Messenger;
using Cyber.HabboHotel.Users.Relationships;
using Cyber.HabboHotel.Users.Subscriptions;
using System;
using System.Collections.Generic;
namespace Cyber.HabboHotel.Users.UserDataManagement
{
	internal class UserData
	{
		internal uint userID;
		internal Dictionary<string, UserAchievement> achievements;
		internal Dictionary<int, UserTalent> talents;
		internal List<uint> favouritedRooms;
		internal List<uint> ignores;
		internal List<string> tags;
		internal Subscription subscriptions;
		internal List<Badge> badges;
		internal List<UserItem> inventory;
		internal List<AvatarEffect> effects;
		internal Dictionary<uint, MessengerBuddy> friends;
		internal Dictionary<uint, MessengerRequest> requests;
		internal HashSet<RoomData> rooms;
		internal Dictionary<uint, Pet> pets;
		internal Dictionary<uint, int> quests;
		internal Habbo user;
		internal Dictionary<uint, RoomBot> Botinv;
		internal Dictionary<int, Relationship> Relations;
		internal HashSet<uint> suggestedPolls;
        internal int miniMailCount;

		public UserData(uint userID, Dictionary<string, UserAchievement> achievements, Dictionary<int, UserTalent> talents, List<uint> favouritedRooms, List<uint> ignores, List<string> tags, Subscription Sub, List<Badge> badges, List<UserItem> inventory, List<AvatarEffect> effects, Dictionary<uint, MessengerBuddy> friends, Dictionary<uint, MessengerRequest> requests, HashSet<RoomData> rooms, Dictionary<uint, Pet> pets, Dictionary<uint, int> quests, Habbo user, Dictionary<uint, RoomBot> bots, Dictionary<int, Relationship> Relations, HashSet<uint> suggestedPolls, int miniMailCount)
		{
			this.userID = userID;
			this.achievements = achievements;
			this.talents = talents;
			this.favouritedRooms = favouritedRooms;
			this.ignores = ignores;
			this.tags = tags;
			this.subscriptions = Sub;
			this.badges = badges;
			this.inventory = inventory;
			this.effects = effects;
			this.friends = friends;
			this.requests = requests;
			this.rooms = rooms;
			this.pets = pets;
			this.quests = quests;
			this.user = user;
			this.Botinv = bots;
			this.Relations = Relations;
			this.suggestedPolls = suggestedPolls;
            this.miniMailCount = miniMailCount;
		}
	}
}
