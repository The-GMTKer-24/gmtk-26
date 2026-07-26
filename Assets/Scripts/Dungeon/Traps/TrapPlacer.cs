using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class TrapPlacer : MonoBehaviour
{
    [SerializeField] private GameObject trap;
    [SerializeField] private int maxTraps;
    [SerializeField] private float trapSpawnRate;
    
    private  BoxCollider2D spawnArea;

    private readonly List<Vector3> spawnedPoints = new();
    void Start()
    {
        this.spawnArea = gameObject.GetComponent<BoxCollider2D>();
        if (Random.value > trapSpawnRate)
        {
            // Bounds bounds = col.bounds;
            
            for (int i = 0; i < Random.Range(1, maxTraps); i++)
            {
                Vector3 position = GetRandomPoint(spawnArea);
                while (spawnedPoints.Contains(position))
                {
                    position = GetRandomPoint(spawnArea);
                }
                spawnedPoints.Add(position);
                Instantiate(trap, position, Quaternion.identity, transform);
            }
        }
    }
    
    private Vector3 GetRandomPoint(BoxCollider2D box)
    {
        Vector2 halfSize = box.size * 0.5f;

        Vector2 localPoint = box.offset + new Vector2(
            Mathf.Floor(Random.Range(-halfSize.x, halfSize.x))+.5f,
            Mathf.Floor( Random.Range(-halfSize.y, halfSize.y))+.5f
        );

        return box.transform.TransformPoint(
            new Vector3(localPoint.x, localPoint.y, 0f)
        );

    }
}
