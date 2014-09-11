using System;
namespace Cyber.HabboHotel.Misc
{
	internal static class CrossdomainPolicy
	{
		internal static string GetXmlPolicy()
		{
			return "<?xml version=\"1.0\"?>\r\n<!DOCTYPE cross-domain-policy SYSTEM \"/xml/dtds/cross-domain-policy.dtd\">\r\n<cross-domain-policy>\r\n<allow-access-from domain=\"*\" to-ports=\"1-31111\" />\r\n</cross-domain-policy>\0";
		}
	}
}
