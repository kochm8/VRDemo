using CpvrLab.VirtualTable;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace CpvrLab.ArcherGuardian.Scripts.Items
{
	/// <summary>
	/// Simple inventory to stash and take items.
	/// </summary>
	public class InventoryManager : NetworkBehaviour
	{
		#region members
		#region public
		public GameObject Inventory;
		#endregion public
		private GamePlayer _player;
		// Items are only added on the client!
		private List<InventoryItem> _inventoryItems = new List<InventoryItem>();
		private bool _inventoryIsOpen = false;
		private const float TouchPadPos = 0.7f;
		#endregion members

		#region public
		/// <summary>
		/// Stashes the specified item in the inventory.
		/// </summary>
		/// <param name="item">Item to stash.</param>
		/// <returns>True, if item could be stashed.</returns>
		[Client]
		public bool StashItem(InventoryItem item)
		{
			if (item == null)
			{
				Debug.LogError("StashItem: item is null!");
				return false;
			}
			else if (_inventoryItems.Contains(item))
			{
				Debug.LogError("StashItem: Trying to stash an item, that is already stashed.");
				return false;
			}

			_inventoryItems.Add(item);

			// Check for host (client and server)
			if (isServer)
				item.OnStash(Inventory);
			else
				CmdOnStashItem(item.gameObject);

			return true;
		}
		/// <summary>
		/// Takes the item with the specified index from the inventory.
		/// </summary>
		/// <param name="index">Index of the item to take.</param>
		/// <returns>True, if item could be taken.</returns>
		[Client]
		public InventoryItem TakeItem(int index)
		{
			if (_inventoryItems.Count <= index)
			{
				Debug.LogError("TakeItem: Index out of range!");
				return null;
			}

			var item = _inventoryItems[index];
			return TakeItemInternal(item) ? item : null;
		}
		/// <summary>
		/// Takes the specified item from the inventory.
		/// </summary>
		/// <param name="item">Item to take.</param>
		[Client]
		public bool TakeItem(InventoryItem item)
		{
			if (item == null)
			{
				Debug.LogError("TakeItem: item is null!");
				return false;
			}
			else if (!_inventoryItems.Contains(item))
			{
				Debug.LogError("TakeItem: Trying to take an item, that is not stashed.");
				return false;
			}

			return TakeItemInternal(item);
		}
		#endregion public

		private void Start()
		{
			if (isLocalPlayer)
			{
				var player = GetComponent<GamePlayer>();
				if (player == null)
				{
					Debug.LogError("Start: GamePlayer not found!");
				}
				else if (gameObject != player.gameObject)
				{
					Debug.LogError("AGPlayer: Trying to add a player with a different gameobject!");
				}

				_player = player;
			}
		}
		private void Update()
		{
			if (!isLocalPlayer)
				return;

			foreach (var input in _player.GetAllInputs())
			{
				HandleInventory(input);
			}
		}

		/// <summary>
		/// Handles inventory interactions.
		/// </summary>
		/// <param name="input">Input to check.</param>
		private void HandleInventory(PlayerInput input)
		{
			if (!_inventoryIsOpen && input.GetActionDown(PlayerInput.ActionCode.OpenInventory))
			{
				// check for press on the border of the touchpad
				if (!input.SupportsAxisVector(PlayerInput.AxisCode.Touchpad) || input.GetAxisVector(PlayerInput.AxisCode.Touchpad).magnitude > TouchPadPos)
				{
					try
					{
						_inventoryIsOpen = true;
						// Check attachment slot of current input
						var oldUsable = _player.GetUsableItemFrom(input);

						// Only unequip if no new item has to be equipped.
						// This way there is no additional client->server->clients cycle required.
						var unequipRequired = true;

						// Get all items in the inventory (before a new item is added).
						var items = GetItems();
						//TODO: Display the items so the user can actually choose which one he wants.
						var item = items.FirstOrDefault();
						if (item != null)
						{
							if (TakeItem(item))
							{
								var usable = item.GetComponent<UsableItem>();
								if (usable != null)
								{
									// Equip the current item.
									_player.Equip(input, usable, true);
									// Unequip no longer necessary.
									unequipRequired = false;
								}
							}
						}

						// Store old item in the inventory
						if (oldUsable != null)
						{
							// Check if usable has to be unequipped.
							if (unequipRequired)
							{
								_player.Unequip(oldUsable);
							}
							// Check if the player can store the item in his inventory.
							var itemToStore = oldUsable.GetComponent<InventoryItem>();
							if (itemToStore != null)
							{
								StashItem(itemToStore);
							}
						}
					}
					finally
					{
						_inventoryIsOpen = false;
					}
				}
			}
		}

		/// <summary>
		/// Removes the specified item from the inventory.
		/// </summary>
		/// <param name="item">Item to remove</param><
		/// <returns>Is item removed.</returns>
		private bool TakeItemInternal(InventoryItem item)
		{
			if (_inventoryItems.Remove(item))
			{
				// Inform the item, that it has been removed from the inventory.
				// Check for host (client and server)
				if (isServer)
					item.OnTake();
				else
					CmdOnTakeItem(item.gameObject);
				return true;
			}
			return false;
		}

		[Command]
		private void CmdOnStashItem(GameObject itemGO)
		{
			var invItem = GetInventoryItem(itemGO);
			if (invItem == null)
				return;

			// Notify about item about stashing.
			invItem.OnStash(Inventory);
		}
		/// <summary>
		/// Inform server about taking an item out of the inventory.
		/// </summary>
		/// <param name="itemNetId">NetworkInstanceId</param>
		[Command]
		private void CmdOnTakeItem(GameObject itemGO)
		{
			var invItem = GetInventoryItem(itemGO);
			if (invItem == null)
				return;

			// Notify about item about taking.
			invItem.OnTake();
		}
		/// <summary>
		/// Inform server about stashing an item in the inventory.
		/// </summary>
		/// <param name="itemNetId">NetworkInstanceId</param>
		private InventoryItem GetInventoryItem(GameObject itemGO)
		{
			// Check if InventoryItem.
			var inventoryItem = itemGO.GetComponent<InventoryItem>();
			if (inventoryItem == null)
			{
				Debug.LogError("GetInventoryItem: itemGO is not a InventoryItem.");
				return null;
			}
			return inventoryItem;
		}

		/// <summary>
		/// Returns all items currently stashed in the inventory.
		/// </summary>
		/// <returns>Enumerator to iterate through all items.</returns>
		[Client]
		private ReadOnlyCollection<InventoryItem> GetItems()
		{
			return _inventoryItems.AsReadOnly();
		}
	}
}
