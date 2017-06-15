using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.Networking;

namespace CpvrLab.ArcherGuardian.Scripts.Lobby
{

    public class AGLobbyPlayerList : NetworkBehaviour
    {
        public static AGLobbyPlayerList instance = null;
        public RectTransform playerListContentTransform;
        public Text playerEntryPrefab;

        private List<AGLobbyPlayer> _players = new List<AGLobbyPlayer>();

        public void OnEnable()
        {
            instance = this;
        }

        public List<AGLobbyPlayer> getList()
        {
            return _players;
        }

        public void AddPlayer(AGLobbyPlayer player)
        {
            if (_players.Contains(player)) return;

            _players.Add(player);
        }

        public void RemovePlayer(AGLobbyPlayer player)
        {
            _players.Remove(player);
        }

        public void DisplayNames(List<string> names)
        {
            int count = 0;

            foreach (string name in names)
            {
                count++;

                Text plEntry = Instantiate(playerEntryPrefab);
                plEntry.text = name;
                plEntry.transform.SetParent(playerListContentTransform);
                plEntry.GetComponent<RectTransform>().localScale = new Vector3(0.5f, 0.5f, 0.5f);
                plEntry.GetComponent<RectTransform>().localPosition = new Vector3(0, 0, 0);
                plEntry.GetComponent<RectTransform>().anchoredPosition = new Vector3(0.0f, (-40) + (count * -15), 0.0f);
                plEntry.color = Color.blue;
                plEntry.fontSize = 20;
            }
        }
    }
}