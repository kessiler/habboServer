using Cyber.HabboHotel.Items;
using Cyber.HabboHotel.Rooms;
using Cyber.HabboHotel.SoundMachine.Composers;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Cyber.HabboHotel.SoundMachine
{
	internal class RoomMusicController
	{
		private Dictionary<uint, SongItem> mLoadedDisks;
		private SortedDictionary<int, SongInstance> mPlaylist;
		private SongInstance mSong;
		private int mSongQueuePosition;
		private bool mIsPlaying;
		private double mStartedPlayingTimestamp;
		private RoomItem mRoomOutputItem;
		private static bool mBroadcastNeeded;
		public SongInstance CurrentSong
		{
			get
			{
				return this.mSong;
			}
		}
		public bool IsPlaying
		{
			get
			{
				return this.mIsPlaying;
			}
		}
		public double TimePlaying
		{
			get
			{
				return (double)CyberEnvironment.GetUnixTimestamp() - this.mStartedPlayingTimestamp;
			}
		}
		public int SongSyncTimestamp
		{
			get
			{
				if (!this.mIsPlaying || this.mSong == null)
				{
					return 0;
				}
				checked
				{
					if (this.TimePlaying >= this.mSong.SongData.LengthSeconds)
					{
						return (int)this.mSong.SongData.LengthSeconds;
					}
					return (int)unchecked(this.TimePlaying * 1000.0);
				}
			}
		}
		public SortedDictionary<int, SongInstance> Playlist
		{
			get
			{
				SortedDictionary<int, SongInstance> sortedDictionary = new SortedDictionary<int, SongInstance>();
				lock (this.mPlaylist)
				{
					foreach (KeyValuePair<int, SongInstance> current in this.mPlaylist)
					{
						sortedDictionary.Add(current.Key, current.Value);
					}
				}
				return sortedDictionary;
			}
		}
		public int PlaylistCapacity
		{
			get
			{
				return 20;
			}
		}
		public int PlaylistSize
		{
			get
			{
				return this.mPlaylist.Count;
			}
		}
		public bool HasLinkedItem
		{
			get
			{
				return this.mRoomOutputItem != null;
			}
		}
		public uint LinkedItemId
		{
			get
			{
				if (this.mRoomOutputItem == null)
				{
					return 0u;
				}
				return this.mRoomOutputItem.Id;
			}
		}
		public int SongQueuePosition
		{
			get
			{
				return this.mSongQueuePosition;
			}
		}
		public RoomMusicController()
		{
			this.mLoadedDisks = new Dictionary<uint, SongItem>();
			this.mPlaylist = new SortedDictionary<int, SongInstance>();
		}
		public void LinkRoomOutputItem(RoomItem Item)
		{
			this.mRoomOutputItem = Item;
		}
		public int AddDisk(SongItem DiskItem)
		{
			uint songID = DiskItem.songID;
			if (songID == 0u)
			{
				return -1;
			}
			SongData song = SongManager.GetSong(songID);
			if (song == null)
			{
				return -1;
			}
			if (this.mLoadedDisks.ContainsKey(DiskItem.itemID))
			{
				return -1;
			}
			this.mLoadedDisks.Add(DiskItem.itemID, DiskItem);
			int count = this.mPlaylist.Count;
			lock (this.mPlaylist)
			{
				this.mPlaylist.Add(count, new SongInstance(DiskItem, song));
			}
			return count;
		}
		public SongItem RemoveDisk(int PlaylistIndex)
		{
			SongInstance songInstance = null;
			lock (this.mPlaylist)
			{
				if (!this.mPlaylist.ContainsKey(PlaylistIndex))
				{
					return null;
				}
				songInstance = this.mPlaylist[PlaylistIndex];
				this.mPlaylist.Remove(PlaylistIndex);
			}
			lock (this.mLoadedDisks)
			{
				this.mLoadedDisks.Remove(songInstance.DiskItem.itemID);
			}
			this.RepairPlaylist();
			if (PlaylistIndex == this.mSongQueuePosition)
			{
				this.PlaySong();
			}
			return songInstance.DiskItem;
		}
		public void Update(Room Instance)
		{
			if (this.mIsPlaying && (this.mSong == null || this.TimePlaying >= this.mSong.SongData.LengthSeconds + 1.0))
			{
				if (this.mPlaylist.Count == 0)
				{
					this.Stop();
					this.mRoomOutputItem.ExtraData = "0";
					this.mRoomOutputItem.UpdateState();
				}
				else
				{
					this.SetNextSong();
				}
				RoomMusicController.mBroadcastNeeded = true;
			}
			if (RoomMusicController.mBroadcastNeeded)
			{
				this.BroadcastCurrentSongData(Instance);
				RoomMusicController.mBroadcastNeeded = false;
			}
		}
		public void RepairPlaylist()
		{
			List<SongItem> list = null;
			lock (this.mLoadedDisks)
			{
				list = this.mLoadedDisks.Values.ToList<SongItem>();
				this.mLoadedDisks.Clear();
			}
			lock (this.mPlaylist)
			{
				this.mPlaylist.Clear();
			}
			foreach (SongItem current in list)
			{
				this.AddDisk(current);
			}
		}
		public void SetNextSong()
		{
			checked
			{
				this.mSongQueuePosition++;
				this.PlaySong();
			}
		}
		public void PlaySong()
		{
			if (this.mSongQueuePosition >= this.mPlaylist.Count)
			{
				this.mSongQueuePosition = 0;
			}
			if (this.mPlaylist.Count == 0)
			{
				this.Stop();
				return;
			}
			this.mSong = this.mPlaylist[this.mSongQueuePosition];
			this.mStartedPlayingTimestamp = (double)CyberEnvironment.GetUnixTimestamp();
			RoomMusicController.mBroadcastNeeded = true;
		}
		public void Start()
		{
			this.mIsPlaying = true;
			this.mSongQueuePosition = -1;
			this.SetNextSong();
		}
		public void Stop()
		{
			this.mSong = null;
			this.mIsPlaying = false;
			this.mSongQueuePosition = -1;
			RoomMusicController.mBroadcastNeeded = true;
		}
		internal void BroadcastCurrentSongData(Room Instance)
		{
            if (mSong != null)
                Instance.SendMessage(JukeboxComposer.ComposePlayingComposer(mSong.SongData.Id, mSongQueuePosition, 0));
            else
                Instance.SendMessage(JukeboxComposer.ComposePlayingComposer(0, 0, 0));
		}
		internal void OnNewUserEnter(RoomUser user)
		{
			if (user.IsBot || user.GetClient() == null || this.mSong == null)
			{
				return;
			}
			user.GetClient().SendMessage(JukeboxComposer.ComposePlayingComposer(this.mSong.SongData.Id, this.mSongQueuePosition, this.SongSyncTimestamp));
		}
		public void Reset()
		{
			lock (this.mLoadedDisks)
			{
				this.mLoadedDisks.Clear();
			}
			lock (this.mPlaylist)
			{
				this.mPlaylist.Clear();
			}
			this.mRoomOutputItem = null;
			this.mSongQueuePosition = -1;
			this.mStartedPlayingTimestamp = 0.0;
		}
		internal void Destroy()
		{
			if (this.mLoadedDisks != null)
			{
				this.mLoadedDisks.Clear();
			}
			if (this.mPlaylist != null)
			{
				this.mPlaylist.Clear();
			}
			this.mPlaylist = null;
			this.mLoadedDisks = null;
			this.mSong = null;
			this.mRoomOutputItem = null;
		}
	}
}
