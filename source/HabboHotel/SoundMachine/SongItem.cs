using Database_Manager.Database.Session_Details.Interfaces;
using Cyber.HabboHotel.Items;
using System;
namespace Cyber.HabboHotel.SoundMachine
{
	internal class SongItem
	{
		internal uint itemID;
		internal uint songID;
		internal Item baseItem;
		internal string extraData;
		internal string songCode;
		public SongItem(uint itemID, uint songID, int baseItem, string extraData, string songCode)
		{
			this.itemID = itemID;
			this.songID = songID;
			this.baseItem = CyberEnvironment.GetGame().GetItemManager().GetItem(checked((uint)baseItem));
			this.extraData = extraData;
			this.songCode = songCode;
		}
		public SongItem(UserItem item)
		{
			this.itemID = item.Id;
			this.songID = SongManager.GetSongId(item.SongCode);
			this.baseItem = item.GetBaseItem();
			this.extraData = item.ExtraData;
			this.songCode = item.SongCode;
		}
		internal void SaveToDatabase(uint roomID)
		{
			using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
			{
				queryreactor.runFastQuery(string.Concat(new object[]
				{
					"REPLACE INTO items_rooms_songs VALUES (",
					this.itemID,
					",",
					roomID,
					",",
					this.songID,
					")"
				}));
			}
		}
		internal void RemoveFromDatabase()
		{
			using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
			{
				queryreactor.runFastQuery("DELETE FROM items_rooms_songs WHERE itemid = " + this.itemID);
			}
		}
	}
}
