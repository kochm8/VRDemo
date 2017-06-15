using UnityEngine;
using UnityEngine.Networking;
using System;
using CpvrLab.ArcherGuardian.Scripts.Lobby;

namespace CpvrLab.VirtualTable {


    
    /// <summary>
    /// this script handles switching between
    /// different build targets
    /// it will adjust GUI setups and correct player prefab usage
    /// etc...
    /// 
    /// todo:   try a different approach where all of the versions are in one build and can be switched
    ///         by selecting the target platform in the offline menu.
    /// </summary>
    public class BuildOptions : MonoBehaviour {
        public enum Target {
            DefaultPC,    // normal PC game build
            Vive,       // vive with motion controls
            Rift,        // rift with touch controls(?) not implemented yet
            RiftLeap,        // rift with touch controls(?) not implemented yet
        }

        [Serializable]
        public struct AdvancedSettings {
            public AGLobbyManager networkManager;
        }

        [Serializable]
        public struct TargetOptions {
            public GameObject playerPrefab;
            public GameObject offlineCamera;
            public GameObject offlineMenu;
            public bool virtualRealitySupportedChecked;
        }

        public Target buildTarget = Target.DefaultPC;
        public AdvancedSettings advancedSettings;
        public TargetOptions[] targetOptions;
        public bool logOutput = true;
        

        public void UpdateBuildTarget()
        {
            var selectedOption = targetOptions[(int)buildTarget];

            if (logOutput) { Log("BuildOptions changed, updating project accordingly."); }

            advancedSettings.networkManager.playerPrefab = selectedOption.playerPrefab;
            if(logOutput) { Log("Changing NetworkManager.playerPrefab to " + selectedOption.playerPrefab.name); }

            // find out which index our prefab has in the network manager
            var prefabs = advancedSettings.networkManager.playerPrefabs;
            int prefabIndex = -1;
            for (int i = 0; i < prefabs.Length; ++i) {
                if (prefabs[i].Equals(targetOptions[(int)buildTarget].playerPrefab))
                {
                    prefabIndex = i;
                    break;
                }
            }

            if(prefabIndex == -1)
            {
                LogError("Couldn't find the player prefab in the network manager. Make sure to add a player prefab for your custom player to both the network manager and to the build options!");
            }
            else
            {
                advancedSettings.networkManager.networkPrefabIndex = prefabIndex;
            }


            // disable all offline cameras and menus
            foreach (var option in targetOptions)
            {
                if (option.offlineCamera != null)
                    option.offlineCamera.SetActive(false);

                if (option.offlineMenu != null)
                    option.offlineMenu.SetActive(false);
            }
            // enable offline camera for selected target
            if (selectedOption.offlineCamera != null) selectedOption.offlineCamera.SetActive(true);
            if (logOutput) { Log("Changing offline camera to " + selectedOption.offlineCamera.name); }
            
            // enable offline menu for selected target
            if (selectedOption.offlineMenu != null) selectedOption.offlineMenu.SetActive(true);
            if (logOutput) { Log("Changing offline menu to " + selectedOption.offlineMenu.name); }

#if UNITY_EDITOR
            Debug.Log("Setting VR support to " + selectedOption.virtualRealitySupportedChecked);
            UnityEditor.PlayerSettings.virtualRealitySupported = selectedOption.virtualRealitySupportedChecked;
#endif
        }

        private void Log(string msg)
        {
            Debug.Log("BuildOptions: " + msg);
        }
        private void LogError(string msg)
        {
            Debug.LogError("BuildOptions: " + msg);
        }
    }
    

}