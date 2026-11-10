using UnityEngine;
using System.Collections.Generic;
using ArrowMaze.Data;
using ArrowMaze.LevelContent;

namespace ArrowMaze
{
    [System.Serializable]
    public class RawArrow
    {
        public int Dx;
        public int Dy;
        public int X;
        public int Y;
        public List<int> Indices;
    }

    [System.Serializable]
    public class RawBoardObject
    {
        public string Id;
        public string ObjectType;
        public string Type;
        public List<int> Indices;
        public string GroupId;
        public int LockCount;
        public int LockTime;
    }

    [System.Serializable]
    public class RawGridPosition
    {
        public int x;
        public int y;
    }

    [System.Serializable]
    public class RawWayBlocker
    {
        public RawGridPosition position;
        public int lockTime;
        public int lockCount;
        public int size;
    }

    [System.Serializable]
    public class RawLevel
    {
        public string Name;
        public int XSize;
        public int YSize;
        public List<RawArrow> Arrows;
        public List<RawBoardObject> Objects;
        public List<RawWayBlocker> wayBlockers;
        public List<RawWayBlocker> WayBlockers;
    }

    public class LevelImporter : MonoBehaviour
    {
        public static LevelData ImportFromJson(string json)
        {
            LevelData data = ScriptableObject.CreateInstance<LevelData>();
            if (string.IsNullOrWhiteSpace(json))
            {
                data.arrows = new List<ArrowData>();
                data.objects = new List<BoardObjectData>();
                return data;
            }

            RawLevel raw = JsonUtility.FromJson<RawLevel>(json);
            if (raw == null)
            {
                data.arrows = new List<ArrowData>();
                data.objects = new List<BoardObjectData>();
                return data;
            }

            data.name = raw.Name;
            data.width = raw.XSize;
            data.height = raw.YSize;
            data.arrows = new List<ArrowData>();
            data.objects = new List<BoardObjectData>();

            if (raw.Arrows != null)
            {
                foreach (var rawArrow in raw.Arrows)
                {
                    if (rawArrow == null || rawArrow.Indices == null) continue;

                    ArrowData arrow = new ArrowData();
                    arrow.direction = GetDirection(rawArrow.Dx, rawArrow.Dy);
                    arrow.bodyPositions = new List<Vector2Int>();

                    // Convert indices to Vector2Int assuming indices = Y * XSize + X
                    foreach (int index in rawArrow.Indices)
                    {
                        int x = index % raw.XSize;
                        int y = index / raw.XSize;
                        arrow.bodyPositions.Add(new Vector2Int(x, y));
                    }
                    data.arrows.Add(arrow);
                }
            }

            if (raw.Objects != null)
            {
                foreach (var rawObject in raw.Objects)
                {
                    if (rawObject == null || rawObject.Indices == null) continue;

                    BoardObjectData objectData = new BoardObjectData
                    {
                        id = rawObject.Id,
                        objectType = !string.IsNullOrEmpty(rawObject.ObjectType) ? rawObject.ObjectType : rawObject.Type,
                        groupId = rawObject.GroupId,
                        lockCount = rawObject.LockCount,
                        lockTime = rawObject.LockTime,
                        cells = new List<Vector2Int>()
                    };

                    if (string.IsNullOrEmpty(objectData.objectType))
                    {
                        objectData.objectType = BoardObjectTypes.StaticWall;
                    }

                    if (objectData.objectType == BoardObjectTypes.WayBlocker && objectData.lockCount <= 0 && objectData.lockTime > 0)
                    {
                        objectData.lockCount = objectData.lockTime;
                    }

                    foreach (int index in rawObject.Indices)
                    {
                        int x = index % raw.XSize;
                        int y = index / raw.XSize;
                        objectData.cells.Add(new Vector2Int(x, y));
                    }

                    data.objects.Add(objectData);
                }
            }

            ImportWayBlockers(raw.wayBlockers, data.objects);
            ImportWayBlockers(raw.WayBlockers, data.objects);

            return data;
        }

        public static string ExportToJson(LevelData level, bool prettyPrint = true)
        {
            return JsonUtility.ToJson(ExportToRawLevel(level), prettyPrint);
        }

        public static RawLevel ExportToRawLevel(LevelData level)
        {
            RawLevel raw = new RawLevel
            {
                Name = level != null ? level.name : string.Empty,
                XSize = level != null ? level.width : 0,
                YSize = level != null ? level.height : 0,
                Arrows = new List<RawArrow>(),
                Objects = new List<RawBoardObject>()
            };

            if (level == null) return raw;

            if (level.arrows != null)
            {
                foreach (var arrowData in level.arrows)
                {
                    if (arrowData == null || arrowData.bodyPositions == null || arrowData.bodyPositions.Count == 0)
                    {
                        continue;
                    }

                    Vector2Int delta = GetDirectionDelta(arrowData.direction);
                    RawArrow rawArrow = new RawArrow
                    {
                        Dx = delta.x,
                        Dy = delta.y,
                        X = arrowData.bodyPositions[0].x,
                        Y = arrowData.bodyPositions[0].y,
                        Indices = new List<int>()
                    };

                    foreach (var pos in arrowData.bodyPositions)
                    {
                        rawArrow.Indices.Add(pos.y * level.width + pos.x);
                    }

                    raw.Arrows.Add(rawArrow);
                }
            }

            if (level.objects != null)
            {
                foreach (var objectData in level.objects)
                {
                    if (objectData == null || objectData.cells == null || objectData.cells.Count == 0)
                    {
                        continue;
                    }

                    RawBoardObject rawObject = new RawBoardObject
                    {
                        Id = objectData.id,
                        ObjectType = string.IsNullOrEmpty(objectData.objectType) ? BoardObjectTypes.StaticWall : objectData.objectType,
                        GroupId = objectData.groupId,
                        LockCount = objectData.lockCount,
                        LockTime = objectData.lockTime > 0 ? objectData.lockTime : objectData.lockCount,
                        Indices = new List<int>()
                    };

                    foreach (Vector2Int pos in objectData.cells)
                    {
                        rawObject.Indices.Add(pos.y * level.width + pos.x);
                    }

                    raw.Objects.Add(rawObject);
                }
            }

            return raw;
        }

        private static void ImportWayBlockers(List<RawWayBlocker> rawWayBlockers, List<BoardObjectData> targetObjects)
        {
            if (rawWayBlockers == null || targetObjects == null) return;

            for (int i = 0; i < rawWayBlockers.Count; i++)
            {
                RawWayBlocker rawBlocker = rawWayBlockers[i];
                if (rawBlocker == null || rawBlocker.position == null) continue;

                int lockCount = rawBlocker.lockCount > 0 ? rawBlocker.lockCount : rawBlocker.lockTime;
                BoardObjectData objectData = new BoardObjectData
                {
                    id = "way_blocker_" + i.ToString("000"),
                    objectType = BoardObjectTypes.WayBlocker,
                    lockCount = lockCount,
                    lockTime = rawBlocker.lockTime,
                    cells = new List<Vector2Int>
                    {
                        new Vector2Int(rawBlocker.position.x, rawBlocker.position.y)
                    }
                };

                targetObjects.Add(objectData);
            }
        }

        private static MoveDirection GetDirection(int dx, int dy)
        {
            if (dx == 1) return MoveDirection.Right;
            if (dx == -1) return MoveDirection.Left;
            if (dy == 1) return MoveDirection.Up;
            if (dy == -1) return MoveDirection.Down;
            return MoveDirection.Up;
        }

        private static Vector2Int GetDirectionDelta(MoveDirection direction)
        {
            switch (direction)
            {
                case MoveDirection.Right: return Vector2Int.right;
                case MoveDirection.Left: return Vector2Int.left;
                case MoveDirection.Down: return Vector2Int.down;
                case MoveDirection.Up:
                default:
                    return Vector2Int.up;
            }
        }
    }
}
