using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Attacks;
using Entity;
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
    
    // TODO: Telegraph with sprite change
    
    void Awake()
    {
        _attackInstances = new List<AttackInstance>();
        if (InternalAttackMB == null) throw new Exception("No attack given to telegraph!");
        _internalAttack = (IAttack)InternalAttackMB;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        List<AttackInstance> remove = new List<AttackInstance>();
        
        foreach (AttackInstance instance in _attackInstances)
        {
            instance.RemainingAttackTime -= Time.fixedDeltaTime;
            instance.RemainingDisplayTime -= Time.fixedDeltaTime;

            if (instance.Displaying && instance.RemainingDisplayTime <= 0 && instance.DisplayObject)
            {
                instance.Displaying = false;
                
                instance.DisplayObject.SetActive(false);
                instance.DisplayObject.GetComponent<SpriteRenderer>().enabled = false;
                //print("DESTROY!");
                Destroy(instance.DisplayObject);
            }

            if (instance.RemainingAttackTime <= 0)
            {
                _internalAttack.Attack(instance.Target);
                instance.Waiting = false;
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
        return _internalAttack.IsAoe();
    }
    
    public float GetDelay()
    {
        return attackDelay;
    }

    public float GetDamage()
    {
        return _internalAttack.GetDamage();
    }

    public float GetStaminaCost()
    {
        return _internalAttack.GetStaminaCost();
    }

    public float GetTimeCost()
    {
        return _internalAttack.GetTimeCost();
    }
    
    public float GetRange()
    {
        return _internalAttack.GetRange();
    }

    public bool CanHit(Vector2 targetPosition)
    {
        return _internalAttack.CanHit(targetPosition);
    }

    public float OutOfRangeDistance(Vector2 targetPosition)
    {
        return _internalAttack.OutOfRangeDistance(targetPosition);
    }

    public float CountFriendlyFires(Vector2 targetPosition)
    {
        return _internalAttack.CountFriendlyFires(targetPosition);
    }

    public void Attack(GameObject target)
    {
        GameObject displayObject = null;

        if (directionalAnimation)
        {
            displayObject = Instantiate(directionalAnimation, transform.position, Quaternion.identity);
            displayObject.transform.rotation = Quaternion.Euler(0, 0, -Vector2.SignedAngle(target.transform.position - transform.position, Vector2.up));
        }

        if (targetedAnimation)
        {
            displayObject = Instantiate(targetedAnimation, target.transform.position, Quaternion.identity);
        }
        
        AttackInstance instance = new AttackInstance(target, attackDelay, attackDelay + animationLinger, displayObject);
        
        _attackInstances.Add(instance);
    }

    public bool InRange(Vector2 targetPosition)
    {
        return _internalAttack.CanHit(targetPosition);
    }

    /*public Collection<GameObject> GetAllInRange(float factor)
    {
        return InternalAttack.GetAllInRange(factor);
    }

    public Collection<GameObject> GetAllInRange()
    {
        return InternalAttack.GetAllInRange();
    }*/

    private class AttackInstance
    {
        public GameObject Target;
        public GameObject DisplayObject;
        public float RemainingAttackTime;
        public float RemainingDisplayTime;
        public bool Waiting;
        public bool Displaying;

        public AttackInstance(GameObject target, float attackWait, float displayLength, GameObject displayObject)
        {
            Target = target;
            DisplayObject = displayObject;
            Displaying = displayObject;
            
            if (displayLength > 0)
            {
                RemainingDisplayTime = displayLength;
                Displaying = true;
            }
            else
            {
                RemainingDisplayTime = 0f;
                Displaying = false;
            }

            RemainingAttackTime = attackWait;
            
            //print("Attack: " + RemainingAttackTime + ", Display: " + RemainingDisplayTime);
        }
    }
}