using UnityEngine;
using System.Collections;
using CpvrLab.VirtualTable;
using System;

namespace CpvrLab.TellLevels.Scripts
{
    public class TellPlayerData : GamePlayerData
    {
        public int test;
    }


    public class TellTrainingGame : Game
    {

        public GameObject bowPrefab;

        private TellPlayerData GetConcretePlayerData(int index)
        {
            return (TellPlayerData)_playerData[index];
        }

        private TellPlayerData GetConcretePlayerData(GamePlayer player)
        {
            return (TellPlayerData)GetPlayerData(player);
        }

        protected override GamePlayerData CreatePlayerDataImpl(GamePlayer player)
        {
            return new TellPlayerData();
        }

        protected override string GetGameName()
        {
            return "TellGame";
        }



        protected override void OnInitialize()
        {
            base.OnInitialize();
            Debug.Log("TellGame: Initialized");


            for (int i = 0; i < _playerData.Count; i++)
            {
                TellPlayerData pd = GetConcretePlayerData(i);

                foreach (Transform child in pd.player.transform)
                {
                    Debug.Log(child.name);

                    //attach bow to player
                    GameObject bow = Instantiate(bowPrefab);
                    bow.transform.parent = child.transform;
                    bow.transform.localPosition = Vector3.zero;
                    bow.transform.localRotation = Quaternion.Euler(0, 90, 0);
                    bow.GetComponent<ArrowManager>().player = pd.player;
                }
            }

        }

        protected override void OnRemovePlayer(GamePlayerData dataBase)
        {

        }

        protected override void OnStop()
        {
            base.OnStop();
            Debug.Log("TellGame: Stopping");

        }

        public override bool SupportsScoreboard()
        {
            return false;
        }

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.S))
            {
                GameManager gameManager = GameManager.instance;
                gameManager.StartGame(0);
            }
        }

    }
}