using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class TelegraphedAttack : MonoBehaviour, IAttack
{
    [SerializeField] private GameObject directionalAnimation;
    [SerializeField] private GameObject targetedAnimation;
    
    [SerializeField] public MonoBehaviour InternalAttackMB;
    [FormerlySerializedAs("delay")] [SerializeField] public float attackDelay = 1f;
    [SerializeField] public float animationLinger = 0f;

    private List<AttackInstance> _attackInstances;
    private IAttack _internalAttack;

    private bool HasInternalAttack =>
        InternalAttackMB != null &&
        _internalAttack != null;
    
    // TODO: Telegraph with sprite change
    
    private void Awake()
    {
        _attackInstances = new List<AttackInstance>();

        if (InternalAttackMB is not IAttack internalAttack ||
            ReferenceEquals(InternalAttackMB, this))
        {
            Debug.LogError(
                "TelegraphedAttack requires a different component that implements IAttack.",
                this
            );
            enabled = false;
            return;
        }

        _internalAttack = internalAttack;
    }

    private void FixedUpdate()
    {
        List<AttackInstance> remove = new List<AttackInstance>();
        
        foreach (AttackInstance instance in _attackInstances)
        {
            instance.RemainingAttackTime -= Time.fixedDeltaTime;
            instance.RemainingDisplayTime -= Time.fixedDeltaTime;

            if (instance.Displaying &&
                instance.RemainingDisplayTime <= 0f)
            {
                instance.Displaying = false;

                foreach (GameObject displayObject in instance.DisplayObjects)
                {
                    if (displayObject != null)
                    {
                        Destroy(displayObject);
                    }
                }

                instance.DisplayObjects.Clear();
            }

            if (instance.Waiting &&
                instance.RemainingAttackTime <= 0f)
            {
                instance.Waiting = false;

                if (instance.Target != null && HasInternalAttack)
                {
                    _internalAttack.Attack(instance.Target);
                }
            }

            if (!instance.Displaying && !instance.Waiting)
            {
                remove.Add(instance);
            }
        }

        foreach (AttackInstance instance in remove)
        {
            _attackInstances.Remove(instance);
        }
    }

    public bool IsAoe()
    {
        return HasInternalAttack && _internalAttack.IsAoe();
    }
    
    public float GetDelay()
    {
        return Mathf.Max(0f, attackDelay);
    }

    public float GetDamage()
    {
        return HasInternalAttack
            ? Mathf.Max(0f, _internalAttack.GetDamage())
            : 0f;
    }

    public float GetStaminaCost()
    {
        return HasInternalAttack
            ? Mathf.Max(0f, _internalAttack.GetStaminaCost())
            : 0f;
    }

    public float GetTimeCost()
    {
        return HasInternalAttack
            ? Mathf.Max(0f, _internalAttack.GetTimeCost())
            : 0f;
    }
    
    public float GetRange()
    {
        return HasInternalAttack
            ? Mathf.Max(0f, _internalAttack.GetRange())
            : 0f;
    }

    public bool CanHit(Vector2 targetPosition)
    {
        return HasInternalAttack &&
               _internalAttack.CanHit(targetPosition);
    }

    public float OutOfRangeDistance(Vector2 targetPosition)
    {
        return HasInternalAttack
            ? _internalAttack.OutOfRangeDistance(targetPosition)
            : float.PositiveInfinity;
    }

    public float CountFriendlyFires(Vector2 targetPosition)
    {
        return HasInternalAttack
            ? Mathf.Max(
                0f,
                _internalAttack.CountFriendlyFires(targetPosition)
            )
            : 0f;
    }

    public void Attack(GameObject target)
    {
        if (!HasInternalAttack || target == null)
        {
            return;
        }

        List<GameObject> displayObjects = new List<GameObject>();

        if (directionalAnimation != null)
        {
            GameObject displayObject = Instantiate(
                directionalAnimation,
                transform.position,
                Quaternion.identity
            );

            displayObject.transform.rotation = Quaternion.Euler(0, 0, -Vector2.SignedAngle(target.transform.position - transform.position, Vector2.up));
            displayObjects.Add(displayObject);
        }

        if (targetedAnimation != null)
        {
            displayObjects.Add(
                Instantiate(
                    targetedAnimation,
                    target.transform.position,
                    Quaternion.identity
                )
            );
        }
        
        AttackInstance instance = new AttackInstance(
            target,
            Mathf.Max(0f, attackDelay),
            Mathf.Max(0f, attackDelay + animationLinger),
            displayObjects
        );
        
        _attackInstances.Add(instance);
    }

    public bool InRange(Vector2 targetPosition)
    {
        return CanHit(targetPosition);
    }

    private void OnDestroy()
    {
        if (_attackInstances == null)
        {
            return;
        }

        foreach (AttackInstance instance in _attackInstances)
        {
            foreach (GameObject displayObject in instance.DisplayObjects)
            {
                if (displayObject != null)
                {
                    Destroy(displayObject);
                }
            }
        }

        _attackInstances.Clear();
    }

    private class AttackInstance
    {
        public GameObject Target;
        public readonly List<GameObject> DisplayObjects;
        public float RemainingAttackTime;
        public float RemainingDisplayTime;
        public bool Waiting;
        public bool Displaying;

        public AttackInstance(
            GameObject target,
            float attackWait,
            float displayLength,
            List<GameObject> displayObjects
        )
        {
            Target = target;
            DisplayObjects = displayObjects;
            RemainingDisplayTime = displayLength;
            RemainingAttackTime = attackWait;
            Waiting = true;
            Displaying = displayObjects.Count > 0;
        }
    }
}
