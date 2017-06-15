using CpvrLab.VirtualTable;
using System;
using UnityEngine;
using UnityEngine.Networking;

namespace CpvrLab.TellLevels.Scripts
{
	public class ShieldBlockEventArgs : EventArgs
	{
		public ShieldBlockEventArgs(float force)
		{
			Force = force;
		}
		public float Force { get; private set; }
	}
	public class Shield : UsableItem
	{
		public event EventHandler<ShieldBlockEventArgs> Block;

		public GameObject _shieldMesh;

		private Hitable _hitable;
		// Use this for initialization
		void Start()
		{
			_hitable = _shieldMesh.GetComponent<Hitable>();
			_hitable.Hit += _hitable_Hit;
		}

		// Update is called once per frame
		void Update()
		{
			if (_input == null)
				return;
		}

		#region UsableItem
		protected override void OnEquip()
		{
			base.OnEquip();

			if (isLocalPlayer)
			{
				_input.HideModel(true);
			}
		}
		protected override void OnUnequip()
		{
			base.OnUnequip();
		}
		#endregion UsableItem

		void OnCollisionEnter(Collision other)
		{
			// Collisions are handled by the server
			if (!isServer)
				return;

			Debug.Log("Shield - OnCollisionEnter:" + other.gameObject.name);

			//if (OnShieldBlock != null)
			//	OnShieldBlock(other);

			//if (isInUse && _output != null)
			//{
			//	_output.HandleCollision(other.relativeVelocity.magnitude);
			//}
			//if (other.gameObject.name.StartsWith("shield", System.StringComparison.InvariantCultureIgnoreCase))
			//{
			//	Debug.Log("A shield has been hit");
			//}
		}

		private void _hitable_Hit(object sender, HitableEventArgs e)
		{
			Debug.Log("Shield: OnHit, isServer: " + isServer.ToString());

			if (sender is TrainingArrow)
			{
				CmdOnBlockArrow(e.Projectile, e.Force);
				e.IsHandled = true;
			}
		}
		[Command]
		private void CmdOnBlockArrow(GameObject arrow, float force)
		{
			OnBlockArrow(arrow, force);
			RpcOnBlockArrow(arrow, force);
		}
		[ClientRpc]
		private void RpcOnBlockArrow(GameObject arrow, float force)
		{
			Debug.Log("Shield: RpcOnBlockArrow, isServer: " + isServer.ToString());
			OnBlockArrow(arrow, force);
		}
		private void OnBlockArrow(GameObject arrow, float force)
		{
			Debug.Log("Shield: OnBlockArrow, isServer: " + isServer.ToString());
			// Set as parent
			arrow.transform.parent = transform;
			var arrowRigidbody = arrow.GetComponent<Rigidbody>();
			if (arrowRigidbody != null)
			{
				// Set is kinematic true, so the arrow stays attached to the shield.
				arrowRigidbody.isKinematic = true;
			}

			if (Block != null)
			{
				var sbData = new ShieldBlockEventArgs(force);
				Block(this, sbData);
			}

			if (isInUse && _output != null)
			{
				float strength;

				if (force > 10f)
				{
					strength = 0.8f;
				}
				else if (force > 5f)
				{
					strength = 0.5f;
				}
				else if (force > 1f)
				{
					strength = 0.2f;
				}
				else
				{
					strength = 0f;
				}
				_output.HandleCollision(strength);
			}
		}
	}
}