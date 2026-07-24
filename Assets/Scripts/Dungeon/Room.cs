using System;
using System.Collections.Generic;
using System.Linq;
using DefaultNamespace;
using Dungeon;
using UnityEngine;
using Random = UnityEngine.Random;

public class Room : MonoBehaviour
{
    [Header("Game Objects")]
    [SerializeField] private GameObject northDoor;
    [SerializeField] private GameObject eastDoor;
    [SerializeField] private GameObject southDoor;
    [SerializeField] private GameObject westDoor;

    [Header("Room Settings")]
    [SerializeField] private RoomType type;
    [SerializeField] private int minDoors;
    [SerializeField] private int maxDoors;
    [SerializeField] private EnemyPool enemyPool;
    [SerializeField] private float spawnDistance;

    private Door northDoorComponent;
    private Door eastDoorComponent;
    private Door southDoorComponent;
    private Door westDoorComponent;

    private bool supportCached;
    private bool supportsNorth;
    private bool supportsEast;
    private bool supportsSouth;
    private bool supportsWest;

    private readonly List<GameObject> activeEnemies = new List<GameObject>();
    private bool locked;
    private bool triggered;
    public RoomType Type => type;
    public int MaxDoors => maxDoors;
    public int MinDoors => minDoors;

    public bool HasCompleteBounds =>
        northDoor != null &&
        eastDoor != null &&
        southDoor != null &&
        westDoor != null;

    public int SupportedDoorCount
    {
        get
        {
            CacheSupportedDoors();

            int count = 0;
            if (supportsNorth) count++;
            if (supportsEast) count++;
            if (supportsSouth) count++;
            if (supportsWest) count++;

            return count;
        }
    }

    private void Awake()
    {
        CacheDoorComponents();
        CacheSupportedDoors();
    }

    private void Update()
    {
        if (!locked || activeEnemies.Any(enemy => enemy))
        {
            return;
        }

        foreach (Direction direction in EnabledDirections())
        {
            GetDoorComponent(direction)?.OpenDoor();
        }

        locked = false;
    }

    public void PrepareForGeneration()
    {
        CacheDoorComponents();
        CacheSupportedDoors();

        HideDoor(Direction.North);
        HideDoor(Direction.East);
        HideDoor(Direction.South);
        HideDoor(Direction.West);
    }

    public bool SupportsDirection(Direction direction)
    {
        CacheSupportedDoors();

        switch (direction)
        {
            case Direction.North:
                return supportsNorth;
            case Direction.East:
                return supportsEast;
            case Direction.South:
                return supportsSouth;
            case Direction.West:
                return supportsWest;
            default:
                throw new ArgumentOutOfRangeException(
                    nameof(direction),
                    direction,
                    null
                );
        }
    }

    public List<Direction> EnabledDirections()
    {
        List<Direction> directions = new List<Direction>();

        if (IsDoorEnabled(northDoor, northDoorComponent))
        {
            directions.Add(Direction.North);
        }

        if (IsDoorEnabled(eastDoor, eastDoorComponent))
        {
            directions.Add(Direction.East);
        }

        if (IsDoorEnabled(southDoor, southDoorComponent))
        {
            directions.Add(Direction.South);
        }

        if (IsDoorEnabled(westDoor, westDoorComponent))
        {
            directions.Add(Direction.West);
        }

        return directions;
    }

    public void HideDoor(Direction direction)
    {
        GetDoorComponent(direction)?.HideDoor();
    }

    public void ShowDoor(Direction direction)
    {
        if (SupportsDirection(direction))
        {
            GetDoorComponent(direction)?.OpenDoor();
        }
    }

    public void CloseDoor(Direction direction)
    {
        GetDoorComponent(direction)?.CloseDoor();
    }

    public GameObject GetDoorFromDir(Direction direction)
    {
        switch (direction)
        {
            case Direction.North:
                return northDoor;
            case Direction.East:
                return eastDoor;
            case Direction.South:
                return southDoor;
            case Direction.West:
                return westDoor;
            default:
                throw new ArgumentOutOfRangeException(
                    nameof(direction),
                    direction,
                    null
                );
        }
    }

    public Vector3 GetDoorWorldPosition(Direction direction)
    {
        GameObject door = GetDoorFromDir(direction);

        if (door == null)
        {
            throw new InvalidOperationException(
                $"Room '{name}' has no {direction} door transform."
            );
        }

        return door.transform.position;
    }

    public bool TryGetWorldRect(out Rect rect)
    {
        if (!HasCompleteBounds)
        {
            rect = default;
            return false;
        }

        float west = westDoor.transform.position.x;
        float east = eastDoor.transform.position.x;
        float south = southDoor.transform.position.y;
        float north = northDoor.transform.position.y;

        float minX = Mathf.Min(west, east);
        float maxX = Mathf.Max(west, east);
        float minY = Mathf.Min(south, north);
        float maxY = Mathf.Max(south, north);

        rect = Rect.MinMaxRect(minX, minY, maxX, maxY);

        return rect.width > 0f && rect.height > 0f;
    }

    public void OnRoomEnter()
    {
        if (triggered || type != RoomType.Enemy)
        {
            return;
        }

        triggered = true;
        int cost = Random.Range(enemyPool.MinCost, enemyPool.MaxCost);
        int tries = 0;

        while (tries < 10 && cost > 0)
        {
            EnemyPool.SpawnData toSpawn =
                enemyPool.SpawnCosts[
                    Random.Range(0, enemyPool.SpawnCosts.Length)
                ];

            if (cost - toSpawn.cost < 0)
            {
                tries++;
                continue;
            }

            cost -= toSpawn.cost;

            Vector2 spawnPosition =
                Random.insideUnitCircle * spawnDistance +
                (Vector2)transform.position;

            GameObject spawned = Instantiate(
                toSpawn.enemy,
                spawnPosition,
                Quaternion.identity
            );

            activeEnemies.Add(spawned);
        }

        foreach (Direction direction in EnabledDirections())
        {
            GetDoorComponent(direction)?.CloseDoor();
        }

        locked = true;
    }

    private void CacheDoorComponents()
    {
        northDoorComponent = GetDoorComponent(northDoor);
        eastDoorComponent = GetDoorComponent(eastDoor);
        southDoorComponent = GetDoorComponent(southDoor);
        westDoorComponent = GetDoorComponent(westDoor);
    }

    private void CacheSupportedDoors()
    {
        if (supportCached)
        {
            return;
        }

        supportsNorth = northDoor != null && northDoor.activeSelf;
        supportsEast = eastDoor != null && eastDoor.activeSelf;
        supportsSouth = southDoor != null && southDoor.activeSelf;
        supportsWest = westDoor != null && westDoor.activeSelf;

        supportCached = true;
    }

    private Door GetDoorComponent(Direction direction)
    {
        switch (direction)
        {
            case Direction.North:
                return northDoorComponent;
            case Direction.East:
                return eastDoorComponent;
            case Direction.South:
                return southDoorComponent;
            case Direction.West:
                return westDoorComponent;
            default:
                throw new ArgumentOutOfRangeException(
                    nameof(direction),
                    direction,
                    null
                );
        }
    }

    private static Door GetDoorComponent(GameObject door)
    {
        return door != null ? door.GetComponent<Door>() : null;
    }

    private static bool IsDoorEnabled(
        GameObject doorObject,
        Door doorComponent
    )
    {
        return doorObject &&
               doorObject.activeSelf &&
               doorComponent &&
               !doorComponent.Hidden;
    }
}