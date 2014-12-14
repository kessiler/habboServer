using Database_Manager.Database.Session_Details.Interfaces;
using Cyber.Core;
using Cyber.HabboHotel.Groups;
using Cyber.Messages;
using System;
using System.Data;
namespace Cyber.HabboHotel.Items
{
	internal class UserItem
	{
		internal uint Id;
		internal uint BaseItem;
		internal string ExtraData;
		private Item mBaseItem;
		internal bool isWallItem;
		internal int LimitedNo;
		internal int LimitedTot;
		internal string SongCode;
		internal uint GroupId;
		internal UserItem(uint Id, uint BaseItem, string ExtraData, uint Group, string SongCode)
		{
			this.Id = Id;
			this.BaseItem = BaseItem;
			this.ExtraData = ExtraData;
			this.mBaseItem = this.GetBaseItem();
			this.GroupId = Group;
			using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
			{
				queryreactor.setQuery("SELECT * FROM items_limited WHERE item_id=" + Id + " LIMIT 1");
				DataRow row = queryreactor.getRow();
				if (row != null)
				{
					this.LimitedNo = int.Parse(row[1].ToString());
					this.LimitedTot = int.Parse(row[2].ToString());
				}
				else
				{
					this.LimitedNo = 0;
					this.LimitedTot = 0;
				}
			}
			this.isWallItem = (this.mBaseItem.Type == 'i');
			this.SongCode = SongCode;
		}
		internal void SerializeWall(ServerMessage Message, bool Inventory)
		{
			Message.AppendUInt(this.Id);
			Message.AppendString(this.mBaseItem.Type.ToString().ToUpper());
			Message.AppendUInt(this.Id);
			Message.AppendInt32(this.GetBaseItem().SpriteId);

			if (this.GetBaseItem().Name.Contains("a2"))
			{
				Message.AppendInt32(3);
			}
			else
			{
				if (this.GetBaseItem().Name.Contains("wallpaper"))
				{
					Message.AppendInt32(2);
				}
				else
				{
					if (this.GetBaseItem().Name.Contains("landscape"))
					{
						Message.AppendInt32(4);
					}
					else
					{
						Message.AppendInt32(1);
					}
				}
			}
			Message.AppendInt32(0);
			Message.AppendString(this.ExtraData);
			Message.AppendBoolean(this.GetBaseItem().AllowRecycle);
			Message.AppendBoolean(this.GetBaseItem().AllowTrade);
			Message.AppendBoolean(this.GetBaseItem().AllowInventoryStack);
			Message.AppendBoolean(false);
			Message.AppendInt32(-1);
			Message.AppendBoolean(true);
			Message.AppendInt32(-1);
		}
		internal void SerializeFloor(ServerMessage Message, bool Inventory)
		{
			Message.AppendUInt(this.Id);
			Message.AppendString(this.mBaseItem.Type.ToString().ToUpper());
			Message.AppendUInt(this.Id);
			Message.AppendInt32(this.GetBaseItem().SpriteId);

            int ExtraParam = 0;

			if (this.GetBaseItem().InteractionType == InteractionType.gld_item || this.GetBaseItem().InteractionType == InteractionType.gld_gate)
			{
				Guild group = CyberEnvironment.GetGame().GetGroupManager().GetGroup(this.GroupId);
				if (group != null)
				{
					Message.AppendInt32(17);
					Message.AppendInt32(2);
					Message.AppendInt32(5);
					Message.AppendString(this.ExtraData);
					Message.AppendString(group.Id.ToString());
					Message.AppendString(group.Badge);
					Message.AppendString(CyberEnvironment.GetGame().GetGroupManager().GetGroupColour(group.Colour1, true));
					Message.AppendString(CyberEnvironment.GetGame().GetGroupManager().GetGroupColour(group.Colour2, false));
				}
				else
				{
					if (this.GetBaseItem().InteractionType == InteractionType.moplaseed)
					{
						Message.AppendInt32(19);
						Message.AppendInt32(1);
						Message.AppendInt32(1);
						Message.AppendString("rarity");
						Message.AppendString(this.ExtraData.ToString());
					}
					else
					{
						if (this.LimitedNo > 0)
						{
							Message.AppendInt32(1);
							Message.AppendInt32(256);
							Message.AppendString(this.ExtraData);
							Message.AppendInt32(this.LimitedNo);
							Message.AppendInt32(this.LimitedTot);
						}
						else
						{
							Message.AppendInt32((GetBaseItem().InteractionType == InteractionType.gift) ? 9 : 0);
							Message.AppendInt32(0);
                            Message.AppendString((GetBaseItem().InteractionType == InteractionType.gift) ? "" : this.ExtraData);
						}
					}
				}
			}
			else
			{
				Message.AppendInt32(1);
				Message.AppendInt32(0);
				Message.AppendString((GetBaseItem().InteractionType == InteractionType.gift) ? "" : this.ExtraData);
			}
			Message.AppendBoolean(this.GetBaseItem().AllowRecycle);
			Message.AppendBoolean(this.GetBaseItem().AllowTrade);
			Message.AppendBoolean(this.LimitedNo <= 0 && this.GetBaseItem().AllowInventoryStack);
			Message.AppendBoolean(false);
			Message.AppendInt32(-1);
			Message.AppendBoolean(true);
			Message.AppendInt32(-1);
			Message.AppendString("");

            try
            {
                if (GetBaseItem().InteractionType == InteractionType.gift)
                {
                    string[] Split = this.ExtraData.Split((char)9);
                    int Ribbon = int.Parse(Split[2]);
                    int Colour = int.Parse(Split[3]);
                    ExtraParam = (Ribbon * 1000) + Colour;
                }
            }
            catch
            {
                ExtraParam = 1001;
            }
            Message.AppendInt32(ExtraParam);//ExtraParam
		}
		internal Item GetBaseItem()
		{
			return CyberEnvironment.GetGame().GetItemManager().GetItem(this.BaseItem);
		}
	}
}
