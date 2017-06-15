using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

namespace CpvrLab.VirtualTable {
    
    /// <summary>
    /// Unused, see GamePlayer
    /// </summary>
    public class NetworkPlayer : NetworkBehaviour {

        public void SetName()
        {

        }

        private void Log(string msg)
        {
            Debug.Log("NetworkPlayer(" + netId + ", " + (isLocalPlayer ? "local" : "remote") + "): " + msg);
        }

        public override bool OnCheckObserver(NetworkConnection conn)
        {
            Log("OnCheckObserver");
            return base.OnCheckObserver(conn);
        }

        public override void OnDeserialize(NetworkReader reader, bool initialState)
        {
            Log("OnDeserialize");
            base.OnDeserialize(reader, initialState);
        }

        public override void OnNetworkDestroy()
        {
            Log("OnNetworkDestroy");
            base.OnNetworkDestroy();
        }

        public override bool OnRebuildObservers(HashSet<NetworkConnection> observers, bool initialize)
        {
            Log("OnRebuildObservers");
            return base.OnRebuildObservers(observers, initialize);

        }

        public override bool OnSerialize(NetworkWriter writer, bool initialState)
        {
            Log("OnSerialize");
            return base.OnSerialize(writer, initialState);
        }

        public override void OnSetLocalVisibility(bool vis)
        {
            Log("OnSetLocalVisibility");
            base.OnSetLocalVisibility(vis);
        }

        public override void OnStartAuthority()
        {
            Log("OnStartAuthority");
            base.OnStartAuthority();
        }

        public override void OnStartClient()
        {
            Log("OnStartClient");
            base.OnStartClient();
        }

        public override void OnStartLocalPlayer()
        {
            Log("OnStartLocalPlayer");
            base.OnStartLocalPlayer();
        }

        public override void OnStartServer()
        {
            Log("OnStartServer");
            base.OnStartServer();
        }

        public override void OnStopAuthority()
        {
            Log("OnStopAuthority");
            base.OnStopAuthority();
        }

        public override void PreStartClient()
        {
            Log("PreStartClient");
            base.PreStartClient();
        }

    }

}