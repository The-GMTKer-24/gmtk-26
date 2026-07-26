using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class InstantiateOnDestroy : MonoBehaviour
{
    [SerializeField] GameObject obj;
    [SerializeField] private float dropChance;

    private void OnDestroy()
    {
        if (Random.value < dropChance)
        {
            Instantiate(obj, transform.position, Quaternion.identity);
        }
    }
}
