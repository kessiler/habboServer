using Cyber.HabboHotel.GameClients;
using Cyber.HabboHotel.Items;
using System;
using System.Collections.Generic;
namespace Cyber.HabboHotel.Rooms
{
	internal class TradeUser
	{
		internal uint UserId;
		private uint RoomId;
		private bool Accepted;
		internal List<UserItem> OfferedItems;
		internal bool HasAccepted
		{
			get
			{
				return this.Accepted;
			}
			set
			{
				this.Accepted = value;
			}
		}
		internal TradeUser(uint UserId, uint RoomId)
		{
			this.UserId = UserId;
			this.RoomId = RoomId;
			this.Accepted = false;
			this.OfferedItems = new List<UserItem>();
		}
		internal RoomUser GetRoomUser()
		{
			Room room = CyberEnvironment.GetGame().GetRoomManager().GetRoom(this.RoomId);
			if (room == null)
			{
				return null;
			}
			return room.GetRoomUserManager().GetRoomUserByHabbo(this.UserId);
		}
		internal GameClient GetClient()
		{
			return CyberEnvironment.GetGame().GetClientManager().GetClientByUserID(this.UserId);
		}
	}
}
