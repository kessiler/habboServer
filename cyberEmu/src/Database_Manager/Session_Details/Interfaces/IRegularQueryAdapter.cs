using System;
using System.Data;
namespace Database_Manager.Session_Details.Interfaces
{
	public interface IRegularQueryAdapter
	{
		void addParameter(string name, object query);
		bool findsResult();
		int getInteger();
		DataRow getRow();
		string getString();
		DataTable getTable();
		void runFastQuery(string query);
		void setQuery(string query);
	}
}
