using System;
namespace Cyber.HabboHotel.RoomBots
{
	internal class UserBot
	{
		internal uint Id;
		internal string Name;
		internal string Gender;
		internal string Motto;
		internal string Figure;
		internal int X;
		internal int Y;
		internal int Z;
		internal int Rot;
		internal uint RoomId;
		internal UserBot(uint mId, string mName, string mFigure, string mGender, string mMotto)
		{
			this.Id = mId;
			this.Figure = mFigure;
			this.Name = mName;
			this.Gender = mGender;
			this.Motto = mMotto;
			this.X = 0;
			this.Y = 0;
			this.Z = 0;
			this.Rot = 0;
			this.RoomId = 0u;
		}
		internal UserBot(uint mId, string mName, string mFigure, string mGender, string mMotto, uint mBotId, int mX, int mY, int mZ, int mRot, uint mRoom)
		{
			this.Id = mId;
			this.Figure = mFigure;
			this.Name = mName;
			this.Gender = mGender;
			this.Motto = mMotto;
			this.X = mX;
			this.Y = mY;
			this.Z = mZ;
			this.Rot = mRot;
			this.RoomId = mRoom;
		}
	}
}
