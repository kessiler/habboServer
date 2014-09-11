using System;
using System.Collections.Generic;
using System.Data;
namespace Cyber.HabboHotel.Items
{
	internal class PinataItem
	{
		internal uint ItemBaseId;
		internal List<uint> Rewards;
		internal PinataItem(DataRow Row)
		{
			this.Rewards = new List<uint>();
			this.ItemBaseId = Convert.ToUInt32(Row["item_baseid"]);
			string text = (string)Row["rewards"];
			string[] array = text.Split(new char[]
			{
				';'
			});
			for (int i = 0; i < array.Length; i++)
			{
				string value = array[i];
				this.Rewards.Add(Convert.ToUInt32(value));
			}
		}
	}
}
