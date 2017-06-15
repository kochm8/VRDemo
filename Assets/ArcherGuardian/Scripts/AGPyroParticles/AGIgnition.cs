using System.Collections.Generic;
using UnityEngine;

namespace CpvrLab.ArcherGuardian.Scripts.AGPyroParticles
{
	/// <summary>
	/// Base class for fire ignition.
	/// Note: For better performance should the associated game object be on a seperated "ignition" layer.
	/// This layer iteracts only with objects on a seperated "fire" layer.
	/// </summary>
	public class AGIgnition : MonoBehaviour
	{
		#region members
		#region public
		[Range(0f, 60f)]
		[Tooltip("Specifies the duration required to ignite the object (contact time with fire).")]
		public int ReqContactTimeForIgnition;
		public AGFireBaseScript FireToIgnite;
		#endregion public

		private Dictionary<int, float> _objectContactDuration = new Dictionary<int, float>();
		#endregion members
		private void OnTriggerEnter(Collider other)
		{
			if (ReqContactTimeForIgnition == 0f)
				Ignite();

			var id = other.gameObject.GetInstanceID();
			Debug.Log("OnTriggerEnter - Ignition" + id);
			_objectContactDuration.Add(id, 0f);
		}
		private void OnTriggerStay(Collider other)
		{
			if (FireToIgnite.IsIgnited)
				return;

			var id = other.gameObject.GetInstanceID();
			_objectContactDuration[id] += Time.deltaTime;
			if (_objectContactDuration[id] >= ReqContactTimeForIgnition)
			{
				Ignite();
			}
		}
		private void OnTriggerExit(Collider other)
		{
			var id = other.gameObject.GetInstanceID();
			Debug.Log("OnTriggerExit - Ignition" + id);
			_objectContactDuration.Remove(id);
		}

		private void Ignite()
		{
			FireToIgnite.Ignite();
		}
	}
}