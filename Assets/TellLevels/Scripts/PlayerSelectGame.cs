using UnityEngine;
using System.Collections;
using CpvrLab.VirtualTable;
using System;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;

public class PlayerSelectData : GamePlayerData
{

}

public class PlayerSelectGame : Game
{
    private PlayerSelectData GetConcretePlayerData(int index)
    {
        return (PlayerSelectData)_playerData[index];
    }

    private PlayerSelectData GetConcretePlayerData(GamePlayer player)
    {
        return (PlayerSelectData)GetPlayerData(player);
    }

    protected override GamePlayerData CreatePlayerDataImpl(GamePlayer player)
    {
        return new PlayerSelectData();
    }

    protected override string GetGameName()
    {
        return "Player Select";
    }

    public override bool SupportsScoreboard()
    {
        Debug.Log("PlayerSelectGame: SupportsScoreboard");
        return false;
    }


    protected override void OnInitialize()
    {
        base.OnInitialize();
        Debug.Log("PlayerSelectGame: OnInitialize");

        for (int i = 0; i < _playerData.Count; i++)
        {
            PlayerSelectData pd = GetConcretePlayerData(i);
        }

    }

    [ServerCallback]
    public void LoadOnline(string sceneName)
    {

        NetworkManager.singleton.ServerChangeScene(sceneName);

    }

    // Update is called once per frame
    void Update() {
        /*
        if (!isServer) return;

        for (int i = 0; i < _playerData.Count; i++)
        {
            PlayerSelectData pd = GetConcretePlayerData(i);
            GameManager.instance.RemovePlayer(pd.player);
        }

        if (Input.GetKeyDown(KeyCode.Return))
        {
            Debug.Log("Load Scene OnlineGame !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");

            NetworkManager.singleton.ServerChangeScene("OnlineGame");            
        }
        */
    }
}
