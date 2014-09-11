using Database_Manager.Database.Session_Details.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
namespace Cyber.HabboHotel.Items
{
	internal class MoodlightData
	{
		internal bool Enabled;
		internal int CurrentPreset;
		internal List<MoodlightPreset> Presets;
		internal uint ItemId;
		internal MoodlightData(uint ItemId)
		{
			this.ItemId = ItemId;
			DataRow row;
			using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
			{
				queryreactor.setQuery("SELECT enabled,current_preset,preset_one,preset_two,preset_three FROM room_items_moodlight WHERE item_id = " + ItemId);
				row = queryreactor.getRow();
			}
			if (row == null)
			{
				throw new NullReferenceException("No moodlightdata found in the database");
			}
			this.Enabled = CyberEnvironment.EnumToBool(row["enabled"].ToString());
			this.CurrentPreset = (int)row["current_preset"];
			this.Presets = new List<MoodlightPreset>();
			this.Presets.Add(MoodlightData.GeneratePreset((string)row["preset_one"]));
			this.Presets.Add(MoodlightData.GeneratePreset((string)row["preset_two"]));
			this.Presets.Add(MoodlightData.GeneratePreset((string)row["preset_three"]));
		}
		internal void Enable()
		{
			this.Enabled = true;
			using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
			{
				queryreactor.runFastQuery("UPDATE room_items_moodlight SET enabled = '1' WHERE item_id = " + this.ItemId);
			}
		}
		internal void Disable()
		{
			this.Enabled = false;
			using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
			{
				queryreactor.runFastQuery("UPDATE room_items_moodlight SET enabled = '0' WHERE item_id = " + this.ItemId);
			}
		}
		internal void UpdatePreset(int Preset, string Color, int Intensity, bool BgOnly, bool Hax = false)
		{
			if (!MoodlightData.IsValidColor(Color) || (!MoodlightData.IsValidIntensity(Intensity) && !Hax))
			{
				return;
			}
			string text;
			switch (Preset)
			{
			case 2:
				text = "two";
				goto IL_43;
			case 3:
				text = "three";
				goto IL_43;
			}
			text = "one";
			IL_43:
			using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
			{
				queryreactor.setQuery(string.Concat(new object[]
				{
					"UPDATE room_items_moodlight SET preset_",
					text,
					" = '@color,",
					Intensity,
					",",
					CyberEnvironment.BoolToEnum(BgOnly),
					"' WHERE item_id = ",
					this.ItemId
				}));
				queryreactor.addParameter("color", Color);
				queryreactor.runQuery();
			}
			this.GetPreset(Preset).ColorCode = Color;
			this.GetPreset(Preset).ColorIntensity = Intensity;
			this.GetPreset(Preset).BackgroundOnly = BgOnly;
		}
		internal static MoodlightPreset GeneratePreset(string Data)
		{
			string[] array = Data.Split(new char[]
			{
				','
			});
			if (!MoodlightData.IsValidColor(array[0]))
			{
				array[0] = "#000000";
			}
			return new MoodlightPreset(array[0], int.Parse(array[1]), CyberEnvironment.EnumToBool(array[2]));
		}
		internal MoodlightPreset GetPreset(int i)
		{
			checked
			{
				i--;
				if (this.Presets[i] != null)
				{
					return this.Presets[i];
				}
				return new MoodlightPreset("#000000", 255, false);
			}
		}
		internal static bool IsValidColor(string ColorCode)
		{
			switch (ColorCode)
			{
			case "#000000":
			case "#0053F7":
			case "#EA4532":
			case "#82F349":
			case "#74F5F5":
			case "#E759DE":
			case "#F2F851":
				return true;
			}
			return false;
		}
		internal static bool IsValidIntensity(int Intensity)
		{
			return Intensity >= 0 && Intensity <= 255;
		}
		internal string GenerateExtraData()
		{
			MoodlightPreset preset = this.GetPreset(this.CurrentPreset);
			StringBuilder stringBuilder = new StringBuilder();
			if (this.Enabled)
			{
				stringBuilder.Append(2);
			}
			else
			{
				stringBuilder.Append(1);
			}
			stringBuilder.Append(",");
			stringBuilder.Append(this.CurrentPreset);
			stringBuilder.Append(",");
			if (preset.BackgroundOnly)
			{
				stringBuilder.Append(2);
			}
			else
			{
				stringBuilder.Append(1);
			}
			stringBuilder.Append(",");
			stringBuilder.Append(preset.ColorCode);
			stringBuilder.Append(",");
			stringBuilder.Append(preset.ColorIntensity);
			return stringBuilder.ToString();
		}
	}
}
