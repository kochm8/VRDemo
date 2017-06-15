using System;
using UnityEngine;

namespace CpvrLab.ArcherGuardian.Scripts.GameLogic
{
	[Serializable]
	public class AGTargetSettings
	{
		public GameObject TargetPrefab;
		public Vector3 TargetSpawnPoint;

		public uint MaxHealth = 100;
		public uint StartingHealth = 100;

		public uint HealthRegeneration = 10;
	}
}
