using Cyber.HabboHotel.GameClients;
using Cyber.HabboHotel.Rooms;
using System;
namespace Cyber.HabboHotel.RoomBots
{
	internal abstract class BotAI
	{
		internal uint BaseId;
		private int RoomUserId;
		private uint RoomId;
		private RoomUser roomUser;
		private Room room;
		internal BotAI()
		{
		}
		internal void Init(uint pBaseId, int pRoomUserId, uint pRoomId, RoomUser user, Room room)
		{
			this.BaseId = pBaseId;
			this.RoomUserId = pRoomUserId;
			this.RoomId = pRoomId;
			this.roomUser = user;
			this.room = room;
		}
		internal Room GetRoom()
		{
			return this.room;
		}
		internal RoomUser GetRoomUser()
		{
			return this.roomUser;
		}
		internal RoomBot GetBotData()
		{
			if (this.GetRoomUser() == null)
			{
				return null;
			}
			return this.GetRoomUser().BotData;
		}
		internal abstract void OnSelfEnterRoom();
		internal abstract void OnSelfLeaveRoom(bool Kicked);
		internal abstract void OnUserEnterRoom(RoomUser User);
		internal abstract void OnUserLeaveRoom(GameClient Client);
		internal abstract void OnUserSay(RoomUser User, string Message);
		internal abstract void OnUserShout(RoomUser User, string Message);
		internal abstract void OnTimerTick();
	}
}
