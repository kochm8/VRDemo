using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

namespace CpvrLab.VirtualTable
{

    [RequireComponent(typeof(Clickable))]
    public class TVSet : MovableItem
    {
        public AudioClip popSound;

        public bool turnedOn = false;
        private Renderer _renderer;

        //[SyncVar(hook = "SetColor")] public Color color = Color.black;
        public Color color = Color.black;

        void Awake()
        {
            _renderer = GetComponent<Renderer>();
            
            _renderer.material.color = color;
        }
        
        public void SetColor(Color col)
        {
            color = col;
            _renderer.material.color = color;
        }

        public override void OnStartServer()
        {
            base.OnStartServer();
            
            var clickable = gameObject.AddComponent<Clickable>();
            clickable.OnClick.AddListener(OnCLick);
            _renderer.material.color = color;
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            _renderer.material.color = color;
        }

        public void OnCLick(GamePlayer player)
        {
            turnedOn = !turnedOn;

            if (turnedOn)
            {
                TurnOn();
                RpcTurnOn();
            } else
            {
                TurnOff();
                RpcTurnOff();
            }
        }

        [ClientRpc] void RpcTurnOn()
        {
            TurnOn();
        }


        public void TurnOn()
        {
            SetColor(Color.red);
            Debug.Log("TURN TV ON");
        }

        
        [ClientRpc] void RpcTurnOff()
        {
            TurnOff();
        }


        public void TurnOff()
        {
            SetColor(Color.black);
            Debug.Log("TURN TV OFF");
        }
    }

}
