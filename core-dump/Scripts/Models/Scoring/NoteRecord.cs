using System;
using System.Collections.Generic;

namespace AstroDX.Models.Scoring
{
	public class NoteRecord
	{
		public IReadOnlyList<JudgeData> Judged { get; }

		public uint   MaxCount      { get; }
		public uint   PassedCount   { get; private set; }
		public uint   CriticalCount { get; private set; }
		public uint   PerfectCount  { get; private set; }
		public uint   GreatCount    { get; private set; }
		public uint   GoodCount     { get; private set; }
		public uint   MissCount     { get; private set; }
		public double Extras        { get; set; }

		public NoteRecord(uint maxCount)
		{
			Judged      = new List<JudgeData>();
			MaxCount    = maxCount;
			PassedCount = 0;
			Extras      = 0;
		}

		public void Push(in JudgeData data)
		{
			((List<JudgeData>)Judged).Add(data);

			PassedCount++;
			switch (data.grade)
			{
				case JudgeGrade.CriticalPerfect:
					CriticalCount++;
					break;
				case JudgeGrade.Perfect:
					PerfectCount++;
					break;
				case JudgeGrade.Great:
					GreatCount++;
					break;
				case JudgeGrade.Good:
					GoodCount++;
					break;
				case JudgeGrade.Miss:
					MissCount++;
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			Points += GetScore(data.grade);
		}

		public double Points { get; private set; }

		private static double GetScore(JudgeGrade grade)
		{
			return grade switch
			{
				JudgeGrade.CriticalPerfect => 1,
				JudgeGrade.Perfect         => 1,
				JudgeGrade.Great           => 0.8,
				JudgeGrade.Good            => 0.5,
				JudgeGrade.Miss            => 0,
				_ => throw new
					     ArgumentOutOfRangeException()
			};
		}
	}
}
