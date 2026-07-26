using System.Collections.ObjectModel;
using UnityEngine;

public interface IAttack
{
    public bool IsAoe();
    public float GetDelay();
    public float GetDamage();
    public float GetStaminaCost();
    public float GetTimeCost();
    public float GetRange();
    
    public bool CanHit(Vector2 targetPosition);
    public float OutOfRangeDistance(Vector2 targetPosition); // How far from being in range?
    public float CountFriendlyFires(Vector2 targetPosition);
    public void Attack(GameObject target);
    //public Collection<GameObject> GetAllInRange();
    //public Collection<GameObject> GetAllInRange(float factor);
}