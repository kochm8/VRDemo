using UnityEngine;
using System.Collections;
using CpvrLab.ArcherGuardian.Scripts.AbilitySystem;
using CpvrLab.VirtualTable;
using CpvrLab.ArcherGuardian.Scripts.PlayersAndIO;
using CpvrLab.ArcherGuardian.Scripts.Items;
using UnityEngine.Networking;

namespace CpvrLab.ArcherGuardian.Scripts.AI
{
    public class AGAgent : AGSpawnBehaviour
    {
        public enum State
        {
            Patrolling,
            Chasing,
            Searching
        }

        public float chaseSpeed = 6.0f;
        public float alertLevelDecreaseTime = 20.0f;
        public float patrolSpeed = 3.5f;
        public float reachedDistance = 1.2f;

        private State _state;
        private float _alertLevel = 0.0f;
        private Transform _player;
        private Vector3 _lastPositionOfInterest;

        private PatrolPath _path;
        private AgentSight _sight;
        protected NavMeshAgent _agent;
        private Hitable _hitable;

        private float _nextHit = 0.0F;

        void Awake()
        {
            var archer = FindObjectOfType<Archer>();
            if(archer != null) _player = archer.transform;

            _path = FindObjectOfType<PatrolPath>();
            _agent = GetComponent<NavMeshAgent>();
            _sight = GetComponentInChildren<AgentSight>();
        }

        public void Start()
        {

            //_agent.updatePosition = false;
            //_agent.updateRotation = true;

            _hitable = GetComponent<Hitable>();
            _hitable.Hit += HitableHit;

            _state = State.Patrolling;
            OnPatrol();
        }

        void Update()
        {

            switch (_state)
            {
                case State.Patrolling: Patrol(); break;
                case State.Chasing: Chase(); break;
                case State.Searching: Search(); break;
            }
        }

        private void Patrol()
        {
            _agent.speed = patrolSpeed;
            Vector3 dest = _path.currentWaypoint.transform.position;

            _agent.SetDestination(dest);

            // Set the next destination point if we are closer than a threshold  
            if (Vector3.Distance(dest, transform.position) < reachedDistance)
                _path.Next();

            // Reset the look direction to the z-axis 
            _sight.LookForward();
        }

        void OnEnable() { _sight.playerSpotted += PlayerSpotted; }
        void OnDisable() { _sight.playerSpotted -= PlayerSpotted; }

        void PlayerSpotted(Vector3 position)
        {
            _state = State.Chasing;
            OnChase();
            _lastPositionOfInterest = position;
        }

        private void Chase()
        {
            // if player is not in sight and we arrived at the last position he was seen at 
            // then switch to searching the area 
            if (!_sight.playerInSight && _agent.remainingDistance <= _agent.stoppingDistance)
            {
                _state = State.Searching;
                OnSearch();
                return;
            }

            // always look at the current player position while chasing 
            _sight.LookAtPosition(_player.position);

            // update speed 
            _agent.speed = chaseSpeed;

            // update agent destination 
            _agent.SetDestination(_lastPositionOfInterest);

            // keep alert level on maximum 
            _alertLevel = 1.0f;

            float dist = Vector3.Distance(_agent.transform.position, _player.position);
            if (dist < 4)
            {
                OnAttack();
            }
        }

        private void Search()
        {
            
            // set speed to a lerp between chase and patrol speed based on alert level 
            _agent.speed = patrolSpeed + _alertLevel * (chaseSpeed - patrolSpeed);

            // decrease alert level over time 
            if (alertLevelDecreaseTime > 0.0f)
                _alertLevel -= Time.deltaTime / alertLevelDecreaseTime;
            else _alertLevel = 0.0f;

            if (_alertLevel <= 0.0f)
            {
                _alertLevel = 0.0f;
                _state = State.Patrolling;
                OnPatrol();
            }

            // keep looking at the current target 
            _sight.LookAtPosition(_lastPositionOfInterest);
            _agent.SetDestination(_lastPositionOfInterest);
        }

        /*
        void OnDrawGizmos()
        {
            if (sight == null) return;

            // project the world space head direction onto the xz-plane (normale = up-axis) 
            Vector3 headDirProj = Vector3.ProjectOnPlane(sight.headLookWS, Vector3.up);

            // add a half view angle rotation to center the view cone around the up-axis 
            Quaternion rot = Quaternion.AngleAxis(-0.5f * sight.viewAngle, Vector3.up);

            // lerp the color based on alert level 
            UnityEditor.Handles.color = Color.Lerp(new Color(0.0f, 1.0f, 0.0f, 0.2f),
                                       new Color(1.0f, 0.0f, 0.0f, 0.2f), _alertLevel);

            UnityEditor.Handles.DrawSolidArc(transform.position,
                                 Vector3.up,
                                 rot * headDirProj,
                                 sight.viewAngle,
                                 sight.viewRadius);
        }
        */

        private void HitableHit(object sender, HitableEventArgs args)
        {
            Debug.Log("Agent: OnHit, isServer: " + isServer.ToString());

            if (args.HitInfo == HitInfoKey.Head)
            {
                OnDie();
            }
        }

        void OnCollisionEnter(Collision collision)
        {
            var hitable = collision.gameObject.GetComponent<Hitable>();
            if (hitable != null)
            {
                if (Time.time > _nextHit)
                {
                    _nextHit = Time.time + npc.attackSpeed;
                    hitable.HandleZombieHit(this, gameObject, collision, npc.attackDamage);
                }
            }
        }

        protected virtual void OnSearch()
        {
        }

        protected virtual void OnChase()
        {
        }

        protected virtual void OnPatrol()
        {
        }

        protected virtual void OnAttack()
        {
        }

        protected virtual void OnDie()
        {
            if (isServer)
            {
                RpcDisableAgent();
                disableAllComponents();
            }
        }

        [ClientRpc]
        public void RpcDisableAgent()
        {
            disableAllComponents();
        }

        private void disableAllComponents()
        {
            Collider[] colls = gameObject.GetComponentsInChildren<Collider>();
            foreach (Collider coll in colls)
            {
                coll.enabled = false;
            }

            GetComponent<NavMeshAgent>().enabled = false;
            GetComponent<AGZombie>().enabled = false;
            GetComponent<Hitable>().enabled = false;
            GetComponent<NetworkTransform>().enabled = false;
        }
    }
}