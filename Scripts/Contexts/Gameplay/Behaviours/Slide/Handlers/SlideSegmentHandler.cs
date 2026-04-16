using System.Linq;
using SimaiSharp.Structures;
using UnityEngine;

namespace AstroDX.Contexts.Gameplay.Behaviours.Slide.Handlers
{
	public abstract class SlideSegmentHandler
	{
		protected readonly bool isBreak;
		protected readonly bool isEach;

		protected bool disposed;

		protected int indexInSlide = -1;

		protected SlideSegmentHandler(SlideBehaviour slideBehaviour)
		{
			disposed = false;

			ParentSlide = slideBehaviour;

			isEach = ParentSlide.ParentNote.parentCollection
			                    .Sum(n => n.slidePaths.Count) > 1;

			isBreak = ParentSlide.Path.type == NoteType.Break;
		}

		public bool Cleared { get; protected set; }

		protected SlideBehaviour ParentSlide { get; }
		protected SlidePath      ParentPath  => ParentSlide.Path;
		protected Note           ParentNote  => ParentSlide.ParentNote;

		protected double TimeSinceSlideStart => ParentSlide.TimeSinceStart;
		protected bool   IsLastSegment       => indexInSlide != ParentPath.segments.Count - 1;
		public    bool   IsJudgementTarget   { get; set; }

		public void SetIndex(int index)
		{
			indexInSlide = index;
		}

		/// <summary>
		///     Describes the position and the up vector for the judgement text.
		/// </summary>
		public abstract void GetJudgementVector(out Vector2 position, out float rotation);

		public abstract void OnUpdate(float segmentT);

		public virtual void OnDestroy()
		{
			disposed = true;
		}

		public abstract float GetLength();

		public abstract float GetRemainingLength();

		public static SlideSegmentHandler Recommend(SlideBehaviour slideBehaviour,
		                                            SlideSegment   segment,
		                                            Location       startLocation)
		{
			return segment.slideType switch
			{
				SlideType.Fan => new FanSlideSegmentHandler(slideBehaviour, segment, startLocation),
				_             => new RegularSlideSegmentHandler(slideBehaviour, segment, startLocation)
			};
		}
	}
}