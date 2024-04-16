using System.Collections.Generic;
using SimaiSharp.Structures;
using UnityEngine;

namespace AstroDX.Contexts.Gameplay.SlideGenerators
{
	public sealed class StraightGenerator : SlideGenerator
	{
		private readonly Vector2 _endPoint;
		private readonly float _rotation;
		private readonly float   _length;
		private readonly Vector2 _startPoint;

		public StraightGenerator(IReadOnlyList<Location> vertices)
		{
			_startPoint = GetPosition(vertices[0]);
			_endPoint   = GetPosition(vertices[1]);

			var segment = _endPoint - _startPoint;
			_length  = segment.magnitude;
			_rotation = Mathf.Atan2(segment.y, segment.x);
		}

		public override float GetLength()
		{
			return _length;
		}

		public override void GetPoint(float t, out Vector2 position, out float rotation)
		{
			position = Vector2.Lerp(_startPoint, _endPoint, t);
			rotation  = _rotation;
		}
	}
}