using UnityEngine;

namespace CpvrLab.ArcherGuardian.Scripts.AGPyroParticles
{
	/// <summary>
	/// Simple script to fade in and out a light for an effect, as well as randomize movement for constant effects
	/// </summary>
	[RequireComponent(typeof(Light))]
	public class AGFireLightScript : MonoBehaviour
	{
		#region members
		#region public
		[Tooltip("Random seed for movement, 0 for no movement.")]
		public float Seed = 100.0f;

		[Tooltip("Min and max intensity range.")]
		[Range(0f, 8f)]
		public float IntensityMin = 0f;
		[Tooltip("Min and max intensity range.")]
		[Range(0f, 8f)]
		public float IntensityMax = 0f;

		[Tooltip("Fire base script.")]
		public AGFireBaseScript FireBaseScript;
		[Tooltip("Fire point light.")]
		public Light FirePointLight;
		#endregion public

		private Transform _firePointLightTrans;
		private Vector3 _basePos;
		private float _lightIntensity;
		private float _seed;
		#endregion members

		private void Awake()
		{
			if (IntensityMax < IntensityMin)
				IntensityMax = IntensityMin;

			// set the intensity to 0 so it can fade in nicely
			_lightIntensity = FirePointLight.intensity;
			FirePointLight.intensity = 0.0f;
			_firePointLightTrans = FirePointLight.transform;
			_basePos = _firePointLightTrans.localPosition;

			// Make sure its not 0 (random value could be 0)
			_seed = Seed == 0f ? 0f : Mathf.Max(Random.value * Seed, 0.01f);
		}

		private void Update()
		{
			var intensity = FireBaseScript.CurrentFireIntensity;

			if (_seed != 0f)
			{
				// we have a random movement seed, set up with random movement

				if (!FireBaseScript.Stopping && intensity > 0f)
				{
					// Don't randomize intensity during a stop, it looks bad
					intensity = Mathf.Clamp(intensity * Mathf.PerlinNoise(_seed + Time.time, _seed + 1f + Time.time), IntensityMin, IntensityMax);
				}

				// random movement with perlin noise
				float x = Mathf.PerlinNoise(_seed + 0 + Time.time * 2, _seed + 1 + Time.time * 2) - 0.5f;
				float y = Mathf.PerlinNoise(_seed + 2 + Time.time * 2, _seed + 3 + Time.time * 2) - 0.5f;
				float z = Mathf.PerlinNoise(_seed + 4 + Time.time * 2, _seed + 5 + Time.time * 2) - 0.5f;

				_firePointLightTrans.localPosition = _basePos + new Vector3(x, y, z);
			}
			FirePointLight.intensity = _lightIntensity * intensity;
		}
	}
}