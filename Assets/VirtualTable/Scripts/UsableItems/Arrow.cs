using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

namespace CpvrLab.VirtualTable {

    public class Arrow : UsableItem {

        public float        arrowForceFactor = 20.0f;
        public Transform    centerOfMass;
        public AudioClip    bowShootSound;

        [SyncVar]
        private bool        _isAttached = false;
        private bool        _isFlying = false;
        private GameObject  _bow;
        private Vector3     _bowLocalPosUnpulled;
        private Quaternion  _bowLocalRotUnpulled;
        private GameObject  _stringBone;
        private Vector3     _stringBoneLocalPosUnpulled;
        private Rigidbody   _rigidbody;
        private AudioSource _audioSource;

        void Start()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _audioSource = GetComponent<AudioSource>();
        }

        void Update()
        {
            if (_input == null)
                return;

            bool triggerPressed  = _input.GetAction(PlayerInput.ActionCode.Button0);
            bool triggerReleased = _input.GetActionUp(PlayerInput.ActionCode.Button0);
            
            if (triggerReleased)
            {
                ShootArrow();
                //CmdShootArrow();
            }


            if (triggerPressed)
            {
                PullArrow();
                //CmdPullArrow();
            }


            if (_isAttached)
            {
                this.transform.position = _stringBone.transform.position;
                this.transform.rotation = _stringBone.transform.rotation;
                this.transform.rotation *= Quaternion.Euler(0.0f,0.0f,-90.0f);
            }
        }

        void FixedUpdate()
        {
            if (_isFlying && _rigidbody.velocity != Vector3.zero)
            {
                // add a force like to drag, opposite the direction of travel, and apply it after of the center of mass, typically (0,0,0)
                _rigidbody.AddForceAtPosition(_rigidbody.velocity * -0.1f, transform.TransformPoint(0,0,0)); 
            }
        }

        void OnTriggerEnter(Collider other)
        {
            if (_input == null) return;

            if (other.name == "bow")
            {
                _bow = other.gameObject.transform.parent.gameObject; // bow_rig_simple
                _bowLocalPosUnpulled = _bow.transform.localPosition;
                _bowLocalRotUnpulled = _bow.transform.localRotation;

                _stringBone = Utils.getChildRec(_bow, "stringBone");
                
                if (_stringBone != null)
                {
                    _isAttached = true;
                    CmdSetIsAttached(true);

                    _stringBoneLocalPosUnpulled = _stringBone.transform.localPosition;

                    Debug.Log("Arrow::OnTriggerEnter: other.name =" + other.name + 
                              ", isRight=" + _input.IsRightHandInput() + 
                              ", isAttached=" + _isAttached);
                }
            }
        }        

        void OnCollisionEnter(Collision other)
        {
            Debug.Log("OnCollisionEnter:" + other.gameObject.name);
            
            if (_isFlying && other.gameObject.name != "Bow")
            {
                _rigidbody.velocity = Vector3.zero;
                _rigidbody.isKinematic = true;
                _isFlying = false;
                _isAttached = false;
                Detach();
            }
        }

        
        [Command] void CmdSetIsAttached(bool value) { _isAttached = value; }
        
        
        /////////////
        // Pulling //
        /////////////

        [Command] void CmdPullArrow()
        {
            RpcPullArrow();
            
            if (!hasAuthority && !isClient)
            {
                Debug.Log("Calling PullArrow from CmdPullArrow");
                PullArrow();
            }
        }
        [ClientRpc] void RpcPullArrow()
        {
            // only call shoot for non local players
            if (!hasAuthority)
            {
                PullArrow();
            }
        }
        void PullArrow()
        {
            if (_isAttached)
            {
                // hide controllers during pulling
                _input.HideModel(true);

                // let the bow aim to a point in front of the bow on the aiming line
                Vector3 aimDir = _bow.transform.position - _input.GetTrackedTransform().position;
                _bow.transform.LookAt(_bow.transform.position + aimDir, _bow.transform.up);

                // attach the string bone to the arrow controller
                _stringBone.transform.position = _input.GetTrackedTransform().position;
            }
        }
        
        //////////////
        // Shooting //
        //////////////

        [Command] void CmdShootArrow()
        {
            RpcShootArrow();

            // do hit calculations on the server (if it's a dedicated server that is)
            // else we already called Shoot in the update method
            if (!hasAuthority && !isClient)
            {
                Debug.Log("Calling ShootArrow from CmdShootArrow");
                ShootArrow();
            }
        }
        [ClientRpc] void RpcShootArrow()
        {
            // only call shoot for non local players
            if (!hasAuthority)
            {
                ShootArrow();
            }
        }
        void ShootArrow()
        {
            float pullDist = (_bow.transform.position - _input.GetTrackedTransform().position).magnitude;
            _bow.transform.localPosition = _bowLocalPosUnpulled;
            _bow.transform.localRotation = _bowLocalRotUnpulled;
            _stringBone.transform.localPosition = _stringBoneLocalPosUnpulled;
            
            _rigidbody.isKinematic = false;
            _rigidbody.velocity = transform.up * arrowForceFactor * pullDist;

            _input.HideModel(false);
            _isAttached = false;
            _isFlying = true;
            _audioSource.PlayOneShot(bowShootSound);
            
            Detach();
        }



        protected override void OnUnequip()
        {
            base.OnUnequip();
            _bow = null;
            _stringBone = null;
            _isAttached = false;
        }
    }
}