using System;
namespace ConnectionManager.Socket_Exceptions
{
	internal class SocketInitializationException : Exception
	{
		public SocketInitializationException(string message) : base(message)
		{
		}
	}
}
