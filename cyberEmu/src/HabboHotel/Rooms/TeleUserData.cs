using Cyber.HabboHotel.Users;
using Cyber.Messages;
using System;
namespace Cyber.HabboHotel.Rooms
{
	internal class TeleUserData
	{
		private uint RoomId;
		private uint TeleId;
		private GameClientMessageHandler mHandler;
		private Habbo mUserRefference;
		internal TeleUserData(GameClientMessageHandler pHandler, Habbo pUserRefference, uint RoomId, uint TeleId)
		{
			this.mHandler = pHandler;
			this.mUserRefference = pUserRefference;
			this.RoomId = RoomId;
			this.TeleId = TeleId;
		}
		internal void Execute()
		{
			if (this.mHandler == null || this.mUserRefference == null)
			{
				return;
			}
			this.mUserRefference.IsTeleporting = true;
			this.mUserRefference.TeleporterId = this.TeleId;
			this.mHandler.PrepareRoomForUser(this.RoomId, "");
		}
	}
}
