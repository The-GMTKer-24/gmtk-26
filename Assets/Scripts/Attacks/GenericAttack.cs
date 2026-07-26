using System.Collections.Generic;
using System.Collections.ObjectModel;
using Entity;
using UnityEngine;

namespace Attacks
{
    public abstract class GenericAttack : MonoBehaviour, IAttack
    {
        [SerializeField, Min(0f)] public float damage = 10f;
        [SerializeField, Min(0f)] public float range = 10f;
        [SerializeField, Min(0f)] public float staminaCost = 10f;
        [SerializeField, Min(0f)] public float timeCost = 10f;
        
        protected TimeEntity TimeEntity;
        protected StaminaEntity StaminaEntity;

        protected void Awake()
        {
            TimeEntity = GetComponent<TimeEntity>();
            StaminaEntity = GetComponent<StaminaEntity>();

            if (TimeEntity == null && timeCost > 0f)
            {
                Debug.LogError(
                    $"{GetType().Name} requires a TimeEntity when its time cost is nonzero.",
                    this
                );
            }

            if (StaminaEntity == null && staminaCost > 0f)
            {
                Debug.LogError(
                    $"{GetType().Name} requires a StaminaEntity when its stamina cost is nonzero.",
                    this
                );
            }
        }

        public abstract bool IsAoe();
        public abstract void Attack(GameObject target);
        public abstract bool CanHit(Vector2 targetPosition);
        public abstract float OutOfRangeDistance(Vector2 targetPosition);
        public abstract float CountFriendlyFires(Vector2 targetPosition);
        
        public virtual float GetDelay()
        {
            return 0f;
        }
        
        public float GetDamage()
        {
            return Mathf.Max(0f, damage);
        }
    
        public float GetStaminaCost()
        {
            return Mathf.Max(0f, staminaCost);
        }
    
        public float GetTimeCost()
        {
            return Mathf.Max(0f, timeCost);
        }
    
        public float GetRange()
        {
            return Mathf.Max(0f, range);
        }

        protected bool TryConsumeStaminaCost()
        {
            float safeTimeCost = GetTimeCost();
            float safeStaminaCost = GetStaminaCost();

            if (safeTimeCost > 0f && TimeEntity == null)
            {
                return false;
            }

            return safeStaminaCost <= 0f ||
                   (StaminaEntity != null &&
                    StaminaEntity.ConsumeStaminaIf(safeStaminaCost));
        }

        protected void ApplyTimeCost()
        {
            float safeTimeCost = GetTimeCost();

            if (safeTimeCost > 0f && TimeEntity != null)
            {
                TimeEntity.DealDamage(safeTimeCost);
            }
        }
    
        public bool InRange(Vector2 targetPosition)
        {
            Vector2 thisPosition = this.gameObject.transform.position;
            float distance = Vector2.Distance(thisPosition, targetPosition);
            return distance <= GetRange();
        }
    
        // Returns all entities that can take damage within range
        protected Collection<GameObject> GetAllInRange(float factor)
        {
            Collection<GameObject> targets = new Collection<GameObject>();
            HashSet<EntityId> targetIds = new HashSet<EntityId>();
            float effectiveRange = Mathf.Max(0f, factor) * GetRange();
            float rangeSquared = effectiveRange * effectiveRange;

            if (GnomeTracker.Instance != null)
            {
                foreach (GnomeAI gnomeAI in GnomeTracker.Instance.GetGnomeEnumerator())
                {
                    if (gnomeAI == null || gnomeAI.gameObject == gameObject)
                    {
                        continue;
                    }

                    if (Vector2.SqrMagnitude(
                            gnomeAI.transform.position - transform.position
                        ) > rangeSquared)
                    {
                        continue;
                    }

                    if (targetIds.Add(gnomeAI.gameObject.GetEntityId()))
                    {
                        targets.Add(gnomeAI.gameObject);
                    }
                }
            }

            if (Player.Player.Instance != null)
            {
                GameObject player = Player.Player.Instance.gameObject;

                if (player != gameObject &&
                    Vector2.SqrMagnitude(
                        player.transform.position - transform.position
                    ) <= rangeSquared &&
                    targetIds.Add(player.GetEntityId()))
                {
                    targets.Add(player);
                }
            }
            
            return targets;
        }
    
        protected Collection<GameObject> GetAllInRange()
        {
            return GetAllInRange(1f);
        }
    }

    internal static class AttackUtility
    {
        public static TimeEntity FindTimeEntity(GameObject target)
        {
            if (target == null)
            {
                return null;
            }

            GnomeAI gnome = target.GetComponentInParent<GnomeAI>();

            if (gnome != null && gnome.timeEntity != null)
            {
                return gnome.timeEntity;
            }

            return target.GetComponentInParent<TimeEntity>();
        }

        public static int GetSortingOrder(GameObject owner)
        {
            if (owner == null)
            {
                return 0;
            }

            GnomeAI gnome = owner.GetComponentInParent<GnomeAI>();

            if (gnome != null)
            {
                return gnome.GetSortingOrder();
            }

            SpriteRenderer spriteRenderer =
                owner.GetComponentInParent<SpriteRenderer>();

            return spriteRenderer != null
                ? spriteRenderer.sortingOrder
                : GetSortingOrderAtPosition(owner.transform.position);
        }

        public static int GetSortingOrderAtPosition(Vector3 position)
        {
            return Mathf.RoundToInt(position.y * -100f);
        }

        public static void SetSortingOrder(
            GameObject animationObject,
            int sortingOrder
        )
        {
            if (animationObject == null)
            {
                return;
            }

            SpriteRenderer spriteRenderer =
                animationObject.GetComponentInChildren<SpriteRenderer>();

            if (spriteRenderer != null)
            {
                spriteRenderer.sortingOrder = sortingOrder;
            }
        }
    }
}
