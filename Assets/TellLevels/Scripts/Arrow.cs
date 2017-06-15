using UnityEngine;
using System.Collections;
using CpvrLab.VirtualTable;
using UnityEngine.Networking;


namespace CpvrLab.TellLevels.Scripts
{

[RequireComponent(typeof(AudioSource))]

    public class Arrow : NetworkBehaviour
    {
        public AudioClip arrowShootSound;
        public AudioClip arrowImpactSound;

        private bool isFired = false;

        //[HideInInspector]
        public AudioSource audioSource;

        void Start()
        {
            audioSource = GetComponent<AudioSource>();
        }


        void FixedUpdate()
        {
            if (isFired)
            {
                //Richte fliegenden Pfeil aus
                transform.LookAt(transform.position + transform.GetComponent<Rigidbody>().velocity);
            }
        }

        void OnTriggerStay(Collider other)
        {
            if (other.tag == "bow")
            {
                if (ArrowManager.Instance.hasArrowInHand)
                {
                    AttachArrow();
                }
            }
        }

        public void Fired()
        {
            audioSource = GetComponent<AudioSource>();
            audioSource.PlayOneShot(arrowShootSound);
            isFired = true;
        }

        void OnTriggerEnter(Collider other)
        {

            isFired = false;

            if (other.gameObject.tag == "walter")
            {
                //Play Impact Sound
                audioSource.PlayOneShot(arrowImpactSound);

                // how deep the arrow will enter the apple
                float depth = 0.1f;

                this.GetComponent<Rigidbody>().velocity = Vector3.zero;
                this.GetComponent<Rigidbody>().isKinematic = true;
                this.GetComponent<Rigidbody>().useGravity = false;
                // move the arrow deep inside 
                this.GetComponent<BoxCollider>().transform.Translate(depth * Vector3.forward);

            }
        }


        void OnCollisionEnter(Collision other)
        {

            isFired = false;

            Debug.Log("OnCollisionEnter:" + other.gameObject.name);

            if (isFired && other.gameObject.tag != "arrow")
            {
                ArrowManager.Instance.isAttached = false;
                isFired = false;
            }


            if (other.gameObject.tag == "apple")
            {
                // how deep the arrow will enter the apple
                float depth = 0.3f;

                this.GetComponent<Rigidbody>().velocity = Vector3.zero;
                this.GetComponent<Rigidbody>().isKinematic = true;

                // move the arrow deep inside 
                this.GetComponent<BoxCollider>().transform.Translate(depth * Vector3.forward);

                // stuck the arrow to the apple 
                this.transform.parent = other.gameObject.transform;

            }

            if (other.gameObject.tag == "walter")
            {
                Debug.Log("Walter col");
                //Play Impact Sound
                audioSource.PlayOneShot(arrowImpactSound);

                // how deep the arrow will enter the apple
                //float depth = 0.3f;

                this.GetComponent<Rigidbody>().velocity = Vector3.zero;
                this.GetComponent<Rigidbody>().isKinematic = true;
                this.GetComponent<Rigidbody>().useGravity = false;
                // move the arrow deep inside 
                //this.GetComponent<BoxCollider>().transform.Translate(depth * Vector3.forward);

                // stuck the arrow to the apple 
                this.transform.parent = other.gameObject.transform;
            }
        }



        void OnTriggerExit(Collider other)
        {
            if (other.tag == "bow")
            {
                if (ArrowManager.Instance.isAttached)
                {
                    ArrowManager.Instance.ArrowIsOutOfBowString();
                }
            }
        }

        private void AttachArrow()
        {
            /*
            var device = SteamVR_Controller.Input((int)ArrowManager.Instance.trackedObj.index);
            if (!ArrowManager.Instance.isAttached && device.GetTouch(SteamVR_Controller.ButtonMask.Trigger))
            {
                ArrowManager.Instance.AttachBowToArrow();
            }
            */
        }
    }
}