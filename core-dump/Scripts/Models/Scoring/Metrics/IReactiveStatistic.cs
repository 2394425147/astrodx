using SimaiSharp.Structures;

namespace AstroDX.Models.Scoring.Metrics
{
	public interface IReactiveStatistic
	{
		void Push(in NoteType type, in JudgeData data);
	}
}