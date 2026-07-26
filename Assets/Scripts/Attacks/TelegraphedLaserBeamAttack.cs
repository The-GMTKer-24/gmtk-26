using System.Collections.Generic;
using Attacks;
using Entity;
using UnityEngine;
using UnityEngine.Serialization;

public class TelegraphedLaserBeamAttack: MonoBehaviour, IAttack
{
    [SerializeField, Min(0f)] public float damage = 10f;
    [SerializeField, Min(0f)] public float range = 10f;
    [SerializeField, Min(0f)] public float staminaCost = 10f;
    [SerializeField, Min(0f)] public float timeCost = 10f;
    [SerializeField, Min(0f)] public float turnSpeed = 10f;
    
    [FormerlySerializedAs("directionalAnimation")] [SerializeField] private GameObject telegraphAnimation;
    [SerializeField] GameObject beamAnimation;
    
    [FormerlySerializedAs("delay")] [SerializeField] public float attackDelay = 1f;

    private List<AttackInstance> _attackInstances;
    
    protected TimeEntity TimeEntity;
    protected StaminaEntity StaminaEntity;

    private void Awake()
    {
        _attackInstances = new List<AttackInstance>();
        TimeEntity = GetComponent<TimeEntity>();
        StaminaEntity = GetComponent<StaminaEntity>();

        if (TimeEntity == null && timeCost > 0f)
        {
            Debug.LogError(
                "TelegraphedLaserBeamAttack requires a TimeEntity when its time cost is nonzero.",
                this
            );
        }

        if (StaminaEntity == null && staminaCost > 0f)
        {
            Debug.LogError(
                "TelegraphedLaserBeamAttack requires a StaminaEntity when its stamina cost is nonzero.",
                this
            );
        }
    }

    public bool IsAoe()
    {
        return false;
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

    public float GetDelay()
    {
        return Mathf.Max(0f, attackDelay);
    }

    public float CountFriendlyFires(Vector2 targetPosition)
    {
        if (GnomeTracker.Instance == null)
        {
            return 0f;
        }

        Vector2 direction = targetPosition - (Vector2)transform.position;
        float attackRange = GetRange();

        if (direction.sqrMagnitude <= Mathf.Epsilon ||
            attackRange <= 0f)
        {
            return 0f;
        }

        RaycastHit2D[] hits = Physics2D.RaycastAll(
            transform.position,
            direction.normalized,
            attackRange
        );

        float friendlyFires = 0f;
        HashSet<EntityId> countedGnomes = new HashSet<EntityId>();
        
        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider == null)
            {
                continue;
            }

            GnomeAI gnome = hit.collider.GetComponentInParent<GnomeAI>();

            if (gnome != null &&
                gnome.gameObject != gameObject &&
                countedGnomes.Add(gnome.gameObject.GetEntityId()))
            {
                friendlyFires += 1f;
            }
        }
        
        return friendlyFires;
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

    // Update is called once per frame
    private void Update()
    {
        foreach (AttackInstance instance in _attackInstances)
        {
            if (instance.Target != null)
            {
                Vector2 toTarget =
                    instance.Target.transform.position - transform.position;

                instance.DirectionController.Update(toTarget, Time.deltaTime);
            }
        }
    }

    private void FixedUpdate()
    {
        List<AttackInstance> remove = new List<AttackInstance>();
        
        foreach (AttackInstance instance in _attackInstances)
        {
            instance.RemainingAttackTime -= Time.fixedDeltaTime;

            if (instance.RemainingAttackTime <= 0)
            {
                LaserAttack(instance);
                DestroyDisplay(instance);
                remove.Add(instance);
                continue;
            }
            
            if (instance.DisplayObject != null)
            {
                instance.DisplayObject.transform.position =
                    transform.position;
                instance.DisplayObject.transform.rotation =
                    Quaternion.Euler(
                        0f,
                        0f,
                        instance.DirectionController.direction
                    );
            }
        }

        foreach (AttackInstance instance in remove)
        {
            _attackInstances.Remove(instance);
        }
    }

    public void Attack(GameObject target)
    {
        if (target == null)
        {
            return;
        }

        if (telegraphAnimation == null)
        {
            Debug.LogError(
                "TelegraphedLaserBeamAttack has no telegraph animation assigned.",
                this
            );
            return;
        }

        float safeTimeCost = GetTimeCost();
        float safeStaminaCost = GetStaminaCost();

        if (safeTimeCost > 0f && TimeEntity == null)
        {
            return;
        }

        if (safeStaminaCost > 0f &&
            (StaminaEntity == null ||
             !StaminaEntity.ConsumeStaminaIf(safeStaminaCost)))
        {
            return;
        }

        GameObject displayObject = Instantiate(
            telegraphAnimation,
            transform.position,
            Quaternion.identity
        );

        displayObject.transform.rotation = Quaternion.Euler(
            0f,
            0f,
            -Vector2.SignedAngle(
                target.transform.position - transform.position,
                Vector2.up
            )
        );
        
        AttackInstance instance = new AttackInstance(
            gameObject,
            target,
            Mathf.Max(0f, attackDelay),
            displayObject,
            turnSpeed
        );
        
        _attackInstances.Add(instance);
    }
    
    private void LaserAttack(AttackInstance instance)
    {
        float rot = instance.DirectionController.direction;
        Vector2 direction = new Vector2(
            -Mathf.Sin(Mathf.Deg2Rad * rot),
            Mathf.Cos(Mathf.Deg2Rad * rot)
        );

        RaycastHit2D[] hits = Physics2D.RaycastAll(
            transform.position,
            direction,
            GetRange()
        );
        HashSet<EntityId> damagedEntities = new HashSet<EntityId>();

        foreach (RaycastHit2D rayHit in hits)
        {
            if (rayHit.collider == null)
            {
                continue;
            }

            TimeEntity targetTimeEntity =
                AttackUtility.FindTimeEntity(rayHit.collider.gameObject);

            if (targetTimeEntity == null ||
                targetTimeEntity == TimeEntity ||
                !damagedEntities.Add(
                    targetTimeEntity.gameObject.GetEntityId()
                ))
            {
                continue;
            }

            float safeDamage = GetDamage();

            if (safeDamage > 0f)
            {
                targetTimeEntity.DealDamage(safeDamage);
            }
        }

        if (beamAnimation != null)
        {
            GameObject beam = Instantiate(
                beamAnimation,
                transform.position,
                Quaternion.identity
            );

            beam.transform.rotation = Quaternion.Euler(
                0f,
                0f,
                instance.DirectionController.direction
            );
        }

        float safeTimeCost = GetTimeCost();

        if (safeTimeCost > 0f && TimeEntity != null)
        {
            TimeEntity.DealDamage(safeTimeCost);
        }
    }

    private void OnDestroy()
    {
        if (_attackInstances == null)
        {
            return;
        }

        foreach (AttackInstance instance in _attackInstances)
        {
            DestroyDisplay(instance);
        }

        _attackInstances.Clear();
    }

    private void DestroyDisplay(AttackInstance instance)
    {
        if (instance.DisplayObject != null)
        {
            Destroy(instance.DisplayObject);
            instance.DisplayObject = null;
        }
    }

    private class AttackInstance
    {
        public GameObject Target;
        public GameObject DisplayObject;
        public float RemainingAttackTime;
        public DirectionController DirectionController;

        public AttackInstance(GameObject source, GameObject target, float attackWait, GameObject displayObject, float moveSpeed)
        {
            Target = target;
            DisplayObject = displayObject;
            RemainingAttackTime = attackWait;
            Vector2 delta = Target.transform.position - source.transform.position;
            DirectionController = new DirectionController(
                -Vector2.SignedAngle(delta, Vector2.up),
                moveSpeed
            );
        }
    }
}
