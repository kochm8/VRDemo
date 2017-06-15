using UnityEngine;

namespace CpvrLab.ArcherGuardian.Scripts.PlayersAndIO
{
	public abstract class AGPlayerSettings
	{
		public uint MaxHealth = 100;
		public uint StartingHealth = 100;

		public uint MaxStamina = 100;
		public uint StartingStamina = 100;

		public uint HealthRegeneration = 10;
		public uint StaminaRegeneration = 10;

		#region teleport
		public LayerMask TeleportLayers;
		public PlayAreaVis PlayAreaVisPrefab;

		public Color LegalAreaColor = Color.green;
		public Color IllegalAreaColor = Color.red;
		#endregion teleport
	}
}
