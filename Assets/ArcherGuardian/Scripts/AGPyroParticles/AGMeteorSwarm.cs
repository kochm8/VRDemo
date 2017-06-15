using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace CpvrLab.ArcherGuardian.Scripts.AGPyroParticles
{
	/// <summary>
	/// Handles the spawning of all meteors.
	/// </summary>
	public class AGMeteorSwarm : NetworkBehaviour
	{
		#region members
		[Tooltip("The game object prefab that represents the meteor.")]
		public GameObject MeteorPrefab;

		[Tooltip("The destination radius")]
		public float DestinationRadius;

		[Tooltip("The source radius")]
		public float SourceRadius;

		[Tooltip("The time it should take the meteors to impact assuming a clear path to destination.")]
		public float TimeToImpact = 1.0f;

		[Tooltip("The height from which the meteors are spawned.")]
		public float SpawnHeight = 100f;

		[Tooltip("Min meteor emission.")]
		[Range(0f, 10f)]
		public float MeteorsMin = 5f;
		[Tooltip("Max meteor emission.")]
		[Range(0f, 10f)]
		public float MeteorsMax = 10f;
		#endregion members

		private void Start()
		{
			if (MeteorsMax < MeteorsMin)
				MeteorsMax = MeteorsMin;

			SpawnMeteors();
			// Destroy after 5 sec
			Destroy(gameObject, 5f);
		}
		[Server]
		private void SpawnMeteors()
		{
			for (int i = 0; i < (int)Random.Range(MeteorsMin, MeteorsMax); i++)
			{
				StartCoroutine(SpawnMeteor());
			}
		}
		[Server]
		private IEnumerator SpawnMeteor()
		{
			float delay = Random.Range(0.0f, 1.0f);
			yield return new WaitForSeconds(delay);

			// find a random source and destination point within the specified radius
			Vector3 src = transform.position + (Random.insideUnitSphere * SourceRadius) + new Vector3(0f, SpawnHeight, 0f);
			var meteorGO = Instantiate(MeteorPrefab, src, Quaternion.identity) as GameObject;
			NetworkServer.Spawn(meteorGO);

			Vector3 dest = transform.position + (Random.insideUnitSphere * DestinationRadius);

			// get the direction and set speed based on how fast the meteor should arrive at the destination
			Vector3 dir = (dest - src);
			Vector3 vel = dir / TimeToImpact;
			Rigidbody meteorRb = meteorGO.GetComponent<Rigidbody>();
			meteorRb.velocity = vel;

			float xRot = Random.Range(-90.0f, 90.0f);
			float yRot = Random.Range(-90.0f, 90.0f);
			float zRot = Random.Range(-90.0f, 90.0f);
			meteorRb.angularVelocity = new Vector3(xRot, yRot, zRot);

			var meteor = meteorGO.GetComponent<AGMeteor>();
			// Initialize meteor with random values (but same on all clients)
			meteor.RpcInitMeteor(
				Random.Range(0, meteor.MeteorMeshes.Length - 1),
				Random.Range(0, meteor.MeteorMaterials.Length - 1),
				Random.Range(0, meteor.EmissionSounds.Length - 1),
				Random.Range(0, meteor.ExplosionSounds.Length - 1));
		}
	}
}