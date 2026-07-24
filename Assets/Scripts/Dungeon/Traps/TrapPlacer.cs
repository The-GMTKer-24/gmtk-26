using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class TrapPlacer : MonoBehaviour
{
    [SerializeField] private GameObject trap;
    [SerializeField] private int maxTraps;
    [SerializeField] private float trapSpawnRate;

    private readonly List<Vector3> spawnedPoints = new();
    void Start()
    {
        if (Random.value > trapSpawnRate)
        {
            Bounds bounds = gameObject.GetComponent<Collider2D>().bounds;
            
            for (int i = 0; i < Random.Range(1, maxTraps); i++)
            {
                Vector3 position = GetRandomPoint(bounds);
                while (spawnedPoints.Contains(position))
                {
                    position = GetRandomPoint(bounds);
                }
                spawnedPoints.Add(position);
                Vector3 offsetSpawn = position + transform.position;
                Instantiate(trap, offsetSpawn, Quaternion.identity, transform);
            }
        }
    }
    
    private static Vector3 GetRandomPoint(Bounds bounds)
    {
        return new Vector3(
            Mathf.Floor(Random.Range(bounds.min.x, bounds.max.x))+.5f,
            Mathf.Floor(Random.Range(bounds.min.y, bounds.max.y))+.5f,
            Mathf.Floor(Random.Range(bounds.min.z, bounds.max.z))+.5f
        );
    }
}
