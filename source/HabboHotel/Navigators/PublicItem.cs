using Cyber.HabboHotel.Rooms;
using Cyber.Messages;
using System;
namespace Cyber.HabboHotel.Navigators
{
	internal class PublicItem
	{
		private readonly int BannerId;
		internal int Type;
		internal string Caption;
		internal string Image;
		internal PublicImageType ImageType;
		internal uint RoomId;
		internal int ParentId;
		internal string Description;
		internal bool Recommended;
		internal int CategoryId;
		internal PublicItemType itemType;
		internal string TagsToSearch = "";
		internal int Id
		{
			get
			{
				return this.BannerId;
			}
		}
		internal RoomData RoomData
		{
			get
			{
				if (this.RoomId == 0u)
				{
					return new RoomData();
				}
				if (CyberEnvironment.GetGame() == null)
				{
					throw new NullReferenceException();
				}
				if (CyberEnvironment.GetGame().GetRoomManager() == null)
				{
					throw new NullReferenceException();
				}
				if (CyberEnvironment.GetGame().GetRoomManager().GenerateRoomData(this.RoomId) == null)
				{
					throw new NullReferenceException();
				}
				return CyberEnvironment.GetGame().GetRoomManager().GenerateRoomData(this.RoomId);
			}
		}
		internal RoomData RoomInfo
		{
			get
			{
				RoomData result;
				try
				{
					if (this.RoomId > 0u)
					{
						result = CyberEnvironment.GetGame().GetRoomManager().GenerateRoomData(this.RoomId);
					}
					else
					{
						result = null;
					}
				}
				catch
				{
					result = null;
				}
				return result;
			}
		}
		internal PublicItem(int mId, int mType, string mCaption, string mDescription, string mImage, PublicImageType mImageType, uint mRoomId, int mCategoryId, int mParentId, bool mRecommand, int mTypeOfData, string mTags)
		{
			this.BannerId = mId;
			this.Type = mType;
			this.Caption = mCaption;
			this.Description = mDescription;
			this.Image = mImage;
			this.ImageType = mImageType;
			this.RoomId = mRoomId;
			this.ParentId = mParentId;
			this.CategoryId = mCategoryId;
			this.Recommended = mRecommand;
			if (mTypeOfData == 1)
			{
				this.itemType = PublicItemType.TAG;
				return;
			}
			if (mTypeOfData == 2)
			{
				this.itemType = PublicItemType.FLAT;
				return;
			}
			if (mTypeOfData == 3)
			{
				this.itemType = PublicItemType.PUBLIC_FLAT;
				return;
			}
			if (mTypeOfData == 4)
			{
				this.itemType = PublicItemType.CATEGORY;
				return;
			}
			this.itemType = PublicItemType.NONE;
		}
		internal void Serialize(ServerMessage Message)
		{
			try
			{
				Message.AppendInt32(this.Id);
				Message.AppendString(this.Caption);
				Message.AppendString(this.Description);
				Message.AppendInt32(this.Type);
				Message.AppendString(this.Caption);
				Message.AppendString(this.Image);
				Message.AppendInt32((this.ParentId > 0) ? this.ParentId : 0);
				Message.AppendInt32((this.RoomInfo != null) ? this.RoomInfo.UsersNow : 0);
				Message.AppendInt32((this.itemType == PublicItemType.NONE) ? 0 : ((this.itemType == PublicItemType.TAG) ? 1 : ((this.itemType == PublicItemType.FLAT) ? 2 : ((this.itemType == PublicItemType.PUBLIC_FLAT) ? 2 : ((this.itemType == PublicItemType.CATEGORY) ? 4 : 0)))));
				if (this.itemType == PublicItemType.TAG)
				{
					Message.AppendString(this.TagsToSearch);
				}
				else
				{
					if (this.itemType == PublicItemType.CATEGORY)
					{
						Message.AppendBoolean(false);
					}
					else
					{
						if (this.itemType == PublicItemType.FLAT)
						{
							this.RoomInfo.Serialize(Message, false);
						}
						else
						{
							if (this.itemType == PublicItemType.PUBLIC_FLAT)
							{
								this.RoomInfo.Serialize(Message, false);
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine("Exception on publicitems composing: " + ex.ToString());
			}
		}
	}
}
