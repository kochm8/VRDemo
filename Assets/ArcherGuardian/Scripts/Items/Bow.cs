using CpvrLab.ArcherGuardian.Scripts.IK;
using CpvrLab.ArcherGuardian.Scripts.Items.Arrows;
using CpvrLab.VirtualTable;
using System;
using System.Collections;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.Networking;


namespace CpvrLab.ArcherGuardian.Scripts.Items
{
	public class BowStringPullEventArgs : CancelEventArgs
	{
		public BowStringPullEventArgs(float force)
		{
			Force = force;
		}
		public float Force { get; private set; }
	}
	public class Bow : UsableItem
	{
		#region members
		#region public
		public AudioClip BowReleaseSound;
		public AudioClip BowDrawSound;

		public Transform Notch;
		public Transform StringAttachPoint;
		public Transform StringStartPoint;
		#endregion public

		private AudioSource _audioSource;

		private BaseArrow _currentArrow;
		private PlayerInput _arrowInput;
		private Transform _transInput;
		private PlayerOutput _arrowOutput;

		//Interval in which pull events are fired.
		private const float EventInterval = 0.5f;
		private const float BowPowerMultiplier = 45f;
		private bool _canPull = false;
		private float _lastDist = 0;
		#endregion members

		#region public
		public event EventHandler<BowStringPullEventArgs> BowStringPulled;
		/// <summary>
		/// Tries to attach the specified arrow.
		/// </summary>
		/// <param name="arrowInput">Input that handles the arrow.</param>
		/// <param name="arrow">Arrow to attach.</param>
		/// <returns>True if attachment was successful.</returns>
		public bool AttachArrow(BaseArrow arrow, PlayerInput arrowInput, PlayerOutput arrowOutput)
		{
			if (_currentArrow != null)
				return false;

			// Reset can pull to true;
			_canPull = true;
			// Start firing pull events
			StartCoroutine(OnBowStringPulled());


			// TODO: Make sure that the player has authority on both objects?
			_arrowInput = arrowInput;
			// Get the transform of the arrow input
			_transInput = _arrowInput.GetTrackedTransform();
			_arrowOutput = arrowOutput;
			_currentArrow = arrow;
			_currentArrow.transform.SetParent(StringAttachPoint);
			_currentArrow.transform.localPosition = Vector3.zero;
			_currentArrow.transform.localRotation = Quaternion.identity;

			return true;
		}
		#endregion public

		#region overrides
		[Client]
		protected override void OnEquip()
		{
		}

        [Client]
        protected override AGHandEffector.PoseableObjects UseHandEffector()
        {
            return AGHandEffector.PoseableObjects.Bow;
        }
        #endregion overrides

        // Use this for initialization
        private void Start()
		{
			_audioSource = GetComponent<AudioSource>();
			_audioSource.clip = BowDrawSound;
			_audioSource.pitch = 0.5f;
		}

		private void Update()
		{
			if (!hasAuthority)
				return;
		}

		// Update is called once per frame
		private void FixedUpdate()
		{
			if (!hasAuthority)
				return;

			PullString();
		}

		private void OnTriggerExit(Collider other)
		{
			// Check if the current arrow falls down.
			if (_currentArrow != null && other.transform.parent != null
				&& other.transform.parent.gameObject == _currentArrow.gameObject)
			{
				ArrowIsOutOfBowString();
			}
		}

		[Client]
		private void PullString()
		{
			if (_currentArrow == null)
				return;

			if (_transInput == null)
			{
				// TODO: Change this...
				// Special handling for FPSPlayer
				// Check if the player is ready to shoot.
				if (_arrowInput.GetActionUp(PlayerInput.ActionCode.Shoot))
				{
					Shoot(0.5f);
				}
				return;
			}

			// Draw bowstring
			StringAttachPoint.position = _transInput.position;

			// Aim arrow
			_currentArrow.transform.LookAt(Notch);

			// Calculate the distance
			float dist = (StringStartPoint.position - _transInput.position).magnitude;

			//Pause sound as soon as the bowstring isn't moved anymore.
			if (Math.Abs(_lastDist - dist) < 0.002)
			{
				_audioSource.Pause();
			}
			else if (!_audioSource.isPlaying)
			{
				// Play draw sound
				_audioSource.Play();
			}
			_lastDist = dist;

			// Check for a bow output
			if (_output != null)
			{
				// TODO: Calculate draw force.
				_output.HandleObjectDraw(dist / 2f);
			}
			// Check for an arrow output
			if (_arrowOutput != null)
			{
                // TODO: Calculate draw force.
                _arrowOutput.HandleObjectDraw(dist);
			}

			// Check if the player is ready to shoot or if the pulling was canceled.
			// Canceled right now means, that the user ran out of stamina and can't hold the bow string any longer.
			if (_arrowInput.GetActionUp(PlayerInput.ActionCode.Shoot) || !_canPull)
			{
				Shoot(dist * BowPowerMultiplier);
			}
		}
		[Client]
		private void ArrowIsOutOfBowString()
		{
			if (_currentArrow == null)
				return;

			Shoot(0.01f);
		}
		private IEnumerator OnBowStringPulled()
		{
			// Fire the event
			if (BowStringPulled != null)
			{
				BowStringPullEventArgs args;
				do
				{
					args = new BowStringPullEventArgs(Mathf.Clamp(_lastDist, 0f, 1f));
					BowStringPulled(this, args);
					// wait a little while...
					yield return new WaitForSeconds(EventInterval);
				}
				// Check if pulling is canceled.
				while (!args.Cancel);
			}
		}
		private void Shoot(float force)
		{
			if (_currentArrow == null)
			{
				Debug.LogError("Fire: No Arrow attached");
				return;
			}

			StopCoroutine(OnBowStringPulled());

			// Stop playing sound.
			_audioSource.Stop();

			//Fire Arrow
			_currentArrow.transform.parent = null;
			_currentArrow.Shoot(force);

			//Reset bowstring
			StringAttachPoint.position = StringStartPoint.position;

			//Reset transform rotation
			//transform.localRotation = Quaternion.identity;

			_currentArrow = null;
			_arrowInput = null;
			_transInput = null;
			_arrowOutput = null;
		}
	}

}