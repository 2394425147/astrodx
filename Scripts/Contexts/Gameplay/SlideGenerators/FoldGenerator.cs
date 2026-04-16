using System.Collections.Generic;
using SimaiSharp.Structures;
using UnityEngine;

namespace AstroDX.Contexts.Gameplay.SlideGenerators
{
	public sealed class FoldGenerator : SlideGenerator
	{
		private readonly Vector2 _endPoint;
		private readonly float _endRotation;
		private readonly Vector2 _midPoint;
		private readonly Vector2 _startPoint;

		private readonly float _startRotation;

		private readonly float _startSegmentLength;
		private readonly float _totalLength;

		public FoldGenerator(IReadOnlyList<Location> vertices)
		{
			_startPoint = GetPosition(vertices[0]);
			_endPoint   = GetPosition(vertices[1]);
			_midPoint   = Vector2.zero;

			var startSegment = _midPoint - _startPoint;
			_startSegmentLength = startSegment.magnitude;
			_startRotation      = Mathf.Atan2(startSegment.y, startSegment.x);

			var endSegment     = _endPoint - _midPoint;
			var endSegmentSpan = endSegment.magnitude;
			_endRotation = Mathf.Atan2(endSegment.y, endSegment.x);

			_totalLength = _startSegmentLength + endSegmentSpan;
		}

		public override float GetLength()
		{
			return _totalLength;
		}

		public override void GetPoint(float t, out Vector2 position, out float rotation)
		{
			var distanceFromStart = t * _totalLength;

			if (distanceFromStart < _startSegmentLength)
			{
				position = Vector2.Lerp(_startPoint, _midPoint,
				                        Mathf.InverseLerp(0,
				                                          _startSegmentLength / _totalLength,
				                                          t));
				rotation = _startRotation;
			}
			else
			{
				position = Vector2.Lerp(_midPoint, _endPoint,
				                        Mathf.InverseLerp(_startSegmentLength / _totalLength,
				                                          1,
				                                          t));
				rotation = _endRotation;
			}
		}
	}
}