using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class AttackContainer : MonoBehaviour
{
    [Range(0f, 1f)] [SerializeField] private float randomProportion;
    [SerializeField] private MonoBehaviour[] attacks;
    
    private readonly List<IAttack> _chosenAttacks = new List<IAttack>();

    private void Awake()
    {
        List<IAttack> verAttacks = new List<IAttack>();

        if (attacks == null)
        {
            return;
        }

        foreach (MonoBehaviour mb in attacks)
        {
            if (mb is IAttack attack)
            {
                verAttacks.Add(attack);
            }
            else
            {
                Debug.LogError(
                    mb == null
                        ? "AttackContainer contains an unassigned attack."
                        : $"{mb.GetType().Name} does not implement IAttack.",
                    this
                );
            }
        }

        if (verAttacks.Count == 0)
        {
            return;
        }

        if (verAttacks.Count == 1)
        {
            _chosenAttacks.Add(verAttacks[0]);
            return;
        }

        if (randomProportion <= 0f)
        {
            int randIndex = Random.Range(0, verAttacks.Count);
            _chosenAttacks.Add(verAttacks[randIndex]);
            return;
        }
        
        foreach (IAttack attack in verAttacks)
        {
            if (Random.value < randomProportion)
            {
                _chosenAttacks.Add(attack);
            }
        }

        // Always leave the owner with at least one usable attack.
        if (_chosenAttacks.Count == 0)
        {
            _chosenAttacks.Add(verAttacks[Random.Range(0, verAttacks.Count)]);
        }
    }

    public IEnumerable<IAttack> GetAttacks()
    {
        return _chosenAttacks.AsReadOnly();
    }
}
