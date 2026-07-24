using System;
using Entity;
using UnityEngine;

public class GnomeAI : MonoBehaviour
{
    [SerializeField] public float speed;
    
    [SerializeField] public float timeConservationPreference = 0f; // -1 to 1. Negative means preferring to conserve stamina over time, positive prefers time over stamina.
    [SerializeField] public float selfishness = 0.1f; // 0 to 1
    [SerializeField] public float outOfRangeLoss = 1f; // 0.0+
    [SerializeField] public float telegraphLoss = 1f; // 0.0 +, loss per second of telegraphing
    [SerializeField] public float desperationTimeCutoff = 5f; // Time value
    [SerializeField] public float desperationInstantAttackPreference = 10f;
    
    [SerializeField] public float currentLoss = 0f; // Debug readout - do not edit
    
    public TimeEntity timeEntity;
    public StaminaEntity staminaEntity;

    private GameObject _player;
    private Rigidbody _rb;
    private IAttack _previousAttack;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _player = Player.Player.Instance.gameObject;
        _previousAttack = null;
    }

    // Update is called once per frame
    void Update()
    {
        // Choose attack to execute :)
        
        Vector2 playerPos = _player.transform.position;

        IAttack chosenAttack = null;
        float attackLoss = float.PositiveInfinity;
        
        float timeGain = (timeConservationPreference + 1) / 2;
        float staminaGain = 1 - timeGain;

        bool desperate = timeEntity.GetTime() <= desperationTimeCutoff;
        float desperation = timeEntity.GetTime() / desperationTimeCutoff;
        
        // TODO: I have no idea if you can getcomponents an interface
        foreach (IAttack attack in GetComponents<IAttack>())
        {
            int friendlyFireCount = 0;
            foreach (GameObject hit in attack.GetTargets())
            {
                GnomeAI gnomeAI = hit.GetComponent<GnomeAI>();

                if (gnomeAI != null)
                {
                    friendlyFireCount ++;
                }
            }
            float totalDamageLoss = (friendlyFireCount * (1 - selfishness)) + Math.Min(attack.GetTimeCost(), timeEntity.GetTime());
            
            float damagePerStamina = attack.GetDamage() / attack.GetStaminaCost();
            float damagePerTimeLoss = attack.GetDamage() / totalDamageLoss;
            
            float totalLoss = damagePerStamina * staminaGain + damagePerTimeLoss * timeGain;
            if (!attack.InRange(playerPos)) totalLoss += outOfRangeLoss;
            totalLoss += attack.GetDelay() * telegraphLoss;

            if (desperate)
            {
                bool canAttackNow = attack.GetStaminaCost() >= staminaEntity.GetStamina() && attack.InRange(playerPos);
                float desperateLoss = damagePerStamina + attack.GetDelay() * telegraphLoss + (canAttackNow ? 0 : desperationInstantAttackPreference);
                
                totalLoss = desperation * desperateLoss + (1 - desperation) * totalLoss;
            }

            if (totalLoss < attackLoss)
            {
                chosenAttack = attack;
                attackLoss = totalLoss;
            }
        }

        currentLoss = attackLoss;

        if (chosenAttack != null && chosenAttack.GetStaminaCost() >= staminaEntity.GetStamina() && chosenAttack.InRange(playerPos))
        {
            if (chosenAttack is IAttackArea) { (chosenAttack as IAttackArea).Attack(); }
            else if (chosenAttack is IAttackTargeted) { (chosenAttack as IAttackTargeted).Attack(_player); }
            else { throw new System.NotImplementedException("Couldn't attack using IAttack " + chosenAttack + "! Not implemented yet!"); }
        }
    }
}
