using UnityEngine;
using System.Collections;

namespace CpvrLab.VirtualTable
{
    [RequireComponent(typeof(LineRenderer))]
    public class GunshotEffect : MonoBehaviour
    {
        public ParticleSystem gunSmokePS;
        public float traceFadeTime = 0.5f;
        public float traceLineThickness = 0.01f;
        public Color traceLineColor = Color.white;

        private float _fadeTime = 0.0f;

        void Awake()
        {
            var line = GetComponent<LineRenderer>();
            var transparent = new Color(0, 0, 0, 0);
            line.SetVertexCount(2);
            line.SetColors(transparent, transparent);
            line.SetWidth(traceLineThickness, traceLineThickness);
            line.useWorldSpace = true;
        }

        public void Play(Vector3 hitPoint)
        {
            gunSmokePS.Play();
            StartCoroutine(TracerLineEffect(hitPoint));
        }

        IEnumerator TracerLineEffect(Vector3 hitPoint)
        {
            _fadeTime = traceFadeTime;
            var line = GetComponent<LineRenderer>();
            var color = traceLineColor;

            line.SetPosition(0, transform.position);
            line.SetPosition(1, hitPoint);

            while (_fadeTime > 0.0f)
            {
                color.a = _fadeTime / traceFadeTime;
                line.SetColors(color, color);
                _fadeTime -= Time.deltaTime;
                yield return null;
            }

            color.a = 0.0f;
            line.SetColors(color, color);
        }
    }

}