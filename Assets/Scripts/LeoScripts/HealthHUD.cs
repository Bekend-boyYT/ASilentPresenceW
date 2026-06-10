using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace StarterAssets
{
    [RequireComponent(typeof(CanvasGroup))]
    public class HealthHUD : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("The health component that drives this HUD.")]
        public HealthComponent PlayerHealth;

        [Tooltip("Optional settings asset for the health HUD and screen effects.")]
        public HealthSettings HealthSettings;

        [Header("UI Elements")]
        public Image PanelBackground;
        public Image FillImage;
        public Slider FillSlider;
        public TextMeshProUGUI HealthText;
        public TextMeshProUGUI StateText;
        public ECGLineGraphic ECGLine;
        public Image OverlayTint;
        public RectTransform HudContainer;

        [Header("Audio Hooks")]
        public AudioClip HeartbeatClip;
        public AudioClip CriticalAlertClip;

        [Header("Animation")]
        public float TransitionSpeed = 8f;
        public float OverlaySpeed = 6f;

        private CanvasGroup _canvasGroup;
        private AudioSource _heartbeatSource;
        private AudioSource _sfxSource;
        private Vector2 _initialHudPosition;
        private float _displayHealthRatio;
        private float _overlayTarget;
        private Color _targetFillColor = Color.green;
        private HealthSettings.HealthStateType _currentState;
        private float _pulseTimer;

        private void Awake()
        {
            if (PlayerHealth == null)
                PlayerHealth = FindAnyObjectByType<HealthComponent>();

            if (HealthSettings == null)
                HealthSettings = HealthSettings.Default;

            _canvasGroup = GetComponent<CanvasGroup>();
            if (_canvasGroup == null)
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();

            if (FillSlider != null)
            {
                FillSlider.minValue = 0f;
                FillSlider.maxValue = 1f;
                FillSlider.wholeNumbers = false;
            }

            if (HudContainer == null)
                HudContainer = GetComponent<RectTransform>();

            if (HudContainer != null)
                _initialHudPosition = HudContainer.anchoredPosition;
        }

        private void Start()
        {
            if (PlayerHealth == null)
            {
                Debug.LogWarning("HealthHUD: No HealthComponent assigned or found in scene.");
                enabled = false;
                return;
            }

            SubscribeToHealth();
            SetupAudio();
            RefreshHUD(true);
        }

        private void OnEnable()
        {
            SubscribeToHealth();
        }

        private void OnDisable()
        {
            UnsubscribeFromHealth();
        }

        private void Update()
        {
            if (PlayerHealth == null)
                return;

            _displayHealthRatio = Mathf.MoveTowards(_displayHealthRatio, PlayerHealth.HealthPercent, Time.deltaTime * TransitionSpeed);
            UpdateFillBar(_displayHealthRatio);
            UpdatePulseEffects(_displayHealthRatio);
            UpdateAudio(_displayHealthRatio);
        }

        private void SubscribeToHealth()
        {
            if (PlayerHealth == null)
                return;

            PlayerHealth.OnHealthChanged -= OnHealthChanged;
            PlayerHealth.OnHealthStateChanged -= OnStateChanged;
            PlayerHealth.OnHealthChanged += OnHealthChanged;
            PlayerHealth.OnHealthStateChanged += OnStateChanged;
        }

        private void UnsubscribeFromHealth()
        {
            if (PlayerHealth == null)
                return;

            PlayerHealth.OnHealthChanged -= OnHealthChanged;
            PlayerHealth.OnHealthStateChanged -= OnStateChanged;
        }

        private void OnHealthChanged(float currentHealth, float healthRatio)
        {
            RefreshHUD(false);
        }

        private void OnStateChanged(HealthSettings.HealthStateType newState)
        {
            ApplyState(newState);
        }

        private void RefreshHUD(bool immediate)
        {
            if (PlayerHealth == null)
                return;

            _displayHealthRatio = immediate ? PlayerHealth.HealthPercent : _displayHealthRatio;
            UpdateFillBar(_displayHealthRatio);
            ApplyState(PlayerHealth.CurrentState);
            UpdateHealthText(_displayHealthRatio);
            UpdateOverlay(PlayerHealth.CurrentState, PlayerHealth.HealthPercent, immediate);
        }

        private void ApplyState(HealthSettings.HealthStateType state)
        {
            _currentState = state;
            HealthStateData stateData = HealthSettings.GetStateData(state);
            _targetFillColor = stateData.Color;
            SetVisualColor(stateData.Color);
            if (ECGLine != null)
            {
                ECGLine.SetHealthState(state);
                ECGLine.color = stateData.Color;
            }

            if (OverlayTint != null)
            {
                _overlayTarget = state switch
                {
                    HealthSettings.HealthStateType.Wounded => HealthSettings.LowHealthTintAlpha,
                    HealthSettings.HealthStateType.SeverelyWounded => HealthSettings.CriticalTintAlpha * 0.75f,
                    HealthSettings.HealthStateType.Critical => HealthSettings.CriticalTintAlpha,
                    HealthSettings.HealthStateType.Dead => HealthSettings.DeadTintAlpha,
                    _ => 0f
                };
            }

            if (state == HealthSettings.HealthStateType.Critical)
                PlayCriticalAlert();
        }

        private void UpdateFillBar(float healthRatio)
        {
            if (FillSlider != null)
            {
                FillSlider.value = Mathf.Clamp01(healthRatio);
                SetSliderColor(FillSlider, _targetFillColor);
            }

            if (FillImage != null)
            {
                FillImage.fillAmount = Mathf.Clamp01(healthRatio);
                FillImage.color = Color.Lerp(FillImage.color, _targetFillColor, Time.deltaTime * TransitionSpeed);
            }

            UpdateHealthText(healthRatio);
        }

        private void SetSliderColor(Slider slider, Color color)
        {
            if (slider.fillRect == null)
                return;

            Image fillImage = slider.fillRect.GetComponent<Image>();
            if (fillImage != null)
                fillImage.color = Color.Lerp(fillImage.color, color, Time.deltaTime * TransitionSpeed);
        }

        private void UpdateHealthText(float healthRatio)
        {
            if (HealthText != null)
            {
                int percentage = Mathf.RoundToInt(Mathf.Clamp01(healthRatio) * 100f);
                HealthText.text = percentage <= 0 ? "0%" : $"{percentage}%";
            }

            if (StateText != null)
                StateText.text = HealthSettings.GetStateData(_currentState).Label;
        }

        private void UpdatePulseEffects(float healthRatio)
        {
            float warning = HealthSettings.GetStateData(_currentState).WarningIntensity;
            float pulseFrequency = _currentState == HealthSettings.HealthStateType.Critical
                ? HealthSettings.CriticalPulseFrequency
                : HealthSettings.LowHealthPulseFrequency;

            _pulseTimer += Time.deltaTime * pulseFrequency;
            float pulse = (Mathf.Sin(_pulseTimer * Mathf.PI * 2f) + 1f) * 0.5f;
            float intensity = Mathf.Lerp(0f, warning, pulse);

            if (OverlayTint != null)
            {
                Color target = HealthSettings.WarningOverlayColor;
                target.a = Mathf.Lerp(0f, _overlayTarget, intensity);
                OverlayTint.color = Color.Lerp(OverlayTint.color, target, Time.deltaTime * OverlaySpeed);
            }

            if (HudContainer != null)
            {
                Vector2 shake = Vector2.zero;
                if (_currentState == HealthSettings.HealthStateType.Critical || _currentState == HealthSettings.HealthStateType.SeverelyWounded)
                {
                    float jitter = HealthSettings.ShakeIntensity * warning * 0.08f;
                    shake = new Vector2(
                        (Mathf.PerlinNoise(Time.time * 3.1f, 0f) - 0.5f) * jitter,
                        (Mathf.PerlinNoise(0f, Time.time * 3.7f) - 0.5f) * jitter);
                }

                HudContainer.anchoredPosition = Vector2.Lerp(HudContainer.anchoredPosition, _initialHudPosition + shake, Time.deltaTime * 8f);
            }
        }

        private void UpdateAudio(float healthRatio)
        {
            if (_heartbeatSource == null)
                return;

            float intensity = 1f - healthRatio;
            float pitch = Mathf.Lerp(HealthSettings.HeartbeatPitchMin, HealthSettings.HeartbeatPitchMax, intensity);
            float volume = Mathf.Lerp(HealthSettings.HeartbeatVolumeMin, HealthSettings.HeartbeatVolumeMax, intensity);

            _heartbeatSource.pitch = pitch;
            _heartbeatSource.volume = volume;
        }

        private void SetVisualColor(Color color)
        {
            if (PanelBackground != null)
                PanelBackground.color = Color.Lerp(PanelBackground.color, HealthSettings.PanelBackgroundColor, Time.deltaTime * TransitionSpeed);

            if (StateText != null)
                StateText.color = color;

            if (HealthText != null)
                HealthText.color = color;
        }

        private void UpdateOverlay(HealthSettings.HealthStateType state, float healthRatio, bool immediate)
        {
            if (OverlayTint == null)
                return;

            float targetAlpha = state switch
            {
                HealthSettings.HealthStateType.Wounded => HealthSettings.LowHealthTintAlpha,
                HealthSettings.HealthStateType.SeverelyWounded => HealthSettings.CriticalTintAlpha * 0.6f,
                HealthSettings.HealthStateType.Critical => HealthSettings.CriticalTintAlpha,
                HealthSettings.HealthStateType.Dead => HealthSettings.DeadTintAlpha,
                _ => 0f
            };

            Color color = HealthSettings.WarningOverlayColor;
            color.a = targetAlpha;
            OverlayTint.color = immediate ? color : Color.Lerp(OverlayTint.color, color, Time.deltaTime * OverlaySpeed);
        }

        private void SetupAudio()
        {
            if (HeartbeatClip != null)
            {
                _heartbeatSource = gameObject.AddComponent<AudioSource>();
                _heartbeatSource.clip = HeartbeatClip;
                _heartbeatSource.loop = true;
                _heartbeatSource.spatialBlend = 0f;
                _heartbeatSource.playOnAwake = false;
                _heartbeatSource.volume = HealthSettings.HeartbeatVolumeMin;
                _heartbeatSource.pitch = HealthSettings.HeartbeatPitchMin;
                _heartbeatSource.Play();
            }

            if (CriticalAlertClip != null)
            {
                _sfxSource = gameObject.AddComponent<AudioSource>();
                _sfxSource.spatialBlend = 0f;
                _sfxSource.playOnAwake = false;
                _sfxSource.loop = false;
            }
        }

        private void PlayCriticalAlert()
        {
            if (_sfxSource == null || CriticalAlertClip == null)
                return;

            _sfxSource.PlayOneShot(CriticalAlertClip, 0.6f);
        }
    }
}
