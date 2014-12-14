using Cyber.HabboHotel.Items;
using Cyber.Messages;
using Cyber.Util;
using Cyber.Messages.Headers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
namespace Cyber.HabboHotel.Rooms.Wired.Handlers.Effects
{
	internal class MoveRotateFurni : WiredItem, WiredCycler
	{
		private WiredItemType mType = WiredItemType.EffectMoveRotateFurni;
		private RoomItem mItem;
		private Room mRoom;
		private List<RoomItem> mItems;
		private int mDelay;
		private long mNext;
		private int mRot;
		private int mDir;
		public WiredItemType Type
		{
			get
			{
				return this.mType;
			}
		}
		public RoomItem Item
		{
			get
			{
				return this.mItem;
			}
			set
			{
				this.mItem = value;
			}
		}
		public Room Room
		{
			get
			{
				return this.mRoom;
			}
		}
		public List<RoomItem> Items
		{
			get
			{
				return this.mItems;
			}
			set
			{
				this.mItems = value;
			}
		}
		public string OtherString
		{
			get
			{
				return this.mRot + ";" + this.mDir;
			}
			set
			{
				this.mRot = int.Parse(value.Split(new char[]
				{
					';'
				})[0]);
				this.mDir = int.Parse(value.Split(new char[]
				{
					';'
				})[1]);
			}
		}
		public string OtherExtraString
		{
			get
			{
				return "";
			}
			set
			{
			}
		}
		public string OtherExtraString2
		{
			get
			{
				return "";
			}
			set
			{
			}
		}
		public bool OtherBool
		{
			get
			{
				return true;
			}
			set
			{
			}
		}
		public int Delay
		{
			get
			{
				return this.mDelay;
			}
			set
			{
				this.mDelay = value;
			}
		}
		public Queue ToWork
		{
			get
			{
				return null;
			}
			set
			{
			}
		}
		public MoveRotateFurni(RoomItem Item, Room Room)
		{
			this.mItem = Item;
			this.mRoom = Room;
			this.mItems = new List<RoomItem>();
			this.mDelay = 0;
			this.mNext = 0L;
			this.mRot = 0;
			this.mDir = 0;
		}
		public bool Execute(params object[] Stuff)
		{
			if (this.mItems.Count == 0)
			{
				return false;
			}
			checked
			{
				if (this.mDelay > 0)
				{
					if (this.mNext == 0L || this.mNext < CyberEnvironment.Now())
					{
						this.mNext = CyberEnvironment.Now() + unchecked((long)this.mDelay);
					}
					this.Room.GetWiredHandler().EnqueueCycle(this);
				}
				else
				{
					this.mNext = 0L;
					if (!this.OnCycle())
					{
						if (this.mNext == 0L || this.mNext < CyberEnvironment.Now())
						{
							this.mNext = CyberEnvironment.Now() + unchecked((long)this.mDelay);
						}
						this.Room.GetWiredHandler().EnqueueCycle(this);
					}
				}
				return true;
			}
		}
		public bool OnCycle()
		{
			long num = CyberEnvironment.Now();
			if (this.Room == null || this.Room.GetRoomItemHandler() == null || this.Room.GetRoomItemHandler().mFloorItems == null)
			{
				return false;
			}
			if (this.mNext < num)
			{
				foreach (RoomItem current in this.mItems)
				{
					if (current != null && this.Room.GetRoomItemHandler().mFloorItems.ContainsKey(current.Id))
					{
						Point left = this.HandleMovement(this.mDir, new Point(current.GetX, current.GetY));
						int num2 = this.HandleRotation(this.mRot, current.Rot);
						if (this.mRoom.GetGameMap().CanRollItemHere(left.X, left.Y) && !this.mRoom.GetGameMap().SquareHasUsers(left.X, left.Y))
						{
							double num3 = 0.0;
							bool flag = true;
							List<RoomItem> roomItemForSquare = this.mRoom.GetGameMap().GetRoomItemForSquare(left.X, left.Y);
							foreach (RoomItem current2 in roomItemForSquare)
							{
								if (current2 != null)
								{
									if (current2.TotalHeight > num3)
									{
										num3 = current.TotalHeight;
									}
									if (flag && !current2.GetBaseItem().Stackable)
									{
										flag = false;
									}
								}
							}
							if (num2 != current.Rot)
							{
								current.Rot = num2;
								ServerMessage message = new ServerMessage(Outgoing.UpdateRoomItemMessageComposer);
								current.Serialize(message);
								this.mRoom.SendMessage(message);
							}
							if (flag && left != current.Coordinate)
							{
								ServerMessage serverMessage = new ServerMessage();
								serverMessage.Init(Outgoing.ItemAnimationMessageComposer);
								serverMessage.AppendInt32(current.GetX);
								serverMessage.AppendInt32(current.GetY);
								serverMessage.AppendInt32(left.X);
								serverMessage.AppendInt32(left.Y);
								serverMessage.AppendInt32(1);
								serverMessage.AppendUInt(current.Id);
								serverMessage.AppendString(TextHandling.GetString(current.GetZ));
								serverMessage.AppendString(TextHandling.GetString(num3));
								serverMessage.AppendInt32(0);
								this.mRoom.SendMessage(serverMessage);
								this.mRoom.GetRoomItemHandler().SetFloorItem(current, left.X, left.Y, num3);
							}
						}
					}
				}
				this.mNext = 0L;
				return true;
			}
			return false;
		}
		private int HandleRotation(int mode, int rotation)
		{
			checked
			{
				switch (mode)
				{
				case 1:
					rotation += 2;
					if (rotation > 6)
					{
						rotation = 0;
					}
					break;
				case 2:
					rotation -= 2;
					if (rotation < 0)
					{
						rotation = 6;
					}
					break;
				case 3:
				{
					Random random = new Random();
					if (random.Next(0, 2) == 0)
					{
						rotation += 2;
						if (rotation > 6)
						{
							rotation = 0;
						}
					}
					else
					{
						rotation -= 2;
						if (rotation < 0)
						{
							rotation = 6;
						}
					}
					break;
				}
				}
				return rotation;
			}
		}
		private Point HandleMovement(int Mode, Point Position)
		{
			Point result = default(Point);
			Random random = new Random();
			checked
			{
				switch (Mode)
				{
				case 0:
					result = Position;
					break;
				case 1:
					switch (random.Next(1, 5))
					{
					case 1:
						result = new Point(Position.X + 1, Position.Y);
						break;
					case 2:
						result = new Point(Position.X - 1, Position.Y);
						break;
					case 3:
						result = new Point(Position.X, Position.Y + 1);
						break;
					case 4:
						result = new Point(Position.X, Position.Y - 1);
						break;
					}
					break;
				case 2:
					if (random.Next(0, 2) == 1)
					{
						result = new Point(Position.X - 1, Position.Y);
					}
					else
					{
						result = new Point(Position.X + 1, Position.Y);
					}
					break;
				case 3:
					if (random.Next(0, 2) == 1)
					{
						result = new Point(Position.X, Position.Y - 1);
					}
					else
					{
						result = new Point(Position.X, Position.Y + 1);
					}
					break;
				case 4:
					result = new Point(Position.X, Position.Y - 1);
					break;
				case 5:
					result = new Point(Position.X + 1, Position.Y);
					break;
				case 6:
					result = new Point(Position.X, Position.Y + 1);
					break;
				case 7:
					result = new Point(Position.X - 1, Position.Y);
					break;
				}
				return result;
			}
		}
	}
}
