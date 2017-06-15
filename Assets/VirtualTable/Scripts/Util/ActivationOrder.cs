using UnityEngine;

namespace CpvrLab.VirtualTable
{

    public class ActivationOrder : MonoBehaviour
    {
        public GameObject[] objs;

        void Awake()
        {
            foreach (var obj in objs)
                obj.SetActive(true);
        }
    }

}