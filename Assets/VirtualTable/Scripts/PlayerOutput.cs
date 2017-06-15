using UnityEngine;

namespace CpvrLab.VirtualTable
{
	/// <summary>
	/// Base class for all player outputs (usally haptic).
	/// 
	/// TODO: Find better names...
	/// </summary>
	public abstract class PlayerOutput : MonoBehaviour
	{
		/// <summary>
		/// The player collides with an object.
		/// </summary>
		/// <param name="force">Force to apply. Value has to be between 0 and 1.</param>
		public abstract void HandleCollision(float force);

		/// <summary>
		/// The player draws, streches, pulls, etc. an object.
		/// </summary>
		/// <param name="force">Force to apply. Value has to be between 0 and 1.</param>
		public abstract void HandleObjectDraw(float force);
	}
}
