using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System;
using CpvrLab.ArcherGuardian.Scripts.GameLogic;

namespace CpvrLab.ArcherGuardian.Scripts.AbilitySystem
{

    public class AGSpawnBehaviour : NetworkBehaviour
    {
        [HideInInspector]
        public NpcAttribut npc;
        [HideInInspector]
        public GameObject prefab;
        [HideInInspector]
        public Vector3 spawnPoint;

        public void Spawn(Vector3 spawnPoint)
        {
            if (!isLocalPlayer) return;

            CmdSpawn();
        }

        [Command]
        public void CmdSpawn()
        {
            var obj = Instantiate(prefab, spawnPoint, Quaternion.identity) as GameObject;
            obj.GetComponent<AGSpawnBehaviour>().npc = npc;
            NetworkServer.Spawn(obj);
        }
    }
}




