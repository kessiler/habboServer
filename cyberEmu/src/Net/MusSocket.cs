using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
namespace Cyber.Net
{
	internal class MusSocket
	{
		internal Socket msSocket;
		internal string musIp;
		internal int musPort;
		internal HashSet<string> allowedIps;
		internal MusSocket(string _musIp, int _musPort, string[] _allowedIps, int backlog)
		{
			this.musIp = _musIp;
			this.musPort = _musPort;
			this.allowedIps = new HashSet<string>();
			for (int i = 0; i < _allowedIps.Length; i++)
			{
				string item = _allowedIps[i];
				this.allowedIps.Add(item);
			}
			try
			{
				this.msSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
				this.msSocket.Bind(new IPEndPoint(IPAddress.Any, this.musPort));
				this.msSocket.Listen(backlog);
				this.msSocket.BeginAccept(new AsyncCallback(this.OnEvent_NewConnection), this.msSocket);
			}
			catch (Exception ex)
			{
				throw new ArgumentException("No se pudo iniciar el Socket MUS:\n" + ex.ToString());
			}
		}
		internal void OnEvent_NewConnection(IAsyncResult iAr)
		{
			try
			{
				Socket socket = ((Socket)iAr.AsyncState).EndAccept(iAr);
				string text = socket.RemoteEndPoint.ToString().Split(new char[]
				{
					':'
				})[0];
				if (this.allowedIps.Contains(text) || text == "127.0.0.1")
				{
					new MusConnection(socket);
				}
				else
				{
					socket.Close();
				}
			}
			catch (Exception)
			{
			}
			this.msSocket.BeginAccept(new AsyncCallback(this.OnEvent_NewConnection), this.msSocket);
		}
	}
}
