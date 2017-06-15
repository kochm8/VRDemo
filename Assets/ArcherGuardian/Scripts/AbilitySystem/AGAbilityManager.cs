using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using CpvrLab.VirtualTable;

namespace CpvrLab.ArcherGuardian.Scripts.AbilitySystem
{

    [RequireComponent(typeof(AGSpellCastBehaviour))]
    [RequireComponent(typeof(AGSpawnBehaviour))]
    public class AGAbilityManager : NetworkBehaviour
    {
        public Character caracterAbilites;

        private GamePlayer _player;

        public void SetPlayer(GamePlayer pl)
        {
            _player = pl;
        }

        void Start()
        {
            if (isServer) initAbility();
        }

        void Update()
        {
            if (!isLocalPlayer) return;
            if (caracterAbilites == null) return;
            if (_player == null) return;

            foreach (var input in _player.GetAllInputs())
            {
                foreach (Ability ability in caracterAbilites.abilities)
                {
                    if (ability.checkInput(input))
                    {
                        ability.Initialize(gameObject, input);
                        ability.TriggerAbility();
                    }
                }
            }
        }

        [Server]
        private void initAbility()
        {
            if (caracterAbilites == null) return;

            foreach (Ability ability in caracterAbilites.abilities)
            {
                ability.Initialize(gameObject, null);
            }
        }

    }
}