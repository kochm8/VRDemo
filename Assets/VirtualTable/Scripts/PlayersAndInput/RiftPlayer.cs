using UnityEngine;
using System.Collections;
using System;
using UnityStandardAssets.Characters.FirstPerson;
using System.Collections.Generic;

namespace CpvrLab.VirtualTable {


    /// <summary>
    /// Player using the oculus rift
    /// </summary>
    public class RiftPlayer : GamePlayer {

        [Header("Rift Player Properties")]
        public GameObject head;
        public GameObject cam;
        public GameObject leftHandGoal;
        public GameObject rightHandGoal;
        public GameObject localHands;
        public LeapInteractionController leftInteractionController;
        public LeapInteractionController rightInteractionController;

        public GameObject attachPointLeft;
        public GameObject attachPointRight;

        void Awake()
        {
            AddAttachmentSlot(attachPointLeft);
            AddAttachmentSlot(attachPointRight);
        }
        //public override void OnStartClient()
        //{
        //    AddAttachmentSlot(attachPointLeft);
        //    AddAttachmentSlot(attachPointRight);
        //}

        public override void OnStartLocalPlayer()
        {
            base.OnStartLocalPlayer();
            cam.SetActive(true);
            cam.GetComponent<Camera>().enabled = true;
            localHands.SetActive(true);

            // enable hand goal scripts necessary for the local player to work
            leftHandGoal.GetComponent<HandPoseLerp>().enabled = true;
            leftHandGoal.GetComponent<HandConfidenceWeightFade>().enabled = true;
            rightHandGoal.GetComponent<HandPoseLerp>().enabled = true;
            rightHandGoal.GetComponent<HandConfidenceWeightFade>().enabled = true;

            // Update teh input slots added in OnStartClient
            // todo: maybe do this a bit differently, we can't rely on OnStartClient
            //       to run before OnStartLocalPlayer, even though it probably always will.
            FindAttachmentSlot(attachPointLeft).input = leftInteractionController.input;
            FindAttachmentSlot(attachPointRight).input = rightInteractionController.input;

            leftInteractionController.UsableItemPickedUp += ItemPickedUp;
            rightInteractionController.UsableItemPickedUp += ItemPickedUp;
            leftInteractionController.UsableItemDropped += ItemDropped;
            rightInteractionController.UsableItemDropped += ItemDropped;

            leftInteractionController.MovableItemPickedUp += MovableItemPickedUp;
            rightInteractionController.MovableItemPickedUp += MovableItemPickedUp;
            leftInteractionController.MovableItemDropped += MovableItemDropped;
            rightInteractionController.MovableItemDropped += MovableItemDropped;
        }

        /// <summary>
        /// Called when ever one of our controllers picks up a UsableItem
        /// </summary>
        /// <param name="input"></param>
        /// <param name="item"></param>
        void ItemPickedUp(PlayerInput input, UsableItem item)
        {
            Equip(input, item, false);
        }

        /// <summary>
        /// Called when ever one of our controllers drops a UsableItem
        /// </summary>
        /// <param name="input"></param>
        /// <param name="item"></param>
        void ItemDropped(PlayerInput input, UsableItem item)
        {
            Unequip(item);
        }
        
        void MovableItemPickedUp(PlayerInput input, MovableItem item)
        {
            CmdGrabMovableItem(item.gameObject, GetSlotIndex(FindAttachmentSlot(input)));
        }
        void MovableItemDropped(PlayerInput input, MovableItem item)
        {
            CmdReleaseMovableItem(item.gameObject, GetSlotIndex(FindAttachmentSlot(input)));
        }

        void Update()
        {
            if(!isLocalPlayer)
                return;

            head.transform.position = cam.transform.position;
            head.transform.rotation = cam.transform.rotation;
        }

        protected override PlayerInput GetMainInput()
        {
            return leftInteractionController.input;
        }

        public override PlayerInput GetOtherInput(PlayerInput input)
        {
            if (input == leftInteractionController.input)
                return rightInteractionController.input;
            else
                return leftInteractionController.input;
        }
		public override IEnumerable<PlayerInput> GetAllInputs()
		{
			yield return leftInteractionController.input;
			yield return rightInteractionController.input;
		}
		protected override void OnEquip(AttachmentSlot slot)
        {
        }
        
        protected override void OnUnequip(UsableItem item)
        {
        }
        
    } // class
    
} // namespace