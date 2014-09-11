using System;
namespace ConsoleWriter
{
	internal class LogMessage
	{
		internal string message;
		internal string location;

		public LogMessage(string message, string location)
		{
			this.message = message;
			this.location = location;
		}
		internal void Dispose()
		{
			this.message = null;
			this.location = null;
		}
	}
}
