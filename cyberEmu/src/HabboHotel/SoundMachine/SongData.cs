using System;
namespace Cyber.HabboHotel.SoundMachine
{
	internal class SongData
	{
		private uint mId;
		private string mCodename;
		private string mName;
		private string mArtist;
		private string mData;
		private double mLength;
		public uint Id
		{
			get
			{
				return this.mId;
			}
		}
		public string Codename
		{
			get
			{
				return this.mCodename;
			}
		}
		public string Name
		{
			get
			{
				return this.mName;
			}
		}
		public string Artist
		{
			get
			{
				return this.mArtist;
			}
		}
		public string Data
		{
			get
			{
				return this.mData;
			}
		}
		public double LengthSeconds
		{
			get
			{
				return this.mLength;
			}
		}
		public int LengthMiliseconds
		{
			get
			{
				return checked((int)unchecked(this.mLength * 1000.0));
			}
		}
		public SongData(uint Id, string Codename, string Name, string Artist, string Data, double Length)
		{
			this.mId = Id;
			this.mCodename = Codename;
			this.mName = Name;
			this.mArtist = Artist;
			this.mData = Data;
			this.mLength = Length;
		}
	}
}
