using CpvrLab.ArcherGuardian.Scripts.Items.Arrows;
using System;
using UnityEngine;
using UnityEngine.Networking;

namespace CpvrLab.ArcherGuardian.Scripts.Items
{
	public class ShieldBlockEventArgs : EventArgs
	{
		public ShieldBlockEventArgs(float force)
		{
			Force = force;
		}
		/// <summary>
		/// Value between zero and one.
		/// </summary>
		public float Force { get; private set; }
	}
	public class Shield : VirtualTable.UsableItem
	{
		#region members
		#region public
		#endregion public

		private Hitable _hitable;
		private float _explosionForceFactor = 0.5f;
		#endregion members

		#region public
		public event EventHandler<ShieldBlockEventArgs> Block;
		#endregion public

		#region overrides
		protected override void OnEquip()
		{
			base.OnEquip();

			if (_input != null)
			{
				_input.HideModel(true);
			}
		}
		protected override void OnUnequip()
		{
			base.OnUnequip();
		}
		#endregion overrides

		private void Start()
		{
			if (isServer)
			{
				_hitable = GetComponent<Hitable>();
				_hitable.Hit += HitableHit;
			}
		}

		// Update is called once per frame
		private void Update()
		{
			if (!hasAuthority)
				return;
		}


		private void OnCollisionEnter(Collision other)
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

		private void HitableHit(object sender, HitableEventArgs args)
		{
			Debug.Log("Shield: OnHit, isServer: " + isServer.ToString());

			float force = 0f;
			switch (args.HitType)
			{
				case HitTypeKey.Explosion:
					force = args.Force * _explosionForceFactor;
					break;
				case HitTypeKey.Projectile:
					var arrow = sender as BaseArrow;
					if (arrow != null)
					{
						force = Mathf.Clamp01(args.Force / arrow.MaxForce);
					}
					break;
			}
			if (force > 0f)
			{
				OnBlock(force);

				if (_owner != null && !_owner.isLocalPlayer)
				{
					// Inform the owner of the shield about the block event.
					TargetOnBlock(_owner.connectionToClient, force);
				}
			}
		}
		[TargetRpc]
		private void TargetOnBlock(NetworkConnection target, float force)
		{
			OnBlock(force);
		}
		private void OnBlock(float force)
		{
			if (Block != null)
			{
				var sbData = new ShieldBlockEventArgs(force);
				Block(this, sbData);
			}
			if (_output != null)
			{
				_output.HandleCollision(force);
			}
		}
	}
}