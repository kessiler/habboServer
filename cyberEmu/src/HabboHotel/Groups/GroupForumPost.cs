using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace Cyber.HabboHotel.Groups
{
    class GroupForumPost
    {
        internal uint Id;
        internal uint ParentId;
        internal uint GroupId;
        internal int Timestamp;

        internal bool Pinned;
        internal bool Locked;
        internal bool Hidden;

        internal uint PosterId;
        internal string PosterName;
        internal string PosterLook;

        internal string Subject;
        internal string PostContent;

        internal int MessageCount;
        internal string Hider;

        internal GroupForumPost(DataRow Row)
        {
            this.Id = uint.Parse(Row["id"].ToString());
            this.ParentId = uint.Parse(Row["parent_id"].ToString());
            this.GroupId = uint.Parse(Row["group_id"].ToString());
            this.Timestamp = int.Parse(Row["timestamp"].ToString());
            this.Pinned = Row["pinned"].ToString() == "1";
            this.Locked = Row["locked"].ToString() == "1";
            this.Hidden = Row["hidden"].ToString() == "1";

            this.PosterId = uint.Parse(Row["poster_id"].ToString());
            this.PosterName = Row["poster_name"].ToString();
            this.PosterLook = Row["poster_look"].ToString();
            this.Subject = Row["subject"].ToString();
            this.PostContent = Row["post_content"].ToString();
            this.Hider = Row["post_hider"].ToString();

            this.MessageCount = 0;
            if (ParentId == 0)
            {
                this.MessageCount = CyberEnvironment.GetGame().GetGroupManager().GetMessageCountForThread(Id);
            }
        }
    }
}
