using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using UnityEngine.Networking.Match;

namespace CpvrLab.VirtualTable {

    // test class to see how unity's networkmanager works
    public class TestNetworkManager : NetworkManager {

        private void Log(string msg)
        {
            Debug.Log("NetworkManager: " + msg);
        }

        public override NetworkClient StartHost()
        {
            Log("StartHost");
            return base.StartHost();
        }

        public override void ServerChangeScene(string newSceneName)
        {
            Log("ServerChangeScene");
            base.ServerChangeScene(newSceneName);
        }

        public override NetworkClient StartHost(ConnectionConfig config, int maxConnections)
        {
            Log("StartHost");
            return base.StartHost(config, maxConnections);
        }

        public override NetworkClient StartHost(MatchInfo info)
        {
            Log("StartHost");
            return base.StartHost(info);
        }

        public override void OnClientConnect(NetworkConnection conn)
        {
            Log("OnClientConnect");
            base.OnClientConnect(conn);
        }

        public override void OnStartServer()
        {
            Log("OnStartServer");
            base.OnStartServer();
        }

        public override void OnClientDisconnect(NetworkConnection conn)
        {
            Log("OnClientDisconnect");
            base.OnClientDisconnect(conn);
        }

        public override void OnClientError(NetworkConnection conn, int errorCode)
        {
            Log("OnClientError");
            base.OnClientError(conn, errorCode);
        }
        public override void OnClientNotReady(NetworkConnection conn)
        {
            Log("OnClientNotReady");
            base.OnClientNotReady(conn);
        }
        public override void OnClientSceneChanged(NetworkConnection conn)
        {
            Log("OnClientSceneChanged");
            base.OnClientSceneChanged(conn);
        }

        public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
        {
            Log("OnServerAddPlayer");
            base.OnServerAddPlayer(conn, playerControllerId);
        }
        public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId, NetworkReader extraMessageReader)
        {
            Log("OnServerAddPlayer");
            base.OnServerAddPlayer(conn, playerControllerId, extraMessageReader);
        }
        public override void OnServerConnect(NetworkConnection conn)
        {
            Log("OnServerConnect");
            base.OnServerConnect(conn);
        }
        public override void OnServerDisconnect(NetworkConnection conn)
        {
            Log("OnServerDisconnect");
            base.OnServerDisconnect(conn);
        }
        public override void OnServerError(NetworkConnection conn, int errorCode)
        {
            Log("OnServerError");
            base.OnServerError(conn, errorCode);
        }
        public override void OnServerReady(NetworkConnection conn)
        {
            Log("OnServerReady");
            base.OnServerReady(conn);
        }
        public override void OnServerRemovePlayer(NetworkConnection conn, PlayerController player)
        {
            Log("OnServerRemovePlayer");
            base.OnServerRemovePlayer(conn, player);
        }
        public override void OnServerSceneChanged(string sceneName)
        {
            Log("OnServerSceneChanged");
            base.OnServerSceneChanged(sceneName);
        }
        public override void OnStartClient(NetworkClient client)
        {
            Log("OnStartClient");
            base.OnStartClient(client);
        }
        public override void OnStartHost()
        {
            Log("OnStartHost");
            base.OnStartHost();
        }
        public override void OnStopClient()
        {
            Log("OnStopClient");
            base.OnStopClient();
        }
        public override void OnStopHost()
        {
            Log("OnStopHost");
            base.OnStopHost();
        }
        public override void OnStopServer()
        {
            Log("OnStopServer");
            base.OnStopServer();
        }
    }

}
