using Cyber.HabboHotel.Rooms;
using System;
using System.Collections.Generic;
namespace Cyber.HabboHotel.RoomBots
{
	internal class RoomBot
	{
		internal uint BotId;
		internal uint RoomId;
		internal int VirtualId;
		internal uint OwnerId;
		internal AIType AiType;
		internal string WalkingMode;
		internal string Name;
		internal string Motto;
		internal string Look;
		internal string Gender;
		internal int X;
		internal int Y;
		internal double Z;
		internal int Rot;
		internal int minX;
		internal int maxX;
		internal int minY;
		internal int maxY;
		internal int DanceId;
		internal RoomUser RoomUser;
		internal int LastSpokenPhrase;
        internal bool WasPicked;
		internal bool IsBartender;
		internal List<RandomSpeech> RandomSpeech;
		internal List<BotResponse> Responses;
		internal bool IsPet
		{
			get
			{
				return this.AiType == AIType.Pet;
			}
		}
		internal RoomBot(uint BotId, uint OwnerId, uint RoomId, AIType AiType, string WalkingMode, string Name, string Motto, string Look, int X, int Y, double Z, int Rot, int minX, int minY, int maxX, int maxY, ref List<RandomSpeech> Speeches, ref List<BotResponse> Responses, string Gender, int Dance, bool Bartender)
		{
			this.OwnerId = OwnerId;
			this.BotId = BotId;
			this.RoomId = RoomId;
			this.AiType = AiType;
			this.WalkingMode = WalkingMode;
			this.Name = Name;
			this.Motto = Motto;
			this.Look = Look;
			this.X = X;
			this.Y = Y;
			this.Z = Z;
			this.Rot = Rot;
			this.minX = minX;
			this.minY = minY;
			this.maxX = maxX;
			this.maxY = maxY;
			this.Gender = Gender.ToUpper();
			this.VirtualId = -1;
			this.RoomUser = null;
			this.DanceId = Dance;
			this.RandomSpeech = Speeches;
			this.Responses = Responses;
			this.LastSpokenPhrase = 1;
			this.IsBartender = Bartender;
            this.WasPicked = (RoomId == 0);
		}
		internal void LoadRandomSpeech(List<RandomSpeech> Speeches)
		{
			this.RandomSpeech = Speeches;
		}
		internal void LoadResponses(List<BotResponse> Response)
		{
			this.Responses = Response;
		}
		internal BotResponse GetResponse(string Message)
		{
			foreach (BotResponse current in this.Responses)
			{
				if (current.KeywordMatched(Message))
				{
					return current;
				}
			}
			return null;
		}
		internal RandomSpeech GetRandomSpeech(bool MixPhrases)
		{
			if (this.RandomSpeech.Count < 1)
			{
				return new RandomSpeech("", false);
			}
			checked
			{
				if (MixPhrases)
				{
					return this.RandomSpeech[CyberEnvironment.GetRandomNumber(0, this.RandomSpeech.Count)];
				}
				RandomSpeech result = new RandomSpeech("", false);
				if (this.LastSpokenPhrase >= this.RandomSpeech.Count)
				{
					this.LastSpokenPhrase = 1;
				}
				result = this.RandomSpeech[this.LastSpokenPhrase - 1];
				this.LastSpokenPhrase++;
				return result;
			}
		}
		internal BotAI GenerateBotAI(int VirtualId, int botId)
		{
			AIType aiType = this.AiType;
			if (aiType == AIType.Pet)
			{
				return new PetBot(VirtualId);
			}
			return new GenericBot(VirtualId, botId, this.AiType, this.IsBartender);
		}
	}
}
