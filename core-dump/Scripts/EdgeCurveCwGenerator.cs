using System.Collections.Generic;
using AstroDX.Contexts.Gameplay.PlayerScope;
using AstroDX.Utilities;
using SimaiSharp.Structures;
using UnityEngine;

namespace AstroDX.Contexts.Gameplay.SlideGenerators
{
	public sealed class EdgeCurveCwGenerator : SlideGenerator
	{
		private const float CurveRadius         = RenderManager.CenterRadius * 1.2f;
		private const float CenterAngularOffset = Trigonometry.Tau / 4 - Trigonometry.Tau / 16;
		private const float CenterRadialOffset  = RenderManager.PlayFieldRadius * 0.4662f;

		private readonly Vector2 _centerPosition;
		private readonly float   _curveLength;
		private readonly Vector2 _endPoint;

		private readonly float _startRotation;
		private readonly float _endRotation;

		private readonly float   _startLength;
		private readonly Vector2 _startPoint;
		private readonly Vector2 _tangentInPoint;
		private readonly float   _tangentInRotation;
		private readonly Vector2 _tangentOutPoint;
		private readonly float   _totalLength;

		public EdgeCurveCwGenerator(IReadOnlyList<Location> vertices)
		{
			var startRotation = GetRotation(vertices[0]);
			var endRotation   = GetRotation(vertices[1]);

			var centerAngle = startRotation + CenterAngularOffset;
			_centerPosition = new Vector2(CenterRadialOffset * Mathf.Cos(centerAngle),
			                              CenterRadialOffset * Mathf.Sin(centerAngle));

			_startPoint = GetPositionRadial(startRotation);

			var relativeStartRotation = Trigonometry.ToPolarAngle(_startPoint, _centerPosition);

			var magnitude  = (_centerPosition - _startPoint).magnitude;
			var startDelta = Trigonometry.GetTangentAngleDelta(CurveRadius, magnitude, true);

			_tangentInRotation = relativeStartRotation + startDelta;
			_tangentInPoint = GetPositionRadial(_tangentInRotation, CurveRadius) +
			                  _centerPosition;

			_endPoint = GetPositionRadial(endRotation);

			var relativeEndRotation = Trigonometry.ToPolarAngle(_endPoint, _centerPosition);
			var endMagnitude        = (_endPoint - _centerPosition).magnitude;
			var endDelta            = Trigonometry.GetTangentAngleDelta(CurveRadius, endMagnitude, false);

			var tangentOutRotation = relativeEndRotation + endDelta;
			_tangentOutPoint = GetPositionRadial(tangentOutRotation, CurveRadius) +
			                   _centerPosition;

			var startSegment = _tangentInPoint - _startPoint;
			_startLength  = startSegment.magnitude;
			_startRotation = Mathf.Atan2(startSegment.y, startSegment.x);

			_curveLength = Trigonometry.GetAngleSpan(_tangentInRotation, tangentOutRotation,
			                                         true, Trigonometry.Tau / 4f) * CurveRadius;

			var endSegment = _endPoint - _tangentOutPoint;
			var endLength  = endSegment.magnitude;
			_endRotation = Mathf.Atan2(endSegment.y, endSegment.x);

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

				rotation = _startRotation;
			}
			else if (distanceFromStart < _startLength + _curveLength)
			{
				var localT = Mathf.InverseLerp(_startLength, _startLength + _curveLength, distanceFromStart);
				position = new Vector2(Mathf.Cos(_tangentInRotation - _curveLength / CurveRadius * localT) *
				                       CurveRadius,
				                       Mathf.Sin(_tangentInRotation - _curveLength / CurveRadius * localT) *
				                       CurveRadius);

				var forward = position.Rotate(-Trigonometry.Tau / 4);
				rotation  =  Mathf.Atan2(forward.y, forward.x);
				position += _centerPosition;
			}
			else
			{
				position = Vector2.Lerp(_tangentOutPoint,
				                        _endPoint,
				                        Mathf.InverseLerp(_startLength + _curveLength,
				                                          _totalLength,
				                                          distanceFromStart));

				rotation = _endRotation;
			}
		}
	}
}