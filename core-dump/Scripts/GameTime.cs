using Unity.Mathematics;
using UnityEngine;

namespace Timing
{
	public class GameTime : MonoBehaviour
	{
		private double _lastCapturedDspTime;
		public static bool Active { get; private set; }
		public static double StartTime { get; private set; }
		public static double TimeSinceClipStart { get; private set; }

		private static NativeLinearRegression TimePrediction { get; set; }

		private void Update()
		{
			if (!Active) return;

			#region Smoothen DSP buffer time

			if (AudioSettings.dspTime > _lastCapturedDspTime)
			{
				TimePrediction.Sample(new double2(Time.realtimeSinceStartupAsDouble, AudioSettings.dspTime));
				_lastCapturedDspTime = AudioSettings.dspTime;
			}

			var smoothDspTime = TimePrediction.SampleCount < 2
				                    ? AudioSettings.dspTime
				                    : TimePrediction.Predict(Time.realtimeSinceStartupAsDouble);

			#endregion

			TimeSinceClipStart = smoothDspTime - StartTime;
		}

		private void OnDestroy()
		{
			TimePrediction.Dispose();
		}

		/// <summary>
		///     Tells the time manager to start counting time
		/// </summary>
		public static void StartFrom(double startTime)
		{
			StartTime = startTime;
			TimeSinceClipStart = AudioSettings.dspTime - startTime;
			Active = true;

			TimePrediction ??= new NativeLinearRegression();

			TimePrediction.Clear();
		}

		public static void Pause()
		{
			Active = false;
		}

		public static void UnPause(double timeSinceClipStart)
		{
			TimePrediction.Clear();
			StartTime = AudioSettings.dspTime - timeSinceClipStart;
			TimeSinceClipStart = timeSinceClipStart;

			Active = true;
		}
	}
}
