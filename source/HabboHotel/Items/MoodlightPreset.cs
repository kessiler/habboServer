using System;
namespace Cyber.HabboHotel.Items
{
	internal class MoodlightPreset
	{
		internal string ColorCode;
		internal int ColorIntensity;
		internal bool BackgroundOnly;
		internal MoodlightPreset(string ColorCode, int ColorIntensity, bool BackgroundOnly)
		{
			this.ColorCode = ColorCode;
			this.ColorIntensity = ColorIntensity;
			this.BackgroundOnly = BackgroundOnly;
		}
	}
}
