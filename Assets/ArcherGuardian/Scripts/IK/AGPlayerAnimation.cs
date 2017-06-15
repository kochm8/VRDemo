using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

namespace CpvrLab.ArcherGuardian.Scripts.IK
{

    public class AGPlayerAnimation : NetworkBehaviour
    {

        private Animator _animator;
        private Vector3 _lastPosition;


        void Start()
        {
            _animator = GetComponent<Animator>();
        }

        void FixedUpdate()
        {
            DoWalkAnimation();
        }


        private void DoWalkAnimation()
        {
            float distance = Vector3.Distance(_lastPosition, transform.position);

            if (System.Math.Abs(distance) > 0.005f)
            {
                _animator.SetBool("Walk", true);
            }
            else
            {
                _animator.SetBool("Walk", false);
            }

            _lastPosition = transform.position;
        }


        public override void OnStartLocalPlayer()
        {

            Renderer[] renderers = GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers)
            {
                renderer.enabled = false;
            }

            GetComponent<NetworkAnimator>().SetParameterAutoSend(0, true);
            base.OnStartLocalPlayer();
        }

        public override void PreStartClient()
        {
            GetComponent<NetworkAnimator>().SetParameterAutoSend(0, true);
        }
    }
}