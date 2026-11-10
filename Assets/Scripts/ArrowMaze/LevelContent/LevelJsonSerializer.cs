using System;
using System.Collections.Generic;
using ArrowMaze.Common;
using UnityEngine;

namespace ArrowMaze.LevelContent
{
    public static class LevelJsonSerializer
    {
        [Serializable]
        private sealed class CurrentLevelJson
        {
            public string Name;
            public int XSize;
            public int YSize;
            public int TimeLimit;
            public List<CurrentArrowJson> Arrows;
            public List<CurrentObjectJson> Objects;
            public List<CurrentWayBlockerJson> wayBlockers;
            public List<CurrentWayBlockerJson> WayBlockers;
        }

        [Serializable]
        private sealed class CurrentArrowJson
        {
            public int Dx;
            public int Dy;
            public int X;
            public int Y;
            public List<int> Indices;
        }

        [Serializable]
        private sealed class CurrentPositionJson
        {
            public int x;
            public int y;
        }

        [Serializable]
        private sealed class CurrentWayBlockerJson
        {
            public CurrentPositionJson position;
            public int lockTime;
            public int lockCount;
            public int size;
        }

        [Serializable]
        private sealed class CurrentObjectJson
        {
            public string Id;
            public string ObjectType;
            public string Type;
            public string GroupId;
            public int LockCount;
            public int LockTime;
            public List<int> Indices;
        }

        public static Result<LevelDefinition> FromCurrentJson(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return Result<LevelDefinition>.Failure("Level JSON is empty.");
            }

            CurrentLevelJson raw;
            try
            {
                raw = JsonUtility.FromJson<CurrentLevelJson>(json);
            }
            catch (Exception ex)
            {
                return Result<LevelDefinition>.Failure("Level JSON parse failed: " + ex.Message);
            }

            if (raw == null)
            {
                return Result<LevelDefinition>.Failure("Level JSON parse returned no data.");
            }

            var level = new LevelDefinition
            {
                Name = raw.Name,
                Width = raw.XSize,
                Height = raw.YSize,
                TimeLimitSeconds = raw.TimeLimit > 0 ? raw.TimeLimit : 0,
                SchemaVersion = 1,
                Mode = LevelMode.MainSaga
            };

            if (raw.Arrows != null)
            {
                for (int i = 0; i < raw.Arrows.Count; i++)
                {
                    CurrentArrowJson rawArrow = raw.Arrows[i];
                    if (rawArrow == null) continue;

                    var snake = new SnakeDefinition
                    {
                        Id = "snake_" + i.ToString("000"),
                        SourceIndex = i,
                        Direction = GridDirectionExtensions.FromDelta(rawArrow.Dx, rawArrow.Dy),
                        HasDeclaredHead = true,
                        DeclaredHead = new GridCoord(rawArrow.X, rawArrow.Y)
                    };

                    if (rawArrow.Indices != null)
                    {
                        foreach (int index in rawArrow.Indices)
                        {
                            snake.Cells.Add(GridCoord.FromIndex(index, raw.XSize));
                        }
                    }

                    level.Snakes.Add(snake);
                }
            }

            if (raw.Objects != null)
            {
                for (int i = 0; i < raw.Objects.Count; i++)
                {
                    CurrentObjectJson rawObject = raw.Objects[i];
                    if (rawObject == null) continue;

                    var boardObject = new BoardObjectDefinition
                    {
                        Id = !string.IsNullOrEmpty(rawObject.Id) ? rawObject.Id : "object_" + i.ToString("000"),
                        ObjectType = !string.IsNullOrEmpty(rawObject.ObjectType) ? rawObject.ObjectType : rawObject.Type,
                        GroupId = rawObject.GroupId,
                        LockCount = rawObject.LockCount,
                        LockTime = rawObject.LockTime
                    };

                    if (string.IsNullOrEmpty(boardObject.ObjectType))
                    {
                        boardObject.ObjectType = BoardObjectTypes.StaticWall;
                    }

                    if (boardObject.ObjectType == BoardObjectTypes.WayBlocker && boardObject.LockCount <= 0 && boardObject.LockTime > 0)
                    {
                        boardObject.LockCount = boardObject.LockTime;
                    }

                    if (rawObject.Indices != null)
                    {
                        foreach (int index in rawObject.Indices)
                        {
                            boardObject.Cells.Add(GridCoord.FromIndex(index, raw.XSize));
                        }
                    }

                    level.Objects.Add(boardObject);
                }
            }

            ImportWayBlockers(raw.wayBlockers, level);
            ImportWayBlockers(raw.WayBlockers, level);

            return Result<LevelDefinition>.Success(level);
        }

        private static void ImportWayBlockers(List<CurrentWayBlockerJson> rawWayBlockers, LevelDefinition level)
        {
            if (rawWayBlockers == null || level == null) return;

            for (int i = 0; i < rawWayBlockers.Count; i++)
            {
                CurrentWayBlockerJson rawBlocker = rawWayBlockers[i];
                if (rawBlocker == null || rawBlocker.position == null) continue;

                int lockCount = rawBlocker.lockCount > 0 ? rawBlocker.lockCount : rawBlocker.lockTime;
                var boardObject = new BoardObjectDefinition
                {
                    Id = "way_blocker_" + i.ToString("000"),
                    ObjectType = BoardObjectTypes.WayBlocker,
                    LockCount = lockCount,
                    LockTime = rawBlocker.lockTime
                };
                boardObject.Cells.Add(new GridCoord(rawBlocker.position.x, rawBlocker.position.y));
                level.Objects.Add(boardObject);
            }
        }

        public static string ToCurrentJson(LevelDefinition level, bool prettyPrint = true)
        {
            var raw = new CurrentLevelJson
            {
                Name = level != null ? level.Name : string.Empty,
                XSize = level != null ? level.Width : 0,
                YSize = level != null ? level.Height : 0,
                TimeLimit = level != null ? level.TimeLimitSeconds : 0,
                Arrows = new List<CurrentArrowJson>(),
                Objects = new List<CurrentObjectJson>()
            };

            if (level == null)
            {
                return JsonUtility.ToJson(raw, prettyPrint);
            }

            if (level.Snakes != null)
            {
                for (int i = 0; i < level.Snakes.Count; i++)
                {
                    SnakeDefinition snake = level.Snakes[i];
                    if (snake == null || snake.Cells == null || snake.Cells.Count == 0) continue;

                    GridCoord delta = snake.Direction.ToDelta();
                    var rawArrow = new CurrentArrowJson
                    {
                        Dx = delta.X,
                        Dy = delta.Y,
                        X = snake.Head.X,
                        Y = snake.Head.Y,
                        Indices = new List<int>()
                    };

                    for (int cellIndex = 0; cellIndex < snake.Cells.Count; cellIndex++)
                    {
                        rawArrow.Indices.Add(snake.Cells[cellIndex].ToIndex(level.Width));
                    }

                    raw.Arrows.Add(rawArrow);
                }
            }

            if (level.Objects != null)
            {
                for (int i = 0; i < level.Objects.Count; i++)
                {
                    BoardObjectDefinition boardObject = level.Objects[i];
                    if (boardObject == null || boardObject.Cells == null || boardObject.Cells.Count == 0) continue;

                    var rawObject = new CurrentObjectJson
                    {
                        Id = boardObject.Id,
                        ObjectType = string.IsNullOrEmpty(boardObject.ObjectType) ? BoardObjectTypes.StaticWall : boardObject.ObjectType,
                        GroupId = boardObject.GroupId,
                        LockCount = boardObject.LockCount,
                        LockTime = boardObject.LockTime > 0 ? boardObject.LockTime : boardObject.LockCount,
                        Indices = new List<int>()
                    };

                    for (int cellIndex = 0; cellIndex < boardObject.Cells.Count; cellIndex++)
                    {
                        rawObject.Indices.Add(boardObject.Cells[cellIndex].ToIndex(level.Width));
                    }

                    raw.Objects.Add(rawObject);
                }
            }

            return JsonUtility.ToJson(raw, prettyPrint);
        }
    }
}
