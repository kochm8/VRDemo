using UnityEngine;
using System.Collections;

namespace CpvrLab.ArcherGuardian.Scripts.IK
{

    public class AGControllerMidPosition : MonoBehaviour
    {

        public Transform leftController;
        public Transform rightController;

        void Update()
        {
            float x = rightController.position.x + (leftController.position.x - rightController.position.x) / 2;
            float y = rightController.position.y + (leftController.position.y - rightController.position.y) / 2;
            float z = rightController.position.z + (leftController.position.z - rightController.position.z) / 2;
            Vector3 vec = rightController.position + (leftController.position - rightController.position);
            //Debug.DrawLine(leftController.position, rightController.position);
            transform.position = new Vector3(x, y, z);
            //transform.rotation = Quaternion.Euler(vec);
        }
    }
}
