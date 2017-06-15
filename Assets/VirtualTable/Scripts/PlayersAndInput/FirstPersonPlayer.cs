using UnityEngine;
using System.Collections;
using System;
using UnityStandardAssets.Characters.FirstPerson;
using System.Collections.Generic;

namespace CpvrLab.VirtualTable {


    /// <summary>
    /// Prototype implementation of a FirstPersonPlayer. This player is controlled by mouse and 
    /// keyboard like a traditional first person game. 
    /// </summary>
    [RequireComponent(typeof(FirstPersonPlayerInput))]
    public class FirstPersonPlayer : GamePlayer {

        [Header("First Person Properties")]
        [Range(0.5f, 3f)]
        public float pickupRange;
        public GameObject attachPoint;
        public Transform head;
        public GameObject fpsGUIPrefab;
        protected GameObject _fpsGUIInstance;

        protected UsableItem _currentlyEquipped = null;
        protected MovableItem _currentlyHolding = null;
        protected Quaternion _currentlyEquippedInitialRot = Quaternion.identity;
        protected FirstPersonPlayerInput _playerInput;

        public void Awake()
        {

            AddAttachmentSlot(attachPoint);
        }

        public override void OnStartClient()
        {
            base.OnStartClient();

            // add attachment slots on all clients
        }

        public override void OnStartLocalPlayer()
        {
            base.OnStartLocalPlayer();

            // temporary solution?
            GetComponent<CharacterController>().enabled = true;
            GetComponent<FirstPersonController>().enabled = true;
            head.GetComponent<AudioListener>().enabled = true;
            
            _playerInput = GetComponent<FirstPersonPlayerInput>();
			_playerInput.SetControllerRole(PlayerInput.ControllerRole.Main);
            
            // add the player input component to our attachment slot
            // todo: this implementation doesn't seem that good
            //       although we don't need a sanity check here it still feels dangerous and wrong.
            FindAttachmentSlot(attachPoint).input = _playerInput;

            // instantiate the GUI
            _fpsGUIInstance = Instantiate(fpsGUIPrefab);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            Destroy(_fpsGUIInstance);
        }

        protected override void OnObserverStateChanged(bool val)
        {
            base.OnObserverStateChanged(val);
            _fpsGUIInstance.SetActive(!val);
        }

        void Update()
        {
            if(!isLocalPlayer)
                return;

			if (useDefaultItemInteraction)
			{
				if (_currentlyEquipped != null)
				{
					if (Input.GetKeyDown(KeyCode.E))
					{
						Unequip(_currentlyEquipped); // remove item from equipped input slot
					}
				}
				if (_currentlyHolding != null)
				{
					if (Input.GetKeyUp(KeyCode.E))
						ReleaseMovableItem(_currentlyHolding);
				}
				else
				{
					// handle object pickups
					HandleItemInteractions();
				}
			}

            // test for model switching
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                NextLocalModel();
                NextRemoteModel();
            }
        }

        void HandleItemInteractions()
        {
            Ray ray = new Ray(head.position, head.forward);

            Debug.DrawLine(head.position, head.position + head.forward * pickupRange, Color.red);

            RaycastHit hit;
            if(!Physics.Raycast(ray, out hit, pickupRange))
                return;
            
            // todo: store the tags in some kind of const global variable!
            if(hit.transform.CompareTag("UsableItem")) {
                HandleUsableItem(hit);
            }
            
            // todo: store the tags in some kind of const global variable!
            if(hit.transform.CompareTag("MovableItem")) {
                HandleMovableItem(hit);
            }
        }

        void HandleUsableItem(RaycastHit hit)
		{
			// Don't equip a new item, if we are already have one equipped.
			if (_currentlyEquipped != null)
				return;
			
			// check if the object has the required UsableItem component attached
			var usableItem = hit.transform.GetComponent<UsableItem>();
            if(usableItem == null)
                return;            

            // Do we want to pick the item up?
            if(Input.GetKeyDown(KeyCode.E)) {
                Equip(usableItem);
            }
        }

        void HandleMovableItem(RaycastHit hit)
		{
			// Don't grab a new item, if we are already holding one.
			if (_currentlyHolding != null)
				return;

            // check if the object has the required MovableItem component attached
            var movableItem = hit.transform.GetComponent<MovableItem>();
            if(movableItem == null)
                return;

            if(Input.GetKeyDown(KeyCode.E)) {
                GrabMovableItem(movableItem);
            }
        }

        void GrabMovableItem(MovableItem item)
        {
            // todo: grab the object
            //       best way would be to add the functionality into holdable item
            //       attach a fixed joint (or maybe a loose joint with the object hanging around?)
            //       anyway, what is   
            _currentlyHolding = item;
            //item.Attach(attachPoint.GetComponent<Rigidbody>());
            CmdGrabMovableItem(item.gameObject, 0);
        }

        void ReleaseMovableItem(MovableItem item)
        {
            _currentlyHolding = null;
            //item.Detach();
            CmdReleaseMovableItem(item.gameObject, 0);
        }

        protected override PlayerInput GetMainInput()
        {
            return _playerInput;
        }

        public override PlayerInput GetOtherInput(PlayerInput input)
        {
            return null;
        }
		public override IEnumerable<PlayerInput> GetAllInputs()
		{
			yield return _playerInput;
		}
		protected override void OnEquip(AttachmentSlot slot)
        {
            _currentlyEquipped = slot.item;
            slot.item.transform.localRotation = Quaternion.identity;
            
            _currentlyEquippedInitialRot = Quaternion.FromToRotation(slot.item.transform.worldToLocalMatrix * slot.item.aimDir, Vector3.forward);
            _currentlyEquipped.transform.localRotation = _currentlyEquippedInitialRot;
        }
        
        protected override void OnUnequip(UsableItem item)
        {
            _currentlyEquipped = null;
        }
        
    } // class
    
} // namespace