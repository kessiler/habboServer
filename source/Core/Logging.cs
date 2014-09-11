using ConsoleWriter;
using System;
using System.Text;
using System.Security.Cryptography;
using System.IO;
using System.Runtime;

namespace Cyber.Core
{
	public static class Logging
	{
		internal static bool DisabledState
		{
			get
			{
				return Writer.DisabledState;
			}
			set
			{
				Writer.DisabledState = value;
			}
		}
		internal static void WriteLine(string Line, ConsoleColor Colour = ConsoleColor.Gray)
		{
			Writer.WriteLine(Line, Colour);
		}
		internal static void LogException(string logText)
		{
			Writer.LogException(Environment.NewLine + logText + Environment.NewLine);
		}
		internal static void LogCriticalException(string logText)
		{
			Writer.LogCriticalException(logText);
		}
		internal static void LogCacheError(string logText)
		{
			Writer.LogCacheError(logText);
		}
		internal static void LogMessage(string logText)
		{
			Writer.LogMessage(logText);
		}
		internal static void LogThreadException(string Exception, string Threadname)
		{
			Writer.LogThreadException(Exception, Threadname);
		}
		public static void LogQueryError(Exception Exception, string query)
		{
			Writer.LogQueryError(Exception, query);
		}
		internal static void LogPacketException(string Packet, string Exception)
		{
			Writer.LogPacketException(Packet, Exception);
		}
		internal static void HandleException(Exception pException, string pLocation)
		{
			Writer.HandleException(pException, pLocation);
		}
		internal static void DisablePrimaryWriting(bool ClearConsole)
		{
			Writer.DisablePrimaryWriting(ClearConsole);
		}
	}
}
