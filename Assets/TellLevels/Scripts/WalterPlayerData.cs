using CpvrLab.VirtualTable;
using UnityEngine.Assertions;

namespace CpvrLab.TellLevels.Scripts
{
	public class WalterPlayerData : GamePlayerData
	{
		public bool IsGuardian { get; set; }
		public WalterPlayerData(GamePlayer gamePlayer)
		{
			Assert.IsNotNull(gamePlayer, "gamePlayer not set");
			
			player = gamePlayer;
			IsGuardian = true;
		}
	}
}
