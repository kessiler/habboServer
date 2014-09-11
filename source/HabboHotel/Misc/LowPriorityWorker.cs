using Database_Manager.Database.Session_Details.Interfaces;
using Cyber.Core;
using System;
using System.Diagnostics;
using System.Net;
using System.Threading;

namespace Cyber.HabboHotel.Misc
{
    internal class LowPriorityWorker
    {
        private static int UserPeak;
        private static Timer mTimer;

        internal static void Init(IQueryAdapter dbClient)
        {
            dbClient.setQuery("SELECT userpeak FROM server_status");
            LowPriorityWorker.UserPeak = dbClient.getInteger();

        }

        internal static void StartProcessing()
        {
            mTimer = new Timer(new TimerCallback(Process), null, 0, 60000);
        }

        internal static void Process(object Caller)
        {
            int clientCount = CyberEnvironment.GetGame().GetClientManager().ClientCount;
            int loadedRoomsCount = CyberEnvironment.GetGame().GetRoomManager().LoadedRoomsCount;
            DateTime dateTime = new DateTime((DateTime.Now - CyberEnvironment.ServerStarted).Ticks);
            string text = dateTime.ToString("HH:mm:ss");


            Console.Title = string.Concat(new object[]
						{
							"CYBER EMULATOR - VERSION: ", CyberEnvironment.PrettyBuild,  " | TIME: ",
							text,
							" | ONLINE COUNT: ",
							clientCount,
							" | ROOM COUNT: ",
							loadedRoomsCount
						});

            if (clientCount > LowPriorityWorker.UserPeak)
            {
                LowPriorityWorker.UserPeak = clientCount;
            }
            using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
            {
                queryreactor.runFastQuery(string.Concat(new object[]
							{
								"UPDATE server_status SET stamp = '",
								CyberEnvironment.GetUnixTimestamp(),
								"', users_online = ",
								clientCount,
								", rooms_loaded = ",
								loadedRoomsCount,
								", server_ver = 'Cyber Emulator', userpeak = ",
								LowPriorityWorker.UserPeak
							}));
            }
        }
    }
}
