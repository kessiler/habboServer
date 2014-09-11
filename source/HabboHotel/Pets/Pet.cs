using Cyber.HabboHotel.Rooms;
using Cyber.Messages;
using Cyber.Messages.Headers;
using System.Collections.Generic;
using System;
namespace Cyber.HabboHotel.Pets
{
	internal class Pet
	{
		internal uint PetId;
		internal uint OwnerId;
		internal int VirtualId;
		internal uint Type;
		internal string Name;
		internal string Race;
		internal string Color;
		internal int HairDye;
		internal int PetHair;
		internal int Experience;
		internal int Energy;
		internal int Nutrition;
		internal uint RoomId;
		internal int X;
		internal int Y;
		internal double Z;
		internal int Respect;
		internal int Rarity;
		internal double CreationStamp;
		internal bool PlacedInRoom;
		internal DateTime LastHealth;
		internal DateTime UntilGrown;
		internal MoplaBreed MoplaBreed;
        internal Dictionary<short, bool> PetCommands;

		internal int[] experienceLevels = new int[]
		{
			100,
			200,
			400,
			600,
			1000,
			1300,
			1800,
			2400,
			3200,
			4300,
			7200,
			8500,
			10100,
			13300,
			17500,
			23000,
			51900
		};
		internal DatabaseUpdateState DBState;
		internal bool HaveSaddle;
		internal int AnyoneCanRide;
		internal Room Room
		{
			get
			{
				if (!this.IsInRoom)
				{
					return null;
				}
				return CyberEnvironment.GetGame().GetRoomManager().GetRoom(this.RoomId);
			}
		}
		internal bool IsInRoom
		{
			get
			{
				return this.RoomId > 0u;
			}
		}
		internal int Level
		{
			get
			{
				checked
				{
					for (int i = 0; i < this.experienceLevels.Length; i++)
					{
						if (this.Experience < this.experienceLevels[i])
						{
							return i + 1;
						}
					}
					return this.experienceLevels.Length + 1;
				}
			}
		}
		internal static int MaxLevel
		{
			get
			{
				return 20;
			}
		}
		internal int experienceGoal
		{
			get
			{
				return this.experienceLevels[checked(this.Level - 1)];
			}
		}
		internal static int MaxEnergy
		{
			get
			{
				return 100;
			}
		}
		internal static int MaxNutrition
		{
			get
			{
				return 150;
			}
		}
		internal int Age
		{
            get
            {
                DateTime Creation = CyberEnvironment.UnixToDateTime(this.CreationStamp);
                return (int)(DateTime.Now - Creation).TotalDays;
            }
		}
		internal string Look
		{
			get
			{
				return string.Concat(new object[]
				{
					this.Type,
					" ",
					this.Race,
					" ",
					this.Color
				});
			}
		}
		internal string OwnerName
		{
			get
			{
				return CyberEnvironment.GetGame().GetClientManager().GetNameById(this.OwnerId);
			}
		}
		internal Pet(uint PetId, uint OwnerId, uint RoomId, string Name, uint Type, string Race, string Color, int experience, int Energy, int Nutrition, int Respect, double CreationStamp, int X, int Y, double Z, bool havesaddle, int Anyonecanride, int Dye, int PetHer, int Rarity, DateTime LastHealth, DateTime UntilGrown, MoplaBreed MoplaBreed)
		{
			this.PetId = PetId;
			this.OwnerId = OwnerId;
			this.RoomId = RoomId;
			this.Name = Name;
			this.Type = Type;
			this.Race = Race;
			this.Color = Color;
			this.Experience = experience;
			this.Energy = Energy;
			this.Nutrition = Nutrition;
			this.Respect = Respect;
			this.CreationStamp = CreationStamp;
			this.X = X;
			this.Y = Y;
			this.Z = Z;
			this.PlacedInRoom = false;
			this.DBState = DatabaseUpdateState.Updated;
			this.HaveSaddle = havesaddle;
			this.AnyoneCanRide = Anyonecanride;
			this.PetHair = PetHer;
			this.HairDye = Dye;
			this.Rarity = Rarity;
			this.LastHealth = LastHealth;
			this.UntilGrown = UntilGrown;
			this.MoplaBreed = MoplaBreed;
            this.PetCommands = PetCommandHandler.GetPetCommands(this);
		}

        internal bool HasCommand(short Command)
        {
            if (!PetCommands.ContainsKey(Command))
            {
                return false;
            }
            return PetCommands[Command];
        }

		internal void OnRespect()
		{
			checked
			{
				this.Respect++;
				ServerMessage serverMessage = new ServerMessage(Outgoing.RespectPetMessageComposer);
				serverMessage.AppendInt32(this.VirtualId);
				serverMessage.AppendInt32(0);
				this.Room.SendMessage(serverMessage);
				if (this.DBState != DatabaseUpdateState.NeedsInsert)
				{
					this.DBState = DatabaseUpdateState.NeedsUpdate;
				}
				if (this.Experience <= 51900)
				{
					this.AddExperience(10);
				}
                if (Type == 16)
                {
                    Energy = 100;
                }
				this.LastHealth = DateTime.Now.AddSeconds(129600.0);
			}
		}

		internal void AddExperience(int Amount)
		{
			checked
			{
				this.Experience += Amount;
				if (this.Experience >= 51900)
				{
					return;
				}
				if (this.DBState != DatabaseUpdateState.NeedsInsert)
				{
					this.DBState = DatabaseUpdateState.NeedsUpdate;
				}
				if (this.Room != null)
				{
					ServerMessage serverMessage = new ServerMessage(Outgoing.AddPetExperienceMessageComposer);
					serverMessage.AppendUInt(this.PetId);
					serverMessage.AppendInt32(this.VirtualId);
					serverMessage.AppendInt32(Amount);
					this.Room.SendMessage(serverMessage);
					if (this.Experience > this.experienceGoal)
					{
                        GameClients.GameClient OwnerSession = CyberEnvironment.GetGame().GetClientManager().GetClientByUserID(OwnerId);

                        if (OwnerSession != null)
                        {
                            ServerMessage LevelNotify = new ServerMessage(Outgoing.NotifyNewPetLevelMessageComposer);
                            LevelNotify.AppendUInt(PetId);
                            LevelNotify.AppendString(Name);
                            LevelNotify.AppendInt32(Level);
                            OwnerSession.SendMessage(LevelNotify);
                        }

                        // Reset pet commands
                        PetCommands.Clear();
                        PetCommands = PetCommandHandler.GetPetCommands(this);
					}
				}
			}
		}
		internal void PetEnergy(bool Add)
		{
			checked
			{
				int num;
				if (Add)
				{
					if (this.Energy == 100)
					{
						return;
					}
					if (this.Energy > 85)
					{
						num = Pet.MaxEnergy - this.Energy;
					}
					else
					{
						num = 10;
					}
				}
				else
				{
					num = 15;
				}
				if (num <= 4)
				{
					num = 15;
				}
				int randomNumber = CyberEnvironment.GetRandomNumber(4, num);
				if (!Add)
				{
					this.Energy -= randomNumber;
					if (this.Energy < 0)
					{
						this.Energy = 1;
					}
				}
				else
				{
					this.Energy += randomNumber;
				}
				if (this.DBState != DatabaseUpdateState.NeedsInsert)
				{
					this.DBState = DatabaseUpdateState.NeedsUpdate;
				}
			}
		}
		internal void SerializeInventory(ServerMessage Message)
		{
			Message.AppendUInt(this.PetId);
			Message.AppendString(this.Name);
			Message.AppendUInt(this.Type);
			Message.AppendInt32(int.Parse(this.Race));
			Message.AppendString((this.Type == 16u) ? "ffffff" : this.Color);
			Message.AppendUInt((this.Type == 16u) ? 0u : this.Type);
			if (this.Type == 16u && this.MoplaBreed != null)
			{
				string[] array = this.MoplaBreed.PlantData.Substring(12).Split(new char[]
				{
					' '
				});
				string[] array2 = array;
				for (int i = 0; i < array2.Length; i++)
				{
					string s = array2[i];
					Message.AppendInt32(int.Parse(s));
				}
				Message.AppendInt32(this.MoplaBreed.GrowingStatus);
				return;
			}
			Message.AppendInt32(0);
			Message.AppendInt32(0);
		}

        internal void ManageGestures()
        {

        }

		internal ServerMessage SerializeInfo()
		{
			ServerMessage serverMessage = new ServerMessage(Outgoing.PetInfoMessageComposer);
			serverMessage.AppendUInt(this.PetId);
			serverMessage.AppendString(this.Name);
            if (this.Type == 16)
            {
                serverMessage.AppendInt32(this.MoplaBreed.GrowingStatus);
                serverMessage.AppendInt32(7);
            }
            else
            {
                serverMessage.AppendInt32(this.Level);
                serverMessage.AppendInt32(Pet.MaxLevel);
            }
			serverMessage.AppendInt32(this.Experience);
			serverMessage.AppendInt32(this.experienceGoal);
			serverMessage.AppendInt32(this.Energy);
			serverMessage.AppendInt32(Pet.MaxEnergy);
			serverMessage.AppendInt32(this.Nutrition);
			serverMessage.AppendInt32(Pet.MaxNutrition);
			serverMessage.AppendInt32(this.Respect);
			serverMessage.AppendUInt(this.OwnerId);
			serverMessage.AppendInt32(this.Age);
			serverMessage.AppendString(this.OwnerName);
            serverMessage.AppendInt32((Type == 16) ? 0 : 1);
			serverMessage.AppendBoolean(this.HaveSaddle);
			serverMessage.AppendBoolean(CyberEnvironment.GetGame().GetRoomManager().GetRoom(this.RoomId).GetRoomUserManager().GetRoomUserByVirtualId(this.VirtualId).RidingHorse);
			serverMessage.AppendInt32(0);
			serverMessage.AppendInt32(this.AnyoneCanRide);
            if (Type == 16)
            {
                if (MoplaBreed.LiveState == MoplaState.GROWN)
                {
                    serverMessage.AppendBoolean(true);
                }
                else
                {
                    serverMessage.AppendBoolean(false);
                }
            }
            else
            {
                serverMessage.AppendBoolean(false); // Plant is grown
            }
			serverMessage.AppendBoolean(false);
            if (Type == 16)
            {
                if (MoplaBreed.LiveState == MoplaState.DEAD)
                {
                    serverMessage.AppendBoolean(true);
                }
                else
                {
                    serverMessage.AppendBoolean(false);
                }
            }
            else
            {
                serverMessage.AppendBoolean(false);
            }
			serverMessage.AppendInt32(this.Rarity);
			checked
			{
				if (this.Type == 16u)
				{
                    serverMessage.AppendInt32(129600);
					if (this.MoplaBreed.LiveState == MoplaState.DEAD)
					{
						serverMessage.AppendInt32(0);
						serverMessage.AppendInt32(0);
					}
                    else if(this.MoplaBreed.LiveState == MoplaState.GROWN)
                    {
                        serverMessage.AppendInt32((int)(this.LastHealth - DateTime.Now).TotalSeconds);
                        serverMessage.AppendInt32(0);
                    }
					else
					{
                        serverMessage.AppendInt32((int)(this.LastHealth - DateTime.Now).TotalSeconds);
                        serverMessage.AppendInt32((int)(this.UntilGrown - DateTime.Now).TotalSeconds);
					}
				}
				else
				{
					serverMessage.AppendInt32(-1);
					serverMessage.AppendInt32(-1);
					serverMessage.AppendInt32(-1);
				}

				serverMessage.AppendBoolean(false); // Allow breed?
				return serverMessage;
			}
		}
	}
}
