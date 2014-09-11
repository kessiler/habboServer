using Database_Manager.Database.Session_Details.Interfaces;
using Cyber.HabboHotel.Rooms;
using System;
using System.Data;
namespace Cyber.HabboHotel.Items
{
	internal static class TeleHandler
	{
		internal static uint GetLinkedTele(uint TeleId, Room pRoom)
		{
			uint result;
			using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
			{
				queryreactor.setQuery("SELECT tele_two_id FROM tele_links WHERE tele_one_id = " + TeleId);
				DataRow row = queryreactor.getRow();
				if (row == null)
				{
					result = 0u;
				}
				else
				{
					result = Convert.ToUInt32(row[0]);
				}
			}
			return result;
		}
		internal static uint GetTeleRoomId(uint TeleId, Room pRoom)
		{
			if (pRoom.GetRoomItemHandler().GetItem(TeleId) != null)
			{
				return pRoom.RoomId;
			}
			uint result;
			using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
			{
				queryreactor.setQuery("SELECT room_id FROM items WHERE id = " + TeleId + " LIMIT 1");
				DataRow row = queryreactor.getRow();
				if (row == null)
				{
					result = 0u;
				}
				else
				{
					result = Convert.ToUInt32(row[0]);
				}
			}
			return result;
		}
		internal static bool IsTeleLinked(uint TeleId, Room pRoom)
		{
			uint linkedTele = TeleHandler.GetLinkedTele(TeleId, pRoom);
			if (linkedTele == 0u)
			{
				return false;
			}
			RoomItem item = pRoom.GetRoomItemHandler().GetItem(linkedTele);
			return (item != null && item.GetBaseItem().InteractionType == InteractionType.teleport) || TeleHandler.GetTeleRoomId(linkedTele, pRoom) != 0u;
		}
	}
}
