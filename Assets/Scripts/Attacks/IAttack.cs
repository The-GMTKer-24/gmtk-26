using System.Collections.ObjectModel;
using UnityEngine;

public interface IAttack
{
    public float GetDelay();
    public float GetDamage();
    public float GetStaminaCost();
    public float GetTimeCost();
    public float GetRange();
    
    public bool InRange(Vector2 targetPosition);
    public Collection<GameObject> GetAllInRange();
    public Collection<GameObject> GetAllInRange(float factor);
}