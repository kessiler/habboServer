using Cyber.HabboHotel.Items;
using Cyber.Messages;
using Cyber.Messages.Headers;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Cyber.HabboHotel.Rooms.Wired.Handlers.Effects
{
	public class GiveReward : WiredItem
	{
		private WiredItemType mType = WiredItemType.EffectGiveReward;
		private Room mRoom;
		private RoomItem mItem;
		private string mText;
		private string mExtra;
		private bool mBool;
		private string mExtra2;
		private List<WiredItemType> mBanned;
		public WiredItemType Type
		{
			get
			{
				return this.mType;
			}
		}
		public RoomItem Item
		{
			get
			{
				return this.mItem;
			}
			set
			{
				this.mItem = value;
			}
		}
		public Room Room
		{
			get
			{
				return this.mRoom;
			}
		}
		public List<RoomItem> Items
		{
			get
			{
				return new List<RoomItem>();
			}
			set
			{
			}
		}
		public int Delay
		{
			get
			{
				return 0;
			}
			set
			{
			}
		}
		public string OtherString
		{
			get
			{
				return this.mText;
			}
			set
			{
				this.mText = value;
			}
		}
		public string OtherExtraString
		{
			get
			{
				return this.mExtra;
			}
			set
			{
				this.mExtra = value;
			}
		}
		public string OtherExtraString2
		{
			get
			{
				return this.mExtra2;
			}
			set
			{
				this.mExtra2 = value;
			}
		}
		public bool OtherBool
		{
			get
			{
				return this.mBool;
			}
			set
			{
				this.mBool = value;
			}
		}
		public GiveReward(RoomItem Item, Room Room)
		{
			this.mItem = Item;
			this.mRoom = Room;
			this.mText = "";
			this.mExtra = "";
			this.mExtra2 = "";
            this.mBanned = new List<WiredItemType>();
		}
		public bool Execute(params object[] Stuff)
		{
			RoomUser roomUser = (RoomUser)Stuff[0];
			WiredItemType item = (WiredItemType)Stuff[1];
			if (this.mBanned.Contains(item))
			{
				return false;
			}
			if (roomUser == null)
			{
				return false;
			}
			string[] array = this.OtherString.Split(new char[]
			{
				';'
			});
			bool flag = true;
			Random random = new Random();
			if (!this.OtherBool)
			{
				string[] array2 = array;
				int i = 0;
				while (i < array2.Length)
				{
					string text = array2[i];
					string[] array3 = text.Split(new char[]
					{
						','
					});
                    bool flag2 = false;
                    string Product = "";
                    int num = 0;
                    try
                    {
                        flag2 = int.Parse(array3[0]) == 0;
                        Product = array3[1];
                        num = int.Parse(array3[2]);
                    }
                    catch
                    {

                    }
					if (random.Next(0, 101) <= num)
					{
						if (flag2)
						{
							roomUser.GetClient().GetHabbo().GetBadgeComponent().GiveBadge(Product, true, roomUser.GetClient(), true);
							break;
						}
						if (Product.StartsWith("avatar_effect"))
						{
							string s = Product.Trim().Replace("avatar_effect", "");
							int effectId = int.Parse(s);
							roomUser.GetClient().GetHabbo().GetAvatarEffectsInventoryComponent().AddNewEffect(effectId, 68400);
							ServerMessage serverMessage = new ServerMessage(Outgoing.WiredRewardAlertMessageComposer);
							serverMessage.AppendInt32(6);
							roomUser.GetClient().SendMessage(serverMessage);
							break;
						}
						Item item2 = null;
						try
						{
							item2 = (
								from x in CyberEnvironment.GetGame().GetItemManager().Items.Values
								where x.Name == Product
								select x).FirstOrDefault<Item>();
						}
						catch (Exception)
						{
							flag = false;
						}
						if (item2 == null)
						{
							flag = false;
							break;
						}
						roomUser.GetClient().GetHabbo().GetInventoryComponent().AddNewItem(0u, item2.ItemId, "0", 0u, true, false, 0, 0, "");
						ServerMessage serverMessage2 = new ServerMessage(Outgoing.WiredRewardAlertMessageComposer);
						serverMessage2.AppendInt32(6);
						roomUser.GetClient().SendMessage(serverMessage2);
						roomUser.GetClient().SendMessage(new ServerMessage(Outgoing.UpdateInventoryMessageComposer));
						break;
					}
					else
					{
						flag = false;
						i++;
					}
				}
			}
			else
			{
				string[] array4 = array;
				for (int j = 0; j < array4.Length; j++)
				{
					string text2 = array4[j];
					string[] array5 = text2.Split(new char[]
					{
						','
					});
					bool flag3 = int.Parse(array5[0]) == 0;
					string Product = array5[1];
					int.Parse(array5[2]);
					if (flag3)
					{
						roomUser.GetClient().GetHabbo().GetBadgeComponent().GiveBadge(Product, true, roomUser.GetClient(), true);
					}
					else
					{
						if (Product.StartsWith("avatar_effect"))
						{
							string s2 = Product.Trim().Replace("avatar_effect", "");
							int effectId2 = int.Parse(s2);
							roomUser.GetClient().GetHabbo().GetAvatarEffectsInventoryComponent().AddNewEffect(effectId2, 68400);
							ServerMessage serverMessage3 = new ServerMessage(Outgoing.WiredRewardAlertMessageComposer);
							serverMessage3.AppendInt32(6);
							roomUser.GetClient().SendMessage(serverMessage3);
						}
						else
						{
							Item item3 = null;
							try
							{
								item3 = (
									from x in CyberEnvironment.GetGame().GetItemManager().Items.Values
									where x.Name == Product
									select x).FirstOrDefault<Item>();
							}
							catch (Exception)
							{
								flag = false;
							}
							if (item3 == null)
							{
								flag = false;
							}
							else
							{
								roomUser.GetClient().GetHabbo().GetInventoryComponent().AddNewItem(0u, item3.ItemId, "0", 0u, true, false, 0, 0, "");
								ServerMessage serverMessage4 = new ServerMessage(Outgoing.WiredRewardAlertMessageComposer);
								serverMessage4.AppendInt32(6);
								roomUser.GetClient().SendMessage(serverMessage4);
								roomUser.GetClient().SendMessage(new ServerMessage(Outgoing.UpdateInventoryMessageComposer));
							}
						}
					}
				}
			}
			if (!flag)
			{
				ServerMessage serverMessage5 = new ServerMessage(Outgoing.WiredRewardAlertMessageComposer);
				serverMessage5.AppendInt32(4);
				roomUser.GetClient().SendMessage(serverMessage5);
			}
			return true;
		}
	}
}
