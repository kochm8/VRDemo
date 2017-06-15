using UnityEngine;
using System.Collections;
using System;

namespace CpvrLab.VirtualTable
{
    public class VivePlayerModel : PlayerModel
    {
        public GameObject headRepresentation;
        public GameObject leftHandRepresentation;
        public GameObject rightHandRepresentation;

        protected VivePlayer _player;

        public override void InitializeModel(GamePlayer player)
        {
            _player = (VivePlayer)player;
        }

        void LateUpdate()
        {
            // copy head and hand transforms from our player
            headRepresentation.transform.position = _player.head.transform.position;
            headRepresentation.transform.rotation = _player.head.transform.rotation;

            leftHandRepresentation.transform.position = _player.leftController.transform.position;
            leftHandRepresentation.transform.rotation = _player.leftController.transform.rotation;

            rightHandRepresentation.transform.position = _player.rightController.transform.position;
            rightHandRepresentation.transform.rotation = _player.rightController.transform.rotation;
        }

        public override void RenderPreview(RenderTexture target)
        {
            throw new NotImplementedException();
        }
    }
}