using System;

namespace CpvrLab.TellLevels.Scripts.PlayersAndIO
{
	[Serializable]
	public class GuardianSettings
	{
		public GuardianSettings()
		{
			MaxHealth = 100;
			StartingHealth = 100;

			MaxStamina = 100;
			StartingStamina = 100;

		}
		public int MaxHealth { get; set; }
		public int StartingHealth { get; set; }

		public int MaxStamina { get; set; }
		public int StartingStamina { get; set; }
	}
}
