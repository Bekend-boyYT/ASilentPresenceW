using System;
using UnityEngine;

namespace StarterAssets
{
    [DisallowMultipleComponent]
    public class HealthComponent : MonoBehaviour
    {
        [Tooltip("Optional settings asset for health thresholds, colors, and warning intensity.")]
        public HealthSettings HealthSettingsAsset;

        [Header("Runtime Health")]
        [SerializeField] private float _currentHealth = 100f;

        public event Action<float, float> OnHealthChanged;
        public event Action<HealthSettings.HealthStateType> OnHealthStateChanged;
        public event Action<float> OnDamageTaken;
        public event Action OnDeath;

        private HealthSettings.HealthStateType _currentState;

        public float CurrentHealth => _currentHealth;
        public float MaxHealth => GetSettings().MaxHealth;
        public float HealthPercent => MaxHealth > 0f ? _currentHealth / MaxHealth : 0f;
        public bool IsDead => _currentHealth <= 0f;
        public HealthSettings.HealthStateType CurrentState => _currentState;

        private HealthSettings Settings => HealthSettingsAsset != null ? HealthSettingsAsset : HealthSettings.Default;

        private void Awake()
        {
            EnsureValidHealth();
            _currentState = Settings.GetStateForHealthRatio(HealthPercent);
        }

        private void Start()
        {
            if (_currentHealth <= 0f)
                _currentHealth = MaxHealth;

            EnsureValidHealth();
        }

        public void TakeDamage(float amount)
        {
            if (amount <= 0f || IsDead)
                return;

            OnDamageTaken?.Invoke(amount);
            ModifyHealth(-amount);
        }

        public void Heal(float amount)
        {
            if (amount <= 0f || IsDead)
                return;

            ModifyHealth(amount);
        }

        public void SetHealth(float newHealth)
        {
            if (newHealth < 0f)
                newHealth = 0f;

            if (newHealth > MaxHealth)
                newHealth = MaxHealth;

            if (Mathf.Approximately(newHealth, _currentHealth))
                return;

            float previousHealth = _currentHealth;
            _currentHealth = newHealth;
            DispatchHealthChange(previousHealth);
        }

        public void ResetHealth()
        {
            _currentHealth = MaxHealth;
            DispatchHealthChange(_currentHealth);
        }

        private void ModifyHealth(float delta)
        {
            float previousHealth = _currentHealth;
            _currentHealth = Mathf.Clamp(_currentHealth + delta, 0f, MaxHealth);
            DispatchHealthChange(previousHealth);
        }

        private void DispatchHealthChange(float previousHealth)
        {
            float healthRatio = HealthPercent;
            if (!Mathf.Approximately(previousHealth, _currentHealth))
                OnHealthChanged?.Invoke(_currentHealth, healthRatio);

            HealthSettings.HealthStateType newState = Settings.GetStateForHealthRatio(healthRatio);
            if (newState != _currentState)
            {
                _currentState = newState;
                OnHealthStateChanged?.Invoke(_currentState);
            }

            if (IsDead && previousHealth > 0f)
                OnDeath?.Invoke();
        }

        private void EnsureValidHealth()
        {
            if (MaxHealth <= 0f)
                return;

            _currentHealth = Mathf.Clamp(_currentHealth, 0f, MaxHealth);
        }

        private HealthSettings GetSettings()
        {
            return HealthSettingsAsset != null ? HealthSettingsAsset : HealthSettings.Default;
        }

        private void OnValidate()
        {
            if (_currentHealth < 0f)
                _currentHealth = 0f;

            if (HealthSettingsAsset == null)
                return;

            if (HealthSettingsAsset.MaxHealth <= 0f)
                HealthSettingsAsset.MaxHealth = 1f;

            _currentHealth = Mathf.Clamp(_currentHealth, 0f, MaxHealth);
        }
    }
}
