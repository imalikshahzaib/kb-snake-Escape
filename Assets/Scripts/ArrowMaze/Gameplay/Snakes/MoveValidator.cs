using System.Collections.Generic;
using ArrowMaze.Common;
using ArrowMaze.Gameplay.Board;
using ArrowMaze.LevelContent;

namespace ArrowMaze.Gameplay.Snakes
{
    public static class MoveValidator
    {
        public static SnakeMoveResult Validate(BoardModel board, SnakeMoveCommand command)
        {
            if (board == null)
            {
                return SnakeMoveResult.Invalid(null, "Board is null.");
            }

            if (command == null || string.IsNullOrEmpty(command.SnakeId))
            {
                return SnakeMoveResult.Invalid(null, "Move command has no snake id.");
            }

            SnakeModel snake = board.FindSnake(command.SnakeId);
            return Validate(board, snake, command.ForceMove);
        }

        public static SnakeMoveResult Validate(BoardModel board, SnakeModel snake, bool forceMove = false)
        {
            if (board == null)
            {
                return SnakeMoveResult.Invalid(snake, "Board is null.");
            }

            if (snake == null)
            {
                return SnakeMoveResult.Invalid(null, "Snake is null.");
            }

            if (snake.HasEscaped)
            {
                return SnakeMoveResult.Invalid(snake, "Snake has already escaped.");
            }

            if (snake.Direction == GridDirection.None)
            {
                return SnakeMoveResult.Invalid(snake, "Snake has no valid direction.");
            }

            List<GridCoord> route = MovePlanner.BuildRouteToExit(board, snake);
            if (forceMove)
            {
                return SnakeMoveResult.CanEscape(snake, route);
            }

            for (int i = 0; i < route.Count; i++)
            {
                GridCoord cell = route[i];
                if (!board.Bounds.Contains(cell))
                {
                    return SnakeMoveResult.CanEscape(snake, route);
                }

                SnakeModel blocker;
                if (board.Occupancy.TryGetSnakeAt(cell, out blocker) && blocker != null && blocker.Id != snake.Id)
                {
                    return SnakeMoveResult.Blocked(snake, blocker, cell, route);
                }

                BoardObjectDefinition boardObject;
                if (board.Occupancy.TryGetObjectAt(cell, out boardObject) &&
                    boardObject != null &&
                    BoardObjectTypes.BlocksMovement(boardObject))
                {
                    return SnakeMoveResult.BlockedByObject(snake, boardObject.Id, boardObject.ObjectType, cell, route);
                }
            }

            return SnakeMoveResult.CanEscape(snake, route);
        }

        public static bool CanEscape(BoardModel board, SnakeModel snake)
        {
            return Validate(board, snake).Status == SnakeMoveStatus.CanEscape;
        }
    }
}
