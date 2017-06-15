using UnityEngine;
using System.Collections;
using CpvrLab.VirtualTable;
using System;

namespace CpvrLab.ArcherGuardian.Scripts.AbilitySystem
{

    [Serializable]
    public class NpcAttribut
    {
        public int health;
        public int attackDamage;
        public int attackSpeed;
        public int walkSpeed;
    }

    [CreateAssetMenu(menuName = "Abilities/NPCSpawnAbility")]
    public class NPCSpawnAbility : Ability
    {
        public NpcAttribut npc;
        public Vector3 spawnPoint;
        private AGSpawnBehaviour _spawnObject;

        public override void Initialize(GameObject obj, PlayerInput input)
        {
            _spawnObject = obj.GetComponent<AGSpawnBehaviour>();
            if (_spawnObject == null)
            {
                _spawnObject = obj.AddComponent<AGSpawnBehaviour>();
            }
            _spawnObject.prefab = prefab;
            _spawnObject.spawnPoint = spawnPoint;
            _spawnObject.npc = npc;
        }

        public override void TriggerAbility()
        {
            _spawnObject.Spawn(spawnPoint);
        }
    }
}
