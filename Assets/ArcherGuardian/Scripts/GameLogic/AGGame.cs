using CpvrLab.ArcherGuardian.Scripts.PlayersAndIO;
using CpvrLab.VirtualTable;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace CpvrLab.ArcherGuardian.Scripts.GameLogic
{
	/// <summary>
	/// Class with the global game logic.
	/// </summary>
	public class AGGame : Game
	{
		#region members
		#region public
		public int CountdownTime = 5;
		public int GameTime = 300;
		public int RemainingTimeWarning = 10;
		public Text GameStatusText;

		public ArcherSettings ArcherSettings;
		public GuardianSettings GuardianSettings;
		public AGTargetSettings TargetSettings;
		#endregion public
		private AGTarget _target;
		#endregion members

		#region overrides
		protected override void OnInitialize()
		{
			Debug.Log("Game: Initialize - is server: " + isServer.ToString());
			base.OnInitialize();

			if (!isServer)
				return;

			// Spawn the target.
			var targetGO = (GameObject)Instantiate(TargetSettings.TargetPrefab, TargetSettings.TargetSpawnPoint, Quaternion.identity);
			NetworkServer.Spawn(targetGO);
			_target = targetGO.GetComponent<AGTarget>();
			_target.SetSettings(TargetSettings);
			_target.TargetDestroyed += _target_TargetDestroyed;

			if (CheckGameState())
			{
				RpcSetStatusText("Get ready!");

				StartCoroutine(Countdown());
			}

		}

		protected override void OnStop()
		{
			base.OnStop();

			StopAllCoroutines();

			// Destroy the target
			_target.TargetDestroyed -= _target_TargetDestroyed;
			Destroy(_target);
			_target = null;
		}

		protected override GamePlayerData CreatePlayerDataImpl(GamePlayer player)
		{
			// create player data
			AGPlayerTypes playerType;

			if (player.gameObject.GetComponent<Archer>() != null)
			{
				playerType = AGPlayerTypes.Archer;
			}
			else if (player.gameObject.GetComponent<Guardian>() != null)
			{
				playerType = AGPlayerTypes.Guardian;
			}
			else
			{
				playerType = AGPlayerTypes.Spectator;
			}

			return new AGPlayerData(player, playerType);
		}

		public override bool SupportsScoreboard()
		{
			return false;
		}
		#endregion overrides
		public void Awake()
		{
			DontDestroyOnLoad(this);
		}

		private bool CheckGameState()
		{
			return true;
		}
		private IEnumerator GameTimer()
		{
			yield return new WaitForSeconds(GameTime - RemainingTimeWarning);

			float countDown = RemainingTimeWarning;
			while (countDown > 0.0f)
			{
				RpcSetStatusText(countDown.ToString("F2"));
				countDown -= Time.deltaTime;

				yield return null;
			}

			RpcSetStatusText("Game over - Guardian won!");

			Stop();
		}
		private IEnumerator Countdown()
		{
			float timer = CountdownTime;

			while (timer > 0.0f)
			{
				RpcSetStatusText("Get ready: " + timer.ToString("F2"));
				timer -= Time.deltaTime;

				yield return null;
			}

			RpcSetStatusText("Fire!");

			// Initialize players and start the game timer.
			PreparePlayers();
			StartCoroutine(GameTimer());
		}
		private void PreparePlayers()
		{
			foreach (AGPlayerData playerData in _playerData)
			{
				// Initialize on the local player as well
				if (!playerData.player.isLocalPlayer)
				{
					TargetPrepareLocalPlayer(playerData.player.connectionToClient, playerData.player.gameObject);
				}
				switch (playerData.PlayerType)
				{
					case AGPlayerTypes.Archer:
						// Get the Archer script
						var archer = playerData.player.gameObject.GetComponent<Archer>();
						archer.InitFromSettings(ArcherSettings);
						break;
					case AGPlayerTypes.Guardian:
						// Get the Guardian script
						var guardian = playerData.player.gameObject.GetComponent<Guardian>();
						guardian.InitFromSettings(GuardianSettings);
						break;
				}
			}
		}
		/// <summary>
		/// Add an item to the inventory of the player.
		/// </summary>
		/// <param name="target">Client connection</param>
		/// <param name="player"><seealso cref="GameObject"/> of the player.</param>
		[TargetRpc]
		protected void TargetPrepareLocalPlayer(NetworkConnection target, GameObject player)
		{
			// Check if archer
			var archer = player.GetComponent<Archer>();
			if (archer != null)
			{
				archer.InitFromSettings(ArcherSettings);
				return;
			}
			// Check if guardian
			var guardian = player.GetComponent<Guardian>();
			if (guardian != null)
			{
				guardian.InitFromSettings(GuardianSettings);
				return;
			}

			Debug.LogError("TargetPrepareLocalPlayer - invalid player!");
		}
		[ClientRpc]
		private void RpcSetStatusText(string text)
		{
			GameStatusText.text = text;
		}

		private void _target_TargetDestroyed(object sender, TargetDestroyedEventArgs e)
		{
			RpcSetStatusText("Game over - Archer won!");
			Stop();
		}
	}
}