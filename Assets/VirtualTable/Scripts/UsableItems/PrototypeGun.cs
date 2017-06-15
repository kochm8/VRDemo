using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

namespace CpvrLab.VirtualTable {

    public class PrototypeGun : UsableItem {

        public Transform trigger;
        public float triggerAnglePressed;
        public float triggerAngleReleased;

        public float maxFireRate = 0.1f;
        private float _shotCooldown = 0.0f;

        public GameObject gunshotEffectPrefab;
        public GameObject muzzle;

        public AudioClip[] gunShotSounds;
        public AudioClip[] ricochetSounds;
        public AudioClip shellFallingSound;
        public AudioClip dryFireSound;

        private AudioSource _audioSource;
        private GunshotEffect[] _effects;
        private int _effectIndex = 0;

        void Start()
        {
            int effectCount = 10;
            _effects = new GunshotEffect[effectCount];
            for(int i = 0; i < effectCount; i++)
            {
                _effects[i] = Instantiate(gunshotEffectPrefab).GetComponent<GunshotEffect>();
                _effects[i].transform.parent = muzzle.transform;
                _effects[i].transform.localPosition = Vector3.zero;
                _effects[i].transform.rotation = Quaternion.identity;
            }

            _audioSource = GetComponent<AudioSource>();
        }

        void Update()
        {
            if (_input == null)
                return;

            bool shoot = _input.GetActionDown(PlayerInput.ActionCode.Button0);

            if (_shotCooldown > 0 || !inputEnabled)
            {
                _shotCooldown -= Time.deltaTime;

                if (shoot)
                    _audioSource.PlayOneShot(dryFireSound);
            }

            else if (shoot)
            {
                Shoot();
                CmdShoot();

                _shotCooldown = maxFireRate;
            }

            // in case axis 0 is bound, show the trigger getting pressed
            float factor = _input.GetAxis(PlayerInput.AxisCode.Axis0);
            float angle = Mathf.Lerp(triggerAngleReleased, triggerAnglePressed, factor);
            trigger.localRotation = Quaternion.AngleAxis(angle, Vector3.right);
        }

        [Command] void CmdShoot()
        {
            RpcShoot();

            // do hit calculations on the server (if it's a dedicated server that is)
            // else we already called Shoot in the update method
            if (!hasAuthority && !isClient)
            {
                Debug.Log("Calling Shoot from CmdShoot");
                Shoot();
            }
        }

        [ClientRpc] void RpcShoot()
        {
            // only call shoot for non local players
            if (!hasAuthority)
            {
                Shoot();
            }
        }

        void Shoot()
        {
            Ray ray = new Ray(muzzle.transform.position, muzzle.transform.forward);
            RaycastHit rayInfo;
            float distance = 100.0f;
            if(Physics.Raycast(ray, out rayInfo, 100.0f))
            {
                if (isServer)
                {
                    var rb = rayInfo.rigidbody;

                    // if the other object has a rigidbody attached then it might be a shootable item
                    if (rb != null)
                    {
                        // check if the other object is shootable
                        var shootable = rb.GetComponent<Shootable>();
                        if (shootable != null)
                        {
                            // let the other object know it has been shot
                            shootable.Hit(rayInfo.point, _owner);
                        }
                    }
                }

                distance = rayInfo.distance;

                //_audioSource.PlayOneShot(ricochetSounds[Random.Range(0, ricochetSounds.Length - 1)]);
            }
            
            _audioSource.PlayOneShot(gunShotSounds[Random.Range(0, gunShotSounds.Length - 1)]);


            // todo: delay shell sound
            //_audioSource.PlayOneShot(shellFallingSound);


            Vector3 hitPoint = muzzle.transform.position + muzzle.transform.forward * distance;
            _effects[_effectIndex].Play(hitPoint);

            // increment effect index
            _effectIndex++;
            _effectIndex %= _effects.Length;
        }
    }

}