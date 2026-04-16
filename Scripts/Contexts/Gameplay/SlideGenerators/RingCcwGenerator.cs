using System.Collections.Generic;
using AstroDX.Utilities;
using SimaiSharp.Structures;
using UnityEngine;

namespace AstroDX.Contexts.Gameplay.SlideGenerators
{
	public sealed class RingCcwGenerator : SlideGenerator
	{
		private readonly float _angleSpan;
		private readonly float _endRadius;
		private readonly float _length;

		private readonly float _startRadius;
		private readonly float _startRotation;

		public RingCcwGenerator(IReadOnlyList<Location> vertices)
		{
			var inPosition = GetPosition(vertices[0]);
			var inRadians  = Trigonometry.ToPolarAngle(inPosition);

			var outPosition = GetPosition(vertices[1]);
			var outRadians  = Trigonometry.ToPolarAngle(outPosition);

			_startRotation = inRadians;

			_angleSpan   = Trigonometry.GetAngleSpan(inRadians, outRadians, false);
			_startRadius = GetRadiusFromCenter(vertices[0]);
			_endRadius   = GetRadiusFromCenter(vertices[1]);

			_length = _angleSpan * (_startRadius + _endRadius) / 2;
		}

		public override float GetLength()
		{
			return _length;
		}

		public override void GetPoint(float       t,
		                              out Vector2 position,
		                              out float   rotation)
		{
			var radiusAtT   = Mathf.Lerp(_startRadius, _endRadius, t);
			var rotationAtT = _startRotation + _angleSpan * t;

			position = new Vector2(Mathf.Cos(rotationAtT) * radiusAtT,
			                       Mathf.Sin(rotationAtT) * radiusAtT);

			rotation = rotationAtT + Trigonometry.Tau / 4;
		}
	}
}