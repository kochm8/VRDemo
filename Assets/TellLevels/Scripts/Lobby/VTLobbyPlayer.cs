using UnityEngine;
using UnityEngine.Networking;

namespace CpvrLab.TellLevels.Scripts
{

    public class VTLobbyPlayer : NetworkLobbyPlayer
    {

        private string playerName;

        public override void OnClientEnterLobby()
        {
            Debug.Log("VTLobbyPlayer::OnClientEnterLobby");
            base.OnClientEnterLobby();

            VTLobbyPlayerList.instance.AddPlayer(this);
        }

        
        void Update()
        {
        }

        void OnTriggerEnter(Collider other)
        {
            VTLobbyPlayerList.instance.isReady(this);

            Debug.Log("Player ist ready");
            readyToBegin = true;
            OnClientReady(true);
            SendReadyToBeginMessage();

            other.GetComponent<Renderer>().material.color = Color.yellow;
        }
    }
}
