using System.Collections;
using UnityEngine;
using Valve.VR;

namespace CpvrLab.VirtualTable
{
	public class ViveOutput : PlayerOutput
	{
		[HideInInspector]
		public SteamVR_TrackedObject trackedObj;

		#region PlayerOutput implemention
		public override void HandleCollision(float force)
		{
			float duration = 1f;

			//Debug.Log("StartCoroutine - ViveOutput.TriggerHapticPulses: duration: " + duration + " force " + force);
			StartCoroutine(TriggerHapticPulses(duration, force));
		}
		public override void HandleObjectDraw(float force)
		{
			TriggerHapticPulseOnce((ushort)Mathf.Lerp(0, 3999, force));
		}
		#endregion PlayerOutput implemention

		protected void TriggerHapticPulseOnce(ushort durationMicroSec = 500, EVRButtonId buttonId = EVRButtonId.k_EButton_Axis0)
		{
			if (trackedObj == null)
			{
				Debug.LogError("TriggerHapticPulseOnce: trackedObj not set!");
				return;
			}

			if (trackedObj.index == SteamVR_TrackedObject.EIndex.None)
			{
				Debug.Log("ViveOutput.TriggerHapticPulse: no tracked object");
				return;
			}
			//Debug.Log("ViveOutput.TriggerHapticPulse: durationMicroSec: " + durationMicroSec);
			var device = SteamVR_Controller.Input((int)trackedObj.index);
			device.TriggerHapticPulse(durationMicroSec, buttonId);
		}

		/// <summary>
		/// Trigger a series of haptic pulses.
		/// </summary>
		/// <param name="durationSec">How long the vibration should go for (in seconds).</param>
		/// <param name="strength">Vibration strength from 0-1.</param>
		/// <param name="buttonId">EVRButtonId</param>
		/// <returns></returns>
		protected IEnumerator TriggerHapticPulses(float durationSec, float strength, EVRButtonId buttonId = EVRButtonId.k_EButton_Axis0)
		{
			for (float i = 0; i < durationSec; i += Time.deltaTime)
			{
				TriggerHapticPulseOnce((ushort)Mathf.Lerp(0, 3999, strength), buttonId);
				yield return null;
			}
			yield return null;
		}
	}
}
