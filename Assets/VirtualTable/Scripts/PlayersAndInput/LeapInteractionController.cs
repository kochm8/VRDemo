using UnityEngine;
using System.Collections;


namespace CpvrLab.VirtualTable
{

    public class LeapInteractionController : MonoBehaviour
    {
        public event UsableItemInteraction UsableItemPickedUp;
        public event UsableItemInteraction UsableItemDropped;
        public event MovableItemInteraction MovableItemPickedUp;
        public event MovableItemInteraction MovableItemDropped;

        public PlayerInput input;
        public float pickupRadius = 0.1f;

        // currently holding an item?
        private bool _holdingItem = false;
        private UsableItem _currentlyEquipped = null;
        private MovableItem _activeMovableItem = null;

        void Start()
        {
            var col = GetComponent<SphereCollider>();
            col.radius = pickupRadius;
            col.isTrigger = true;

            var rb = GetComponent<Rigidbody>();
            rb.isKinematic = true;
        }


        void OnTriggerEnter(Collider other)
        {
            if (other.attachedRigidbody == null)
                return;
            
            // todo: store these tags in a global common file as static const etc...
            bool usable = other.attachedRigidbody.CompareTag("UsableItem");
            bool movable = other.attachedRigidbody.CompareTag("MovableItem");

            if (!usable && !movable)
                return;

            if (_holdingItem)
                return;

            if (usable)
            {
                var item = other.attachedRigidbody.gameObject.GetComponent<UsableItem>();
                if (item != null)
                {
                    if (item.isInUse)
                        return;

                    _currentlyEquipped = item;
                    if (UsableItemPickedUp != null)
                        UsableItemPickedUp(input, _currentlyEquipped);

                    _holdingItem = true;
                }
            }
            else if (movable)
            {
                _activeMovableItem = other.attachedRigidbody.gameObject.GetComponent<MovableItem>();
            }
        }


        void Update()
        {
            // todo: add functionality to drop the item

            if (_activeMovableItem != null)
            {
                if (input.GetActionDown(PlayerInput.ActionCode.Button0))
                {
                    _holdingItem = true;
                    if (MovableItemPickedUp != null)
                        MovableItemPickedUp(input, _activeMovableItem);
                }
                else if (input.GetActionUp(PlayerInput.ActionCode.Button0))
                {
                    _holdingItem = false;

                    if (MovableItemDropped != null)
                        MovableItemDropped(input, _activeMovableItem);

                    _activeMovableItem = null;

                }
            }
        }
    } // class

}// namespace