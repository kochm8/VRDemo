using UnityEngine;
using System.Collections.Generic;
using System;

namespace CpvrLab.VirtualTable {


    class ConnectFourPlayerData : GamePlayerData {

    }

    /// <summary>
    /// Simple connect four game where players have to drop stones of their color into a 
    /// playing field until they manage to get four in a row.
    /// </summary>
    public class ConnectFourGame : Game {

        private ConnectFourPlayerData GetConcretePlayerData(int index)
        {
            return (ConnectFourPlayerData)_playerData[index];
        }

        protected override GamePlayerData CreatePlayerDataImpl(GamePlayer player)
        {
            return new ConnectFourPlayerData();
        }

        protected override void OnInitialize()
        {
            base.OnInitialize();
            Debug.Log("ConnectFourGame: Initialized");
        }

        public override void OnUpdate()
        {
            base.OnUpdate();
        }

        protected override string GetGameName()
        {
            throw new NotImplementedException();
        }

        protected override string[] GetScoreHeaders()
        {
            throw new NotImplementedException();
        }

        protected override string[] GetScoreValues(int playerIndex)
        {
            throw new NotImplementedException();
        }
        
    }

}