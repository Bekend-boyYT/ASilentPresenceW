using UnityEngine;

namespace StarterAssets
{
    [CreateAssetMenu(fileName = "HealthSettings", menuName = "StarterAssets/Health Settings", order = 100)]
    public class HealthSettings : ScriptableObject
    {
        public enum HealthStateType
        {
            Dead,
            Critical,
            SeverelyWounded,
            Wounded,
            Healthy
        }

        [Header("Health Thresholds")]
        [Range(0f, 1f)] public float HealthyThreshold = 0.76f;
        [Range(0f, 1f)] public float WoundedThreshold = 0.51f;
        [Range(0f, 1f)] public float SeverelyWoundedThreshold = 0.16f;
        [Range(0f, 1f)] public float CriticalThreshold = 0.15f;

        [Header("Core Health")]
        public float MaxHealth = 100f;
        public float RefillRate = 1f;

        [Header("State Colors")]
        public Color HealthyColor = new Color(0.2f, 0.95f, 0.36f);
        public Color WoundedColor = new Color(1f, 0.65f, 0.12f);
        public Color SeverelyWoundedColor = new Color(1f, 0.32f, 0.1f);
        public Color CriticalColor = new Color(0.72f, 0.05f, 0.07f);
        public Color DeadColor = new Color(0.06f, 0.06f, 0.06f);

        [Header("State Feedback")]
        [Range(0f, 1f)] public float HealthyWarningIntensity = 0f;
        [Range(0f, 1f)] public float WoundedWarningIntensity = 0.18f;
        [Range(0f, 1f)] public float SeverelyWoundedWarningIntensity = 0.35f;
        [Range(0f, 1f)] public float CriticalWarningIntensity = 0.78f;

        [Header("ECG Settings")]
        public float ECGBaseSpeed = 24f;
        public float ECGIrregularity = 0.12f;
        public float ECGCriticalSpeedMultiplier = 2.0f;
        public float ECGCriticalJitter = 0.35f;
        public float ECGSevereJitter = 0.16f;

        [Header("Screen Effects")]
        public float LowHealthPulseFrequency = 1.35f;
        public float CriticalPulseFrequency = 2.8f;
        public float LowHealthPulseStrength = 0.05f;
        public float CriticalPulseStrength = 0.14f;
        public float LowHealthTintAlpha = 0.05f;
        public float CriticalTintAlpha = 0.22f;
        public float DeadTintAlpha = 0.38f;
        public float DesaturationAmount = 0.25f;
        public float ShakeIntensity = 4f;

        [Header("Audio Hooks")]
        public float HeartbeatPitchMin = 0.95f;
        public float HeartbeatPitchMax = 1.75f;
        public float HeartbeatVolumeMin = 0.08f;
        public float HeartbeatVolumeMax = 0.85f;
        public float CriticalBeepInterval = 1.4f;

        [Header("HUD Styling")]
        public Color PanelBackgroundColor = new Color(0.06f, 0.06f, 0.08f, 0.85f);
        public Color StatePanelColor = new Color(0.12f, 0.12f, 0.14f, 0.92f);
        public Color WarningOverlayColor = new Color(0.7f, 0f, 0f, 1f);

        public HealthStateData HealthyState = new HealthStateData
        {
            Label = "HEALTHY",
            WarningIntensity = 0.0f,
            EffectStrength = 0.0f,
            Jitter = 0.08f,
            Color = new Color(0.2f, 0.95f, 0.36f)
        };

        public HealthStateData WoundedState = new HealthStateData
        {
            Label = "WOUNDED",
            WarningIntensity = 0.18f,
            EffectStrength = 0.18f,
            Jitter = 0.15f,
            Color = new Color(1f, 0.65f, 0.12f)
        };

        public HealthStateData SeverelyWoundedState = new HealthStateData
        {
            Label = "SEVERE",
            WarningIntensity = 0.36f,
            EffectStrength = 0.4f,
            Jitter = 0.24f,
            Color = new Color(1f, 0.32f, 0.1f)
        };

        public HealthStateData CriticalState = new HealthStateData
        {
            Label = "CRITICAL",
            WarningIntensity = 0.72f,
            EffectStrength = 0.72f,
            Jitter = 0.38f,
            Color = new Color(0.72f, 0.05f, 0.07f)
        };

        public HealthStateData DeadState = new HealthStateData
        {
            Label = "DEAD",
            WarningIntensity = 1.0f,
            EffectStrength = 1.0f,
            Jitter = 0.0f,
            Color = new Color(0.06f, 0.06f, 0.06f)
        };

        public HealthStateData GetStateData(HealthStateType state)
        {
            return state switch
            {
                HealthStateType.Healthy => HealthyState,
                HealthStateType.Wounded => WoundedState,
                HealthStateType.SeverelyWounded => SeverelyWoundedState,
                HealthStateType.Critical => CriticalState,
                HealthStateType.Dead => DeadState,
                _ => HealthyState
            };
        }

        public HealthStateType GetStateForHealthRatio(float ratio)
        {
            if (ratio <= 0f)
                return HealthStateType.Dead;

            if (ratio <= CriticalThreshold)
                return HealthStateType.Critical;

            if (ratio <= SeverelyWoundedThreshold)
                return HealthStateType.SeverelyWounded;

            if (ratio <= WoundedThreshold)
                return HealthStateType.Wounded;

            return HealthStateType.Healthy;
        }

        private static HealthSettings _defaultInstance;
        public static HealthSettings Default
        {
            get
            {
                if (_defaultInstance == null)
                {
                    _defaultInstance = CreateInstance<HealthSettings>();
                    _defaultInstance.hideFlags = HideFlags.HideAndDontSave;
                }

                return _defaultInstance;
            }
        }
    }

    [System.Serializable]
    public struct HealthStateData
    {
        public string Label;
        public Color Color;
        [Range(0f, 1f)] public float WarningIntensity;
        [Range(0f, 1f)] public float EffectStrength;
        [Range(0f, 1f)] public float Jitter;
    }
}
