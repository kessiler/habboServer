using System;
using System.Collections;
using System.Collections.Generic;
using Cyber.Messages;
using System.Collections.Specialized;
using System.Linq;

namespace Cyber.HabboHotel.Catalogs
{
	internal class CatalogPage
	{
		private int Id;
		internal int ParentId;
		internal string CodeName;
		internal string Caption;
		internal bool Visible;
		internal bool Enabled;
		internal bool ComingSoon;
		internal uint MinRank;
		internal int IconImage;
		internal string Layout;
		internal string LayoutHeadline;
		internal string LayoutTeaser;
		internal string LayoutSpecial;
		internal string Text1;
		internal string Text2;
		internal string TextDetails;
		internal string TextTeaser;
		internal string PageLinkTitle;
		internal string PageLink;
        internal int OrderNum;

        internal HybridDictionary Items;
		internal Dictionary<int, uint> FlatOffers;
        internal ServerMessage CachedContentsMessage;

		internal int PageId
		{
			get
			{
				return this.Id;
			}
		}

		internal CatalogPage(int Id, int ParentId, string CodeName, string Caption, bool Visible, bool Enabled, bool ComingSoon, uint MinRank, int IconImage, string Layout, string LayoutHeadline, string LayoutTeaser, string LayoutSpecial, string Text1, string Text2, string TextDetails, string TextTeaser, string PageLinkTitle, string PageLink, int OrderNum, ref HybridDictionary CataItems)
		{
			this.Id = Id;
			this.ParentId = ParentId;
			this.CodeName = CodeName;
			this.Caption = Caption;
			this.Visible = Visible;
			this.Enabled = Enabled;
			this.ComingSoon = ComingSoon;
			this.MinRank = MinRank;
			this.IconImage = IconImage;
			this.Layout = Layout;
			this.LayoutHeadline = LayoutHeadline;
			this.LayoutTeaser = LayoutTeaser;
			this.LayoutSpecial = LayoutSpecial;
			this.Text1 = Text1;
			this.PageLinkTitle = PageLinkTitle;
			this.PageLink = PageLink;
			this.Text2 = Text2;
			this.TextDetails = TextDetails;
			this.TextTeaser = TextTeaser;
            this.OrderNum = OrderNum;

            if (Layout.StartsWith("frontpage"))
            {
                OrderNum = -2;
            }

            this.Items = new HybridDictionary();
            this.FlatOffers = new Dictionary<int, uint>();

            foreach (CatalogItem catalogItem in CataItems.Values.OfType<CatalogItem>().Where(x => x.PageID == Id))
            {
                if (catalogItem.GetFirstBaseItem() == null)
                {
                    continue;
                }

                this.Items.Add(catalogItem.Id, catalogItem);
                int flatId = catalogItem.GetFirstBaseItem().FlatId;

                if (flatId != -1 && !FlatOffers.ContainsKey(flatId))
                {
                    this.FlatOffers.Add(catalogItem.GetFirstBaseItem().FlatId, catalogItem.Id);
                }
            }

            CachedContentsMessage = CatalogPacket.ComposePage(this);
		}
		internal CatalogItem GetItem(int pId)
		{
            uint num = (uint)pId;
            if (this.FlatOffers.ContainsKey(pId))
			{
                return (CatalogItem)this.Items[FlatOffers[pId]];
			}
            if (this.Items.Contains(num))
			{
				return (CatalogItem)this.Items[num];
			}
			return null;
		}
	}
}
