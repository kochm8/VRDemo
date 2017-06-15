using System;
using UnityEngine;
using UnityEngine.Assertions;

namespace CpvrLab.TellLevels.Scripts
{
	public class HitableEventArgs : EventArgs
	{
		public HitableEventArgs(GameObject projectile, float force)
		{
			Assert.IsNotNull(projectile, "HitableEventArgs: projectile not set!");
			Projectile = projectile;
			Force = force;
		}
		public bool IsHandled { get; set; }
		public GameObject Projectile { get; private set; }
		public float Force { get; private set; }
	}

	public class Hitable : MonoBehaviour
	{
		public event EventHandler<HitableEventArgs> Hit;

		/// <summary>
		/// Handle the hit.
		/// </summary>
		/// <param name="sender">Sender</param>
		/// <param name="projectile">Gameobject that caused the hit.</param>
		/// <param name="force">The force with which the projectile hits the hitable.</param>
		/// <returns>True if the hit has been handled, otherwise false.</returns>
		public bool HandleHit(object sender, GameObject projectile, float force)
		{
			if (Hit == null)
				return false;

			var args = new HitableEventArgs(projectile, force);
			Hit(sender, args);

			return args.IsHandled;
		}
	}

}