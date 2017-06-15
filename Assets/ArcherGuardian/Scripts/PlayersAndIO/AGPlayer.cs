using CpvrLab.ArcherGuardian.Scripts.AbilitySystem;
using CpvrLab.ArcherGuardian.Scripts.Items;
using CpvrLab.ArcherGuardian.Scripts.Items.Arrows;
using CpvrLab.ArcherGuardian.Scripts.Lobby;
using CpvrLab.ArcherGuardian.Scripts.Util.Interface;
using CpvrLab.VirtualTable;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace CpvrLab.ArcherGuardian.Scripts.PlayersAndIO
{
	public abstract class AGPlayer : NetworkBehaviour
	{
		#region members
		#region public
		#endregion public

		[SyncVar(hook = "HealthHook")]
		private uint _currentHealth;
		[SyncVar(hook = "StaminaHook")]
		private uint _currentStamina;
		[SyncVar(hook = "IsDeadHook")]
		private bool _isDead;

		private AGPlayerSettings _settings;
		private InventoryManager _inventory;

		private const float RegInterval = 2f;

		private const float StaminaForTeleportPerUnit = 6f;
		private IList<Teleporter> _teleporters;
		private AGAbilityManager abilityManager;

		#endregion members

		#region properties
		protected GamePlayer Player { get; private set; }

		/// <summary>
		/// True, if the player is dead
		/// </summary>
		public bool IsDead
		{
			get
			{
				return _isDead;
			}
		}
		#endregion properties

		#region public
		/// <summary>
		/// Apply the specified settings to the archer.
		/// </summary>
		/// <param name="settings">Settings (<seealso cref="AGPlayerSettings"/>)</param>
		public virtual void InitFromSettings(AGPlayerSettings settings)
		{
			_settings = settings;

			// Set min and max values for the health slider.
			Player.PlayerProperties.SetPropertyBarMinMax(GameObjectProperties.PropertyKey.Health, 0f, _settings.MaxHealth);
			// Set min and max values for the stamina slider.
			Player.PlayerProperties.SetPropertyBarMinMax(GameObjectProperties.PropertyKey.Stamina, 0f, _settings.MaxStamina);

			if (isServer)
			{
				// sync vars...
				_currentHealth = _settings.StartingHealth;
				_currentStamina = _settings.StartingStamina;
				_isDead = false;

				// Start regeneration coroutines
				StartCoroutine(Regenerate());
			}
			if (isLocalPlayer)
			{
				_teleporters = new List<Teleporter>();
				foreach (var input in Player.GetAllInputs())
				{
					var teleporter = input.GetComponent<Teleporter>();
					if (teleporter == null)
						teleporter = input.gameObject.AddComponent<Teleporter>();

					teleporter.InitFromSettings(settings);
					teleporter.SetPlayerInput(Player, input);
					teleporter.CanTeleport += CanTeleport;
					teleporter.Teleport += Teleport;

					_teleporters.Add(teleporter);
				}
			}
		}

		public virtual void RemoveComponent()
		{
			Debug.Log("AGPlayer: RemoveComponent, isServer: " + isServer.ToString());

			Destroy(this);
		}
		#endregion public

		void Awake()
		{
			AGLobbyPlayer[] LobbyPlayers = GameObject.FindObjectsOfType<AGLobbyPlayer>();
			foreach (var lobbyplayer in LobbyPlayers)
			{
				lobbyplayer.gameObject.SetActive(false);
			}
		}

		/// <summary>
		/// Possible issues with start:
		/// http://answers.unity3d.com/questions/1163108/unet-networkserverspawn-correcting-my-workflow.html
		/// At the moment no scripts are added so this should work.
		/// </summary>
		protected virtual void Start()
		{
			Debug.Log("AGPlayer - Start: isServer " + isServer + " isClient " + isClient + " isLocalPlayer " + isLocalPlayer);

			var player = GetComponent<GamePlayer>();
			if (player == null)
			{
				Debug.LogError("Start: GamePlayer not found!");
			}
			else if (gameObject != player.gameObject)
			{
				Debug.LogError("AGPlayer: Trying to add a player with a different gameobject!");
			}
			_inventory = GetComponent<InventoryManager>();

			Player = player;

			//register listener only on server
			if (isServer)
			{
				// Get the hitable.
				var hitable = Player.PlayerModel.GetComponent<Hitable>();
				if (hitable == null)
				{
					//FPS player has hitable on the player object instead of the model.
					hitable = Player.GetComponent<Hitable>();
				}
				hitable.Hit += HitableHit;
			}


			//init AbilityManager
			abilityManager = GetComponent<AGAbilityManager>();

			if (abilityManager != null)
			{
				abilityManager.SetPlayer(player);

				//Register event
				foreach (Ability ab in abilityManager.caracterAbilites.abilities)
				{
					ab.UseAbility += useAbility;
				}

			}
		}
		protected virtual void Update()
		{
			if (!isLocalPlayer)
				return;
		}

		protected virtual void OnDestroy()
		{
			StopAllCoroutines();

			if (_teleporters != null)
			{
				foreach (var tp in _teleporters)
				{
					tp.RemovePlayer();
					tp.CanTeleport -= CanTeleport;
					tp.Teleport -= Teleport;
				}
			}
			if (isServer)
			{
				// Remove event handlers
				var hitable = Player.PlayerModel.GetComponent<Hitable>();
				if (hitable != null)
				{
					hitable.Hit -= HitableHit;
				}
			}
		}

		#region inventory
		/// <summary>
		/// Add an item to the inventory of the player.
		/// </summary>
		/// <param name="target">Client connection</param>
		/// <param name="itemGO"><seealso cref="GameObject"/> of the item (<seealso cref="InventoryItem"/>).</param>
		[TargetRpc]
		protected void TargetAddInventoryItem(NetworkConnection target, GameObject itemGO)
		{
			if (!isLocalPlayer)
			{
				Debug.LogError("TargetAddInventoryItem - Wrong target connection specified.");
				return;
			}
			if (itemGO == null)
			{
				Debug.LogError("TargetAddInventoryItem - GameObject not found.");
			}

			// Check if InventoryItem.
			var inventoryItem = itemGO.GetComponent<InventoryItem>();
			if (inventoryItem == null)
			{
				Debug.LogError("RpcAddInventoryItem: item is not a InventoryItem.");
				return;
			}

			_inventory.StashItem(inventoryItem);
		}
		#endregion inventory

		#region teleport
		/// <summary>
		/// Event handler for teleportation events. 
		/// This handler is called when a <seealso cref="Teleporter"/> object fires a <seealso cref="Teleporter.Teleport"/> event.
		/// </summary>
		/// <param name="sender">Source of the teleport (instance of a script).</param>
		/// <param name="args">Teleport arguments (<seealso cref="TeleportEventArgs"/>).</param>
		private void Teleport(object sender, TeleportEventArgs args)
		{
			if (!PerformAction(Convert.ToUInt32(StaminaForTeleportPerUnit * args.Distance), true))
			{
				// Cancel the teleport.
				args.Cancel = true;
			}
		}
		/// <summary>
		/// Event handler to check if teleportation is possible. 
		/// This handler is called when a <seealso cref="Teleporter"/> object fires a <seealso cref="Teleporter.CanTeleport"/> event.
		/// </summary>
		/// <param name="sender">Source of the teleport (instance of a script).</param>
		/// <param name="args">Teleport arguments (<seealso cref="TeleportEventArgs"/>).</param>
		private void CanTeleport(object sender, TeleportEventArgs args)
		{
			// Check if the player has enough stamina for the teleport.
			args.Cancel = !CanPerformAction(Convert.ToUInt32(StaminaForTeleportPerUnit * args.Distance));
		}
		#endregion teleport

		#region hit
		/// <summary>
		/// Event handler for hits. This is called each time something hits the player.
		/// </summary>
		/// <param name="sender">Source of the hit (instance of a script).</param>
		/// <param name="args">Hit arguments (<seealso cref="HitableEventArgs"/>).</param>
		private void HitableHit(object sender, HitableEventArgs args)
		{
			switch (args.HitType)
			{
				case HitTypeKey.Explosion:
					TakeDamage(Convert.ToUInt32(args.Force));
					break;
				case HitTypeKey.Projectile:
					var arrow = sender as BaseArrow;
					if (arrow != null)
					{
						var force = Mathf.Clamp01(args.Force / arrow.MaxForce);

						// Double damage for headshots.
						if (args.HitInfo == HitInfoKey.Head)
							force *= 2;

						TakeDamage(Convert.ToUInt32(arrow.ImpactDamage * force));
					}
					break;
                case HitTypeKey.Zombie:
                    TakeDamage(Convert.ToUInt32(args.Force));
                    break;
            }
		}
		#endregion hit

		#region ability
		private void useAbility(object sender, AbiltyEventArgs args)
		{
			if (!PerformAction(Convert.ToUInt32(args.Cost), true))
			{
				args.Cancel = true;
			}
		}
		#endregion ability

		/// <summary>
		/// Regenerate.
		/// </summary>
		/// <returns>IEnumerator</returns>
		protected IEnumerator Regenerate()
		{
			while (!_isDead)
			{
				// Check if already at max (avoid unnecessary sync with client).
				if (_currentHealth != _settings.MaxHealth)
					_currentHealth = Math.Min(_settings.MaxHealth, _currentHealth + _settings.HealthRegeneration);
				if (_currentStamina != _settings.MaxStamina)
					_currentStamina = Math.Min(_settings.MaxStamina, _currentStamina + _settings.StaminaRegeneration);

				// wait a little while...
				yield return new WaitForSeconds(RegInterval);
			}
		}

		/// <summary>
		/// Can the user perform an action that requires stamina.
		/// </summary>
		/// <param name="staminaAmount">Amount of stamina the action requires.</param>
		/// <returns>Action can be performed.</returns>
		protected bool CanPerformAction(uint staminaAmount)
		{
			return _currentStamina > staminaAmount;
		}
		/// <summary>
		/// Performs an action that requires stamina.
		/// </summary>
		/// <param name="staminaAmount">Amount of stamina the action requires.</param>
		/// <param name="noHealthSubstitution">Specifies if health can be used as a substitution for missing stammina.</param>
		/// <returns>Action has been performed.</returns>
		protected bool PerformAction(uint staminaAmount, bool noHealthSubstitution)
		{
			// If no health substitution is possible, check if the player can perform the action.
			if (noHealthSubstitution && !CanPerformAction(staminaAmount))
				return false;

			if (isServer)
			{
				UseStamina(staminaAmount);
			}
			else
			{
				CmdUseStamina(staminaAmount);
			}

			return true;
		}
		[Command]
		protected virtual void CmdUseStamina(uint amount)
		{
			UseStamina(amount);
		}
		/// <summary>
		/// Consumes an amount of stamina.
		/// </summary>
		/// <param name="amount">Stamina amount.</param>
		/// <returns>True enough stamina.</returns>
		[Server]
		protected virtual void UseStamina(uint amount)
		{
			// Check if not enough stamina
			if (amount > _currentStamina)
			{
				// Damage the player for the remaining value.
				TakeDamage(amount - _currentStamina);
				_currentStamina = 0;
			}
			else
			{
				_currentStamina -= amount;
			}
		}
		/// <summary>
		/// Take an amount of damage.
		/// </summary>
		/// <param name="amount">Damage amount.</param>
		[Server]
		protected virtual void TakeDamage(uint amount)
		{
			Debug.Log("AGPlayer: TakeDamage amount: " + amount + "current health " + _currentHealth);

			if (_currentHealth > amount)
			{
				_currentHealth -= amount;
			}
			else
			{
				_currentHealth = 0;
				Die();
			}
		}
		/// <summary>
		/// Handles the death of the guardian.
		/// </summary>
		[Server]
		protected virtual void Die()
		{
			Debug.Log("AGPlayer: Die");

			// Set the death flag so this function won't be called again.
			_isDead = true;

			// Stop regenerating
			StopCoroutine(Regenerate());

			// Turn off other scripts
		}

		protected virtual void HealthHook(uint val)
		{
			Debug.Log("Is server " + isServer + " Health is now at " + val);

			_currentHealth = val;

			// Display in health bar
			Player.PlayerProperties.SetPropertyBarValue(GameObjectProperties.PropertyKey.Health, val);
		}
		protected virtual void StaminaHook(uint val)
		{
			Debug.Log("Is server " + isServer + " Stamina is now at " + val);

			_currentStamina = val;

			// Display in stamina bar
			Player.PlayerProperties.SetPropertyBarValue(GameObjectProperties.PropertyKey.Stamina, val);
		}
		protected virtual void IsDeadHook(bool val)
		{
			Debug.Log("AGPlayer: IsDeadHook");

			_isDead = val;
		}
	}
}
