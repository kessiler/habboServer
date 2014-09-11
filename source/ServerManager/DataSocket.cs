using Cyber.Core;
using System;
using System.Net;
using System.Net.Sockets;
namespace Cyber.ServerManager
{
	internal class DataSocket
	{
		private static Socket mListener;
		private static AsyncCallback mConnectionReqCallback;
		internal static void SetupListener(int Port)
		{
			SessionManagement.Init();
			DataSocket.mListener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			IPEndPoint localEP = new IPEndPoint(IPAddress.Any, Port);
			DataSocket.mListener.Bind(localEP);
			Console.WriteLine(Port);
			DataSocket.mListener.Listen(1000);
			DataSocket.mConnectionReqCallback = new AsyncCallback(DataSocket.ConnectionRequest);
		}
		internal static void Start()
		{
			DataSocket.WaitForNextConnection();
		}
		private static void ConnectionRequest(IAsyncResult iAr)
		{
			try
			{
				Socket pSock = ((Socket)iAr.AsyncState).EndAccept(iAr);
				new Session(pSock);
			}
			catch
			{
			}
			DataSocket.WaitForNextConnection();
		}
		private static void WaitForNextConnection()
		{
			try
			{
				DataSocket.mListener.BeginAccept(DataSocket.mConnectionReqCallback, DataSocket.mListener);
			}
			catch (Exception pException)
			{
				Logging.HandleException(pException, "DataSocket.WaitForNextConnection");
			}
		}
	}
}
