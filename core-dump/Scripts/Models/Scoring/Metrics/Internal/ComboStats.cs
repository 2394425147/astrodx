using System;
using SimaiSharp.Structures;

namespace AstroDX.Models.Scoring.Metrics.Internal
{
	public sealed class ComboStats : IReactiveStatistic
	{
		public long lastCombo { get; private set; }
		public long maxCombo  { get; private set; }

		public void Push(in NoteType type, in JudgeData data = default)
		{
			if (data.grade == JudgeGrade.Miss)
			{
				lastCombo = 0;
				return;
			}

			lastCombo++;
			maxCombo = Math.Max(lastCombo, maxCombo);
		}
	}
}