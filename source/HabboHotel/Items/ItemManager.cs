using Database_Manager.Database.Session_Details.Interfaces;
using Cyber.Core;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml.Linq;
namespace Cyber.HabboHotel.Items
{
	internal class ItemManager
	{
		internal Dictionary<uint, Item> Items;
		internal ItemManager()
		{
			this.Items = new Dictionary<uint, Item>();
		}

        internal void LoadItems(IQueryAdapter dbClient, out uint itemLoaded)
        {
            LoadItems(dbClient);
            itemLoaded = (uint)Items.Count;
        }

		internal void LoadItems(IQueryAdapter dbClient)
		{
			this.Items = new Dictionary<uint, Item>();
			dbClient.setQuery("SELECT * FROM furniture");
			DataTable table = dbClient.getTable();
			if (table != null)
			{
				string[] array = null;
				foreach (DataRow dataRow in table.Rows)
				{
					try
					{
						uint num = Convert.ToUInt32(dataRow["id"]);
						int sprite = (int)dataRow["sprite_id"];
						int flatId = (int)dataRow["flat_id"];
						string publicName = (string)dataRow["public_name"];
						string name = (string)dataRow["item_name"];
						string type = dataRow["type"].ToString();
						int width = (int)dataRow["width"];
						int length = (int)dataRow["length"];
						double height;
						if (dataRow["stack_height"].ToString().Contains(";"))
						{
							array = dataRow["stack_height"].ToString().Split(new char[]
							{
								';'
							});
							height = Convert.ToDouble(array[0]);
						}
						else
						{
							height = Convert.ToDouble(dataRow["stack_height"]);
						}
						bool stackable = Convert.ToInt32(dataRow["can_stack"]) == 1;
						bool walkable = Convert.ToInt32(dataRow["is_walkable"]) == 1;
						bool isSeat = Convert.ToInt32(dataRow["can_sit"]) == 1;
						bool allowRecycle = Convert.ToInt32(dataRow["allow_recycle"]) == 1;
						bool allowTrade = Convert.ToInt32(dataRow["allow_trade"]) == 1;
						bool allowMarketplaceSell = Convert.ToInt32(dataRow["allow_marketplace_sell"]) == 1;
						bool allowGift = Convert.ToInt32(dataRow["allow_gift"]) == 1;
						bool allowInventoryStack = Convert.ToInt32(dataRow["allow_inventory_stack"]) == 1;
						InteractionType typeFromString = InterractionTypes.GetTypeFromString((string)dataRow["interaction_type"]);
						int modes = (int)dataRow["interaction_modes_count"];
						string vendingIds = (string)dataRow["vending_ids"];
						bool sub = CyberEnvironment.EnumToBool(dataRow["subscriber"].ToString());
						int effect = (int)dataRow["effectid"];
						bool stackMultiple = CyberEnvironment.EnumToBool(dataRow["stack_multiplier"].ToString());
						Item value = new Item(num, sprite, publicName, name, type, width, length, height, stackable, walkable, isSeat, allowRecycle, allowTrade, allowMarketplaceSell, allowGift, allowInventoryStack, typeFromString, modes, vendingIds, sub, effect, stackMultiple, array, flatId);
						this.Items.Add(num, value);
					}
					catch (Exception ex)
					{
						Console.WriteLine(ex.ToString());
						Console.ReadKey();
						Logging.WriteLine("Could not load item #" + Convert.ToUInt32(dataRow[0]) + ", please verify the data is okay.", ConsoleColor.Gray);
					}
				}
			}
		}
		internal void UpdateFlats()
		{

			XDocument xDocument = XDocument.Load("http://www.habbo.es/gamedata/furnidata_xml");
			string text = "";
            string text2 = "";
			int num = -1;
			int num2 = 0;
            int num4 = -1;
			WebClient webClient = new WebClient();
			webClient.Encoding = Encoding.UTF8;
			webClient.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
			using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
			{
				IEnumerable<XElement> enumerable = xDocument.Descendants("roomitemtypes").Descendants("furnitype");
				IEnumerable<XElement> enumerable2 = xDocument.Descendants("wallitemtypes").Descendants("furnitype");
				foreach (XElement current in enumerable)
				{
					try
					{
						text = current.Attribute("classname").Value;
                        text2 = current.Element("name").Value;
						num2 = Convert.ToInt32(current.Attribute("id").Value);
						num = Convert.ToInt32(current.Element("offerid").Value);
                        num4 = Convert.ToInt32(current.Element("rentofferid").Value);
                        if (num4 != -1)
                            num = num4;

						Console.WriteLine(text);
					}
					catch (Exception ex)
					{
						Console.WriteLine(ex.ToString());
					}
					try
					{
                        queryreactor.setQuery("UPDATE furniture SET public_name = @pubn , flat_id = @flatid , sprite_id = @spriteid WHERE item_name = @name");
                        queryreactor.addParameter("pubn", text2);
						queryreactor.addParameter("flatid", num);
						queryreactor.addParameter("spriteid", num2);
						queryreactor.addParameter("name", text);
						queryreactor.runQuery();
					}
					catch (Exception ex2)
					{
						Console.WriteLine(ex2.ToString());
					}
				}
				foreach (XElement current2 in enumerable2)
				{
					text = current2.Attribute("classname").Value;
                    text2 = current2.Element("name").Value;
					num2 = Convert.ToInt32(current2.Attribute("id").Value);
					num = Convert.ToInt32(current2.Element("offerid").Value);
                    num4 = Convert.ToInt32(current2.Element("rentofferid").Value);
                    if (num4 != -1)
                        num = num4;

					Console.WriteLine(text);
					try
					{
						queryreactor.setQuery("UPDATE furniture SET public_name = @pubn , flat_id = @flatid , sprite_id = @spriteid WHERE item_name = @name");
                        queryreactor.addParameter("pubn", text2);
						queryreactor.addParameter("flatid", num);
						queryreactor.addParameter("spriteid", num2);
						queryreactor.addParameter("name", text);
						queryreactor.runQuery();
					}
					catch (Exception ex3)
					{
						Console.WriteLine(ex3.ToString());
					}
				}
			}
		}
		internal bool ContainsItem(uint Id)
		{
			return this.Items.ContainsKey(Id);
		}
		internal Item GetItem(uint Id)
		{
			if (this.ContainsItem(Id))
			{
				return this.Items[Id];
			}
			return null;
		}
		internal Item GetItemBySprite(int SpriteId)
		{
			return (
				from x in this.Items.Values
				where x.SpriteId == SpriteId
				select x).FirstOrDefault<Item>();
		}
	}
}
