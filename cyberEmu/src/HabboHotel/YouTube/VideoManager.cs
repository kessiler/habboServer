using Database_Manager.Database.Session_Details.Interfaces;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Linq;
using System.Xml;
using Cyber.Messages;
using Cyber.Messages.Headers;
using Cyber.HabboHotel.Items;

namespace Cyber.HabboHotel.YouTube
{
	internal class VideoManager
	{
        
		private Dictionary<string, Playlist> Playlists;
        private Playlist DefaultPlaylist;
        private HybridDictionary LoadedTVs;

		internal VideoManager()
		{
			this.Playlists = new Dictionary<string, Playlist>();
            this.LoadedTVs = new HybridDictionary();
		}

        internal void Load(IQueryAdapter dbClient, out uint loadedPly)
        {
            Load(dbClient);
            loadedPly = (uint)Playlists.Count;
        }

		internal void Load(IQueryAdapter DbClient)
		{
			this.Playlists.Clear();
			DbClient.setQuery("SELECT * FROM youtube_playlists");
			DataTable table = DbClient.getTable();
			if (table != null)
			{
				foreach (DataRow dataRow in table.Rows)
				{
                    this.Playlists.Add(dataRow["playlist_id"].ToString(), new Playlist(int.Parse(dataRow["id"].ToString()), dataRow["playlist_id"].ToString(), dataRow["name"].ToString(), dataRow["description"].ToString()));
				}
			}
            if (Playlists.Count == 0)
                DefaultPlaylist = new Playlist(-1, "", "", "");
            else
                DefaultPlaylist = Playlists.Values.First();
		}

		internal static void GetVideosForPL(Playlist Playlist)
		{
            if (Playlist.Id == -1)
                return;

            try
            {
                XmlDocument xmlDocument = new XmlDocument();
                string filename = "http://gdata.youtube.com/feeds/api/playlists/" + Playlist.PlaylistId + "?v=2.1";
                xmlDocument.Load(filename);
                if (xmlDocument != null)
                {
                    XmlNodeList elementsByTagName = xmlDocument.GetElementsByTagName("yt:videoid");
                    foreach (XmlNode xmlNode in elementsByTagName)
                    {
                        try
                        {
                            string innerText = xmlNode.InnerText;
                            if (!Playlist.Videos.ContainsKey(innerText))
                            {
                                if (!Playlist.Videos.Keys.Contains(innerText))
                                {
                                    Playlist.Videos.Add(innerText, 720);
                                }
                            }
                        }
                        catch
                        {
                            continue;
                        }
                    }
                }
            }
            catch
            {
            }
		}

		public bool TVExists(uint ItemId)
		{
            return LoadedTVs.Contains(ItemId);
		}

		internal PlayerTV AddOrGetTV(uint ItemId, uint RoomId)
		{
			if (!this.TVExists(ItemId))
			{
				PlayerTV playerTV = new PlayerTV(ItemId, RoomId);
				LoadedTVs.Add(ItemId, playerTV);
				return playerTV;
			}
            return (PlayerTV)LoadedTVs[ItemId];
		}

		internal Dictionary<string, Playlist> GetPlaylists()
		{
            return this.Playlists;
		}

		internal Playlist GetPlaylist(string Playlist)
		{
            if (PlaylistExists(Playlist))
                return this.Playlists[Playlist];
            else
                return DefaultPlaylist;
		}

		internal Playlist GetDefaultPlaylist()
		{
            return DefaultPlaylist;
		}

		internal bool PlaylistExists(string Playlist)
		{
            return this.Playlists.ContainsKey(Playlist);
		}

        internal bool PlayVideoInRoom(Rooms.Room Room, string Video)
        {
            Video = Video.Replace("http://www.youtube.", "");
            Video = Video.Replace("www.youtube.", "");
            try
            {
                Video = Video.Split('=')[1];
            }
            catch { return false; }

            if (Video == "")
                return false;

            foreach (PlayerTV Tv in LoadedTVs.Values)
            {
                if (Tv.RoomId == Room.RoomId)
                {
                    Tv.CustomVideo = Video;

                    RoomItem Item = Room.GetRoomItemHandler().GetItem(Tv.Item);
                    if (Item != null)
                    {
                        Item.ExtraData = Video;
                        var ServerMessage = new ServerMessage(Outgoing.UpdateRoomItemMessageComposer);
                        Item.Serialize(ServerMessage);
                        Room.SendMessage(ServerMessage);
                    }

                }
            }

            return true;
        }
    }
}
