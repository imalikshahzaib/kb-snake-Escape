using System.Collections.Generic;
using ArrowMaze.Common;

namespace ArrowMaze.Gameplay.Snakes
{
    public enum SnakeMoveStatus
    {
        Invalid = 0,
        Blocked = 1,
        CanEscape = 2,
        Escaped = 3
    }

    public sealed class SnakeMoveResult
    {
        public SnakeMoveStatus Status { get; private set; }
        public SnakeModel Snake { get; private set; }
        public SnakeModel Blocker { get; private set; }
        public string BlockerId { get; private set; }
        public string BlockerType { get; private set; }
        public GridCoord BlockedCell { get; private set; }
        public List<GridCoord> Route { get; private set; }
        public string Message { get; private set; }

        private SnakeMoveResult(
            SnakeMoveStatus status,
            SnakeModel snake,
            SnakeModel blocker,
            string blockerId,
            string blockerType,
            GridCoord blockedCell,
            List<GridCoord> route,
            string message)
        {
            Status = status;
            Snake = snake;
            Blocker = blocker;
            BlockerId = blockerId ?? string.Empty;
            BlockerType = blockerType ?? string.Empty;
            BlockedCell = blockedCell;
            Route = route ?? new List<GridCoord>();
            Message = message ?? string.Empty;
        }

        public static SnakeMoveResult Invalid(SnakeModel snake, string message)
        {
            return new SnakeMoveResult(SnakeMoveStatus.Invalid, snake, null, string.Empty, string.Empty, new GridCoord(0, 0), null, message);
        }

        public static SnakeMoveResult Blocked(SnakeModel snake, SnakeModel blocker, GridCoord blockedCell, List<GridCoord> route)
        {
            string blockerId = blocker != null ? blocker.Id : string.Empty;
            return new SnakeMoveResult(SnakeMoveStatus.Blocked, snake, blocker, blockerId, "Snake", blockedCell, route, "Blocked at " + blockedCell + ".");
        }

        public static SnakeMoveResult BlockedByObject(SnakeModel snake, string blockerId, string blockerType, GridCoord blockedCell, List<GridCoord> route)
        {
            string type = string.IsNullOrEmpty(blockerType) ? "Object" : blockerType;
            return new SnakeMoveResult(SnakeMoveStatus.Blocked, snake, null, blockerId, type, blockedCell, route, "Blocked by " + type + " at " + blockedCell + ".");
        }

        public static SnakeMoveResult CanEscape(SnakeModel snake, List<GridCoord> route)
        {
            return new SnakeMoveResult(SnakeMoveStatus.CanEscape, snake, null, string.Empty, string.Empty, new GridCoord(0, 0), route, string.Empty);
        }

        public static SnakeMoveResult Escaped(SnakeModel snake, List<GridCoord> route)
        {
            return new SnakeMoveResult(SnakeMoveStatus.Escaped, snake, null, string.Empty, string.Empty, new GridCoord(0, 0), route, string.Empty);
        }
    }
}
