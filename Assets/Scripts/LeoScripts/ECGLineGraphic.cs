using UnityEngine;
using UnityEngine.UI;

namespace StarterAssets
{
    [RequireComponent(typeof(CanvasRenderer))]
    public class ECGLineGraphic : MaskableGraphic
    {
        [Header("Wave Settings")]
        public int SampleCount = 96;
        public float BaseAmplitude = 0.18f;
        public float LineThickness = 3.4f;
        public float CurveIntensity = 2.0f;
        public float WaveSpeed = 0.9f;

        [Header("State Rhythm")]
        public float HealthySpeedModifier = 1.0f;
        public float WoundedSpeedModifier = 1.2f;
        public float SevereSpeedModifier = 1.4f;
        public float CriticalSpeedModifier = 1.9f;

        [Header("Jitter")]
        public float HealthyJitter = 0.06f;
        public float WoundedJitter = 0.12f;
        public float SevereJitter = 0.22f;
        public float CriticalJitter = 0.42f;

        private float _phase;
        private HealthSettings.HealthStateType _healthState = HealthSettings.HealthStateType.Healthy;

        public void SetHealthState(HealthSettings.HealthStateType healthState)
        {
            _healthState = healthState;
            SetVerticesDirty();
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();

            if (SampleCount < 2)
                return;

            Rect rect = GetPixelAdjustedRect();
            float width = rect.width;
            float height = rect.height;
            float halfHeight = height * 0.5f;
            float step = width / (SampleCount - 1);
            Vector2 previous = Vector2.zero;
            bool firstPoint = true;

            for (int i = 0; i < SampleCount; i++)
            {
                float t = i / (float)(SampleCount - 1);
                float x = rect.xMin + step * i;
                float y = rect.yMin + halfHeight + EvaluateWave(t) * height * 0.45f;
                Vector2 point = new Vector2(x, y);

                if (firstPoint)
                {
                    previous = point;
                    firstPoint = false;
                    continue;
                }

                AddSegment(vh, previous, point, LineThickness, color);
                previous = point;
            }
        }

        private float EvaluateWave(float normalizedX)
        {
            float speed = WaveSpeed * GetStateSpeedModifier();
            float jitter = GetStateJitter();
            float time = _phase + normalizedX * 2.4f;
            float heartbeat = Mathf.Sin(time * Mathf.PI * speed * 2f);
            float pulse = Mathf.Pow(Mathf.Abs(Mathf.Sin((time + 0.25f) * Mathf.PI * speed * 1.6f)), 0.85f);
            float spike = Mathf.Clamp01(1f - Mathf.Abs(((normalizedX * 4f + _phase * 0.6f) % 1f) - 0.5f) * 4.5f);
            float wave = heartbeat * 0.55f + pulse * 0.25f + spike * 0.45f;
            float noise = (Mathf.PerlinNoise(normalizedX * 3.5f + _phase * 0.9f, _phase * 0.9f) - 0.5f) * jitter;
            return Mathf.Clamp(wave + noise, -1f, 1f) * BaseAmplitude;
        }

        private float GetStateSpeedModifier()
        {
            return _healthState switch
            {
                HealthSettings.HealthStateType.Wounded => WoundedSpeedModifier,
                HealthSettings.HealthStateType.SeverelyWounded => SevereSpeedModifier,
                HealthSettings.HealthStateType.Critical => CriticalSpeedModifier,
                HealthSettings.HealthStateType.Dead => CriticalSpeedModifier,
                _ => HealthySpeedModifier,
            };
        }

        private float GetStateJitter()
        {
            return _healthState switch
            {
                HealthSettings.HealthStateType.Wounded => WoundedJitter,
                HealthSettings.HealthStateType.SeverelyWounded => SevereJitter,
                HealthSettings.HealthStateType.Critical => CriticalJitter,
                HealthSettings.HealthStateType.Dead => CriticalJitter,
                _ => HealthyJitter,
            };
        }

        private void AddSegment(VertexHelper vh, Vector2 start, Vector2 end, float thickness, Color32 vertexColor)
        {
            Vector2 direction = (end - start).normalized;
            Vector2 normal = new Vector2(-direction.y, direction.x) * thickness * 0.5f;

            UIVertex v1 = UIVertex.simpleVert;
            v1.color = vertexColor;
            v1.position = start - normal;

            UIVertex v2 = UIVertex.simpleVert;
            v2.color = vertexColor;
            v2.position = start + normal;

            UIVertex v3 = UIVertex.simpleVert;
            v3.color = vertexColor;
            v3.position = end + normal;

            UIVertex v4 = UIVertex.simpleVert;
            v4.color = vertexColor;
            v4.position = end - normal;

            int index = vh.currentVertCount;
            vh.AddVert(v1);
            vh.AddVert(v2);
            vh.AddVert(v3);
            vh.AddVert(v4);
            vh.AddTriangle(index, index + 1, index + 2);
            vh.AddTriangle(index, index + 2, index + 3);
        }

        private void Update()
        {
            _phase += Time.deltaTime * 0.42f;
            SetVerticesDirty();
        }
    }
}
