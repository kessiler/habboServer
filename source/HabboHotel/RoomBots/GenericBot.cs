using Database_Manager.Database.Session_Details.Interfaces;
using Cyber.HabboHotel.GameClients;
using Cyber.HabboHotel.Rooms;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Linq;
namespace Cyber.HabboHotel.RoomBots
{
    internal class GenericBot : BotAI
    {
        private bool canSpeak;
        private int virtualId;
        private bool MixPhrases;
        private bool IsBartender;

        // new bot system
        private int SpeechTimer;
        private Timer chatTimer;
        private int moveTimer;

        internal GenericBot(int VirtualId, int BotId, AIType Type, bool IsBartender)
        {
            int num = 7;

            using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
            {
                queryreactor.setQuery("SELECT speaking_interval from bots WHERE id = " + BotId);
                num = queryreactor.getInteger();
                queryreactor.setQuery("SELECT automatic_chat from bots WHERE id = " + BotId);
                this.canSpeak = Convert.ToBoolean(queryreactor.getString());
                queryreactor.setQuery("SELECT mix_phrases from bots WHERE id = " + BotId);
                this.MixPhrases = Convert.ToBoolean(queryreactor.getString());
            }

            this.SpeechTimer = num * 1000;
            this.moveTimer = 0;
            this.virtualId = VirtualId;
            this.IsBartender = IsBartender;

            if (SpeechTimer < 2000)
            {
                SpeechTimer = 2000;
            }

            if (canSpeak || MixPhrases)
            {
                this.chatTimer = new Timer(new TimerCallback(ChatTimerTick), null, SpeechTimer, SpeechTimer);
            }
        }

        private void StopTimerTick()
        {
            this.chatTimer.Change(Timeout.Infinite, Timeout.Infinite);
            this.chatTimer.Dispose();
        }

        private void ChatTimerTick(object o)
        {
            if (base.GetBotData().WasPicked)
            {
                StopTimerTick();
                return;
            }

            if (base.GetBotData().RandomSpeech.Count > 0)
            {
                RandomSpeech randomSpeech = base.GetBotData().GetRandomSpeech(this.MixPhrases);
                base.GetRoomUser().Chat(null, randomSpeech.Message, randomSpeech.Shout, 0, 0);
            }
        }

        internal override void OnTimerTick()
        {
            if (this.moveTimer > 0)
            {
                this.moveTimer--;
            }
            else
            {
                HashSet<Point> walkableList = base.GetRoom().GetGameMap().walkableList;
                if (walkableList.Count == 0)
                    return;
                int index = new Random(DateTime.Now.Millisecond + this.virtualId ^ 2).Next(0, walkableList.Count - 1);
                Point element = walkableList.ElementAt(index);
                base.GetRoomUser().MoveTo(element);
                this.moveTimer = new Random(DateTime.Now.Millisecond + this.virtualId ^ 2).Next(15, 30);
            }
        }

        internal override void OnSelfEnterRoom()
        {
        }
        internal override void OnSelfLeaveRoom(bool Kicked)
        {
        }
        internal override void OnUserEnterRoom(RoomUser User)
        {
        }
        internal override void OnUserLeaveRoom(GameClient Client)
        {
        }
        internal override void OnUserSay(RoomUser User, string Message)
        {
            // if (Gamemap.TileDistance(base.GetRoomUser().X, base.GetRoomUser().Y, User.X, User.Y) > 16)
            //    {
            //       return;
            //    }
            if (this.IsBartender)
            {
                Random random = new Random();
                try
                {
                    Message = Message.Substring(1);
                }
                catch
                {
                    Message = "";
                }
                string key;
                switch (key = Message.ToLower())
                {
                    case "ven":
                    case "comehere":
                    case "come here":
                    case "ven aquí":
                    case "come":
                        base.GetRoomUser().Chat(null, "¡Voy!", false, 0, 0);
                        base.GetRoomUser().MoveTo(User.SquareInFront);
                        return;
                    case "sirve":
                    case "serve":
                        if (base.GetRoom().CheckRights(User.GetClient()))
                        {
                            foreach (RoomUser current in base.GetRoom().GetRoomUserManager().GetRoomUsers())
                            {
                                current.CarryItem(random.Next(1, 38));
                            }
                            base.GetRoomUser().Chat(null, "Vale. Ya teneis todos algo para zampar.", false, 0, 0);
                            return;
                        }
                        return;
                    case "agua":
                    case "té":
                    case "te":
                    case "tea":
                    case "juice":
                    case "water":
                    case "zumo":
                        base.GetRoomUser().Chat(null, "Aquí tienes.", false, 0, 0);
                        User.CarryItem(random.Next(1, 3));
                        return;
                    case "helado":
                    case "icecream":
                    case "ice cream":
                        base.GetRoomUser().Chat(null, "Aquí tienes. ¡Que no se te quede pegada la lengua, je je!", false, 0, 0);
                        User.CarryItem(4);
                        return;
                    case "rose":
                    case "rosa":
                        base.GetRoomUser().Chat(null, "Aquí tienes... que te vaya bien en tu cita.", false, 0, 0);
                        User.CarryItem(random.Next(1000, 1002));
                        return;
                    case "girasol":
                    case "sunflower":
                        base.GetRoomUser().Chat(null, "Aquí tienes algo muy bonito de la naturaleza.", false, 0, 0);
                        User.CarryItem(1002);
                        return;
                    case "flor":
                    case "flower":
                        base.GetRoomUser().Chat(null, "Aquí tienes algo muy bonito de la naturaleza.", false, 0, 0);
                        if (random.Next(1, 3) == 2)
                        {
                            User.CarryItem(random.Next(1019, 1024));
                            return;
                        }
                        User.CarryItem(random.Next(1006, 1010));
                        return;
                    case "zanahoria":
                    case "zana":
                    case "carrot":
                        base.GetRoomUser().Chat(null, "Aquí tienes una buena verdura. ¡Provecho!", false, 0, 0);
                        User.CarryItem(3);
                        return;
                    case "café":
                    case "cafe":
                    case "capuccino":
                    case "coffee":
                    case "latte":
                    case "mocha":
                    case "espresso":
                    case "expreso":
                        base.GetRoomUser().Chat(null, "Aquí tienes tu café. ¡Está espumoso!", false, 0, 0);
                        User.CarryItem(random.Next(11, 18));
                        return;
                    case "fruta":
                    case "fruit":
                        base.GetRoomUser().Chat(null, "Aquí tienes algo sano, fresco y natural. ¡Que lo disfrutes!", false, 0, 0);
                        User.CarryItem(random.Next(36, 40));
                        return;
                    case "naranja":
                    case "orange":
                        base.GetRoomUser().Chat(null, "Aquí tienes algo sano, fresco y natural. ¡Que lo disfrutes!", false, 0, 0);
                        User.CarryItem(38);
                        return;
                    case "manzana":
                    case "apple":
                        base.GetRoomUser().Chat(null, "Aquí tienes algo sano, fresco y natural. ¡Que lo disfrutes!", false, 0, 0);
                        User.CarryItem(37);
                        return;
                    case "cola":
                    case "habbocola":
                    case "habbo cola":
                    case "coca cola":
                    case "cocacola":
                        base.GetRoomUser().Chat(null, "Aquí tienes un refresco bastante famoso.", false, 0, 0);
                        User.CarryItem(19);
                        return;
                    case "pear":
                    case "pera":
                        base.GetRoomUser().Chat(null, "Aquí tienes algo sano, fresco y natural. ¡Que lo disfrutes!", false, 0, 0);
                        User.CarryItem(36);
                        return;
                    case "ananá":
                    case "pineapple":
                    case "piña":
                    case "rodaja de piña":
                        base.GetRoomUser().Chat(null, "Aquí tienes algo sano, fresco y natural. ¡Que lo disfrutes!", false, 0, 0);
                        User.CarryItem(39);
                        return;

                    case "puta":
                    case "puto":
                    case "gilipollas":
                    case "metemela":
                    case "polla":
                    case "pene":
                    case "penis":
                    case "idiot":
                    case "fuck":
                    case "bastardo":
                    case "idiota":
                    case "chupamela":
                    case "tonta":
                    case "tonto":
                    case "mierda":
                        base.GetRoomUser().Chat(null, "¡No me trates así, eh!", true, 0, 0);
                        return;

                    case "lindo":
                    case "hermoso":
                    case "linda":
                    case "guapa":
                    case "beautiful":
                    case "handsome":
                    case "love":
                    case "guapo":
                    case "i love you":
                    case "hermosa":
                    case "preciosa":
                    case "te amo":
                    case "amor":
                    case "mi amor":
                        base.GetRoomUser().Chat(null, "Soy un bot, err... esto se está poniendo incómodo, ¿sabes?", false, 0, 0);
                        return;
                }
                base.GetRoomUser().Chat(null, "¿Necesitas algo?", false, 0, 0);
            }
        }
        internal override void OnUserShout(RoomUser User, string Message)
        {
            if (this.IsBartender)
            {
                base.GetRoomUser().Chat(null, "A mí no me vengas a gritar. Si quieres que te sirva algo, dímelo bien.", false, 0, 0);
            }
        }
    }
}