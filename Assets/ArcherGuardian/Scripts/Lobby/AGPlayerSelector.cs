using UnityEngine;
using System.Collections;
using UnityEngine.Assertions;
using UnityEngine.Networking;

namespace CpvrLab.ArcherGuardian.Scripts.Lobby
{
    public class AGPlayerSelector : NetworkBehaviour
    {
        public bool isArcher;
        public bool isGuardian;

        [HideInInspector]
        [SyncVar]
        public bool isSelectedByPlayer = false;
    }
}