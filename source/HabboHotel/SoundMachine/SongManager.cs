using Database_Manager.Database.Session_Details.Interfaces;
using Cyber.HabboHotel.GameClients;
using Cyber.HabboHotel.Items;
using Cyber.HabboHotel.Rooms;
using Cyber.HabboHotel.SoundMachine.Composers;
using Cyber.Messages;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
namespace Cyber.HabboHotel.SoundMachine
{
	internal class SongManager
	{
		private const int CACHE_LIFETIME = 180;
		internal static Dictionary<uint, SongData> songs;
		private static Dictionary<uint, double> cacheTimer;
		internal static uint GetSongId(string Codename)
		{
			foreach (SongData current in SongManager.songs.Values)
			{
				if (current.Codename == Codename)
				{
					return current.Id;
				}
			}
			return 0u;
		}

		internal static SongData GetSong(string CodeName)
		{
			foreach (SongData current in SongManager.songs.Values)
			{
				if (current.Codename == CodeName)
				{
                    return current;
				}
			}
			return null;
		}

        internal static SongData GetSongById(uint Id)
        {
            foreach (SongData current in SongManager.songs.Values)
            {
                if (current.Id == Id)
                {
                    return current;
                }
            }
            return null;
        }
        internal static String GetCodeById(uint Id)
        {
            foreach (SongData current in SongManager.songs.Values)
            {
                if (current.Id == Id)
                {
                    return current.Codename;
                }
            }
            return null;
        }
		internal static void Initialize()
		{
			SongManager.songs = new Dictionary<uint, SongData>();
			SongManager.cacheTimer = new Dictionary<uint, double>();
			DataTable table;

            SongManager.songs.Clear();
			using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
			{
				queryreactor.setQuery("SELECT * FROM songs ORDER BY id");
				table = queryreactor.getTable();
			}
			foreach (DataRow dRow in table.Rows)
			{
				SongData songFromDataRow = SongManager.GetSongFromDataRow(dRow);
				SongManager.songs.Add(songFromDataRow.Id, songFromDataRow);
			}
		}
		internal static void ProcessThread()
		{
			double num = (double)CyberEnvironment.GetUnixTimestamp();
			List<uint> list = new List<uint>();
			foreach (KeyValuePair<uint, double> current in SongManager.cacheTimer)
			{
				if (num - current.Value >= 180.0)
				{
					list.Add(current.Key);
				}
			}
			foreach (uint current2 in list)
			{
				SongManager.songs.Remove(current2);
				SongManager.cacheTimer.Remove(current2);
			}
		}
		internal static SongData GetSongFromDataRow(DataRow dRow)
		{
			return new SongData(Convert.ToUInt32(dRow["id"]), dRow["codename"].ToString(), (string)dRow["name"], (string)dRow["artist"], (string)dRow["song_data"], (double)dRow["length"]);
		}
		internal static SongData GetSong(uint SongId)
		{
			SongData result = null;
			SongManager.songs.TryGetValue(SongId, out result);
			return result;
		}
		private static void GetSongData(GameClient Session, ClientMessage Message)
		{
			int num = Message.PopWiredInt32();
			List<SongData> list = new List<SongData>();
			checked
			{
				for (int i = 0; i < num; i++)
				{
					SongData song = SongManager.GetSong(Message.PopWiredUInt());
					if (song != null)
					{
						list.Add(song);
					}
				}
				Session.SendMessage(JukeboxComposer.Compose(list));
			}
		}
		private static void AddToPlaylist(GameClient Session, ClientMessage Message)
		{
			Room currentRoom = Session.GetHabbo().CurrentRoom;
			if (currentRoom == null || !currentRoom.CheckRights(Session, true, false) || !currentRoom.GotMusicController() || currentRoom.GetRoomMusicController().PlaylistSize >= currentRoom.GetRoomMusicController().PlaylistCapacity)
			{
				return;
			}
			UserItem item = Session.GetHabbo().GetInventoryComponent().GetItem(Message.PopWiredUInt());
			if (item == null || item.GetBaseItem().InteractionType != InteractionType.musicdisc)
			{
				return;
			}
			SongItem songItem = new SongItem(item);
			int num = currentRoom.GetRoomMusicController().AddDisk(songItem);
			if (num < 0)
			{
				return;
			}
			Session.GetHabbo().GetInventoryComponent().RemoveItem(songItem.itemID, true);
			Session.SendMessage(JukeboxComposer.Compose(currentRoom.GetRoomMusicController().PlaylistCapacity, currentRoom.GetRoomMusicController().Playlist.Values.ToList<SongInstance>()));
		}
		private static void RemoveFromPlaylist(GameClient Session, ClientMessage Message)
		{
			Room currentRoom = Session.GetHabbo().CurrentRoom;
			if (currentRoom == null || !currentRoom.GotMusicController() || !currentRoom.CheckRights(Session, true, false))
			{
				return;
			}
			SongItem songItem = currentRoom.GetRoomMusicController().RemoveDisk(Message.PopWiredInt32());
			if (songItem == null)
			{
				return;
			}
			Session.GetHabbo().GetInventoryComponent().AddNewItem(songItem.itemID, songItem.baseItem.ItemId, songItem.songID.ToString(), 0u, true, true, 0, 0, "");
			Session.SendMessage(JukeboxComposer.Compose(Session));
			Session.SendMessage(JukeboxComposer.Compose(currentRoom.GetRoomMusicController().PlaylistCapacity, currentRoom.GetRoomMusicController().Playlist.Values.ToList<SongInstance>()));
		}
		private static void GetDisks(GameClient Session, ClientMessage Message)
		{
			Session.SendMessage(JukeboxComposer.Compose(Session));
		}
		private static void GetPlaylist(GameClient Session, ClientMessage Message)
		{
			Room currentRoom = Session.GetHabbo().CurrentRoom;
			if (currentRoom == null || !currentRoom.CheckRights(Session, true, false))
			{
				return;
			}
			Session.SendMessage(JukeboxComposer.Compose(currentRoom.GetRoomMusicController().PlaylistCapacity, currentRoom.GetRoomMusicController().Playlist.Values.ToList<SongInstance>()));
		}
	}
}
