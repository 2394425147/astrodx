using System;
using System.Collections.Generic;
using System.Linq;
using AstroDX.Models.Scoring.Metrics.Internal;
using Sigtrap.Relays;
using SimaiSharp.Structures;

namespace AstroDX.Models.Scoring.Metrics
{
	[Serializable]
	public sealed class Statistics
	{
		public Relay<NoteType, JudgeData> OnJudgementReceived { get; } = new();

		public  FluctuationStats            FluctuationStats { get; }
		public  ComboStats                  ComboStats       { get; }
		public  JudgementStats              JudgementStats   { get; }
		public  AchievementStats            AchievementStats { get; }
		public  DxScoreStats                DxScoreStats     { get; }
		private HashSet<IReactiveStatistic> Extensions       { get; }

		public Statistics(ReadOnlyMemory<NoteCollection> chart)
		{
			JudgementStats      = new JudgementStats(chart);
			FluctuationStats    = new FluctuationStats();
			ComboStats          = new ComboStats();
			AchievementStats    = new AchievementStats(this);
			DxScoreStats        = new DxScoreStats(this);
			Extensions = new HashSet<IReactiveStatistic>();
		}

		public void Push(in NoteType type, in JudgeData data)
		{
			JudgementStats.Push(in type, in data);
			ComboStats.Push(in type, in data);
			FluctuationStats.Push(in type, in data);

			foreach (var statistic in Extensions)
				statistic.Push(in type, in data);

			OnJudgementReceived.Dispatch(type, data);
		}

		public void RegisterExtension(IReactiveStatistic extension)
		{
			Extensions.Add(extension);
		}

		public IReactiveStatistic GetExtensionOrDefault<T>()
			where T : IReactiveStatistic
		{
			return Extensions.FirstOrDefault(e => e.GetType() == typeof(T));
		}
	}
}
