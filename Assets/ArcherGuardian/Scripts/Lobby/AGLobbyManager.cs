using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.Networking.Match;
using UnityEngine.SceneManagement;
using CpvrLab.VirtualTable;
using CpvrLab.ArcherGuardian.Scripts.PlayersAndIO;
using System;

namespace CpvrLab.ArcherGuardian.Scripts.Lobby
{

    /// <summary>
    /// Virtual table network manager. The current version of VirtualTable is building upon Unity's HLAPI
    /// for it's networking functionality.
    /// </summary>
    public class AGLobbyManager : NetworkLobbyManager
    {
        public GameObject gameManagerPrefab;
        public GameObject gamePrefab;

        public int networkPrefabIndex = 0;

        public GameObject[] playerPrefabs;
        public GameObject[] lobbyPlayerPrefabs;

        public GameObject[] archerPrefabs;
        public GameObject[] guardianPrefabs;

        public Vector3 guardianSpawnPoint = new Vector3(9f, 0.36f, 8f);

        // todo: make sure players have unique names

        /// <summary>
        /// We store the local players name as a variable of the network manager. We later
        /// send the name to the server instance when the player connects to it.
        /// todo: this seems like unnecessary mixing, can we implement it in a way where we don't need to
        /// store the players name in here before connecting to a server?
        /// </summary>
        [HideInInspector]
        public string localPlayerName = "player";
        


        /// <summary>
        /// Currently an ugly workaround for players who suddenly disconnect but still have UsableItems equipped. 
        /// In this case we do the following: 
        ///     1. We find and iterate over every UsableItem in the scene. 
        ///     2. identify items that have a matching client authority with the disconnected player.
        ///     3. Remove the client authority from those items manually.
        ///     
        /// Why we do this: Items that have local client authority that belong to a client that disconnects
        ///     will be deleted by the NetworkManager. We need to prevent that.
        ///     
        /// todo: Implement this a bit more nicely. 
        ///         possible improvements: Add a static list to UsableItem on the server side that 
        ///         keeps track of all the server side intances of UsableItem. This way we don't require to run
        ///         a GameObject.Find call.
        /// </summary>
        /// <param name="conn">Connection to the disconnected client</param>
        public override void OnServerDisconnect(NetworkConnection conn)
        {
            var usableItems = FindObjectsOfType<UsableItem>();
            foreach(var item in usableItems)
            {
                var networkId = item.GetComponent<NetworkIdentity>();
                if (networkId.clientAuthorityOwner != null && conn == networkId.clientAuthorityOwner)
                {
                    networkId.RemoveClientAuthority(networkId.clientAuthorityOwner);
                }
            }

			base.OnServerDisconnect(conn);
        }
        
        /// <summary>
        /// Called on every client after connecting to a server.
        /// </summary>
        /// <param name="client"></param>
        public override void OnStartClient(NetworkClient client)
        {
            base.OnStartClient(client);

            // Register our custom game player prefabs with the client scene
            foreach (var prefab in playerPrefabs)
                ClientScene.RegisterPrefab(prefab);

            foreach (var prefab in lobbyPlayerPrefabs)
                ClientScene.RegisterPrefab(prefab);

            foreach (var prefab in archerPrefabs)
                ClientScene.RegisterPrefab(prefab);

            foreach (var prefab in guardianPrefabs)
                ClientScene.RegisterPrefab(prefab);

            ClientScene.RegisterPrefab(gameManagerPrefab);
            ClientScene.RegisterPrefab(gamePrefab);
        }

        /// <summary>
        /// Called on the server when a new client connects and is requesting to be spawned as a player object.
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="playerControllerId"></param>
        /// <param name="netMsg"></param>
        public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId, NetworkReader netMsg)
        {
            Debug.Log("NM:OnServerAddPlayer");
            base.OnServerAddPlayer(conn, playerControllerId, netMsg);
            //players.Add(conn, conn.playerControllers[0].gameObject.GetComponent<GamePlayer>());

            /*
            var msg = netMsg.ReadMessage<AddPlayerMessage>();
            Debug.Log("Adding player... " + msg.name + " " + playerPrefabs[msg.playerPrefabIndex].name);
            GameObject player = (GameObject)Instantiate(playerPrefabs[msg.playerPrefabIndex], Vector3.zero, Quaternion.identity);

            player.AddComponent<AGLobbyPlayer>();

            var gamePlayer = player.GetComponent<GamePlayer>();
            gamePlayer.transform.position = startPositions[numPlayers % startPositions.Count].transform.position;
            Debug.Log("Start position: " + gamePlayer.transform.position);
            Debug.Log("OnServerAddPlayer::numPlayers = " + numPlayers);
            // choose a unique user name for the player
            string uniqueName = msg.name;
            bool nameIsUnique = false;
            int nameCount = 0;
            while (!nameIsUnique)
            {
                nameIsUnique = true;
                foreach (var p in players)
                {
                    if (p.Value.displayName.Equals(uniqueName))
                    {
                        nameIsUnique = false;
                        nameCount++;
                        uniqueName = msg.name + " (" + nameCount + ")";
                    }

                }
            }

            gamePlayer.displayName = uniqueName;
            players.Remove(conn);


            /*
            byte slot = FindSlot();
            var newLobbyPlayer = player.GetComponent<NetworkLobbyPlayer>();
            newLobbyPlayer.slot = slot;
            lobbySlots[slot] = newLobbyPlayer;
            //

            NetworkServer.AddPlayerForConnection(conn, player, playerControllerId);
            */
        }

        public void Disconnect()
        {
            Debug.Log("NetworkManager: Disconnect");

            if (NetworkServer.active)
                StopHost();
            else
                StopClient();
        }

        /// <summary>
        /// Called on the client when ever the scene changes.
        /// 
        /// todo: Don't handle player spawning in here. We currently do it here because we want to spawn
        /// a player object as soon as we change to the online scene. However further down the road we
        /// will have more than just one scene and will eventually run into problems where this function
        /// is called for every scene. Which we don't want potentially. Rather we will not be unloading
        /// spawned player objects on scene changes.
        /// </summary>
        /// <param name="conn"></param>
        public override void OnClientSceneChanged(NetworkConnection conn)
        {
            //if (conn == null) return;

            base.OnClientSceneChanged(conn);
            Debug.Log("OnClientSceneChanged");
			/*
            

           

            // always become ready.
            ClientScene.Ready(conn);

            bool addPlayer = false;
            if (ClientScene.localPlayers.Count == 0)
            {
                // no players exist
                addPlayer = true;
            }

            bool foundPlayer = false;
            foreach (var playerController in ClientScene.localPlayers)
            {
                if (playerController.gameObject != null)
                {
                    foundPlayer = true;
                    break;
                }
            }
            if (!foundPlayer)
            {
                // there are players, but their game objects have all been deleted
                addPlayer = true;
            }
            if (addPlayer)
            {
                // spawn our player
                var msg = new AddPlayerMessage();
                msg.playerPrefabIndex = networkPrefabIndex;
                msg.name = localPlayerName;
                ClientScene.AddPlayer(conn, 0, msg);
            }*/
		}


        public override void OnServerSceneChanged(string sceneName)
        {
            base.OnServerSceneChanged(sceneName);
            Debug.Log("OnServerSceneChanged");
            // force objects already in the scene to be spawned correctly
            NetworkServer.SpawnObjects();
        }

        public override void ServerChangeScene(string newSceneName)
        {
            Debug.Log("ServerChangeScene");
            base.ServerChangeScene(newSceneName);
        }

        // the entire NetworkServer interface below, complete with debug outputs in case we should need it
        /*
        public override NetworkClient StartHost()
        {
            Debug.Log("NetworkManager: StartHost");
            return base.StartHost();
        }

        public override void ServerChangeScene(string newSceneName)
        {
            Debug.Log("NetworkManager: ServerChangeScene");
            base.ServerChangeScene(newSceneName);
        }

        public override NetworkClient StartHost(ConnectionConfig config, int maxConnections)
        {
            Debug.Log("NetworkManager: StartHost");
            return base.StartHost(config, maxConnections);
        }

        public override NetworkClient StartHost(MatchInfo info)
        {
            Debug.Log("NetworkManager: StartHost");
            return base.StartHost(info);
        }
        */
        public override void OnClientConnect(NetworkConnection conn)
        {
            Debug.Log("NetworkManager: OnClientConnect");
            base.OnClientConnect(conn);
        }
        
        public override void OnStartServer()
        {
            Debug.Log("NetworkManager: OnStartServer");
            base.OnStartServer();
        }
       
        public override void OnClientDisconnect(NetworkConnection conn)
        {
            Debug.Log("NetworkManager: OnClientDisconnect");
            base.OnClientDisconnect(conn);
        }
        
       public override void OnClientError(NetworkConnection conn, int errorCode)
       {
           Debug.Log("NetworkManager: OnClientError");
           base.OnClientError(conn, errorCode);
       }
       public override void OnClientNotReady(NetworkConnection conn)
       {
           Debug.Log("NetworkManager: OnClientNotReady");
           base.OnClientNotReady(conn);
       }
        
       public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
       {
           Debug.Log("NetworkManager: OnServerAddPlayer");
           base.OnServerAddPlayer(conn, playerControllerId);
        }

       public override void OnServerConnect(NetworkConnection conn)
       {
           Debug.Log("NetworkManager: OnServerConnect");
           base.OnServerConnect(conn);
       }

       public override void OnServerError(NetworkConnection conn, int errorCode)
       {
           Debug.Log("NetworkManager: OnServerError");
           base.OnServerError(conn, errorCode);
       }
       public override void OnServerReady(NetworkConnection conn)
       {
           Debug.Log("NetworkManager: OnServerReady");
           base.OnServerReady(conn);
       }

       public override void OnServerRemovePlayer(NetworkConnection conn, PlayerController player)
       {
           Debug.Log("NetworkManager: OnServerRemovePlayer");
           base.OnServerRemovePlayer(conn, player);
       }
        public override void OnStartHost()
        {
            Debug.Log("NetworkManager: OnStartHost");
            base.OnStartHost();
        }
        
        public override void OnStopClient()
        {
            Debug.Log("NetworkManager: OnStopClient");
            base.OnStopClient();
        }
        public override void OnStopHost()
        {
            Debug.Log("NetworkManager: OnStopHost");
            base.OnStopHost();
        }
        public override void OnStopServer()
        {
            Debug.Log("NetworkManager: OnStopServer");
            base.OnStopServer();
        }


        public override void OnLobbyClientSceneChanged(NetworkConnection conn)
        {
            Debug.Log("NM: OnLobbyClientSceneChanged");
             
            base.OnLobbyClientSceneChanged(conn);
        }

        public override void OnLobbyServerConnect(NetworkConnection conn)
        {
            Debug.Log("NM: OnLobbyServerConnect");
             
            base.OnLobbyServerConnect(conn);
        }

        public override GameObject OnLobbyServerCreateGamePlayer(NetworkConnection conn, short playerControllerId)
        {
            Debug.Log("NM: OnLobbyServerCreateGamePlayer");

            AGLobbyPlayer lobbyPlayer = conn.playerControllers[0].gameObject.GetComponent<AGLobbyPlayer>();
            GamePlayer lobbyGamePlayer = conn.playerControllers[0].gameObject.GetComponent<GamePlayer>();

            GameObject gamePlayer = null;

            if (lobbyPlayer.isGuardian)
            {
                gamePlayer = (GameObject)Instantiate(guardianPrefabs[networkPrefabIndex], guardianSpawnPoint, Quaternion.identity);
                gamePlayer.gameObject.GetComponent<GamePlayer>().modelScaleFactor = lobbyGamePlayer.modelScaleFactor;
            }

            if (lobbyPlayer.isArcher)
            {
                Transform startPos = GetStartPosition();
                gamePlayer = (GameObject)Instantiate(archerPrefabs[networkPrefabIndex], startPos.position, startPos.rotation);
                gamePlayer.gameObject.GetComponent<GamePlayer>().modelScaleFactor = lobbyGamePlayer.modelScaleFactor;
            }

            return gamePlayer;
        }

        public override GameObject OnLobbyServerCreateLobbyPlayer(NetworkConnection conn, short playerControllerId)
        {
            Debug.Log("NM: OnLobbyServerCreateLobbyPlayer");

            Transform startPos = GetStartPosition();
            GameObject LobbyPlayer = (GameObject)Instantiate(lobbyPlayerPrefabs[networkPrefabIndex], startPos.position, startPos.rotation);
            LobbyPlayer.GetComponent<GamePlayer>().displayName = localPlayerName;
            return LobbyPlayer;
        }

        public override void OnLobbyServerDisconnect(NetworkConnection conn)
        {
            Debug.Log("NM: OnLobbyServerDisconnect");
             
            base.OnLobbyServerDisconnect(conn);
        }

        public override void OnLobbyServerPlayerRemoved(NetworkConnection conn, short playerControllerId)
        {
            Debug.Log("NM: OnLobbyServerPlayerRemoved");
             
            base.OnLobbyServerPlayerRemoved(conn, playerControllerId);
        }
        //
        // Summary:
        //     ///
        //     This is called on the server when all the players in the lobby are ready.
        //     ///
        public override void OnLobbyServerPlayersReady()
        {
            Debug.Log("NM: OnLobbyServerPlayersReady");
            
            GameObject game = Instantiate(gamePrefab);
            GameObject gameManager = Instantiate(gameManagerPrefab);

            gameManager.GetComponent<GameManager>().games = new Game[] { game.GetComponent<Game>() };

            NetworkServer.Spawn(game);
            NetworkServer.Spawn(gameManager);
           
            Debug.Log("NM: Spawn GameManager on Clients");

            base.OnLobbyServerPlayersReady();
        }

        public override void OnLobbyServerSceneChanged(string sceneName)
        {
            Debug.Log("NM: OnLobbyServerSceneChanged");
             
            base.OnLobbyServerSceneChanged(sceneName);
        }
        //
        // Summary:
        //     ///
        //     This is called on the server when it is told that a client has finished switching
        //     from the lobby scene to a game player scene.
        //     ///
        //
        // Parameters:
        //   lobbyPlayer:
        //     The lobby player object.
        //
        //   gamePlayer:
        //     The game player object.
        //
        // Returns:
        //     ///
        //     False to not allow this player to replace the lobby player.
        //     ///
        public override bool OnLobbyServerSceneLoadedForPlayer(GameObject lobbyPlayer, GameObject gamePlayer)
        {
            Debug.Log("NM: OnLobbyServerSceneLoadedForPlayer");

            //lobbyPlayer.SetActive(false);

            gamePlayer.gameObject.GetComponent<GamePlayer>().displayName = lobbyPlayer.gameObject.GetComponent<AGLobbyPlayer>().playerName;
            return true;
        }


        //
        // Summary:
        //     ///
        //     This is called on the client when a client is started.
        //     ///
        //
        // Parameters:
        //   lobbyClient:
        public override void OnLobbyStartClient(NetworkClient lobbyClient)
        {
            Debug.Log("NM: OnLobbyStartClient");

            //Hide menu canvas
            GameObject.FindGameObjectWithTag(AGTags.LobbyMenu).SetActive(false);

            //Show lobby camera
            GameObject.FindGameObjectWithTag(AGTags.LobbyCamera).SetActive(false);

            base.OnLobbyStartClient(lobbyClient);
        }
        //
        // Summary:
        //     ///
        //     This is called on the host when a host is started.
        //     ///
        public override void OnLobbyStartHost()
        {
            Debug.Log("NM: OnLobbyStartHost");
             
            base.OnLobbyStartHost();
        }
        //
        // Summary:
        //     ///
        //     This is called on the server when the server is started - including when a host
        //     is started.
        //     ///
        public override void OnLobbyStartServer()
        {
            Debug.Log("NM: OnLobbyStartServer");
             
            base.OnLobbyStartServer();
        }
        //
        // Summary:
        //     ///
        //     This is called on the client when the client stops.
        //     ///
        public override void OnLobbyStopClient()
        {
            Debug.Log("NM: OnLobbyStopClient");
             
            base.OnLobbyStopClient();
        }
        //
        // Summary:
        //     ///
        //     This is called on the host when the host is stopped.
        //     ///
        public override void OnLobbyStopHost()
        {
            Debug.Log("NM: OnLobbyStopHost");
             
            base.OnLobbyStopHost();
        }
    }

}
