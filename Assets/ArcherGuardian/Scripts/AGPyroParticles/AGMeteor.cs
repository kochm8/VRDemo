using DigitalRuby.PyroParticles;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace CpvrLab.ArcherGuardian.Scripts.AGPyroParticles
{
	/// <summary>
	/// Handles the meteor swarm effect
	/// </summary>
	public class AGMeteor : AGFireBaseScript
	{
		#region members

		#region public
		[Tooltip("A list of materials to use for the meteors. One will be chosen at random for each meteor.")]
		public Material[] MeteorMaterials;
		[Tooltip("A list of meshes to use for the meteors. One will be chosen at random for each meteor.")]
		public Mesh[] MeteorMeshes;
		[Tooltip("Array of emission sounds. One will be chosen at random upon meteor creation.")]
		public AudioClip[] EmissionSounds;
		[Tooltip("Array of explosion sounds. One will be chosen at random upon impact.")]
		public AudioClip[] ExplosionSounds;
		#endregion public

		#endregion members
		[ClientRpc]
		public void RpcInitMeteor(int meshIdx, int matIdx, int audioIdxEmission, int audioIdxExplosion)
		{
			// setup material
			var renderer = GetComponent<Renderer>();
			renderer.sharedMaterial = MeteorMaterials[matIdx];

			// setup mesh
			var meshFilter = GetComponent<MeshFilter>();
			meshFilter.mesh = MeteorMeshes[meshIdx];

			if (EmissionSounds != null && EmissionSounds.Length > audioIdxEmission)
			{
				AudioSource.PlayOneShot(EmissionSounds[audioIdxEmission]);
			}
			if (ExplosionSounds != null && ExplosionSounds.Length > audioIdxExplosion)
			{
				ExplosionClip = ExplosionSounds[audioIdxExplosion];
			}
		}

		protected override void Start()
		{
			base.Start();
		}

		protected override void OnTriggerEnter(Collider other)
		{
			base.OnTriggerEnter(other);

		}
		protected override void OnExplode()
		{
			base.OnExplode();
			
			GameObject.Destroy(GetComponent<Renderer>());
			Destroy(gameObject, 4f);
		}
	}
}