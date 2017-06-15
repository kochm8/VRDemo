using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using System;

namespace CpvrLab.TellLevels.Scripts
{

    public class VTLobbyPlayerList : MonoBehaviour
    {
        public static VTLobbyPlayerList instance = null;
        public RectTransform playerListContentTransform;
        public Text playerEntryPrefab;
        private List<VTLobbyPlayer> _players = new List<VTLobbyPlayer>();


        public void OnEnable()
        {
            instance = this;
        }

        public void AddPlayer(VTLobbyPlayer player)
        {

            Debug.Log("VTLobbyPlayerList::AddPlayer");

            if (_players.Contains(player))
                return;

            _players.Add(player);
            
            Text plEntry = Instantiate(playerEntryPrefab);
            plEntry.text = player.GetInstanceID().ToString() + player.name;
            plEntry.transform.SetParent(playerListContentTransform);

            Vector3 pos = plEntry.transform.position;
            plEntry.GetComponent<RectTransform>().anchoredPosition = new Vector3(pos.x, _players.Count * -30, pos.z);

            plEntry.color = Color.green;
            plEntry.fontSize = 15;
        }

        public void RemovePlayer(VTLobbyPlayer player)
        {
            _players.Remove(player);
        }

        public void isReady(VTLobbyPlayer player)
        {
            foreach(VTLobbyPlayer pl in _players)
            {
                if (pl.Equals(player))
                {
                    
                }
            }
        }

    }
}