using Cyber.Core;
using Cyber.Util;
using Cyber.HabboHotel.GameClients;
using Cyber.HabboHotel.Groups;
using Cyber.HabboHotel.Items;
using Cyber.HabboHotel.PathFinding;
using Cyber.HabboHotel.Pets;
using Cyber.HabboHotel.Quests;
using Cyber.HabboHotel.RoomBots;
using Cyber.HabboHotel.Rooms.Games;
using Cyber.HabboHotel.Rooms.Wired;
using Cyber.HabboHotel.Users;
using Cyber.Messages;
using Cyber.Messages.Headers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
namespace Cyber.HabboHotel.Rooms
{
	public class RoomUser : IEquatable<RoomUser>
	{
		internal uint HabboId;
		internal int VirtualId;
		internal uint RoomId;
        internal uint UserID;

		internal bool InteractingGate;
		internal uint GateId;
		internal int LastInteraction;
		internal int LockedTilesCount;
        internal int CarryItemID;
        internal int CarryTimer;
        internal int SignTime;
        internal int IdleTime;

        internal int X;
        internal int Y;
        internal double Z;
        internal byte SqState;
        internal int RotHead;
        internal int RotBody;
        internal bool CanWalk;
        internal bool AllowOverride;
        internal bool TeleportEnabled;
        internal int GoalX;
        internal int GoalY;
        
        internal List<Vector2D> Path = new List<Vector2D>();
		internal bool PathRecalcNeeded;
		internal int PathStep = 1;
		internal bool SetStep;
		internal int SetX;
		internal int SetY;
		internal double SetZ;
		
		internal RoomBot BotData;
		internal BotAI BotAI;
		internal ItemEffectType CurrentItemEffect;
		internal bool Freezed;//En el freeze
		internal bool Frozen;//por comando
		internal int FreezeCounter;
		internal Team team;
		internal FreezePowerUp banzaiPowerUp;
		internal int FreezeLives;
		internal bool shieldActive;
		internal int shieldCounter;
		internal bool throwBallAtGoal;
		internal bool IsMoonwalking;
		internal bool IsSitting;
		internal bool IsLyingDown;
		internal bool HasPathBlocked;
        internal bool IsFlooded;
        internal int FloodExpiryTime;

		internal bool RidingHorse;
		internal uint HorseID;

		internal uint LastItem;
		internal bool OnCampingTent;
		internal bool FastWalking;
        internal int LastBubble = 0;
		internal Pet PetData;
		internal bool IsWalking;
		internal bool UpdateNeeded;
		internal bool IsAsleep;
		internal Dictionary<string, string> Statusses;
		internal int DanceId;
		internal int TeleDelay;
		private int FloodCount;
		internal bool IsSpectator;
		internal int InternalRoomID;
		private Queue events;
		private GameClient mClient;
		private Room mRoom;
		internal Point Coordinate
		{
			get
			{
				return new Point(this.X, this.Y);
			}
		}

       internal Point SquareBehind
        {
            get
            {
                int x = X;
                int y = Y;

                switch (RotBody)
                {
                    case 0:
                        y++;
                        break;

                    case 1:
                        x--;
                        y++;
                        break;
  
                    case 2:
                        x--;
                        break;

                    case 3:
                        x--;
                        y--;
                        break;
                        
                    case 4:
                        y--;
                        break;

                    case 5:
                        x++;
                        y--;
                        break;

                    case 6:
                        x++;
                        break;

                    case 7:
                        x++; y++;
                        break;
                }

                return new Point(x, y);
            }
        }

		internal Point SquareInFront
		{
			get
			{
				checked
				{
					int x = this.X + 1;
					int y = 0;
					if (this.RotBody == 0)
					{
						x = this.X;
						y = this.Y - 1;
					}
					else
					{
						if (this.RotBody == 1)
						{
							x = this.X + 1;
							y = this.Y - 1;
						}
						else
						{
							if (this.RotBody == 2)
							{
								x = this.X + 1;
								y = this.Y;
							}
							else
							{
								if (this.RotBody == 3)
								{
									x = this.X + 1;
									y = this.Y + 1;
								}
								else
								{
									if (this.RotBody == 4)
									{
										x = this.X;
										y = this.Y + 1;
									}
									else
									{
										if (this.RotBody == 5)
										{
											x = this.X - 1;
											y = this.Y + 1;
										}
										else
										{
											if (this.RotBody == 6)
											{
												x = this.X - 1;
												y = this.Y;
											}
											else
											{
												if (this.RotBody == 7)
												{
													x = this.X - 1;
													y = this.Y - 1;
												}
											}
										}
									}
								}
							}
						}
					}
					return new Point(x, y);
				}
			}
		}
		internal bool IsPet
		{
			get
			{
				return this.IsBot && this.BotData.IsPet;
			}
		}
		internal int CurrentEffect
		{
			get
			{
				return this.GetClient().GetHabbo().GetAvatarEffectsInventoryComponent().CurrentEffect;
			}
		}
		internal bool IsDancing
		{
			get
			{
				return this.DanceId >= 1;
			}
		}
		internal bool NeedsAutokick
		{
			get
			{
				return !this.IsBot && (this.GetClient() == null || this.GetClient().GetHabbo() == null || (this.GetClient().GetHabbo().Rank <= 6u && this.IdleTime >= 1800));
			}
		}
		internal bool IsTrading
		{
			get
			{
				return !this.IsBot && this.Statusses.ContainsKey("trd");
			}
		}
		internal bool IsBot
		{
			get
			{
				return this.BotData != null;
			}
		}
		public bool Equals(RoomUser comparedUser)
		{
			return comparedUser.HabboId == this.HabboId;
		}
		internal string GetUsername()
		{
			if (this.IsBot)
			{
                if (!this.IsPet)
                {
                    return (BotData == null) ? "" : BotData.Name;
                }
                return PetData.Name;
			}
			if (this.GetClient() != null)
			{
				return this.GetClient().GetHabbo().Username;
			}
			return string.Empty;
		}
		internal bool IsOwner()
		{
			return !this.IsBot && this.GetUsername() == this.GetRoom().Owner;
		}
		internal RoomUser(uint HabboId, uint RoomId, int VirtualId, Room room, bool isSpectator)
		{
			this.Freezed = false;
			this.HabboId = HabboId;
			this.RoomId = RoomId;
			this.VirtualId = VirtualId;
			this.IdleTime = 0;
			this.X = 0;
			this.Y = 0;
			this.Z = 0.0;
			this.RotHead = 0;
			this.RotBody = 0;
			this.UpdateNeeded = true;
			this.Statusses = new Dictionary<string, string>();
			this.TeleDelay = -1;
			this.mRoom = room;
			this.AllowOverride = false;
			this.CanWalk = true;
			this.IsSpectator = isSpectator;
			this.SqState = 3;
			this.InternalRoomID = 0;
			this.CurrentItemEffect = ItemEffectType.None;
			this.events = new Queue();
			this.FreezeLives = 0;
			this.InteractingGate = false;
			this.GateId = 0u;
			this.LastInteraction = 0;
			this.LockedTilesCount = 0;
		}
		internal RoomUser(uint HabboId, uint RoomId, int VirtualId, GameClient pClient, Room room)
		{
			this.mClient = pClient;
			this.Freezed = false;
			this.HabboId = HabboId;
			this.RoomId = RoomId;
			this.VirtualId = VirtualId;
			this.IdleTime = 0;
			this.X = 0;
			this.Y = 0;
			this.Z = 0.0;
			this.RotHead = 0;
			this.RotBody = 0;
			this.UpdateNeeded = true;
			this.Statusses = new Dictionary<string, string>();
			this.TeleDelay = -1;
			this.LastInteraction = 0;
			this.AllowOverride = false;
			this.CanWalk = true;
			if (this.GetClient().GetHabbo().SpectatorMode)
			{
				this.IsSpectator = true;
			}
			else
			{
				this.IsSpectator = false;
			}
			this.SqState = 3;
			this.InternalRoomID = 0;
			this.CurrentItemEffect = ItemEffectType.None;
			this.mRoom = room;
			this.events = new Queue();
			this.InteractingGate = false;
			this.GateId = 0u;
			this.LockedTilesCount = 0;
		}
		internal void UnIdle()
		{
			this.IdleTime = 0;
			if (this.IsAsleep)
			{
				this.IsAsleep = false;
                ServerMessage Sleep = new ServerMessage(Outgoing.RoomUserIdleMessageComposer);
                Sleep.AppendInt32(VirtualId);
                Sleep.AppendBoolean(false);
                GetRoom().SendMessage(Sleep);
			}
		}
		internal void Dispose()
		{
			this.Statusses.Clear();
			this.mRoom = null;
			this.mClient = null;
		}

        internal void Chat(GameClient Session, string Message, bool Shout, int count, int TextColor = 0)
        {
            if (IsPet || IsBot)
            {
                if (!IsPet) { TextColor = 2; }

                ServerMessage botChatmsg = new ServerMessage();
                botChatmsg.Init(Shout ? Outgoing.ShoutMessageComposer : Outgoing.ChatMessageComposer);
                botChatmsg.AppendInt32(VirtualId);
                botChatmsg.AppendString(Message);
                botChatmsg.AppendInt32(0);
                botChatmsg.AppendInt32(TextColor);
                botChatmsg.AppendInt32(0);
                botChatmsg.AppendInt32(count);

                this.GetRoom().SendMessage(botChatmsg);
                return;
            }

            if (Session.GetHabbo().Rank <= 9 && AntiPublicistas.CheckPublicistas(Message))
            {
                Session.PublicistaCount++;
                Session.HandlePublicista(Message);
                return;
            }

            if (!IsBot && IsFlooded && FloodExpiryTime <= CyberEnvironment.GetUnixTimestamp())
            {
                IsFlooded = false;
            }
            else if (!IsBot && IsFlooded)
                return; // ciao flooders!

            if (Session.GetHabbo().Rank < 4 && GetRoom().CheckMute(Session))
            {
                return;
            }

                this.UnIdle();
                if (!this.IsPet && !this.IsBot)
                {
                    if (Message.StartsWith(":") && GetClient().GetHabbo().GetCommandHandler().Parse(Message))
                    {
                        return;
                    }

                    ServerMessage message;
                    Habbo habbo = this.GetClient().GetHabbo();

                    if (GetRoom().GetWiredHandler().ExecuteWired(WiredItemType.TriggerUserSaysKeyword, new object[]
			{
				this,
				Message
			}))
                    {
                        return;
                    }


                    uint rank = 1;

                    if ((Session != null) && (Session.GetHabbo() != null))
                    {
                        rank = Session.GetHabbo().Rank;
                    }

                    GetRoom().AddChatlog(Session.GetHabbo().Id, Message, false);

                    foreach (string current in GetRoom().WordFilter)
                    {
                        Message = System.Text.RegularExpressions.Regex.Replace(Message, current, "bobba", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                    }

                    if (rank < 4)
                    {
                        TimeSpan span = (TimeSpan)(DateTime.Now - habbo.spamFloodTime);
                        if ((span.TotalSeconds > habbo.spamProtectionTime) && habbo.spamProtectionBol)
                        {
                            this.FloodCount = 0;
                            habbo.spamProtectionBol = false;
                            habbo.spamProtectionAbuse = 0;
                        }
                        else if (span.TotalSeconds > 4.0)
                        {
                            this.FloodCount = 0;
                        }
                        if ((span.TotalSeconds < habbo.spamProtectionTime) && habbo.spamProtectionBol)
                        {
                            message = new ServerMessage(Outgoing.FloodFilterMessageComposer);
                            int i = habbo.spamProtectionTime - span.Seconds;
                            message.AppendInt32(i);
                            this.IsFlooded = true;
                            this.FloodExpiryTime = CyberEnvironment.GetUnixTimestamp() + i;
                            this.GetClient().SendMessage(message);
                            return;
                        }
                        if (((span.TotalSeconds < 4.0) && (this.FloodCount > 5)) && (rank < 5))
                        {
                            message = new ServerMessage(Outgoing.FloodFilterMessageComposer);
                            habbo.spamProtectionCount++;
                            if ((habbo.spamProtectionCount % 2) == 0)
                            {
                                habbo.spamProtectionTime = 10 * habbo.spamProtectionCount;
                            }
                            else
                            {
                                habbo.spamProtectionTime = 10 * (habbo.spamProtectionCount - 1);
                            }
                            habbo.spamProtectionBol = true;
                            int j = (int)(habbo.spamProtectionTime - span.Seconds);
                            message.AppendInt32(j);
                            this.IsFlooded = true;
                            this.FloodExpiryTime = CyberEnvironment.GetUnixTimestamp() + j;
                            this.GetClient().SendMessage(message);
                            return;
                        }
                        habbo.spamFloodTime = DateTime.Now;
                        this.FloodCount++;
                    }
                }
                else
                {
                    if (!IsPet)
                    {
                        TextColor = 2;
                    }
                }

                ServerMessage chatMsg = new ServerMessage();
                chatMsg.Init(Shout ? Outgoing.ShoutMessageComposer : Outgoing.ChatMessageComposer);
                chatMsg.AppendInt32(VirtualId);
                chatMsg.AppendString(Message);
                chatMsg.AppendInt32(RoomUser.GetSpeechEmotion(Message));
                chatMsg.AppendInt32(TextColor);
                chatMsg.AppendInt32(0);
                chatMsg.AppendInt32(count);
                GetRoom().BroadcastChatMessage(chatMsg, this, Session.GetHabbo().Id);

                this.GetRoom().OnUserSay(this, Message, Shout);

                this.GetRoom().GetRoomUserManager().TurnHeads(this.X, this.Y, this.HabboId);
                
        }
		

		internal bool IncrementAndCheckFlood(out int MuteTime)
		{
			MuteTime = 20;
			TimeSpan timeSpan = DateTime.Now - this.GetClient().GetHabbo().spamFloodTime;
			if (timeSpan.TotalSeconds > (double)this.GetClient().GetHabbo().spamProtectionTime && this.GetClient().GetHabbo().spamProtectionBol)
			{
				this.FloodCount = 0;
				this.GetClient().GetHabbo().spamProtectionBol = false;
				this.GetClient().GetHabbo().spamProtectionAbuse = 0;
			}
			else
			{
				if (timeSpan.TotalSeconds > 2.0)
				{
					this.FloodCount = 0;
				}
			}
			checked
			{
				if (timeSpan.TotalSeconds < 2.0 && this.FloodCount > 6 && this.GetClient().GetHabbo().Rank < 5u)
				{
					MuteTime = this.GetClient().GetHabbo().spamProtectionTime - timeSpan.Seconds + 30;
					return true;
				}
				this.GetClient().GetHabbo().spamFloodTime = DateTime.Now;
				this.FloodCount++;
				return false;
			}
		}

		internal static int GetSpeechEmotion(string Message)
		{
			Message = Message.ToLower();
			if (Message.Contains(":)") || Message.Contains(":d") || Message.Contains("=]") || Message.Contains("=d") || Message.Contains(":>"))
			{
				return 1;
			}
			if (Message.Contains(">:(") || Message.Contains(":@"))
			{
				return 2;
			}
			if (Message.Contains(":o"))
			{
				return 3;
			}
			if (Message.Contains(":(") || Message.Contains("=[") || Message.Contains(":'(") || Message.Contains("='["))
			{
				return 4;
			}
			return 0;
		}
		internal void ClearMovement(bool Update)
		{
			this.IsWalking = false;
			this.Statusses.Remove("mv");
			this.GoalX = 0;
			this.GoalY = 0;
			this.SetStep = false;
            try
            {
                GetRoom().GetRoomUserManager().ToSet.Remove(new Point(this.SetX, this.SetY));
            }
            catch (Exception) { }
			this.SetX = 0;
			this.SetY = 0;
			this.SetZ = 0.0;
			if (Update)
			{
				this.UpdateNeeded = true;
			}
		}
		internal void MoveTo(Point c)
		{
			this.MoveTo(c.X, c.Y);
		}

		internal void MoveTo(int pX, int pY, bool pOverride)
		{
			if (this.TeleportEnabled)
			{
				this.UnIdle();
				this.GetRoom().SendMessage(this.GetRoom().GetRoomItemHandler().UpdateUserOnRoller(this, new Point(pX, pY), 0u, this.GetRoom().GetGameMap().SqAbsoluteHeight(this.GoalX, this.GoalY)));
				if (this.Statusses.ContainsKey("sit"))
				{
					this.Z -= 0.35;
				}
				this.UpdateNeeded = true;
				this.GetRoom().GetRoomUserManager().UpdateUserStatus(this, false);
				return;
			}
			if (this.GetRoom().GetGameMap().SquareHasUsers(pX, pY) && !pOverride)
			{
				return;
			}
            if (this.Frozen)
			{
				return;
			}
			if (!this.IsBot)
			{
				if (this.IsMoonwalking)
				{
					this.GetClient().GetHabbo().GetAvatarEffectsInventoryComponent().ActivateCustomEffect(136);
				}
				else
				{
					if (!this.IsMoonwalking && this.GetClient().GetHabbo().GetAvatarEffectsInventoryComponent().CurrentEffect == 136)
					{
						this.GetClient().GetHabbo().GetAvatarEffectsInventoryComponent().ActivateCustomEffect(0);
					}
				}
			}
			CoordItemSearch coordItemSearch = new CoordItemSearch(this.GetRoom().GetGameMap().CoordinatedItems);
			List<RoomItem> allRoomItemForSquare = coordItemSearch.GetAllRoomItemForSquare(pX, pY);
			if (this.RidingHorse && !this.IsBot && allRoomItemForSquare.Count > 0)
			{
				foreach (RoomItem current in allRoomItemForSquare)
				{
					if (checked(Math.Abs(this.X - pX) < 2 && Math.Abs(this.Y - pY) < 2) && (current.GetBaseItem().IsSeat || current.GetBaseItem().InteractionType == InteractionType.lowpool || current.GetBaseItem().InteractionType == InteractionType.pool || current.GetBaseItem().InteractionType == InteractionType.haloweenpool))
					{
						return;
					}
				}
			}
			this.UnIdle();
			this.GoalX = pX;
			this.GoalY = pY;
			this.PathRecalcNeeded = true;
            this.throwBallAtGoal = false;
		}
		internal void MoveTo(int pX, int pY)
		{
			this.MoveTo(pX, pY, false);
		}
		internal void UnlockWalking()
		{
			this.AllowOverride = false;
			this.CanWalk = true;
		}
		internal void SetPos(int pX, int pY, double pZ)
		{
			this.X = pX;
			this.Y = pY;
			this.Z = pZ;
		}
		internal void CarryItem(int Item)
		{
			this.CarryItemID = Item;
			if (Item > 0)
			{
				this.CarryTimer = 240;
			}
			else
			{
				this.CarryTimer = 0;
			}
			ServerMessage serverMessage = new ServerMessage(Outgoing.ApplyHanditemMessageComposer);
			serverMessage.AppendInt32(this.VirtualId);
			serverMessage.AppendInt32(Item);
			this.GetRoom().SendMessage(serverMessage);
		}
		internal void SetRot(int Rotation)
		{
			this.SetRot(Rotation, false);
		}
		internal void SetRot(int Rotation, bool HeadOnly)
		{
			if (this.Statusses.ContainsKey("lay") || this.IsWalking)
			{
				return;
			}
			checked
			{
				int num = this.RotBody - Rotation;
				this.RotHead = this.RotBody;
				if (this.Statusses.ContainsKey("sit") || HeadOnly)
				{
					if (this.RotBody == 2 || this.RotBody == 4)
					{
						if (num > 0)
						{
							this.RotHead = this.RotBody - 1;
						}
						else
						{
							if (num < 0)
							{
								this.RotHead = this.RotBody + 1;
							}
						}
					}
					else
					{
						if (this.RotBody == 0 || this.RotBody == 6)
						{
							if (num > 0)
							{
								this.RotHead = this.RotBody - 1;
							}
							else
							{
								if (num < 0)
								{
									this.RotHead = this.RotBody + 1;
								}
							}
						}
					}
				}
				else
				{
					if (num <= -2 || num >= 2)
					{
						this.RotHead = Rotation;
						this.RotBody = Rotation;
					}
					else
					{
						this.RotHead = Rotation;
					}
				}
				this.UpdateNeeded = true;
			}
		}
		internal void SetStatus(string Key, string Value)
		{
			if (this.Statusses.ContainsKey(Key))
			{
				this.Statusses[Key] = Value;
				return;
			}
			this.AddStatus(Key, Value);
		}
		internal void AddStatus(string Key, string Value)
		{
			this.Statusses[Key] = Value;
		}
		internal void RemoveStatus(string Key)
		{
			if (this.Statusses.ContainsKey(Key))
			{
				this.Statusses.Remove(Key);
			}
		}
		internal void ApplyEffect(int effectID)
		{
			if (this.IsBot || this.GetClient() == null || this.GetClient().GetHabbo() == null || this.GetClient().GetHabbo().GetAvatarEffectsInventoryComponent() == null)
			{
				return;
			}
			this.GetClient().GetHabbo().GetAvatarEffectsInventoryComponent().ActivateCustomEffect(effectID);
		}
		internal void Serialize(ServerMessage Message, bool gotPublicRoom)
		{
			if (Message == null)
			{
				return;
			}
			if (this.IsSpectator)
			{
				return;
			}
			if (!this.IsBot)
			{
				if (!this.IsBot && this.GetClient() != null && this.GetClient().GetHabbo() != null)
				{
					Guild group = CyberEnvironment.GetGame().GetGroupManager().GetGroup(this.GetClient().GetHabbo().FavouriteGroup);
					Habbo habbo = this.GetClient().GetHabbo();
					Message.AppendUInt(habbo.Id);
					Message.AppendString(habbo.Username);
					Message.AppendString(habbo.Motto);
					Message.AppendString(habbo.Look);
					Message.AppendInt32(this.VirtualId);
					Message.AppendInt32(this.X);
					Message.AppendInt32(this.Y);
					Message.AppendString(TextHandling.GetString(this.Z));
					Message.AppendInt32(0);
					Message.AppendInt32(1);
					Message.AppendString(habbo.Gender.ToLower());
					if (group != null)
					{
						Message.AppendUInt(group.Id);
						Message.AppendInt32(0);
						Message.AppendString(group.Name);
					}
					else
					{
						Message.AppendInt32(0);
						Message.AppendInt32(0);
						Message.AppendString("");
					}
					Message.AppendString("");
					Message.AppendInt32(habbo.AchievementPoints);
					Message.AppendBoolean(false);
				}
				return;
			}
			Message.AppendUInt(this.BotAI.BaseId);
			Message.AppendString(this.BotData.Name);
			Message.AppendString(this.BotData.Motto);
			if (this.BotData.AiType == AIType.Pet)
			{
				if (this.PetData.Type == 16u)
				{
					Message.AppendString(this.PetData.MoplaBreed.PlantData);
				}
				else
				{
					if (this.PetData.HaveSaddle == Convert.ToBoolean(2))
					{
						Message.AppendString(string.Concat(new object[]
						{
							this.BotData.Look.ToLower(),
							" 3 4 10 0 2 ",
							this.PetData.PetHair,
							" ",
							this.PetData.HairDye,
							" 3 ",
							this.PetData.PetHair,
							" ",
							this.PetData.HairDye
						}));
					}
					else
					{
						if (this.PetData.HaveSaddle == Convert.ToBoolean(1))
						{
							Message.AppendString(string.Concat(new object[]
							{
								this.BotData.Look.ToLower(),
								" 3 2 ",
								this.PetData.PetHair,
								" ",
								this.PetData.HairDye,
								" 3 ",
								this.PetData.PetHair,
								" ",
								this.PetData.HairDye,
								" 4 9 0"
							}));
						}
						else
						{
							Message.AppendString(string.Concat(new object[]
							{
								this.BotData.Look.ToLower(),
								" 2 2 ",
								this.PetData.PetHair,
								" ",
								this.PetData.HairDye,
								" 3 ",
								this.PetData.PetHair,
								" ",
								this.PetData.HairDye
							}));
						}
					}
				}
			}
			else
			{
				Message.AppendString(this.BotData.Look.ToLower());
			}
			Message.AppendInt32(this.VirtualId);
			Message.AppendInt32(this.X);
			Message.AppendInt32(this.Y);
			Message.AppendString(TextHandling.GetString(this.Z));
			Message.AppendInt32(0);
			Message.AppendInt32((this.BotData.AiType == AIType.Generic) ? 4 : 2);
			if (this.BotData.AiType == AIType.Pet)
			{
				Message.AppendUInt(this.PetData.Type);
				Message.AppendUInt(this.PetData.OwnerId);
				Message.AppendString(this.PetData.OwnerName);
				Message.AppendInt32((this.PetData.Type == 16u) ? 0 : 1);
				Message.AppendBoolean(this.PetData.HaveSaddle);
				Message.AppendBoolean(this.RidingHorse);
				Message.AppendInt32(0);
				Message.AppendInt32((this.PetData.Type == 16u) ? 1 : 0);
				Message.AppendString((this.PetData.Type == 16u) ? this.PetData.MoplaBreed.GrowStatus : "");
				return;
			}
			Message.AppendString(this.BotData.Gender.ToLower());
			Message.AppendUInt(this.BotData.OwnerId);
			Message.AppendString(CyberEnvironment.GetGame().GetClientManager().GetNameById(this.BotData.OwnerId));
			Message.AppendInt32(4);
			Message.AppendShort(1);
			Message.AppendShort(2);
			Message.AppendShort(5);
			Message.AppendShort(4);
		}
		internal void SerializeStatus(ServerMessage Message)
		{
			Message.AppendInt32(this.VirtualId);
			Message.AppendInt32(this.X);
			Message.AppendInt32(this.Y);
			Message.AppendString(TextHandling.GetString(this.Z));
			Message.AppendInt32(this.RotHead);
			Message.AppendInt32(this.RotBody);
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("/");
			if (this.IsPet && this.PetData.Type == 16u)
			{
				stringBuilder.Append("/" + this.PetData.MoplaBreed.GrowStatus + ((this.Statusses.Count >= 1) ? "/" : ""));
			}
			lock (this.Statusses)
			{
				foreach (KeyValuePair<string, string> current in this.Statusses)
				{
					stringBuilder.Append(current.Key);
					if (current.Value != string.Empty)
					{
						stringBuilder.Append(" ");
						stringBuilder.Append(current.Value);
					}
					stringBuilder.Append("/");
				}
			}
			stringBuilder.Append("/");
			Message.AppendString(stringBuilder.ToString());
		}
		internal void SerializeStatus(ServerMessage Message, string Status)
		{
			if (this.IsSpectator)
			{
				return;
			}
			Message.AppendInt32(this.VirtualId);
			Message.AppendInt32(this.X);
			Message.AppendInt32(this.Y);
			Message.AppendString(TextHandling.GetString(this.Z));
			Message.AppendInt32(this.RotHead);
			Message.AppendInt32(this.RotBody);
			new StringBuilder();
			Message.AppendString(Status);
		}
		internal GameClient GetClient()
		{
			if (this.IsBot)
			{
				return null;
			}
			if (this.mClient == null)
			{
				this.mClient = CyberEnvironment.GetGame().GetClientManager().GetClientByUserID(this.HabboId);
			}
			return this.mClient;
		}
		private Room GetRoom()
		{
			if (this.mRoom == null)
			{
				this.mRoom = CyberEnvironment.GetGame().GetRoomManager().GetRoom(this.RoomId);
			}
			return this.mRoom;
		}

        internal void SendMessage(byte[] message)
        {
            GetClient().GetConnection().SendData(message);
        }

    }
}
