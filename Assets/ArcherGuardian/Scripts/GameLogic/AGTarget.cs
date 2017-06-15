using CpvrLab.ArcherGuardian.Scripts.Items;
using CpvrLab.ArcherGuardian.Scripts.Items.Arrows;
using CpvrLab.ArcherGuardian.Scripts.Util.Interface;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace CpvrLab.ArcherGuardian.Scripts.GameLogic
{
	public class TargetDestroyedEventArgs : EventArgs
	{
		public TargetDestroyedEventArgs()
		{
		}
	}
	public class AGTarget : NetworkBehaviour
	{
		#region members
		#region public
		#endregion public

		private Hitable _hitable;
		[SyncVar(hook = "HealthHook")]
		private uint _currentHealth;
		[SyncVar]
		private bool _isDestroyed;

		private const float RegInterval = 2f;

		private AGTargetSettings _settings;
		private GameObjectProperties _properties;
		#endregion members

		#region public
		public event EventHandler<TargetDestroyedEventArgs> TargetDestroyed;
		/// <summary>
		/// Apply the specified settings to the archer.
		/// </summary>
		/// <param name="settings">Settings (<seealso cref="AGPlayerSettings"/>)</param>
		[Server]
		public virtual void SetSettings(AGTargetSettings settings)
		{
			_settings = settings;
		}
		#endregion public
		private void Awake()
		{
			_properties = GetComponent<GameObjectProperties>();
		}
		private void Start()
		{
			if (isServer)
			{
				_hitable = GetComponent<Hitable>();
				_hitable.Hit += HitableHit;

				if (_settings == null)
				{
					Debug.LogError("Settings not set!");
					return;
				}

				// Set min and max values for the health slider (only on clients or host).
				RpcSetMaxHealth(_settings.MaxHealth);

				// sync var
				_currentHealth = _settings.StartingHealth;

				// Start regeneration coroutine
				StartCoroutine(Regenerate());
			}
		}

		/// <summary>
		/// Regenerate.
		/// </summary>
		/// <returns>IEnumerator</returns>
		protected IEnumerator Regenerate()
		{
			while (!_isDestroyed)
			{
				// Check if already at max (avoid unnecessary sync with client).
				if (_currentHealth != _settings.MaxHealth)
					_currentHealth = Math.Min(_settings.MaxHealth, _currentHealth + _settings.HealthRegeneration);

				// wait a little while...
				yield return new WaitForSeconds(RegInterval);
			}
		}
		[ClientRpc]
		private void RpcSetMaxHealth(uint maxHealth)
		{
			SetMaxHealth(maxHealth);
		}
		[Client]
		private void SetMaxHealth(uint maxHealth)
		{
			_properties.SetPropertyBarMinMax(GameObjectProperties.PropertyKey.Health, 0f, maxHealth);
		}
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

						TakeDamage(Convert.ToUInt32(arrow.ImpactDamage * force));
					}
					break;
			}
		}
		/// <summary>
		/// Take an amount of damage.
		/// </summary>
		/// <param name="amount">Damage amount.</param>
		[Server]
		protected virtual void TakeDamage(uint amount)
		{
			Debug.Log("TakeDamage amount: " + amount + "current health " + _currentHealth);

			if (_currentHealth > amount)
			{
				_currentHealth -= amount;
			}
			else
			{
				_currentHealth = 0;
				OnDestroyed();
			}
		}
		/// <summary>
		/// Handles the destruction of the target and notifies subscribers.
		/// </summary>
		[Server]
		protected virtual void OnDestroyed()
		{
			// Set the is destroyed flag .
			_isDestroyed = true;

			if (TargetDestroyed != null)
			{
				var tdData = new TargetDestroyedEventArgs();
				TargetDestroyed(this, tdData);
			}
			//CpvrLab.VirtualTable.GameManager.instance.StopGame();

			// Turn off other scripts
		}
		protected virtual void HealthHook(uint val)
		{
			_currentHealth = val;

			// Display in health bar
			if (isClient)
			{
				_properties.SetPropertyBarValue(GameObjectProperties.PropertyKey.Health, val);
			}
		}
	}
}
