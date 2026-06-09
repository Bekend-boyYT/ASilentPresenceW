using UnityEngine;

namespace StarterAssets
{
    [RequireComponent(typeof(Collider))]
    public class DamageTriggerZone : MonoBehaviour
    {
        [Tooltip("Amount of damage applied when the player enters the trigger.")]
        public float DamageAmount = 10f;

        [Tooltip("Optional player tag to filter collisions. If empty, the script uses HealthComponent lookup.")]
        public string PlayerTag = "Player";

        private bool _playerInside;

        private void Reset()
        {
            Collider collider = GetComponent<Collider>();
            if (collider != null)
                collider.isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_playerInside)
                return;

            if (IsPlayerCollider(other, out HealthComponent health))
            {
                health.TakeDamage(DamageAmount);
                _playerInside = true;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (!_playerInside)
                return;

            if (IsPlayerCollider(other, out _))
            {
                _playerInside = false;
            }
        }

        private bool IsPlayerCollider(Collider other, out HealthComponent health)
        {
            health = null;

            if (!string.IsNullOrEmpty(PlayerTag) && other.CompareTag(PlayerTag))
            {
                health = other.GetComponentInParent<HealthComponent>();
                return health != null;
            }

            health = other.GetComponentInParent<HealthComponent>();
            return health != null;
        }
    }
}
