using System;
using System.Collections.Generic;
using Cyber.Messages;
using Cyber.Messages.Headers;
using Database_Manager.Database.Session_Details.Interfaces;

namespace Cyber.HabboHotel.Groups
{
	internal class Guild
	{
		internal uint Id;
		internal string Name;
		internal string Description;
		internal uint RoomId;
		internal string Badge;
		internal uint State;
		internal uint AdminOnlyDeco;
		internal int CreateTime;
		internal uint CreatorId;
		internal int Colour1;
		internal int Colour2;
		internal Dictionary<uint, GroupUser> Members;
		internal Dictionary<uint, GroupUser> Admins;
		internal List<uint> Requests;

        internal bool HasForum;
        internal string ForumName;
        internal string ForumDescription;
        internal uint ForumMessagesCount;
        internal double ForumScore;
        internal uint ForumLastPosterId;
        internal string ForumLastPosterName;
        internal int ForumLastPosterTimestamp;
 
        internal int ForumLastPostTime
        {
            get
            {
                // En segundos
                return (CyberEnvironment.GetUnixTimestamp() - ForumLastPosterTimestamp);
            }
        }

		internal Guild(uint Id, string Name, string Desc, uint RoomId, string Badge, int Create, uint Creator, int Colour1, int Colour2, Dictionary<uint, GroupUser> Members, List<uint> Requests, Dictionary<uint, GroupUser> Admins, uint State, uint AdminOnlyDeco,  bool HasForum, string ForumName, string ForumDescription, uint ForumMessagesCount, double ForumScore, uint ForumLastPosterId, string ForumLastPosterName, int ForumLastPosterTimestamp)
		{
			this.Id = Id;
			this.Name = Name;
			this.Description = Desc;
			this.RoomId = RoomId;
			this.Badge = Badge;
			this.CreateTime = Create;
			this.CreatorId = Creator;
			this.Colour1 = ((Colour1 == 0) ? 1 : Colour1);
			this.Colour2 = ((Colour2 == 0) ? 1 : Colour2);
			this.Members = Members;
			this.Requests = Requests;
			this.Admins = Admins;
			this.State = State;
			this.AdminOnlyDeco = AdminOnlyDeco;

            //Cyber
            this.HasForum = HasForum;
            this.ForumName = ForumName;
            this.ForumDescription = ForumDescription;
            this.ForumMessagesCount = ForumMessagesCount;
            this.ForumScore = ForumScore;
            this.ForumLastPosterId = ForumLastPosterId;
            this.ForumLastPosterName = ForumLastPosterName;
            this.ForumLastPosterTimestamp = ForumLastPosterTimestamp;
		}

        internal ServerMessage ForumDataMessage(uint RequesterId)
        {
            ServerMessage Message = new ServerMessage(Outgoing.GroupForumDataMessageComposer);
            Message.AppendUInt(this.Id);
            Message.AppendString(this.Name);//nombre del foro
            Message.AppendString(this.Description);
            Message.AppendString(this.Badge);
            Message.AppendInt32(0);// nosé
            Message.AppendInt32(0);
            Message.AppendUInt(this.ForumMessagesCount);//Mensajes
            Message.AppendInt32(0);//Mensajes no leídos
            Message.AppendInt32(0);//mensajes?
            Message.AppendUInt(this.ForumLastPosterId);//Id de quien publicó el último mensaje
            Message.AppendString(this.ForumLastPosterName);//Quién publicó eL último mensaje.
            Message.AppendInt32(this.ForumLastPostTime);//hace cuantos segundos se publicó
            Message.AppendInt32(0);
            Message.AppendInt32(1);
            Message.AppendInt32(1);
            Message.AppendInt32(2);
            Message.AppendString(""); // (si no está vacío: Acceso denegado para ver el foro)
            Message.AppendString((this.Members.ContainsKey(RequesterId) ? "" : "not_member"));
            Message.AppendString((this.Members.ContainsKey(RequesterId) ? "" : "not_member"));
            Message.AppendString((this.Admins.ContainsKey(RequesterId) ? "" : "not_admin"));
            Message.AppendString(""); // 
            Message.AppendBoolean(false);
            Message.AppendBoolean(false);
            return Message;
        }

        internal void SerializeForumRoot(ServerMessage Message)
        {
            Message.AppendUInt(this.Id);
            Message.AppendString(this.Name);//nombre del foro
            Message.AppendString("");
            Message.AppendString(this.Badge);
            Message.AppendInt32(0);// nosé
            Message.AppendInt32((int)Math.Round(this.ForumScore)); // Puntaje del foro
            Message.AppendUInt(this.ForumMessagesCount);//Mensajes
            Message.AppendInt32(0);//Mensajes no leídos
            Message.AppendInt32(0);//mensajes?
            Message.AppendUInt(this.ForumLastPosterId);//Id de quien publicó el último mensaje
            Message.AppendString(this.ForumLastPosterName);//Quién publicó eL último mensaje.
            Message.AppendInt32(this.ForumLastPostTime);//hace cuantos segundos se publicó
        }

        internal void UpdateForum()
        {
            if (!HasForum)
            {
                return;
            }
            using (IQueryAdapter Adapter = CyberEnvironment.GetDatabaseManager().getQueryReactor())
            {
                Adapter.setQuery("UPDATE groups SET has_forum = '1', forum_name = @name , forum_description = @desc , forum_messages_count = @msgcount , forum_score = @score , forum_lastposter_id = @lastposterid , forum_lastposter_name = @lastpostername , forum_lastposter_timestamp = @lasttimestamp WHERE id =" + Id);
                Adapter.addParameter("name", ForumName);
                Adapter.addParameter("desc", ForumDescription);
                Adapter.addParameter("msgcount", ForumMessagesCount);
                Adapter.addParameter("score", ForumScore.ToString());
                Adapter.addParameter("lastposterid", ForumLastPosterId);
                Adapter.addParameter("lastpostername", ForumLastPosterName);
                Adapter.addParameter("lasttimestamp", ForumLastPosterTimestamp);
                Adapter.runQuery();
            }
        }
    }
}
