using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using CpvrLab.ArcherGuardian.Scripts.IK;

namespace CpvrLab.VirtualTable {

    /// <summary>
    /// Base class for all usable items. A usable item is an object that can be equipped by a GamePlayer.
    /// When equipped by a GamePlayer a UsableItem will be attachd to an attachment point defined by the
    /// GamePlayer and receive input from one of the GamePlayer's PlayerInput components.
    /// 
    /// todo:   properly sync the equip state of this item. If a player connects late he must be able
    ///         to know which items are in use and which ones he can safely pick up.
    /// </summary>
    [RequireComponent(typeof(Rigidbody), typeof(NetworkTransform))]
    public class UsableItem : NetworkBehaviour {

        public Transform attachPoint;

        // Some objects might want to
        public Transform aimDirTransform; // todo: make aimDirTransform private but settable in the editor
        public Vector3 aimDir { get { return (aimDirTransform != null) ? aimDirTransform.forward : transform.forward; } }

        protected PlayerInput _input;
		protected PlayerOutput _output;
		protected Transform _prevParent = null;
        protected GamePlayer _owner = null;
        [SyncVar] protected GameObject _ownerGameObject = null;
        public bool isInUse { get { return _owner != null; } }
        [SyncVar] protected bool _unequipDone;
        [SyncVar(hook ="OnVisibilityChanged")] public bool isVisible = true;
        [SyncVar] public bool inputEnabled = true;

        private AGHandEffector _handEffector;

        private void OnVisibilityChanged(bool value)
        {
            isVisible = value;
            for(int i = 0; i < transform.childCount; i++)
                transform.GetChild(i).gameObject.SetActive(value);
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            if (_ownerGameObject != null)
            {
                _owner = _ownerGameObject.GetComponent<GamePlayer>();
                _handEffector = _owner.PlayerModel.GetComponentInChildren<AGHandEffector>();
            }
        }

        // tempararily used for debugging purposes
        public override void OnStartAuthority()
        {
            base.OnStartAuthority();
        }

        // tempararily used for debugging purposes
        public override void OnStopAuthority()
        {
            base.OnStopAuthority();
        }

        /// <summary>
        /// Attaches this usable item to a given attachment point.
        /// Currently this is done by setting the rigidbody of the item to
        /// be kinematic and childing it to the attach GameObject.
        /// 
        /// Concrete GamePlayers can change the local position and rotation of the item
        /// by overriding GamePlayer.OnEquip and changing the values there.
        /// </summary>
        /// <param name="attach"></param>
        [Client] public void Attach(GameObject attach)
        {
            _prevParent = transform.parent;

            transform.parent = attach.transform;
            transform.localRotation = Quaternion.identity;
            transform.localPosition = Vector3.zero;

            //move the item to the specified attachPoint
            if (attachPoint != null)
            {
                transform.localPosition = attachPoint.transform.localPosition;
            }

            // "disable" rigidbody by setting it to kinematic
            var rb = GetComponent<Rigidbody>();
            rb.isKinematic = true;
        }

        /// <summary>
        /// Detaches an item from the current attachment point.
        /// </summary>
        [Client] public void Detach()
        {
            // return if we're not attached to anything.
            if (transform.parent == _prevParent)
                return;
            
            transform.parent = _prevParent;
            var rb = GetComponent<Rigidbody>();
            rb.isKinematic = false;
        }

		/// <summary>
		/// Assign an owner of this UsableItem. If the local GamePlayer is the owner then input/output will
		/// contain a non null value. Else only owner will be assigned.
		/// </summary>
		/// <param name="owner"></param>
		/// <param name="input"></param>
		/// <param name="output"></param>
		[Client] public void AssignOwner(GamePlayer owner, PlayerInput input, PlayerOutput output) {
            _owner = owner;
            _ownerGameObject = _owner.gameObject;
            _input = input;
			_output = output;

            OnEquip();
        }

        /// <summary>
        /// poses hands from model after attaching item to player
        /// </summary>
        /// <param name="input"></param>
        public void PoseHands(PlayerInput input)
        {
            if (input == null) return;

            if (UseHandEffector() != AGHandEffector.PoseableObjects.Default)
            {
               if (input.IsInControllerRole(PlayerInput.ControllerRole.LeftHand))
                {
                    CmdActiveHandEffector(UseHandEffector(), AGHandEffector.Hand.Left);
                }
                if (input.IsInControllerRole(PlayerInput.ControllerRole.RightHand))
                {
                    CmdActiveHandEffector(UseHandEffector(), AGHandEffector.Hand.Right);
                }
            }
        }

        /// <summary>
        /// deactivates the specified HandPoser - Set the effector to the default goal
        /// </summary>
        /// <param name="input"></param>
        public void DeposeHands(PlayerInput input)
        {
            if (input == null) return;

            if (UseHandEffector() != AGHandEffector.PoseableObjects.Default)
            {
                if (input.IsInControllerRole(PlayerInput.ControllerRole.LeftHand))
                {
                    CmdDeactiveHandEffector( AGHandEffector.Hand.Left);
                }
                if (input.IsInControllerRole(PlayerInput.ControllerRole.RightHand))
                {
                    CmdDeactiveHandEffector(AGHandEffector.Hand.Right);
                }
            }
        }

        [Command]
        void CmdActiveHandEffector(AGHandEffector.PoseableObjects obj, AGHandEffector.Hand hand)
        {
            RpcActiveHandEffector(obj, hand);
        }

        [Command]
        void CmdDeactiveHandEffector(AGHandEffector.Hand hand)
        {
            RpcDeactiveHandEffector(hand);
        }

        [ClientRpc]
        void RpcActiveHandEffector(AGHandEffector.PoseableObjects obj, AGHandEffector.Hand hand)
        {
            _handEffector = _owner.PlayerModel.GetComponentInChildren<AGHandEffector>();
            _handEffector.ActivateHandEffector(obj, hand);
        }

        [ClientRpc]
        void RpcDeactiveHandEffector(AGHandEffector.Hand hand)
        { 
            _handEffector.DeactiveHandEffector(hand);
        }

        /// <summary>
        ///choose the posed hand for the this usableItem
        /// </summary>
        [Client]
        protected virtual AGHandEffector.PoseableObjects UseHandEffector()
        {
            return AGHandEffector.PoseableObjects.Default;
        }

        /// <summary>
        /// OnEquip is the initialization method for concrete UsableItems and is called on all client representations.
        /// 
        /// todo: find a better name for this.
        /// </summary>
        [Client] protected virtual void OnEquip()
        {
        }

        [Client] public void ClearOwner()
        {
            OnUnequip(); // ??? Must be called before _input is set to null?
            _owner = null;
            _ownerGameObject = null;
            _input = null;
			_output = null;
        }

        /// <summary>
        /// OnUnequip is the last method called before this item loses its owner. Used for item cleanup if necessary.
        /// </summary>
        [Client] protected virtual void OnUnequip()
        {
            if (_input)
                _input.HideModel(false);
        }



        // Release client authority
        // todo:    implement a more reliable solution for the problem of releasing authority
        //          after OnUnequip has been called.
        //          the problem: OnUnequip is called, a concrete UsableObject might want to
        //          send off some final commands in there and have them executed on all clients
        //          if we'd release authority in OnUnequip the RPC calls wouldn't trigger anymore
        //          resulting in mismatches for different clients
        //          The below implementation won't fix this in all instances
        //          all it does is do an additional cycle of client -> server -> client
        //          before actually releasing authority. But it works for now and I have more
        //          immediate concerns right now than implementing this properly.
        //          You're welcome 'future me'

		// OnUnequip has already been called. No need to delay release of authority.
        public void ReleaseAuthority() { if(hasAuthority) CmdReleaseAuthority(); }
        //[Command] private void CmdReleaseAuthorityDelay() { RpcReleaseAuthorityDelay(); }
        //[ClientRpc] private void RpcReleaseAuthorityDelay() { if(hasAuthority) CmdReleaseAuthority(); }        
        [Command] private void CmdReleaseAuthority()
        {
            var nId = GetComponent<NetworkIdentity>();
            nId.RemoveClientAuthority(nId.clientAuthorityOwner);
        }

    }
}