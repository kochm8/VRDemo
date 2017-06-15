using UnityEngine;
using System.Collections;
using CpvrLab.ArcherGuardian.Scripts.Items;
using UnityEngine.Networking;

namespace CpvrLab.ArcherGuardian.Scripts.AI
{
    public class AGZombie : AGAgent
    {
        public AudioClip ZombieDieSound;
        public AudioClip ZombieAttackSound;
        public AudioClip ZombiePatrolSound;
        public AudioClip ZombieChaseSound;

        private AudioSource _audioSource;
        private Animator _animator;
        private float _nextSound;

        new void Start()
        {
            _animator = GetComponent<Animator>();
            _audioSource = GetComponent<AudioSource>();
            base.Start();
        }

        public override void OnStartLocalPlayer()
        {
            GetComponent<NetworkAnimator>().SetParameterAutoSend(0, true);
        }

        public override void PreStartClient()
        {
            GetComponent<NetworkAnimator>().SetParameterAutoSend(0, true);
        }

        protected override void OnChase()
        {
            //Debug.Log("AGZombie::OnChase()");
            playClip(ZombieChaseSound, 5);
            _animator.SetInteger("zombie", 3);
            _animator.speed = 2;
        }

        protected override void OnPatrol()
        {
            //Debug.Log("AGZombie::OnPatrol()");
            playClip(ZombiePatrolSound, 5);
            _animator.SetInteger("zombie", 3);
            _animator.speed = 1;
        }

        protected override void OnSearch()
        {
            //Debug.Log("AGZombie::OnSearch()");
            _animator.speed = 1;
            _animator.SetInteger("zombie", 2);
        }

        protected override void OnAttack()
        {
            //Debug.Log("AGZombie::OnAttack()");
            playClip(ZombieAttackSound, 2);
            _animator.speed = 1;
            _animator.SetInteger("zombie", 1);
        }

        protected override void OnDie()
        {
            playClip(ZombieDieSound, 0.1f);
            _animator.SetInteger("zombie", 4);
            base.OnDie();
        }

        private void playClip(AudioClip clip, float soundDelay)
        {
            if (!isClient) return;
            
            if (Time.time > _nextSound)
            {
                _nextSound = Time.time + soundDelay;

                if (!_audioSource.isPlaying)
                {
                    _audioSource.PlayOneShot(clip);
                }
            }

        }

        /*
        // Combine Animation and NavMeshAgent movement
        void OnAnimatorMove()
        {
            _agent.velocity = _animator.deltaPosition / Time.deltaTime;
            _agent.velocity = new Vector3(2, 2, 2);
            Vector3 position = _animator.rootPosition;
            position.y = _agent.nextPosition.y;
            transform.position = position;
        }*/

    }
}