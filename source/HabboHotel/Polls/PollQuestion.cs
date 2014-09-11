using Cyber.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Cyber.HabboHotel.Polls
{
	internal class PollQuestion
	{
		internal enum PollAnswerType
		{
			RadioSelection = 1,
			Selection = 2,
			Text = 3,
		}
		internal uint Index;
		internal string Question;
		internal PollQuestion.PollAnswerType AType;
		internal List<string> Answers = new List<string>();
		internal string CorrectAnswer;
		internal PollQuestion(uint Index, string Question, int AType, string[] Answers, string CorrectAnswer)
		{
			this.Index = Index;
			this.Question = Question;
			this.AType = (PollQuestion.PollAnswerType)AType;
			this.Answers = Answers.ToList<string>();
			this.CorrectAnswer = CorrectAnswer;
		}
		public void Serialize(ServerMessage Message, int QuestionNumber)
		{
			Message.AppendUInt(this.Index);
			Message.AppendInt32(QuestionNumber);
			Message.AppendInt32((int)this.AType);
			Message.AppendString(this.Question);
			if (this.AType == PollQuestion.PollAnswerType.Selection || this.AType == PollQuestion.PollAnswerType.RadioSelection)
			{
				Message.AppendInt32(1);
				Message.AppendInt32(this.Answers.Count);
				foreach (string current in this.Answers)
				{
					Message.AppendString(current);
					Message.AppendString(current);
				}
			}
		}
	}
}
