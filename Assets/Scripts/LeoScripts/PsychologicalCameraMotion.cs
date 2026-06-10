using System;
using UnityEngine;

/*
PsychologicalCameraMotion

Modular camera motion module designed to be attached to a camera or camera-target
to add a layered, organic, handheld psychological-horror feel (inspired by MADiSON).

Design goals:
- Layered Perlin noise + low-frequency oscillators for non-repeating natural sway
- Separate idle, walking, and sprint layers
- Rotation inertia / soft follow for mouse look (cinematic lag, not input latency)
- Tiny roll tied to strafing / turning / acceleration
- Settle / overshoot on start/stop
- Inspector-facing, tweakable sliders and tooltips
- Safe to use with CharacterController or Rigidbody — does not change player physics

Integration:
- Assign `MotionTarget` to your camera transform or Cinemachine virtual target
- Call `SetSpeedReferences(walkSpeed, sprintSpeed)` from your player controller
- Call `InitializeMotion()` once at Start, and `UpdateCameraMotion(...)` every LateUpdate

*/

namespace StarterAssets
{
    [DisallowMultipleComponent]
    public class PsychologicalCameraMotion : MonoBehaviour
    {
        [Header("Motion Target")]
        [Tooltip("The transform (camera or virtual camera target) that will receive motion offsets.")]
        public Transform MotionTarget;

        [Header("General Smoothing")]
        [Tooltip("How quickly the camera eases to the target rotation offsets. Lower = snappier.")]
        [Range(0.01f, 0.5f)] public float RotationSmoothTime = 0.08f;
        [Tooltip("How quickly the camera eases to the target position offsets. Lower = snappier.")]
        [Range(0.01f, 0.6f)] public float PositionSmoothTime = 0.12f;

        [Header("Idle Sway")]
        [Tooltip("Intensity of subtle breathing / handheld sway while idle (meters).")]
        public Vector3 IdleSwayPosition = new Vector3(0.012f, 0.006f, 0.006f);
        [Tooltip("Rotation intensity of idle sway (degrees).")]
        public Vector3 IdleSwayRotation = new Vector3(0.25f, 0.25f, 0.12f);
        [Tooltip("Base speed for idle sway oscillation.")]
        [Range(0.02f, 1.0f)] public float IdleSwaySpeed = 0.13f;

        [Header("Walking / Movement Sway")]
        [Tooltip("Position intensity applied while moving (meters).")]
        public Vector3 WalkSwayPosition = new Vector3(0.03f, 0.02f, 0.02f);
        [Tooltip("Rotation intensity applied while moving (degrees).")]
        public Vector3 WalkSwayRotation = new Vector3(1.2f, 0.6f, 1.8f);
        [Tooltip("How quickly walking sway responds to footstep rhythm and velocity.")]
        [Range(0.2f, 4f)] public float WalkSwaySpeed = 0.9f;
        [Tooltip("Small roll amount applied while strafing / turning.")]
        public float WalkRollIntensity = 2.0f;

        [Header("Sprint Modifiers")]
        [Tooltip("Multiplier applied to walk intensities while sprinting (keeps subtle).")]
        [Range(1f, 3f)] public float SprintIntensityMultiplier = 1.35f;

        [Header("Inertia & Settling")]
        [Tooltip("Rotational inertia smoothing for mouse look follow-through (seconds).")]
        public float RotationInertia = 0.06f;
        [Tooltip("How much the camera overshoots/settles when starting/stopping movement.")]
        public float SettleStrength = 0.85f;
        [Tooltip("How quickly the camera settles after movement stops.")]
        public float SettleSpeed = 3.2f;

        [Header("Noise & Randomization")]
        [Tooltip("Extra noise added to prevent mathematical perfection (meters).")]
        public Vector3 NoisePosition = new Vector3(0.007f, 0.004f, 0.003f);
        [Tooltip("Extra rotational noise (degrees).")]
        public Vector3 NoiseRotation = new Vector3(0.18f, 0.18f, 0.12f);
        [Tooltip("Random seed to vary patterns between sessions.")]
        public int NoiseSeed = 0;

        [Header("Inspector Helpers")]
        [Tooltip("How much breathing influences idle sway (0 = none).")]
        [Range(0f, 1f)] public float BreathingInfluence = 0.35f;

        // internal state (non-serialized)
        Vector3 _originalLocalPosition;
        Quaternion _originalLocalRotation;

        // dynamic offsets we smooth toward
        Vector3 _targetPosOffset;
        Vector3 _currentPosOffset;
        Vector3 _posVel;

        Vector3 _targetRotOffset; // euler degrees
        Vector3 _currentRotOffset;
        Vector3 _rotVel;

        float _time;
        float _walkReferenceSpeed = 3f;
        float _sprintReferenceSpeed = 6f;

        // randomized seeds for Perlin layers
        float _seedA, _seedB, _seedC;

        // small reusable buffer
        int _randOffsetA;

        /// <summary>
        /// Provide references to player speeds so motion can scale with velocity.
        /// </summary>
        public void SetSpeedReferences(float walkSpeed, float sprintSpeed)
        {
            _walkReferenceSpeed = Mathf.Max(0.001f, walkSpeed);
            _sprintReferenceSpeed = Mathf.Max(_walkReferenceSpeed + 0.01f, sprintSpeed);
        }

        /// <summary>
        /// Must be called once after assigning MotionTarget. Caches original transforms and seeds.
        /// </summary>
        public void InitializeMotion()
        {
            if (MotionTarget == null)
            {
                MotionTarget = transform;
            }

            _originalLocalPosition = MotionTarget.localPosition;
            _originalLocalRotation = MotionTarget.localRotation;

            _targetPosOffset = Vector3.zero;
            _currentPosOffset = Vector3.zero;
            _targetRotOffset = Vector3.zero;
            _currentRotOffset = Vector3.zero;

            _time = 0f;

            // initialize noise seeds deterministically so inspector seed works
            UnityEngine.Random.InitState(NoiseSeed == 0 ? Environment.TickCount : NoiseSeed);
            _seedA = UnityEngine.Random.Range(0f, 1000f);
            _seedB = UnityEngine.Random.Range(0f, 1000f);
            _seedC = UnityEngine.Random.Range(0f, 1000f);
            _randOffsetA = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
        }

        /// <summary>
        /// Main update entry. Call from player's LateUpdate after camera rotation.
        /// Inputs are read-only and used to scale intensities.
        /// </summary>
        public void UpdateCameraMotion(
            float deltaTime,
            Vector2 lookInput,
            Vector2 moveInput,
            Vector3 velocity,
            bool isSprinting,
            bool grounded,
            bool isCrouching,
            float currentSpeed)
        {
            if (MotionTarget == null) return;
            if (deltaTime <= 0f) return;

            // advance time with slight randomized step to avoid phase-locking
            _time += deltaTime * (1f + (Mathf.PerlinNoise(_seedA, _time * 0.1f) - 0.5f) * 0.04f);

            // blended walk intensity from speed relative to reference speeds
            float moveSpeedRatio = Mathf.Clamp01(currentSpeed / (_sprintReferenceSpeed));
            float walkBlend = Mathf.Clamp01(currentSpeed / _walkReferenceSpeed);
            float sprintFactor = isSprinting ? SprintIntensityMultiplier : 1f;

            // --- Idle sway (very low frequency, breathing influenced) ---
            Vector3 idlePos = new Vector3(
                (Mathf.PerlinNoise(_seedA, _time * IdleSwaySpeed) - 0.5f) * 2f,
                (Mathf.PerlinNoise(_seedB, _time * IdleSwaySpeed * 1.07f) - 0.5f) * 2f,
                (Mathf.PerlinNoise(_seedC, _time * IdleSwaySpeed * 0.91f) - 0.5f) * 2f);

            Vector3 idleRot = new Vector3(
                (Mathf.PerlinNoise(_seedA + 17f, _time * IdleSwaySpeed) - 0.5f) * 2f,
                (Mathf.PerlinNoise(_seedB + 29f, _time * IdleSwaySpeed * 1.05f) - 0.5f) * 2f,
                (Mathf.PerlinNoise(_seedC + 47f, _time * IdleSwaySpeed * 0.95f) - 0.5f) * 2f);

            Vector3 idlePosOffset = Vector3.Scale(idlePos, IdleSwayPosition);
            Vector3 idleRotOffset = Vector3.Scale(idleRot, IdleSwayRotation);

            // small breathing pulse on Y position (slow LFO)
            float breathing = (Mathf.PerlinNoise(_seedA + 99f, _time * IdleSwaySpeed * 0.35f) - 0.45f) * 2f;
            idlePosOffset += Vector3.up * breathing * IdleSwayPosition.y * BreathingInfluence;

            // --- Movement sway (layered, asymmetric, non-sinusoidal) ---
            float movementPhase = _time * (WalkSwaySpeed + walkBlend * WalkSwaySpeed * 0.8f);
            Vector3 moveNoiseA = new Vector3(
                (Mathf.PerlinNoise(_seedA + 101f, movementPhase) - 0.5f) * 2f,
                (Mathf.PerlinNoise(_seedB + 103f, movementPhase * 1.1f) - 0.5f) * 2f,
                (Mathf.PerlinNoise(_seedC + 107f, movementPhase * 0.9f) - 0.5f) * 2f);

            // lateral sway tied to strafing and footstep rhythm (asymmetric)
            float lateral = moveInput.x + (Mathf.PerlinNoise(_seedB + 201f, _time * WalkSwaySpeed * 0.9f) - 0.5f) * 0.4f;
            float forwardBias = moveInput.y + (Mathf.PerlinNoise(_seedC + 223f, _time * WalkSwaySpeed * 1.1f) - 0.5f) * 0.25f;

            Vector3 movePosOffset = new Vector3(
                lateral * WalkSwayPosition.x * walkBlend,
                Mathf.Abs(forwardBias) * WalkSwayPosition.y * walkBlend * 0.6f,
                -forwardBias * WalkSwayPosition.z * walkBlend * 0.4f);

            // rotational movement offsets
            Vector3 moveRotOffset = new Vector3(
                (Mathf.PerlinNoise(_seedA + 301f, movementPhase * 1.05f) - 0.5f) * WalkSwayRotation.x * walkBlend,
                (lateral * WalkSwayRotation.y) * walkBlend,
                (lateral * WalkRollIntensity) * walkBlend);

            // subtle forward inertia when accelerating (delayed settling)
            Vector3 accelInertia = Vector3.zero;
            {
                float accel = velocity.magnitude - currentSpeed; // approximate accel
                accelInertia.z = Mathf.Clamp(accel * 0.0035f, -0.02f, 0.02f);
            }

            // --- Noise layer to avoid obvious loops ---
            Vector3 noisePos = new Vector3(
                (Mathf.PerlinNoise(_seedA + 401f, _time * 1.37f) - 0.5f) * 2f,
                (Mathf.PerlinNoise(_seedB + 421f, _time * 1.59f) - 0.5f) * 2f,
                (Mathf.PerlinNoise(_seedC + 431f, _time * 1.23f) - 0.5f) * 2f);

            Vector3 noiseRot = new Vector3(
                (Mathf.PerlinNoise(_seedA + 443f, _time * 1.11f) - 0.5f) * 2f,
                (Mathf.PerlinNoise(_seedB + 457f, _time * 1.19f) - 0.5f) * 2f,
                (Mathf.PerlinNoise(_seedC + 461f, _time * 1.07f) - 0.5f) * 2f);

            Vector3 noisePosOffset = Vector3.Scale(noisePos, NoisePosition);
            Vector3 noiseRotOffset = Vector3.Scale(noiseRot, NoiseRotation);

            // combine layers with sensible scaling and sprint multiplier
            float intensityScale = isSprinting ? sprintFactor : 1f;

            _targetPosOffset = idlePosOffset * (1f - walkBlend) + (movePosOffset + accelInertia) * walkBlend;
            _targetPosOffset += noisePosOffset * 0.6f * intensityScale;
            _targetPosOffset *= intensityScale;

            _targetRotOffset = idleRotOffset * (1f - walkBlend) + moveRotOffset * walkBlend;
            _targetRotOffset += noiseRotOffset * 0.75f * intensityScale;
            _targetRotOffset *= intensityScale;

            // small additional roll during fast turning / mouse input
            float lookMag = lookInput.magnitude;
            _targetRotOffset.z += lookMag * 0.65f * WalkRollIntensity;

            // apply critical-damped smooth to position and rotation offsets
            _currentPosOffset = Vector3.SmoothDamp(_currentPosOffset, _targetPosOffset, ref _posVel, Mathf.Max(0.001f, PositionSmoothTime), Mathf.Infinity, deltaTime);

            // smooth rotation in euler space (safe because offsets are tiny)
            _currentRotOffset = Vector3.SmoothDamp(_currentRotOffset, _targetRotOffset, ref _rotVel, Mathf.Max(0.001f, RotationSmoothTime), Mathf.Infinity, deltaTime);

            // settling behaviour: when player stops, add a tiny overshoot then settle
            if (moveInput == Vector2.zero && velocity.magnitude < 0.05f)
            {
                _currentPosOffset = Vector3.Lerp(_currentPosOffset, _currentPosOffset * (1f - SettleStrength), SettleSpeed * deltaTime * 0.5f);
                _currentRotOffset = Vector3.Lerp(_currentRotOffset, _currentRotOffset * (1f - SettleStrength), SettleSpeed * deltaTime * 0.5f);
            }

            // write back to MotionTarget
            MotionTarget.localPosition = _originalLocalPosition + _currentPosOffset;

            Quaternion rotOffset = Quaternion.Euler(_currentRotOffset);
            MotionTarget.localRotation = _originalLocalRotation * rotOffset;
        }

        // Optional helper to externally nudge a panic/breathing spike
        public void PulseStress(float intensity, float duration)
        {
            // Placeholder: designers can add timed curves that temporarily increase intensities
            // Kept intentionally simple: a momentary, tiny additive offset to rotation.
            StartCoroutine(PulseRoutine(intensity, duration));
        }

        System.Collections.IEnumerator PulseRoutine(float intensity, float duration)
        {
            float t = 0f;
            Vector3 savedPos = _targetPosOffset;
            Vector3 savedRot = _targetRotOffset;
            while (t < duration)
            {
                float q = 1f - Mathf.Pow(1f - t / duration, 2f);
                _targetRotOffset += UnityEngine.Random.insideUnitSphere * intensity * 0.5f * q;
                _targetPosOffset += UnityEngine.Random.insideUnitSphere * intensity * 0.0025f * q;
                t += Time.deltaTime;
                yield return null;
            }
            _targetPosOffset = savedPos;
            _targetRotOffset = savedRot;
        }
    }
}
