using UnityEngine;
using System.Collections;
using System;
using CpvrLab.VirtualTable;
using UnityEngine.Networking;
using UnityEngine.Assertions;


namespace CpvrLab.TellLevels.Scripts
{

    public class ArrowManager : UsableItem
    {

        public static ArrowManager Instance;

        public AudioClip bowReleaseSound;
        public AudioClip bowDrawSound;

        public GamePlayer player;

        private GameObject currentArrow;
        public GameObject stringAttachPoint;
        public GameObject arrowStartPoint;
        public GameObject stringStartPoint;

        public GameObject arrowPrefab;

        public GameObject bow;

        private bool _isAttached = false;
        private bool _hasArrowInHand = false;


        public GameObject notch;

        public GameObject arrowTakerPrefab;
        private GameObject arrowTaker;

        private GameObject leftController;
        private AudioSource _audioSource;

        private float lastdist;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
        }

        void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        // Use this for initialization
        void Start()
        {
            _audioSource = GetComponent<AudioSource>();
            lastdist = 0;
        }

        void Update()
        {

            if (!isLocalPlayer)
            {
                return;
            }
            
            if (Input.GetKeyDown(KeyCode.Return))
            {
                if (!_hasArrowInHand)
                {
                    _hasArrowInHand = true;
                    currentArrow = Instantiate(arrowPrefab);
                    currentArrow.transform.parent = bow.transform;
                    currentArrow.transform.localPosition = new Vector3(0f, 0f, 0f);
                    currentArrow.transform.localRotation = Quaternion.Euler(0, -90, 0);
                    Fire(20);
                }
                _hasArrowInHand = false;
            }
        }

        // Update is called once per frame
        void FixedUpdate()
        {

            if (!isLocalPlayer)
            {
                return;
            }

            PullString();
        }

        private void PullString()
        {
            if (_isAttached)
            {

                Assert.IsNotNull(currentArrow, "No Arrow attached");

                //play draw sound
                if (!_audioSource.isPlaying)
                {
                    _audioSource.clip = bowDrawSound;
                    _audioSource.pitch = 0.5f;
                    _audioSource.Play();
                }


                //Ziehe Bogensehene zurück
                stringAttachPoint.transform.position = player.transform.position;

                //Richte den Pfeil aus
                currentArrow.transform.LookAt(notch.transform);

                //Richte Bogen aus
                Vector3 targetPostition = new Vector3(player.transform.position.x,
                                            bow.transform.position.y,
                                            player.transform.position.z);

                bow.transform.LookAt(player.transform.position);

                //Korrigiere Bogen Verdrehung
                bow.transform.Rotate(new Vector3(1, 0, 0), 270);
                bow.transform.Rotate(new Vector3(0, 0, 1), 180);

                //Distanz gespannte Sehne
                float dist = (stringStartPoint.transform.position - player.transform.position).magnitude;

                //Pause sound, wenn Sehne nicht mehr bewegt wird
                if (Math.Abs(lastdist - dist) < 0.002)
                {
                    _audioSource.Pause();
                }
                lastdist = dist;

                //Aktiviere Controller-Vibration beim Spannen der Sehne
                //SteamVR_Controller.Input((int)ArrowManager.Instance.trackedObj.index).TriggerHapticPulse(Convert.ToUInt16(dist * 1000));
                SteamVR_Controller.Input(_input.GetInstanceID()).TriggerHapticPulse(Convert.ToUInt16(dist * 1000));


                //Schiesse wenn Trigger Up
                var device = SteamVR_Controller.Input(_input.GetInstanceID());
                if (device.GetTouchUp(SteamVR_Controller.ButtonMask.Trigger))
                {

                    Fire(dist * 35f);

                    //Reset bowstring
                    stringAttachPoint.transform.position = stringStartPoint.transform.position;

                    currentArrow = null;
                    _isAttached = false;
                    _hasArrowInHand = false;

                    //Reset Bow Rotation
                    bow.transform.localRotation = Quaternion.identity;

                    _audioSource.Stop();
                }
            }
        }


        public int getControllerIndex()
        {
            return _input.GetInstanceID();
        }


        private void Fire(float strength)
        {
            Assert.IsNotNull(currentArrow, "No Arrow attached");

            //Fire Arrow
            currentArrow.transform.parent = null;
            currentArrow.GetComponent<Arrow>().Fired();

            //Aktiviere Gravitation, da der Pfeil nun in der Lusft ist
            Rigidbody r = currentArrow.GetComponent<Rigidbody>();
            r.velocity = currentArrow.transform.forward * strength;
            r.useGravity = true;
            r.isKinematic = false;

            //Spawn Arrow on the Clients
            NetworkServer.Spawn(currentArrow);
        }

        public void AttachArrow()
        {
            if (currentArrow == null)
            {
                currentArrow = Instantiate(arrowPrefab);
                currentArrow.transform.parent = player.transform;
                currentArrow.transform.localPosition = new Vector3(0f, 0f, 0f);
                currentArrow.transform.localRotation = Quaternion.identity;
                _hasArrowInHand = true;
            }
        }

        public void AttachBowToArrow()
        {
            currentArrow.transform.parent = stringAttachPoint.transform;
            currentArrow.transform.localPosition = new Vector3(0f, 0f, 0f);
            _isAttached = true;
        }

        public void ArrowIsOutOfBowString()
        {
            Fire(0.01f);

            //Reset bowstring
            stringAttachPoint.transform.position = stringStartPoint.transform.position;

            currentArrow = null;
            _isAttached = false;
            _hasArrowInHand = false;

            //Reset Bow Rotation
            bow.transform.localRotation = Quaternion.identity;
        }


        public bool isAttached
        {
            get { return _isAttached; }
            set { _isAttached = value; }
        }

        public bool hasArrowInHand
        {
            get { return _hasArrowInHand; }
            set { _hasArrowInHand = value; }
        }
    }

}