using UnityEngine;
using System;

namespace CpvrLab.VirtualTable
{

    public class RiftLeapModel : PlayerModel
    {
        public GameObject headRepresentation;
        public HandPoseMapper leftModelHand;
        public HandPoseMapper rightModelHand;

        protected RiftPlayer _player;

        public override void InitializeModel(GamePlayer player)
        {
            _player = (RiftPlayer)player;
            leftModelHand.otherHand = _player.leftHandGoal.GetComponent<HandMapping>();
            rightModelHand.otherHand = _player.rightHandGoal.GetComponent<HandMapping>();
        }

        void LateUpdate()
        {
            // copy head and hand transforms from our player
            headRepresentation.transform.position = _player.head.transform.position;
            headRepresentation.transform.rotation = _player.head.transform.rotation;
        }

        public override void RenderPreview(RenderTexture target)
        {
            throw new NotImplementedException();
        }
    }

}