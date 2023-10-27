using System.Collections.Generic;
using AstroDX.Contexts.Gameplay.PlayerScope;
using AstroDX.Utilities;
using SimaiSharp.Structures;
using UnityEngine;

namespace AstroDX.Contexts.Gameplay.SlideGenerators
{
	public sealed class CurveCwGenerator : SlideGenerator
	{
		private const    float   CurveRadius = RenderManager.CenterRadius;
		private const    float   RingRadius  = RenderManager.PlayFieldRadius;
		private readonly float   _curveLength;
		private readonly float _endForward;
		private readonly Vector2 _endPoint;

		private readonly float _startForward;

		private readonly float _startLength;

		private readonly Vector2 _startPoint;
		private readonly Vector2 _tangentInPoint;
		private readonly float   _tangentInRotation;
		private readonly Vector2 _tangentOutPoint;
		private readonly float   _totalLength;

		public CurveCwGenerator(IReadOnlyList<Location> vertices)
		{
			var startRotation = GetRotation(vertices[0]);
			var endRotation   = GetRotation(vertices[1]);

			_tangentInRotation = startRotation +
			                     Trigonometry.GetTangentAngleDelta(CurveRadius, RingRadius, true);
			var tangentOutRotation = endRotation +
			                         Trigonometry.GetTangentAngleDelta(CurveRadius, RingRadius, false);

			_startPoint      = GetPositionRadial(startRotation);
			_tangentInPoint  = GetPositionRadial(_tangentInRotation, CurveRadius);
			_tangentOutPoint = GetPositionRadial(tangentOutRotation, CurveRadius);
			_endPoint        = GetPositionRadial(endRotation);

			var startSegment = _tangentInPoint - _startPoint;
			_startLength  = startSegment.magnitude;
			_startForward = Mathf.Atan2(startSegment.y, startSegment.x);

			_curveLength = Trigonometry.GetAngleSpan(_tangentInRotation, tangentOutRotation,
			                                         true) * CurveRadius;

			var endSegment = _endPoint - _tangentOutPoint;
			var endLength  = endSegment.magnitude;
			_endForward = Mathf.Atan2(endSegment.y, endSegment.x);

			_totalLength = _startLength + _curveLength + endLength;
		}

		public override float GetLength()
		{
			return _totalLength;
		}

		public override void GetPoint(float t, out Vector2 position, out float rotation)
		{
			var distanceFromStart = t * _totalLength;

			if (distanceFromStart < _startLength)
			{
				position = Vector2.Lerp(_startPoint,
				                        _tangentInPoint,
				                        Mathf.InverseLerp(0,
				                                          _startLength,
				                                          distanceFromStart));

				rotation = _startForward;
			}
			else if (distanceFromStart < _startLength + _curveLength)
			{
				var localT = Mathf.InverseLerp(_startLength, _startLength + _curveLength, distanceFromStart);
				position = new Vector2(Mathf.Cos(_tangentInRotation - _curveLength / CurveRadius * localT) *
				                       CurveRadius,
				                       Mathf.Sin(_tangentInRotation - _curveLength / CurveRadius * localT) *
				                       CurveRadius);

				var forward = position.Rotate(-Trigonometry.Tau / 4);
				rotation = Mathf.Atan2(forward.y, forward.x);
			}
			else
			{
				position = Vector2.Lerp(_tangentOutPoint,
				                        _endPoint,
				                        Mathf.InverseLerp(_startLength + _curveLength,
				                                          _totalLength,
				                                          distanceFromStart));

				rotation = _endForward;
			}
		}
	}
}