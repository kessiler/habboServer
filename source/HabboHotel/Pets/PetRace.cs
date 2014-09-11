using Database_Manager.Database.Session_Details.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
namespace Cyber.HabboHotel.Pets
{
	public class PetRace
	{
		public int RaceId;
		public int Color1;
		public int Color2;
		public bool Has1Color;
		public bool Has2Color;
		public static List<PetRace> Races;
		public static void Init(IQueryAdapter dbClient)
		{
			dbClient.setQuery("SELECT * FROM catalog_petbreeds");
			DataTable table = dbClient.getTable();
			PetRace.Races = new List<PetRace>();
			foreach (DataRow dataRow in table.Rows)
			{
				PetRace petRace = new PetRace();
				petRace.RaceId = (int)dataRow["breed_id"];
				petRace.Color1 = (int)dataRow["color1"];
				petRace.Color2 = (int)dataRow["color2"];
				petRace.Has1Color = ((string)dataRow["color1_enabled"] == "1");
				petRace.Has2Color = ((string)dataRow["color2_enabled"] == "1");
				PetRace.Races.Add(petRace);
			}
		}
		public static List<PetRace> GetRacesForRaceId(int sRaceId)
		{
			List<PetRace> list = new List<PetRace>();
			foreach (PetRace current in PetRace.Races)
			{
				if (current.RaceId == sRaceId)
				{
					list.Add(current);
				}
			}
			return list;
		}
		public static bool RaceGotRaces(int sRaceId)
		{
			return PetRace.GetRacesForRaceId(sRaceId).Count > 0;
		}
		public static int GetPetId(string Type, out string Packet)
		{
			int result = 0;
			Packet = "";
			switch (Type)
			{
			case "a0 pet0":
				Packet = "a0 pet0";
				result = 0;
				break;
			case "a0 pet1":
				Packet = "a0 pet1";
				result = 1;
				break;
			case "a0 pet2":
				Packet = "a0 pet2";
				result = 2;
				break;
			case "a0 pet3":
				Packet = "a0 pet3";
				result = 3;
				break;
			case "a0 pet4":
				Packet = "a0 pet4";
				result = 4;
				break;
			case "a0 pet5":
				Packet = "a0 pet5";
				result = 5;
				break;
			case "a0 pet6":
				Packet = "a0 pet6";
				result = 6;
				break;
			case "a0 pet7":
				Packet = "a0 pet7";
				result = 7;
				break;
			case "a0 pet8":
				Packet = "a0 pet8";
				result = 8;
				break;
			case "a0 pet9":
				Packet = "a0 pet9";
				result = 9;
				break;
			case "a0 pet10":
				Packet = "a0 pet10";
				result = 10;
				break;
			case "a0 pet11":
				Packet = "a0 pet11";
				result = 11;
				break;
			case "a0 pet12":
				Packet = "a0 pet12";
				result = 12;
				break;
			case "a0 pet13":
				Packet = "a0 pet13";
				result = 13;
				break;
			case "a0 pet14":
				Packet = "a0 pet14";
				result = 14;
				break;
			case "a0 pet15":
				Packet = "a0 pet15";
				result = 15;
				break;
			case "a0 pet16":
				Packet = "a0 pet16";
				result = 16;
				break;
			case "a0 pet17":
				Packet = "a0 pet17";
				result = 17;
				break;
			case "a0 pet18":
				Packet = "a0 pet18";
				result = 18;
				break;
			case "a0 pet19":
				Packet = "a0 pet19";
				result = 19;
				break;
			case "a0 pet20":
				Packet = "a0 pet20";
				result = 20;
				break;
			case "a0 pet21":
				Packet = "a0 pet21";
				result = 21;
				break;
			case "a0 pet22":
				Packet = "a0 pet22";
				result = 22;
				break;
			case "a0 pet23":
				Packet = "a0 pet23";
				result = 23;
				break;
			case "a0 pet24":
				Packet = "a0 pet24";
				result = 24;
				break;
			case "a0 pet25":
				Packet = "a0 pet25";
				result = 25;
				break;
			case "a0 pet26":
				Packet = "a0 pet26";
				result = 26;
				break;
			}
			return result;
		}
	}
}
