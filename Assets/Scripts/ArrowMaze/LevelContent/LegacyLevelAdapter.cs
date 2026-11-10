using ArrowMaze.Common;
using ArrowMaze.Data;

namespace ArrowMaze.LevelContent
{
    public static class LegacyLevelAdapter
    {
        public static LevelDefinition FromLevelData(LevelData source, string fallbackName = null)
        {
            var level = new LevelDefinition
            {
                Name = source != null && !string.IsNullOrEmpty(source.name) ? source.name : fallbackName,
                Width = source != null ? source.width : 0,
                Height = source != null ? source.height : 0,
                SchemaVersion = 1,
                Mode = LevelMode.MainSaga
            };

            if (source == null || source.arrows == null)
            {
                return level;
            }

            for (int i = 0; i < source.arrows.Count; i++)
            {
                ArrowData arrow = source.arrows[i];
                if (arrow == null) continue;

                var snake = new SnakeDefinition
                {
                    Id = "snake_" + i.ToString("000"),
                    SourceIndex = i,
                    Direction = FromLegacyDirection(arrow.direction),
                    HasDeclaredHead = arrow.bodyPositions != null && arrow.bodyPositions.Count > 0
                };

                if (arrow.bodyPositions != null)
                {
                    for (int cellIndex = 0; cellIndex < arrow.bodyPositions.Count; cellIndex++)
                    {
                        var pos = arrow.bodyPositions[cellIndex];
                        var coord = new GridCoord(pos.x, pos.y);
                        if (cellIndex == 0)
                        {
                            snake.DeclaredHead = coord;
                        }

                        snake.Cells.Add(coord);
                    }
                }

                level.Snakes.Add(snake);
            }

            if (source.objects != null)
            {
                for (int i = 0; i < source.objects.Count; i++)
                {
                    BoardObjectData sourceObject = source.objects[i];
                    if (sourceObject == null) continue;

                    var boardObject = new BoardObjectDefinition
                    {
                        Id = !string.IsNullOrEmpty(sourceObject.id) ? sourceObject.id : "object_" + i.ToString("000"),
                        ObjectType = !string.IsNullOrEmpty(sourceObject.objectType) ? sourceObject.objectType : BoardObjectTypes.StaticWall,
                        GroupId = sourceObject.groupId,
                        LockCount = sourceObject.lockCount,
                        LockTime = sourceObject.lockTime
                    };

                    if (boardObject.ObjectType == BoardObjectTypes.WayBlocker && boardObject.LockCount <= 0 && boardObject.LockTime > 0)
                    {
                        boardObject.LockCount = boardObject.LockTime;
                    }

                    if (sourceObject.cells != null)
                    {
                        for (int cellIndex = 0; cellIndex < sourceObject.cells.Count; cellIndex++)
                        {
                            var pos = sourceObject.cells[cellIndex];
                            boardObject.Cells.Add(new GridCoord(pos.x, pos.y));
                        }
                    }

                    level.Objects.Add(boardObject);
                }
            }

            return level;
        }

        private static GridDirection FromLegacyDirection(MoveDirection direction)
        {
            switch (direction)
            {
                case MoveDirection.Up:
                    return GridDirection.Up;
                case MoveDirection.Down:
                    return GridDirection.Down;
                case MoveDirection.Left:
                    return GridDirection.Left;
                case MoveDirection.Right:
                    return GridDirection.Right;
                default:
                    return GridDirection.None;
            }
        }
    }
}
