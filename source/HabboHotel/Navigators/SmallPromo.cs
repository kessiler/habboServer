using Cyber.Messages;
using System;
namespace Cyber.HabboHotel.Navigators
{
	public class SmallPromo
	{
		private int Index;
		private string Header;
		private string Body;
		private string Button;
		private int inGamePromo;
		private string SpecialAction;
		private string Image;
		public SmallPromo(int index, string header, string body, string button, int inGame, string specialAction, string image)
		{
			this.Index = index;
			this.Header = header;
			this.Body = body;
			this.Button = button;
			this.inGamePromo = inGame;
			this.SpecialAction = specialAction;
			this.Image = image;
		}
		internal ServerMessage Serialize(ServerMessage Composer)
		{
			Composer.AppendInt32(this.Index);
			Composer.AppendString(this.Header);
			Composer.AppendString(this.Body);
			Composer.AppendString(this.Button);
			Composer.AppendInt32(this.inGamePromo);
			Composer.AppendString(this.SpecialAction);
			Composer.AppendString(this.Image);
			return Composer;
		}
	}
}
