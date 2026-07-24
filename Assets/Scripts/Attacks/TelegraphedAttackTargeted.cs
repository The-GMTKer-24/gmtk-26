using System;
using System.Collections.ObjectModel;
using Entity;
using UnityEngine;

public class TelegraphedAttackTargeted : MonoBehaviour, IAttackTargeted
{
    [SerializeField] public IAttackTargeted InternalAttack;
    [SerializeField] public float delay = 1f;

    private float _remainingTime = 0f;
    private bool _telegraphing = false;
    private GameObject _telegraphedTarget = null;
    
    // TODO: Telegraph with sprite change
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (_telegraphing)
        {
            _remainingTime -= Time.deltaTime;
            if (_remainingTime <= 0)
            {
                _telegraphing = false;
                InternalAttack.Attack(_telegraphedTarget);
                _telegraphedTarget = null;
            }
        }
    }
    
    public void Attack(GameObject target)
    {
        _telegraphing = true;
        _telegraphedTarget = target;
    }
    
    public float GetDelay()
    {
        return delay;
    }

    public float GetDamage()
    {
        return InternalAttack.GetDamage();
    }

    public float GetStaminaCost()
    {
        return InternalAttack.GetStaminaCost();
    }

    public float GetTimeCost()
    {
        return InternalAttack.GetTimeCost();
    }
    
    public float GetRange()
    {
        return InternalAttack.GetRange();
    }

    public bool InRange(Vector2 targetPosition)
    {
        return InternalAttack.InRange(targetPosition);
    }

    public Collection<GameObject> GetAllInRange(float factor)
    {
        return InternalAttack.GetAllInRange(factor);
    }
    
    public Collection<GameObject> GetAllInRange()
    {
        return InternalAttack.GetAllInRange();
    }
}