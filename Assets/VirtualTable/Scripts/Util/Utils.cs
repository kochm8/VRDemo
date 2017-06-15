using UnityEngine;
using System.Collections;

namespace CpvrLab.VirtualTable
{
    public class Utils
    {
        /// <summary>
        /// Returns a child game object by name with recursive search 
        /// </summary>
        /// <param name="fromGameObject"></param>
        /// <param name="withName"></param>
        /// <returns>The found game object or null</returns>
        static public GameObject getChildRec(GameObject fromGameObject, string withName)
        {
            Transform[] ts = fromGameObject.transform.GetComponentsInChildren<Transform>(true);
            foreach (Transform t in ts)
                if (t.gameObject.name == withName)
                    return t.gameObject;

            return null;
        }
    }
}
