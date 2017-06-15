using CpvrLab.ArcherGuardian.Scripts.Items;
using System;
using UnityEngine;
using UnityEngine.Networking;

namespace CpvrLab.ArcherGuardian.Scripts.AGPyroParticles
{
	public class AGFireBaseScript : NetworkBehaviour
	{
		#region members
		#region public
		[Tooltip("Optional audio source to play when the script starts.")]
		public AudioSource AudioSource;
		[Tooltip("Optional audio clip to play once when the object collides with another.")]
		public AudioClip ExplosionClip;

		[Tooltip("How long the script takes to fully start. This is used to fade in animations and sounds, etc.")]
		public float StartTime = 1.0f;

		[Tooltip("How long the script takes to fully stop. This is used to fade out animations and sounds, etc.")]
		public float StopTime = 3.0f;

		[Tooltip("How long the effect lasts. Once the duration ends, the script lives for StopTime and then the object is destroyed.")]
		public float Duration = 2.0f;

		[Tooltip("Specifies whether or not this effect is constant. Duration has no effect if IsConstant is set to true.")]
		public bool IsConstant = false;

		[Tooltip("Specifies if the object explodes once it is ignited.")]
		public bool ExplodeOnIgnition = false;
		[Tooltip("Specifies if the object explodes once a collision occurs.")]
		public bool ExplodeOnCollision = false;
		[Tooltip("How much force to create at the center (explosion), 0 for none.")]
		public float ExplosionForce;
		[Tooltip("The radius of the force, 0 for none.")]
		public float ExplosionRadius;

		[Tooltip("Specifies if an ingnition is required to start the partical systems (ManualParticleSystems, ExplosionParticleSystems excluded).")]
		public bool IgnitionRequired;

		[Tooltip("Particle systems that are started when this object explodes.")]
		public ParticleSystem[] ExplosionParticleSystems;
		[Tooltip("Particle systems that must be manually started and will not be played automatically.")]
		public ParticleSystem[] ManualParticleSystems;

		[Tooltip("Game object with all fire related components.")]
		public GameObject FireGameObject;
		#endregion public

		private float _startTimeMultiplier;
		private float _stopTimeMultiplier;
		private float _currentDuration;
		private int _waterLayer;
		private bool _isStarted = false;
		#endregion members

		#region public
		public bool IsIgnited { get; private set; }
		public bool Starting { get; private set; }
		public bool Stopping { get; private set; }
		public float CurrentFireIntensity { get; private set; }
		/// <summary>
		/// Specifies if the <see cref="FireGameObject"/> should be disabled automatically after the fire is stopped.
		/// </summary>
		public bool DisableAfterStop { get; set; }

		/// <summary>
		/// Ignite the current fire. Will only execute on the server or a client with authority.
		/// </summary>
		public virtual void Ignite()
		{
			if (IsIgnited)
				return;

			if (isServer)
			{
				RpcStartFire();
			}
			else if (hasAuthority)
			{
				CmdStartFire();
			}
		}
		#endregion public

		protected virtual void Awake()
		{
			// Disable the collider
			var collider = FireGameObject.GetComponent<Collider>();
			if (collider != null)
				collider.enabled = false;

			_waterLayer = LayerMask.NameToLayer(AGLayers.Water);
		}
		protected virtual void Start()
		{
			if (!IgnitionRequired && !IsIgnited)
			{
				StartFire();
			}
			_isStarted = true;
		}
		protected virtual void OnEnable()
		{
			// Only if the start method has been called (particle systems don't work before that).
			if (_isStarted && !IgnitionRequired && !IsIgnited)
			{
				StartFire();
			}
		}
		protected virtual void OnDisable()
		{
			StopFire();
		}
		protected virtual void Update()
		{
			if (Stopping)
			{
				if (CurrentFireIntensity > 0f)
				{
					CurrentFireIntensity -= Time.deltaTime * _stopTimeMultiplier;
				}
				else
				{
					CurrentFireIntensity = 0f;
					Stopping = false;
					IsIgnited = false;
				}
			}
			else if (Starting)
			{
				if (CurrentFireIntensity < 1f)
				{
					CurrentFireIntensity += Time.deltaTime * _startTimeMultiplier;
				}
				else
				{
					CurrentFireIntensity = 1f;
					Starting = false;
				}
			}
			else if (!IsConstant)
			{
				// reduce the duration
				_currentDuration -= Time.deltaTime;
				if (_currentDuration <= 0.0f)
				{
					if (hasAuthority)
					{
						// time to stop, no duration left
						if (isServer)
						{
							if (!isClient)
							{
								Stopping = true;
							}
							RpcStopFire();
						}
						else
						{
							CmdStopFire();
						}
					}
				}
			}
		}
		protected virtual void OnTriggerEnter(Collider other)
		{
			if (!isServer)
				return;

			if (other.gameObject.layer == _waterLayer)
			{
				StopFire();
			}

			if (ExplodeOnCollision)
			{
				// If this effect has an explosion force, apply that now
				RpcExplode(FireGameObject.transform.position);

				if (!isClient)
					Explode(FireGameObject.transform.position);
			}
		}

		[Command]
		protected virtual void CmdStartFire()
		{
			if (!isClient)
			{
				IsIgnited = true;
			}
			RpcStartFire();
		}
		[ClientRpc]
		protected virtual void RpcStartFire()
		{
			StartFire();
		}

		protected virtual void StartFire()
		{
			// Set ignited to true, ignore future collisions.
			IsIgnited = true;

			if (AudioSource != null)
			{
				AudioSource.Play();
			}

			CurrentFireIntensity = 0f;
			_currentDuration = Duration;
			// Precalculate time multiplier.
			_stopTimeMultiplier = 1.0f / StopTime;
			_startTimeMultiplier = 1.0f / StartTime;
			Starting = true;

			if (isServer)
			{
				// Enable collider (this fire object can now ignite objects)
				var collider = FireGameObject.GetComponent<Collider>();
				if (collider != null)
					collider.enabled = true;
			}

			if (ExplodeOnIgnition)
			{
				// If this effect has an explosion force, apply that now
				Explode(FireGameObject.transform.position);
			}

			// Start any particle system that is not in the list of manual start particle systems.
			StartParticleSystems();
		}

		[Command]
		protected virtual void CmdStopFire()
		{
			if (!isClient)
			{
				Stopping = true;
			}
			RpcStopFire();
		}
		[ClientRpc]
		protected virtual void RpcStopFire()
		{
			StopFire();
		}

		protected virtual void StopFire()
		{
			if (Stopping || !IsIgnited)
				return;

			Stopping = true;

			if (isServer)
			{
				// Disable collider (this fire object can't ignite objects anymore)
				var collider = FireGameObject.GetComponent<Collider>();
				if (collider != null)
					collider.enabled = false;
			}

			// cleanup particle systems
			foreach (ParticleSystem p in FireGameObject.GetComponentsInChildren<ParticleSystem>())
			{
				p.Stop();
			}

			if (DisableAfterStop)
			{
				FireGameObject.SetActive(false);
			}
		}

		[ClientRpc]
		protected void RpcExplode(Vector3 pos)
		{
			Explode(pos);
		}
		protected void Explode(Vector3 pos)
		{
			if (ExplosionRadius <= 0.0f || ExplosionForce <= 0.0f)
				return;

			if (ExplosionClip != null)
			{
				AudioSource.PlayOneShot(ExplosionClip);
			}

			if (isServer)
			{
				// find all colliders and add explosive force
				Collider[] colliders = Physics.OverlapSphere(pos, ExplosionRadius);
				foreach (Collider col in colliders)
				{
					// Check if collisions between these two objects should be ignored.
					if (Physics.GetIgnoreLayerCollision(FireGameObject.layer, col.gameObject.layer))
						continue;

					var rb = col.GetComponent<Rigidbody>();
					if (rb != null)
					{
						rb.AddExplosionForce(ExplosionForce, pos, ExplosionRadius);
					}
					var hitable = col.GetComponent<Hitable>();
					if (hitable != null)
					{
						// Linearly reduce the damage, based on the distance to the center of the explosion.
						float distFactor = (pos - col.transform.position).magnitude / ExplosionRadius;
						hitable.HandleExplosion(this, FireGameObject, col, ExplosionForce * distFactor);
					}
				}
			}
			if (ExplosionParticleSystems != null)
			{
				foreach (var pSys in ExplosionParticleSystems)
				{
					pSys.transform.position = pos;
					pSys.Emit(UnityEngine.Random.Range(10, 20));
				}
			}
			OnExplode();
		}

		protected virtual void OnExplode()
		{
		}

		private void StartParticleSystems()
		{
			foreach (ParticleSystem pSys in FireGameObject.GetComponentsInChildren<ParticleSystem>())
			{
				if ((ManualParticleSystems == null || Array.IndexOf(ManualParticleSystems, pSys) < 0)
					&& (ExplosionParticleSystems == null || Array.IndexOf(ExplosionParticleSystems, pSys) < 0))
				{
					// wait until next frame because the transform may change
					if (pSys.startDelay == 0.0f)
						pSys.startDelay = 0.01f;

					pSys.Play();
				}
			}
		}
	}
}