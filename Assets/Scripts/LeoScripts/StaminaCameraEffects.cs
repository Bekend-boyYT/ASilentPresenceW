using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace StarterAssets
{
    [RequireComponent(typeof(Volume))]
    public class StaminaCameraEffects : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Reference to the player's first person controller. Used to read stamina state.")]
        public FirstPersonController PlayerController;

        [Tooltip("Volume that contains the post-processing overrides for the camera.")]
        public Volume TargetVolume;

        [Header("Vignette")]
        public float VignetteMinIntensity = 0.05f;
        public float VignetteMaxIntensity = 0.9f;
        public float VignetteMinSmoothness = 0.7f;
        public float VignetteMaxSmoothness = 0.4f;
        public float VignetteCenterOffsetX = 0.5f;
        public float VignetteCenterOffsetY = 0.5f;

        [Header("Motion Blur")]
        public float MotionBlurMinIntensity = 0f;
        public float MotionBlurMaxIntensity = 1f;
        public float MotionBlurMinClamp = 0.05f;
        public float MotionBlurMaxClamp = 0.2f;

        [Header("Chromatic Aberration")]
        public float ChromaticMinIntensity = 0.0f;
        public float ChromaticMaxIntensity = 0.25f;

        [Header("Color & Tone")]
        [Tooltip("How much saturation is removed at low stamina.")]
        public float DesaturationAmount = 0.45f;
        [Tooltip("How much contrast is reduced at low stamina.")]
        public float ContrastReduction = 10f;

        [Header("Lens Distortion")]
        public float LensDistortionMinIntensity = 0f;
        public float LensDistortionMaxIntensity = -2.5f;
        public float LensDistortionMinScale = 1f;
        public float LensDistortionMaxScale = 0.98f;
        public float LensDistortionTensionMultiplier = 0.75f;

        [Header("Pulse")]
        [Tooltip("How fast the low-stamina pulse effect oscillates.")]
        public float LowStaminaPulseFrequency = 2.0f;
        [Tooltip("How strong the low-stamina pulse is when stamina is near empty.")]
        public float LowStaminaPulseAmplitude = 0.06f;
        public float LowStaminaPulseThreshold = 0.7f;

        private Vignette _vignette;
        private MotionBlur _motionBlur;
        private ChromaticAberration _chromaticAberration;
        private ColorAdjustments _colorAdjustments;
        private LensDistortion _lensDistortion;

        private void Awake()
        {
            if (TargetVolume == null)
                TargetVolume = GetComponent<Volume>();

            if (PlayerController == null)
                PlayerController = FindAnyObjectByType<FirstPersonController>();
        }

        private void Start()
        {
            if (PlayerController == null)
                Debug.LogWarning("StaminaCameraEffects: PlayerController is not assigned. Assign it in the inspector or place a FirstPersonController in the scene.");

            if (TargetVolume == null)
            {
                Debug.LogError("StaminaCameraEffects: No Volume found. Attach a Volume to the same GameObject or assign one.");
                enabled = false;
                return;
            }

            if (TargetVolume.profile == null)
            {
                Debug.LogError("StaminaCameraEffects: Volume has no profile assigned. Please create or assign a Volume Profile.");
                enabled = false;
                return;
            }

            TargetVolume.profile.TryGet(out _vignette);
            TargetVolume.profile.TryGet(out _motionBlur);
            TargetVolume.profile.TryGet(out _chromaticAberration);
            TargetVolume.profile.TryGet(out _colorAdjustments);
            TargetVolume.profile.TryGet(out _lensDistortion);

            if (_vignette == null)
                Debug.LogWarning("StaminaCameraEffects: Vignette override is missing from the volume profile.");

            if (_motionBlur == null)
                Debug.LogWarning("StaminaCameraEffects: Motion Blur override is missing from the volume profile.");

            if (_chromaticAberration == null)
                Debug.LogWarning("StaminaCameraEffects: Chromatic Aberration override is missing from the volume profile.");

            if (_colorAdjustments == null)
                Debug.LogWarning("StaminaCameraEffects: Color Adjustments override is missing from the volume profile.");

            if (_lensDistortion == null)
                Debug.LogWarning("StaminaCameraEffects: Lens Distortion override is missing from the volume profile.");

            EnableOverride(_vignette);
            EnableOverride(_motionBlur);
            EnableOverride(_chromaticAberration);
            EnableOverride(_colorAdjustments);
            EnableOverride(_lensDistortion);
        }

        private void Update()
        {
            if (PlayerController == null || TargetVolume == null || TargetVolume.profile == null)
                return;

            float staminaRatio = Mathf.Clamp01(PlayerController.StaminaRatio);
            float tension = 1f - staminaRatio;
            float pulse = ComputeLowStaminaPulse(tension);

            UpdateVignette(tension, pulse);
            UpdateMotionBlur(tension);
            UpdateChromaticAberration(tension);
            UpdateColorAdjustments(tension);
            UpdateLensDistortion(tension);
        }

        private float ComputeLowStaminaPulse(float tension)
        {
            float pulseStrength = Mathf.InverseLerp(LowStaminaPulseThreshold, 1f, tension);
            pulseStrength = Mathf.Clamp01(pulseStrength);
            return Mathf.Sin(Time.time * LowStaminaPulseFrequency * Mathf.PI * 2f) * LowStaminaPulseAmplitude * pulseStrength;
        }

        private void UpdateVignette(float tension, float pulse)
        {
            if (_vignette == null) return;

            float baseIntensity = Mathf.Lerp(VignetteMinIntensity, VignetteMaxIntensity, tension);
            _vignette.intensity.value = Mathf.Clamp01(baseIntensity + pulse);
            _vignette.center.value = new Vector2(VignetteCenterOffsetX, VignetteCenterOffsetY);
            _vignette.smoothness.value = Mathf.Lerp(VignetteMinSmoothness, VignetteMaxSmoothness, tension);
        }

        private void UpdateMotionBlur(float tension)
        {
            if (_motionBlur == null) return;

            _motionBlur.intensity.value = Mathf.Lerp(MotionBlurMinIntensity, MotionBlurMaxIntensity, tension);
            _motionBlur.clamp.value = Mathf.Lerp(MotionBlurMinClamp, MotionBlurMaxClamp, tension);
        }

        private void UpdateChromaticAberration(float tension)
        {
            if (_chromaticAberration == null) return;

            _chromaticAberration.intensity.value = Mathf.Lerp(ChromaticMinIntensity, ChromaticMaxIntensity, tension * 0.9f);
        }

        private void UpdateColorAdjustments(float tension)
        {
            if (_colorAdjustments == null) return;

            _colorAdjustments.saturation.value = Mathf.Lerp(0f, -DesaturationAmount * 100f, tension);
            _colorAdjustments.contrast.value = Mathf.Lerp(0f, -ContrastReduction, tension);
            _colorAdjustments.postExposure.value = Mathf.Lerp(0f, -0.15f, tension * 0.9f);
        }

        private void UpdateLensDistortion(float tension)
        {
            if (_lensDistortion == null) return;

            _lensDistortion.intensity.value = Mathf.Lerp(LensDistortionMinIntensity, LensDistortionMaxIntensity, tension * LensDistortionTensionMultiplier);
            _lensDistortion.scale.value = Mathf.Lerp(LensDistortionMinScale, LensDistortionMaxScale, tension * LensDistortionTensionMultiplier);
        }

        private static void EnableOverride<T>(T volumeComponent) where T : VolumeComponent
        {
            if (volumeComponent == null)
                return;

            if (!volumeComponent.active)
                volumeComponent.active = true;

            // Most override fields are already enabled in the profile, but this ensures the component is active.
        }
    }
}
