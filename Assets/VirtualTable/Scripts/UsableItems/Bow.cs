using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

namespace CpvrLab.VirtualTable {

    public class Bow : UsableItem {

        public Transform stringBone;
        public GameObject rightHandler;
        public GameObject leftHandler;

        void Start()
        {
        }

        void Update()
        {
            if (_input == null) return;

        }

        protected override void OnEquip()
        {
            base.OnEquip();
            rightHandler.SetActive(_input.IsRightHandInput());
            leftHandler.SetActive(!_input.IsRightHandInput());
            _input.HideModel(true);
            Debug.Log("Bow::OnEquip: isRight=" + _input.IsRightHandInput());
        }
        
        protected override void OnUnequip()
        {
            base.OnUnequip();
        }

        void OnTriggerEnter(Collider other)
        {
            if (_input == null) return;
            if (other.name == "arrow")
            {
                Debug.Log("Bow::OnTriggerEnter: other.name =" + other.name + ", isRight=" + _input.IsRightHandInput());
            }
        }
    }
}