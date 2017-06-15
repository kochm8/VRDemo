using CpvrLab.ArcherGuardian.Scripts.Items;
using CpvrLab.VirtualTable;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace CpvrLab.ArcherGuardian.Scripts.Lobby
{

	public class AGLobbyPlayer : NetworkLobbyPlayer
	{
		#region members
		public LayerMask TeleportLayers;
		public PlayAreaVis PlayAreaVis;

		[HideInInspector]
		public string playerName;

		[HideInInspector]
		public bool isArcher;

		[HideInInspector]
		public bool isGuardian;

		private List<string> _playerNames = new List<string>();
		private List<Teleporter> _teleporters;
		#endregion members

		//[SyncVar(hook = "OnPlayerExit")]
		//public bool _exitsLobby;

		void Update()
		{
			if (!isLocalPlayer) return;

			if (playerName == "")
			{
				playerName = AGNetworkMenu.getPlayerName();
				CmdSetPlayerName(playerName);
			}
			if (_teleporters == null)
			{
				var gamePlayer = gameObject.GetComponent<GamePlayer>();
				if (gamePlayer == null)
				{
					Debug.LogError("Start - No GamePlayer found!");
					return;
				}

				_teleporters = new List<Teleporter>();
				foreach (var input in gamePlayer.GetAllInputs())
				{
					var teleporter = input.GetComponent<Teleporter>();
					if (teleporter == null)
						teleporter = input.gameObject.AddComponent<Teleporter>();

					teleporter.Init(TeleportLayers, Color.green, Color.red, PlayAreaVis);
					teleporter.SetPlayerInput(gamePlayer, input);

					_teleporters.Add(teleporter);
				}
			}
		}

		public override void OnClientEnterLobby()
		{
			Debug.Log("AGLobbyPlayer::OnClientEnterLobby");
			OnClientReady(false);
			base.OnClientEnterLobby();
		}

		void OnDisable()
		{
			Debug.Log("AGLobbyPlayer::OnDisable ID:" + netId);
			//Debug.LogError("isClient:" + isClient + " isServer:" + isServer + " isLocalPlayer:" + isLocalPlayer + " hasAuthority:" + hasAuthority);
		}

		public override void OnClientExitLobby()
		{
			Debug.Log("AGLobbyPlayer::OnClientExitLobby");

			AGLobbyPlayerList.instance.RemovePlayer(this);
			//gameObject.SetActive(false);
			base.OnClientExitLobby();
		}

		/*
        void OnPlayerExit(bool state)
        {
            Debug.Log("AGLobbyPlayer::OnPlayerExitsLobby");

            if (state)
            {
                CmdDisableLobbyPlayer(gameObject);
            }
        }

        [Command]
        void CmdDisableLobbyPlayer(GameObject lobbyPlayer)
        {
            lobbyPlayer.SetActive(false);
            Debug.Log("AGLobbyPlayer::CmdDisableLobbyPlayer");
            RpcDisableLobbyPlayer(lobbyPlayer);
        }

        [ClientRpc]
        void RpcDisableLobbyPlayer(GameObject lobbyPlayer)
        {
            lobbyPlayer.SetActive(false);
            Debug.LogError("AGLobbyPlayer::RpcDisableLobbyPlayer");
        }
        */

		public override void OnClientReady(bool readyState)
		{
			Debug.Log("AGLobbyPlayer::OnClientReady");

			base.OnClientReady(readyState);
		}

		void OnTriggerEnter(Collider other)
		{
			handleTriggerSelector(other);
		}

		public void handleTriggerSelector(Collider other)
		{
			if (!isLocalPlayer) return;

			//Player has already selected
			if (isArcher || isGuardian) return;

			//Check, if the role is still available
			if (!other.gameObject.GetComponent<AGPlayerSelector>().isSelectedByPlayer)
			{
				isArcher = other.gameObject.GetComponent<AGPlayerSelector>().isArcher;
				isGuardian = other.gameObject.GetComponent<AGPlayerSelector>().isGuardian;

				CmdSetGamePlayer(isArcher, isGuardian);
				CmdChangeToReadyColor(other.gameObject);
				CmdIsSelectedByPlayer(other.gameObject);

				//Send a ready message to the lobbyManager
				SendReadyToBeginMessage();
			}
		}

		[Command]
		void CmdSetGamePlayer(bool isArcher, bool isGuardian)
		{
			this.isArcher = isArcher;
			this.isGuardian = isGuardian;
		}


		[Command]
		void CmdChangeToReadyColor(GameObject obj)
		{
			obj.GetComponent<Renderer>().material.color = Color.yellow;
			RpcChangeToReadyColor(obj);
		}

		[ClientRpc]
		void RpcChangeToReadyColor(GameObject obj)
		{
			obj.GetComponent<Renderer>().material.color = Color.yellow;
		}


		[Command]
		void CmdIsSelectedByPlayer(GameObject obj)
		{
			obj.GetComponent<AGPlayerSelector>().isSelectedByPlayer = true;
			RpcIsSelectedByPlayer(obj);
		}

		[ClientRpc]
		void RpcIsSelectedByPlayer(GameObject obj)
		{
			obj.GetComponent<AGPlayerSelector>().isSelectedByPlayer = true;
		}

		[Command]
		void CmdSetPlayerName(string name)
		{
			playerName = name;

			AGLobbyPlayerList.instance.AddPlayer(this);
			List<AGLobbyPlayer> lobbyPlayerList = AGLobbyPlayerList.instance.getList();

			foreach (AGLobbyPlayer lobbyPlayer in lobbyPlayerList)
			{
				RpcShowPlayerList(lobbyPlayer.playerName);
			}

			RpcShowNames();
		}

		[ClientRpc]
		void RpcShowPlayerList(string name)
		{
			if (!_playerNames.Contains(name))
			{
				_playerNames.Add(name);
			}
		}

		[ClientRpc]
		void RpcShowNames()
		{
			AGLobbyPlayerList.instance.DisplayNames(_playerNames);
		}

	}
}