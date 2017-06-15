using UnityEngine;
using RootMotion.FinalIK;
using System;

namespace CpvrLab.ArcherGuardian.Scripts.IK
{

    [Serializable]
    public class AGPosedHand
    {
        public GameObject posedLeftHand;
        public GameObject posedRightHand;
    }

    public class AGHandEffector : MonoBehaviour
    {
        #region public
        public FullBodyBipedIK ik;

        public Transform defaultLeftHandGoal;
        public Transform defaultRightHandGoal;

        public GameObject[] toPoseHands;
        public AGPosedHand[] posedHands;
        #endregion public

        #region members
        private GameObject _posedHandLeft;
        private bool _doPosingLeft = false;

        private GameObject _posedHandRight;
        private bool _doPosingRight = false;

        private int _activEffectorLeft = -1;
        private int _activEffectorRight = -1;
        #endregion members

        #region enums
        public enum PoseableObjects : int
        {
            Default = -1,
            Bow = 0,
            Arrow = 1,
            Shild = 2,
        };

        public enum Hand : int
        {
            Left = 0,
            Right = 1,
        };
        #endregion enums

        /// <summary>
        /// Takes control over the effectors from FinalIK
        /// </summary>
        void LateUpdate()
        {
            if (ik == null) return;

            if (_doPosingLeft)
            {
                ik.solver.leftHandEffector.target = _posedHandLeft.transform;
                ik.solver.leftHandEffector.positionWeight = 1f;
                ik.solver.leftHandEffector.rotationWeight = 1f;
            }
            else
            {
                ik.solver.leftHandEffector.target = defaultLeftHandGoal;
                ik.solver.leftHandEffector.positionWeight = 1f;
                ik.solver.leftHandEffector.rotationWeight = 0.5f;
            }

            if (_doPosingRight)
            {
                ik.solver.rightHandEffector.target = _posedHandRight.transform;
                ik.solver.rightHandEffector.positionWeight = 1f;
                ik.solver.rightHandEffector.rotationWeight = 1f;
            }
            else
            {
                ik.solver.rightHandEffector.target = defaultRightHandGoal;
                ik.solver.rightHandEffector.positionWeight = 1f;
                ik.solver.rightHandEffector.rotationWeight = 0.5f;
            }
        }

        /// <summary>
        /// Activates the HandEffector (FinalIK) for a specified hand and object 
        /// </summary>
        public void ActivateHandEffector(PoseableObjects effector, Hand hand)
        {
            if (effector == PoseableObjects.Default) return;

            int effectorIndex = Convert.ToInt32(effector);
            int handIndex = Convert.ToInt32(hand);

            if (hand == Hand.Left)
            {
                _posedHandLeft = posedHands[effectorIndex].posedLeftHand;
                ActivateHandPoser(toPoseHands[handIndex], _posedHandLeft);
                _activEffectorLeft = effectorIndex;
                _doPosingLeft = true;
            }
            if (hand == Hand.Right)
            {
                _posedHandRight = posedHands[effectorIndex].posedRightHand;
                ActivateHandPoser(toPoseHands[handIndex], _posedHandRight);
                _activEffectorRight = effectorIndex;
                _doPosingRight = true;
            }
        }

        /// <summary>
        /// Deactivates the HandEffector (FinalIK) for a specified hand
        /// </summary>
        public void DeactiveHandEffector(Hand hand)
        {
            int handIndex = Convert.ToInt32(hand);

            toPoseHands[handIndex].GetComponent<HandPoser>().enabled = false;

            if (hand == Hand.Left)
            {
                _posedHandLeft = null;
                _activEffectorLeft = Convert.ToInt32(PoseableObjects.Default);
                _doPosingLeft = false;
            }
            if (hand == Hand.Right)
            {
                _posedHandRight = null;
                _activEffectorRight = Convert.ToInt32(PoseableObjects.Default);
                _doPosingRight = false;
            }
        }

        /// <summary>
        /// Deactivates the HandEffector (FinalIK) for a specified object
        /// </summary>
        public void DeactiveHandEffector(PoseableObjects effector)
        {
            int effectorIndex = Convert.ToInt32(effector);

            if (effectorIndex == _activEffectorLeft)
            {
                DeactiveHandEffector(Hand.Left);
            }
            if (effectorIndex == _activEffectorRight)
            {
                DeactiveHandEffector(Hand.Right);
            }
        }

        /// <summary>
        /// Internal method for add the HandPoser Script to the Character
        /// </summary>
        private void ActivateHandPoser(GameObject toPoseHandEffector, GameObject target)
        {
            HandPoser handPoser = toPoseHandEffector.GetComponent<HandPoser>();

            if (handPoser == null)
            {
                handPoser = toPoseHandEffector.AddComponent<HandPoser>();
            }

            handPoser.poseRoot = target.transform;
            handPoser.enabled = true;
        }
    }
}