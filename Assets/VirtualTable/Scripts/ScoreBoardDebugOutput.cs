using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

namespace CpvrLab.VirtualTable
{





    /// <summary>
    /// Generic score board that can hold any kind of table data and sync over the network
    /// </summary>
    public class ScoreBoardDebugOutput : MonoBehaviour
    {

        void Start()
        {
            GameManager.instance.scoreBoardData.OnDataChanged += DebugDisplay;
        }

        void OnDestroy()
        {
            GameManager.instance.scoreBoardData.OnDataChanged -= DebugDisplay;
        }
        
        /// very simple debug output of the scoreboard content for testing purposes
        public void DebugDisplay(ScoreBoard sb)
        {
            Debug.Log("------------------------------------------");
            Debug.Log("------------{ " + sb.title + " }------------");
            Debug.Log("------------------------------------------");

            // print headers
            string line = "| ";
            foreach (var val in sb.headers)
                line += val + " | ";
            Debug.Log(line);

            // print content
            foreach(var row in sb.rowData)
            {
                line = "| ";
                foreach (var val in row)
                    line += val + " | ";
                Debug.Log(line);
            }

            Debug.Log("------------------------------------------");
        }
    }

}