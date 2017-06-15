using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

namespace CpvrLab.VirtualTable {

    /// <summary>
    /// Not much here yet. 
    /// </summary>
    public class TVRemote : UsableItem {

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

        public Clickable tvSet;

        void Start()
        {
            _audioSource = GetComponent<AudioSource>();
        }

        void Update()
        {
            if (_input == null)
                return;

            bool click = _input.GetActionDown(PlayerInput.ActionCode.Button0);

            if (click)
            {
                Click();
                CmdClick();

                _shotCooldown = maxFireRate;
            }

            // in case axis 0 is bound, show the trigger getting pressed
            float factor = _input.GetAxis(PlayerInput.AxisCode.Axis0);
            float angle = Mathf.Lerp(triggerAngleReleased, triggerAnglePressed, factor);
        }

        [Command] void CmdClick()
        {
            RpcClick();

            // do hit calculations on the server (if it's a dedicated server that is)
            // else we already called Shoot in the update method
            if (!hasAuthority && !isClient)
            {
                Debug.Log("Calling Click from CmdClick");
                Click();
            }
        }

        [ClientRpc] void RpcClick()
        {
            // only call shoot for non local players
            if (!hasAuthority)
            {
                Click();
            }
        }

        void Click()
        {
            TVSet t2 = GameObject.Find("TVSet").GetComponent<TVSet>();
            t2.OnCLick(_owner);

            /*
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
                        var clickable = rb.GetComponent<Clickable>();
                        if (clickable != null)
                        {
                            // let the other object know it has been shot
                            clickable.Click(rayInfo.point, _owner);
                        }
                    }
                }

                distance = rayInfo.distance;

                //_audioSource.PlayOneShot(ricochetSounds[Random.Range(0, ricochetSounds.Length - 1)]);
            }*/

            //_audioSource.PlayOneShot(gunShotSounds[Random.Range(0, gunShotSounds.Length - 1)]);


            // todo: delay shell sound
            _audioSource.PlayOneShot(shellFallingSound);
        }
    }

}