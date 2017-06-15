using CpvrLab.TellLevels.Scripts.PlayersAndIO;
using CpvrLab.VirtualTable;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace CpvrLab.TellLevels.Scripts
{
	/// <summary>
	/// Walter training game is played as follows: The player can pickup a shield 
	/// and try to defend himself by blocking as many incoming arrows as possible.
	/// </summary>
	public class WalterTrainingGame : Game
	{
		#region public variables
		public GameObject _arrowPrefab;
		public int _countdownTime = 5;
		public int _arrowCount = 10;
		public int _spawnTime = 2;
		public Vector2 _spawnExtents = new Vector2(1, 1);

		public Text _gameStatusText;

		public GuardianSettings _guardianSettings;
		#endregion public variables

		private GameObject[] _arrows;

		private WalterPlayerData GetConcretePlayerData(int index)
		{
			return (WalterPlayerData)_playerData[index];
		}

		private WalterPlayerData GetConcretePlayerData(GamePlayer player)
		{
			return (WalterPlayerData)GetPlayerData(player);
		}

		protected override GamePlayerData CreatePlayerDataImpl(GamePlayer player)
		{
			return new WalterPlayerData(player);
		}

		protected override void OnInitialize()
		{
			base.OnInitialize();

			Debug.Log("WalterTrainingGame: OnInitialize - is server: " + isServer.ToString());

			_arrows = new GameObject[_arrowCount];

			if (!CheckGameState())
			{
				RpcSetStatusText("Player's aren't ready!");
				return;
			}

			// initialize our custom player data
			for (int i = 0; i < _playerData.Count; i++)
			{
				var pd = GetConcretePlayerData(i);

				// unequip all of the items the player is using
				// pd.player.UnequipAll();

				// todo: disable item pickups for the player

				//if (pd.gun == null)
				//{
				//	pd.gun = Instantiate(gunPrefab).GetComponent<PrototypeGun>();
				//	NetworkServer.Spawn(pd.gun.gameObject);
				//}

				//pd.gun.isVisible = true;
				//pd.player.Equip(pd.gun, true);

				if (pd.IsGuardian)
				{
					// Attach Guardian script (this is only attached on the server)
					var guardian = pd.player.gameObject.AddComponent<Guardian>();
					guardian.Init(pd.player, _guardianSettings);
				}
			}

			RpcSetStatusText("Get ready!");
			StartCoroutine(Countdown());
			StartCoroutine(HandleSpawning());
			UpdateScoreGUI();
		}
		private bool CheckGameState()
		{
			// Check for at least one guardian
			for (int i = 0; i < _playerData.Count; i++)
			{
				var pd = GetConcretePlayerData(i);
				if (pd.IsGuardian)
				{
					return true;
				}
			}
			return false;
		}
		protected override void OnStop()
		{
			base.OnStop();
			StopAllCoroutines();

			for (int i = 0; i < _playerData.Count; i++)
			{
				var pd = GetConcretePlayerData(i);
				pd.player.UnequipAll();

				if (pd.IsGuardian)
				{
					var guardian = pd.player.gameObject.GetComponent<Guardian>();
					if (guardian != null)
						guardian.RemoveComponent();
				}
				foreach (var arrow in _arrows)
				{
					if (arrow != null)
						NetworkServer.Destroy(arrow);
				}

				// hide objects
				//if (pd.gun != null)
				//	pd.gun.isVisible = false;
			}
		}

		private IEnumerator HandleSpawning()
		{
			yield return new WaitForSeconds(_countdownTime);
			float radius = 8f;
			for (int arrowsFired = 0; arrowsFired < _arrowCount; arrowsFired++)
			{
				float angle = arrowsFired * Mathf.PI * 2 / _arrowCount;
				Vector3 spawnPoint = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * radius;

				var arrowGO = (GameObject)Instantiate(_arrowPrefab, spawnPoint, Quaternion.identity);
				NetworkServer.Spawn(arrowGO);

				_arrows[arrowsFired] = arrowGO;

				var trainingArrow = arrowGO.GetComponent<TrainingArrow>();
				trainingArrow.CmdSetTarget(_playerData[0].player.transform.position);
				trainingArrow.CmdShootArrow();
				yield return new WaitForSeconds(_spawnTime);
			}
		}
		private void EnableInput(bool enable)
		{
			foreach (var pd in _playerData)
			{
				//((WalterTrainingPlayerData)pd).gun.inputEnabled = enable;
			}
		}

		private IEnumerator Countdown()
		{
			float timer = _countdownTime;

			while (timer > 0.0f)
			{
				RpcSetStatusText("Get ready: " + timer.ToString("F2"));
				timer -= Time.deltaTime;

				yield return null;
			}

			RpcSetStatusText("Fire!");
			EnableInput(true);
		}

		[ClientRpc]
		void RpcSetStatusText(string text)
		{
			_gameStatusText.text = text;
		}

		IEnumerator AutoRestartGame(float waitTime)
		{
			yield return new WaitForSeconds(waitTime);
			HandleSpawning();
		}

		protected override string GetGameName()
		{
			return "Walter Training Game";
		}

		protected override string[] GetScoreHeaders()
		{
			return new string[]
			{
				"Player", "Rounds Won", "Best Time"
			};
		}

		protected override string[] GetScoreValues(int playerIndex)
		{
			var pd = GetConcretePlayerData(playerIndex);

			return new string[]
			{
				pd.player.displayName,
				//pd.roundsWon.ToString(),
				//((pd.bestTime > 0.0f) ? pd.bestTime.ToString("F2") + "s" : "NAN")
			};
		}
	}
}