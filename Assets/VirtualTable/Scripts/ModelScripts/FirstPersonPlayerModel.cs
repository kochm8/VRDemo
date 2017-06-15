using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

namespace CpvrLab.VirtualTable
{

    public class FirstPersonPlayerModel : PlayerModel
    {
        protected FirstPersonPlayer _player;


        public override void InitializeModel(GamePlayer player)
        {
            _player = (FirstPersonPlayer)player;
        }

        void LateUpdate()
        {
            // copy body root transfrom
            transform.position = _player.transform.position - 1.08f * Vector3.up;
            transform.rotation = _player.transform.rotation;
        }

        public override void RenderPreview(RenderTexture target)
        {
            throw new NotImplementedException();
        }
    }
}