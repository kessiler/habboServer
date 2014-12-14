using System;
namespace Cyber.HabboHotel.SoundMachine
{
	internal class SongInstance
	{
		private SongItem mDiskItem;
		private SongData mSongData;
		public SongItem DiskItem
		{
			get
			{
				return this.mDiskItem;
			}
		}
		public SongData SongData
		{
			get
			{
				return this.mSongData;
			}
		}
		public SongInstance(SongItem Item, SongData SongData)
		{
			this.mDiskItem = Item;
			this.mSongData = SongData;
		}
	}
}
