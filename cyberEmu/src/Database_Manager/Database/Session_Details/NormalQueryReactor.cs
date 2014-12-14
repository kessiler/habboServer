using Database_Manager.Database.Database_Exceptions;
using Database_Manager.Database.Session_Details.Interfaces;
using Database_Manager.Session_Details.Interfaces;
using System;
namespace Database_Manager.Database.Session_Details
{
	internal class NormalQueryReactor : QueryAdapter, IQueryAdapter, IRegularQueryAdapter, IDisposable
	{
		public NormalQueryReactor(IDatabaseClient Client) : base(Client)
		{
			this.command = Client.createNewCommand();
		}
		public void Dispose()
		{
			this.command.Dispose();
			this.client.reportDone();
		}
		public void doCommit()
		{
			new TransactionException("Can't use rollback on a non-transactional Query reactor");
		}
		public void doRollBack()
		{
			new TransactionException("Can't use rollback on a non-transactional Query reactor");
		}
		internal bool getAutoCommit()
		{
			return true;
		}
	}
}
