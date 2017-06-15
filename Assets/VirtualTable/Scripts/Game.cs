using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System;

namespace CpvrLab.VirtualTable {
    
    /// <summary>
    /// Base class for game player data. This class contains at least a reference to the GamePlayer
    /// that this data is describing. A concrete implementation of the class could keep track
    /// of player score or other relevant information a concrete game could need to store and display.
    /// 
    /// todo: Do we really need GamePlayer as a member of the data? Wouldn't a dictionary in the base
    /// Game class work much better for our usecases? Change if necessary.
    /// </summary>
    public class GamePlayerData {
        public GamePlayer player;
    }

    /// <summary>
    /// Base class for all game implementations.
    /// 
    /// todo:   for now a Game is a MonoBehaviour so that we can edit it easier in the editor
    ///         but it would be great if we could edit the game rules etc in a custom list editor
    ///         in the game manager editor itself where each game exposes its settings
    ///         via a property drawer.
    /// </summary>
    public abstract class Game : NetworkBehaviour {

        public string gameName;
        public event Action<Game> GameFinished;

        public float gameTime { get { return _gameTime; } }

        protected bool _usingCustomScene = false;
        protected string _customSceneName = "";
        protected bool _isRunning = false;
        protected List<GamePlayerData> _playerData = new List<GamePlayerData>();

        protected int hubSceneIndex;

        // note: player limit is ignored for now
        protected bool _hasPlayerLimit;
        protected int _minPlayers;
        protected int _maxPlayers;

        protected float _gameTime;
        

        public override void OnStartClient()
        {
            base.OnStartClient();
            ShowChildren(false);
        }


        public void ClearPlayerList()
        {
            _playerData.Clear();
        }

        protected GamePlayerData GetPlayerData(GamePlayer player)
        {
            return _playerData.Find(e => e.player == player);
        }

        // Add a player to the game player list
        public void AddPlayer(GamePlayer player)
        {
            var element = _playerData.Find(e => e.player == player);

            if(element == null)
                _playerData.Add(CreatePlayerData(player));
            else
                Debug.LogWarning("Game WARNING: Careful, someone tried to add an already existing player to our list!");
        }

        public void AddPlayers(GamePlayer[] players)
        {
            foreach(var p in players) {
                AddPlayer(p);
            }
        }

        // remove player from game player list
        public void RemovePlayer(GamePlayer player)
        {
            var element = _playerData.Find(e => e.player == player);

            OnRemovePlayer(element);

            if (element != null)
                _playerData.Remove(element);
            else
                Debug.LogWarning("Game WARNING: Careful, someone tried to remove a player that is not in the list!");
        }

        /// <summary>
        /// Use this function in your concrete implementation to clean up anything related
        /// to player data
        /// </summary>
        protected virtual void OnRemovePlayer(GamePlayerData data)
        {
        }
        
        public void RemovePlayers(GamePlayer[] players)
        {
            foreach(var p in players) {
                RemovePlayer(p);
            }
        }

        public void RemoveAllPlayers()
        {
            foreach(var p in _playerData)
                RemovePlayer(p.player);
        }

        private GamePlayerData CreatePlayerData(GamePlayer player)
        {
            var data = CreatePlayerDataImpl(player);
            data.player = player;

            return data;
        }

        protected abstract GamePlayerData CreatePlayerDataImpl(GamePlayer player);

        public void Initialize()
        {
            if (!isServer)
                return;

            if(_usingCustomScene) {
                // todo: load scene gracefully (async) 
                SceneManager.LoadScene(_customSceneName);
            }

            // reset game time
            _gameTime = 0.0f;

            // initialize score board
            if(SupportsScoreboard())
                InitScoreBoard();


            OnInitialize();
        }

        // stops the current game gracefully
        public void Stop()
        {
            OnStop();

            // notify others about the game ending
            if(GameFinished != null)
                GameFinished(this);
        }

        /// <summary>
        /// We use this function to hide and show child objects of a game (set game object active/inactive) 
        /// I'm not sure if this is the best solution for now we'll leave it be.
        /// </summary>
        /// <param name="hide"></param>
        [ClientRpc] void RpcShowChildren(bool show)
        {
            ShowChildren(show);
        }

        protected void ShowChildren(bool show)
        {
            for (int i = 0; i < transform.childCount; i++)
                transform.GetChild(i).gameObject.SetActive(show);
        }
        
        protected virtual void OnInitialize() {
            RpcShowChildren(true);
        }
        protected virtual void OnStop()
        {
            RpcShowChildren(false);
        }

        public virtual void OnUpdate()
        {
            if (!isServer) return;
            _gameTime += Time.fixedDeltaTime;
        }

        protected virtual void ClearStats()
        {
        }

        protected virtual string GetGameName() { return gameName; }
        public virtual bool SupportsScoreboard() { return true; }
        protected virtual string[] GetScoreHeaders() { return null; }
        protected virtual string[] GetScoreValues(int playerIndex) { return null; }
        protected virtual float[] GetScoreDimensionRatios()
        {
            var columns = GetScoreHeaders().Length;
            float ratio = 1.0f / columns;
            float[] ratios = new float[columns];
            for(int i = 0; i < columns; i++)
                ratios[i] = ratio;

            return ratios;
        }

        protected virtual void UpdateScoreGUI()
        {
            var sb = GameManager.instance.scoreBoardData;
            for (int i = 0; i < _playerData.Count; i++)
            {
                sb.SetRowData(i, GetScoreValues(i));
            }
        }

        protected virtual void InitScoreBoard()
        {
            var sb = GameManager.instance.scoreBoardData;
            sb.ClearData();
            sb.SetTitle(GetGameName());
            sb.SetHeaders(GetScoreHeaders());
            for(int i = 0; i < _playerData.Count; i++)
            {
                sb.AddRow(GetScoreValues(i));
            }
        }
    }

}