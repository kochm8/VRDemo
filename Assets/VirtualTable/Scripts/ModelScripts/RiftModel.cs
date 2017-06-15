using UnityEngine;
using System;

namespace CpvrLab.VirtualTable
{

    public class RiftModel : PlayerModel
    {
        public GameObject headRepresentation;

        protected RiftPlayer _player;

        public override void InitializeModel(GamePlayer player)
        {
            _player = (RiftPlayer)player;
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