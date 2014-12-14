using Cyber.HabboHotel.GameClients;
using Cyber.HabboHotel.Pathfinding;
using Cyber.HabboHotel.Rooms;
using Cyber.Messages;
using Cyber.Messages.Headers;
using System;
using System.Drawing;
using Cyber.HabboHotel.PathFinding;

namespace Cyber.HabboHotel.Items.Interactor
{
	internal class InteractorPuzzleBox : IFurniInteractor
	{
		public void OnPlace(GameClient Session, RoomItem Item)
		{
		}
		public void OnRemove(GameClient Session, RoomItem Item)
		{
		}
		public void OnTrigger(GameClient Session, RoomItem Item, int Request, bool HasRights)
		{
			if (Session == null)
			{
				return;
			}
			RoomUser roomUserByHabbo = Item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
			if (roomUserByHabbo == null)
			{
				return;
			}
			checked
			{
				if (Math.Abs(roomUserByHabbo.X - Item.GetX) < 2 && Math.Abs(roomUserByHabbo.Y - Item.GetY) < 2)
				{
                    roomUserByHabbo.SetRot(PathFinder.CalculateRotation(roomUserByHabbo.X, roomUserByHabbo.Y, Item.GetX, Item.GetY), false);
					Room room = Item.GetRoom();
					Point point = new Point(0, 0);
					if (roomUserByHabbo.RotBody == 4)
					{
						point = new Point(Item.GetX, Item.GetY + 1);
					}
					else
					{
						if (roomUserByHabbo.RotBody == 0)
						{
							point = new Point(Item.GetX, Item.GetY - 1);
						}
						else
						{
							if (roomUserByHabbo.RotBody == 6)
							{
								point = new Point(Item.GetX - 1, Item.GetY);
							}
							else
							{
								if (roomUserByHabbo.RotBody != 2)
								{
									return;
								}
								point = new Point(Item.GetX + 1, Item.GetY);
							}
						}
					}
					if (room.GetGameMap().validTile(point.X, point.Y))
					{
						double num = Item.GetRoom().GetGameMap().SqAbsoluteHeight(point.X, point.Y);
						ServerMessage serverMessage = new ServerMessage();
						serverMessage.Init(Outgoing.ItemAnimationMessageComposer);
						serverMessage.AppendInt32(Item.GetX);
						serverMessage.AppendInt32(Item.GetY);
						serverMessage.AppendInt32(point.X);
						serverMessage.AppendInt32(point.Y);
						serverMessage.AppendInt32(1);
						serverMessage.AppendUInt(Item.Id);
						serverMessage.AppendString(Item.GetZ.ToString(CyberEnvironment.cultureInfo));
						serverMessage.AppendString(num.ToString(CyberEnvironment.cultureInfo));
						serverMessage.AppendInt32(0);
						room.SendMessage(serverMessage);
						Item.GetRoom().GetRoomItemHandler().SetFloorItem(roomUserByHabbo.GetClient(), Item, point.X, point.Y, Item.Rot, false, false, false);
						return;
					}
				}
				else
				{
					roomUserByHabbo.MoveTo(Item.GetX + 1, Item.GetY);
				}
			}
		}
		public void OnUserWalk(GameClient Session, RoomItem Item, RoomUser User)
		{
		}
		public void OnWiredTrigger(RoomItem Item)
		{
		}
	}
}
