using System;
using System.Collections.ObjectModel;
using Attacks;
using Entity;
using UnityEngine;

public class MeleeAttack : GenericAttack, IAttackTargeted
{
    // TODO: Add sprite config
    
    private TimeEntity _timeEntity;
    private StaminaEntity _staminaEntity;

    private void Awake()
    {
        _timeEntity = this.gameObject.GetComponent<TimeEntity>();
        if (_timeEntity == null && timeCost != 0)
        {
            throw new Exception("Cannot apply a nonzero time-cost attack to an object with no TimeEntity!");
        }
        _staminaEntity = this.gameObject.GetComponent<StaminaEntity>();
        if (_staminaEntity == null && staminaCost != 0)
        {
            throw new Exception("Cannot apply a nonzero stamina-cost attack to an object with no StaminaEntity!");
        }
    }

    public void Attack(GameObject target)
    {
        TimeEntity targetTimeEntity;
        if (Player.Player.Instance.gameObject.Equals(target)) targetTimeEntity = Player.Player.Instance.TimeEntity;
        else
        {
            GnomeAI potentialGnome = GnomeTracker.Instance.GetGnome(target.GetEntityId());
            if (potentialGnome != null)
            {
                targetTimeEntity = potentialGnome.timeEntity;
            }
            else
            {
                targetTimeEntity = target.GetComponent<TimeEntity>(); 
            }
        }
        
        if (targetTimeEntity == null)
        {
            throw new Exception("Target entity cannot take damage!");
        }
        
        bool success = true;

        if (timeCost != 0)
        {
            _timeEntity.DealDamage(timeCost);
        }

        if (staminaCost != 0)
        {
            if (!_staminaEntity.ConsumeStaminaIf(staminaCost))
            {
                success = false;
            }
        }

        if (success)
        {
            targetTimeEntity.DealDamage(damage);
        }
    }
}