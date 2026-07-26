using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Dungeon
{
    public sealed class DungeonMinimap : MonoBehaviour
    {
        [Header("Layout")] [SerializeField] private RectTransform content;

        [Min(0f)] [SerializeField] private float padding = 10f;

        [Min(1f)] [SerializeField] private float lineThickness = 3f;

        [Min(0.001f)] [SerializeField] private float doorMatchTolerance = 0.05f;

        [SerializeField] private bool revealFirstRoom = true;

        [Header("Colors")] [SerializeField] private Color currentRoomColor = new(1f, 0.8f, 0.15f, 1f);

        [SerializeField] private Color exploredRoomColor = new(0.25f, 0.85f, 1f, 0.95f);

        [SerializeField] private Color adjacentRoomColor = new(0.55f, 0.55f, 0.55f, 0.8f);

        private readonly HashSet<Room> exploredRooms = new();

        private readonly Dictionary<Room, HashSet<Room>> neighbors = new();

        private readonly Dictionary<Room, RoomOutline> outlines = new();

        private Room currentRoom;

        private RectTransform generatedRoot;
        private Room trackedFirstRoom;

        private RoomManager trackedManager;
        private int trackedRoomCount = -1;

        public static DungeonMinimap Instance { get; private set; }

        private void Awake()
        {
            if (Instance && Instance != this)
            {
                Debug.LogWarning(
                    "A second DungeonMinimap component was disabled.",
                    this);

                enabled = false;
                return;
            }

            Instance = this;

            if (!content) content = transform as RectTransform;

            EnsureGeneratedRoot();
        }

        private void LateUpdate()
        {
            var manager = RoomManager.Instance;

            if (manager == null)
            {
                if (trackedManager != null) Clear();

                return;
            }

            var managerRooms = manager.Rooms;

            var firstRoom = managerRooms.Count > 0
                ? managerRooms[0]
                : null;

            var layoutChanged =
                manager != trackedManager ||
                firstRoom != trackedFirstRoom ||
                managerRooms.Count != trackedRoomCount;

            if (layoutChanged) Rebuild(manager, managerRooms);
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public static void RoomEntered(Room room)
        {
            if (Instance) Instance.SetCurrentRoom(room);
        }

        /// <summary>
        ///     Called when the active RoomManager is destroyed.
        /// </summary>
        public static void ClearActive()
        {
            if (Instance) Instance.Clear();
        }

        public void Clear()
        {
            ClearGraphics();

            exploredRooms.Clear();
            currentRoom = null;

            trackedManager = null;
            trackedFirstRoom = null;
            trackedRoomCount = -1;
        }

        private void SetCurrentRoom(Room room)
        {
            if (room == null) return;

            currentRoom = room;
            exploredRooms.Add(room);

            RefreshColors();
        }

        private void Rebuild(
            RoomManager manager,
            IReadOnlyList<Room> sourceRooms)
        {
            var previousExplored =
                new HashSet<Room>(exploredRooms);

            var previousCurrent = currentRoom;

            ClearGraphics();

            exploredRooms.Clear();
            currentRoom = null;

            EnsureGeneratedRoot();

            var validRooms = new List<Room>();
            var roomRects =
                new Dictionary<Room, Rect>();

            Rect dungeonBounds = default;
            var hasDungeonBounds = false;

            for (var i = 0; i < sourceRooms.Count; i++)
            {
                var room = sourceRooms[i];

                if (room == null ||
                    !room.TryGetWorldRect(out var roomRect))
                    continue;

                validRooms.Add(room);
                roomRects.Add(room, roomRect);

                if (!hasDungeonBounds)
                {
                    dungeonBounds = roomRect;
                    hasDungeonBounds = true;
                }
                else
                {
                    dungeonBounds = Rect.MinMaxRect(
                        Mathf.Min(dungeonBounds.xMin, roomRect.xMin),
                        Mathf.Min(dungeonBounds.yMin, roomRect.yMin),
                        Mathf.Max(dungeonBounds.xMax, roomRect.xMax),
                        Mathf.Max(dungeonBounds.yMax, roomRect.yMax));
                }
            }

            trackedManager = manager;
            trackedFirstRoom = sourceRooms.Count > 0
                ? sourceRooms[0]
                : null;
            trackedRoomCount = sourceRooms.Count;

            if (!hasDungeonBounds || validRooms.Count == 0) return;

            Canvas.ForceUpdateCanvases();

            var availableWidth = Mathf.Max(
                1f,
                content.rect.width - padding * 2f);

            var availableHeight = Mathf.Max(
                1f,
                content.rect.height - padding * 2f);

            var worldWidth = Mathf.Max(
                0.0001f,
                dungeonBounds.width);

            var worldHeight = Mathf.Max(
                0.0001f,
                dungeonBounds.height);

            var scale = Mathf.Min(
                availableWidth / worldWidth,
                availableHeight / worldHeight);

            foreach (var room in validRooms)
            {
                var outline = CreateOutline(
                    room,
                    roomRects[room],
                    dungeonBounds,
                    scale);

                outlines.Add(room, outline);

                if (previousExplored.Contains(room)) exploredRooms.Add(room);
            }

            BuildAdjacency(validRooms);

            if (previousCurrent &&
                outlines.ContainsKey(previousCurrent))
            {
                currentRoom = previousCurrent;
                exploredRooms.Add(previousCurrent);
            }
            else if (revealFirstRoom)
            {
                currentRoom = validRooms[0];
                exploredRooms.Add(currentRoom);
            }

            RefreshColors();
        }

        private RoomOutline CreateOutline(
            Room room,
            Rect worldRect,
            Rect dungeonBounds,
            float scale)
        {
            var rootObject = new GameObject(
                room.name,
                typeof(RectTransform))
            {
                layer = generatedRoot.gameObject.layer
            };

            var root =
                rootObject.GetComponent<RectTransform>();

            root.SetParent(generatedRoot, false);
            root.anchorMin = new Vector2(0.5f, 0.5f);
            root.anchorMax = new Vector2(0.5f, 0.5f);
            root.pivot = new Vector2(0.5f, 0.5f);

            root.sizeDelta = worldRect.size * scale;
            root.anchoredPosition =
                (worldRect.center - dungeonBounds.center) * scale;

            var north = CreateLine(
                root,
                "North",
                new Vector2(0f, 1f),
                new Vector2(1f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0f, lineThickness));

            var east = CreateLine(
                root,
                "East",
                new Vector2(1f, 0f),
                new Vector2(1f, 1f),
                new Vector2(1f, 0.5f),
                new Vector2(lineThickness, 0f));

            var south = CreateLine(
                root,
                "South",
                new Vector2(0f, 0f),
                new Vector2(1f, 0f),
                new Vector2(0.5f, 0f),
                new Vector2(0f, lineThickness));

            var west = CreateLine(
                root,
                "West",
                new Vector2(0f, 0f),
                new Vector2(0f, 1f),
                new Vector2(0f, 0.5f),
                new Vector2(lineThickness, 0f));

            rootObject.SetActive(false);

            return new RoomOutline(
                rootObject,
                new[] { north, east, south, west });
        }

        private static Image CreateLine(
            RectTransform parent,
            string lineName,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 pivot,
            Vector2 sizeDelta)
        {
            var lineObject = new GameObject(
                lineName,
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image))
            {
                layer = parent.gameObject.layer
            };

            var lineRect =
                lineObject.GetComponent<RectTransform>();

            lineRect.SetParent(parent, false);
            lineRect.anchorMin = anchorMin;
            lineRect.anchorMax = anchorMax;
            lineRect.pivot = pivot;
            lineRect.anchoredPosition = Vector2.zero;
            lineRect.sizeDelta = sizeDelta;

            var image = lineObject.GetComponent<Image>();
            image.color = Color.clear;
            image.raycastTarget = false;

            return image;
        }

        private void BuildAdjacency(List<Room> validRooms)
        {
            neighbors.Clear();

            foreach (var room in validRooms) neighbors.Add(room, new HashSet<Room>());

            for (var i = 0; i < validRooms.Count; i++)
            {
                var first = validRooms[i];

                for (var j = i + 1; j < validRooms.Count; j++)
                {
                    var second = validRooms[j];

                    if (!RoomsShareDoor(first, second)) continue;

                    neighbors[first].Add(second);
                    neighbors[second].Add(first);
                }
            }
        }

        private bool RoomsShareDoor(Room first, Room second)
        {
            var firstDirections =
                first.EnabledDirections();

            var secondDirections =
                second.EnabledDirections();

            var toleranceSquared =
                doorMatchTolerance * doorMatchTolerance;

            foreach (var direction in firstDirections)
            {
                var opposite = Opposite(direction);

                if (!secondDirections.Contains(opposite)) continue;

                var firstDoor =
                    first.GetDoorWorldPosition(direction);

                var secondDoor =
                    second.GetDoorWorldPosition(opposite);

                var deltaX = firstDoor.x - secondDoor.x;
                var deltaY = firstDoor.y - secondDoor.y;

                var distanceSquared =
                    deltaX * deltaX + deltaY * deltaY;

                if (distanceSquared <= toleranceSquared) return true;
            }

            return false;
        }

        private void RefreshColors()
        {
            var adjacentRooms =
                new HashSet<Room>();

            foreach (var exploredRoom in exploredRooms)
            {
                if (!neighbors.TryGetValue(
                        exploredRoom,
                        out var connectedRooms))
                    continue;

                foreach (var connectedRoom in connectedRooms)
                    if (!exploredRooms.Contains(connectedRoom))
                        adjacentRooms.Add(connectedRoom);
            }

            foreach (
                var entry
                in outlines)
            {
                var room = entry.Key;
                var outline = entry.Value;

                if (room == currentRoom)
                    outline.Apply(true, currentRoomColor);
                else if (exploredRooms.Contains(room))
                    outline.Apply(true, exploredRoomColor);
                else if (adjacentRooms.Contains(room))
                    outline.Apply(true, adjacentRoomColor);
                else
                    outline.Apply(false, Color.clear);
            }
        }

        private void EnsureGeneratedRoot()
        {
            if (generatedRoot != null || content == null) return;

            var rootObject = new GameObject(
                "Generated Room Outlines",
                typeof(RectTransform));

            rootObject.layer = content.gameObject.layer;

            generatedRoot =
                rootObject.GetComponent<RectTransform>();

            generatedRoot.SetParent(content, false);
            generatedRoot.anchorMin = Vector2.zero;
            generatedRoot.anchorMax = Vector2.one;
            generatedRoot.pivot = new Vector2(0.5f, 0.5f);
            generatedRoot.offsetMin = Vector2.zero;
            generatedRoot.offsetMax = Vector2.zero;
        }

        private void ClearGraphics()
        {
            if (generatedRoot != null)
                for (
                    var i = generatedRoot.childCount - 1;
                    i >= 0;
                    i--)
                {
                    var child =
                        generatedRoot.GetChild(i).gameObject;

                    child.SetActive(false);

                    if (Application.isPlaying)
                        Destroy(child);
                    else
                        DestroyImmediate(child);
                }

            outlines.Clear();
            neighbors.Clear();
        }

        private static Direction Opposite(Direction direction)
        {
            switch (direction)
            {
                case Direction.North:
                    return Direction.South;

                case Direction.East:
                    return Direction.West;

                case Direction.South:
                    return Direction.North;

                case Direction.West:
                    return Direction.East;

                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(direction),
                        direction,
                        null);
            }
        }

        private sealed class RoomOutline
        {
            private readonly Image[] lines;
            private readonly GameObject root;

            public RoomOutline(GameObject root, Image[] lines)
            {
                this.root = root;
                this.lines = lines;
            }

            public void Apply(bool visible, Color color)
            {
                root.SetActive(visible);

                if (!visible) return;

                foreach (var line in lines) line.color = color;
            }
        }
    }
}