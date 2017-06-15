using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using System;

namespace CpvrLab.VirtualTable {

    class LightPaintingPlayerData : GamePlayerData {
        public LightPainter lightPainter = null;
    }


    /// <summary>
    /// This isn't really a "game" but more of an expericne where every player 
    /// receives a light painting UsableItem to draw into the air. We could possibly add functionality like sharing drawings, saving
    /// or other collaboration stuff. Who knows, we'll see.
    /// </summary>
    public class LightPaintingGame : Game {

        public GameObject lightPainterPrefab;

        private LightPaintingPlayerData GetConcretePlayerData(int index)
        {
            return (LightPaintingPlayerData)_playerData[index];
        }

        protected override GamePlayerData CreatePlayerDataImpl(GamePlayer player)
        {
            return new LightPaintingPlayerData();
        }

        protected override void OnInitialize()
        {
            base.OnInitialize();
            Debug.Log("LightPaintingGame: Initialized");

            // initialize our custom player data
            for(int i = 0; i < _playerData.Count; i++) {
                var pd = GetConcretePlayerData(i);
                Debug.Log("LightPaintingGame: " + i);

                // unequip all of the items the player is using
                pd.player.UnequipAll();
                
                // equip a light painter to the players main slot
                if (pd.lightPainter == null)
                {
                    // if the light painter for this player hasn't been spawned do it now
                    pd.lightPainter = Instantiate(lightPainterPrefab).GetComponent<LightPainter>();
                    NetworkServer.Spawn(pd.lightPainter.gameObject);
                }
                pd.lightPainter.isVisible = true;
                pd.player.Equip(pd.lightPainter, true);
            }           
        }

        protected override void OnRemovePlayer(GamePlayerData dataBase)
        {
            var data = (LightPaintingPlayerData)dataBase;

            NetworkServer.Destroy(data.lightPainter.gameObject);
            data.lightPainter = null;
        }

        protected override void OnStop()
        {
            base.OnStop();
            Debug.Log("LightPaintingGame: Stopping");
            

            for(int i = 0; i < _playerData.Count; i++) {
                var pd = GetConcretePlayerData(i);
                pd.player.UnequipAll();

                // hide objects
                pd.lightPainter.isVisible = false;
                
                // old version where we destroyed the light painter objects
                //NetworkServer.Destroy(pd.lightPainter.gameObject);
                //Destroy(pd.lightPainter.gameObject);
                //pd.lightPainter = null;           
            }
        }

        public override bool SupportsScoreboard()
        {
            return false;
        }

        protected override string GetGameName()
        {
            return "Light Painting";
        }
        
    }

}