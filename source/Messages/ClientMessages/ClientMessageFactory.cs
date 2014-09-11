using System;
using System.Collections.Concurrent;
namespace Cyber.Messages.ClientMessages
{
	internal static class ClientMessageFactory
	{
		private static readonly ConcurrentQueue<ClientMessage> _freeObjects = new ConcurrentQueue<ClientMessage>();
		public static ClientMessage GetClientMessage(int MessageId, byte[] Body)
		{
			ClientMessage clientMessage = null;
			if (ClientMessageFactory._freeObjects.TryDequeue(out clientMessage))
			{
				clientMessage.Init(MessageId, Body);
				return clientMessage;
			}
			return new ClientMessage(MessageId, Body);
		}
		public static void ObjectCallback(ClientMessage Message)
		{
			ClientMessageFactory._freeObjects.Enqueue(Message);
		}
	}
}
