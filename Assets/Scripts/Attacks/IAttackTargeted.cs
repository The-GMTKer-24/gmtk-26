using UnityEngine;

public interface IAttackTargeted : IAttack
{
    public void Attack(GameObject target);
}