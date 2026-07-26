using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class AttackContainer : MonoBehaviour
{
    [Range(0f, 1f)] [SerializeField] private float randomProportion;
    [SerializeField] private MonoBehaviour[] attacks;
    
    private List<IAttack> _chosenAttacks;

    private void Awake()
    {
        List<IAttack> verAttacks = new List<IAttack>();

        foreach (MonoBehaviour mb in attacks)
        {
            if (mb is IAttack attack)
            {
                verAttacks.Add(attack);
            }
            else
            {
                throw new Exception("MonoBehaviour " + mb + " in AttackContainer is not IAttack!");
            }
        }

        if (verAttacks.Count == 0)
        {
            _chosenAttacks = new List<IAttack>();
            return;
        }
        if (verAttacks.Count == 1)
        {
            _chosenAttacks = new List<IAttack>();
            _chosenAttacks.Add(verAttacks[0]);
            return;
        }
        if (randomProportion < 0.01f)
        {
            _chosenAttacks = new List<IAttack>();
            int randIndex = Random.Range(0, verAttacks.Count);
            _chosenAttacks.Add(verAttacks[randIndex]);
            return;
        }
        
        while (true)
        {
            _chosenAttacks = new List<IAttack>();

            foreach (IAttack attack in verAttacks)
            {
                float randValue = Random.Range(0f, 1f);

                if (randValue < randomProportion)
                {
                    _chosenAttacks.Add(attack);
                }
            }

            if (_chosenAttacks.Count > 0)
            {
                print(_chosenAttacks.Count + " Attacks");
                return;
            }
        }
    }

    public IEnumerable<IAttack> GetAttacks()
    {
        return _chosenAttacks.AsReadOnly().AsEnumerable();
    }
}