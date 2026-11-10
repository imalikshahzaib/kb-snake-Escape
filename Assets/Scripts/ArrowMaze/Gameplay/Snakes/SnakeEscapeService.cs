using ArrowMaze.Gameplay.Board;

namespace ArrowMaze.Gameplay.Snakes
{
    public static class SnakeEscapeService
    {
        public static SnakeMoveResult TryEscape(BoardModel board, SnakeModel snake, bool forceMove = false)
        {
            SnakeMoveResult validation = MoveValidator.Validate(board, snake, forceMove);
            if (validation.Status != SnakeMoveStatus.CanEscape)
            {
                return validation;
            }

            board.MarkEscaped(snake);
            return SnakeMoveResult.Escaped(snake, validation.Route);
        }
    }
}
