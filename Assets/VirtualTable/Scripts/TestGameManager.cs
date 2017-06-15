using UnityEngine;
using UnityEngine.Networking;
using System.Collections;


namespace CpvrLab.VirtualTable {

    /// <summary>
    /// This game manager was used for early tests. Will probably be removed pretty soon.
    /// </summary>
    public class TestGameManager : GameManager {
                

        protected override void GameFinished(Game game)
        {
            base.GameFinished(game);
            Debug.Log("TestGameManager: Game finished " + game.name);
        }
    }

}