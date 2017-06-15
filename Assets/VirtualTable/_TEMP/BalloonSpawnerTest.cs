using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class BalloonSpawnerTest : NetworkBehaviour {

    public bool spawn = true;
    public float spawnInterval = 4;
    public float maxLifeTime = 20.0f;
    public GameObject balloonPrefab;

    public Transform[] spawnPositions;

    public override void OnStartServer()
    {
        base.OnStartServer();

        StartCoroutine(SpawnCoroutine());
    }

    IEnumerator SpawnCoroutine()
    {
        while(spawn)
        {
            yield return new WaitForSeconds(spawnInterval);
            SpawnBalloon();
        }
    }
    
    void SpawnBalloon()
    {
        var go = Instantiate(balloonPrefab);

        go.transform.position = spawnPositions[Random.Range(0, spawnPositions.Length)].position;
        NetworkServer.Spawn(go);
    }
}
