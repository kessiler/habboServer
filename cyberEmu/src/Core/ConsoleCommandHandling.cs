namespace Cyber.Core
{
    using Database_Manager.Database.Session_Details.Interfaces;
    using Cyber;
    using Cyber.HabboHotel;
    using Cyber.Messages;
    using Cyber.Messages.Headers;
    using System;

    internal class ConsoleCommandHandling
    {
        internal static bool isWaiting = false;

        internal static Game getGame()
        {
            return CyberEnvironment.GetGame();
        }

        internal static void InvokeCommand(string inputData)
        {
            if (string.IsNullOrEmpty(inputData) && Logging.DisabledState)
            {
                return;
            }
            Console.WriteLine();
            try
            {
                string[] strArray = inputData.Split(new char[] { ' ' });
                switch (strArray[0])
                {
                    case "shutdown":
                    case "apagar":
                        Logging.LogMessage("Server shutting down at " + DateTime.Now);
                        Logging.DisablePrimaryWriting(true);
                        Logging.WriteLine("Attempting to shut down...", ConsoleColor.Yellow);
                        CyberEnvironment.PerformShutDown(false);
                        Console.WriteLine();
                        break;

                    case "flush":
                    case "refrescar":
                        if (strArray.Length >= 2)
                        {
                            break;
                        }
                        Console.WriteLine("Please specify parameter. Type 'help' to know more about Console Commands");
                        Console.WriteLine();
                        break;


                    case "alert":
                        {
                            string str = inputData.Substring(6);
                            ServerMessage message = new ServerMessage(Outgoing.BroadcastNotifMessageComposer);
                            message.AppendString(str);
                            message.AppendString("");
                            getGame().GetClientManager().QueueBroadcaseMessage(message);
                            Console.WriteLine("[" + str + "] was sent!");
                            break;
                        }

                    case "help":
                        Console.WriteLine("shutdown / apagar - for shutting down MercuryEmulator");
                        Console.WriteLine("flush / refrescar");
                        Console.WriteLine("      settings");
                        Console.WriteLine("           catalog - re-load Catalogue");
                        Console.WriteLine("alert (msg) - send alert to Every1!");
                        Console.WriteLine();
                            break;

                    default:
                            unknownCommand(inputData);
                            break;
                }
                switch (strArray[1])
                {
                    case "database":
                        CyberEnvironment.GetDatabaseManager().Destroy();
                        Console.WriteLine("Database destroyed");
                        Console.WriteLine();
                            break;

                    case "console":
                    case "consola":
                        Console.Clear();
                        Console.WriteLine();
                            break;

                    default:
                         unknownCommand(inputData);
                Console.WriteLine();
                break;
                }

                switch (strArray[2])
                {
                    case "catalog":
                    case "shop":
                    case "catalogo":
                        using (IQueryAdapter adapter = CyberEnvironment.GetDatabaseManager().getQueryReactor())
                        {
                            getGame().GetCatalog().Initialize(adapter);
                        }
                        getGame().GetClientManager().QueueBroadcaseMessage(new ServerMessage(Outgoing.PublishShopMessageComposer));
                        Console.WriteLine("Catalogue was re-loaded.");
                        Console.WriteLine();
                            break;

                    case "modeldata":
                        using (IQueryAdapter adapter2 = CyberEnvironment.GetDatabaseManager().getQueryReactor())
                        {
                            getGame().GetRoomManager().LoadModels(adapter2);
                        }
                        Console.WriteLine("Room models were re-loaded.");
                        Console.WriteLine();
                        break;

                    case "bans":
                        using (IQueryAdapter adapter3 = CyberEnvironment.GetDatabaseManager().getQueryReactor())
                        {
                            getGame().GetBanManager().LoadBans(adapter3);
                        }
                        Console.WriteLine("Bans were re-loaded");
                        Console.WriteLine();
                        break;

                    default:
                        Console.WriteLine();
                        break;
                }
            }
            catch (Exception)
            {
            }
        }

        private static void unknownCommand(string command)
        {
        }
    }
}

