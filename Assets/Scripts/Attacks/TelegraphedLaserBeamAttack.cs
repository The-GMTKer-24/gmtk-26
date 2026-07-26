using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Attacks;
using Entity;
using UnityEngine;
using UnityEngine.Serialization;

public class TelegraphedLaserBeamAttack: MonoBehaviour, IAttack
{
    [SerializeField] public float damage = 10f;
    [SerializeField] public float range = 10f;
    [SerializeField] public float staminaCost = 10f;
    [SerializeField] public float timeCost = 10f;
    [SerializeField] public float turnSpeed = 10f;
    
    [FormerlySerializedAs("directionalAnimation")] [SerializeField] private GameObject telegraphAnimation;
    [SerializeField] GameObject beamAnimation;
    
    [FormerlySerializedAs("delay")] [SerializeField] public float attackDelay = 1f;

    private List<AttackInstance> _attackInstances;
    
    protected TimeEntity TimeEntity;
    protected StaminaEntity StaminaEntity;

    protected void Awake()
    {
        _attackInstances = new List<AttackInstance>();
        TimeEntity = this.gameObject.GetComponent<TimeEntity>();
        if (TimeEntity == null && timeCost != 0)
        {
            throw new Exception("Cannot apply a nonzero time-cost attack to an object with no TimeEntity!");
        }
        StaminaEntity = this.gameObject.GetComponent<StaminaEntity>();
        if (StaminaEntity == null && staminaCost != 0)
        {
            throw new Exception("Cannot apply a nonzero stamina-cost attack to an object with no StaminaEntity!");
        }
    }

    public bool IsAoe()
    {
        return false;
    }
    
    public float GetDamage()
    {
        return damage;
    }
    
    public float GetStaminaCost()
    {
        return staminaCost;
    }
    
    public float GetTimeCost()
    {
        return timeCost;
    }
    
    public float GetRange()
    {
        return range;
    }

    public float GetDelay()
    {
        return attackDelay;
    }

    public float CountFriendlyFires(Vector2 targetPosition)
    {
        RaycastHit2D[] hits = Physics2D.RaycastAll(this.gameObject.transform.position,
            targetPosition - (Vector2)this.gameObject.transform.position, range);

        float friendlyFires = 0f;
        
        foreach (RaycastHit2D hit in hits)
        {
            friendlyFires += GnomeTracker.Instance.DoesGnomeExist(hit.collider.gameObject.GetEntityId()) ? 1f : 0f;
        }
        
        return friendlyFires;
    }

    public bool CanHit(Vector2 targetPosition)
    {
        float distanceSq = Vector2.SqrMagnitude(targetPosition - (Vector2)transform.position);

        return distanceSq <= range * range;
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
            Vector2 vec = instance.Target.transform.position - this.transform.position;
            instance.DirectionController.Update(vec, Time.deltaTime);
            print(instance.DirectionController.print);
        }
    }

    void FixedUpdate()
    {
        List<AttackInstance> remove = new List<AttackInstance>();
        
        foreach (AttackInstance instance in _attackInstances)
        {
            instance.RemainingAttackTime -= Time.fixedDeltaTime;

            if (instance.RemainingAttackTime <= 0)
            {
                LaserAttack(instance, Player.Player.Instance.gameObject.transform.position);
                instance.DisplayObject.SetActive(false);
                instance.DisplayObject.GetComponent<SpriteRenderer>().enabled = false;
                Destroy(instance.DisplayObject);
                remove.Add(instance);
                continue;
            }
            
            //print("new rot: " + instance.DirectionController.direction);
            instance.DisplayObject.transform.position = gameObject.transform.position;
            instance.DisplayObject.transform.rotation = Quaternion.Euler(0, 0, instance.DirectionController.direction);
        }

        foreach (AttackInstance instance in remove)
        {
            _attackInstances.Remove(instance);
        }
    }

    public void Attack(GameObject target)
    {
        print("A");
        if (StaminaEntity.GetStamina() <= staminaCost) return;
        print("B");
        
        GameObject displayObject = null;

        if (telegraphAnimation)
        {
            displayObject = Instantiate(telegraphAnimation, transform.position, Quaternion.identity);
            displayObject.transform.rotation = Quaternion.Euler(0, 0, -Vector2.SignedAngle(target.transform.position - transform.position, Vector2.up));
        }
        else
        {
            throw new Exception("aaaaaaaadasfilakyfuaswy");
        }
        
        AttackInstance instance = new AttackInstance(this.gameObject, target, attackDelay, displayObject, turnSpeed);
        
        _attackInstances.Add(instance);
        
        StaminaEntity.ConsumeStaminaIf(staminaCost);
    }
    
    private void LaserAttack(AttackInstance instance, Vector2 targetPosition)
    {
        float rot = instance.DirectionController.direction;
        Vector2 direction = new Vector2(-Mathf.Sin(Mathf.Deg2Rad*rot), Mathf.Cos(Mathf.Deg2Rad*rot));
        //print("dir: " + direction);
        Vector2 endPosition = direction * range + (Vector2)this.gameObject.transform.position;
        //Instantiate(this.gameObject, endPosition, Quaternion.identity); // WOW!
        
        RaycastHit2D[] hits = Physics2D.RaycastAll(this.gameObject.transform.position, direction, range);

        foreach (RaycastHit2D rayHit in hits)
        {
            GameObject hit = rayHit.collider.gameObject;
            
            TimeEntity targetTimeEntity;
            if (Player.Player.Instance.gameObject.Equals(hit)) targetTimeEntity = Player.Player.Instance.TimeEntity;
            else
            {
                GnomeAI potentialGnome = GnomeTracker.Instance.GetGnome(hit.GetEntityId());
                if (potentialGnome)
                {
                    targetTimeEntity = potentialGnome.timeEntity;
                }
                else
                {
                    targetTimeEntity = hit.GetComponent<TimeEntity>();
                }
            }

            if (!targetTimeEntity)
            {
                continue;
            }

            targetTimeEntity.DealDamage(damage);
        }
        
        GameObject beam = Instantiate(this.beamAnimation, transform.position, Quaternion.identity);
        beam.transform.rotation = Quaternion.Euler(0, 0, instance.DirectionController.direction);
        
        TimeEntity.DealDamage(timeCost);
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
        public DirectionController DirectionController;

        public AttackInstance(GameObject source, GameObject target, float attackWait, GameObject displayObject, float moveSpeed)
        {
            Target = target;
            DisplayObject = displayObject;
            RemainingAttackTime = attackWait;
            Vector2 delta = Target.transform.position - source.transform.position;
            DirectionController = new DirectionController((Unity.Mathematics.math.atan2(delta.y, delta.x) / Unity.Mathematics.math.PI2 * 360 + 360 - 90f) % 360, moveSpeed); // TODO: ???? my brain is less reliable than an llm
            
            //print("Attack: " + RemainingAttackTime + ", Display: " + RemainingDisplayTime);
        }
    }
}