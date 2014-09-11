using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cyber.HabboHotel.GameClients;

namespace Cyber.HabboHotel.Support
{
    class HelperSession
    {
        internal GameClient Helper;
        internal GameClient Requester;
        internal List<string> Chats;

        internal HelperSession(GameClient Helper, GameClient Requester, string Question)
        {
            this.Helper = Helper;
            this.Requester = Requester;
            this.Chats = new List<string>();
            this.Chats.Add(Question);
            this.Response(Requester, Question);
        }

        internal void Response(GameClient ResponseClient, string Response)
        {

        }
    }
}
