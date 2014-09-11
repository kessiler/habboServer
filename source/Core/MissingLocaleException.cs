using System;
namespace Cyber.Core
{
	internal class MissingLocaleException : Exception
	{
		public MissingLocaleException(string message) : base(message)
		{
		}
	}
}
