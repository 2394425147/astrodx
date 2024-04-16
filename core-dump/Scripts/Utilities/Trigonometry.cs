using UnityEngine;

namespace AstroDX.Utilities
{
	public static class Trigonometry
	{
		public const float Tau = Mathf.PI * 2;

		public static Vector2 Rotate(in this Vector2 v, in float degreesRad)
		{
			var magnitude       = v.magnitude;
			var originalDegrees = Mathf.Atan2(v.y, v.x);
			var newDegrees      = originalDegrees + degreesRad;

			var newX = Mathf.Cos(newDegrees) * magnitude;
			var newY = Mathf.Sin(newDegrees) * magnitude;

			return new Vector2(newX, newY);
		}

		/// <summary>
		///     Calculates a point's angle relative to a center point.
		/// </summary>
		/// <param name="position">The absolute position of a point.</param>
		/// <param name="offset">The offset of the calculation's center point.</param>
		/// <returns>The relative angle of a point from the given center point.</returns>
		internal static float ToPolarAngle(in Vector2 position, in Vector2? offset = null)
		{ 
			if (!offset.HasValue)
				return Mathf.Atan2(position.y, position.x);

			var difference = position - offset.Value;
			return Mathf.Atan2(difference.y, difference.x);
		}

		/// <summary>
		///     Calculates the angle between a line to a point on a ring
		///     and another line perpendicular from a tangent line of that point.
		/// </summary>
		/// <param name="adjacent"></param>
		/// <param name="hypotenuse"></param>
		/// <param name="clockwise"></param>
		/// <returns></returns>
		internal static float GetTangentAngleDelta(in float adjacent,
		                                           in float hypotenuse,
		                                           in bool  clockwise)
		{
			var angleDiff = Mathf.Acos(adjacent / hypotenuse);
			return clockwise ? -angleDiff : angleDiff;
		}

		/// <summary>
		///     <para>
		///         Calculates the angle between <c>startRotation</c> and <c>endRotation</c>,
		///         given its traversing direction.
		///     </para>
		/// </summary>
		/// <param name="startRotation">The starting rotation.</param>
		/// <param name="endRotation">The ending rotation.</param>
		/// <param name="clockwise">Traversing direction.</param>
		/// <param name="wrapThreshold">
		///     <para>Wraps to full circle for spans smaller than this value.</para>
		///     <para><code>Tau / 4f</code> is recommended for offset circles</para>
		/// </param>
		/// <returns>
		///     The span between the starting rotation and the ending rotation on a unit circle,
		///     in radians.
		/// </returns>
		public static float GetAngleSpan(in float startRotation,
		                                 in float endRotation,
		                                 bool     clockwise,
		                                 float    wrapThreshold = Tau / 32f)
		{
			var span = clockwise
				           ? (startRotation - endRotation   + 2 * Tau) % Tau
				           : (endRotation   - startRotation + 2 * Tau) % Tau;

			if (span <= wrapThreshold)
				span += Tau;

			return span;
		}
	}
}