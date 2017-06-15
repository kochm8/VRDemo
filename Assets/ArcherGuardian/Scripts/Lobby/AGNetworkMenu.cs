using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

namespace CpvrLab.ArcherGuardian.Scripts.Lobby
{

    public class AGNetworkMenu : MonoBehaviour {


        public InputField playerNameInput;
        public InputField ipInput;
        public InputField portInput;


        private static string _playerName = "";

        void Start()
        {
            // keep track of the users choices
            string[] savedStrings = {
                "player.name", "connect.ip", "connect.port"
            };
            InputField[] fields =
            {
                playerNameInput, ipInput, portInput
            };

            for (int i = 0; i < savedStrings.Length; i++)
            {
                var identifier = savedStrings[i];
                var field = fields[i];
                var savedString = PlayerPrefs.GetString(identifier);
                if (!string.IsNullOrEmpty(savedString))
                {
                    field.text = savedString;
                }

                field.onValueChanged.AddListener(
                        value => { PlayerPrefs.SetString(identifier, value); }
                    );
            }

        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Return))
                OnHostClicked();
            if (Input.GetKeyDown(KeyCode.Space))
                OnConnectClicked();
        }

        public void OnConnectClicked()
        {
            Debug.Log("Connectclicked");
            var netMngr = NetworkManager.singleton;

            netMngr.networkAddress = ipInput.text;
            netMngr.networkPort = int.Parse(portInput.text);

            Debug.Log("Trying to connect to " + netMngr.networkAddress + ":" + netMngr.networkPort);

            netMngr.StartClient();
        }

        public void OnHostClicked()
        {
            var netMngr = NetworkManager.singleton;
            netMngr.networkPort = int.Parse(portInput.text);

            netMngr.StartHost();
        }

        public void OnNameInputChanged(string text)
        {
           AGLobbyManager netMngr = (AGLobbyManager)NetworkManager.singleton;
           netMngr.localPlayerName = text;
            _playerName = text;
        }

        public static string getPlayerName()
        {
            return _playerName;
        }
    }
}