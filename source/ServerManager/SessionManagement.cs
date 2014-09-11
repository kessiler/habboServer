using System;
using System.Collections.Generic;
namespace Cyber.ServerManager
{
	internal static class SessionManagement
	{
		private static List<Session> mSessions;
		internal static void Init()
		{
			SessionManagement.mSessions = new List<Session>();
		}
		internal static void RegisterSession(Session pSession)
		{
			if (!SessionManagement.mSessions.Contains(pSession))
			{
				SessionManagement.mSessions.Add(pSession);
			}
		}
		internal static void RemoveSession(Session pSession)
		{
			SessionManagement.mSessions.Remove(pSession);
		}
		internal static void IncreaseError()
		{
			checked
			{
				foreach (Session current in SessionManagement.mSessions)
				{
					current.DisconnectionError++;
				}
			}
		}
		internal static void IncreaseDisconnection()
		{
			if (SessionManagement.mSessions == null)
			{
				return;
			}
			checked
			{
				foreach (Session current in SessionManagement.mSessions)
				{
					current.Disconnection++;
				}
			}
		}
		private static void BroadcastMessage(string message)
		{
			try
			{
				foreach (Session current in SessionManagement.mSessions)
				{
					current.SendMessage(message);
				}
			}
			catch
			{
			}
		}
	}
}
