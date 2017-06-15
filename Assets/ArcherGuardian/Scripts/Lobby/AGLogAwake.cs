using UnityEngine;
using System.Collections;

namespace CpvrLab.ArcherGuardian.Scripts.Lobby
{
    public class AGLogAwake : MonoBehaviour {

        void Awake()
        {
            Debug.LogError(this.gameObject.name + " -AWAKE");
        }
        // Use this for initialization
        void Start() {

        }

        // Update is called once per frame
        void Update() {

        }
    }
}
