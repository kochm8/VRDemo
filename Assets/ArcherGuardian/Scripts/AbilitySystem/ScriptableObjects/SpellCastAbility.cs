using UnityEngine;
using System.Collections;
using System;
using CpvrLab.VirtualTable;

namespace CpvrLab.ArcherGuardian.Scripts.AbilitySystem
{

    [Serializable]
    [CreateAssetMenu(menuName = "Abilities/SpellCastAbility")]
    public class SpellCastAbility : Ability
    {
        private AGSpellCastBehaviour spell;

        public override void Initialize(GameObject obj, PlayerInput input)
        {
            spell = obj.GetComponent<AGSpellCastBehaviour>();
            if (spell == null)
            {
                spell = obj.AddComponent<AGSpellCastBehaviour>();
            }
            spell.ability = this;
            spell.input = input;
            spell.Initialize();
        }

        public override void TriggerAbility()
        {
            spell.Use();
        }
    }
}
