using UnityEngine;
using CpvrLab.VirtualTable;

namespace CpvrLab.ArcherGuardian.Scripts.PlayersAndIO
{

    public class AGHeightAdjusting : MonoBehaviour
    {
        #region members
        public Transform playerHeadGoal;
        public GameObject playerModel;
        public float heightOfModel = 1.52f;

        private GamePlayer _gameplayer;
        #endregion members

        void Update()
        {
            //disable the script in PlayScene
            if (_gameplayer.isInPlayScene())
            {
                enabled = false;
            }

            if (!_gameplayer.isLocalPlayer) return;

            //check for PlayerInput
            foreach (var input in _gameplayer.GetAllInputs())
            {
                if (input.GetActionDown(PlayerInput.ActionCode.AdjustPlayerSize))
                {
                    AdjustingSize();
                }
            }
        }

        /// <summary>
        /// Calculate the scale Factor. It depends on the playerHeadGoal.
        /// </summary>
        private void AdjustingSize()
        {
            Vector3 player = playerHeadGoal.TransformPoint(Vector3.zero);
            float _scaleFactor = player.y / heightOfModel;

            Debug.Log("Resized PlayerModel - ModelHead:" + heightOfModel + " PlayerHead:" + player.y + " ScaleFactor:" + _scaleFactor);

            _gameplayer.modelScaleFactor = _scaleFactor;
            _gameplayer.CmdScaleModel(_scaleFactor);

            ResizePreview(_scaleFactor);
        }

        /// <summary>
        /// Resize the preview Model in the Lobby. This preview is not synchronized in the network.
        /// </summary>
        /// <param name="scale"></param>
        private void ResizePreview(float scale)
        {
            GameObject playerModelPreview = GameObject.FindGameObjectWithTag(AGTags.PlayerModelPreview);
            playerModelPreview.transform.localScale = new Vector3(scale, scale, scale);
        }

        public GameObject GetPlayerModel()
        {
            return playerModel;
        }

        public void SetGamePlayer(GamePlayer gamePlayer)
        {
            _gameplayer = gamePlayer;
        }
    }
}
