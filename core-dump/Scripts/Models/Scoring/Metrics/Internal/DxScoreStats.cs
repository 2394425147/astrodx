using System.Linq;

namespace AstroDX.Models.Scoring.Metrics.Internal
{
	public sealed class DxScoreStats
	{
		private readonly Statistics _statistics;

		private const int CriticalWeight = 3;

		private static readonly int[] Weights =
		{
			CriticalWeight, 2, 1, 0, 0
		};

		public DxScoreStats(Statistics statistics) => _statistics = statistics;

		public uint TotalDxScore => _statistics.JudgementStats.MaxNoteCount * CriticalWeight;

		/// <summary>
		///     This is used to generate max health or max dx score.
		/// </summary>
		public uint MaxAchievableDxScore => _statistics.JudgementStats.JudgedNoteCount * CriticalWeight;

		/// <summary>
		///     This is used to generate the current health or dx score.
		/// </summary>
		public uint DxScore =>
			(uint)(_statistics.JudgementStats.TapRecord.Judged.Sum(c => Weights[(int)c.grade])   +
			       _statistics.JudgementStats.TouchRecord.Judged.Sum(c => Weights[(int)c.grade]) +
			       _statistics.JudgementStats.HoldRecord.Judged.Sum(c => Weights[(int)c.grade])  +
			       _statistics.JudgementStats.SlideRecord.Judged.Sum(c => Weights[(int)c.grade]) +
			       _statistics.JudgementStats.BreakRecord.Judged.Sum(c => Weights[(int)c.grade]));

		public int GetGrade()
		{
			var dxScorePercentage = (float)DxScore / TotalDxScore;
			
			// // add 1 for every passed threshold
			for (var i = 0; i < DxGradeThresholds.Thresholds.Length; i++)
			{
				if (dxScorePercentage < DxGradeThresholds.Thresholds[i])
					return i;
			}
			
			return DxGradeThresholds.Thresholds.Length;
		}
	}
}