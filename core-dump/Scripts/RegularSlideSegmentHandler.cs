using System.Collections.Generic;
using System.Linq;
using AstroDX.Contexts.Gameplay.Behaviours.Slide.SlideMarkers;
using AstroDX.Contexts.Gameplay.Interactions;
using AstroDX.Contexts.Gameplay.PlayerScope;
using AstroDX.Contexts.Gameplay.SlideGenerators;
using AstroDX.Globals;
using Medicine;
using SimaiSharp.Structures;
using UnityEngine;

namespace AstroDX.Contexts.Gameplay.Behaviours.Slide.Handlers
{
	public class RegularSlideSegmentHandler : SlideSegmentHandler
	{
		private readonly SlideGenerator _generator;

		private readonly
			List<(TouchInteractable relevantSensor,
				List<RegularSlideArrow> relevantArrows)> _interactionPath = new();

		/// <summary>
		///     Used in auto judgement
		/// </summary>
		private int _initialSensorCount;

		/// <summary>
		///     Used in auto judgement
		/// </summary>
		private int _lastCheckedClearIndex;

		private readonly SlideStarBehaviour _slideStar;

		public RegularSlideSegmentHandler(SlideBehaviour slideBehaviour,
		                                  SlideSegment   segment,
		                                  Location       startLocation) :
			base(slideBehaviour)
		{
			var vertices = segment.vertices.ToList();
			vertices.Insert(0, startLocation);

			_generator = SlideManager.GetGenerator(segment.slideType, vertices);

			_slideStar = SlideManager.slideStars.Get();
			_slideStar.Initialize(ParentPath, ParentNote.parentCollection);

			GenerateArrows();
		}

		[Inject.Single]
		private SlideManager SlideManager { get; }

		public override float GetLength()
		{
			return _generator.GetLength();
		}

		public override void OnUpdate(float segmentT)
		{
			if (IsJudgementTarget)
				CheckInteraction(segmentT);

			UpdateSlideStarPosition(segmentT);
			UpdateArrowOpacity();
		}

		private float _arrowOpacity;

		private void UpdateArrowOpacity()
		{
			var timeSinceNoteStart = TimeSinceSlideStart + ParentPath.delay;

			if (timeSinceNoteStart < 0)
			{
				_arrowOpacity = Mathf.InverseLerp(-Persistent.Settings.Gameplay.slideFadeInDuration,
				                                  0,
				                                  (float)timeSinceNoteStart);

				_arrowOpacity = Mathf.Lerp(0, 0.8f, _arrowOpacity);
			}
			else
			{
				if (_arrowOpacity >= 1)
					return;
				
				_arrowOpacity = 1;
			}

			foreach (var (_, relevantArrows) in _interactionPath)
			{
				foreach (var arrow in relevantArrows)
				{
					arrow.SetAlpha(_arrowOpacity);
				}
			}
		}

		private void CheckInteraction(float segmentT)
		{
			var clearCount = GetClearCount(segmentT);

			for (; clearCount > 0 && _interactionPath.Count > 0; clearCount--)
			{
				foreach (var slideArrow in _interactionPath[0].relevantArrows)
					slideArrow.ExitSequence();

				_interactionPath.RemoveAt(0);
			}

			Cleared = _interactionPath.Count == 0;
		}

		public override float GetRemainingLength()
		{
			return (float)_interactionPath.Count / _initialSensorCount * GetLength();
		}

		public override void OnDestroy()
		{
			base.OnDestroy();
			
			foreach (var (_, arrows) in _interactionPath)
			foreach (var slideArrow in arrows)
				SlideManager.regularSlideArrows.Release(slideArrow);

			SlideManager.slideStars.Release(_slideStar);
		}

		public override void GetJudgementVector(out Vector2 position, out float rotation)
		{
			_generator.GetPoint(1, out position, out rotation);
		}

		private void UpdateSlideStarPosition(float segmentT)
		{
			if (disposed)
				return;

			var starTransform = _slideStar.transform;

			if (segmentT >= 1 && IsLastSegment)
			{
				starTransform.localScale    = Vector3.one * Persistent.Settings.Gameplay.noteScale;
				_slideStar.Appearance.Color = new Color(1, 1, 1, 0);
				return;
			}

			_generator.GetPoint(Mathf.Clamp01(segmentT), out var position, out var rotation);
			starTransform.SetLocalPositionAndRotation(position, Quaternion.Euler(0, 0, rotation * Mathf.Rad2Deg - 90));

			if (TimeSinceSlideStart   < 0  &&
			    indexInSlide          == 0 &&
			    ParentNote.slideMorph != SlideMorph.SuddenIn)
			{
				var interpolation =
					Mathf.InverseLerp(-ParentPath.delay, 0,
					                  (float)TimeSinceSlideStart);
				starTransform.localScale = new Vector3(interpolation, interpolation) *
				                           Persistent.Settings.Gameplay.noteScale;
				_slideStar.Appearance.Color = new Color(1, 1, 1, interpolation);
			}

			if (segmentT < 0)
				return;

			starTransform.localScale    = Vector3.one * Persistent.Settings.Gameplay.noteScale;
			_slideStar.Appearance.Color = Color.white;
		}

		private void GenerateArrows()
		{
			var               relevantArrows = new List<RegularSlideArrow>();
			TouchInteractable relevantSensor = null;

			var totalLength = GetLength();

			var arrowCount = Mathf.FloorToInt(totalLength / SlideManager.ArrowDistance);

			for (var (distance, index) = (SlideManager.ArrowDistance, 0);
			     distance <= totalLength;
			     distance += SlideManager.ArrowDistance)
			{
				_generator.GetPoint(distance / totalLength, out var position, out var rotation);

				var slideArrow = SlideManager.regularSlideArrows.Get();
				slideArrow.Initialize(position, rotation,
				                      arrowCount, index, isBreak, isEach);

				var sensor = slideArrow.GetClosestSensor();

				if (relevantSensor != sensor)
				{
					if (relevantSensor != null)
						_interactionPath.Add((relevantSensor, relevantArrows));

					relevantArrows = new List<RegularSlideArrow>();
					relevantSensor = sensor;
				}

				relevantArrows.Add(slideArrow);
				index++;
			}

			if (relevantArrows.Count > 0) _interactionPath.Add((relevantSensor, relevantArrows));

			_initialSensorCount = _interactionPath.Count;
		}

		private int GetClearCount(double segmentT)
		{
			if (Persistent.Settings.Mods.Auto && segmentT >= 0)
			{
				var clearCount = Mathf.FloorToInt((float)(segmentT * _initialSensorCount));


				if (_lastCheckedClearIndex >= clearCount)
					return 0;

				var lastCheckedClearIndex = _lastCheckedClearIndex;
				_lastCheckedClearIndex = clearCount;
				return clearCount - lastCheckedClearIndex;
			}
			else
			{
				var clearCount = 0;
				var scanIndex  = 0;
				var tolerance  = Persistent.Settings.Judgement.slideTolerance;

				while (tolerance >= 0 && scanIndex < _interactionPath.Count)
				{
					if (_interactionPath[scanIndex].relevantSensor.GetSlideJudgement())
					{
						tolerance  = Persistent.Settings.Judgement.slideTolerance;
						clearCount = scanIndex + 1;
					}
					else
					{
						tolerance--;
					}

					scanIndex++;
				}

				return clearCount;
			}
		}
	}
}