using UnityEngine;
using UnityEngine.Networking;

namespace CpvrLab.VirtualTable.Scripts
{

	/// <summary>
	/// Base class for all wearable equipment. A wearable item is an object that can be equipped by a GamePlayer.
	/// When equipped by a GamePlayer a WearableItem will be attachd to a attachment point defined by the GamePlayer.
	/// </summary>
	[RequireComponent(typeof(NetworkTransform))]
	public class WearableItem : NetworkBehaviour
	{
		#region members
		public Transform AttachPoint;
		[SyncVar(hook = "OnVisibilityChanged")]
		public bool IsVisible = true;
		[SyncVar]
		private GameObject _ownerGameObject = null;
		#endregion members

		#region properties
		public bool IsInUse { get { return Owner != null; } }

		protected Transform Transform { get; private set; }
		protected Transform PrevParent { get; set; }
		protected GamePlayer Owner { get; set; }
		#endregion properties

		private void OnVisibilityChanged(bool value)
		{
			IsVisible = value;
			for (int i = 0; i < Transform.childCount; i++)
				Transform.GetChild(i).gameObject.SetActive(value);
		}

		public override void OnStartClient()
		{
			base.OnStartClient();
			if (_ownerGameObject != null)
			{
				Owner = _ownerGameObject.GetComponent<GamePlayer>();
			}
		}
		protected virtual void Awake()
		{
			Transform = transform;
		}

		/// <summary>
		/// Attaches this usable item to a given attachment point.
		/// Currently this is done by setting the rigidbody of the item to
		/// be kinematic and childing it to the attach GameObject.
		/// 
		/// Concrete GamePlayers can change the local position and rotation of the item
		/// by overriding GamePlayer.OnEquip and changing the values there.
		/// </summary>
		/// <param name="attachPoint">GameObject, to which the current item should be attach to.</param>
		[Client]
		public void Equip(GameObject attachPoint)
		{
			PrevParent = Transform.parent;

			Transform.parent = attachPoint.transform;
			Transform.localRotation = Quaternion.identity;
			Transform.localPosition = Vector3.zero;

			//move the item to the specified attachPoint
			if (AttachPoint != null)
			{
				Transform.localPosition = -AttachPoint.transform.localPosition;
			}
		}

		/// <summary>
		/// Unequips an item.
		/// </summary>
		[Client]
		public void Unequip()
		{
			// return if we're not attached to anything.
			if (Transform.parent == PrevParent)
				return;

			Transform.parent = PrevParent;
		}

		/// <summary>
		/// Assign an owner of this UsableItem. If the local GamePlayer is the owner then input/output will
		/// contain a non null value. Else only owner will be assigned.
		/// </summary>
		/// <param name="owner">GamePlayer</param>
		[Client]
		public void AssignOwner(GamePlayer owner)
		{
			Owner = owner;
			_ownerGameObject = Owner.gameObject;

			OnAssignOwner();
		}
		/// <summary>
		/// Clear current owner.
		/// </summary>
		[Client]
		public void ClearOwner()
		{
			OnClearOwner();
			Owner = null;
			_ownerGameObject = null;
		}

		/// <summary>
		/// OnAssignOwner is called whenever a new owner is assigned.
		/// </summary>
		[Client]
		protected virtual void OnAssignOwner() { }
		/// <summary>
		/// OnClearOwner is called when the owner is removed.
		/// </summary>
		[Client]
		protected virtual void OnClearOwner() { }

		public void ReleaseAuthority()
		{
			if (hasAuthority)
				CmdReleaseAuthority();
		}

		[Command]
		private void CmdReleaseAuthority()
		{
			var nId = GetComponent<NetworkIdentity>();
			nId.RemoveClientAuthority(nId.clientAuthorityOwner);
		}
	}
}