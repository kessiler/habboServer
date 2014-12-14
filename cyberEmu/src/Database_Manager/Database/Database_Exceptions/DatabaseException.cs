using System;
namespace Database_Manager.Database.Database_Exceptions
{
	public class DatabaseException : Exception
	{
		public DatabaseException(string message) : base(message)
		{
		}
	}
}
