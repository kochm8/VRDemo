using System;
using UnityEngine;

namespace CpvrLab.ArcherGuardian.Scripts.PlayersAndIO
{
	[Serializable]
	public class ArcherSettings : AGPlayerSettings
	{
		public ArcherSettings() : base()
		{
		}

		public GameObject BowPrefab;
		public GameObject QuiverPrefab;
    }
}
