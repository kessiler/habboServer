using Database_Manager.Database.Session_Details.Interfaces;
using Cyber.HabboHotel.Items;
using Cyber.HabboHotel.Rooms.Wired.Handlers.Conditions;
using Cyber.HabboHotel.Rooms.Wired.Handlers.Effects;
using Cyber.HabboHotel.Rooms.Wired.Handlers.Triggers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using Cyber.HabboHotel.Rooms.Wired;

namespace Cyber.HabboHotel.Rooms.Wired
{
	public class WiredHandler
	{
		private List<WiredItem> wiredItems;
		private Room Room;
		private Queue cycleItems;
		public WiredHandler(Room Room)
		{
			this.wiredItems = new List<WiredItem>();
			this.Room = Room;
			this.cycleItems = new Queue();
		}
		public WiredItem LoadWired(WiredItem Item)
		{
			if (Item == null || Item.Item == null)
			{
				if (this.wiredItems.Contains(Item))
				{
					this.wiredItems.Remove(Item);
				}
				return null;
			}
			using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
			{
				queryreactor.setQuery("SELECT * FROM wired_items WHERE id=@id LIMIT 1");
				queryreactor.addParameter("id", Item.Item.Id);
				DataRow row = queryreactor.getRow();
				if (row == null)
				{
					WiredItem wiredItem = this.GenerateNewItem(Item.Item);
					this.AddWired(wiredItem);
					this.SaveWired(wiredItem);
					return wiredItem;
				}
				Item.OtherString = row["string"].ToString();
				Item.OtherBool = (row["bool"].ToString() == "1");
				Item.Delay = (int)row["delay"];
				Item.OtherExtraString = row["extra_string"].ToString();
				Item.OtherExtraString2 = row["extra_string_2"].ToString();
				string[] array = row["items"].ToString().Split(new char[]
				{
					';'
				});
				for (int i = 0; i < array.Length; i++)
				{
					string s = array[i];
					int value = 0;
					if (int.TryParse(s, out value))
					{
						RoomItem item = this.Room.GetRoomItemHandler().GetItem(Convert.ToUInt32(value));
						if (Item != null)
						{
							Item.Items.Add(item);
						}
					}
				}
				this.AddWired(Item);
			}
			return Item;
		}
		public void SaveWired(WiredItem Item)
		{
            if (Item == null)
            {
                return;
            }
			checked
			{
				using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
				{
					string text = "";
					int num = 0;
					foreach (RoomItem current in Item.Items)
					{
						if (num != 0)
						{
							text += ";";
						}
						text += current.Id;
						num++;
					}
					if (Item.OtherString == null)
					{
						Item.OtherString = "";
					}
					if (Item.OtherExtraString == null)
					{
						Item.OtherExtraString = "";
					}
					if (Item.OtherExtraString2 == null)
					{
						Item.OtherExtraString2 = "";
					}
					queryreactor.setQuery("REPLACE INTO wired_items VALUES (@id, @items, @delay, @string, @bool, @extrastring, @extrastring2)");
					queryreactor.addParameter("id", Item.Item.Id);
					queryreactor.addParameter("items", text);
					queryreactor.addParameter("delay", Item.Delay);
					queryreactor.addParameter("string", Item.OtherString);
					queryreactor.addParameter("bool", CyberEnvironment.BoolToEnum(Item.OtherBool));
					queryreactor.addParameter("extrastring", Item.OtherExtraString);
					queryreactor.addParameter("extrastring2", Item.OtherExtraString2);
					queryreactor.runQuery();
				}
			}
		}
		public void ReloadWired(WiredItem Item)
		{
			this.SaveWired(Item);
			this.RemoveWired(Item);
			this.AddWired(Item);
		}

        public void ResetExtraString(WiredItemType Type)
        {
            lock (wiredItems)
            {
                foreach (WiredItem current in wiredItems)
                {
                    if (current.Type == Type)
                    {
                        current.OtherExtraString = "0";
                    }
                }
            }
        }

		public bool ExecuteWired(WiredItemType Type, params object[] Stuff)
		{
			bool flag = false;
			bool result;
			try
			{
				if (!this.IsTrigger(Type))
				{
					result = false;
				}
				else
				{
					foreach (WiredItem current in this.wiredItems)
					{
						if (current.Type == Type && current.Execute(Stuff))
						{
							flag = true;
						}
					}
					result = flag;
				}
			}
			catch
			{
				result = false;
			}
			return result;
		}
		public void OnCycle()
		{
			foreach (WiredItem current in this.wiredItems)
			{
				if (current is WiredCycler && current.Type == WiredItemType.TriggerRepeatEffect && !this.IsCycleQueued(current))
				{
					this.cycleItems.Enqueue(current);
				}
                else if (current is WiredCycler && current.Type == WiredItemType.TriggerLongRepeater && !this.IsCycleQueued(current))
                {
                    this.cycleItems.Enqueue(current);
                }
			}
			Queue queue = new Queue();
			lock (this.cycleItems.SyncRoot)
			{
				while (this.cycleItems.Count > 0)
				{
					WiredItem wiredItem = (WiredItem)this.cycleItems.Dequeue();
					if (wiredItem is WiredCycler)
					{
						WiredCycler wiredCycler = (WiredCycler)wiredItem;
						if (!wiredCycler.OnCycle())
						{
							queue.Enqueue(wiredItem);
						}
					}
				}
			}
			this.cycleItems = queue;
		}
		public void EnqueueCycle(WiredItem Item)
		{
			if (!this.cycleItems.Contains(Item))
			{
				this.cycleItems.Enqueue(Item);
			}
		}
		public bool IsCycleQueued(WiredItem Item)
		{
			return this.cycleItems.Contains(Item);
		}
		private bool IsTrigger(WiredItemType Type)
		{
			switch (Type)
			{
			case WiredItemType.TriggerUserEntersRoom:
			case WiredItemType.TriggerUserSaysKeyword:
			case WiredItemType.TriggerRepeatEffect:
			case WiredItemType.TriggerGameStarts:
			case WiredItemType.TriggerGameEnds:
			case WiredItemType.TriggerToggleFurni:
			case WiredItemType.TriggerWalksOnFurni:
			case WiredItemType.TriggerWalksOffFurni:
                case WiredItemType.TriggerScoreAchieved:
                case WiredItemType.TriggerLongRepeater:
				return true;
			default:
				return false;
			}
		}
		private bool IsEffect(WiredItemType Type)
		{
			switch (Type)
			{
			case WiredItemType.EffectShowMessage:
			case WiredItemType.EffectTeleportToFurni:
			case WiredItemType.EffectToggleFurniState:
			case WiredItemType.EffectMoveRotateFurni:
			case WiredItemType.EffectKickUser:
			case WiredItemType.EffectGiveReward:
                case WiredItemType.EffectResetPosition:
                case WiredItemType.EffectGiveScore:
                case WiredItemType.EffectResetTimers:
                case WiredItemType.EffectMuteUser:
				return true;
			}
			return false;
		}
        private bool IsCondition(WiredItemType Type)
        {
            switch (Type)
            {
                case WiredItemType.ConditionFurniHasUsers:
                case WiredItemType.ConditionFurniHasFurni:
                case WiredItemType.ConditionTriggererOnFurni:
                case WiredItemType.ConditionFurniCoincides:
                case WiredItemType.ConditionIsGroupMember:
                case WiredItemType.ConditionFurniTypeMatches:
                case WiredItemType.ConditionHowManyUsers:
                case WiredItemType.ConditionTriggererNotOnFurni:
                case WiredItemType.ConditionFurniHasNotFurni:
                case WiredItemType.ConditionFurniHaveNotUsers:
                case WiredItemType.ConditionItemsDontMatch:
                case WiredItemType.ConditionFurniTypeDontMatch:
                case WiredItemType.ConditionIsNotGroupMember:
                case WiredItemType.ConditionNotHowManyUsers:
                case WiredItemType.ConditionIsWearingEffect:
                case WiredItemType.ConditionIsNotWearingEffect:
                case WiredItemType.ConditionIsWearingBadge:
                case WiredItemType.ConditionIsNotWearingBadge:
                case WiredItemType.ConditionDateRangeActive:
                    return true;
                default:
                    return false;
            }
        }
		public void AddWired(WiredItem Item)
		{
			if (this.wiredItems.Contains(Item))
			{
				this.wiredItems.Remove(Item);
			}
			this.wiredItems.Add(Item);
		}
		public void RemoveWired(WiredItem Item)
		{
			if (!this.wiredItems.Contains(Item))
			{
				this.wiredItems.Remove(Item);
			}
			this.wiredItems.Remove(Item);
		}
		public void RemoveWired(RoomItem Item)
		{
			foreach (WiredItem current in this.wiredItems)
			{
				if (current.Item.Id == Item.Id)
				{
					Queue queue = new Queue();
					lock (this.cycleItems.SyncRoot)
					{
						while (this.cycleItems.Count > 0)
						{
							WiredItem wiredItem = (WiredItem)this.cycleItems.Dequeue();
							if (wiredItem.Item.Id != Item.Id)
							{
								queue.Enqueue(wiredItem);
							}
						}
					}
					this.cycleItems = queue;
					this.wiredItems.Remove(current);
					break;
				}
			}
		}
        public WiredItem GenerateNewItem(RoomItem Item)
        {
            switch (Item.GetBaseItem().InteractionType)
            {
                case InteractionType.triggerroomenter:
                    return new UserEntersRoom(Item, this.Room);
                case InteractionType.triggergameend:
                    return new GameEnds(Item, this.Room);
                case InteractionType.triggergamestart:
                    return new GameStarts(Item, this.Room);
                case InteractionType.triggerrepeater:
                    return new Repeater(Item, this.Room);
                case InteractionType.triggerlongrepeater:
                    return new LongRepeater(Item, this.Room);
                case InteractionType.triggeronusersay:
                    return new SaysKeyword(Item, this.Room);
                case InteractionType.triggerscoreachieved:
                    return new ScoreAchieved(Item, this.Room);
                case InteractionType.triggerstatechanged:
                    return new FurniStateToggled(Item, this.Room);
                case InteractionType.triggerwalkonfurni:
                    return new WalksOnFurni(Item, this.Room);
                case InteractionType.triggerwalkofffurni:
                    return new WalksOffFurni(Item, this.Room);
                case InteractionType.actionmoverotate:
                    return new MoveRotateFurni(Item, this.Room);
                case InteractionType.actionshowmessage:
                    return new ShowMessage(Item, this.Room);
                case InteractionType.actionteleportto:
                    return new TeleportToFurni(Item, this.Room);
                case InteractionType.actiontogglestate:
                    return new ToggleFurniState(Item, this.Room);
                case InteractionType.actionkickuser:
                    return new KickUser(Item, this.Room);

                case InteractionType.conditionfurnishaveusers:
                    return new FurniHasUsers(Item, this.Room);

                // CONDICIONES NUEVAS:

                case InteractionType.conditionitemsmatches:
                    return new ItemsCoincide(Item, this.Room);

                case InteractionType.conditionfurnitypematches:
                    return new ItemsTypeMatches(Item, this.Room);
                case InteractionType.conditionhowmanyusersinroom:
                    return new HowManyUsers(Item, this.Room);

                case InteractionType.conditiongroupmember:
                    return new IsGroupMember(Item, this.Room);
                case InteractionType.conditiontriggeronfurni:
                    return new TriggererOnFurni(Item, this.Room);
                case InteractionType.conditionfurnihasfurni:
                    return new FurniHasFurni(Item, this.Room);
                case InteractionType.conditionuserwearingeffect:
                    return new UserIsWearingEffect(Item, this.Room);
                case InteractionType.conditionuserwearingbadge:
                    return new UserIsWearingBadge(Item, this.Room);
                case InteractionType.conditiondaterangeactive:
                    return new DateRangeActive(Item, this.Room);

                // CONDICIONES NEGATIVAS:
                case InteractionType.conditiontriggerernotonfurni:
                    return new TriggererNotOnFurni(Item, this.Room);

                case InteractionType.conditionfurnihasnotfurni:
                    return new FurniHasNotFurni(Item, this.Room);

                case InteractionType.conditionfurnishavenotusers:
                    return new FurniHasNotUsers(Item, this.Room);

                case InteractionType.conditionitemsdontmatch:
                    return new ItemsNotCoincide(Item, this.Room);

                case InteractionType.conditionfurnitypedontmatch:
                    return new ItemsTypeDontMatch(Item, this.Room);

                case InteractionType.conditionnotgroupmember:
                    return new IsNotGroupMember(Item, this.Room);

                case InteractionType.conditionnegativehowmanyusers:
                    return new NotHowManyUsersInRoom(Item, this.Room);
                case InteractionType.conditionusernotwearingeffect:
                    return new UserIsNotWearingEffect(Item, this.Room);
                case InteractionType.conditionusernotwearingbadge:
                    return new UserIsNotWearingBadge(Item, this.Room);

                // Efectos NUEVOS:
                case InteractionType.actiongivereward:
                    return new GiveReward(Item, this.Room);
                case InteractionType.actionposreset:
                    return new ResetPosition(Item, this.Room);
                case InteractionType.actiongivescore:
                    return new GiveScore(Item, this.Room);
                case InteractionType.actionmuteuser:
                    return new MuteUser(Item, this.Room);
            }
            return null;
        }

		public void OnEvent(WiredItem Item)
		{
			Item.Item.ExtraData = Item.Item.ExtraData == "0" ? "1" : "0";
			Item.Item.UpdateState(false, true);
			Item.Item.ReqUpdate(1, true);
		}
		public List<WiredItem> GetConditions(WiredItem Item)
		{
			List<WiredItem> list = new List<WiredItem>();
			foreach (WiredItem current in this.wiredItems)
			{
				if (this.IsCondition(current.Type) && current.Item.GetX == Item.Item.GetX && current.Item.GetY == Item.Item.GetY)
				{
					list.Add(current);
				}
			}
			return list;
		}
		public List<WiredItem> GetEffects(WiredItem Item)
		{
			List<WiredItem> list = new List<WiredItem>();
			foreach (WiredItem current in this.wiredItems)
			{
				if (this.IsEffect(current.Type) && current.Item.GetX == Item.Item.GetX && current.Item.GetY == Item.Item.GetY)
				{
					list.Add(current);
				}
			}
			return list;
		}
		public WiredItem GetWired(RoomItem Item)
		{
			foreach (WiredItem current in this.wiredItems)
			{
				if (Item.Id == current.Item.Id)
				{
					return current;
				}
			}
			return null;
		}
		public void MoveWired(RoomItem Item)
		{
			WiredItem wired = this.GetWired(Item);
			if (wired == null)
			{
				return;
			}
			wired.Item = Item;
			this.RemoveWired(Item);
			this.AddWired(wired);
		}
		public void Destroy()
		{
			this.wiredItems.Clear();
			this.cycleItems.Clear();
		}
	}
}
