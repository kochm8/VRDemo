using UnityEngine;

namespace CpvrLab.ArcherGuardian.Scripts.Lobby
{
    public class AGViveSelector : MonoBehaviour
    {

        public AGLobbyPlayer lobbyPlayer;

        void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.GetComponent<AGPlayerSelector>())
            {
                lobbyPlayer.handleTriggerSelector(other);
            }
        }
    }
}
