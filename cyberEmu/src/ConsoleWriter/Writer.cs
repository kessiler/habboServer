using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
namespace ConsoleWriter
{
	public class Writer
	{
		private static Queue logQueue;
		private static bool mDisabled = false;
		public static bool DisabledState
		{
			get
			{
				return Writer.mDisabled;
			}
			set
			{
				Writer.mDisabled = value;
			}
		}
		public static void Init()
		{
			Writer.logQueue = new Queue();
		}
		private static void WriteToLogFile()
		{
			while (true)
			{
				try
				{
					Dictionary<string, StringBuilder> dictionary = new Dictionary<string, StringBuilder>();
					while (Writer.logQueue.Count > 0)
					{
						LogMessage logMessage = null;
						lock (Writer.logQueue.SyncRoot)
						{
							if (Writer.logQueue.Count > 0)
							{
								logMessage = (LogMessage)Writer.logQueue.Dequeue();
							}
						}
						if (logMessage != null)
						{
							if (dictionary.ContainsKey(logMessage.location))
							{
								dictionary[logMessage.location].Append(logMessage.message);
							}
							else
							{
								dictionary.Add(logMessage.location, new StringBuilder(logMessage.message));
							}
						}
					}
					foreach (KeyValuePair<string, StringBuilder> current in dictionary)
					{
						Writer.WriteFileContent(current.Value.ToString(), current.Key);
					}
					dictionary.Clear();
				}
				catch (Exception ex)
				{
					Console.WriteLine("Error in console writer: " + ex.ToString());
				}
				Thread.Sleep(3000);
			}
		}
		private static void WriteFileContent(string message, string location)
		{
			FileStream fileStream = new FileStream(location, FileMode.Create, FileAccess.ReadWrite, FileShare.None, 4096, true);
			byte[] bytes = Encoding.ASCII.GetBytes(Environment.NewLine + message);
			fileStream.BeginWrite(bytes, 0, bytes.Length, new AsyncCallback(Writer.WriteCallback), fileStream);
		}
		public static void WriteLine(string Line, ConsoleColor Colour = ConsoleColor.Gray)
		{
			if (!Writer.mDisabled)
			{
				Console.ForegroundColor = Colour;
                Console.WriteLine("[" + DateTime.Now.ToString("HH:mm:ss:fff") + "] " + Line);
			}
		}
		public static void LogException(string logText)
		{
			Writer.WriteToFile("Logs\\ExceptionsLog.txt", logText + "\r\n\r\n");
			Writer.WriteLine("Se ha loggeado una excepción", ConsoleColor.Red);
		}
		public static void LogCriticalException(string logText)
		{
			Writer.WriteToFile("Logs\\ExceptionsLog.txt", logText + "\r\n\r\n");
			Writer.WriteLine("Error crítico loggeado.", ConsoleColor.Red);
		}
		public static void LogCacheError(string logText)
		{
			Writer.WriteToFile("Logs\\ErrorLog.txt", logText + "\r\n\r\n");
			Writer.WriteLine("Error loggeado.", ConsoleColor.Red);
		}
		public static void LogMessage(string logText)
		{
			Writer.WriteToFile("Logs\\CommonLog.txt", logText + "\r\n\r\n");
			Writer.WriteLine(logText, ConsoleColor.Red);
		}
		public static void LogDDOS(string logText)
		{
			Writer.WriteToFile("Logs\\DDosLog.txt", logText + "\r\n\r\n");
			Writer.WriteLine(logText, ConsoleColor.Red);
		}
		public static void LogThreadException(string Exception, string Threadname)
		{
			Writer.WriteToFile("Logs\\ErrorLog.txt", string.Concat(new string[]
			{
				"Error en thread ",
				Threadname,
				": \r\n",
				Exception,
				"\r\n\r\n"
			}));
			Writer.WriteLine("Error en Thread " + Threadname, ConsoleColor.Red);
		}
		public static void LogQueryError(Exception Exception, string query)
		{
            if (query.Contains("xdrcms")) { return; }
			Writer.WriteToFile("Logs\\MySQLErrors.txt", string.Concat(new object[]
			{
				"Error en query: \r\n",
				query,
				"\r\n",
				Exception,
				"\r\n\r\n"
			}));
			Writer.WriteLine("Error en query MySQL.", ConsoleColor.Red);
		}
		public static void LogPacketException(string Packet, string Exception)
		{
			Writer.WriteToFile("Logs\\PacketLogError.txt", string.Concat(new string[]
			{
				"Error en packet ",
				Packet,
				": \r\n",
				Exception,
				"\r\n\r\n"
			}));
			Writer.WriteLine("A packet error has been logged.", ConsoleColor.DarkMagenta);
		}
		public static void HandleException(Exception pException, string pLocation)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine(string.Concat(new string[]
			{
				"Exception logged ",
				DateTime.Now.ToString(),
				" in ",
				pLocation,
				":"
			}));
			stringBuilder.AppendLine(pException.ToString());
			if (pException.InnerException != null)
			{
				stringBuilder.AppendLine("Inner exception:");
				stringBuilder.AppendLine(pException.InnerException.ToString());
			}
			if (pException.HelpLink != null)
			{
				stringBuilder.AppendLine("Help link:");
				stringBuilder.AppendLine(pException.HelpLink);
			}
			if (pException.Source != null)
			{
				stringBuilder.AppendLine("Source:");
				stringBuilder.AppendLine(pException.Source);
			}
			if (pException.Data != null)
			{
				stringBuilder.AppendLine("Data:");
				foreach (DictionaryEntry dictionaryEntry in pException.Data)
				{
					stringBuilder.AppendLine(string.Concat(new object[]
					{
						"  Key: ",
						dictionaryEntry.Key,
						"Value: ",
						dictionaryEntry.Value
					}));
				}
			}
			if (pException.Message != null)
			{
				stringBuilder.AppendLine("Message:");
				stringBuilder.AppendLine(pException.Message);
			}
			if (pException.StackTrace != null)
			{
				stringBuilder.AppendLine("Stack trace:");
				stringBuilder.AppendLine(pException.StackTrace);
			}
			stringBuilder.AppendLine();
			stringBuilder.AppendLine();
			Writer.LogException(stringBuilder.ToString());
		}
		public static void DisablePrimaryWriting(bool ClearConsole)
		{
			Writer.mDisabled = true;
			if (ClearConsole)
			{
				Console.Clear();
			}
		}

		private static void WriteToFile(string path, string content)
		{
            try
            {
                FileStream fileStream = new FileStream(path, FileMode.Append, FileAccess.Write);
                byte[] bytes = Encoding.ASCII.GetBytes(Environment.NewLine + content);
                fileStream.Write(bytes, 0, bytes.Length);
                fileStream.Dispose();
            }
            catch
            {
            }
		}
		private static void WriteCallback(IAsyncResult callback)
		{
			FileStream fileStream = (FileStream)callback.AsyncState;
			fileStream.EndWrite(callback);
			fileStream.Dispose();
		}
	}
}
