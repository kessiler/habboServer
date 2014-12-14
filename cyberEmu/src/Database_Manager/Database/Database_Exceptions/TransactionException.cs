using System;
namespace Database_Manager.Database.Database_Exceptions
{
	public class TransactionException : Exception
	{
		public TransactionException(string message) : base(message)
		{
		}
	}
}
