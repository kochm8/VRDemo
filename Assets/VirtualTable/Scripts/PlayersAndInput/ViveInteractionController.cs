using UnityEngine;
using System.Collections;
using Valve.VR;


namespace CpvrLab.VirtualTable {


    public delegate void UsableItemInteraction(PlayerInput input, UsableItem target);
    public delegate void MovableItemInteraction(PlayerInput input, MovableItem target);

    // todo:    separation of usable and movable items is tedious
    //          could this be simplified?
    // 
    // note:    at the moment only usable items work

    /// <summary>
    /// ViveInteractionController can be added to a vive controller to interact with UsableItems.
    /// This class will notify VivePlayer about picked up items etc.
    /// </summary>
    [RequireComponent(typeof(SphereCollider), typeof(Rigidbody))]
    public class ViveInteractionController : MonoBehaviour {

        public event UsableItemInteraction UsableItemPickedUp;
        public event UsableItemInteraction UsableItemDropped;
        public event MovableItemInteraction MovableItemPickedUp;
        public event MovableItemInteraction MovableItemDropped;

        public PlayerInput input;
        public float pickupRadius = 0.1f;
        private SteamVR_Controller.Device _device;

        // currently holding an item?
        private bool _holdingItem = false;
        private UsableItem _currentlyEquipped = null;
        private MovableItem _activeMovableItem = null;

		public bool UseDefaultItemInteraction { get; set; }
        // Use this for initialization
        void Start()
        {
            // todo: sanity checks
            var trackedObject = GetComponent<SteamVR_TrackedObject>();
            var index = (int)trackedObject.index;
            _device = SteamVR_Controller.Input(index);

            var col = GetComponent<SphereCollider>();
            col.radius = pickupRadius;
            col.isTrigger = true;

            var rb = GetComponent<Rigidbody>();
            rb.isKinematic = true;
        }
        
        void OnTriggerEnter(Collider other)
        {
            if(!UseDefaultItemInteraction || other.attachedRigidbody == null)
                return;

            // todo: store these tags in a global common file as static const etc...
            bool usable = other.attachedRigidbody.CompareTag("UsableItem");
            bool movable = other.attachedRigidbody.CompareTag("MovableItem");
            
            if(!usable && !movable)
                return;

            if(_holdingItem)
                return;
                                    
            if(usable) {

                var item = other.attachedRigidbody.gameObject.GetComponent<UsableItem>();
                if (item != null)
                {
                    if (item.isInUse)
                        return;

                    _currentlyEquipped = item;
                    if (UsableItemPickedUp != null)
                    {
                        Debug.Log("UsableItemPickedUp");
                        UsableItemPickedUp(input, _currentlyEquipped);
                    }

                    _holdingItem = true;
                }
            }
            else if(movable)
            {
                _activeMovableItem = other.attachedRigidbody.gameObject.GetComponent<MovableItem>();
            }
        }
        

        void Update()
        {
			if (!UseDefaultItemInteraction)
				return;

            if (_device.GetPressDown(EVRButtonId.k_EButton_Grip))
            {
                if (_holdingItem)
                {
                    if (UsableItemDropped != null)
                        UsableItemDropped(input, _currentlyEquipped);

                    _currentlyEquipped = null;
                    _holdingItem = false;
                }
            }

            if (_activeMovableItem != null)
            {
                if (_device.GetPressDown(EVRButtonId.k_EButton_SteamVR_Trigger))
                {
                    _holdingItem = true;
                    if(MovableItemPickedUp != null)
                        MovableItemPickedUp(input, _activeMovableItem);
                }
                else if (_device.GetPressUp(EVRButtonId.k_EButton_SteamVR_Trigger))
                {
                    _holdingItem = false;

                    if (MovableItemDropped != null)
                        MovableItemDropped(input, _activeMovableItem);

                    _activeMovableItem = null;

                }
            }
        }
    }
}