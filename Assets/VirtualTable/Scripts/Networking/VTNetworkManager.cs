using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.Networking.Match;
using UnityEngine.SceneManagement;

namespace CpvrLab.VirtualTable
{
    

    public class VTMsgType
    {
        public static short AddPlayer = MsgType.Highest + 1;
        public static short StartGame = MsgType.Highest + 2;
        public static short StopGame = MsgType.Highest + 3;


        public static short ScoreBoard = MsgType.Highest + 4;
        public static short ScoreBoardRowData = MsgType.Highest + 5;

        public static short NetworkTransformTree = MsgType.Highest + 6;
    };

    public class AddPlayerMessage : MessageBase
    {
        public string name;
        public int playerPrefabIndex = 0;
    }

    /// <summary>
    /// Virtual table network manager. The current version of VirtualTable is building upon Unity's HLAPI
    /// for it's networking functionality.
    /// </summary>
    public class VTNetworkManager : NetworkManager
    {

        //public NetworkPlayer playerPrefab;
        Dictionary<NetworkConnection, GamePlayer> players = new Dictionary<NetworkConnection, GamePlayer>();

        public int networkPrefabIndex = 0;
        public GameObject[] playerPrefabs;

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
            
            // remove the player from our dictionary
            players.Remove(conn);

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
        }

        /// <summary>
        /// Called on the server when a new client connects and is requesting to be spawned as a player object.
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="playerControllerId"></param>
        /// <param name="netMsg"></param>
        public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId, NetworkReader netMsg)
        {
            //base.OnServerAddPlayer(conn, playerControllerId, netMsg);


            var msg = netMsg.ReadMessage<AddPlayerMessage>();
            Debug.Log("Adding player... " + msg.name + " " + playerPrefabs[msg.playerPrefabIndex].name);
            GameObject player = (GameObject)Instantiate(playerPrefabs[msg.playerPrefabIndex], Vector3.zero, Quaternion.identity);

            var gamePlayer = player.GetComponent<GamePlayer>();
            gamePlayer.transform.position = startPositions[numPlayers%startPositions.Count].transform.position;
            Debug.Log("Start position: " + gamePlayer.transform.position);
            Debug.Log("OnServerAddPlayer::numPlayers = " + numPlayers);
            // choose a unique user name for the player
            string uniqueName = msg.name;
            bool nameIsUnique = false;
            int nameCount = 0;
            while(!nameIsUnique)
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

            // add player to the servers dictionary
            players.Add(conn, gamePlayer);


            NetworkServer.AddPlayerForConnection(conn, player, playerControllerId);
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
            if (conn == null) return;

            base.OnClientSceneChanged(conn);
            Debug.Log("OnClientSceneChanged");          

            // spawn our player
            var msg = new AddPlayerMessage();
            msg.playerPrefabIndex = networkPrefabIndex;
            msg.name = localPlayerName;
            ClientScene.AddPlayer(conn, 0, msg);
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
            //SceneManager.LoadScene(newSceneName);
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
        public override void OnClientSceneChanged(NetworkConnection conn)
        {
            Debug.Log("NetworkManager: OnClientSceneChanged");
            base.OnClientSceneChanged(conn);
        }
        public override void OnMatchCreate(CreateMatchResponse matchInfo)
        {
            Debug.Log("NetworkManager: OnMatchCreate");
            base.OnMatchCreate(matchInfo);
        }
        public override void OnMatchList(ListMatchResponse matchList)
        {
            Debug.Log("NetworkManager: OnMatchList");
            base.OnMatchList(matchList);
        }
        public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
        {
            Debug.Log("NetworkManager: OnServerAddPlayer");
            base.OnServerAddPlayer(conn, playerControllerId);
        }
        public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId, NetworkReader extraMessageReader)
        {
            Debug.Log("NetworkManager: OnServerAddPlayer");
            base.OnServerAddPlayer(conn, playerControllerId, extraMessageReader);
        }
        public override void OnServerConnect(NetworkConnection conn)
        {
            Debug.Log("NetworkManager: OnServerConnect");
            base.OnServerConnect(conn);
        }
        public override void OnServerDisconnect(NetworkConnection conn)
        {
            Debug.Log("NetworkManager: OnServerDisconnect");
            base.OnServerDisconnect(conn);
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
        */
        public override void OnServerRemovePlayer(NetworkConnection conn, PlayerController player)
        {
            Debug.Log("NetworkManager: OnServerRemovePlayer");
            base.OnServerRemovePlayer(conn, player);
            //remove player correctly
            players.Remove(conn);
        }
        /*
        public override void OnServerSceneChanged(string sceneName)
        {
            Debug.Log("NetworkManager: OnServerSceneChanged");
            base.OnServerSceneChanged(sceneName);
        }
        public override void OnStartClient(NetworkClient client)
        {
            Debug.Log("NetworkManager: OnStartClient");
            base.OnStartClient(client);
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
        }*/
    }

}
