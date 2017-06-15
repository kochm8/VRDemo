using UnityEngine;
using System.Collections;

namespace CpvrLab.VirtualTable
{

    [RequireComponent(typeof(Camera))]
    public class ObserverCamera : MonoBehaviour
    {
        private Camera _cam;
        private static Camera _activeObserverCam;
        private static Camera _playerCamera;
        
        void Awake()
        {
            _cam = GetComponent<Camera>();
            _cam.enabled = false;
        }

        public void Activate()
        {
            if (_playerCamera == null)
            {
                _playerCamera = Camera.main;
                _playerCamera.enabled = false;
            }
            
            if (_activeObserverCam != null) _activeObserverCam.enabled = false;
            _cam.enabled = true;

            _activeObserverCam = _cam;
        }

        public static void Deactivate()
        {
            if (_activeObserverCam == null) return;
            _activeObserverCam.enabled = false;
            _activeObserverCam = null;
            _playerCamera.enabled = true;
            _playerCamera = null;
        }
    }

}