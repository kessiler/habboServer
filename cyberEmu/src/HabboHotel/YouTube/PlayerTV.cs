using System;
namespace Cyber.HabboHotel.YouTube
{
	internal class PlayerTV
	{
        //TODO: Add a timer for videos

		public uint Item;
        public uint RoomId;
		public Playlist Playlist;
		public int CurrentOrder;
        internal string CustomVideo = "";

		internal string CurrentVideo
		{
			get
			{
                if (CustomVideo != "")
                    return CustomVideo;

				if (this.Playlist == null || this.Playlist.Videos.Count == 0)
				{
					return "";
				}
				return this.Playlist.VideoList[checked(this.CurrentOrder - 1)];
			}
		}
		public PlayerTV(uint Item, uint RoomId)
		{
			this.Item = Item;
            this.RoomId = RoomId;
			this.CurrentOrder = 1;
		}
		internal void SetPlaylist(Playlist Playlist)
		{
            this.CustomVideo = "";
			this.CurrentOrder = 1;
			this.Playlist = Playlist;
		}
		internal void SetPreviousVideo()
		{
            CustomVideo = "";
			if (this.CurrentOrder <= 1)
			{
				this.CurrentOrder = this.Playlist.Videos.Count;
				return;
			}
			checked
			{
				this.CurrentOrder--;
			}
		}
		internal void SetNextVideo()
        {
            CustomVideo = "";
			if (this.CurrentOrder >= this.Playlist.Videos.Count)
			{
				this.CurrentOrder = 1;
				return;
			}
			checked
			{
				this.CurrentOrder++;
			}
		}
	}
}
