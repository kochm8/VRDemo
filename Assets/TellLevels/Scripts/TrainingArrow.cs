using UnityEngine;
using UnityEngine.Networking;

namespace CpvrLab.TellLevels.Scripts
{
	public class TrainingArrow : NetworkBehaviour
	{
		public AudioClip _bowShootSound;

		private bool _isFlying = false;
		private Rigidbody _rigidbody;
		private AudioSource _audioSource;
		private Vector3? _target = null;

		void Awake()
		{
			_rigidbody = GetComponent<Rigidbody>();
			_audioSource = GetComponent<AudioSource>();
		}
		void Start()
		{
		}

		void Update()
		{
		}

		void FixedUpdate()
		{
			if (_isFlying && _rigidbody.velocity != Vector3.zero)
			{
				// add a force like to drag, opposite the direction of travel, and apply it after of the center of mass, typically (0,0,0)
				_rigidbody.AddForceAtPosition(_rigidbody.velocity * -0.1f, transform.TransformPoint(0, 0, 0));
			}
		}

		void OnTriggerEnter(Collider other)
		{
			// Only if arrow is flying.
			if (!_isFlying)
				return;

			Debug.Log("OnTriggerEnter: " + other.gameObject.name);
			var hitable = other.gameObject.GetComponent<Hitable>();
			if (hitable != null)
			{
				if (hitable.HandleHit(this, gameObject, _rigidbody.velocity.magnitude))
				{
					Debug.Log("OnTriggerEnter: Hitable " + other.gameObject.name);
					// the hit has been handled.
					_rigidbody.velocity = Vector3.zero;
					_isFlying = false;

					//transform.parent = other.gameObject.transform;
					//// Set is kinematic true, so the arrow stays attached to the object.
					//_rigidbody.isKinematic = true;
				}
				else
				{
					Debug.Log("OnTriggerEnter: Hitable " + other.gameObject.name + " didn't handle the hit!");
				}
			}
		}

		[Command]
		public void CmdSetTarget(Vector3 target)
		{
			_target = target;
		}

		//////////////
		// Shooting //
		//////////////

		[Command]
		public void CmdShootArrow()
		{
			if (_target == null)
				Debug.LogError("CmdShootArrow: No target to shoot at");

			if (hasAuthority)
			{
				ShootArrow();
			}
			else
			{
				RpcShootArrow();
			}
		}
		[ClientRpc]
		void RpcShootArrow()
		{
			if (hasAuthority)
			{
				ShootArrow();
			}
		}
		void ShootArrow()
		{
			_rigidbody.isKinematic = false;
			// Aim at target.
			_rigidbody.transform.LookAt(_target.Value);

			var distance = Vector3.Distance(_target.Value, transform.position) + 1f;
			_rigidbody.velocity = (transform.up / 2 + transform.forward) * distance;

			_isFlying = true;

			if (_bowShootSound != null)
			{
				_audioSource.PlayOneShot(_bowShootSound);
			}
		}
	}
}