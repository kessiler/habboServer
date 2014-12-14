using Cyber.Core;
using Cyber.Util;
using Cyber.HabboHotel.GameClients;
using Database_Manager.Database.Session_Details.Interfaces;
using Cyber.HabboHotel.PathFinding;
using Cyber.HabboHotel.Pets;
using Cyber.HabboHotel.Rooms;
using Cyber.HabboHotel.Items;
using System;
using System.Linq;
using System.Drawing;
using System.Threading;

namespace Cyber.HabboHotel.RoomBots
{
	internal class PetBot : BotAI
	{
		private int SpeechTimer;
		private int ActionTimer;
		private int EnergyTimer;
		internal PetBot(int VirtualId)
		{
			checked
			{
				this.SpeechTimer = new Random((VirtualId ^ 2) + DateTime.Now.Millisecond).Next(10, 60);
				this.ActionTimer = new Random((VirtualId ^ 2) + DateTime.Now.Millisecond).Next(10, 30 + VirtualId);
				this.EnergyTimer = new Random((VirtualId ^ 2) + DateTime.Now.Millisecond).Next(10, 60);
			}
		}

		private void RemovePetStatus()
		{
			RoomUser roomUser = base.GetRoomUser();
			roomUser.Statusses.Clear();
            roomUser.UpdateNeeded = true;
		}
		internal override void OnSelfEnterRoom()
		{
			Point randomWalkableSquare = base.GetRoom().GetGameMap().getRandomWalkableSquare();
			if (base.GetRoomUser() != null && base.GetRoomUser().PetData.Type != 16u)
			{
				base.GetRoomUser().MoveTo(randomWalkableSquare.X, randomWalkableSquare.Y);
			}
		}
		internal override void OnSelfLeaveRoom(bool Kicked)
		{
		}

		internal override void OnUserEnterRoom(RoomUser User)
		{
			if (User.GetClient() != null && User.GetClient().GetHabbo() != null)
			{
				RoomUser roomUser = base.GetRoomUser();
				if (roomUser != null && User.GetClient().GetHabbo().Username == roomUser.PetData.OwnerName)
				{
					Random random = new Random();
					string[] value = PetLocale.GetValue("welcome.speech.pet");
					string message = value[random.Next(0, checked(value.Length - 1))];
                    message += User.GetUsername();
					roomUser.Chat(null, message, false, 0, 0);
				}
			}
		}
		internal override void OnUserLeaveRoom(GameClient Client)
		{
		}

		internal override void OnUserSay(RoomUser User, string Message)
		{
            RoomUser roomUser = base.GetRoomUser();

            if (roomUser.PetData.OwnerId != User.GetClient().GetHabbo().Id)
            {
                return;
            }

			if (Message == "" || Message == null)
            {
                Message = " ";
            }

            Message = Message.Substring(1);

            bool Lazy = false;
            bool Unknown = false;
            bool Sleeping = false;

            try
            {
            switch (Message.ToUpper())
            {
                case "DESCANSA":
                case "RELAX":
                case "REST":
                    this.RemovePetStatus();
                    break;

                case "COME":
                case "COMER":
                case "EAT":
                    if (!roomUser.PetData.HasCommand(43))
                    {
                        Unknown = true;
                        break;
                    }

                    this.RemovePetStatus();
                    break;

                case "SI�NTATE":
                case "SIENTATE":
                case "SIT":
                case "SIÉNTATE":
                    if (roomUser.PetData.Energy < 20 || roomUser.PetData.Nutrition < 25)
                    {
                        Lazy = true;
                        break;
                    }
                    this.RemovePetStatus();
							roomUser.PetData.AddExperience(10);
							roomUser.Statusses.Add("sit", "");
                            roomUser.Statusses.Add("gst", "joy");
                            roomUser.UpdateNeeded = true;
							this.ActionTimer = 25;
                            this.EnergyTimer = 10;

                            SubtractAttributes();
                            break;

                case "TUMBATE":
                case "LAY":
                case "ACUESTATE":
                case "ACUÉSTATE":
                case "TÚMBATE":
                            if (!roomUser.PetData.HasCommand(2))
                            {
                                Unknown = true;
                                break;
                            }
                            if (roomUser.PetData.Energy < 20 || roomUser.PetData.Nutrition < 25)
                            {
                                Lazy = true;
                                break;
                            }
                    this.RemovePetStatus();
							roomUser.PetData.AddExperience(10);
							roomUser.Statusses.Add("lay", "");
                            roomUser.Statusses.Add("gst", "sml");
                            roomUser.UpdateNeeded = true;
							this.ActionTimer = 25;
                            this.EnergyTimer = 10;

                            SubtractAttributes();
                            break;

                case "VEN":
                case "VEN AQUÍ":
                case "VEN AQU�":
                case "VEN AQUí":
                case "SÍGUEME":
                case "FOLLOW":
                case "FOLLOW ME":
                            if (!roomUser.PetData.HasCommand(3))
                            {
                                Unknown = true;
                                break;
                            }
                            else if (!roomUser.PetData.HasCommand(7))
                            {
                                Unknown = true;
                                break;
                            }
                            if (roomUser.PetData.Energy < 20 || roomUser.PetData.Nutrition < 25)
                            {
                                Lazy = true;
                                break;
                            }
                    this.RemovePetStatus();
							roomUser.PetData.AddExperience(11);
                            roomUser.MoveTo(User.SquareInFront);
                            roomUser.Statusses.Add("gst", "sml");
                            roomUser.UpdateNeeded = true;

							this.ActionTimer = 25;
                            this.EnergyTimer = 10;
                            SubtractAttributes();
                            break;

                case "LEVANTA":
                case "STAND":
                            if (!roomUser.PetData.HasCommand(8))
                            {
                                Unknown = true;
                                break;
                            }
                            if (roomUser.PetData.Energy < 20 || roomUser.PetData.Nutrition < 25)
                            {
                                Lazy = true;
                                break;
                            }
                            this.RemovePetStatus();
							roomUser.PetData.AddExperience(25);
                            roomUser.Statusses.Add("std", "");
                            roomUser.UpdateNeeded = true;

							this.ActionTimer = 25;
                            this.EnergyTimer = 10;
                            SubtractAttributes();
                            break;

                case "JUMP":
                case "SALTA":
                case "BOTA":
                            if (!roomUser.PetData.HasCommand(19))
                            {
                                Unknown = true;
                                break;
                            }
                            else if (!roomUser.PetData.HasCommand(9))
                            {
                                Unknown = true;
                                break;
                            }
                            else if (roomUser.PetData.Energy < 20 || roomUser.PetData.Nutrition < 25)
                            {
                                Lazy = true;
                                break;
                            }
                            this.RemovePetStatus();
							roomUser.PetData.AddExperience(35);
                            roomUser.Statusses.Add("jmp", "");
                            roomUser.Statusses.Add("gst", "joy");
                            roomUser.UpdateNeeded = true;

							this.ActionTimer = 45;
                            this.EnergyTimer = 20;
                            SubtractAttributes();
                            break;

                case "ADELANTE":
                case "FORWARD":
                case "DELANTE":
                case "MOVE FORWARD":
                case "STRAIGHT":
                            if (!roomUser.PetData.HasCommand(24))
                            {
                                Unknown = true;
                                break;
                            }
                            if (roomUser.PetData.Energy < 20 || roomUser.PetData.Nutrition < 25)
                            {
                                Lazy = true;
                                break;
                            }
                            this.RemovePetStatus();
                            roomUser.MoveTo(roomUser.SquareInFront);
                            roomUser.PetData.AddExperience(35);
                            roomUser.Statusses.Add("gst", "sml");
                            roomUser.UpdateNeeded = true;
                            break;

                case "IZQUIERDA":
                case "FOLLOW LEFT":
                case "LEFT":
                            if (!roomUser.PetData.HasCommand(15))
                            {
                                Unknown = true;
                                break;
                            }
                            if (roomUser.PetData.Energy < 20 || roomUser.PetData.Nutrition < 25)
                            {
                                Lazy = true;
                                break;
                            }

                            this.RemovePetStatus();

                            switch (roomUser.RotBody)
                            {
                                case 0:
                                    roomUser.MoveTo(roomUser.X + 2, roomUser.Y);
                                    break;

                                case 1:
                                    roomUser.MoveTo(roomUser.X - 2, roomUser.Y - 2);
                                    break;
                                    
                                case 2:
                                    roomUser.MoveTo(roomUser.X, roomUser.Y + 2);
                                    break;

                                case 3:
                                    roomUser.MoveTo(roomUser.X + 2, roomUser.Y - 2);
                                    break;

                                case 4:
                                    roomUser.MoveTo(roomUser.X - 2, roomUser.Y);
                                    break;

                                case 5:
                                    roomUser.MoveTo(roomUser.X + 2, roomUser.Y + 2);
                                    break;

                                case 6:
                                    roomUser.MoveTo(roomUser.X, roomUser.Y - 2);
                                    break;

                                case 7:
                                    roomUser.MoveTo(roomUser.X - 2, roomUser.Y + 2);
                                    break;
                            }

                            roomUser.PetData.AddExperience(35);
                            roomUser.Statusses.Add("gst", "sml");
                            roomUser.UpdateNeeded = true;
                            break;

                case "DERECHA":
                case "FOLLOW RIGHT":
                case "RIGHT":
                            if (!roomUser.PetData.HasCommand(16))
                            {
                                Unknown = true;
                                break;
                            }
                            if (roomUser.PetData.Energy < 20 || roomUser.PetData.Nutrition < 25)
                            {
                                Lazy = true;
                                break;
                            }

                            this.RemovePetStatus();

                            switch (roomUser.RotBody)
                            {
                                case 0:
                                    roomUser.MoveTo(roomUser.X - 2, roomUser.Y);
                                    break;

                                case 1:
                                    roomUser.MoveTo(roomUser.X + 2, roomUser.Y + 2);
                                    break;
                                    
                                case 2:
                                    roomUser.MoveTo(roomUser.X, roomUser.Y - 2);
                                    break;

                                case 3:
                                    roomUser.MoveTo(roomUser.X - 2, roomUser.Y + 2);
                                    break;

                                case 4:
                                    roomUser.MoveTo(roomUser.X + 2, roomUser.Y);
                                    break;

                                case 5:
                                    roomUser.MoveTo(roomUser.X - 2, roomUser.Y - 2);
                                    break;

                                case 6:
                                    roomUser.MoveTo(roomUser.X, roomUser.Y + 2);
                                    break;

                                case 7:
                                    roomUser.MoveTo(roomUser.X + 2, roomUser.Y - 2);
                                    break;
                            }

                            roomUser.PetData.AddExperience(35);
                            roomUser.Statusses.Add("gst", "sml");
                            roomUser.UpdateNeeded = true;
                            break;

                case "PIDE":
                case "BEG":
                            if (!roomUser.PetData.HasCommand(4))
                            {
                                Unknown = true;
                                break;
                            }
                     if (roomUser.PetData.Energy < 20 || roomUser.PetData.Nutrition < 25)
                            {
                                Lazy = true;
                                break;
                            }
                    this.RemovePetStatus();
							roomUser.PetData.AddExperience(11);
                            roomUser.Statusses.Add("beg", "");
                            roomUser.Statusses.Add("gst", "sml");
                            roomUser.UpdateNeeded = true;

							this.ActionTimer = 25;
                            this.EnergyTimer = 10;
                            SubtractAttributes();
                            break;

                case "DEAD":
                case "PLAY DEAD":
                case "HAZ EL MUERTO":
                    //
                    if (!roomUser.PetData.HasCommand(5))
                    {
                        Unknown = true;
                        break;
                    }
                            if (roomUser.PetData.Energy < 20 || roomUser.PetData.Nutrition < 25)
                            {
                                Lazy = true;
                                break;
                            }
                            this.RemovePetStatus();
                            roomUser.PetData.AddExperience(12);
                            roomUser.Statusses.Add("ded", "");
                            roomUser.UpdateNeeded = true;

                            this.ActionTimer = 25;
                            this.EnergyTimer = 10;
                            break;

                case "FUTBOL":
                case "FOOTBALL":
                case "SOCCER":
                case "FÚTBOL":
                            if (!roomUser.PetData.HasCommand(5))
                            {
                                Unknown = true;
                                break;
                            }
                            if (roomUser.PetData.Energy < 20 || roomUser.PetData.Nutrition < 25)
                            {
                                Lazy = true;
                                break;
                            }
                            this.RemovePetStatus();
                            var Footballs = base.GetRoom().GetRoomItemHandler().mFloorItems.Values.Where(x => x.GetBaseItem().InteractionType == Items.InteractionType.football);
                            if (Footballs.Count() < 1)
                            {
                                Lazy = true;
                                break;
                            }
                            else
                            {
                                Items.RoomItem Item = Footballs.FirstOrDefault();
                                this.ActionTimer = 50;
                                this.EnergyTimer = 30;
                                roomUser.MoveTo(Item.GetX, Item.GetY);
                                roomUser.PetData.AddExperience(35);
                            }
                            SubtractAttributes();
                            break;

                case "JUEGA":
                case "JUGAR":
                case "PLAY":
                            if (!roomUser.PetData.HasCommand(11))
                            {
                                Unknown = true;
                                break;
                            }
                            if (roomUser.PetData.Energy < 20 || roomUser.PetData.Nutrition < 25)
                            {
                                Lazy = true;
                                break;
                            }
                            this.RemovePetStatus();

                            SubtractAttributes();
                            break;

                case "QUIETO":
                case "CALLA":
                case "CALM":
                case "ESTATUA":
                case "STATUE":
                case "SHUT":
                case "SHUT UP":
                case "SILENCE":
                     this.RemovePetStatus();
                     this.ActionTimer = 650;
                     this.EnergyTimer = 20;
                     roomUser.AddStatus("wav", "");
                     roomUser.UpdateNeeded = true;
                            break;

                case "HABLA":
                case "SPEAK":
                case "TALK":
                            if (!roomUser.PetData.HasCommand(10))
                            {
                                Unknown = true;
                                break;
                            }
                            if (roomUser.PetData.Energy < 20 || roomUser.PetData.Nutrition < 25)
                            {
                                Lazy = true;
                                break;
                            }
                            this.ActionTimer = 1;
                            this.EnergyTimer = 10;
                            roomUser.Statusses.Add("gst", "sml");
                            roomUser.UpdateNeeded = true;
                            roomUser.PetData.AddExperience(35);
                            SubtractAttributes();
                            break;

                case "DORMIR":
                case "DUERME":
                case "A CASA":
                case "TO NEST":
                case "A DORMIR":
                    this.RemovePetStatus();
                            
                    var PetNest = base.GetRoom().GetRoomItemHandler().mFloorItems.Values.Where(x => x.GetBaseItem().InteractionType == Items.InteractionType.petnest);
                    if (PetNest.Count() < 1)
                    {
                        Lazy = true;
                        break;
                    }
                    else
                    {
                        Items.RoomItem Item = PetNest.FirstOrDefault();
                        roomUser.MoveTo(Item.GetX, Item.GetY);
                        roomUser.PetData.AddExperience(40);
                        int RndmEnergy = new Random().Next(25, 51);
                        if (roomUser.PetData.Energy < (Pet.MaxEnergy - RndmEnergy))
                        {
                            roomUser.PetData.Energy += RndmEnergy;
                        }
                        roomUser.PetData.Nutrition += 15;
                        roomUser.AddStatus("lay", "");
                        roomUser.AddStatus("gst", "eyb");
                        roomUser.UpdateNeeded = true;
                        Sleeping = true;
                        this.ActionTimer = 500;
                        this.EnergyTimer = 500;
                    }
                    break;

                default:
                    Lazy = true;
                    SubtractAttributes();
                    break;
            }
        }
            catch (Exception)
            {
                Lazy = true;
                SubtractAttributes();
            }

            if (Sleeping)
            {
                string[] value = PetLocale.GetValue("tired");
                string message = value[new Random().Next(0, checked(value.Length - 1))];
                roomUser.Chat(null, message, false, 0, 0);

            }
            else if (Unknown)
            {
                string[] value = PetLocale.GetValue("pet.unknowncommand");
                string message = value[new Random().Next(0, checked(value.Length - 1))];
                roomUser.Chat(null, message, false, 0, 0);
            }
            else if (Lazy)
            {
                string[] value = PetLocale.GetValue("pet.lazy");
                string message = value[new Random().Next(0, checked(value.Length - 1))];
                roomUser.Chat(null, message, false, 0, 0);
            }
            else
            {
                string[] value = PetLocale.GetValue("pet.done");
                string message = value[new Random().Next(0, checked(value.Length - 1))];
                roomUser.Chat(null, message, false, 0, 0);
            }
		}

        private void SubtractAttributes()
        {
            RoomUser roomUser = base.GetRoomUser();

            if (roomUser.PetData.Energy < 11)
            {
                roomUser.PetData.Energy = 0;
            }
            else
            {
                roomUser.PetData.Energy -= 10;
            }
            if (roomUser.PetData.Nutrition < 6)
            {
                roomUser.PetData.Nutrition = 0;
            }
            else
            {
                roomUser.PetData.Nutrition -= 5;
            }
        }

		internal override void OnUserShout(RoomUser User, string Message)
		{
		}

		internal override void OnTimerTick()
		{
			checked
			{
				if (this.SpeechTimer <= 0)
				{
					RoomUser roomUser = base.GetRoomUser();
					if (roomUser.PetData.DBState != DatabaseUpdateState.NeedsInsert)
					{
						roomUser.PetData.DBState = DatabaseUpdateState.NeedsUpdate;
					}
					if (roomUser != null)
					{
						Random random = new Random();
						this.RemovePetStatus();
						string[] value = PetLocale.GetValue("speech.pet" + roomUser.PetData.Type);
						string text = value[random.Next(0, value.Length - 1)];
						if (text.Length != 3)
						{
							roomUser.Chat(null, text, false, 0, 0);
						}
						else
						{
							roomUser.Statusses.Add(text, TextHandling.GetString(roomUser.Z));
						}
					}
					this.SpeechTimer = CyberEnvironment.GetRandomNumber(20, 120);
				}
				else
				{
					this.SpeechTimer--;
				}
				if (this.ActionTimer <= 0)
				{
					try
					{
						this.RemovePetStatus();
						this.ActionTimer = CyberEnvironment.GetRandomNumber(15, 40 + base.GetRoomUser().PetData.VirtualId);
						if (!base.GetRoomUser().RidingHorse)
						{
							if (base.GetRoomUser().PetData.Type != 16u)
							{
								Point randomWalkableSquare = base.GetRoom().GetGameMap().getRandomWalkableSquare();
								base.GetRoomUser().MoveTo(randomWalkableSquare.X, randomWalkableSquare.Y);
							}
						}

                        if (new Random().Next(2, 15) % 2 == 0)
                        {
                            if (base.GetRoomUser().PetData.Type == 16)
                            {
                                MoplaBreed breed = base.GetRoomUser().PetData.MoplaBreed;
                                base.GetRoomUser().PetData.Energy--;
                                base.GetRoomUser().AddStatus("gst", (breed.LiveState == MoplaState.DEAD) ? "sad" : "sml");
                                base.GetRoomUser().PetData.MoplaBreed.OnTimerTick(base.GetRoomUser().PetData.LastHealth, base.GetRoomUser().PetData.UntilGrown);
                            }
                            else
                            {
                                if (base.GetRoomUser().PetData.Energy < 30)
                                {
                                    base.GetRoomUser().AddStatus("lay", "");
                                }
                                else
                                {
                                    base.GetRoomUser().AddStatus("gst", "joy");
                                    if (new Random().Next(1, 7) == 3)
                                    {
                                        base.GetRoomUser().AddStatus("snf", "");
                                    }
                                }
                            }

                            base.GetRoomUser().UpdateNeeded = true;
                        }
                        
						goto IL_1B5;
					}
					catch (Exception pException)
					{
						Logging.HandleException(pException, "PetBot.OnTimerTick");
						goto IL_1B5;
					}
				}
				this.ActionTimer--;
				IL_1B5:
				if (this.EnergyTimer <= 0)
				{
					this.RemovePetStatus();
					RoomUser roomUser2 = base.GetRoomUser();
					roomUser2.PetData.PetEnergy(true);
					this.EnergyTimer = CyberEnvironment.GetRandomNumber(30, 120);
					return;
				}
				this.EnergyTimer--;
			}
		}
	}
}
