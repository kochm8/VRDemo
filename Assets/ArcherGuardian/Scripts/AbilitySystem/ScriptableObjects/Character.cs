using UnityEngine;

namespace CpvrLab.ArcherGuardian.Scripts.AbilitySystem
{
    [CreateAssetMenu(menuName = "Abilities/Character")]
    public class Character : ScriptableObject
    {
        public Ability[] abilities;
    }
}