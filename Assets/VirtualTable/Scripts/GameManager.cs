using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using System;
using UnityEngine.SceneManagement;

namespace CpvrLab.VirtualTable {

    public class StartGameMessage : MessageBase
    {
        public int gameIndex = -1;
    }
    public class StopGameMessage : MessageBase
    { }


    /// <summary>
    /// The GameManager is responsible of loading and unloading games and everything related to it.
    /// If a game requires a scene change then this is also handled here. If a game is over the 
    /// game manager is also responsible of displaying the games results by rendering the games
    /// GamePlayerData in list form (this last part is still only an idea for the future)
    /// 
    /// note:   at the time of writing this comment the GameManager class is still pretty much untested
    ///         and may change immensely over the next few iterations.
    /// </summary>
    public class GameManager : NetworkBehaviour {

        public static GameManager instance { get { return _instance; } }
        private static GameManager _instance = null;

        public ScoreBoard scoreBoardData;
        public Game[] games;
        protected Game _currentGame;

        public Action<Game> OnGameChanged;

        public Game currentGame { get { return _currentGame; } }

        // list of players currently able to play
        protected List<GamePlayer> _players = new List<GamePlayer>();

        // dirty flag in case the players list has changed
        protected bool _dirty = false;

        void Awake()
        {
            if(_instance != null) {
                Debug.LogError("GameManager: You are trying to instantiate multiple game managers, only one is allowed!");
                DestroyImmediate(this);
                return;
            }
            _instance = this;

            Debug.Log("GameManager: Awake");
            DontDestroyOnLoad(gameObject);
        }

        public override void OnStartClient()
        {
            base.OnStartClient();


            OnInitialize();
        }

        public override void OnStartServer()
        {
            base.OnStartServer();

            Debug.Log("Gamemanager::OnStartServer");

            NetworkServer.RegisterHandler(VTMsgType.StartGame, StartGameMsgHandler);
            NetworkServer.RegisterHandler(VTMsgType.StopGame, StopGameMsgHandler);

			if (scoreBoardData != null)
			{
				scoreBoardData.Show(false);
			}
        }


        protected virtual void OnInitialize() { }

        public void AddPlayer(GamePlayer player)
        {
            if (!_players.Contains(player))
            {
                _players.Add(player);
                _dirty = true;
            }
            else
                Debug.LogError("GameManager: Trying to register the same player twice!");
        }

        public void AddPlayers(GamePlayer[] players)
        {
            Debug.LogError("GameManager: AddPlayers");
            foreach (var p in players) {
                AddPlayer(p);
            }
        }

        public void RemovePlayer(GamePlayer player)
        {
            if (_players.Contains(player))
            {
                _players.Remove(player);
                _dirty = true;
            }
        }

        public void RemovePlayers(GamePlayer[] players)
        {
            foreach(var p in players) {
                RemovePlayer(p);
            }
        }
        
        public void StartGame(int index)
        {
            if (isServer)
            {
                StartGameInternal(index);
            }
            else
            {
                var netMngr = NetworkManager.singleton as NetworkManager; //VTNetworkManager
                var msg = new StartGameMessage();
                msg.gameIndex = index;
                netMngr.client.Send(VTMsgType.StartGame, msg);
            }
        }
        
        [Server] private void StartGameMsgHandler(NetworkMessage netMsg)
        {
            Debug.Log("GameManager::StartGameMsgHandler");
            var msg = netMsg.ReadMessage<StartGameMessage>();
            StartGameInternal(msg.gameIndex);
        }


        [Server] public void StartGameInternal(int index)
        {
            //Scale the PlayerModel
            foreach(GamePlayer gamePlayer in _players)
            {
                gamePlayer.ScaleModel();
            }

            Debug.Log("GameManager::StartGameInternal");
            if (index < 0 && games.Length <= index)
            {
                Debug.LogError("GameManager: Trying to load a game with an invalid index.");
                return;
            }
            
            StartGame(games[index]);


			if (scoreBoardData != null)
			{
				scoreBoardData.Show(games[index].SupportsScoreboard());
			}
            if (OnGameChanged != null)
                OnGameChanged(games[index]);
        }

        // todo:    allow for graceful starting and stopping of games
        //          use a callback paradigm to call start game as soon
        //          as the previous game has unloaded
        //          
        [Server] void StartGame(Game game)
        {
            // stop current game
            StopGame();

			// Update game players.
			UpdateGamePlayers();

			_currentGame = game;
            _currentGame.Initialize();

            _currentGame.GameFinished += GameFinished;
        }

        public void StopGame()
        {
            CmdStopGame();
        }

        [Command] void CmdStopGame()
        {
            if (_currentGame == null)
                return;

            _currentGame.Stop();

			if (scoreBoardData != null)
			{
				scoreBoardData.Show(false);
			}
            // only gets sent on the server!
            if (OnGameChanged != null)
                OnGameChanged(null);
        }

        protected virtual void GameFinished(Game game)
        {
            _currentGame = null;
        }

        void FixedUpdate()
		{
			if (!isServer)
				return;

			// todo: this dirty flag stuff is temporary... probably
			UpdateGamePlayers();

			// advance current game logic
			if (_currentGame != null)
				_currentGame.OnUpdate();
		}

		private void UpdateGamePlayers()
		{
			if (_dirty && _currentGame == null)
			{
				foreach (var game in games)
				{
					game.ClearPlayerList();
					game.AddPlayers(_players.ToArray());
				}

				_dirty = false;
			}
		}

		[Server] private void StopGameMsgHandler(NetworkMessage netMsg)
        {
            StopGame();
        }
    }

}