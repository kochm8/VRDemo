using CpvrLab.ArcherGuardian.Scripts.Items.Arrows;
using CpvrLab.ArcherGuardian.Scripts.PlayersAndIO;
using System;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.Networking;

namespace CpvrLab.ArcherGuardian.Scripts.Items
{
	public class HitableEventArgs : EventArgs
	{
		public HitableEventArgs(GameObject hitObject, float force, HitInfoKey hitInfo, HitTypeKey hitType)
		{
			if (hitObject == null)
				Debug.LogError("HitableEventArgs: projectile not set!");

			HitObject = hitObject;
			Force = force;
			HitInfo = hitInfo;
			HitType = hitType;
		}
		public GameObject HitObject { get; private set; }
		public float Force { get; private set; }
		public HitInfoKey HitInfo { get; private set; }
		public HitTypeKey HitType { get; private set; }
	}
	public enum HitInfoKey : int
	{
		Default = 1,
		Head = 2,
		// TODO: Add others...
	}
	public enum HitTypeKey : int
	{
		Projectile = 1,
		Explosion = 2,
        Zombie = 3,
    }
	public class Hitable : NetworkBehaviour
	{
		public Transform AttachTransform;
		[Range(0, 1)]
		public float AttachDistanceFactor = 1f;
		public event EventHandler<HitableEventArgs> Hit;
		[Tooltip("Add a specific key for a collider. Should a hit be detected on the collider, the specified key will be available in the hit event.")]
		public HitInfoDefinition[] HitDefinition;

		private void Start()
		{
			if (AttachTransform == null)
			{
				AttachTransform = transform;
			}
		}

		/// <summary>
		/// Handle the hit.
		/// </summary>
		/// <param name="sender">Sender</param>
		/// <param name="projectile">Gameobject that caused the hit.</param>
		/// <param name="collision">The collision object of this hit (<seealso cref="Collision"/>).</param>
		/// <returns>True if the hit has been handled, otherwise false.</returns>
		public virtual bool HandleProjectile(object sender, GameObject projectile, Collision collision)
		{
			Notify(sender, projectile, HitTypeKey.Projectile, collision.collider, collision.relativeVelocity.magnitude);

			var arrow = sender as BaseArrow;
			if (arrow != null)
			{
				AttachArrow(projectile, collision, arrow);
				return true;
			}
			return false;
		}

		public virtual void HandleExplosion(object sender, GameObject explosionSrc, Collider collider, float force)
		{
			Notify(sender, explosionSrc, HitTypeKey.Explosion, collider, force);
		}


        public virtual void HandleZombieHit(object sender, GameObject agentSrc, Collision collision, float force)
        {
            Notify(sender, agentSrc, HitTypeKey.Zombie, collision.collider, force);
        }

        /// <summary>
        /// Notifies all registered subscribers about the hit event.
        /// </summary>
        /// <param name="sender">Source class.</param>
        /// <param name="hitObject">Game object that caused the hit.</param>
        /// <param name="collider">The collider that was hit by the hit object.</param>
        /// <param name="force">The force of the collision.</param>
        /// <param name="hitType">The type of the hit.</param>
        private void Notify(object sender, GameObject hitObject, HitTypeKey hitType, Collider collider, float force)
		{
			if (Hit != null)
			{
				// Check if a specific area was hit.
				var hitInfo = HitInfoKey.Default;
				foreach (var def in HitDefinition)
				{
					if (def.Collider == collider)
					{
						hitInfo = def.Key;
					}
				}

				// Notifiy subscribers...
				var args = new HitableEventArgs(hitObject, force, hitInfo, hitType);
				Hit(sender, args);
			}
		}
		/// <summary>
		/// Attach the arrow to the hitable (for arrows with <seealso cref="ImpactBehaviour.Attach"/>).
		/// </summary>
		/// <param name="projectile">Gameobject that caused the hit.</param>
		/// <param name="collision">The collision object of this hit (<seealso cref="Collision"/>).</param>
		/// <param name="arrow">Arrow</param>
		private void AttachArrow(GameObject projectile, Collision collision, BaseArrow arrow)
		{
			// child needs to be set immediately (AddChild will be called twice on a host, not on a dedicated server though).
			AddChild(projectile);
			RpcAddChild(projectile);

			// Get contact point and calculate the desired attach position.
			//var colContact = collision.contacts[0];

			//// Distance from the center of the arrow to the contact point (point on the arrow collider).
			//var arrowDist = (projectile.transform.position - colContact.point).magnitude;
			//// Distance from the center of the arrow to the collider.
			//var dist = arrowDist + colContact.separation - arrow.ArrowPenetrationDepth * AttachDistanceFactor;

			//var dist = arrow.ArrowPenetrationDepth * AttachDistanceFactor + colContact.separation;
			//// Translate the arrow to the correct attach position.
			//projectile.transform.Translate(dist * arrow.GetFlightDirection());

			arrow.OnAttachedToParent();
		}
		[ClientRpc]
		private void RpcAddChild(GameObject projectile)
		{
			AddChild(projectile);
		}
		/// <summary>
		/// Set the parent of the projectile.
		/// </summary>
		/// <param name="projectile">Projectile GameObject to add as a child.</param>
		private void AddChild(GameObject projectile)
		{
			projectile.transform.SetParent(AttachTransform);
		}

		[Serializable]
		public class HitInfoDefinition
		{
			public HitInfoKey Key;
			public Collider Collider;
		}
	}

}