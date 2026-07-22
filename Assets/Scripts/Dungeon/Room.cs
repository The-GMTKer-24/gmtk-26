using System;
using System.Collections.Generic;
using DefaultNamespace;
using Dungeon;
using UnityEngine;

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

    private bool supportCached;
    private bool supportsNorth;
    private bool supportsEast;
    private bool supportsSouth;
    private bool supportsWest;

    public RoomType Type => type;
    public int MaxDoors => maxDoors;
    public int MinDoors => minDoors;
    

    public bool HasCompleteBounds =>
        northDoor != null && eastDoor != null && southDoor != null && westDoor != null;

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
        CacheSupportedDoors();
    }

    public void PrepareForGeneration()
    {
        // Cache which doors the prefab supports before hiding the runtime visuals.
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
            case Direction.North: return supportsNorth;
            case Direction.East: return supportsEast;
            case Direction.South: return supportsSouth;
            case Direction.West: return supportsWest;
            default: throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
        }
    }

    public List<Direction> EnabledDirections()
    {
        List<Direction> directions = new List<Direction>();

        if (northDoor != null && northDoor.activeSelf) directions.Add(Direction.North);
        if (eastDoor != null && eastDoor.activeSelf) directions.Add(Direction.East);
        if (southDoor != null && southDoor.activeSelf) directions.Add(Direction.South);
        if (westDoor != null && westDoor.activeSelf) directions.Add(Direction.West);

        return directions;
    }

    public void HideDoor(Direction direction)
    {
        GetDoorFromDir(direction).GetComponent<Door>().HideDoor();
    }

    public void ShowDoor(Direction direction)
    {
        if (SupportsDirection(direction))
        {
            GetDoorFromDir(direction).GetComponent<Door>().OpenDoor();
        }
    }

    public void CloseDoor(Direction direction)
    {
        GameObject door = GetDoorFromDir(direction);
        if (door == null)
        {
            return;
        }
        GetDoorFromDir(direction).GetComponent<Door>().CloseDoor();
        // Close the door
    }

    public GameObject GetDoorFromDir(Direction direction)
    {
        switch (direction)
        {
            case Direction.North: return northDoor;
            case Direction.East: return eastDoor;
            case Direction.South: return southDoor;
            case Direction.West: return westDoor;
            default: throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
        }
    }

    public Vector3 GetDoorWorldPosition(Direction direction)
    {
        GameObject door = GetDoorFromDir(direction);
        if (door == null)
        {
            throw new InvalidOperationException($"Room '{name}' has no {direction} door transform.");
        }

        return door.transform.position;
    }

    public bool TryGetWorldRect(out Rect rect)
    {
        if (!HasCompleteBounds)
        {
            rect = default(Rect);
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
        return rect is { width: > 0f, height: > 0f };
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

    private static void SetDoorActive(GameObject door, bool active)
    {
        if (door != null)
        {
            door.SetActive(active);
        }
    }

    public void OnRoomEnter()
    {
        Debug.Log("Enter room!");
    }
}