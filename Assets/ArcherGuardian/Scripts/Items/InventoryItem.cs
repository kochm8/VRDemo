using CpvrLab.VirtualTable;
using System;
using UnityEngine;
using UnityEngine.Networking;

namespace CpvrLab.ArcherGuardian.Scripts.Items
{
	/// <summary>
	/// Inventory item types.
	/// </summary>
	public enum InventoryItemType
	{
		Usable,
		//Consumable,
		// Add more types here
	}
	/// <summary>
	/// Component for all objects that can be stored in an <seealso cref="Inventory"/>.
	/// TODO: Add methods to support a graphical version of the inventory.
	/// </summary>
	public class InventoryItem : NetworkBehaviour
	{
		#region members
		public InventoryItemType ItemType;

		private UsableItem _usable;
		#endregion members
		/// <summary>
		/// Called when the item is stashed in the inventory. 
		/// </summary>
		/// <param name="inventory">Inventory where the item is stashed.</param>
		[Server]
		public void OnStash(GameObject inventory)
		{
			switch (ItemType)
			{
				case InventoryItemType.Usable:
					OnStashUsable(inventory);
					break;
				default:
					throw new NotImplementedException("OnStash: ItemType is not implemented!");
			}
		}
		/// <summary>
		/// Called when the item is taken out of the inventory.
		/// </summary>
		[Server]
		public void OnTake()
		{
			switch (ItemType)
			{
				case InventoryItemType.Usable:
					OnTakeUsable();
					break;
				default:
					throw new NotImplementedException("OnTake: ItemType is not implemented!");
			}
		}

		//Note: Use Awake() and not Start() for this!
		//		Start() is called on the instantiated object just before it receives it's first Update().
		//		This item might get instatiated on the server and immediately stashed in the inventory.
		//		In such a case members may not yet be correctly initialized.
		private void Awake()
		{
			switch (ItemType)
			{
				case InventoryItemType.Usable:
					_usable = GetComponent<UsableItem>();
					break;
				default:
					throw new NotImplementedException("ItemType is not implemented!");
			}
		}

		/// <summary>
		/// Called when a usable item gets stashed in the inventory.
		/// </summary>
		[Server]
		private void OnStashUsable(GameObject inventory)
		{
			_usable.isVisible = false;
			_usable.inputEnabled = false;

			// Set the inventory game object as the parent (so the scene doesn't get cluttered with stashed items).
			_usable.transform.SetParent(inventory.transform);
		}
		/// <summary>
		/// Called when a usable item gets taken out of the inventory.
		/// </summary>
		[Server]
		private void OnTakeUsable()
		{
			_usable.isVisible = true;
			_usable.inputEnabled = true;
		}
	}
}