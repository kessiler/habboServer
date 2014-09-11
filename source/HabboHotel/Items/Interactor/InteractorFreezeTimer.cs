using Cyber.HabboHotel.GameClients;
using Cyber.HabboHotel.Rooms;
using System;
namespace Cyber.HabboHotel.Items.Interactor
{
	internal class InteractorFreezeTimer : IFurniInteractor
	{
		public void OnPlace(GameClient Session, RoomItem Item)
		{
		}
		public void OnRemove(GameClient Session, RoomItem Item)
		{
		}
		public void OnTrigger(GameClient Session, RoomItem Item, int Request, bool HasRights)
		{
			if (!Item.GetRoom().CheckRights(Session))
			{
				return;
			}
			int num = 0;
			if (!string.IsNullOrEmpty(Item.ExtraData))
			{
				try
				{
					num = int.Parse(Item.ExtraData);
				}
				catch
				{
				}
			}
			if (Request == 2)
			{
				if (Item.pendingReset && num > 0)
				{
					num = 0;
					Item.pendingReset = false;
				}
				else
				{
					if (num == 0 || num == 30 || num == 60 || num == 120 || num == 180 || num == 300 || num == 600)
					{
						if (num == 0)
						{
							num = 30;
						}
						else
						{
							if (num == 30)
							{
								num = 60;
							}
							else
							{
								if (num == 60)
								{
									num = 120;
								}
								else
								{
									if (num == 120)
									{
										num = 180;
									}
									else
									{
										if (num == 180)
										{
											num = 300;
										}
										else
										{
											if (num == 300)
											{
												num = 600;
											}
											else
											{
												if (num == 600)
												{
													num = 0;
												}
											}
										}
									}
								}
							}
						}
					}
					else
					{
						num = 0;
					}
					Item.UpdateNeeded = false;
				}
			}
			else
			{
				if (Request == 1 && !Item.GetRoom().GetFreeze().GameIsStarted)
				{
					Item.UpdateNeeded = !Item.UpdateNeeded;
					if (Item.UpdateNeeded)
					{
						Item.GetRoom().GetFreeze().StartGame();
					}
					Item.pendingReset = true;
				}
			}
			Item.ExtraData = num.ToString();
			Item.UpdateState();
		}
		public void OnUserWalk(GameClient Session, RoomItem Item, RoomUser User)
		{
		}
		public void OnWiredTrigger(RoomItem Item)
		{
		}
	}
}
