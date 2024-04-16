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
	public class FanSlideSegmentHandler : SlideSegmentHandler
	{
		private const int Margin     = 2;
		private const int ArrowCount = 11;

		private readonly List<FanSlideArrow> _arrows = new();
		private readonly Vector2[]           _endPositions;

		private readonly List<List<TouchInteractable>> _interactableGroups = new();

		private readonly Vector2   _startPosition;
		private readonly Vector2[] _ups;

		private int _interactionReceivedState;

		private SlideStarBehaviour[] _stars;

		public FanSlideSegmentHandler(SlideBehaviour slideBehaviour,
		                              SlideSegment   segment,
		                              Location       startLocation) :
			base(slideBehaviour)
		{
			var vertices = segment.vertices.ToList();
			vertices.Insert(0, startLocation);

			_startPosition = SlideGenerator.GetPosition(vertices[0]);

			var ccwEndLocation = startLocation;
			ccwEndLocation.index += 3;

			var centerEndLocation = startLocation;
			centerEndLocation.index += 4;

			var cwEndLocation = startLocation;
			cwEndLocation.index += 5;

			_endPositions = new[]
			                {
				                SlideGenerator.GetPosition(ccwEndLocation),
				                SlideGenerator.GetPosition(centerEndLocation),
				                SlideGenerator.GetPosition(cwEndLocation)
			                };

			_ups = new[]
			       {
				       _endPositions[0] - _startPosition,
				       _endPositions[1] - _startPosition,
				       _endPositions[2] - _startPosition
			       };

			_stars = new[]
			         {
				         SlideManager.slideStars.Get(),
				         SlideManager.slideStars.Get(),
				         SlideManager.slideStars.Get()
			         };

			foreach (var star in _stars) star.Initialize(ParentPath, ParentNote.parentCollection);

			GenerateSensors();
			GenerateArrows();
		}

		[Inject.Single]
		private SlideManager SlideManager { get; }

		[Inject.Single]
		private TouchManager TouchManager { get; }

		public override float GetLength()
		{
			return RenderManager.PlayFieldRadius * 2;
		}

		public override void OnUpdate(float segmentT)
		{
			if (IsJudgementTarget)
				CheckInteraction();

			UpdateSlideStarPosition(segmentT);
			UpdateArrowOpacity();
		}

		private void UpdateArrowOpacity()
		{
			var timeSinceNoteStart = TimeSinceSlideStart + ParentPath.delay;

			var alpha = 0.8f;
			if (timeSinceNoteStart < 0)
			{
				var colorPoint = Mathf.InverseLerp(-Persistent.Settings.Gameplay.slideFadeInDuration,
				                                   0,
				                                   (float)timeSinceNoteStart);

				alpha = Mathf.Lerp(0, 0.8f, colorPoint);
			}

			foreach (var arrow in _arrows.Select(a => a.spriteRenderer))
			{
				var color = arrow.color;
				color.a = alpha;

				arrow.color = color;
			}
		}

		private void CheckInteraction()
		{
			if (_interactionReceivedState == 0)
			{
				if (!_interactableGroups[0][0].GetSlideJudgement())
					return;

				_interactionReceivedState = 1;
				foreach (var slideArrow in _arrows
				                           .Where(a => a.interactionGroupIndex == 0)
				                           .ToList())
				{
					_arrows.Remove(slideArrow);
					slideArrow.ExitSequence();
				}
			}

			if (_interactionReceivedState >= 3)
			{
				Cleared = true;
				return;
			}

			for (var groupIndex = _interactionReceivedState; groupIndex < 3; groupIndex++)
			{
				foreach (var interactable in
				         _interactableGroups[groupIndex]
					         .Where(interactable => interactable.GetSlideJudgement())
					         .ToList())
					_interactableGroups[groupIndex].Remove(interactable);

				if (_interactableGroups[groupIndex].Count > 0 ||
				    _interactionReceivedState             < groupIndex)
					continue;

				foreach (var slideArrow in _arrows
				                           .Where(a => a.interactionGroupIndex == _interactionReceivedState)
				                           .ToList())
				{
					_arrows.Remove(slideArrow);
					slideArrow.ExitSequence();
				}

				_interactionReceivedState++;
			}
		}

		public override float GetRemainingLength()
		{
			return (3 - _interactionReceivedState) / 3f * GetLength();
		}

		public override void OnDestroy()
		{
			base.OnDestroy();
			
			foreach (var slideArrow in _arrows)
				SlideManager.fanSlideArrows.Release(slideArrow);

			foreach (var star in _stars) 
				SlideManager.slideStars.Release(star);
		}

		private void UpdateSlideStarPosition(float segmentT)
		{
			if (disposed || _stars is null)
				return;

			for (var index = 0; index < _stars.Length; index++)
			{
				var star = _stars[index];

				if (star == null)
					continue;

				var transform = star.transform;

				if (segmentT >= 1 && indexInSlide != ParentPath.segments.Count - 1)
				{
					transform.localScale  = Vector3.one * Persistent.Settings.Gameplay.noteScale;
					star.Appearance.Color = new Color(1, 1, 1, 0);

					continue;
				}

				var position = Vector3.Lerp(_startPosition,
				                            _endPositions[index],
				                            segmentT);

				transform.localPosition = position;
				transform.up            = _ups[index];

				if (TimeSinceSlideStart   < 0  &&
				    indexInSlide          == 0 &&
				    ParentNote.slideMorph != SlideMorph.SuddenIn)
				{
					var interpolation =
						Mathf.InverseLerp(-ParentPath.delay, 0,
						                  (float)TimeSinceSlideStart);
					transform.localScale = new Vector3(interpolation, interpolation) *
					                       Persistent.Settings.Gameplay.noteScale;
					star.Appearance.Color = new Color(1, 1, 1, interpolation);

					return;
				}

				if (segmentT < 0)
					return;

				transform.localScale  = Vector3.one * Persistent.Settings.Gameplay.noteScale;
				star.Appearance.Color = Color.white;
			}
		}

		public override void GetJudgementVector(out Vector2 position, out float rotation)
		{
			position = _endPositions[1];
			rotation = Mathf.Atan2(_ups[1].y, _ups[1].x);
		}

		private void GenerateSensors()
		{
			_interactableGroups.AddRange(new List<TouchInteractable>[]
			                             {
				                             new()
				                             {
					                             TouchManager.GetCollider(new Location
					                                                      {
						                                                      group = NoteGroup.ASensor,
						                                                      index = ParentNote.location.index
					                                                      })
				                             },
				                             new()
				                             {
					                             TouchManager.GetCollider(new Location
					                                                      {
						                                                      group = NoteGroup.BSensor,
						                                                      index = (ParentNote.location.index + 2) %
							                                                      8
					                                                      }),
					                             TouchManager.GetCollider(new Location
					                                                      {
						                                                      group = NoteGroup.BSensor,
						                                                      index = (ParentNote.location.index + 6) %
							                                                      8
					                                                      })
				                             },
				                             new()
				                             {
					                             TouchManager.GetCollider(new Location
					                                                      {
						                                                      group = NoteGroup.ASensor,
						                                                      index = (ParentNote.location.index + 5) %
							                                                      8
					                                                      }),
					                             TouchManager.GetCollider(new Location
					                                                      {
						                                                      group = NoteGroup.ASensor,
						                                                      index = (ParentNote.location.index + 3) %
							                                                      8
					                                                      }),
					                             TouchManager.GetCollider(new Location
					                                                      {
						                                                      group = NoteGroup.ASensor,
						                                                      index = (ParentNote.location.index + 4) %
							                                                      8
					                                                      })
				                             }
			                             }
			                            );
		}

		private void GenerateArrows()
		{
			var halfMargin = Mathf.RoundToInt(Margin / 2f);
			for (var i = 0;
			     i < ArrowCount;
			     i++)
			{
				var position = Vector3.Lerp(_startPosition,
				                            _endPositions[1],
				                            (float)(i + halfMargin) / (ArrowCount + Margin));

				var arrow = SlideManager.fanSlideArrows.Get();
				arrow.Initialize(position,
				                 _ups[1],
				                 ArrowCount,
				                 i,
				                 isBreak,
				                 isEach);

				AssignArrows(arrow, i);
			}
		}

		private void AssignArrows(FanSlideArrow arrow, int index)
		{
			_arrows.Add(arrow);

			if (index < ArrowCount / 3f)
				arrow.interactionGroupIndex = 0;
			else if (index < ArrowCount * 2 / 3f)
				arrow.interactionGroupIndex = 1;
			else
				arrow.interactionGroupIndex = 2;
		}
	}
}