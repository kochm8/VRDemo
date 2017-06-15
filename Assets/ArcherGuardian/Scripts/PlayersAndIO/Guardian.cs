using CpvrLab.ArcherGuardian.Scripts.Items;
using CpvrLab.ArcherGuardian.Scripts.Items.Arrows;
using System;
using UnityEngine;
using UnityEngine.Networking;

namespace CpvrLab.ArcherGuardian.Scripts.PlayersAndIO
{
	public class Guardian : AGPlayer
	{
		#region members
		private Shield _shield;
		private const float StaminaForBlock = 50f;
		#endregion members

		#region properties
		/// <summary>
		/// Only set on the server!
		/// </summary>
		private GuardianSettings Settings { get; set; }
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

			Settings = settings as GuardianSettings;
			if (Settings == null)
			{
				Debug.LogError("InitFromSettings: GuardianSettings missing!");
				return;
			}
			if (isServer)
			{
				// Create a new shield for the guardian.
				var shieldGO = (GameObject)Instantiate(Settings.ShieldPrefab, Vector3.zero, Quaternion.identity);
				NetworkServer.Spawn(shieldGO);

				// Check for equipped shield
				_shield = shieldGO.GetComponent<Shield>();
				if (_shield != null)
				{
					_shield.Block += ShieldBlock;
				}

				TargetAddInventoryItem(Player.connectionToClient, shieldGO);
			}
		}
		#endregion overrides

		protected override void OnDestroy()
		{
			base.OnDestroy();
			if (isServer)
			{
				if (_shield != null)
				{
					_shield.Block -= ShieldBlock;
				}
			}
		}
		/// <summary>
		/// Event handler for blocks. This is called each time the player blocks an object with his shield.
		/// </summary>
		/// <param name="sender">Source of the hit (instance of a script).</param>
		/// <param name="args">Block arguments (<seealso cref="ShieldBlockEventArgs"/>).</param>
		private void ShieldBlock(object sender, ShieldBlockEventArgs args)
		{
			UseStamina(Convert.ToUInt32(StaminaForBlock * args.Force));
		}
	}
}
