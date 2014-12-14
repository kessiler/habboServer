using Database_Manager.Database.Session_Details.Interfaces;
using System;
using System.Data;

namespace Cyber.HabboHotel.Pets
{

    internal class MoplaBreed
    {
        private string BreedData;
        private Pet Pet;
        private bool DBUpdateNeeded;
        internal int GrowingStatus;
        internal MoplaState LiveState;
        private string MoplaName;
        private readonly uint PetId;
        private readonly int Rarity;

        internal MoplaBreed(DataRow Row)
        {
            this.PetId = uint.Parse(Row["pet_id"].ToString());
            this.Rarity = int.Parse(Row["rarity"].ToString());
            this.MoplaName = Row["plant_name"].ToString();
            this.BreedData = Row["plant_data"].ToString();
            this.LiveState = (MoplaState)int.Parse(Row["plant_state"].ToString());
            this.GrowingStatus = int.Parse(Row["growing_status"].ToString());
        }

        internal MoplaBreed(Pet Pet, uint PetId, int Rarity, string MoplaName, string BreedData, int LiveState, int GrowingStatus)
        {
            this.Pet = Pet;
            this.PetId = PetId;
            this.Rarity = Rarity;
            this.MoplaName = MoplaName;
            this.BreedData = BreedData;
            this.LiveState = (MoplaState)LiveState;
            this.GrowingStatus = GrowingStatus;
        }

        internal static MoplaBreed CreateMonsterplantBreed(Pet Pet)
        {
            MoplaBreed breed = null;
            if (Pet.Type == 16)
            {
                Tuple<string, string> tuple = GeneratePlantData(Pet.Rarity);
                breed = new MoplaBreed(Pet, Pet.PetId, Pet.Rarity, tuple.Item1, tuple.Item2, 0, 1);
                using (IQueryAdapter adapter = CyberEnvironment.GetDatabaseManager().getQueryReactor())
                {
                    adapter.setQuery("INSERT INTO bots_monsterplants (pet_id, rarity, plant_name, plant_data) VALUES (@petid , @rarity , @plantname , @plantdata)");
                    adapter.addParameter("petid", Pet.PetId);
                    adapter.addParameter("rarity", Pet.Rarity);
                    adapter.addParameter("plantname", tuple.Item1);
                    adapter.addParameter("plantdata", tuple.Item2);
                    adapter.runQuery();
                }
            }
            return breed;
        }

        internal static Tuple<string, string> GeneratePlantData(int Rarity)
        {
            string str = "";
            int num = 0;
            int num2 = 1;
            Random random = new Random();
            switch (Rarity)
            {
                case 1:
                    if ((random.Next(0, 4) % 2) != 0)
                    {
                        if (random.Next(0, 2) == 0)
                        {
                            num = 0;
                            str = str + "Aenueus ";
                        }
                        else
                        {
                            num = 3;
                            str = str + "Viridulus ";
                        }
                    }
                    else
                    {
                        num = 9;
                        str = str + "Fulvus ";
                    }
                    if (random.Next(0, 2) == 1)
                    {
                        num2 = 1;
                        str = str + "Blungon";
                    }
                    else
                    {
                        num2 = 3;
                        str = str + "Stumpy";
                    }
                    break;

                case 2:
                    if ((random.Next(0, 4) % 2) != 0)
                    {
                        if (random.Next(0, 2) == 0)
                        {
                            num = 5;
                            str = str + "Incarnatus ";
                        }
                        else
                        {
                            num = 2;
                            str = str + "Phoenicus ";
                        }
                    }
                    else
                    {
                        num = 1;
                        str = str + "Griseus ";
                    }
                    if (random.Next(0, 2) == 1)
                    {
                        num2 = 3;
                        str = str + "Stumpy";
                    }
                    else
                    {
                        num2 = 2;
                        str = str + "Wailzor";
                    }
                    break;

                case 3:
                    if ((random.Next(0, 4) % 2) != 0)
                    {
                        if (random.Next(0, 7) == 5)
                        {
                            num = 1;
                            str = str + "Griseus ";
                        }
                        else if (random.Next(0, 2) == 0)
                        {
                            num = 10;
                            str = str + "Cinereus ";
                        }
                        else
                        {
                            num = 8;
                            str = str + "Amethyst ";
                        }
                    }
                    else
                    {
                        num = 2;
                        str = str + "Phoenicus ";
                    }
                    if (random.Next(0, 2) == 1)
                    {
                        num2 = 2;
                        str = str + "Wailzor";
                    }
                    else if ((random.Next(0, 5) % 2) == 0)
                    {
                        num2 = 6;
                        str = str + "Shroomer";
                    }
                    else
                    {
                        num2 = 9;
                        str = str + "Weggytum";
                    }
                    break;

                case 4:
                    if ((random.Next(0, 4) % 2) != 0)
                    {
                        if (random.Next(0, 7) == 5)
                        {
                            num = 8;
                            str = str + "Amethyst ";
                        }
                        else if (random.Next(0, 2) == 0)
                        {
                            num = 6;
                            str = str + "Azureus ";
                        }
                        else if (random.Next(0, 5) == 4)
                        {
                            num = 10;
                            str = str + "Cinereus ";
                        }
                        else if (random.Next(0, 7) % 2 != 0)
                        {
                            num = 8;
                            str = str + "Amethyst ";
                        }
                        else
                        {
                            num = 7;
                            str = str + "Amatasc ";
                        }
                    }
                    else
                    {
                        num = 5;
                        str = str + "Incarnatus ";
                    }
                    if (random.Next(0, 2) == 1)
                    {
                        num2 = 7;
                        str = str + "Zuchinu";
                    }
                    else if ((random.Next(0, 5) % 2) == 0)
                    {
                        num2 = 6;
                        str = str + "Shroomer";
                    }
                    else
                    {
                        num2 = 4;
                        str = str + "Sunspike";
                    }
                    break;

                case 5:
                    if ((random.Next(0, 4) % 2) != 0)
                    {
                        if (random.Next(0, 7) == 5)
                        {
                            num = 4;
                            str = str + "Cyaneus ";
                        }
                        else if (random.Next(0, 2) == 0)
                        {
                            num = 6;
                            str = str + "Azureus ";
                        }
                        else
                        {
                            num = 7;
                            str = str + "Amatasc ";
                        }
                    }
                    else
                    {
                        num = 3;
                        str = str + "Viridulus ";
                    }
                    if (random.Next(0, 2) == 1)
                    {
                        num2 = 7;
                        str = str + "Zuchinu";
                    }
                    else if ((random.Next(0, 5) % 2) == 2)
                    {
                        num2 = 11;
                        str = str + "Hairbullis";
                    }
                    else
                    {
                        num2 = 9;
                        str = str + "Weggytum";
                    }
                    break;

                case 6:
                    if ((random.Next(0, 4) % 2) != 0)
                    {
                        if (random.Next(0, 7) == 5)
                        {
                            num = 8;
                            str = str + "Amethyst ";
                        }
                        else if (random.Next(0, 2) == 0)
                        {
                            num = 7;
                            str = str + "Atamasc ";
                        }
                        else
                        {
                            num = 2;
                            str = str + "Phoenicus ";
                        }
                    }
                    else
                    {
                        num = 6;
                        str = str + "Azureus ";
                    }
                    if (random.Next(0, 2) == 1)
                    {
                        num2 = 10;
                        str = str + "Wystique";
                    }
                    else if ((random.Next(0, 5) % 2) == 2)
                    {
                        num2 = 11;
                        str = str + "Hairbullis";
                    }
                    else
                    {
                        num2 = 3;
                        str = str + "Stumpy";
                    }
                    break;

                case 7:
                    if ((random.Next(0, 4) % 2) != 0)
                    {
                        if (random.Next(0, 7) == 5)
                        {
                            num = 6;
                            str = str + "Azureus ";
                        }
                        else if (random.Next(0, 2) == 0)
                        {
                            num = 7;
                            str = str + "Atamasc ";
                        }
                        else
                        {
                            num = 1;
                            str = str + "Griseus ";
                        }
                    }
                    else
                    {
                        num = 4;
                        str = str + "Cyaneus ";
                    }
                    if (random.Next(0, 2) == 1)
                    {
                        num2 = 2;
                        str = str + "Wailzor";
                    }
                    else if ((random.Next(0, 5) % 2) == 2)
                    {
                        num2 = 4;
                        str = str + "Sunspike";
                    }
                    else if (random.Next(0, 3) == 2)
                    {
                        num2 = 10;
                        str = str + "Wystique";
                    }
                    else
                    {
                        num2 = 6;
                        str = str + "Shroomer";
                    }
                    break;

                case 8:
                    if ((random.Next(0, 4) % 2) != 0)
                    {
                        if (random.Next(0, 7) == 5)
                        {
                            num = 7;
                            str = str + "Atamasc ";
                        }
                        else if (random.Next(0, 2) == 0)
                        {
                            num = 10;
                            str = str + "Cinereus ";
                        }
                        else if ((random.Next(12, 0x13) % 2) == 1)
                        {
                            num = 6;
                            str = str + "Azureus ";
                        }
                        else
                        {
                            num = 8;
                            str = str + "Amethyst ";
                        }
                    }
                    else
                    {
                        num = 4;
                        str = str + "Cyaneus ";
                    }
                    if (random.Next(0, 2) == 1)
                    {
                        num2 = 11;
                        str = str + "Hairbullis";
                    }
                    else if ((random.Next(0, 5) % 2) == 2)
                    {
                        num2 = 10;
                        str = str + "Wystique";
                    }
                    else if (random.Next(0, 3) == 2)
                    {
                        num2 = 7;
                        str = str + "Zuchinu";
                    }
                    else
                    {
                        num2 = 6;
                        str = str + "Shroomer";
                    }
                    break;

                case 9:
                    if ((random.Next(0, 4) % 2) != 0)
                    {
                        if (random.Next(0, 7) == 5)
                        {
                            num = 7;
                            str = str + "Atamasc ";
                        }
                        else if (random.Next(0, 2) == 0)
                        {
                            num = 6;
                            str = str + "Azureus ";
                        }
                        else
                        {
                            num = 6;
                            str = str + "Azureus ";
                        }
                    }
                    else
                    {
                        num = 4;
                        str = str + "Cyaneus ";
                    }
                    if (random.Next(0, 2) == 1)
                    {
                        num2 = 11;
                        str = str + "Hairbullis";
                    }
                    else if ((random.Next(0, 5) % 2) == 2)
                    {
                        num2 = 10;
                        str = str + "Wystique";
                    }
                    else
                    {
                        num2 = 8;
                        str = str + "Abysswirl";
                    }
                    break;

                case 10:
                    num = 4;
                    str = str + "Cyaneus ";
                    num2 = 8;
                    str = str + "Abysswirl";
                    break;

                case 11:
                    num = 4;
                    str = str + "Cyaneus ";
                    num2 = 12;
                    str = str + "Snozzle";
                    break;

                default:
                    if ((random.Next(0, 4) % 2) == 0)
                    {
                        num = 9;
                        str = str + "Fulvus ";
                    }
                    else
                    {
                        num = 0;
                        str = str + "Aenueus ";
                    }
                    if (random.Next(0, 2) == 1)
                    {
                        num2 = 5;
                        str = str + "Squarg";
                    }
                    else
                    {
                        num2 = 1;
                        str = str + "Blungon";
                    }
                    break;
            }
            return new Tuple<string, string>(str, string.Concat(new object[] { "16 ", num, " ffffff 2 1 ", num2, " ", num, " 0 -1 7" }));
        }

        internal void KillPlant()
        {
            this.LiveState = MoplaState.DEAD;
            this.DBUpdateNeeded = true;
        }

        internal void OnTimerTick(DateTime LastHealth, DateTime UntilGrown)
        {
            if ((int)LiveState != 0)
            {
                return;
            }
            TimeSpan span = (TimeSpan)(LastHealth - DateTime.Now);
            if (span.TotalSeconds <= 0)
            {
                this.KillPlant();
            }
            else
            {
                if (this.GrowingStatus != 7)
                {
                    TimeSpan span2 = (TimeSpan)(UntilGrown - DateTime.Now);
                    if (span2.TotalSeconds <= 10 && this.GrowingStatus == 6)
                    {
                        this.GrowingStatus = 7;
                        this.LiveState = MoplaState.GROWN;
                        this.DBUpdateNeeded = true;
                    }
                    else if (span2.TotalSeconds <= 24000 && this.GrowingStatus == 5)
                    {
                        this.GrowingStatus = 6;
                        this.DBUpdateNeeded = true;
                    }
                    else if (span2.TotalSeconds <= 48000 && this.GrowingStatus == 4)
                    {
                        this.GrowingStatus = 5;
                        this.DBUpdateNeeded = true;
                    }
                    else if (span2.TotalSeconds <= 96000 && this.GrowingStatus == 3)
                    {
                        this.GrowingStatus = 4;
                        this.DBUpdateNeeded = true;
                    }
                    else if (span2.TotalSeconds <= 110000 && this.GrowingStatus == 2)
                    {
                        this.GrowingStatus = 3;
                        this.DBUpdateNeeded = true;
                    }
                    else if (span2.TotalSeconds <= 160000 && this.GrowingStatus == 1)
                    {
                        this.GrowingStatus = 2;
                        this.DBUpdateNeeded = true;
                    }

                    if (span2.TotalSeconds % 8 == 0)
                    {
                        Pet.Energy--;
                    }
                }
            }
            if (this.DBUpdateNeeded)
            {
                using (IQueryAdapter adapter = CyberEnvironment.GetDatabaseManager().getQueryReactor())
                {
                    adapter.setQuery("REPLACE INTO bots_monsterplants (pet_id, rarity, plant_name, plant_data, plant_state, growing_status) VALUES (@petid , @rarity , @plantname , @plantdata , @plantstate , @growing)");
                    adapter.addParameter("petid", this.PetId);
                    adapter.addParameter("rarity", this.Rarity);
                    adapter.addParameter("plantname", this.MoplaName);
                    adapter.addParameter("plantdata", this.BreedData);
                    adapter.addParameter("plantstate", ((int)this.LiveState).ToString());
                    adapter.addParameter("growing", this.GrowingStatus);
                    adapter.runQuery();
                }
            }
        }

        internal bool RevivePlant()
        {
            if (this.LiveState != MoplaState.DEAD)
            {
                return false;
            }
            this.LiveState = (this.GrowingStatus < 7) ? MoplaState.ALIVE : MoplaState.GROWN;
            DBUpdateNeeded = true;
            return true;
        }

        internal string GrowStatus
        {
            get
            {
                if (this.LiveState == MoplaState.DEAD)
                {
                    return "rip";
                }
                else if (this.LiveState == MoplaState.GROWN)
                {
                    return "std";
                }
                return ("grw" + this.GrowingStatus);
            }
        }

        internal string Name
        {
            get
            {
                return this.MoplaName;
            }
        }

        internal string PlantData
        {
            get
            {
                return this.BreedData;
            }
        }
    }
}

