using CpvrLab.VirtualTable;
using UnityEngine;
using UnityEngine.Assertions;

namespace CpvrLab.ArcherGuardian.Scripts
{
	public enum AGPlayerTypes
	{
		Archer,
		Guardian,
		Spectator
	}
	public class AGPlayerData : GamePlayerData
	{
		public AGPlayerTypes PlayerType { get; private set; }
		public AGPlayerData(GamePlayer gamePlayer, AGPlayerTypes playerType)
		{
			if (gamePlayer == null)
				Debug.LogError("AGPlayerData: gamePlayer not set");

			player = gamePlayer;
			PlayerType = playerType;
		}
	}
}
