using System;
using Entity;
using UnityEngine;

public class GnomeAI : MonoBehaviour
{
    private static readonly int Up = Animator.StringToHash("Up");
    private static readonly int Down = Animator.StringToHash("Down");
    private static readonly int Left = Animator.StringToHash("Left");
    private static readonly int Right = Animator.StringToHash("Right");
    [SerializeField] public float animationSpeed = 1f;
    [SerializeField] public float speed = 2f;
    [SerializeField] public float forceDropoff = 0.2f;
    
    [SerializeField] public bool canRepeatAttacks;
    
    [SerializeField] public float timeConservationPreference = 0f; // -1 to 1. Negative means preferring to conserve stamina over time, positive prefers time over stamina.
    [SerializeField] public float selfishness = 0.1f; // 0 to 1
    [SerializeField] public float outOfRangeLoss = 1f; // 0.0+
    [SerializeField] public float telegraphLoss = 1f; // 0.0 +, loss per second of telegraphing
    [SerializeField] public float desperationTimeCutoff = 5f; // Time value
    [SerializeField] public float desperationInstantAttackPreference = 10f;
    [SerializeField] public float constantDecrowdingStrength = 0.1f;
    [SerializeField] public float aoeDecrowdingStrength = 0.5f;
    
    [SerializeField] public float currentLoss = 0f; // Debug readout - do not edit
    
    public TimeEntity timeEntity;
    public StaminaEntity staminaEntity;

    private GameObject _player;
    private Rigidbody2D _rb;
    private IAttack _previousAttack;
    private Animator _animator;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _player = Player.Player.Instance.gameObject;
        _previousAttack = null;
        _animator = GetComponent<Animator>();
    }
    
    void FixedUpdate()
    {
        // Choose attack to execute
        
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
            if (!canRepeatAttacks && attack == _previousAttack) continue;
            
            int friendlyFireCount = 0;
            foreach (GameObject hit in attack.GetAllInRange())
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

        if (chosenAttack == null) throw new Exception("Unreachable state");

        currentLoss = attackLoss;
        
        print("Chosen attack: " + chosenAttack.ToString());
        print("Stamina: " + staminaEntity.GetStamina() + " / " + chosenAttack.GetStaminaCost());
        print("Distance: " + Vector2.Distance(playerPos, _rb.transform.position));

        if (chosenAttack.GetStaminaCost() >= staminaEntity.GetStamina() && chosenAttack.InRange(playerPos))
        {
            print("Attacking!");
            _previousAttack = chosenAttack;
            if (chosenAttack is IAttackArea) { (chosenAttack as IAttackArea).Attack(); }
            else if (chosenAttack is IAttackTargeted) { (chosenAttack as IAttackTargeted).Attack(_player); }
            else { throw new System.NotImplementedException("Couldn't attack using IAttack " + chosenAttack + "! Not implemented yet!"); }
        }
        
        // Determine movement
        // TODO: implement pathfinding!
        float targetDist = chosenAttack.GetRange() * 0.8f;
        float currentDist = Vector2.Distance(playerPos, _rb.position);
        float approachStrength = (currentDist / chosenAttack.GetRange() - 0.8f) / 0.2f;

        Vector2 approachVector = Vector2.Normalize(approachStrength * (playerPos - _rb.position));
        Vector2 disperseVector = Vector2.zero;

        foreach (GameObject hit in chosenAttack.GetAllInRange())
        {
            GnomeAI gnomeAI = hit.GetComponent<GnomeAI>();

            if (gnomeAI != null)
            {
                disperseVector += Vector2.Normalize(_rb.transform.position - gnomeAI.transform.position);
            }
        }
        
        disperseVector = Vector2.Normalize(disperseVector);
        approachStrength = Math.Clamp(Math.Abs(approachStrength), 0f, 1f);
        float disperseStrength = (chosenAttack is IAttackArea) ? aoeDecrowdingStrength : constantDecrowdingStrength;
        Vector2 moveVector = approachVector * approachStrength + disperseVector * disperseStrength;
        float curAnimationSpeed = animationSpeed;
        if (moveVector.magnitude >= forceDropoff)
        {
            moveVector = Vector2.Normalize(moveVector) * speed;
        }
        else
        {
            curAnimationSpeed = animationSpeed * moveVector.magnitude / forceDropoff;
            moveVector = moveVector * speed / forceDropoff; // Mathematical!
        }
        
        //print("Vector: " + moveVector.ToString());
        _rb.linearVelocity = moveVector;
        _animator.speed = curAnimationSpeed;

        if (moveVector.magnitude > 0.01f)
        {
            if (Math.Abs(moveVector.x) > Math.Abs(moveVector.y))
            {
                _animator.SetBool(moveVector.x > 0 ? Right : Left, true);
            }
            else
            {
                _animator.SetBool(moveVector.y > 0 ? Up : Down, true);
            }
        }
    }
}

