using System;
namespace ConnectionManager
{
	public class GameSocketManagerStatics
	{
		public static readonly int BUFFER_SIZE = 1024;
		public static readonly int MAX_PACKET_SIZE = checked(GameSocketManagerStatics.BUFFER_SIZE - 4);
	}
}
