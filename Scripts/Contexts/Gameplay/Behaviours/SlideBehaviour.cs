using System;
using System.Collections.Generic;
using AstroDX.Contexts.Gameplay.Behaviours.Slide.Handlers;
using AstroDX.Contexts.Gameplay.PlayerScope;
using AstroDX.Contexts.Gameplay.SceneScope;
using AstroDX.Globals;
using Medicine;
using SimaiSharp.Structures;
using UnityEngine;
using TimeSpan = AstroDX.Models.Scoring.TimeSpan;

namespace AstroDX.Contexts.Gameplay.Behaviours.Slide
{
	public sealed class SlideBehaviour : MonoBehaviour, IDisposable
	{
		private readonly List<(float distance, SlideSegmentHandler handler)> _segments = new();
		private          float                                             _endRotation;

		private Vector2   _endPosition;
		private SlideType _endSegmentType;
		private bool      _judged;

		private int _judgedSegmentCount;

		private float _length;

		private float _slideStartTime;

		[Inject.Single]
		private StatisticsManager StatisticsManager { get; }

		[Inject.Single]
		private MusicManager MusicManager { get; }

		[Inject.Single]
		private SlideManager SlideManager { get; }

		public SlidePath Path       { get; private set; }
		public Note      ParentNote { get; private set; }

		public double TimeSinceStart => MusicManager.Time - _slideStartTime;

		private void Update()
		{
			UpdateSegments();

			if (TimeSinceStart >= 0)
				CheckInteraction();

			if (Persistent.Settings.Mods.Auto &&
			    TimeSinceStart > Path.duration)
				Judge();
			else if (TimeSinceStart > Path.duration +
			         TimeSpan.SlideFinish.TimingWindows[^1].lateSpan)
				Judge();
		}

		public void Init(in SlidePath path, in Note parent)
		{
			_judgedSegmentCount = 0;
			_judged             = false;

			Path            = path;
			ParentNote      = parent;
			_slideStartTime = ParentNote.parentCollection.time + Path.delay;

			GenerateHandlers(path);
		}

		private void UpdateSegments()
		{
			var t = (float)TimeSinceStart / Path.duration * _length;

			var isJudgementTarget = TimeSinceStart + Path.delay >= 0;

			for (var i = 0; i < _segments.Count; i++)
			{
				var (segmentStartT, handler) = _segments[i];
				var segmentEndT = i + 1 < _segments.Count ? _segments[i + 1].distance : _length;

				var segmentT = (t - segmentStartT) / (segmentEndT - segmentStartT);

				handler.IsJudgementTarget = isJudgementTarget;
				handler.OnUpdate(segmentT);

				if (!handler.Cleared)
					isJudgementTarget = false;
			}
		}

		private void CheckInteraction()
		{
			while (_judgedSegmentCount < _segments.Count)
			{
				var currentHandler = _segments[_judgedSegmentCount].handler;

				var segmentCleared = currentHandler.Cleared;

				if (!segmentCleared)
					return;

				_judgedSegmentCount++;
			}

			Judge();
		}

		private void GenerateHandlers(in SlidePath slidePath)
		{
			if (_segments.Count > 0)
				_segments.Clear();

			var totalDistance = 0f;

			var handlers = new List<SlideSegmentHandler>();

			for (var i = 0; i < slidePath.segments.Count; i++)
			{
				var segment = slidePath.segments[i];

				var startLocation = i > 0 ? slidePath.segments[i - 1].vertices[^1] : ParentNote.location;

				var handler = SlideSegmentHandler.Recommend(this, segment, startLocation);
				handlers.Add(handler);
			}

			var index = 0;
			foreach (var segmentHandler in handlers)
			{
				segmentHandler.SetIndex(index);
				_segments.Add((totalDistance, segmentHandler));
				totalDistance += segmentHandler.GetLength();

				index++;
			}

			_segments[^1].handler.GetJudgementVector(out _endPosition, out _endRotation);
			_endSegmentType = Path.segments[^1].slideType;

			_length = totalDistance;
		}

		private void Judge()
		{
			if (_judged)
				return;

			_judged = true;

			var incomplete = _judgedSegmentCount < _segments.Count;

			var startTime = ParentNote.parentCollection.time +
			                Path.delay;

			var timeFromEnd = MusicManager.Time - (startTime + Path.duration);

			var distanceToEnd = incomplete
				                    ? _segments[_judgedSegmentCount].handler.GetRemainingLength()
				                    : _length - Mathf.InverseLerp(startTime,
				                                                  startTime + Path.duration,
				                                                  (float)MusicManager.Time) * _length;

			if (incomplete)
			{
				var multipleSegmentsRemaining = _judgedSegmentCount + 1 < _segments.Count;

				if (multipleSegmentsRemaining)
					distanceToEnd += _length - _segments[_judgedSegmentCount + 1].distance;
			}

			StatisticsManager.TallySlide(Path,
			                             distanceToEnd,
			                             timeFromEnd,
			                             _endPosition,
			                             _endRotation,
			                             _endSegmentType,
			                             incomplete);
			
			Dispose();
		}

		public void Dispose()
		{
			foreach (var segment in _segments)
				segment.handler.OnDestroy();

			SlideManager.slides.Release(this);
		}
	}
}