using UnityEngine;
using System.Collections;
using System;

namespace CpvrLab.VirtualTable
{
    
    public class ViveIKModel : PlayerModel
    {
        public GameObject headGoal;
        public GameObject leftHandGoal;
        public GameObject rightHandGoal;

        protected VivePlayer _player;

        public override void InitializeModel(GamePlayer player)
        {
            _player = (VivePlayer)player;
        }

        void LateUpdate()
        {
            // copy head and hand transforms from our player
            headGoal.transform.position = _player.head.transform.position;
            headGoal.transform.rotation = _player.head.transform.rotation;

            leftHandGoal.transform.position = _player.leftController.transform.position;
            leftHandGoal.transform.rotation = _player.leftController.transform.rotation;

            rightHandGoal.transform.position = _player.rightController.transform.position;
            rightHandGoal.transform.rotation = _player.rightController.transform.rotation;
        }

        public override void RenderPreview(RenderTexture target)
        {
            throw new NotImplementedException();
        }
    }
}