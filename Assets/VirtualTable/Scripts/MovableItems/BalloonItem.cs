using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using Random = UnityEngine.Random;

namespace CpvrLab.VirtualTable
{

    [RequireComponent(typeof(Shootable))]
    public class BalloonItem : MovableItem
    {

        public Transform centerOfMass;
        public GameObject popEffect;
        public bool tempPopTest = false;
        public AudioClip popSound;
        public float boyancyForce = 0.3f;
        public bool autoDestroy = true;
        public float lifeTime = 10.0f;
        public bool randomColor = true;
        public Color[] availableColors;

        private Rigidbody _rigidbody;
        private Renderer _renderer;
        [SyncVar(hook = "SetColor")] public Color color = Color.black;

        void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _renderer = GetComponent<Renderer>();
            if (centerOfMass != null)
                _rigidbody.centerOfMass = centerOfMass.localPosition;

            if (randomColor)
            {
                color = availableColors[Random.Range(0, availableColors.Length)];
            }
            _renderer.material.color = color;


            if (autoDestroy)
                StartCoroutine(AutoDestroy());
        }
        
        public void SetColor(Color col)
        {
            color = col;
            _renderer.material.color = color;
            
        }

        public override void OnStartServer()
        {
            base.OnStartServer();
            
            var shootable = gameObject.AddComponent<Shootable>();
            shootable.OnHit.AddListener(OnHit);
            _renderer.material.color = color;
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            _renderer.material.color = color;
        }

        void OnHit(Vector3 position, GamePlayer shooter)
        {
            Pop();
            RpcPop();
        }

        IEnumerator AutoDestroy()
        {
            yield return new WaitForSeconds(lifeTime);
            PopWithSound(false);
        }

        void FixedUpdate()
        {
            _rigidbody.AddForce(Vector3.up * boyancyForce);

            if (tempPopTest)
                Pop();
        }

        [ClientRpc] void RpcPop()
        {
            Pop();
        }


        public void Pop()
        {
            PopWithSound(true);
        }

        public void PopWithSound(bool sound)
        {
            var popParticle = Instantiate(popEffect, centerOfMass.position, centerOfMass.rotation) as GameObject;
            var popPS = popParticle.GetComponent<ParticleSystem>();
            var pr = popPS.GetComponent<Renderer>();
            //pr.material.color = color;
            //popPS.startColor = color;
            Destroy(popParticle, popPS.duration);

            if (sound) {
                var popAudio = popParticle.AddComponent<AudioSource>();
                popAudio.volume = 0.5f;
                popAudio.clip = popSound;
                popAudio.pitch = Random.Range(0.8f, 1.5f);
                popAudio.spatialBlend = 1.0f;
                popAudio.minDistance = 0.2f;
                popAudio.minDistance = 10.0f;
                popAudio.rolloffMode = AudioRolloffMode.Logarithmic;
                popAudio.Play();
            }

            Destroy(gameObject);
        }
    }

}
