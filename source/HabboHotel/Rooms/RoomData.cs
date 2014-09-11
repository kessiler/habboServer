using Database_Manager.Database.Session_Details.Interfaces;
using Cyber.Core;
using Cyber.HabboHotel.GameClients;
using Cyber.HabboHotel.Groups;
using Cyber.Messages;
using Cyber.Messages.Headers;
using System;
using System.Collections.Generic;
using System.Data;
namespace Cyber.HabboHotel.Rooms
{
	internal class RoomData
	{
		internal uint Id;
		internal string Name;
		internal string Description;
		internal string Type;
		internal string Owner;
		internal int OwnerId;
		internal string Password;
		internal int State;
		internal int TradeState;
		internal int Category;
		internal int UsersNow;
		internal int UsersMax;
		internal string ModelName;
		internal string CCTs;
		internal int Score;
		internal List<string> Tags;
		internal int AllowPets;
		internal int AllowPetsEating;
		internal int AllowWalkthrough;
		internal int ChatType;
		internal int ChatBalloon;
		internal int ChatSpeed;
		internal int ChatMaxDistance;
		internal int ChatFloodProtection;
		internal bool AllowRightsOverride;
		internal int Hidewall;
		internal string Wallpaper;
		internal string Floor;
		internal string Landscape;
		private RoomModel mModel;
		internal int WallThickness;
		internal int FloorThickness;
		internal Guild Group;
		internal RoomEvent Event;
		internal int GameId;
		internal int WhoCanKick;
		internal int WhoCanBan;
		internal int WhoCanMute;
		internal uint GroupId;
		internal HashSet<Chatlog> RoomChat;
		internal List<string> WordFilter;
		internal int WallHeight;
		internal int TagCount
		{
			get
			{
				return this.Tags.Count;
			}
		}
		internal bool HasEvent
		{
			get
			{
				return false;
			}
		}
        internal void ResetModel()
        {
           this.mModel = CyberEnvironment.GetGame().GetRoomManager().GetModel(this.ModelName);
        }
		internal RoomModel Model
		{
			get
			{
				if (this.mModel == null)
				{
					this.mModel = CyberEnvironment.GetGame().GetRoomManager().GetModel(this.ModelName);
				}
				return this.mModel;
			}
		}
		internal RoomData()
		{
		}
		internal void FillNull(uint pId)
		{
			this.Id = pId;
			this.Name = "Unknown Room";
			this.Description = "-";
			this.Type = "private";
			this.Owner = "-";
			this.Category = 0;
			this.UsersNow = 0;
			this.UsersMax = 0;
			this.ModelName = "NO_MODEL";
			this.CCTs = "";
			this.Score = 0;
			this.Tags = new List<string>();
			this.AllowPets = 1;
			this.AllowPetsEating = 0;
			this.AllowWalkthrough = 1;
			this.Hidewall = 0;
			this.Password = "";
			this.Wallpaper = "0.0";
			this.Floor = "0.0";
			this.Landscape = "0.0";
			this.WallThickness = 0;
			this.FloorThickness = 0;
			this.Group = null;
			this.AllowRightsOverride = false;
			this.Event = null;
			this.GameId = 0;
			this.WhoCanBan = 0;
			this.WhoCanKick = 0;
			this.WhoCanMute = 0;
			this.TradeState = 2;
			this.State = 0;
			this.RoomChat = new HashSet<Chatlog>();
			this.WordFilter = new List<string>();
			this.WallHeight = -1;
			this.mModel = CyberEnvironment.GetGame().GetRoomManager().GetModel(this.ModelName);
		}
		internal void Fill(DataRow Row)
		{
			this.Id = Convert.ToUInt32(Row["id"]);
			this.Name = (string)Row["caption"];
			this.Description = (string)Row["description"];
			this.Type = (string)Row["roomtype"];
			this.Owner = (string)Row["owner"];
			this.OwnerId = 0;
            this.RoomChat = new HashSet<Chatlog>();
			this.WordFilter = new List<string>();
			using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
			{
				queryreactor.setQuery("SELECT id FROM users WHERE username = @owner");
				queryreactor.addParameter("owner", this.Owner);
				int integer = queryreactor.getInteger();
				if (integer > 0)
				{
					this.OwnerId = integer;
				}
				queryreactor.setQuery("SELECT user_id, message, timestamp FROM chatlogs WHERE room_id=@id ORDER BY timestamp DESC LIMIT 150");
				queryreactor.addParameter("id", this.Id);
				DataTable table = queryreactor.getTable();
				foreach (DataRow dataRow in table.Rows)
				{
					this.RoomChat.Add(new Chatlog((uint)dataRow[0], (string)dataRow[1], Convert.ToDouble(dataRow[2]), false));
				}
				queryreactor.setQuery("SELECT word FROM room_wordfilter WHERE room_id = @id");
				queryreactor.addParameter("id", this.Id);
				DataTable table2 = queryreactor.getTable();
				foreach (DataRow dataRow2 in table2.Rows)
				{
					this.WordFilter.Add(dataRow2["word"].ToString());
				}
			}
			string a;
			if ((a = Row["state"].ToString().ToLower()) != null)
			{
				if (a == "open")
				{
					this.State = 0;
					goto IL_24B;
				}
				if (a == "password")
				{
					this.State = 2;
					goto IL_24B;
				}
				if (!(a == "locked"))
				{
				}
			}
			this.State = 1;
			IL_24B:
			this.TradeState = int.Parse(Row["trade_state"].ToString());
			this.Category = (int)Row["category"];
			if (!string.IsNullOrEmpty(Row["users_now"].ToString()))
			{
				this.UsersNow = (int)Row["users_now"];
			}
			else
			{
				this.UsersNow = 0;
			}
			this.UsersMax = (int)Row["users_max"];
			this.ModelName = (string)Row["model_name"];
            this.WallHeight = int.Parse(Row["walls_height"].ToString());
			this.CCTs = (string)Row["public_ccts"];
			this.Score = (int)Row["score"];
			this.Tags = new List<string>();
			this.AllowPets = Convert.ToInt32(Row["allow_pets"].ToString());
			this.AllowPetsEating = Convert.ToInt32(Row["allow_pets_eat"].ToString());
			this.AllowWalkthrough = Convert.ToInt32(Row["allow_walkthrough"].ToString());
			this.AllowRightsOverride = false;
			this.Hidewall = Convert.ToInt32(Row["allow_hidewall"].ToString());
			this.Password = (string)Row["password"];
			this.Wallpaper = (string)Row["wallpaper"];
			this.Floor = (string)Row["floor"];
			this.Landscape = (string)Row["landscape"];
			this.FloorThickness = (int)Row["floorthick"];
			this.WallThickness = (int)Row["wallthick"];
			this.ChatType = (int)Row["chat_type"];
			this.ChatBalloon = (int)Row["chat_balloon"];
			this.ChatSpeed = (int)Row["chat_speed"];
			this.ChatMaxDistance = (int)Row["chat_max_distance"];
			this.ChatFloodProtection = (int)Row["chat_flood_protection"];
			this.GameId = (int)Row["game_id"];
			this.WhoCanMute = Convert.ToInt32(Row["mute_settings"]);
			this.WhoCanKick = Convert.ToInt32(Row["kick_settings"]);
			this.WhoCanBan = Convert.ToInt32(Row["ban_settings"]);
			this.GroupId = (uint)Row["group_id"];
			this.Group = CyberEnvironment.GetGame().GetGroupManager().GetGroup(this.GroupId);
			this.Event = CyberEnvironment.GetGame().GetRoomEvents().GetEvent(this.Id);
			Dictionary<int, int> dictionary = new Dictionary<int, int>();
			if (!string.IsNullOrEmpty(Row["icon_items"].ToString()))
			{
				string[] array = Row["icon_items"].ToString().Split(new char[]
				{
					'|'
				});
				for (int i = 0; i < array.Length; i++)
				{
					string text = array[i];
					if (!string.IsNullOrEmpty(text))
					{
						string[] array2 = text.Replace('.', ',').Split(new char[]
						{
							','
						});
						int key = 0;
						int value = 0;
						int.TryParse(array2[0], out key);
						if (array2.Length > 1)
						{
							int.TryParse(array2[1], out value);
						}
						try
						{
							if (!dictionary.ContainsKey(key))
							{
								dictionary.Add(key, value);
							}
						}
						catch (Exception ex)
						{
							Logging.LogException(string.Concat(new string[]
							{
								"Exception: ",
								ex.ToString(),
								"[",
								text,
								"]"
							}));
						}
					}
				}
			}
            if (Row["tags"].ToString() != "")
            {
                string[] array3 = Row["tags"].ToString().Split(new char[]
			{
				','
			});
                for (int j = 0; j < array3.Length; j++)
                {
                    string item = array3[j];
                    this.Tags.Add(item);
                }
            }
			this.mModel = CyberEnvironment.GetGame().GetRoomManager().GetModel(this.ModelName);
		}
		internal void Fill(Room Room)
		{
			this.Id = Room.RoomId;
			this.Name = Room.Name;
			this.Description = Room.Description;
			this.Type = Room.Type;
			this.Owner = Room.Owner;
			this.Category = Room.Category;
			this.State = Room.State;
			this.UsersNow = Room.UsersNow;
			this.UsersMax = Room.UsersMax;
			this.ModelName = Room.ModelName;
            this.WallHeight = Room.WallHeight;
			this.Score = Room.Score;
			this.Tags = new List<string>();
			object[] array = Room.Tags.ToArray();
			for (int i = 0; i < array.Length; i++)
			{
				string item = (string)array[i];
				this.Tags.Add(item);
			}
			this.AllowPets = Room.AllowPets;
			this.AllowPetsEating = Room.AllowPetsEating;
			this.AllowWalkthrough = Room.AllowWalkthrough;
			this.Hidewall = Room.Hidewall;
			this.Password = Room.Password;
			this.Wallpaper = Room.Wallpaper;
			this.Floor = Room.Floor;
			this.Landscape = Room.Landscape;
			this.FloorThickness = Room.FloorThickness;
			this.WallThickness = Room.WallThickness;
			this.Group = Room.Group;
			this.Event = Room.Event;
			this.ChatType = Room.ChatType;
			this.ChatBalloon = Room.ChatBalloon;
			this.ChatSpeed = Room.ChatSpeed;
			this.ChatMaxDistance = Room.ChatMaxDistance;
			this.ChatFloodProtection = Room.ChatFloodProtection;
			this.WhoCanMute = Room.WhoCanMute;
			this.WhoCanKick = Room.WhoCanKick;
			this.WhoCanBan = Room.WhoCanBan;
			this.RoomChat = Room.RoomChat;
			this.WordFilter = Room.WordFilter;
			this.mModel = CyberEnvironment.GetGame().GetRoomManager().GetModel(this.ModelName);
		}
		internal void Serialize(ServerMessage Message, bool ShowEvents)
		{
			Message.AppendUInt(this.Id);
			Message.AppendString(this.Name);
			Message.AppendBoolean(this.Type == "private");
			Message.AppendInt32(this.OwnerId);
			Message.AppendString(this.Owner);
			Message.AppendInt32(this.State);
			Message.AppendInt32(this.UsersNow);
			Message.AppendInt32(this.UsersMax);
			Message.AppendString(this.Description);
			Message.AppendInt32(this.TradeState);
			Message.AppendInt32(this.Score);
			Message.AppendInt32(0);
            Message.AppendInt32(0);
			Message.AppendInt32(this.Category);
			if (this.Group != null)
			{
				Message.AppendUInt(this.Group.Id);
				Message.AppendString(this.Group.Name);
				Message.AppendString(this.Group.Badge);
				Message.AppendString("");
			}
			else
			{
				Message.AppendInt32(0);
				Message.AppendString("");
				Message.AppendString("");
				Message.AppendString("");
			}
			Message.AppendInt32(this.TagCount);
			foreach (string current in this.Tags)
			{
				Message.AppendString(current);
			}
			Message.AppendInt32(0);
			Message.AppendInt32(0);
			Message.AppendBoolean(false);
			Message.AppendBoolean(false);
			if (this.Event != null)
			{
				if (this.Event.HasExpired)
				{
					CyberEnvironment.GetGame().GetRoomEvents().RemoveEvent(this.Id);
				}
				Message.AppendInt32(1);
				Message.AppendString(this.Event.Name);
				Message.AppendString(this.Event.Description);
				Message.AppendInt32(checked((int)Math.Floor((double)(this.Event.Time - CyberEnvironment.GetUnixTimestamp()) / 60.0)));
				return;
			}
			Message.AppendInt32(0);
			Message.AppendString("");
			Message.AppendString("");
			Message.AppendInt32(0);
		}
		internal void SerializeRoomData(ServerMessage Response, bool FromView, GameClient Session, bool SendRoom = false)
		{
			Room room = CyberEnvironment.GetGame().GetRoomManager().GetRoom(Session.GetHabbo().CurrentRoomId);
			if (room == null || !room.CheckRights(Session, true, false))
			{
				return;
			}
			Response.Init(Outgoing.RoomDataMessageComposer);
			Response.AppendBoolean(true);
			Response.AppendUInt(this.Id);
			Response.AppendString(this.Name);
			Response.AppendBoolean(this.Type == "private");
			Response.AppendInt32(this.OwnerId);
			Response.AppendString(this.Owner);
			Response.AppendInt32(this.State);
			Response.AppendInt32(this.UsersNow);
			Response.AppendInt32(this.UsersMax);
			Response.AppendString(this.Description);
			Response.AppendInt32(this.TradeState);
			Response.AppendInt32(this.Score);
			Response.AppendInt32(0);
            Response.AppendInt32(0);
			Response.AppendInt32(this.Category);
			if (this.GroupId > 0u)
			{
				Response.AppendUInt(this.Group.Id);
				Response.AppendString(this.Group.Name);
				Response.AppendString(this.Group.Badge);
				Response.AppendString("");
			}
			else
			{
				Response.AppendInt32(0);
				Response.AppendString("");
				Response.AppendString("");
				Response.AppendString("");
			}
			Response.AppendInt32(this.TagCount);
			string[] array = this.Tags.ToArray();
			for (int i = 0; i < array.Length; i++)
			{
				string s = array[i];
				Response.AppendString(s);
			}
			Response.AppendInt32(0);
			Response.AppendInt32(0);
			Response.AppendInt32(0);
			Response.AppendBoolean(this.AllowPets == 1);
			Response.AppendBoolean(this.AllowPetsEating == 1);
			Response.AppendString("");
			Response.AppendString("");
			Response.AppendInt32(0);
			Response.AppendBoolean(FromView);
			Response.AppendBoolean(CyberEnvironment.GetGame().GetNavigator().RoomIsPublicItem(this.Id));
			Response.AppendBoolean(false);
			Response.AppendBoolean(false);
			Response.AppendInt32(this.WhoCanMute);
			Response.AppendInt32(this.WhoCanKick);
			Response.AppendInt32(this.WhoCanBan);
			Response.AppendBoolean(room.CheckRights(Session, true, false));
			Response.AppendInt32(this.ChatType);
			Response.AppendInt32(this.ChatBalloon);
			Response.AppendInt32(this.ChatSpeed);
			Response.AppendInt32(this.ChatMaxDistance);
			Response.AppendInt32(this.ChatFloodProtection);
			if (SendRoom)
			{
				if (CyberEnvironment.GetGame().GetRoomManager().GetRoom(this.Id) != null)
				{
					CyberEnvironment.GetGame().GetRoomManager().GetRoom(this.Id).SendMessage(Response);
					return;
				}
			}
			else
			{
				Session.SendMessage(Response);
			}
		}
	}
}
