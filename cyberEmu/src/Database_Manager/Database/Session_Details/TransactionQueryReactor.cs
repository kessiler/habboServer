using Database_Manager.Database.Database_Exceptions;
using Database_Manager.Database.Session_Details.Interfaces;
using Database_Manager.Session_Details.Interfaces;
using MySql.Data.MySqlClient;
using System;
namespace Database_Manager.Database.Session_Details
{
	internal class TransactionQueryReactor : QueryAdapter, IQueryAdapter, IRegularQueryAdapter, IDisposable
	{
		private bool finishedTransaction;
		private MySqlTransaction transaction;
		internal TransactionQueryReactor(MySqlClient client) : base(client)
		{
			this.initTransaction();
		}
		public void Dispose()
		{
			if (!this.finishedTransaction)
			{
				throw new TransactionException("The transaction needs to be completed by commit() or rollback() before you can dispose this item.");
			}
			this.command.Dispose();
			this.client.reportDone();
		}
		public void doCommit()
		{
			try
			{
				this.transaction.Commit();
				this.finishedTransaction = true;
			}
			catch (MySqlException ex)
			{
				throw new TransactionException(ex.Message);
			}
		}
		public void doRollBack()
		{
			try
			{
				this.transaction.Rollback();
				this.finishedTransaction = true;
			}
			catch (MySqlException ex)
			{
				throw new TransactionException(ex.Message);
			}
		}
		internal bool getAutoCommit()
		{
			return false;
		}
		private void initTransaction()
		{
			this.command = this.client.createNewCommand();
			this.transaction = this.client.getTransaction();
			this.command.Transaction = this.transaction;
			this.command.Connection = this.transaction.Connection;
			this.finishedTransaction = false;
		}
	}
}
