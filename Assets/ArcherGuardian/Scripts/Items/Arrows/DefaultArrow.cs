using CpvrLab.ArcherGuardian.Scripts.AGPyroParticles;
using UnityEngine;

namespace CpvrLab.ArcherGuardian.Scripts.Items.Arrows
{
	/// <summary>
	/// Class that represents an arrow that can be fired with a <seealso cref="Bow"/>.
	/// </summary>
	[RequireComponent(typeof(AudioSource))]
	public class DefaultArrow : BaseArrow
	{
		#region members
		#endregion members

		#region overrides
		protected override void OnHitableHit(Hitable hitable)
		{
		}
		protected override void OnAttachedToParent(Vector3 localPos, Quaternion localRot)
		{
			base.OnAttachedToParent(localPos, localRot);

			var fireBase = GetComponent<AGFireBaseScript>();
			if (fireBase != null)
			{
				if (fireBase.IsIgnited)
				{
					// Disable the fire object after it is extinguished
					fireBase.DisableAfterStop = true;
					// The arrow has penetrated the target, so the fire has to be moved to a position where it will be visible.
					fireBase.FireGameObject.transform.localPosition -= Vector3.forward * ArrowPenetrationDepth;
				}
				else
				{
					// Disable immediately since the fire isn't burning.
					fireBase.FireGameObject.SetActive(false);
				}
			}
		}
		#endregion overrides

		#region public
		#endregion public
	}
}