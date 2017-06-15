using UnityEngine;
using System.Collections;

namespace CpvrLab.TellLevels
{

    public class AGHeightAdjusting : MonoBehaviour
    {

        public Transform viveHeadTransform;
        public Transform modelHeadTransform;
        public SteamVR_TrackedObject viveController;

        // Use this for initialization
        void Start()
        {
        }

        // Update is called once per frame
        void Update()
        {

            if (Input.GetKeyDown(KeyCode.A))
            {
                AdjustingHeight();
            }

            var device = SteamVR_Controller.Input((int)viveController.index);
            if (device.GetPressDown(SteamVR_Controller.ButtonMask.ApplicationMenu))
            {
                AdjustingHeight();
            }

        }

        void AdjustingHeight()
        {
            Vector3 worldPos = modelHeadTransform.TransformPoint(Vector3.zero);
            Debug.Log("SintelHead:" + worldPos.y + " ViveHead:" + viveHeadTransform.position.y);

            float scaleFactor = viveHeadTransform.position.y / worldPos.y;
            gameObject.transform.localScale = new Vector3(1, scaleFactor, 1);
        }
    }
}
