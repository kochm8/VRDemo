using CpvrLab.VirtualTable;
using System;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Networking;

namespace CpvrLab.TellLevels.Scripts.PlayersAndIO
{
	public class Guardian : NetworkBehaviour
	{
		#region Members
		private GuardianSettings _settings;

		private bool _isDead;
		private GamePlayer _player;

		[SyncVar(hook = "HealthHook")]
		private int _currentHealth;
		[SyncVar(hook = "StaminaHook")]
		private int _currentStamina;
		#endregion Members
		void HealthHook(int val)
		{
			Debug.Log("Is server " + isServer + " Health is now at " + _currentHealth);
		}
		void StaminaHook(int val)
		{
			Debug.Log("Is server " + isServer + " Stamina is now at " + _currentStamina);
		}

		#region Properties

		/// <summary>
		/// True, if Guardian is dead
		/// </summary>
		public bool IsDead
		{
			get
			{
				return _isDead;
			}
		}
		#endregion Properties

		public void Init(GamePlayer player, GuardianSettings settings)
		{
			Assert.IsNotNull(player, "Guardian: player missing!");
			Assert.AreEqual(gameObject, player.gameObject, "Guardian: Trying to add a player with a different gameobject!");

			_player = player;
			InitFromSettings(settings);

			//register listener only on server
			if (isServer)
			{
				// Add hitable if the component doesn't exist.
				var hitable = gameObject.GetComponent<Hitable>();
				if (hitable == null)
					hitable = gameObject.AddComponent<Hitable>();

				hitable.Hit += HitableHit;

				// Check for equipped shield
				var shield = GetComponentInChildren<Shield>();
				Assert.IsNotNull(shield, "Guardian: No shield equipped!");
				if (shield != null)
				{
					shield.Block += ShieldBlock;
				}
			}
		}
		public void RemoveComponent()
		{
			Debug.Log("Guardian: RemoveComponent, isServer: " + isServer.ToString());

			Destroy(this);
		}
		private void OnDestroy()
		{
			// Remove event handlers
			var hitable = GetComponent<Hitable>();
			if (hitable != null)
			{
				hitable.Hit -= HitableHit;
			}
			var shield = GetComponentInChildren<Shield>();
			if (shield != null)
			{
				shield.Block -= ShieldBlock;
			}
		}
		private void InitFromSettings(GuardianSettings settings)
		{
			Assert.IsNotNull(settings, "InitFromSettings: settings missing!");

			_settings = settings;
			_currentHealth = settings.StartingHealth;
			_currentStamina = settings.StartingStamina;
		}

		private void HitableHit(object sender, HitableEventArgs e)
		{
			Debug.Log("Guardian: OnHit, isServer: " + isServer.ToString());

			if (!isServer)
				return;

			if (sender is TrainingArrow)
			{
				//TODO
				TakeDamage(Convert.ToInt32(e.Force));
				// Set handled to true.
				e.IsHandled = true;
			}
		}
		private void ShieldBlock(object sender, ShieldBlockEventArgs data)
		{
			//TOOD: Calculate amount based on collision parameters (velocity, gameobject,...)
			UseStamina(Convert.ToInt32(data.Force));
		}

		private void UseStamina(int amount)
		{
			// Check if not enough stamina
			if (amount > _currentStamina)
			{
				// Damage the guardian for the remaining value.
				TakeDamage(amount - _currentStamina);
				_currentStamina = 0;
			}
			else
			{
				_currentStamina -= amount;
			}
		}

		private void TakeDamage(int amount)
		{
			Debug.Log("Guardian: TakeDamage amount: " + amount + "current health " + _currentHealth);

			_currentHealth -= amount;
			if (_currentHealth <= 0)
			{
				_currentHealth = 0;
				Die();
			}
		}

		private void Die()
		{
			Debug.Log("Guardian: Die");

			// Set the death flag so this function won't be called again.
			_isDead = true;

			// Turn off other scripts
		}
	}
}
