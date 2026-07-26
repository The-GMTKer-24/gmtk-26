using System.Collections.Generic;
using Entity;
using UnityEngine;

namespace Dungeon.Traps
{
    public sealed class PoisonTrapDamage : MonoBehaviour
    {
        [SerializeField, Min(0f)]
        private float damagePerSecond = 10f;

        [SerializeField, Min(0.01f)]
        private float damageInterval = 1f;

        [SerializeField, Min(0f)]
        private float initialDamageDelay = 0.5f;

        [Tooltip("Layers that block the poison, such as Walls and Terrain. " +
                 "Do not include the trap or target layers.")]
        [SerializeField]
        private LayerMask lineOfSightBlockingLayers;

        private readonly Dictionary<TimeEntity, float> damageTimers = new();

        private void OnTriggerEnter2D(Collider2D other)
        {
            TimeEntity target = other.GetComponentInParent<TimeEntity>();

            if (target != null && HasLineOfSight(other))
            {
                // Assignment avoids an exception if the target is already tracked.
                damageTimers[target] = initialDamageDelay;
            }
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            TimeEntity target = other.GetComponentInParent<TimeEntity>();

            if (target == null)
            {
                return;
            }

            if (!HasLineOfSight(other))
            {
                damageTimers.Remove(target);
                return;
            }

            if (!damageTimers.TryGetValue(target, out float timeRemaining))
            {
                timeRemaining = initialDamageDelay;
            }

            timeRemaining -= Time.fixedDeltaTime;

            if (timeRemaining <= 0f)
            {
                float damage = damagePerSecond * damageInterval;
                target.DealDamage(damage);

                // Adding the interval retains any small timing overrun.
                timeRemaining += damageInterval;
            }

            damageTimers[target] = timeRemaining;
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            TimeEntity target = other.GetComponentInParent<TimeEntity>();

            if (target != null)
            {
                damageTimers.Remove(target);
            }
        }

        private bool HasLineOfSight(Collider2D target)
        {
            Vector2 origin = transform.position;
            Vector2 destination = target.bounds.center;

            RaycastHit2D obstruction = Physics2D.Linecast(
                origin,
                destination,
                lineOfSightBlockingLayers
            );

            return obstruction.collider == null;
        }
    }
}