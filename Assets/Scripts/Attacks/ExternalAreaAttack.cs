using System.Collections.Generic;
using System.Collections.ObjectModel;
using Attacks;
using Entity;
using UnityEngine;

public class ExternalAreaAttack : MonoBehaviour, IAttack
{
    [SerializeField] private GameObject floorAnimation;
    [SerializeField] private GameObject frontAnimation;
    [SerializeField] private GameObject backAnimation;
    
    [SerializeField, Min(0f)] public float damage = 60f;
    [SerializeField, Min(0f)] public float shotRange = 10f;
    [SerializeField, Min(0f)] public float shotRadius = 2.5f;
    [SerializeField, Min(0f)] public float staminaCost = 10f;
    [SerializeField, Min(0f)] public float timeCost = 10f;
    
    private TimeEntity _timeEntity;
    private StaminaEntity _staminaEntity;
    
    private void Awake()
    {
        _timeEntity = GetComponent<TimeEntity>();
        _staminaEntity = GetComponent<StaminaEntity>();

        if (_timeEntity == null && timeCost > 0f)
        {
            Debug.LogError(
                "ExternalAreaAttack requires a TimeEntity when its time cost is nonzero.",
                this
            );
        }

        if (_staminaEntity == null && staminaCost > 0f)
        {
            Debug.LogError(
                "ExternalAreaAttack requires a StaminaEntity when its stamina cost is nonzero.",
                this
            );
        }
    }
    
    public bool IsAoe()
    {
        return true;
    }
    
    public float GetDelay() {
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
        return Mathf.Max(0f, shotRange);
    }
    
    public bool CanHit(Vector2 targetPosition)
    {
        float distanceSq = Vector2.SqrMagnitude(targetPosition - (Vector2)transform.position);
        float attackRange = GetRange();
        
        return distanceSq <= attackRange * attackRange;
    }

    public float OutOfRangeDistance(Vector2 targetPosition)
    {
        float distance = Vector2.Distance(targetPosition, (Vector2)transform.position);
        
        return distance - GetRange();
    }

    public float CountFriendlyFires(Vector2 targetPosition)
    {
        float hits = 0f;
        float radius = Mathf.Max(0f, shotRadius);

        if (GnomeTracker.Instance == null)
        {
            return hits;
        }

        foreach (GnomeAI gnomeAI in GnomeTracker.Instance.GetGnomeEnumerator())
        {
            if (gnomeAI != null &&
                gnomeAI.gameObject != gameObject &&
                Vector2.SqrMagnitude(
                    (Vector2)gnomeAI.transform.position - targetPosition
                ) <= radius * radius)
            {
                hits += 1f;
            }
        }
        
        return hits;
    }

    public void Attack(GameObject target)
    {
        float safeTimeCost = GetTimeCost();
        float safeStaminaCost = GetStaminaCost();

        if (target == null ||
            (safeTimeCost > 0f && _timeEntity == null) ||
            (safeStaminaCost > 0f &&
             (_staminaEntity == null ||
              !_staminaEntity.ConsumeStaminaIf(safeStaminaCost))))
        {
            return;
        }

        foreach (GameObject hit in GetAllInRange(1f, target.transform.position))
        {
            TimeEntity targetTimeEntity = AttackUtility.FindTimeEntity(hit);

            if (targetTimeEntity == null)
            {
                continue;
            }

            float safeDamage = GetDamage();

            if (safeDamage > 0f)
            {
                targetTimeEntity.DealDamage(safeDamage);
            }
        }

        Vector3 effectPosition = target.transform.position;
        int layer = AttackUtility.GetSortingOrderAtPosition(effectPosition);

        CreateAnimation(floorAnimation, effectPosition, -32767);
        CreateAnimation(
            frontAnimation,
            effectPosition,
            layer + SortingOrderHandler.RecommendedOffset(-0.3f)
        );
        CreateAnimation(backAnimation, effectPosition, layer - 1);
        
        if (safeTimeCost > 0f && _timeEntity != null)
        {
            _timeEntity.DealDamage(safeTimeCost);
        }
    }
    
    private Collection<GameObject> GetAllInRange(float factor, Vector2 center)
    {
        Collection<GameObject> targets = new Collection<GameObject>();
        HashSet<EntityId> targetIds = new HashSet<EntityId>();
        float radius =
            Mathf.Max(0f, factor) * Mathf.Max(0f, shotRadius);
        float radiusSquared = radius * radius;

        if (GnomeTracker.Instance != null)
        {
            foreach (GnomeAI gnomeAI in GnomeTracker.Instance.GetGnomeEnumerator())
            {
                if (gnomeAI == null || gnomeAI.gameObject == gameObject)
                {
                    continue;
                }

                if (Vector2.SqrMagnitude(
                        (Vector2)gnomeAI.transform.position - center
                    ) > radiusSquared)
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
                    (Vector2)player.transform.position - center
                ) <= radiusSquared &&
                targetIds.Add(player.GetEntityId()))
            {
                targets.Add(player);
            }
        }
            
        return targets;
    }

    private static void CreateAnimation(
        GameObject prefab,
        Vector3 position,
        int sortingOrder
    )
    {
        if (prefab == null)
        {
            return;
        }

        GameObject animationObject = Instantiate(
            prefab,
            position,
            Quaternion.identity
        );

        AttackUtility.SetSortingOrder(animationObject, sortingOrder);
    }
}
