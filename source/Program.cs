using ConsoleWriter;
using Cyber.Core;
using System;
using System.Runtime.InteropServices;
using System.Security.Permissions;
namespace Cyber
{
    internal class Program
    {
        private delegate bool EventHandler(Program.CtrlType sig);
        private enum CtrlType
        {
            CTRL_C_EVENT,
            CTRL_BREAK_EVENT,
            CTRL_CLOSE_EVENT,
            CTRL_LOGOFF_EVENT = 5,
            CTRL_SHUTDOWN_EVENT
        }
        private const int MF_BYCOMMAND = 0;
        public const int SC_CLOSE = 61536;
        [SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.ControlAppDomain)]
        public static void Main(string[] Args)
        {
            Program.DeleteMenu(Program.GetSystemMenu(Program.GetConsoleWindow(), false), 61536, 0);
            Writer.Init();
            Program.InitEnvironment();
            while (CyberEnvironment.isLive)
            {
                Console.CursorVisible = true;
                ConsoleCommandHandling.InvokeCommand(Console.ReadLine());
            }
        }
        public static void InitEnvironment()
        {
            if (!CyberEnvironment.isLive)
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.CursorVisible = false;
                AppDomain currentDomain = AppDomain.CurrentDomain;
                currentDomain.UnhandledException += new UnhandledExceptionEventHandler(Program.MyHandler);
                CyberEnvironment.Initialize();
            }
        }
        private static void MyHandler(object sender, UnhandledExceptionEventArgs args)
        {
            Logging.DisablePrimaryWriting(true);
            Exception ex = (Exception)args.ExceptionObject;
            Logging.LogCriticalException("SYSTEM CRITICAL EXCEPTION: " + ex.ToString());
        }
        [DllImport("Kernel32")]
        private static extern bool SetConsoleCtrlHandler(Program.EventHandler handler, bool add);
        [DllImport("user32.dll")]
        public static extern int DeleteMenu(IntPtr hMenu, int nPosition, int wFlags);
        [DllImport("user32.dll")]
        private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);
        [DllImport("kernel32.dll", ExactSpelling = true)]
        private static extern IntPtr GetConsoleWindow();
    }
}
