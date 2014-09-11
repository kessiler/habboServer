using Database_Manager.Database.Session_Details.Interfaces;
using System;
namespace Cyber.HabboHotel.Items
{
	internal static class HopperHandler
	{
		internal static uint GetAHopper(uint CurRoom)
		{
			uint result;
			using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
			{
				queryreactor.setQuery("SELECT room_id FROM items_hopper WHERE room_id <> @room ORDER BY RAND() LIMIT 1");
				queryreactor.addParameter("room", CurRoom);
				uint num = Convert.ToUInt32(queryreactor.getInteger());
				result = num;
			}
			return result;
		}
		internal static uint GetHopperId(uint NextRoom)
		{
			uint result;
			using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
			{
				queryreactor.setQuery("SELECT hopper_id FROM items_hopper WHERE room_id = @room LIMIT 1");
				queryreactor.addParameter("room", NextRoom);
				string @string = queryreactor.getString();
				if (@string == null)
				{
					result = 0u;
				}
				else
				{
					result = Convert.ToUInt32(@string);
				}
			}
			return result;
		}
	}
}
