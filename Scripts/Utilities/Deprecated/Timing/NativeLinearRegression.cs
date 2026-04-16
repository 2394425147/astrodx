using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Timing
{
	public sealed class NativeLinearRegression : IDisposable
	{
		public int SampleCount { get; private set; }
		public int MaxSampleCount { get; }

		private NativeArray<double2> _samples;

		public NativeLinearRegression(int maxSampleCount = 12)
		{
			MaxSampleCount = maxSampleCount;
			_samples = new NativeArray<double2>(maxSampleCount, Allocator.Persistent);
		}

		public void Sample(double2 plot)
		{
			if (SampleCount < MaxSampleCount)
				SampleCount++;
			else
				for (var i = 1; i < SampleCount; i++)
					_samples[i - 1] = _samples[i];

			_samples[SampleCount - 1] = plot;
		}

		public double Predict(in double x)
		{
			using var result = new NativeArray<double>(2, Allocator.TempJob);

			var jobData = new LinearRegressionJob
			{
				samples = _samples.Slice(0, SampleCount),
				yInterceptAndSlope = result
			};

			jobData.Schedule().Complete();

			var yIntercept = result[0];
			var slope = result[1];

			return x * slope + yIntercept;
		}

		public void Clear()
		{
			SampleCount = 0;
		}

		public void Dispose()
		{
			_samples.Dispose();
		}
	}

	[BurstCompile]
	public struct LinearRegressionJob : IJob
	{
		[ReadOnly]
		public NativeSlice<double2> samples;

		[WriteOnly]
		public NativeArray<double> yInterceptAndSlope;

		public void Execute()
		{
			double sumOfX = 0;
			double sumOfY = 0;
			double sumOfXSq = 0;
			double sumCoDeviates = 0;

			var sampleCount = samples.Length;

			for (var i = 0; i < sampleCount; i++)
			{
				var plot = samples[i];
				sumCoDeviates += plot.x * plot.y;
				sumOfX += plot.x;
				sumOfY += plot.y;
				sumOfXSq += plot.x * plot.x;
			}

			var ssX = sumOfXSq - sumOfX * sumOfX / sampleCount;
			var sCo = sumCoDeviates - sumOfX * sumOfY / sampleCount;

			var meanX = sumOfX / sampleCount;
			var meanY = sumOfY / sampleCount;

			// y-intercept
			yInterceptAndSlope[0] = meanY - sCo / ssX * meanX;

			// slope
			yInterceptAndSlope[1] = sCo / ssX;
		}
	}
}