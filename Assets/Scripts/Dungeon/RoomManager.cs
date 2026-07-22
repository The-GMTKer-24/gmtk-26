using System;
using System.Collections.Generic;
using DefaultNamespace;
using UnityEngine;

public class RoomManager : MonoBehaviour
{
    [Serializable]
    public struct RoomChance
    {
        public Room room;
        [Min(0)] public int weight;
    }

    private sealed class GeneratedRoom
    {
        public Room Room;
        public int DistanceFromStart;
        public bool IsExit;
        public readonly HashSet<Direction> ConnectedDirections = new HashSet<Direction>();
        public readonly HashSet<Direction> BlockedDirections = new HashSet<Direction>();

        public int ConnectionCount => ConnectedDirections.Count;
    }

    private struct DoorSocket
    {
        public GeneratedRoom Owner;
        public Direction Direction;

        public DoorSocket(GeneratedRoom owner, Direction direction)
        {
            Owner = owner;
            Direction = direction;
        }
    }

    private static readonly Direction[] AllDirections =
    {
        Direction.North,
        Direction.East,
        Direction.South,
        Direction.West
    };

    public static RoomManager Instance { get; private set; }

    [Header("Rooms")]
    [SerializeField] private Room startRoom;
    [SerializeField] private Room endRoom;
    [SerializeField] private RoomChance[] roomChances;

    [Header("Generation Settings")]
    [Min(2)]
    [SerializeField] private int maxRoomTarget = 20;

    [Min(1)]
    [SerializeField] private int minDistanceToExit = 4;

    [Min(1)]
    [SerializeField] private int maxDistanceToExit = 10;

    [Tooltip("X is normalized distance: 0 = minimum exit distance, 1 = maximum exit distance. Y is probability.")]
    [SerializeField] private AnimationCurve exitRoomChanceFromDistance =
        AnimationCurve.Linear(0f, 0f, 1f, 1f);

    [Min(1)]
    [SerializeField] private int generationAttempts = 25;

    [Min(1)]
    [SerializeField] private int placementAttemptsPerDoor = 12;

    [Min(0f)]
    [SerializeField] private float overlapTolerance = 0.02f;

    [Header("Seed")]
    [SerializeField] private bool useRandomSeed = true;
    [SerializeField] private int seed = 676767;

    private readonly List<Room> rooms = new List<Room>();
    private readonly List<GeneratedRoom> generatedRooms = new List<GeneratedRoom>();

    private Transform generatedRoot;
    private Room exitInstance;
    private int lastGeneratedSeed;

    public IReadOnlyList<Room> Rooms => rooms;
    public Room ExitRoom => exitInstance;
    public int LastGeneratedSeed => lastGeneratedSeed;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("A second RoomManager was destroyed.", this);
            Destroy(gameObject);
            return;
        }

        Instance = this;
        GenerateDungeon();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    [ContextMenu("Regenerate Dungeon")]
    public void GenerateDungeon()
    {
        if (!ValidateConfiguration())
        {
            return;
        }

        ReleaseRoot(generatedRoot);
        generatedRoot = null;

        int baseSeed = useRandomSeed
            ? Environment.TickCount ^ GetEntityId().GetHashCode()
            : seed;

        for (int attempt = 0; attempt < generationAttempts; attempt++)
        {
            int attemptSeed = unchecked(baseSeed + attempt * 486187739);
            System.Random random = new System.Random(attemptSeed);

            rooms.Clear();
            generatedRooms.Clear();
            exitInstance = null;

            Transform attemptRoot = new GameObject($"Generated Dungeon [{attemptSeed}]").transform;
            attemptRoot.SetParent(transform, true);

            if (TryGenerateLayout(random, attemptRoot))
            {
                generatedRoot = attemptRoot;
                lastGeneratedSeed = attemptSeed;
                Debug.Log($"Dungeon generated with {rooms.Count} rooms. Seed: {attemptSeed}", this);
                return;
            }

            ReleaseRoot(attemptRoot);
        }

        rooms.Clear();
        generatedRooms.Clear();
        exitInstance = null;

        Debug.LogError(
            $"Dungeon generation failed after {generationAttempts} attempts. " +
            "Check room door compatibility, room bounds, min/max door counts, and exit-distance settings.",
            this);
    }

    private bool TryGenerateLayout(System.Random random, Transform root)
    {
        Room startInstance = Instantiate(startRoom, transform.position, Quaternion.identity, root);
        startInstance.name = "Start Room";
        startInstance.PrepareForGeneration();

        GeneratedRoom startState = new GeneratedRoom
        {
            Room = startInstance,
            DistanceFromStart = 0,
            IsExit = false
        };

        rooms.Add(startInstance);
        generatedRooms.Add(startState);

        int safetyCounter = maxRoomTarget * 32 + 128;

        while (rooms.Count < maxRoomTarget && safetyCounter-- > 0)
        {
            if (!MinimumDoorCountsAreStillPossible())
            {
                return false;
            }

            bool mustPlaceExit = exitInstance == null && rooms.Count == maxRoomTarget - 1;

            if (mustPlaceExit)
            {
                if (!TryForcePlaceExit(random, root))
                {
                    return false;
                }

                continue;
            }

            List<DoorSocket> sockets = GetPreferredOpenSockets(exitOnly: false);
            if (sockets.Count == 0)
            {
                return false;
            }

            DoorSocket socket = sockets[random.Next(sockets.Count)];
            int newDistance = socket.Owner.DistanceFromStart + 1;

            if (exitInstance == null &&
                IsValidExitDistance(newDistance) &&
                RollForExit(newDistance, random) &&
                TryPlaceRoom(endRoom, socket, true, root, out _))
            {
                continue;
            }

            if (!TryPlaceWeightedNormalRoom(socket, random, root))
            {
                // This socket cannot accept any normal prefab in the current layout.
                socket.Owner.BlockedDirections.Add(socket.Direction);
            }
        }

        return safetyCounter > 0 && ValidateFinishedLayout();
    }

    private bool TryForcePlaceExit(System.Random random, Transform root)
    {
        List<DoorSocket> exitSockets = GetPreferredOpenSockets(exitOnly: true);
        Shuffle(exitSockets, random);

        foreach (DoorSocket socket in exitSockets)
        {
            if (TryPlaceRoom(endRoom, socket, true, root, out _))
            {
                return true;
            }
        }

        return false;
    }

    private bool TryPlaceWeightedNormalRoom(
        DoorSocket socket,
        System.Random random,
        Transform root)
    {
        Direction requiredEntrance = Opposite(socket.Direction);
        List<RoomChance> candidates = new List<RoomChance>();

        foreach (RoomChance chance in roomChances)
        {
            Room prefab = chance.room;

            if (prefab == null ||
                chance.weight <= 0 ||
                prefab.MaxDoors < 1 ||
                prefab.MinDoors < 0 ||
                prefab.MinDoors > prefab.MaxDoors ||
                prefab.MinDoors > prefab.SupportedDoorCount ||
                !prefab.HasCompleteBounds ||
                !prefab.SupportsDirection(requiredEntrance) ||
                !CanAffordNewRoom(socket, prefab))
            {
                continue;
            }

            candidates.Add(chance);
        }

        int attempts = Mathf.Min(placementAttemptsPerDoor, candidates.Count);

        for (int attempt = 0; attempt < attempts; attempt++)
        {
            int candidateIndex = GetWeightedIndex(candidates, random);
            Room prefab = candidates[candidateIndex].room;
            candidates.RemoveAt(candidateIndex);

            if (TryPlaceRoom(prefab, socket, false, root, out _))
            {
                return true;
            }
        }

        return false;
    }

    private bool TryPlaceRoom(
        Room prefab,
        DoorSocket socket,
        bool isExit,
        Transform root,
        out Room placedRoom)
    {
        placedRoom = null;

        if (prefab == null || !CanAffordNewRoom(socket, prefab))
        {
            return false;
        }

        Direction entranceDirection = Opposite(socket.Direction);
        if (!prefab.SupportsDirection(entranceDirection))
        {
            return false;
        }

        Room candidate = Instantiate(prefab, transform.position, Quaternion.identity, root);
        candidate.PrepareForGeneration();

        if (!candidate.SupportsDirection(entranceDirection))
        {
            ReleaseRoom(candidate);
            return false;
        }

        Vector3 sourceDoorPosition = socket.Owner.Room.GetDoorWorldPosition(socket.Direction);
        Vector3 candidateDoorPosition = candidate.GetDoorWorldPosition(entranceDirection);
        candidate.transform.position += sourceDoorPosition - candidateDoorPosition;

        if (OverlapsAnyPlacedRoom(candidate))
        {
            ReleaseRoom(candidate);
            return false;
        }

        socket.Owner.ConnectedDirections.Add(socket.Direction);

        GeneratedRoom candidateState = new GeneratedRoom
        {
            Room = candidate,
            DistanceFromStart = socket.Owner.DistanceFromStart + 1,
            IsExit = isExit
        };
        candidateState.ConnectedDirections.Add(entranceDirection);

        socket.Owner.Room.ShowDoor(socket.Direction);
        candidate.ShowDoor(entranceDirection);

        rooms.Add(candidate);
        generatedRooms.Add(candidateState);

        candidate.name = isExit
            ? "Exit Room"
            : $"Room {rooms.Count - 1:00}";

        if (isExit)
        {
            exitInstance = candidate;
        }

        placedRoom = candidate;
        return true;
    }

    private List<DoorSocket> GetPreferredOpenSockets(bool exitOnly)
    {
        List<DoorSocket> requiredSockets = new List<DoorSocket>();
        List<DoorSocket> optionalSockets = new List<DoorSocket>();

        foreach (GeneratedRoom generatedRoom in generatedRooms)
        {
            if (generatedRoom.IsExit ||
                generatedRoom.ConnectionCount >= generatedRoom.Room.MaxDoors)
            {
                continue;
            }

            bool needsMoreDoors = generatedRoom.ConnectionCount < generatedRoom.Room.MinDoors;

            foreach (Direction direction in AllDirections)
            {
                if (!generatedRoom.Room.SupportsDirection(direction) ||
                    generatedRoom.ConnectedDirections.Contains(direction) ||
                    generatedRoom.BlockedDirections.Contains(direction))
                {
                    continue;
                }

                if (exitOnly)
                {
                    int exitDistance = generatedRoom.DistanceFromStart + 1;
                    if (!IsValidExitDistance(exitDistance) ||
                        !endRoom.SupportsDirection(Opposite(direction)))
                    {
                        continue;
                    }
                }

                DoorSocket socket = new DoorSocket(generatedRoom, direction);
                if (needsMoreDoors)
                {
                    requiredSockets.Add(socket);
                }
                else
                {
                    optionalSockets.Add(socket);
                }
            }
        }

        return requiredSockets.Count > 0 ? requiredSockets : optionalSockets;
    }

    private bool CanAffordNewRoom(DoorSocket socket, Room prefab)
    {
        int roomsRemainingAfterPlacement = maxRoomTarget - (rooms.Count + 1);
        if (roomsRemainingAfterPlacement < 0)
        {
            return false;
        }

        int deficit = GetTotalMinimumDoorDeficit();
        if (socket.Owner.ConnectionCount < socket.Owner.Room.MinDoors)
        {
            deficit--;
        }

        // The incoming connection already supplies one door to the new room
        deficit += Mathf.Max(0, prefab.MinDoors - 1);

        return deficit <= roomsRemainingAfterPlacement;
    }

    private int GetTotalMinimumDoorDeficit()
    {
        int deficit = 0;

        foreach (GeneratedRoom generatedRoom in generatedRooms)
        {
            deficit += Mathf.Max(
                0,
                generatedRoom.Room.MinDoors - generatedRoom.ConnectionCount);
        }

        return deficit;
    }

    private bool MinimumDoorCountsAreStillPossible()
    {
        foreach (GeneratedRoom generatedRoom in generatedRooms)
        {
            int openSocketCount = 0;

            if (!generatedRoom.IsExit &&
                generatedRoom.ConnectionCount < generatedRoom.Room.MaxDoors)
            {
                foreach (Direction direction in AllDirections)
                {
                    if (generatedRoom.Room.SupportsDirection(direction) &&
                        !generatedRoom.ConnectedDirections.Contains(direction) &&
                        !generatedRoom.BlockedDirections.Contains(direction))
                    {
                        openSocketCount++;
                    }
                }

                int remainingCapacity = generatedRoom.Room.MaxDoors - generatedRoom.ConnectionCount;
                openSocketCount = Mathf.Min(openSocketCount, remainingCapacity);
            }

            if (generatedRoom.ConnectionCount + openSocketCount < generatedRoom.Room.MinDoors)
            {
                return false;
            }
        }

        return GetTotalMinimumDoorDeficit() <= maxRoomTarget - rooms.Count;
    }

    private bool OverlapsAnyPlacedRoom(Room candidate)
    {
        if (!candidate.TryGetWorldRect(out Rect candidateRect))
        {
            Debug.LogError($"Room '{candidate.name}' is missing one or more door-bound transforms.", candidate);
            return true;
        }

        foreach (Room placed in rooms)
        {
            if (!placed.TryGetWorldRect(out Rect placedRect))
            {
                Debug.LogError($"Room '{placed.name}' is missing one or more door-bound transforms.", placed);
                return true;
            }

            if (RectanglesOverlapInside(candidateRect, placedRect, overlapTolerance))
            {
                return true;
            }
        }

        return false;
    }

    private static bool RectanglesOverlapInside(Rect a, Rect b, float tolerance)
    {
        return a.xMin < b.xMax - tolerance &&
               a.xMax > b.xMin + tolerance &&
               a.yMin < b.yMax - tolerance &&
               a.yMax > b.yMin + tolerance;
    }

    private bool RollForExit(int distance, System.Random random)
    {
        float normalizedDistance = minDistanceToExit == maxDistanceToExit
            ? 1f
            : Mathf.InverseLerp(minDistanceToExit, maxDistanceToExit, distance);

        float probability = exitRoomChanceFromDistance == null
            ? normalizedDistance
            : exitRoomChanceFromDistance.Evaluate(normalizedDistance);

        return random.NextDouble() <= Mathf.Clamp01(probability);
    }

    private bool IsValidExitDistance(int distance)
    {
        return distance >= minDistanceToExit && distance <= maxDistanceToExit;
    }

    private bool ValidateFinishedLayout()
    {
        if (rooms.Count != maxRoomTarget || exitInstance == null)
        {
            return false;
        }

        GeneratedRoom exitState = null;

        foreach (GeneratedRoom generatedRoom in generatedRooms)
        {
            int connections = generatedRoom.ConnectionCount;
            if (connections < generatedRoom.Room.MinDoors ||
                connections > generatedRoom.Room.MaxDoors)
            {
                return false;
            }

            if (generatedRoom.IsExit)
            {
                exitState = generatedRoom;
            }
        }

        return exitState != null && IsValidExitDistance(exitState.DistanceFromStart);
    }

    private bool ValidateConfiguration()
    {
        if (startRoom == null || endRoom == null)
        {
            Debug.LogError("RoomManager requires both a start room and an end room.", this);
            return false;
        }
        
        

        if (minDistanceToExit > maxRoomTarget - 1)
        {
            Debug.LogError("The minimum exit distance cannot be reached with maxRoomTarget.", this);
            return false;
        }

        if (!IsPrefabConfigurationValid(startRoom) || !IsPrefabConfigurationValid(endRoom))
        {
            return false;
        }

        if (endRoom.MinDoors > 1)
        {
            Debug.LogError("The end room must have MinDoors set to 0 or 1.", endRoom);
            return false;
        }

        bool hasNormalRoom = false;
        if (roomChances != null)
        {
            foreach (RoomChance chance in roomChances)
            {
                if (chance.room != null &&
                    chance.weight > 0 &&
                    IsPrefabConfigurationValid(chance.room))
                {
                    hasNormalRoom = true;
                    break;
                }
            }
        }

        if (!hasNormalRoom && maxRoomTarget > 2)
        {
            Debug.LogError("At least one valid, positively weighted normal room is required.", this);
            return false;
        }

        return true;
    }

    private static bool IsPrefabConfigurationValid(Room prefab, bool logErrors = true)
    {
        bool valid = prefab != null &&
                     prefab.MinDoors >= 0 &&
                     prefab.MaxDoors >= 1 &&
                     prefab.MinDoors <= prefab.MaxDoors &&
                     prefab.MinDoors <= prefab.SupportedDoorCount &&
                     prefab.SupportedDoorCount > 0 &&
                     prefab.HasCompleteBounds;

        if (!valid && logErrors && prefab != null)
        {
            Debug.LogError(
                $"Room prefab '{prefab.name}' has invalid door counts, no supported doors, " +
                "or is missing one of the four door transforms used for bounds.",
                prefab);
        }

        return valid;
    }

    private static int GetWeightedIndex(List<RoomChance> candidates, System.Random random)
    {
        long totalWeight = 0;
        foreach (RoomChance candidate in candidates)
        {
            totalWeight += candidate.weight;
        }

        double roll = random.NextDouble() * totalWeight;
        long cumulativeWeight = 0;

        for (int i = 0; i < candidates.Count; i++)
        {
            cumulativeWeight += candidates[i].weight;
            if (roll < cumulativeWeight)
            {
                return i;
            }
        }

        return candidates.Count - 1;
    }

    private static void Shuffle<T>(IList<T> list, System.Random random)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = random.Next(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    private static Direction Opposite(Direction direction)
    {
        switch (direction)
        {
            case Direction.North: return Direction.South;
            case Direction.East: return Direction.West;
            case Direction.South: return Direction.North;
            case Direction.West: return Direction.East;
            default: throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
        }
    }

    private static void ReleaseRoom(Room room)
    {
        if (room == null)
        {
            return;
        }

        room.gameObject.SetActive(false);
        if (Application.isPlaying)
        {
            Destroy(room.gameObject);
        }
        else
        {
            DestroyImmediate(room.gameObject);
        }
    }

    private static void ReleaseRoot(Transform root)
    {
        if (root == null)
        {
            return;
        }

        root.gameObject.SetActive(false);
        if (Application.isPlaying)
        {
            Destroy(root.gameObject);
        }
        else
        {
            DestroyImmediate(root.gameObject);
        }
    }
}