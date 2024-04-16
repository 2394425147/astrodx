using System;
using SimaiSharp.Structures;
using UnityEngine;

namespace AstroDX.Models.Scoring.Metrics.Internal
{
	public sealed class JudgementStats : IReactiveStatistic
	{
		public NoteRecord HoldRecord  { get; }
		public NoteRecord SlideRecord { get; }
		public NoteRecord TapRecord   { get; }
		public NoteRecord TouchRecord { get; }
		public NoteRecord BreakRecord { get; }

		public uint MaxTapCount   { get; }
		public uint MaxTouchCount { get; }
		public uint MaxHoldCount  { get; }
		public uint MaxSlideCount { get; }
		public uint MaxBreakCount { get; }

		public uint JudgedCriticalCount { get; private set; }
		public uint JudgedPerfectCount  { get; private set; }
		public uint JudgedGreatCount    { get; private set; }
		public uint JudgedGoodCount     { get; private set; }
		public uint JudgedMissCount     { get; private set; }
		public uint JudgedNoteCount     { get; private set; }

		public uint MaxNoteCount =>
			TapRecord.MaxCount  + TouchRecord.MaxCount +
			HoldRecord.MaxCount + SlideRecord.MaxCount +
			BreakRecord.MaxCount;

		public bool AllNotesPassed =>
			TapRecord.MaxCount   == TapRecord.PassedCount   &&
			TouchRecord.MaxCount == TouchRecord.PassedCount &&
			HoldRecord.MaxCount  == HoldRecord.PassedCount  &&
			SlideRecord.MaxCount == SlideRecord.PassedCount &&
			BreakRecord.MaxCount == BreakRecord.PassedCount;

		public JudgementStats(ReadOnlyMemory<NoteCollection> noteCollections)
		{
			foreach (var noteCollection in noteCollections.Span)
			{
				foreach (var note in noteCollection)
				{
					// ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
					switch (note.type)
					{
						case NoteType.Tap:
							MaxTapCount++;
							break;
						case NoteType.Touch:
							MaxTouchCount++;
							break;
						case NoteType.Hold:
							MaxHoldCount++;
							break;
						case NoteType.Break:
							MaxBreakCount++;
							break;
					}

					foreach (var slide in note.slidePaths)
					{
						if (slide.type == NoteType.Break)
							MaxBreakCount++;
						else
							MaxSlideCount++;
					}
				}
			}

			TapRecord   = new NoteRecord(MaxTapCount);
			TouchRecord = new NoteRecord(MaxTouchCount);
			HoldRecord  = new NoteRecord(MaxHoldCount);
			SlideRecord = new NoteRecord(MaxSlideCount);
			BreakRecord = new NoteRecord(MaxBreakCount);
		}

		public void Push(in NoteType type, in JudgeData data)
		{
			switch (type)
			{
				case NoteType.Tap:
					TapRecord.Push(in data);
					break;
				case NoteType.Touch:
					TouchRecord.Push(in data);
					break;
				case NoteType.Hold:
					HoldRecord.Push(in data);
					break;
				case NoteType.Slide:
					SlideRecord.Push(in data);
					break;
				case NoteType.Break:
					BreakRecord.Push(in data);
					BreakRecord.Extras += data.grade switch
					{
						JudgeGrade.CriticalPerfect => 1,
						JudgeGrade.Perfect =>
							Mathf.Abs((float)data.offset) <= 0.033335f ? 0.75 : 0.5,
						JudgeGrade.Great => 0.4,
						JudgeGrade.Good  => 0.3,
						JudgeGrade.Miss  => 0,
						_                => 0
					};

					break;
				case NoteType.ForceInvalidate:
				default:
					break;
			}

			switch (data.grade)
			{
				case JudgeGrade.CriticalPerfect:
					JudgedCriticalCount++;
					break;
				case JudgeGrade.Perfect:
					JudgedPerfectCount++;
					break;
				case JudgeGrade.Great:
					JudgedGreatCount++;
					break;
				case JudgeGrade.Good:
					JudgedGoodCount++;
					break;
				case JudgeGrade.Miss:
					JudgedMissCount++;
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			JudgedNoteCount++;
		}
	}
}
