using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace StarterAssets
{
	/// <summary>
	/// Displays a real-time stamina meter and breathing state indicator.
	/// Shows which breathing state is active and current stamina percentage.
	/// </summary>
	public class StaminaMeterUI : MonoBehaviour
	{
		[Header("References")]
		[Tooltip("The FirstPersonController to read stamina from")]
		public FirstPersonController PlayerController;

		[Header("UI Elements")]
		[Tooltip("Slider component for the stamina bar")]
		public Slider StaminaSlider;
		[Tooltip("Text showing current stamina percentage")]
		public TextMeshProUGUI StaminaText;
		[Tooltip("Text showing current breathing state")]
		public TextMeshProUGUI BreathingStateText;

		[Header("Visual Feedback")]
		[Tooltip("Color of the bar when stamina is high")]
		public Color HighStaminaColor = Color.green;
		[Tooltip("Color of the bar when stamina is medium")]
		public Color MediumStaminaColor = Color.yellow;
		[Tooltip("Color of the bar when stamina is low")]
		public Color LowStaminaColor = Color.red;
		[Tooltip("Color of the bar when exhausted")]
		public Color ExhaustedColor = new Color(0.8f, 0f, 0f); // Dark red

		[Header("Fade Settings")]
		[Tooltip("How quickly the meter fades in/out when not using stamina")]
		public float FadeSpeed = 2.0f;
		[Tooltip("Alpha when fully hidden")]
		public float MinAlpha = 0.3f;
		[Tooltip("Alpha when fully visible")]
		public float MaxAlpha = 1.0f;

		private CanvasGroup _canvasGroup;
		private float _targetAlpha;
		private float _currentAlpha;

		private void Start()
		{
			// Auto-find the player controller if not assigned
			if (PlayerController == null)
				PlayerController = FindAnyObjectByType<FirstPersonController>();

			// Get CanvasGroup for fading (add if not present)
			_canvasGroup = GetComponent<CanvasGroup>();
			if (_canvasGroup == null)
				_canvasGroup = gameObject.AddComponent<CanvasGroup>();

			_currentAlpha = MaxAlpha;
			_targetAlpha = MaxAlpha;
		}

		private void Update()
		{
			if (PlayerController == null)
			{
				Debug.LogWarning("StaminaMeterUI: FirstPersonController not found!");
				return;
			}

			float staminaRatio = PlayerController.StaminaRatio;
			bool isExhausted = PlayerController.IsExhausted;

			// Update stamina slider
			if (StaminaSlider != null)
			{
				StaminaSlider.value = staminaRatio;
				// Update fill color based on stamina
				UpdateSliderColor(staminaRatio, isExhausted);
			}

			// Update stamina text
			if (StaminaText != null)
			{
				int staminaPercent = Mathf.RoundToInt(staminaRatio * 100f);
				StaminaText.text = $"{staminaPercent}%";
			}

			// Update breathing state text
			if (BreathingStateText != null)
				BreathingStateText.text = GetBreathingState(staminaRatio, isExhausted);

			// Fade UI based on stamina activity
			UpdateUIAlpha(staminaRatio);
		}

		/// <summary>
		/// Updates the slider's fill color based on stamina level and exhaustion state.
		/// </summary>
		private void UpdateSliderColor(float staminaRatio, bool isExhausted)
		{
			Color fillColor = GetStaminaColor(staminaRatio, isExhausted);
			
			// Update the fill image color of the slider
			Image fillImage = StaminaSlider.fillRect.GetComponent<Image>();
			if (fillImage != null)
				fillImage.color = fillColor;
		}

		/// <summary>
		/// Determines the bar color based on stamina level and exhaustion state.
		/// </summary>
		private Color GetStaminaColor(float staminaRatio, bool isExhausted)
		{
			if (isExhausted)
				return ExhaustedColor;

			if (staminaRatio > 0.5f)
				return Color.Lerp(MediumStaminaColor, HighStaminaColor, (staminaRatio - 0.5f) * 2f);
			else if (staminaRatio > 0.2f)
				return Color.Lerp(LowStaminaColor, MediumStaminaColor, (staminaRatio - 0.2f) / 0.3f);
			else
				return LowStaminaColor;
		}

		/// <summary>
		/// Determines which breathing state is active based on stamina.
		/// </summary>
		private string GetBreathingState(float staminaRatio, bool isExhausted)
		{
			if (staminaRatio <= 0.0f)
				return "EXHAUSTED";
			else if (staminaRatio <= 0.2f)
				return "EXHAUSTED";
			else if (staminaRatio <= 0.5f)
				return "HEAVY";
			else if (staminaRatio <= 0.8f)
				return "MEDIUM";
			else
				return "CALM";
		}

		/// <summary>
		/// Fades the UI in/out based on stamina depletion.
		/// Shows more when stamina is depleting/low, fades when recovering.
		/// </summary>
		private void UpdateUIAlpha(float staminaRatio)
		{
			// Target alpha: more visible when stamina is being used (below 80%)
			if (staminaRatio < 0.8f)
				_targetAlpha = MaxAlpha;
			else
				_targetAlpha = MinAlpha;

			// Smoothly fade
			_currentAlpha = Mathf.Lerp(_currentAlpha, _targetAlpha, Time.deltaTime * FadeSpeed);
			if (_canvasGroup != null)
				_canvasGroup.alpha = _currentAlpha;
		}
	}
}
