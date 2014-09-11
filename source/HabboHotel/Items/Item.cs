using System;
using System.Collections.Generic;
namespace Cyber.HabboHotel.Items
{
	internal class Item
	{
		private uint Id;
		internal int SpriteId;
		internal string PublicName;
		internal string Name;
		internal char Type;
		internal int Width;
		internal int Length;
		internal double Height;
		internal bool Stackable;
		internal bool Walkable;
		internal bool IsSeat;
		internal bool AllowRecycle;
		internal bool AllowTrade;
		internal bool AllowMarketplaceSell;
		internal bool AllowGift;
		internal bool AllowInventoryStack;
		internal bool SubscriberOnly;
		internal bool StackMultipler;
		internal string[] ToggleHeight;
		internal InteractionType InteractionType;
		internal List<int> VendingIds;
		internal int Modes;
		internal int EffectId;
		internal int FlatId;
		internal uint ItemId
		{
			get
			{
				return this.Id;
			}
		}
		internal Item(uint Id, int Sprite, string PublicName, string Name, string Type, int Width, int Length, double Height, bool Stackable, bool Walkable, bool IsSeat, bool AllowRecycle, bool AllowTrade, bool AllowMarketplaceSell, bool AllowGift, bool AllowInventoryStack, InteractionType InteractionType, int Modes, string VendingIds, bool sub, int effect, bool StackMultiple, string[] toggle, int flatId)
		{
			this.Id = Id;
			this.SpriteId = Sprite;
			this.PublicName = PublicName;
			this.Name = Name;
			this.Type = char.Parse(Type);
			this.Width = Width;
			this.Length = Length;
			this.Height = Height;
			this.Stackable = Stackable;
			this.Walkable = Walkable;
			this.IsSeat = IsSeat;
			this.AllowRecycle = AllowRecycle;
			this.AllowTrade = AllowTrade;
			this.AllowMarketplaceSell = AllowMarketplaceSell;
			this.AllowGift = AllowGift;
			this.AllowInventoryStack = AllowInventoryStack;
			this.InteractionType = InteractionType;
			this.Modes = Modes;
			this.VendingIds = new List<int>();
			this.SubscriberOnly = sub;
			this.EffectId = effect;
			if (VendingIds.Contains(","))
			{
				string[] array = VendingIds.Split(new char[]
				{
					','
				});
				for (int i = 0; i < array.Length; i++)
				{
					string s = array[i];
					this.VendingIds.Add(int.Parse(s));
				}
			}
			else
			{
				if (!VendingIds.Equals("") && int.Parse(VendingIds) > 0)
				{
					this.VendingIds.Add(int.Parse(VendingIds));
				}
			}
			this.StackMultipler = StackMultiple;
			this.ToggleHeight = toggle;
			this.FlatId = flatId;
		}
	}
}
