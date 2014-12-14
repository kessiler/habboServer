using System;
namespace Cyber.HabboHotel.Support
{
	internal class ModerationTemplate
	{
		internal uint Id;
		internal short Category;
		internal string Caption;
		internal string WarningMessage;
		internal string BanMessage;
		internal short BanHours;
		internal bool AvatarBan;
		internal bool Mute;
		internal bool TradeLock;
		internal ModerationTemplate(uint Id, short Category, string Caption, string WarningMessage, string BanMessage, short BanHours, bool AvatarBan, bool Mute, bool TradeLock)
		{
			this.Id = Id;
			this.Category = Category;
			this.Caption = Caption;
			this.WarningMessage = WarningMessage;
			this.BanMessage = BanMessage;
			this.BanHours = BanHours;
			this.AvatarBan = AvatarBan;
			this.Mute = Mute;
			this.TradeLock = TradeLock;
		}
	}
}
