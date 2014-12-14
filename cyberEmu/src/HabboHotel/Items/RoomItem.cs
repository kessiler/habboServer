using Database_Manager.Database.Session_Details.Interfaces;
using Cyber.Core;
using Cyber.HabboHotel.GameClients;
using Cyber.HabboHotel.Groups;
using Cyber.HabboHotel.Items.Interactor;
using Cyber.HabboHotel.Pathfinding;
using Cyber.HabboHotel.Rooms;
using Cyber.HabboHotel.Rooms.Games;
using Cyber.HabboHotel.Rooms.Wired;
using Cyber.HabboHotel.SoundMachine;
using Cyber.HabboHotel.Users;
using Cyber.Messages;
using Cyber.Messages.Headers;
using Cyber.Util;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;

namespace Cyber.HabboHotel.Items
{
	public class RoomItem : IEquatable<RoomItem>
	{
		internal uint Id;
		internal uint RoomId;
		internal uint BaseItem;
		internal uint UserID;
		internal string ExtraData;
		internal Team team;
		internal byte interactionCountHelper;
		public byte interactionCount;
		internal int value;
		internal FreezePowerUp freezePowerUp;
		private Dictionary<int, ThreeDCoord> mAffectedPoints;

		internal uint GroupId;
		internal uint interactingBallUser;
		
		internal string SongCode;
		private int mX;
		private int mY;
		private double mZ;
		internal int Rot;
		internal string wallCoord;
		private bool updateNeeded;
		internal int UpdateCounter;
		internal uint InteractingUser;
		internal uint InteractingUser2;
		private Item mBaseItem;
		private Room mRoom;
		private bool mIsWallItem;
		private bool mIsFloorItem;
		private bool mIsRoller;
		internal bool IsTrans;
		internal bool pendingReset;
		internal bool MagicRemove;
		internal int LimitedNo;
		internal int LimitedTot;
		internal event OnItemTrigger itemTriggerEventHandler;
		internal event UserWalksFurniDelegate OnUserWalksOffFurni;
		internal event UserWalksFurniDelegate OnUserWalksOnFurni;

        internal bool VikingCotieBurning;
        internal bool OnCannonActing = false;
        
		internal Dictionary<int, ThreeDCoord> GetAffectedTiles
		{
			get
			{
				return this.mAffectedPoints;
			}
		}
		internal int GetX
		{
			get
			{
				return this.mX;
			}
		}
		internal int GetY
		{
			get
			{
				return this.mY;
			}
		}
		internal double GetZ
		{
			get
			{
				return this.mZ;
			}
		}
		internal bool UpdateNeeded
		{
			get
			{
				return this.updateNeeded;
			}
			set
			{
				if (value)
				{
					this.GetRoom().GetRoomItemHandler().QueueRoomItemUpdate(this);
				}
				this.updateNeeded = value;
			}
		}
		internal bool IsRoller
		{
			get
			{
				return this.mIsRoller;
			}
		}
		internal Point Coordinate
		{
			get
			{
				return new Point(this.mX, this.mY);
			}
		}
		internal List<Point> GetCoords
		{
			get
			{
				List<Point> list = new List<Point>();
				list.Add(this.Coordinate);
				foreach (ThreeDCoord current in this.mAffectedPoints.Values)
				{
					list.Add(new Point(current.X, current.Y));
				}
				return list;
			}
		}
		internal double TotalHeight
		{
			get
			{
				if (this.GetBaseItem().StackMultipler && !string.IsNullOrWhiteSpace(this.ExtraData))
				{
					int num = Convert.ToInt32(this.ExtraData);
					return this.mZ + Convert.ToDouble(this.GetBaseItem().ToggleHeight[num]);
				}
				return this.mZ + this.GetBaseItem().Height;
			}
		}
		internal bool IsWallItem
		{
			get
			{
				return this.mIsWallItem;
			}
		}
		internal bool IsFloorItem
		{
			get
			{
				return this.mIsFloorItem;
			}
		}
		internal Point SquareInFront
		{
			get
			{
				Point result = new Point(this.mX, this.mY);
				checked
				{
					if (this.Rot == 0)
					{
						result.Y--;
					}
					else
					{
						if (this.Rot == 2)
						{
							result.X++;
						}
						else
						{
							if (this.Rot == 4)
							{
								result.Y++;
							}
							else
							{
								if (this.Rot == 6)
								{
									result.X--;
								}
							}
						}
					}
					return result;
				}
			}
		}
		internal Point SquareBehind
		{
			get
			{
				Point result = new Point(this.mX, this.mY);
				checked
				{
					if (this.Rot == 0)
					{
						result.Y++;
					}
					else
					{
						if (this.Rot == 2)
						{
							result.X--;
						}
						else
						{
							if (this.Rot == 4)
							{
								result.Y--;
							}
							else
							{
								if (this.Rot == 6)
								{
									result.X++;
								}
							}
						}
					}
					return result;
				}
			}
		}
		internal IFurniInteractor Interactor
		{
			get
			{
				if (this.IsWired)
				{
					return new InteractorWired();
				}

				InteractionType interactionType = this.GetBaseItem().InteractionType;
					
                switch (interactionType)
					{
					case InteractionType.gate:
						return new InteractorGate();
					case InteractionType.scoreboard:
						return new InteractorScoreboard();
					case InteractionType.vendingmachine:
						return new InteractorVendor();
					case InteractionType.alert:
						return new InteractorAlert();
					case InteractionType.onewaygate:
						return new InteractorOneWayGate();
					case InteractionType.loveshuffler:
						return new InteractorLoveShuffler();
					case InteractionType.habbowheel:
						return new InteractorHabboWheel();
					case InteractionType.dice:
						return new InteractorDice();
					case InteractionType.bottle:
						return new InteractorSpinningBottle();
					case InteractionType.hopper:
						return new InteractorHopper();
					case InteractionType.teleport:
						return new InteractorTeleport();
						case InteractionType.football:
							return new InteractorFootball();
						case InteractionType.footballcountergreen:
						case InteractionType.footballcounteryellow:
						case InteractionType.footballcounterblue:
						case InteractionType.footballcounterred:
							return new InteractorScoreCounter();
						case InteractionType.banzaiscoreblue:
						case InteractionType.banzaiscorered:
						case InteractionType.banzaiscoreyellow:
						case InteractionType.banzaiscoregreen:
							return new InteractorBanzaiScoreCounter();
						case InteractionType.banzaicounter:
							return new InteractorBanzaiTimer();
                            case InteractionType.freezetimer:
                                return new InteractorFreezeTimer();

                            case InteractionType.freezeyellowcounter:
                            case InteractionType.freezeredcounter:
                                  case InteractionType.freezebluecounter:
                                  case InteractionType.freezegreencounter:
                                return new InteractorFreezeScoreCounter();
					case InteractionType.freezetileblock:
					case InteractionType.freezetile:
						return new InteractorFreezeTile();
					case InteractionType.jukebox:
						return new InteractorJukebox();
					case InteractionType.puzzlebox:
						return new InteractorPuzzleBox();
                            case InteractionType.mannequin:
							return new InteractorMannequin();
						case InteractionType.fireworks:
							return new InteractorFireworks();
						//case InteractionType.groupforumterminal:
							//return new InteractorGroupForumTerminal();
						case InteractionType.vikingcotie:
							return new InteractorVikingCotie();
                    case InteractionType.cannon:
                            return new InteractorCannon();
                            default:
				return new InteractorGenericSwitch();
			}
            }
        }

        public bool IsWired
        {
            get
            {
                switch (this.GetBaseItem().InteractionType)
                {
                    case InteractionType.triggertimer:
                    case InteractionType.triggerroomenter:
                    case InteractionType.triggergameend:
                    case InteractionType.triggergamestart:
                    case InteractionType.triggerrepeater:
                    case InteractionType.triggerlongrepeater:
                    case InteractionType.triggeronusersay:
                    case InteractionType.triggerscoreachieved:
                    case InteractionType.triggerstatechanged:
                    case InteractionType.triggerwalkonfurni:
                    case InteractionType.triggerwalkofffurni:
                    case InteractionType.actiongivescore:
                    case InteractionType.actionposreset:
                    case InteractionType.actionmoverotate:
                    case InteractionType.actionresettimer:
                    case InteractionType.actionshowmessage:
                    case InteractionType.actionteleportto:
                    case InteractionType.actiontogglestate:
                    case InteractionType.actionkickuser:
                    case InteractionType.actiongivereward:
                    case InteractionType.actionmuteuser:
                    case InteractionType.conditionfurnishaveusers:
                    case InteractionType.conditionstatepos:
                    case InteractionType.conditiontimelessthan:
                    case InteractionType.conditiontimemorethan:
                    case InteractionType.conditiontriggeronfurni:
                    case InteractionType.conditionfurnihasfurni:
                    case InteractionType.conditionitemsmatches:
                    case InteractionType.conditiongroupmember:
                    case InteractionType.conditionfurnitypematches:
                    case InteractionType.conditionhowmanyusersinroom:
                    case InteractionType.conditiontriggerernotonfurni:
                    case InteractionType.conditionfurnihasnotfurni:
                    case InteractionType.conditionfurnishavenotusers:
                    case InteractionType.conditionitemsdontmatch:
                    case InteractionType.conditionfurnitypedontmatch:
                    case InteractionType.conditionnotgroupmember:
                    case InteractionType.conditionnegativehowmanyusers:
                    case InteractionType.conditionuserwearingeffect:
                    case InteractionType.conditionusernotwearingeffect:
                    case InteractionType.conditionuserwearingbadge:
                    case InteractionType.conditionusernotwearingbadge:
                    case InteractionType.conditiondaterangeactive:
                        return true;
                    default:
                        return false;
                }
            }
        }
		internal void SetState(int pX, int pY, double pZ, Dictionary<int, ThreeDCoord> Tiles)
		{
			this.mX = pX;
			this.mY = pY;
			if (!double.IsInfinity(pZ))
			{
				this.mZ = pZ;
			}
			this.mAffectedPoints = Tiles;
		}
		internal void OnTrigger(RoomUser user)
		{
			if (this.itemTriggerEventHandler != null)
			{
				this.itemTriggerEventHandler(null, new ItemTriggeredArgs(user, this));
			}
		}
		internal RoomItem(uint Id, uint RoomId, uint BaseItem, string ExtraData, int X, int Y, double Z, int Rot, Room pRoom, uint Userid, uint Group, int flatId, string SongCode)
		{
			this.Id = Id;
			this.RoomId = RoomId;
			this.BaseItem = BaseItem;
			this.ExtraData = ExtraData;
			this.GroupId = Group;
			this.mX = X;
			this.mY = Y;
			if (!double.IsInfinity(Z))
			{
				this.mZ = Z;
			}
			this.Rot = Rot;
			this.UpdateNeeded = false;
			this.UpdateCounter = 0;
			this.InteractingUser = 0u;
			this.InteractingUser2 = 0u;
			this.IsTrans = false;
			this.interactingBallUser = 0u;
			this.interactionCount = 0;
			this.value = 0;
			this.UserID = Userid;
			this.SongCode = SongCode;
			using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
			{
				queryreactor.setQuery("SELECT * FROM items_limited WHERE item_id=" + Id + " LIMIT 1");
				DataRow row = queryreactor.getRow();
				if (row != null)
				{
					this.LimitedNo = int.Parse(row[1].ToString());
					this.LimitedTot = int.Parse(row[2].ToString());
				}
				else
				{
					this.LimitedNo = 0;
					this.LimitedTot = 0;
				}
			}
			this.mBaseItem = CyberEnvironment.GetGame().GetItemManager().GetItem(BaseItem);
			this.mRoom = pRoom;
			if (this.GetBaseItem() == null)
			{
				Logging.LogException("Unknown baseID: " + BaseItem);
			}
			InteractionType interactionType = this.GetBaseItem().InteractionType;
			if (interactionType <= InteractionType.roller)
			{
				switch (interactionType)
				{
				case InteractionType.hopper:
					this.IsTrans = true;
					this.ReqUpdate(0, true);
					break;
				case InteractionType.teleport:
					this.IsTrans = true;
					this.ReqUpdate(0, true);
					break;
				default:
					if (interactionType == InteractionType.roller)
					{
						this.mIsRoller = true;
						pRoom.GetRoomItemHandler().GotRollers = true;
					}
					break;
				}
			}
			else
			{
				switch (interactionType)
				{
				case InteractionType.footballcountergreen:
				case InteractionType.banzaigategreen:
				case InteractionType.banzaiscoregreen:
				case InteractionType.freezegreencounter:
				case InteractionType.freezegreengate:
					this.team = Team.green;
					break;
				case InteractionType.footballcounteryellow:
				case InteractionType.banzaigateyellow:
				case InteractionType.banzaiscoreyellow:
				case InteractionType.freezeyellowcounter:
				case InteractionType.freezeyellowgate:
					this.team = Team.yellow;
					break;
				case InteractionType.footballcounterblue:
				case InteractionType.banzaigateblue:
				case InteractionType.banzaiscoreblue:
				case InteractionType.freezebluecounter:
				case InteractionType.freezebluegate:
					this.team = Team.blue;
					break;
				case InteractionType.footballcounterred:
				case InteractionType.banzaigatered:
				case InteractionType.banzaiscorered:
				case InteractionType.freezeredcounter:
				case InteractionType.freezeredgate:
					this.team = Team.red;
					break;
				case InteractionType.banzaifloor:
				case InteractionType.banzaicounter:
				case InteractionType.banzaipuck:
				case InteractionType.banzaipyramid:
				case InteractionType.freezetimer:
				case InteractionType.freezeexit:
					break;
				case InteractionType.banzaitele:
					this.ExtraData = "";
					break;
				default:
					if (interactionType == InteractionType.vikingcotie)
					{
						int num;
						if (int.TryParse(ExtraData, out num) && num >= 1 && num < 5)
						{
							this.VikingCotieBurning = true;
						}
					}
					break;
				}
			}
			this.mIsWallItem = (this.GetBaseItem().Type.ToString().ToLower() == "i");
			this.mIsFloorItem = (this.GetBaseItem().Type.ToString().ToLower() == "s");
			this.mAffectedPoints = Gamemap.GetAffectedTiles(this.GetBaseItem().InteractionType, this.GetBaseItem().Length, this.GetBaseItem().Width, this.mX, this.mY, Rot);
		}
		internal RoomItem(uint Id, uint RoomId, uint BaseItem, string ExtraData, string wallCoord, Room pRoom, uint userid, uint Group, int flatId)
		{
			this.Id = Id;
			this.RoomId = RoomId;
			this.BaseItem = BaseItem;
			this.ExtraData = ExtraData;
			this.GroupId = Group;
			this.mX = 0;
			this.mY = 0;
			this.mZ = 0.0;
			this.UpdateNeeded = false;
			this.UpdateCounter = 0;
			this.InteractingUser = 0u;
			this.InteractingUser2 = 0u;
			this.IsTrans = false;
			this.interactingBallUser = 0u;
			this.interactionCount = 0;
			this.value = 0;
			this.wallCoord = wallCoord;
			this.UserID = userid;
			this.mBaseItem = CyberEnvironment.GetGame().GetItemManager().GetItem(BaseItem);
			this.mRoom = pRoom;
			if (this.GetBaseItem() == null)
			{
				Logging.LogException("Unknown baseID: " + BaseItem);
			}
			this.mIsWallItem = true;
			this.mIsFloorItem = false;
			this.mAffectedPoints = new Dictionary<int, ThreeDCoord>();
			this.SongCode = "";
		}
		internal void Destroy()
		{
			this.mRoom = null;
			this.mAffectedPoints.Clear();
			this.itemTriggerEventHandler = null;
			this.OnUserWalksOffFurni = null;
			this.OnUserWalksOnFurni = null;
		}
		public bool Equals(RoomItem comparedItem)
		{
			return comparedItem.Id == this.Id;
		}
		internal void ProcessUpdates()
		{
			checked
			{
				this.UpdateCounter--;
				if (this.UpdateCounter <= 0 || this.IsTrans)
				{
					this.UpdateNeeded = false;
					this.UpdateCounter = 0;
					Random random = new Random();
					InteractionType interactionType = this.GetBaseItem().InteractionType;
                    if (interactionType <= InteractionType.pressurepad)
                    {
                        switch (interactionType)
                        {
                            case InteractionType.scoreboard:
                                {
                                    if (string.IsNullOrEmpty(this.ExtraData))
                                    {
                                        return;
                                    }
                                    int num = 0;
                                    try
                                    {
                                        num = int.Parse(this.ExtraData);
                                    }
                                    catch
                                    {
                                    }
                                    if (num > 0)
                                    {
                                        if (this.interactionCountHelper == 1)
                                        {
                                            num--;
                                            this.interactionCountHelper = 0;
                                            this.ExtraData = num.ToString();
                                            this.UpdateState();
                                        }
                                        else
                                        {
                                            this.interactionCountHelper += 1;
                                        }
                                        this.UpdateCounter = 1;
                                        return;
                                    }
                                    this.UpdateCounter = 0;
                                    return;
                                }
                            case InteractionType.vendingmachine:
                                if (this.ExtraData == "1")
                                {
                                    RoomUser roomUser = this.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(this.InteractingUser);
                                    if (roomUser != null)
                                    {
                                        roomUser.UnlockWalking();
                                        if (this.GetBaseItem().VendingIds.Count > 0)
                                        {
                                            int item = this.GetBaseItem().VendingIds[RandomNumber.GenerateNewRandom(0, this.GetBaseItem().VendingIds.Count - 1)];
                                            roomUser.CarryItem(item);
                                        }
                                    }
                                    this.InteractingUser = 0u;
                                    this.ExtraData = "0";
                                    this.UpdateState(false, true);
                                    return;
                                }
                                break;
                            case InteractionType.alert:
                                if (this.ExtraData == "1")
                                {
                                    this.ExtraData = "0";
                                    this.UpdateState(false, true);
                                    return;
                                }
                                break;
                            case InteractionType.onewaygate:
                                {
                                    RoomUser roomUser = null;
                                    if (this.InteractingUser > 0u)
                                    {
                                        roomUser = this.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(this.InteractingUser);
                                    }
                                    if (roomUser != null && roomUser.X == this.mX && roomUser.Y == this.mY)
                                    {
                                        this.ExtraData = "1";
                                        roomUser.MoveTo(this.SquareBehind);
                                        roomUser.InteractingGate = false;
                                        roomUser.GateId = 0u;
                                        this.ReqUpdate(1, false);
                                        this.UpdateState(false, true);
                                    }
                                    else
                                    {
                                        if (roomUser != null && roomUser.Coordinate == this.SquareBehind)
                                        {
                                            roomUser.UnlockWalking();
                                            this.ExtraData = "0";
                                            this.InteractingUser = 0u;
                                            roomUser.InteractingGate = false;
                                            roomUser.GateId = 0u;
                                            this.UpdateState(false, true);
                                        }
                                        else
                                        {
                                            if (this.ExtraData == "1")
                                            {
                                                this.ExtraData = "0";
                                                this.UpdateState(false, true);
                                            }
                                        }
                                    }
                                    if (roomUser == null)
                                    {
                                        this.InteractingUser = 0u;
                                        return;
                                    }
                                    break;
                                }
                            case InteractionType.loveshuffler:
                                if (this.ExtraData == "0")
                                {
                                    this.ExtraData = RandomNumber.GenerateNewRandom(1, 4).ToString();
                                    this.ReqUpdate(20, false);
                                }
                                else
                                {
                                    if (this.ExtraData != "-1")
                                    {
                                        this.ExtraData = "-1";
                                    }
                                }
                                this.UpdateState(false, true);
                                return;
                            case InteractionType.habbowheel:
                                this.ExtraData = RandomNumber.GenerateNewRandom(1, 10).ToString();
                                this.UpdateState();
                                return;
                            case InteractionType.dice:
                                this.ExtraData = random.Next(1, 7).ToString();
                                this.UpdateState();
                                return;
                            case InteractionType.bottle:
                                this.ExtraData = RandomNumber.GenerateNewRandom(0, 7).ToString();
                                this.UpdateState();
                                return;
                            case InteractionType.hopper:
                                {
                                    bool flag = false;
                                    bool flag2 = false;
                                    int num2 = 0;
                                    if (this.InteractingUser > 0u)
                                    {
                                        RoomUser roomUser = this.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(this.InteractingUser);
                                        if (roomUser != null)
                                        {
                                            if (roomUser.Coordinate == this.Coordinate)
                                            {
                                                roomUser.AllowOverride = false;
                                                if (roomUser.TeleDelay == 0)
                                                {
                                                    uint aHopper = HopperHandler.GetAHopper(roomUser.RoomId);
                                                    uint hopperId = HopperHandler.GetHopperId(aHopper);
                                                    if (!roomUser.IsBot && roomUser != null && roomUser.GetClient() != null && roomUser.GetClient().GetHabbo() != null && roomUser.GetClient().GetMessageHandler() != null)
                                                    {
                                                        roomUser.GetClient().GetHabbo().IsHopping = true;
                                                        roomUser.GetClient().GetHabbo().HopperId = hopperId;
                                                        ServerMessage roomFwd = new ServerMessage(Outgoing.RoomForwardMessageComposer);
                                                        roomFwd.AppendUInt(aHopper);
                                                        roomUser.GetClient().SendMessage(roomFwd);
                                                        this.InteractingUser = 0u;
                                                    }
                                                }
                                                else
                                                {
                                                    roomUser.TeleDelay--;
                                                    flag = true;
                                                }
                                            }
                                            else
                                            {
                                                if (roomUser.Coordinate == this.SquareInFront)
                                                {
                                                    roomUser.AllowOverride = true;
                                                    flag2 = true;
                                                    if (roomUser.IsWalking && (roomUser.GoalX != this.mX || roomUser.GoalY != this.mY))
                                                    {
                                                        roomUser.ClearMovement(true);
                                                    }
                                                    roomUser.CanWalk = false;
                                                    roomUser.AllowOverride = true;
                                                    roomUser.MoveTo(this.Coordinate.X, this.Coordinate.Y, true);
                                                }
                                                else
                                                {
                                                    this.InteractingUser = 0u;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            this.InteractingUser = 0u;
                                        }
                                    }
                                    if (this.InteractingUser2 > 0u)
                                    {
                                        RoomUser roomUserByHabbo = this.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(this.InteractingUser2);
                                        if (roomUserByHabbo != null)
                                        {
                                            flag2 = true;
                                            roomUserByHabbo.UnlockWalking();
                                            roomUserByHabbo.MoveTo(this.SquareInFront);
                                        }
                                        this.InteractingUser2 = 0u;
                                    }
                                    if (flag2)
                                    {
                                        if (this.ExtraData != "1")
                                        {
                                            this.ExtraData = "1";
                                            this.UpdateState(false, true);
                                        }
                                    }
                                    else
                                    {
                                        if (flag)
                                        {
                                            if (this.ExtraData != "2")
                                            {
                                                this.ExtraData = "2";
                                                this.UpdateState(false, true);
                                            }
                                        }
                                        else
                                        {
                                            if (this.ExtraData != "0")
                                            {
                                                if (num2 == 0)
                                                {
                                                    this.ExtraData = "0";
                                                    this.UpdateState(false, true);
                                                }
                                                else
                                                {
                                                    num2--;
                                                }
                                            }
                                        }
                                    }
                                    this.ReqUpdate(1, false);
                                    return;
                                }
                            case InteractionType.teleport:
                                {
                                    bool flag3 = false;
                                    bool flag4 = false;
                                    if (this.InteractingUser > 0)
                                    {
                                        RoomUser roomUserByHabbo2 = this.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(this.InteractingUser);
                                        if (roomUserByHabbo2 == null)
                                        {
                                            this.InteractingUser = 0u;
                                            return;
                                        }
                                        if (roomUserByHabbo2.Coordinate == this.Coordinate)
                                        {
                                            roomUserByHabbo2.AllowOverride = false;
                                            if (TeleHandler.IsTeleLinked(this.Id, this.mRoom))
                                            {
                                                flag4 = true;
                                                uint linkedTele = TeleHandler.GetLinkedTele(this.Id, this.mRoom);
                                                uint teleRoomId = TeleHandler.GetTeleRoomId(linkedTele, this.mRoom);
                                                if (teleRoomId == this.RoomId)
                                                {
                                                    RoomItem item2 = this.GetRoom().GetRoomItemHandler().GetItem(linkedTele);
                                                    if (item2 == null)
                                                    {
                                                        roomUserByHabbo2.UnlockWalking();
                                                    }
                                                    else
                                                    {
                                                        roomUserByHabbo2.SetPos(item2.GetX, item2.GetY, item2.GetZ);
                                                        roomUserByHabbo2.SetRot(item2.Rot, false);
                                                        item2.ExtraData = "2";
                                                        item2.UpdateState(false, true);
                                                        item2.InteractingUser2 = this.InteractingUser;
                                                    }
                                                }
                                                else
                                                {
                                                    if (!roomUserByHabbo2.IsBot && roomUserByHabbo2 != null && roomUserByHabbo2.GetClient() != null && roomUserByHabbo2.GetClient().GetHabbo() != null && roomUserByHabbo2.GetClient().GetMessageHandler() != null)
                                                    {
                                                        roomUserByHabbo2.GetClient().GetHabbo().IsTeleporting = true;
                                                        roomUserByHabbo2.GetClient().GetHabbo().TeleportingRoomID = teleRoomId;
                                                        roomUserByHabbo2.GetClient().GetHabbo().TeleporterId = linkedTele;
                                                        roomUserByHabbo2.GetClient().GetMessageHandler().PrepareRoomForUser(teleRoomId, "");
                                                    }
                                                }
                                                this.InteractingUser = 0u;
                                            }
                                            else
                                            {
                                                roomUserByHabbo2.UnlockWalking();
                                                this.InteractingUser = 0u;
                                                roomUserByHabbo2.CanWalk = true;
                                                roomUserByHabbo2.TeleportEnabled = false;
                                                roomUserByHabbo2.MoveTo(this.SquareInFront);
                                            }
                                        }
                                        else
                                        {
                                            if (roomUserByHabbo2.Coordinate == this.SquareInFront)
                                            {
                                                roomUserByHabbo2.AllowOverride = true;
                                                flag3 = true;
                                                if (roomUserByHabbo2.IsWalking && (roomUserByHabbo2.GoalX != this.mX || roomUserByHabbo2.GoalY != this.mY))
                                                {
                                                    roomUserByHabbo2.ClearMovement(true);
                                                }
                                                roomUserByHabbo2.SetRot(PathFinding.PathFinder.CalculateRotation(roomUserByHabbo2.X, roomUserByHabbo2.Y, this.mX, this.mY));
                                                roomUserByHabbo2.CanWalk = false;
                                                roomUserByHabbo2.AllowOverride = true;
                                                roomUserByHabbo2.UnlockWalking();
                                                roomUserByHabbo2.TeleportEnabled = true;
                                                roomUserByHabbo2.MoveTo(this.GetX, this.GetY, true);
                                            }
                                            else
                                            {
                                                this.InteractingUser = 0u;
                                            }
                                        }
                                    }
                                    if (this.InteractingUser2 > 0u)
                                    {
                                        RoomUser roomUserByHabbo3 = this.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(this.InteractingUser2);
                                        if (roomUserByHabbo3 != null)
                                        {
                                            flag3 = true;
                                            roomUserByHabbo3.UnlockWalking();
                                            roomUserByHabbo3.TeleportEnabled = false;
                                            roomUserByHabbo3.MoveTo(this.SquareInFront);
                                        }
                                        this.InteractingUser2 = 0u;
                                    }
                                    if (flag3)
                                    {
                                        if (this.ExtraData != "1")
                                        {
                                            this.ExtraData = "1";
                                            this.UpdateState(false, true);
                                        }
                                    }
                                    else
                                    {
                                        if (flag4)
                                        {
                                            if (this.ExtraData != "2")
                                            {
                                                this.ExtraData = "2";
                                                this.UpdateState(false, true);
                                            }
                                        }
                                        else
                                        {
                                            if (this.ExtraData != "0")
                                            {
                                                this.ExtraData = "0";
                                                this.UpdateState(false, true);
                                            }
                                        }
                                    }
                                    this.ReqUpdate(1, false);
                                    return;
                                }
                            default:
                                switch (interactionType)
                                {
                                    case InteractionType.banzaifloor:
                                        if (this.value == 3)
                                        {
                                            if (this.interactionCountHelper == 1)
                                            {
                                                this.interactionCountHelper = 0;
                                                switch (this.team)
                                                {
                                                    case Team.red:
                                                        this.ExtraData = "5";
                                                        break;
                                                    case Team.green:
                                                        this.ExtraData = "8";
                                                        break;
                                                    case Team.blue:
                                                        this.ExtraData = "11";
                                                        break;
                                                    case Team.yellow:
                                                        this.ExtraData = "14";
                                                        break;
                                                }
                                            }
                                            else
                                            {
                                                this.ExtraData = "";
                                                this.interactionCountHelper += 1;
                                            }
                                            this.UpdateState();
                                            this.interactionCount += 1;
                                            if (this.interactionCount < 16)
                                            {
                                                this.UpdateCounter = 1;
                                                return;
                                            }
                                            this.UpdateCounter = 0;
                                            return;
                                        }
                                        break;
                                    case InteractionType.banzaiscoreblue:
                                    case InteractionType.banzaiscorered:
                                    case InteractionType.banzaiscoreyellow:
                                    case InteractionType.banzaiscoregreen:
                                    case InteractionType.banzaipyramid:
                                    case InteractionType.freezeexit:
                                    case InteractionType.freezeredcounter:
                                    case InteractionType.freezebluecounter:
                                    case InteractionType.freezeyellowcounter:
                                    case InteractionType.freezegreencounter:
                                    case InteractionType.freezeyellowgate:
                                    case InteractionType.freezeredgate:
                                    case InteractionType.freezegreengate:
                                    case InteractionType.freezebluegate:
                                    case InteractionType.freezetileblock:
                                    case InteractionType.jukebox:
                                    case InteractionType.musicdisc:
                                    case InteractionType.puzzlebox:
                                    case InteractionType.roombg:
                                    case InteractionType.actionkickuser:
                                    case InteractionType.actiongivereward:
                                    case InteractionType.arrowplate:
                                        break;
                                    case InteractionType.banzaicounter:
                                        {
                                            if (string.IsNullOrEmpty(this.ExtraData))
                                            {
                                                return;
                                            }
                                            int num4 = 0;
                                            try
                                            {
                                                num4 = int.Parse(this.ExtraData);
                                            }
                                            catch
                                            {
                                            }
                                            if (num4 > 0)
                                            {
                                                if (this.interactionCountHelper == 1)
                                                {
                                                    num4--;
                                                    this.interactionCountHelper = 0;
                                                    if (!this.GetRoom().GetBanzai().isBanzaiActive)
                                                    {
                                                        break;
                                                    }
                                                    this.ExtraData = num4.ToString();
                                                    this.UpdateState();
                                                }
                                                else
                                                {
                                                    this.interactionCountHelper += 1;
                                                }
                                                this.UpdateCounter = 1;
                                                return;
                                            }
                                            this.UpdateCounter = 0;
                                            this.GetRoom().GetBanzai().BanzaiEnd();
                                            return;
                                        }
                                    case InteractionType.banzaitele:
                                        this.ExtraData = string.Empty;
                                        this.UpdateState();
                                        return;
                                    case InteractionType.banzaipuck:
                                        if (this.interactionCount > 4)
                                        {
                                            this.interactionCount += 1;
                                            this.UpdateCounter = 1;
                                            return;
                                        }
                                        this.interactionCount = 0;
                                        this.UpdateCounter = 0;
                                        return;
                                    case InteractionType.freezetimer:
                                        {
                                            if (string.IsNullOrEmpty(this.ExtraData))
                                            {
                                                return;
                                            }
                                            int num5 = 0;
                                            try
                                            {
                                                num5 = int.Parse(this.ExtraData);
                                            }
                                            catch
                                            {
                                            }
                                            if (num5 > 0)
                                            {
                                                if (this.interactionCountHelper == 1)
                                                {
                                                    num5--;
                                                    this.interactionCountHelper = 0;
                                                    if (!this.GetRoom().GetFreeze().GameIsStarted)
                                                    {
                                                        break;
                                                    }
                                                    this.ExtraData = num5.ToString();
                                                    this.UpdateState();
                                                }
                                                else
                                                {
                                                    this.interactionCountHelper += 1;
                                                }
                                                this.UpdateCounter = 1;
                                                return;
                                            }
                                            this.UpdateNeeded = false;
                                            this.GetRoom().GetFreeze().StopGame();
                                            return;
                                        }
                                    case InteractionType.freezetile:
                                        if (this.InteractingUser > 0u)
                                        {
                                            this.ExtraData = "11000";
                                            this.UpdateState(false, true);
                                            this.GetRoom().GetFreeze().onFreezeTiles(this, this.freezePowerUp, this.InteractingUser);
                                            this.InteractingUser = 0u;
                                            this.interactionCountHelper = 0;
                                            return;
                                        }
                                        break;
                                    case InteractionType.wearitem:
                                        {
                                            this.ExtraData = "1";
                                            this.UpdateState();
                                            string text = "";
                                            GameClient clientByUserID = CyberEnvironment.GetGame().GetClientManager().GetClientByUserID(this.InteractingUser);
                                            unchecked
                                            {
                                                if (!clientByUserID.GetHabbo().Look.Contains("ha"))
                                                {
                                                    text = clientByUserID.GetHabbo().Look + ".ha-1006-1326";
                                                }
                                                else
                                                {
                                                    string[] array = clientByUserID.GetHabbo().Look.Split(new char[]
										{
											'.'
										});
                                                    string[] array2 = array;
                                                    for (int i = 0; i < array2.Length; i++)
                                                    {
                                                        string text2 = array2[i];
                                                        string str = text2;
                                                        if (text2.Contains("ha"))
                                                        {
                                                            str = "ha-1006-1326";
                                                        }
                                                        text = text + str + ".";
                                                    }
                                                }
                                                if (text.EndsWith("."))
                                                {
                                                    text.TrimEnd(new char[]
										{
											'.'
										});
                                                }
                                                clientByUserID.GetHabbo().Look = text;
                                                clientByUserID.GetMessageHandler().GetResponse().Init(Outgoing.UpdateUserDataMessageComposer);
                                                clientByUserID.GetMessageHandler().GetResponse().AppendInt32(-1);
                                                clientByUserID.GetMessageHandler().GetResponse().AppendString(clientByUserID.GetHabbo().Look);
                                                clientByUserID.GetMessageHandler().GetResponse().AppendString(clientByUserID.GetHabbo().Gender.ToLower());
                                                clientByUserID.GetMessageHandler().GetResponse().AppendString(clientByUserID.GetHabbo().Motto);
                                                clientByUserID.GetMessageHandler().GetResponse().AppendInt32(clientByUserID.GetHabbo().AchievementPoints);
                                                clientByUserID.GetMessageHandler().SendResponse();
                                                ServerMessage serverMessage = new ServerMessage();
                                                serverMessage.Init(Outgoing.UpdateUserDataMessageComposer);
                                                serverMessage.AppendUInt(this.InteractingUser2);
                                                serverMessage.AppendString(clientByUserID.GetHabbo().Look);
                                                serverMessage.AppendString(clientByUserID.GetHabbo().Gender.ToLower());
                                                serverMessage.AppendString(clientByUserID.GetHabbo().Motto);
                                                serverMessage.AppendInt32(clientByUserID.GetHabbo().AchievementPoints);
                                                this.GetRoom().SendMessage(serverMessage);
                                                return;
                                            }
                                        }
                                    case InteractionType.triggertimer:
                                    case InteractionType.triggerroomenter:
                                    case InteractionType.triggergameend:
                                    case InteractionType.triggergamestart:
                                    case InteractionType.triggerrepeater:
                                    case InteractionType.triggerlongrepeater:
                                    case InteractionType.triggeronusersay:
                                    case InteractionType.triggerscoreachieved:
                                    case InteractionType.triggerstatechanged:
                                    case InteractionType.triggerwalkonfurni:
                                    case InteractionType.triggerwalkofffurni:
                                    case InteractionType.actiongivescore:
                                    case InteractionType.actionposreset:
                                    case InteractionType.actionmoverotate:
                                    case InteractionType.actionresettimer:
                                    case InteractionType.actionshowmessage:
                                    case InteractionType.actionteleportto:
                                    case InteractionType.actiontogglestate:
                                    case InteractionType.conditionfurnishaveusers:
                                    case InteractionType.conditionstatepos:
                                    case InteractionType.conditiontimelessthan:
                                    case InteractionType.conditiontimemorethan:
                                    case InteractionType.conditiontriggeronfurni:
                                    case InteractionType.conditionfurnihasfurni:
                                    case InteractionType.conditionitemsmatches:
                                    case InteractionType.conditiongroupmember:
                                    case InteractionType.conditionfurnitypematches:
                                    case InteractionType.conditionhowmanyusersinroom:
                                    case InteractionType.conditiontriggerernotonfurni:
                                    case InteractionType.conditionfurnihasnotfurni:
                                    case InteractionType.conditionfurnishavenotusers:
                                    case InteractionType.conditionitemsdontmatch:
                                    case InteractionType.conditionfurnitypedontmatch:
                                    case InteractionType.conditionnotgroupmember:
                                    case InteractionType.conditionuserwearingeffect:
                                    case InteractionType.conditionuserwearingbadge:
                                    case InteractionType.conditionusernotwearingeffect:
                                    case InteractionType.conditionusernotwearingbadge:
                                    case InteractionType.conditiondaterangeactive:
                                        this.ExtraData = "0";
                                        this.UpdateState(false, true);
                                        break;
                                    case InteractionType.pressurepad:
                                        this.ExtraData = "1";
                                        this.UpdateState();
                                        return;
                                    default:
                                        return;
                                }
                                break;
                        }
                    }
                    else
                    {
                        if (interactionType != InteractionType.gift)
                        {
                            if (interactionType != InteractionType.vip_gate)
                            {
                                return;
                            }
                            RoomUser roomUser = null;
                            if (this.InteractingUser > 0u)
                            {
                                roomUser = this.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(this.InteractingUser);
                            }
                            int num6 = 0;
                            int num7 = 0;
                            if (roomUser != null && roomUser.X == this.mX && roomUser.Y == this.mY)
                            {
                                if (roomUser.RotBody == 4)
                                {
                                    num6 = 1;
                                }
                                else
                                {
                                    if (roomUser.RotBody == 0)
                                    {
                                        num6 = -1;
                                    }
                                    else
                                    {
                                        if (roomUser.RotBody == 6)
                                        {
                                            num7 = -1;
                                        }
                                        else
                                        {
                                            if (roomUser.RotBody == 2)
                                            {
                                                num7 = 1;
                                            }
                                        }
                                    }
                                }
                                roomUser.MoveTo(roomUser.X + num7, roomUser.Y + num6);
                                this.ReqUpdate(1, false);
                            }
                            else
                            {
                                if (roomUser != null && (roomUser.Coordinate == this.SquareBehind || roomUser.Coordinate == this.SquareInFront))
                                {
                                    roomUser.UnlockWalking();
                                    this.ExtraData = "0";
                                    this.InteractingUser = 0u;
                                    this.UpdateState(false, true);
                                }
                                else
                                {
                                    if (this.ExtraData == "1")
                                    {
                                        this.ExtraData = "0";
                                        this.UpdateState(false, true);
                                    }
                                }
                            }
                            if (roomUser == null)
                            {
                                this.InteractingUser = 0u;
                                return;
                            }
                        }
                    }
				}
			}
		}
		internal void ReqUpdate(int Cycles, bool setUpdate)
		{
			this.UpdateCounter = Cycles;
			if (setUpdate)
			{
				this.UpdateNeeded = true;
			}
		}
		internal void UpdateState()
		{
			this.UpdateState(true, true);
		}
		internal void UpdateState(bool inDb, bool inRoom)
		{
			if (this.GetRoom() == null)
			{
				return;
			}
			string s = this.ExtraData;
			if (this.GetBaseItem().InteractionType == InteractionType.mystery_box)
			{
				using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
				{
					queryreactor.setQuery("SELECT extra_data FROM items WHERE id=" + this.Id + " LIMIT 1");
					this.ExtraData = queryreactor.getString();
				}
				if (this.ExtraData.Contains(Convert.ToChar(5).ToString()))
				{
					int num = int.Parse(this.ExtraData.Split(new char[]
					{
						Convert.ToChar(5)
					})[0]);
					int num2 = int.Parse(this.ExtraData.Split(new char[]
					{
						Convert.ToChar(5)
					})[1]);
					s = checked(3 * num - num2).ToString();
				}
			}
			if (inDb)
			{
				this.GetRoom().GetRoomItemHandler().UpdateItem(this);
			}
			if (inRoom)
			{
				ServerMessage serverMessage = new ServerMessage(0);
				if (this.IsFloorItem)
				{
					serverMessage.Init(Outgoing.UpdateFloorItemExtraDataMessageComposer);
					serverMessage.AppendString(this.Id.ToString());
					if (this.GetBaseItem().InteractionType == InteractionType.mannequin)
					{
						serverMessage.AppendInt32(1);
						serverMessage.AppendInt32(3);
						if (this.ExtraData.Contains(Convert.ToChar(5).ToString()))
						{
							string[] array = this.ExtraData.Split(new char[]
							{
								Convert.ToChar(5)
							});
							serverMessage.AppendString("GENDER");
							serverMessage.AppendString(array[0]);
							serverMessage.AppendString("FIGURE");
							serverMessage.AppendString(array[1]);
							serverMessage.AppendString("OUTFIT_NAME");
							serverMessage.AppendString(array[2]);
						}
						else
						{
							serverMessage.AppendString("GENDER");
							serverMessage.AppendString("");
							serverMessage.AppendString("FIGURE");
							serverMessage.AppendString("");
							serverMessage.AppendString("OUTFIT_NAME");
							serverMessage.AppendString("");
						}
					}
					else
					{
						if (this.GetBaseItem().InteractionType == InteractionType.pinata)
						{
							serverMessage.AppendInt32(7);
							if (this.ExtraData.Length <= 0)
							{
								serverMessage.AppendString("6");
								serverMessage.AppendInt32(0);
								serverMessage.AppendInt32(100);
							}
							else
							{
								serverMessage.AppendString((int.Parse(this.ExtraData) == 100) ? "8" : "6");
								serverMessage.AppendInt32(int.Parse(this.ExtraData));
								serverMessage.AppendInt32(100);
							}
						}
						else
						{
							serverMessage.AppendInt32(0);
							serverMessage.AppendString(s);
						}
					}
				}
				else
				{
					serverMessage.Init(Outgoing.UpdateRoomWallItemMessageComposer);
					this.Serialize(serverMessage);
				}
				this.GetRoom().SendMessage(serverMessage);
			}
		}

        internal void Serialize(ServerMessage Message)
        {
            checked
            {
                if (this.IsFloorItem)
                {
                    Message.AppendUInt(this.Id);
                    Message.AppendInt32(this.GetBaseItem().SpriteId);
                    Message.AppendInt32(this.mX);
                    Message.AppendInt32(this.mY);
                    Message.AppendInt32(this.Rot);
                    Message.AppendString(string.Format("{0:0.00}", TextHandling.GetString(this.mZ)));
                    Message.AppendString(string.Format("{0:0.00}", TextHandling.GetString(this.GetBaseItem().Height)));
                   
                        if (this.GetBaseItem().InteractionType == InteractionType.groupforumterminal || this.GetBaseItem().InteractionType == InteractionType.gld_item || this.GetBaseItem().InteractionType == InteractionType.gld_gate)
                        {
                            Guild group2 = CyberEnvironment.GetGame().GetGroupManager().GetGroup(GroupId);
                            if (group2 == null)
                            {
                                Message.AppendInt32(1);
                                Message.AppendInt32(0);
                                Message.AppendString(this.ExtraData);
                            }
                            else
                            {
                                Message.AppendInt32(0);
                                Message.AppendInt32(2);
                                Message.AppendInt32(5);
                                Message.AppendString(this.ExtraData);
                                Message.AppendString(this.GroupId.ToString());
                                Message.AppendString(group2.Badge);
                                Message.AppendString(CyberEnvironment.GetGame().GetGroupManager().GetGroupColour(group2.Colour1, true));
                                Message.AppendString(CyberEnvironment.GetGame().GetGroupManager().GetGroupColour(group2.Colour2, false));
                            }
                        }
                        else
                        {
                            if (this.GetBaseItem().InteractionType == InteractionType.youtubetv)
                            {
                                Message.AppendInt32(0);
                                if (!CyberEnvironment.GetGame().GetVideoManager().TVExists(this.Id))
                                {
                                    Message.AppendInt32(0);
                                    Message.AppendString("");
                                }
                                else
                                {
                                    Message.AppendInt32(1);
                                    Message.AppendInt32(1);
                                    Message.AppendString("THUMBNAIL_URL");
                                    Message.AppendString(ExtraSettings.YOUTUBE_GENERATOR_SUBURL + "=" + this.ExtraData);
                                }
                            }
                            else
                            {
                                if (this.GetBaseItem().InteractionType == InteractionType.musicdisc)
                                {
                                    Message.AppendUInt(SongManager.GetSongId(this.SongCode));
                                    Message.AppendInt32(0);
                                    Message.AppendString(this.ExtraData);
                                }
                                else
                                {
                                    if (this.GetBaseItem().InteractionType == InteractionType.background)
                                    {
                                        Message.AppendInt32(0);
                                        Message.AppendInt32(1);
                                        if (this.ExtraData != "")
                                        {
                                            Message.AppendInt32(this.ExtraData.Split(new char[]
											{
												Convert.ToChar(9)
											}).Length / 2);
                                            for (int i = 0; i <= this.ExtraData.Split(new char[]
											{
												Convert.ToChar(9)
											}).Length - 1; i++)
                                            {
                                                Message.AppendString(this.ExtraData.Split(new char[]
												{
													Convert.ToChar(9)
												})[i]);
                                            }
                                        }
                                        else
                                        {
                                            Message.AppendInt32(0);
                                        }
                                    }
                                    else
                                    {
                                        if (this.GetBaseItem().InteractionType == InteractionType.gift)
                                        {
                                            string[] Split = this.ExtraData.Split((char)9);

                                            uint GiverId = 0;
                                            string GiftMessage = "";
                                            int GiftRibbon = 1;
                                            int GiftColor = 2;
                                            bool ShowGiver = false;
                                            string GiverName = "";
                                            string GiverLook = "";
                                            string Product = "A1 PIZ";

                                            try
                                            {
                                                GiverId = uint.Parse(Split[0]);
                                                GiftMessage = Split[1];
                                                GiftRibbon = int.Parse(Split[2]);
                                                GiftColor = int.Parse(Split[3]);
                                                ShowGiver = CyberEnvironment.EnumToBool(Split[4]);
                                                GiverName = Split[5];
                                                GiverLook = Split[6];
                                                Product = Split[7];
                                            }
                                            catch
                                            {

                                            }

                                            int RibbonAndColor = (GiftRibbon * 1000) + GiftColor;

                                            Message.AppendInt32(RibbonAndColor);
                                            Message.AppendInt32(1);
                                            Message.AppendInt32((ShowGiver) ? 6 : 4);
                                            Message.AppendString("EXTRA_PARAM");
                                            Message.AppendString("");
                                            Message.AppendString("MESSAGE");
                                            Message.AppendString(GiftMessage);
                                            if (ShowGiver)
                                            {
                                                Message.AppendString("PURCHASER_NAME");
                                                Message.AppendString(GiverName);
                                                Message.AppendString("PURCHASER_FIGURE");
                                                Message.AppendString(GiverLook);
                                            }
                                            Message.AppendString("PRODUCT_CODE");
                                            Message.AppendString(Product);
                                            Message.AppendString("state");
                                            Message.AppendString(MagicRemove ? "1" : "0");
                                        }
                                        else
                                        {
                                            if (this.GetBaseItem().InteractionType == InteractionType.pinata)
                                            {
                                                Message.AppendInt32(0);
                                                Message.AppendInt32(7);
                                                Message.AppendString((this.ExtraData == "100") ? "8" : "6");
                                                if (this.ExtraData.Length <= 0)
                                                {
                                                    Message.AppendInt32(0);
                                                    Message.AppendInt32(100);
                                                }
                                                else
                                                {
                                                    Message.AppendInt32(int.Parse(this.ExtraData));
                                                    Message.AppendInt32(100);
                                                }
                                            }
                                            else
                                            {
                                                if (this.GetBaseItem().InteractionType == InteractionType.mannequin)
                                                {
                                                    Message.AppendInt32(0);
                                                    Message.AppendInt32(1);
                                                    Message.AppendInt32(3);
                                                    if (this.ExtraData.Contains(Convert.ToChar(5).ToString()))
                                                    {
                                                        string[] array = this.ExtraData.Split(new char[]
														{
															Convert.ToChar(5)
														});
                                                        Message.AppendString("GENDER");
                                                        Message.AppendString(array[0]);
                                                        Message.AppendString("FIGURE");
                                                        Message.AppendString(array[1]);
                                                        Message.AppendString("OUTFIT_NAME");
                                                        Message.AppendString(array[2]);
                                                    }
                                                    else
                                                    {
                                                        Message.AppendString("GENDER");
                                                        Message.AppendString("");
                                                        Message.AppendString("FIGURE");
                                                        Message.AppendString("");
                                                        Message.AppendString("OUTFIT_NAME");
                                                        Message.AppendString("");
                                                    }
                                                }
                                                else
                                                {
                                                    if (this.GetBaseItem().InteractionType == InteractionType.badge_display)
                                                    {
                                                        Message.AppendInt32(0);
                                                        Message.AppendInt32(2);
                                                        Message.AppendInt32(4);
                                                        Message.AppendString("0");
                                                        Message.AppendString(this.ExtraData);
                                                        Message.AppendString("");
                                                        Message.AppendString("");
                                                    }
                                                    else
                                                    {
                                                        if (this.GetBaseItem().InteractionType == InteractionType.moplaseed)
                                                        {
                                                            Message.AppendInt32(0);
                                                            Message.AppendInt32(1);
                                                            Message.AppendInt32(1);
                                                            Message.AppendString("rarity");
                                                            Message.AppendString(this.ExtraData);
                                                        }
                                                        else
                                                        {
                                                            if (this.GetBaseItem().InteractionType == InteractionType.roombg)
                                                            {
                                                                if (this.mRoom.TonerData == null)
                                                                {
                                                                    this.mRoom.TonerData = new TonerData(this.Id);
                                                                }
                                                                this.mRoom.TonerData.GenerateExtraData(Message);
                                                            }
                                                            else
                                                            {
                                                                if (this.GetBaseItem().InteractionType == InteractionType.mystery_box)
                                                                {
                                                                    Message.AppendInt32(0);
                                                                    Message.AppendInt32(0);
                                                                    if (this.ExtraData.Contains(Convert.ToChar(5).ToString()))
                                                                    {
                                                                        int num3 = int.Parse(this.ExtraData.Split(new char[]
																		{
																			Convert.ToChar(5)
																		})[0]);
                                                                        int num4 = int.Parse(this.ExtraData.Split(new char[]
																		{
																			Convert.ToChar(5)
																		})[1]);
                                                                        Message.AppendString((3 * num3 - num4).ToString());
                                                                    }
                                                                    else
                                                                    {
                                                                        this.ExtraData = "0" + Convert.ToChar(5) + "0";
                                                                        Message.AppendString("0");
                                                                    }
                                                                }
                                                                else
                                                                {
                                                                    if (this.LimitedNo > 0)
                                                                    {
                                                                        Message.AppendInt32(1);
                                                                        Message.AppendInt32(256);
                                                                        Message.AppendString(this.ExtraData);
                                                                        Message.AppendInt32(this.LimitedNo);
                                                                        Message.AppendInt32(this.LimitedTot);
                                                                    }
                                                                    else
                                                                    {
                                                                        Message.AppendInt32((this.GetBaseItem().InteractionType == InteractionType.tilestackmagic) ? 0 : 1);
                                                                        Message.AppendInt32(0);
                                                                        Message.AppendString(this.ExtraData);
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    
                    Message.AppendInt32(-1);
                    Message.AppendInt32((this.GetBaseItem().InteractionType == InteractionType.mystery_box || this.GetBaseItem().InteractionType == InteractionType.youtubetv) ? 2 : ((this.GetBaseItem().InteractionType == InteractionType.moplaseed || this.GetBaseItem().Modes > 1) ? 1 : 0));
                    Message.AppendUInt(this.UserID);
                    return;
                }
                if (this.IsWallItem)
                {
                    Message.AppendString(this.Id + string.Empty);
                    Message.AppendInt32(this.GetBaseItem().SpriteId);
                    Message.AppendString(this.wallCoord.ToString());
                    InteractionType interactionType = this.GetBaseItem().InteractionType;
                    if (interactionType == InteractionType.postit)
                    {
                        Message.AppendString(this.ExtraData.Split(new char[]
						{
							' '
						})[0]);
                    }
                    else
                    {
                        Message.AppendString(this.ExtraData);
                    }
                    Message.AppendInt32(-1);
                    Message.AppendInt32((this.GetBaseItem().Modes > 1) ? 1 : 0);
                    Message.AppendUInt(this.UserID);
                }
            }
        }



		internal void refreshItem()
		{
			this.mBaseItem = null;
		}
		internal Item GetBaseItem()
		{
			if (this.mBaseItem == null)
			{
				this.mBaseItem = CyberEnvironment.GetGame().GetItemManager().GetItem(this.BaseItem);
			}
			return this.mBaseItem;
		}
		internal Room GetRoom()
		{
			if (this.mRoom == null)
			{
				this.mRoom = CyberEnvironment.GetGame().GetRoomManager().GetRoom(this.RoomId);
			}
			return this.mRoom;
		}
		internal void UserWalksOnFurni(RoomUser user)
		{
			if (this.OnUserWalksOnFurni != null)
			{
				this.OnUserWalksOnFurni(this, new UserWalksOnArgs(user));
			}
			this.GetRoom().GetWiredHandler().ExecuteWired(WiredItemType.TriggerWalksOnFurni, new object[]
			{
				user,
				this
			});
			user.LastItem = this.Id;
		}
		internal void UserWalksOffFurni(RoomUser user)
		{
			if (this.OnUserWalksOffFurni != null)
			{
				this.OnUserWalksOffFurni(this, new UserWalksOnArgs(user));
			}
			this.GetRoom().GetWiredHandler().ExecuteWired(WiredItemType.TriggerWalksOffFurni, new object[]
			{
				user,
				this
			});
		}
	}
}
