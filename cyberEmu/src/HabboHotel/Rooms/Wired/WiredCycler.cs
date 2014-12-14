using System;
using System.Collections;
namespace Cyber.HabboHotel.Rooms.Wired
{
	public interface WiredCycler
	{
		Queue ToWork
		{
			get;
			set;
		}
		bool OnCycle();
	}
}
