using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.Networking;
using CpvrLab.ArcherGuardian.Scripts;
using CpvrLab.ArcherGuardian.Scripts.IK;

namespace CpvrLab.VirtualTable
{

    /// <summary>
    /// First implementation of a networked vive player.
    /// 
    /// todo:   this class needs a refactoring ASAP, it's only implemented as a proof ofconcept
    ///         and is poorly designed in some places.
    /// </summary>
    public class VivePlayer : GamePlayer
    {

        [Header("Vive Player Properties")]
        public bool isRightHanded = true;

        public GameObject head;
        public GameObject leftController;
        public GameObject rightController;

        // temporary only for testing. we'll use a better solution later
        public GameObject tempCameraRig;

        public GameObject hmd = null;

        // unsure if these variables are necessary 
        protected ViveInteractionController _leftInteraction;
        protected ViveInteractionController _rightInteraction;

        protected VivePlayerInput _leftInput;
        protected VivePlayerInput _rightInput;

		protected ViveOutput _leftOutput;
		protected ViveOutput _rightOutput;


		public override void OnStartClient()
        {
            base.OnStartClient();

            AddAttachmentSlot(leftController);
            AddAttachmentSlot(rightController);
        }

        public override void OnStartLocalPlayer()
        {
            base.OnStartLocalPlayer();

            tempCameraRig.SetActive(true);

            var controllerManager = FindObjectOfType<SteamVR_ControllerManager>();

            var left = controllerManager.left;
            var right = controllerManager.right;
            var leftTrackedObj = left.GetComponent<SteamVR_TrackedObject>();
            var rightTrackedObj = right.GetComponent<SteamVR_TrackedObject>();

			var attachementSlotLeft = FindAttachmentSlot(leftController);
			var attachementSlotRight = FindAttachmentSlot(rightController);

            // add vive player input
            // left
            _leftInput = left.GetComponent<VivePlayerInput>();
            if (_leftInput == null)
                _leftInput = left.AddComponent<VivePlayerInput>();

            // right
            _rightInput = right.GetComponent<VivePlayerInput>();
            if (_rightInput == null)
                _rightInput = right.AddComponent<VivePlayerInput>();
			
            _leftInput.player = this;
            _rightInput.player = this;

			// Set roles
			var leftHandRole = PlayerInput.ControllerRole.LeftHand;
			var rightHandRole = PlayerInput.ControllerRole.RightHand;
			if (isRightHanded)
			{
				rightHandRole |= PlayerInput.ControllerRole.Main;
			}
			else
			{
				leftHandRole |= PlayerInput.ControllerRole.Main;
			}
			_leftInput.SetControllerRole(leftHandRole);
			_rightInput.SetControllerRole(rightHandRole);

			// Update the input slots added in OnStartClient
			attachementSlotLeft.input = _leftInput;
			attachementSlotRight.input = _rightInput;

			// Assign player ??? not sure if this is needed. Is the assignment in AddAtachmentSlot sufficient? 
			//_leftInteraction.input.player = this;
			//_rightInteraction.input.player = this;

			// add the tracked object to the VivePlayerInput
			// for it to know where to get input from
			_leftInput.trackedObj = leftTrackedObj;
            _rightInput.trackedObj = rightTrackedObj;
			
			// add interaction controllers
			// left
			_leftInteraction = left.GetComponent<ViveInteractionController>();
			if (_leftInteraction == null)
				_leftInteraction = left.AddComponent<ViveInteractionController>();
			// right
			_rightInteraction = right.GetComponent<ViveInteractionController>();
			if (_rightInteraction == null)
				_rightInteraction = right.AddComponent<ViveInteractionController>();

			// the interaction controllers also need the player input
			_leftInteraction.input = _leftInput;
			_rightInteraction.input = _rightInput;
			// set if default interaction should be used
			_leftInteraction.UseDefaultItemInteraction = useDefaultItemInteraction;
			_rightInteraction.UseDefaultItemInteraction = useDefaultItemInteraction;

			// connect pickup and drop delegates of the interaction controllers
			// to be notified when we should pick up a usable item
			_leftInteraction.UsableItemPickedUp += ItemPickedUp;
			_rightInteraction.UsableItemPickedUp += ItemPickedUp;
			_leftInteraction.UsableItemDropped += ItemDropped;
			_rightInteraction.UsableItemDropped += ItemDropped;

			_leftInteraction.MovableItemPickedUp += MovableItemPickedUp;
			_rightInteraction.MovableItemPickedUp += MovableItemPickedUp;
			_leftInteraction.MovableItemDropped += MovableItemDropped;
			_rightInteraction.MovableItemDropped += MovableItemDropped;

			// finally we want to find the gameobject representation
			// of the actual vive HMD
			// todo: can we do this a bit cleaner?

			#region output
			// add vive player output
			// left
			_leftOutput = left.GetComponent<ViveOutput>();
			if (_leftOutput == null)
				_leftOutput = left.AddComponent<ViveOutput>();
			
			// right
			_rightOutput = right.GetComponent<ViveOutput>();
			if (_rightOutput == null)
				_rightOutput = right.AddComponent<ViveOutput>();

			// add the tracked object to the VivePlayerOutput
			// for it to know where to get input from
			_leftOutput.trackedObj = leftTrackedObj;
			_rightOutput.trackedObj = rightTrackedObj;

			// Update the input slots added in OnStartClient
			attachementSlotLeft.output = _leftOutput;
			attachementSlotRight.output = _rightOutput;
			#endregion output
		}

        /// <summary>
        /// The local player makes sure to update the three gameObjects
        /// that represent the head and hands over the network
        /// </summary>
        void Update()
        {
            if (!isLocalPlayer)
                return;

            head.transform.position = hmd.transform.position;
            head.transform.rotation = hmd.transform.rotation;
			
			leftController.transform.position = _leftInput.transform.position;
			leftController.transform.rotation = _leftInput.transform.rotation;
			rightController.transform.position = _rightInput.transform.position;
			rightController.transform.rotation = _rightInput.transform.rotation;
        }

        /// <summary>
        /// Called when ever one of our controllers picks up a UsableItem
        /// </summary>
        /// <param name="input"></param>
        /// <param name="item"></param>
        void ItemPickedUp(PlayerInput input, UsableItem item)
        {
            Debug.Log("ItemPickedUp()");
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
            Debug.Log("Grabbing " + GetSlotIndex(FindAttachmentSlot(input)) + " " + item.name);
            CmdGrabMovableItem(item.gameObject, GetSlotIndex(FindAttachmentSlot(input)));
        }
        void MovableItemDropped(PlayerInput input, MovableItem item)
        {
            Debug.Log("Releasing");
            CmdReleaseMovableItem(item.gameObject, GetSlotIndex(FindAttachmentSlot(input)));
        }

        protected override PlayerInput GetMainInput()
        {
            foreach(var input in GetAllInputs())
			{
				if (input.IsInControllerRole(PlayerInput.ControllerRole.Main))
					return input;
			}
			return null;
        }

        public override PlayerInput GetOtherInput(PlayerInput input)
        {
            if (input == _rightInput)
                return _leftInput;
            else
                return _rightInput;
		}
		public override IEnumerable<PlayerInput> GetAllInputs()
		{
			yield return _leftInput;
			yield return _rightInput;
		}


        protected override void OnEquip(AttachmentSlot slot)
        {
            if (slot.item.tag == AGTags.Bow)
            {
                slot.item.transform.localRotation = Quaternion.Euler(90, 0, 0);
            }
        }

        protected override void OnUnequip(UsableItem item)
        {
        }

    } // class

} // namespace