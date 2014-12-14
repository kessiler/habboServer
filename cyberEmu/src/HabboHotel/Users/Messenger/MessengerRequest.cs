using Cyber.Messages;
using System;
namespace Cyber.HabboHotel.Users.Messenger
{
	internal class MessengerRequest
	{
		private uint ToUser;
		private uint FromUser;
		private string mUsername;
		internal uint To
		{
			get
			{
				return this.ToUser;
			}
		}
		internal uint From
		{
			get
			{
				return this.FromUser;
			}
		}
		internal MessengerRequest(uint ToUser, uint FromUser, string pUsername)
		{
			this.ToUser = ToUser;
			this.FromUser = FromUser;
			this.mUsername = pUsername;
		}
		internal void Serialize(ServerMessage Request)
		{
			Request.AppendUInt(this.FromUser);
			Request.AppendString(this.mUsername);
			Habbo habboForName = CyberEnvironment.getHabboForName(this.mUsername);
			Request.AppendString((habboForName != null) ? habboForName.Look : "");
		}
	}
}
