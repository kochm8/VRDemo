using CpvrLab.ArcherGuardian.Scripts.AGPyroParticles;
using CpvrLab.ArcherGuardian.Scripts.IK;
using CpvrLab.VirtualTable;
using System;
using UnityEngine;
using UnityEngine.Networking;

namespace CpvrLab.ArcherGuardian.Scripts.Items.Arrows
{
	public enum ArrowImpactBehaviour
	{
		Attach,
		Explode,
		Destroy
	}
	/// <summary>
	/// Base class for all arrow-like objects.
	/// </summary>
	[RequireComponent(typeof(AudioSource))]
	public abstract class BaseArrow : UsableItem
	{
		#region members
		public AudioClip ArrowShootSound;
		public AudioClip ArrowImpactSound;

		public Collider ArrowCollider;

		/// <summary>
		/// Specifies the behaviour on impact.
		/// </summary>
		public ArrowImpactBehaviour ImpactBehaviour = ArrowImpactBehaviour.Attach;
		/// <summary>
		/// Maximum damage on impact.
		/// </summary>
		public float ImpactDamage = 40f;
		/// <summary>
		/// Maximum force possible to shoot the arrow.
		/// </summary>
		public float MaxForce = 22f;
		/// <summary>
		/// Specifies the arrow penetration depth. Only for arrows with <seealso cref="ArrowImpactBehaviour.Attach"/>.
		/// </summary>
		public float ArrowPenetrationDepth = 0.2f;

		[SyncVar]
		private bool _isFlying = false;

		private AudioSource _audioSource;
		private Rigidbody _rigidbody;
		#endregion members
		protected Transform Transform { get; private set; }

		#region public
		/// <summary>
		/// Shoot the arrow with the specified force.
		/// </summary>
		/// <param name="force">The force to apply to the arrow.</param>
		public void Shoot(float force)
		{
			if (!hasAuthority)
				return;

			if (_isFlying)
				Debug.LogError("Arrow - Shoot: Arrow is already flying.");

			// Client has authority (client has to shoot the arrow).
			_audioSource.PlayOneShot(ArrowShootSound);

			// Invoke shoot on the server.
			CmdShoot(force);
		}
		/// <summary>
		/// Method that is called, when the final attach position of the arrow has been set.
		/// Usally this will be called from the object the arrow collided with.
		/// The server should always be the caller of this method.
		/// </summary>
		public void OnAttachedToParent()
		{
			if (!isServer)
				return;

			if (ImpactBehaviour != ArrowImpactBehaviour.Attach)
				Debug.LogError("OnAttachedToParent called for arrow with an invalid ImpactBehaviour!");

			// Disable all scripts that are no longer required.
			if (!isClient)
			{
				OnAttachedToParent(Transform.localPosition, Transform.localRotation);
			}
			RpcOnAttachedToParent(Transform.localPosition, Transform.localRotation);
		}
		/// <summary>
		/// Returns the flight direction of the arrow.
		/// </summary>
		/// <returns>Flight direction (normalized velocity).</returns>
		public Vector3 GetFlightDirection()
		{
			if (!_isFlying)
				return Vector3.zero;

			return Vector3.Normalize(_rigidbody.velocity);
		}
		#endregion public

		protected virtual void Start()
		{
			Transform = transform;
			_audioSource = GetComponent<AudioSource>();
			_rigidbody = Transform.GetComponent<Rigidbody>();

			if (isServer)
			{
				// Activate ignition on server
				var ignition = GetComponentInChildren<AGIgnition>(true);
				if (ignition != null)
				{
					ignition.gameObject.SetActive(true);
				}
			}
		}
		protected virtual void Update()
		{
			if (Input.GetKeyDown(KeyCode.H))
			{
				Shoot(15f);
			}
		}
		protected virtual void FixedUpdate()
		{
			if (isServer && _isFlying)
			{
				// Aim the arrow
				//Transform.LookAt(transform.position + _rigidbody.velocity);
				Transform.rotation = Quaternion.LookRotation(_rigidbody.velocity);
				//Debug.DrawLine(Transform.position, Transform.position + Vector3.up * 0.5f, Color.red, 15f);
			}
		}
		[Command]
		protected virtual void CmdShoot(float force)
		{
			if (!hasAuthority)
			{
				// Server needs authority to shoot the arrow.
				var nId = GetComponent<NetworkIdentity>();
				nId.RemoveClientAuthority(nId.clientAuthorityOwner);
			}

			_owner.Unequip(this);

			_isFlying = true;

			_rigidbody.velocity = Transform.forward * Mathf.Min(force, MaxForce);
			_rigidbody.useGravity = true;
			_rigidbody.isKinematic = false;

			// Set the isTrigger flag to false (preciser collision detection).
			ArrowCollider.isTrigger = false;
		}

		protected virtual void OnTriggerStay(Collider other)
		{
			if (!isInUse || !hasAuthority)
				return;

			// Check if the collider is a bow and the player wants to start to fire.
			if (other.tag.Equals(AGTags.Bow, StringComparison.CurrentCultureIgnoreCase)
				&& _input.GetActionDown(PlayerInput.ActionCode.Shoot))
			{
				var bow = other.gameObject.GetComponent<Bow>();
				if (bow != null)
				{
					if (bow.AttachArrow(this, _input, _output))
					{
						// Arrow is attached.
						// TODO: Stop some stuff.
					}
				}
			}
		}
		protected virtual void OnCollisionEnter(Collision collision)
		{
			// Only if arrow is flying and is server.
			if (!isServer || !_isFlying)
				return;

			Debug.Log("OnCollisionEnter: " + collision.gameObject.name);

			var hitable = collision.gameObject.GetComponent<Hitable>();
			if (hitable != null)
			{
				if (hitable.HandleProjectile(this, gameObject, collision))
				{
					//Play impact sound
					_audioSource.PlayOneShot(ArrowImpactSound);

					// the hit has been handled.
					_rigidbody.velocity = Vector3.zero;

					_isFlying = false;
				}
				else
				{
					Debug.Log("OnCollisionEnter: Hitable " + collision.gameObject.name + " didn't handle the hit!");
				}
				OnHitableHit(hitable);
			}
		}
		/// <summary>
		/// Override to add a special behaviour after a <seealso cref="Hitable"/> was hit.
		/// </summary>
		/// <param name="hitable">Hitable</param>
		protected abstract void OnHitableHit(Hitable hitable);

		[ClientRpc]
		private void RpcOnAttachedToParent(Vector3 localPos, Quaternion localRot)
		{
			OnAttachedToParent(localPos, localRot);
		}
		/// <summary>
		/// Disable all scripts that are no longer required and set the attach position of the arrow.
		/// </summary>
		/// <param name="localPos">The position relative to the arrows parent position.</param>
		/// <param name="localRot">The roation relative to the arrows parent rotation.</param>
		protected virtual void OnAttachedToParent(Vector3 localPos, Quaternion localRot)
		{
			// Set is kinematic true, so the arrow stays wherever he landed.
			_rigidbody.isKinematic = true;
			_rigidbody.useGravity = false;

			gameObject.GetComponent<NetworkTransform>().enabled = false;

			//Cannot disable yet, because the impact sound is still playing
			//_audioSource.enabled = false;

			enabled = false;
			ArrowCollider.enabled = false;
			Transform.localPosition = localPos;
			Transform.localRotation = localRot;
		}

		[Client]
		protected override AGHandEffector.PoseableObjects UseHandEffector()
		{
			return AGHandEffector.PoseableObjects.Arrow;
		}
	}
}
