using Cyber.Core;
using Cyber.HabboHotel.Items;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
namespace Cyber.HabboHotel.Catalogs
{
	internal class CatalogItem
	{
		internal readonly uint Id;
		internal readonly string ItemIdString;
		internal Dictionary<uint, int> Items;
		internal readonly string Name;
		internal readonly int CreditsCost;
		internal readonly int BelCreditsCost;
		internal readonly int LoyaltyCost;
		internal readonly int DucketsCost;
		internal readonly int PageID;
		internal readonly uint songID;
		internal readonly bool IsLimited;
		internal int LimitedSelled;
		internal string Badge = "";
		internal readonly int LimitedStack;
		internal readonly bool HaveOffer;
		internal readonly bool ClubOnly;
		internal readonly string ExtraData;
		internal uint BaseId;
		internal int FirstAmount;
		internal CatalogItem(DataRow Row)
		{
			this.Id = Convert.ToUInt32(Row["id"]);
			this.Name = (string)Row["catalog_name"];
			this.ItemIdString = Row["item_ids"].ToString();
			this.Items = new Dictionary<uint, int>();
			string[] array = this.ItemIdString.Split(new char[]
			{
				';'
			});
			string[] array2 = Row["amounts"].ToString().Split(new char[]
			{
				';'
			});
			checked
			{
				for (int i = 0; i < array.Length; i++)
				{
					uint key = 0u;
					int value = 0;
					if (uint.TryParse(array[i], out key) && int.TryParse(array2[i], out value))
					{
						this.Items.Add(key, value);
					}
				}
				this.BaseId = this.Items.Keys.First<uint>();
				this.FirstAmount = this.Items.Values.First<int>();
				this.PageID = (int)Row["page_id"];
				this.CreditsCost = (int)Row["cost_credits"];
				this.BelCreditsCost = (int)Row["cost_belcredits"];
				this.LoyaltyCost = (int)Row["cost_loyalty"];
				this.DucketsCost = (int)Row["cost_duckets"];
				this.LimitedSelled = (int)Row["limited_sells"];
				this.LimitedStack = (int)Row["limited_stack"];
				this.IsLimited = (this.LimitedStack > 0);
				this.Badge = (string)Row["badge"];
				this.HaveOffer = ((string)Row["offer_active"] == "1");
				this.ClubOnly = ((string)Row["club_only"] == "1");
				this.ExtraData = (string)Row["extradata"];
				this.songID = (uint)Row["song_id"];
			}
		}
		internal Item GetBaseItem(uint ItemIds)
		{
			Item item = CyberEnvironment.GetGame().GetItemManager().GetItem(ItemIds);
			if (item == null)
			{
				Logging.WriteLine("UNKNOWN ItemIds: " + ItemIds, ConsoleColor.Red);
			}
			return item;
		}
		internal Item GetFirstBaseItem()
		{
			return this.GetBaseItem(this.BaseId);
		}
	}
}
