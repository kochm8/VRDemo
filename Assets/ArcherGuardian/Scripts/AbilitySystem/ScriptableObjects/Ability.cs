using UnityEngine;
using System;
using CpvrLab.VirtualTable;
using System.ComponentModel;

namespace CpvrLab.ArcherGuardian.Scripts.AbilitySystem
{

    public class AbiltyEventArgs : CancelEventArgs
    {
        public AbiltyEventArgs(int cost)
        {
            Cost = cost;
        }
        public int Cost { get; private set; }
    }

    [Serializable]
    public abstract class Ability : ScriptableObject
    {
        public event EventHandler<AbiltyEventArgs> UseAbility;
        public PlayerInput.ActionCode actioncode;
        public string aname = "AbilityName";
        public int cost = 1;
        public GameObject prefab;

        public abstract void Initialize(GameObject obj, PlayerInput input);
        public abstract void TriggerAbility();

        public bool checkInput(PlayerInput input)
        {
            bool action = (input.GetActionDown(actioncode));
            if (action)
            {
                var args = new AbiltyEventArgs(cost);
                UseAbility(this, args);
                if (args.Cancel)
                {
                    action = false;
                }
            }
            return action;
        }
    }
}