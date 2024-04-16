using AstroDX.Utilities.Extensions;
using SimaiSharp.Structures;

namespace AstroDX.Models.Scoring.Metrics.Internal
{
	public sealed class FluctuationStats : IReactiveStatistic
	{
		public long   EarlyCount     { get; private set; }
		public long   LateCount      { get; private set; }
		public double TotalDeviation { get; private set; }
		public long NotesCounted { get; private set; }
		public double Deviation => TotalDeviation / NotesCounted;

		public void Push(in NoteType type, in JudgeData data)
		{
			if ((GameSettings.Settings.Profile.Metrics.fluctuationVisibility & data.grade.AsFlag()) == 0)
				return;

			if (type == NoteType.Slide)
				return;

			NotesCounted++;

			// ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
			switch (data.state)
			{
				case JudgeState.Early:
					EarlyCount++;
					break;
				case JudgeState.Late:
					LateCount++;
					break;
			}

			TotalDeviation += data.offset;
		}
	}
}
