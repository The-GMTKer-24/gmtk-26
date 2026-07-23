using System;
using System.Collections.ObjectModel;
using Entity;
using UnityEngine;

public class TelegraphedAttackArea : MonoBehaviour, IAttackArea
{
    [SerializeField] public IAttackArea InternalAttack;
    [SerializeField] public float delay = 1f;

    private float _remainingTime = 0f;
    private bool _telegraphing = false;
    
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
                InternalAttack.Attack();
            }
        }
    }

    public void Attack()
    {
        _telegraphing = true;
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

    public bool InRange(Vector2 targetPosition)
    {
        return InternalAttack.InRange(targetPosition);
    }

    public Collection<GameObject> GetTargets()
    {
        return InternalAttack.GetTargets();
    }
}