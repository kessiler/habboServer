using System;
namespace Cyber.Messages
{
	internal struct FusedPacket
	{
		internal readonly ServerMessage content;
		internal readonly string requirements;
		public FusedPacket(ServerMessage content, string requirements)
		{
			this.content = content;
			this.requirements = requirements;
		}
	}
}
