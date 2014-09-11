using SharedPacketLib;
using System;
namespace Cyber.Net
{
	public class InitialPacketParser : IDataParser, IDisposable, ICloneable
	{
		public delegate void NoParamDelegate();
		public byte[] currentData;
		public event InitialPacketParser.NoParamDelegate PolicyRequest;
		public event InitialPacketParser.NoParamDelegate SwitchParserRequest;
		public void handlePacketData(byte[] packet)
		{
			if (packet[0] == 60 && this.PolicyRequest != null)
			{
				this.PolicyRequest();
				return;
			}
			if (packet[0] != 67 && this.SwitchParserRequest != null)
			{
				this.currentData = packet;
				this.SwitchParserRequest();
			}
		}
		public void Dispose()
		{
			this.PolicyRequest = null;
			this.SwitchParserRequest = null;
		}
		public object Clone()
		{
			return new InitialPacketParser();
		}
	}
}
