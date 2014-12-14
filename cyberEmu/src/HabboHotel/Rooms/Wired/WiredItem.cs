using Cyber.HabboHotel.Items;
using System;
using System.Collections.Generic;
namespace Cyber.HabboHotel.Rooms.Wired
{
	public interface WiredItem
	{
		WiredItemType Type
		{
			get;
		}
		RoomItem Item
		{
			get;
			set;
		}
		Room Room
		{
			get;
		}
		List<RoomItem> Items
		{
			get;
			set;
		}
		string OtherString
		{
			get;
			set;
		}
		bool OtherBool
		{
			get;
			set;
		}
		string OtherExtraString
		{
			get;
			set;
		}
		string OtherExtraString2
		{
			get;
			set;
		}
		int Delay
		{
			get;
			set;
		}
		bool Execute(params object[] Stuff);
	}
}
