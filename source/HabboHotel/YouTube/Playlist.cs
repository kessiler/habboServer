using Cyber.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Cyber.HabboHotel.YouTube
{
	public class Playlist
	{
		public Dictionary<string, int> Videos;
		public int Id;
		public string PlaylistId;
		public string Caption;
		public string Description;

		internal string FirstVideo
		{
			get
			{
                if (Videos.Count == 0)
                    return "";

                return this.Videos.Keys.First();
			}
		}

		internal List<string> VideoList
		{
			get
			{
				return this.Videos.Keys.ToList<string>();
			}
		}

        public Playlist(int Id, string PlaylistId, string Caption, string Description)
        {
            this.Id = Id;
            this.PlaylistId = PlaylistId;
            this.Caption = Caption;
            this.Description = Description;
            this.Videos = new Dictionary<string, int>();
            VideoManager.GetVideosForPL(this);
        }

		internal void Serialize(ServerMessage Message)
		{
			Message.AppendString(this.PlaylistId);
			Message.AppendString(this.Caption);
			Message.AppendString(this.Description);
		}
		internal int GetDuration(string Video)
		{
			if (!this.Videos.ContainsKey(Video))
			{
				return 0;
			}
            return (this.Videos[Video] + 120);
		}
	}
}
