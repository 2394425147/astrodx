using System;

namespace AstroDX.Models.Scoring.Metrics.Internal
{
	public sealed class AchievementStats
	{
		private readonly Statistics _statistics;
		private readonly double     _baseScore;

		public AchievementStats(Statistics statistics)
		{
			_statistics = statistics;

			var totalPointCount = statistics.JudgementStats.MaxTapCount       +
			                      statistics.JudgementStats.MaxTouchCount     +
			                      statistics.JudgementStats.MaxHoldCount  * 2 +
			                      statistics.JudgementStats.MaxSlideCount * 3 +
			                      statistics.JudgementStats.MaxBreakCount * 5;

			_baseScore = totalPointCount > 0 ? 100.00 / totalPointCount : 0;
		}

		/// <param name="precise">Used when displaying achievement as text to avoid double rounding</param>
		public double GetAchievement(bool precise = false)
		{
			var baseAchievement = GetBaseAchievement();
			var extraAchievement = _statistics.JudgementStats.BreakRecord.MaxCount > 0
				                       ? _statistics.JudgementStats.BreakRecord.Extras /
				                         _statistics.JudgementStats.BreakRecord.MaxCount
				                       : 0;

			var totalAchievement = baseAchievement +
			                       extraAchievement;

			return precise ? totalAchievement : Math.Round(totalAchievement, 4);
		}

		private double GetBaseAchievement()
		{
			if (_baseScore == 0)
				return 0;

			var tapAchievement = _statistics.JudgementStats.TapRecord.Points
			                     * _baseScore;

			var touchAchievement = _statistics.JudgementStats.TouchRecord.Points
			                       * _baseScore;

			var holdAchievement = _statistics.JudgementStats.HoldRecord.Points
			                      * _baseScore * 2;

			var slideAchievement = _statistics.JudgementStats.SlideRecord.Points
			                       * _baseScore * 3;

			var breakAchievement = _statistics.JudgementStats.BreakRecord.Points
			                       * _baseScore * 5;

			return tapAchievement + touchAchievement + holdAchievement + slideAchievement + breakAchievement;
		}

		public double CalculateMaxAchievable(bool includeExtras)
		{
			var tapAchievement   = _statistics.JudgementStats.TapRecord.PassedCount   * _baseScore;
			var touchAchievement = _statistics.JudgementStats.TouchRecord.PassedCount * _baseScore;
			var holdAchievement  = _statistics.JudgementStats.HoldRecord.PassedCount  * _baseScore * 2;
			var slideAchievement = _statistics.JudgementStats.SlideRecord.PassedCount * _baseScore * 3;
			var breakAchievement = _statistics.JudgementStats.BreakRecord.PassedCount * _baseScore * 5;

			return includeExtras && _statistics.JudgementStats.BreakRecord.MaxCount > 0
				       ? tapAchievement + touchAchievement + holdAchievement + slideAchievement + breakAchievement +
				         (double)_statistics.JudgementStats.BreakRecord.PassedCount /
				         _statistics.JudgementStats.BreakRecord.MaxCount
				       : tapAchievement + touchAchievement + holdAchievement + slideAchievement + breakAchievement;
		}

		public ClearType EvaluateClearType(bool whilePlaying = false)
		{
			var achievement = GetAchievement();

			var cleared  = achievement >= 80 && _statistics.JudgementStats.AllNotesPassed;
			var missed   = _statistics.JudgementStats.JudgedMissCount  >= 1;
			var anyGood  = _statistics.JudgementStats.JudgedGoodCount  >= 1;
			var anyGreat = _statistics.JudgementStats.JudgedGreatCount >= 1;

			var chartHasBreaks = _statistics.JudgementStats.MaxBreakCount > 0;

			if (!whilePlaying && !cleared)
				return ClearType.Failed;

			if (missed)
				return ClearType.Clear;

			if (anyGood)
				return ClearType.FullCombo;

			if (anyGreat)
				return ClearType.FullComboPlus;

			if (!chartHasBreaks)
				return ClearType.AllPerfect;

			var maxBreak = _statistics.JudgementStats.BreakRecord.CriticalCount ==
			               _statistics.JudgementStats.BreakRecord.PassedCount;

			return maxBreak
				       ? ClearType.AllPerfectPlus
				       : ClearType.AllPerfect;
		}
	}
}
