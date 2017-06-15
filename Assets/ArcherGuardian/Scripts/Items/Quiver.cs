using CpvrLab.VirtualTable;
using CpvrLab.VirtualTable.Scripts;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace CpvrLab.ArcherGuardian.Scripts.Items
{

	/// <summary>
	/// Quiver to spawn arrows from.
	/// </summary>
	public class Quiver : WearableItem
	{
		#region members
		#region public
		public GameObject ArrowPrefab;
		#endregion public

		private bool _checkCollider = false;
		private Queue<PlayerInput> _inputsRequestingArrows = new Queue<PlayerInput>();
		#endregion members

		private void Update()
		{
			// Only if the quiver is in use and the player has authority.
			if (!IsInUse || !hasAuthority)
				return;

			// Check if no collision is required to reload.
			if (_checkCollider)
				return;

			// Workaround for pc player
			PCPlayerReload();
		}

		protected override void OnAssignOwner()
		{
			base.OnAssignOwner();

			if (IsInUse && hasAuthority)
			{
				// Check if the user grabs the item or just presses a button.
				// Vive player grabs arrows, pc player presses a button.
				_checkCollider = Owner.GetAllInputs().Any(p => p.GetTrackedTransform() != null);
			}
		}

		[Client]
		private void OnTriggerStay(Collider other)
		{
			// Only if the quiver is in use and the player has authority.
			if (!IsInUse || !hasAuthority || !_checkCollider)
				return;

			foreach (var input in Owner.GetAllInputs())
			{
				// Check if input attachpoint is occupied.
				if (Owner.GetUsableItemFrom(input) != null)
					continue;

				// Check if the input is already waiting for an arrow object.
				if (_inputsRequestingArrows.Contains(input))
					continue;

				// Get the transform of the input
				var inputTrans = input.GetTrackedTransform();
				if (inputTrans == null)
				{
					// TODO: Change this...
					// Special handling for FPSPlayer
					inputTrans = Owner.transform.FindChild("FirstPersonCharacter/EquipAttachPoint");
					if (inputTrans == null)
						return;
				}
				// Check if the collider is the current input device.
				if (inputTrans == other.transform)
				{
					// Check if the player wants to reload.
					if (input.GetActionDown(PlayerInput.ActionCode.Reload))
					{
						// Store the input that requested the spawning of the arrow.
						_inputsRequestingArrows.Enqueue(input);
						RequestArrowSpawn();
					}
					// Collider has been found
					break;
				}
			}
		}

		/// <summary>
		/// Check if pc player wants to reload.
		/// </summary>
		private void PCPlayerReload()
		{
			// The player is not required to move his input-controller to the quiver.
			foreach (var input in Owner.GetAllInputs())
			{
				if (input.GetActionDown(PlayerInput.ActionCode.Reload))
				{
					// Store the input that requested the spawning of the arrow.
					_inputsRequestingArrows.Enqueue(input);
					RequestArrowSpawn();
				}
			}
		}
		/// <summary>
		/// Requests a new arrow.
		/// </summary>
		private void RequestArrowSpawn()
		{
			if (!hasAuthority)
			{
				Debug.LogError("RequestArrowSpawn: Trying to spawn an arrow without authority!");
				return;
			}

			if (isServer)
			{
				var arrow = SpawnArrow();
				EquipArrow(arrow);
			}
			else
			{
				CmdRequestSpawnArrow();
			}
		}
		[Command]
		private void CmdRequestSpawnArrow()
		{
			//Only if the quiver has an owner
			if (Owner == null)
				return;

			var arrowGO = SpawnArrow();

			// Notify the owner of the quiver that the arrow has been spawned.
			TargetOnArrowSpawned(Owner.connectionToClient, arrowGO);
		}
		/// <summary>
		/// Spawns an arrow.
		/// </summary>
		private GameObject SpawnArrow()
		{
			// Spawn the arrow
			var arrow = (GameObject)Instantiate(ArrowPrefab, Vector3.zero, Quaternion.identity);
			NetworkServer.Spawn(arrow);
			return arrow;
		}
		/// <summary>
		/// Notifies a specific client that the requested arrow has been spawned.
		/// </summary>
		/// <param name="target">Client</param>
		/// <param name="arrowGO">Spawned arrow game object.</param>
		[TargetRpc]
		private void TargetOnArrowSpawned(NetworkConnection target, GameObject arrowGO)
		{
			EquipArrow(arrowGO);
		}
		/// <summary>
		/// Equips the arrow to the longest waiting input.
		/// </summary>
		/// <param name="arrowGO">Arrow game object</param>
		private void EquipArrow(GameObject arrowGO)
		{
			Debug.Log("EquipArrow" + " isServer " + isServer);
			if (!_inputsRequestingArrows.Any())
			{
				// No input is waiting for an arrow...
				Debug.LogError("OnArrowSpawned: No input found to attach the arrow to.");
				return;
			}
			var input = _inputsRequestingArrows.Dequeue();
			var usable = arrowGO.GetComponent<UsableItem>();
			// Equip the arrow. Only if the input is not occupied.
			Owner.Equip(input, usable, true);
			// Position the arrow where the trigger of the controller would be,
			// so it seems like the arrow is between the index finger and the middle finger.
			arrowGO.transform.localPosition = new Vector3(0f, 0f, -0.05f);
			arrowGO.transform.localRotation = Quaternion.Euler(80f, 0f, 0f);
		}
	}
}