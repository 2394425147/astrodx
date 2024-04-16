using SimaiSharp.Structures;

namespace AstroDX.Models.Scoring.Metrics.Extended
{
	public class HealthStats : IReactiveStatistic
	{
		public HealthStats(int maxHealth)
		{
			MaxHealth = maxHealth;
		}

		public void Push(in NoteType type, in JudgeData data)
		{
			Loss += GameSettings.Settings.Profile.Mods.LifeDrain.GetHealthLoss(type, data);
		}

		public int MaxHealth { get; }
		public int Loss { get; private set; }
	}
}