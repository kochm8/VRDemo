using CpvrLab.ArcherGuardian.Scripts.Items;
using CpvrLab.VirtualTable.Scripts;
using CpvrLab.VirtualTable.Scripts.ModelScripts;
using System;
using UnityEngine;
using UnityEngine.Networking;

namespace CpvrLab.ArcherGuardian.Scripts.PlayersAndIO
{
	public class Archer : AGPlayer
	{
		#region members
		private Bow _bow;
		private float _staminaForBowStringPull = 10f;
		#endregion members

		#region properties
		private ArcherSettings Settings { get; set; }
		#endregion properties

		#region public
		#endregion public

		#region overrides
		protected override void Start()
		{
			base.Start();
		}
		protected override void Update()
		{
			base.Update();
		}
		public override void InitFromSettings(AGPlayerSettings settings)
		{
			base.InitFromSettings(settings);

			Settings = settings as ArcherSettings;
			if (Settings == null)
			{
				Debug.LogError("InitFromSettings: ArcherSettings missing!");
				return;
			}
			if (isServer)
			{
				// Create a new bow for the archer.
				var bowGO = (GameObject)Instantiate(Settings.BowPrefab, Vector3.zero, Quaternion.identity);
				NetworkServer.Spawn(bowGO);
				// Check for equipped shield
				_bow = bowGO.GetComponent<Bow>();
				if (_bow != null)
				{
					_bow.BowStringPulled += BowStringPulled;
				}
				TargetAddInventoryItem(Player.connectionToClient, bowGO);

				// Create a quiver for the archer.
				var quiverGO = (GameObject)Instantiate(Settings.QuiverPrefab, transform.position, Quaternion.identity);
				NetworkServer.Spawn(quiverGO);

				var wearable = quiverGO.GetComponent<WearableItem>();
				Player.EquipWearable(EquipmentSlotKey.Back, wearable, true);
			}
		}

		/// <summary>
		/// Assign owner / authority to local player. Temporary method.
		/// </summary>
		/// <param name="itemGO"><seealso cref="GameObject"/> of the item (<seealso cref="InventoryItem"/>).</param>
		[ClientRpc]
		protected void RpcAssignOwner(GameObject itemGO)
		{
			AssignOwner(itemGO);
		}
		protected void AssignOwner(GameObject itemGO)
		{
			// Check if UsableItem.
			var usable = itemGO.GetComponent<VirtualTable.UsableItem>();
			if (usable == null)
			{
				Debug.LogError("RpcAssignOwner: itemGO is not a UsableItem.");
				return;
			}
			else
			{
				usable.AssignOwner(Player, null, null);
			}
		}
		#endregion overrides

		protected override void OnDestroy()
		{
			base.OnDestroy();
			if (isServer)
			{
				// Remove event handlers
				if (_bow != null)
				{
					_bow.BowStringPulled -= BowStringPulled;
				}
			}
		}
		/// <summary>
		/// Event handler for bow string pulled events. This is called during the time the player is pulling on the bowstring.
		/// </summary>
		/// <param name="sender">Source of the pull (instance of a script).</param>
		/// <param name="args">Pull arguments (<seealso cref="BowStringPullEventArgs"/>).</param>
		private void BowStringPulled(object sender, BowStringPullEventArgs args)
		{
			if (!PerformAction(Convert.ToUInt32(_staminaForBowStringPull * args.Force), true))
			{
				// Cancel the pulling.
				args.Cancel = true;
			}
		}
	}
}
