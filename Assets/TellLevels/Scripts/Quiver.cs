using UnityEngine;
using System.Collections;
using CpvrLab.VirtualTable;


namespace CpvrLab.TellLevels.Scripts
{

    public class Quiver : MonoBehaviour
    {

        void Start()
        {

        }

        void Update()
        {

        }


        void OnTriggerStay(Collider other)
        {
            if (other.tag == "quiver")
            {
                takeArrow();
            }

        }

        void OnTriggerEnter(Collider other)
        {
            if (other.tag == "quiver")
            {

            }

        }

        void OnCollisionEnter(Collision other)
        {
        }

        private void takeArrow()
        {
            int index = ArrowManager.Instance.getControllerIndex(); //(int)ArrowManager.Instance.trackedObj.index

            //var device = SteamVR_Controller.Input((int)ArrowManager.Instance.trackedObj.index);
            var device = SteamVR_Controller.Input(index);
            if (device.GetPressUp(SteamVR_Controller.ButtonMask.Trigger))
            {
                ArrowManager.Instance.AttachArrow();

                SteamVR_Controller.Input(index).TriggerHapticPulse(500);
                Debug.Log("TAKE ARROW");
            }
        }

    }
}