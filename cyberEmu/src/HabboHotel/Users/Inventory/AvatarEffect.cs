using System;
namespace Cyber.HabboHotel.Users.Inventory
{
	internal class AvatarEffect
	{
		internal int EffectId;
		internal int TotalDuration;
		internal bool Activated;
		internal double StampActivated;
		internal int TimeLeft
		{
			get
			{
				if (!this.Activated)
				{
					return -1;
				}
				double num = (double)CyberEnvironment.GetUnixTimestamp() - this.StampActivated;
				if (num >= (double)this.TotalDuration)
				{
					return 0;
				}
				return checked((int)unchecked((double)this.TotalDuration - num));
			}
		}
		internal bool HasExpired
		{
			get
			{
				return this.TimeLeft != -1 && this.TimeLeft <= 0;
			}
		}
		internal AvatarEffect(int EffectId, int TotalDuration, bool Activated, double ActivateTimestamp)
		{
			this.EffectId = EffectId;
			this.TotalDuration = TotalDuration;
			this.Activated = Activated;
			this.StampActivated = ActivateTimestamp;
		}
		internal void Activate()
		{
			this.Activated = true;
			this.StampActivated = (double)CyberEnvironment.GetUnixTimestamp();
		}
	}
}
