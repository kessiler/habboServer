using Cyber.HabboHotel.GameClients;
using Cyber.HabboHotel.Users.UserDataManagement;
using System;
namespace Cyber.HabboHotel.Users.Inventory
{
	internal class InventoryGlobal
	{
		private static void WorkItem()
		{
		}
		internal static InventoryComponent GetInventory(uint UserId, GameClient Client, UserData data)
		{
			return new InventoryComponent(UserId, Client, data);
		}
		internal void saveAll()
		{
		}
	}
}
