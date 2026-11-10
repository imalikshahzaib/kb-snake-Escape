using ArrowMaze.Common;
using ArrowMaze.Gameplay.Board;
using ArrowMaze.LevelContent;

namespace ArrowMaze.Gameplay.Snakes
{
    public static class CollisionResolver
    {
        public static bool TryFindBlocker(BoardModel board, SnakeModel snake, out SnakeModel blocker, out GridCoord blockedCell)
        {
            SnakeMoveResult result = MoveValidator.Validate(board, snake);
            blocker = result.Blocker;
            blockedCell = result.BlockedCell;
            return result.Status == SnakeMoveStatus.Blocked && blocker != null;
        }

        public static bool TryFindObjectBlocker(BoardModel board, SnakeModel snake, out string blockerId, out string blockerType, out GridCoord blockedCell)
        {
            SnakeMoveResult result = MoveValidator.Validate(board, snake);
            blockerId = result.BlockerId;
            blockerType = result.BlockerType;
            blockedCell = result.BlockedCell;

            return result.Status == SnakeMoveStatus.Blocked &&
                   result.Blocker == null &&
                   BoardObjectTypes.BlocksMovement(result.BlockerType);
        }
    }
}
