using System;
using UnityEngine;

namespace CpvrLab.ArcherGuardian.Scripts.PlayersAndIO
{
	[Serializable]
	public class GuardianSettings : AGPlayerSettings
	{
		public GuardianSettings() : base()
		{
		}

		public GameObject ShieldPrefab;

        //public AbilitySettings[] guardianAbilites;

        //public ZombieAbility zombieAbility;
    }
}
