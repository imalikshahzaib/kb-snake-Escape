using System.Collections.Generic;
using ArrowMaze.Gameplay.Board;
using ArrowMaze.Gameplay.Snakes;

namespace ArrowMaze.LevelContent
{
    public sealed class LevelSolvabilityResult
    {
        public bool IsSolvable;
        public string Error;
        public List<string> SolutionSnakeIds = new List<string>();
        public List<string> StuckSnakeIds = new List<string>();
        public int EscapedCount;
        public int TotalSnakeCount;
    }

    public static class LevelSolvabilityChecker
    {
        public static LevelSolvabilityResult Check(LevelDefinition level)
        {
            var result = new LevelSolvabilityResult();
            if (level == null)
            {
                result.Error = "Level is null.";
                return result;
            }

            var boardResult = BoardFactory.Create(level);
            if (!boardResult.IsSuccess)
            {
                result.Error = boardResult.Error;
                return result;
            }

            BoardModel board = boardResult.Value;
            result.TotalSnakeCount = board.Snakes.Count;

            bool moved;
            int guard = board.Snakes.Count + 1;
            do
            {
                moved = false;
                for (int i = 0; i < board.Snakes.Count; i++)
                {
                    SnakeModel snake = board.Snakes[i];
                    if (snake == null || snake.HasEscaped) continue;

                    SnakeMoveResult move = MoveValidator.Validate(board, snake);
                    if (move.Status != SnakeMoveStatus.CanEscape) continue;

                    SnakeEscapeService.TryEscape(board, snake);
                    board.TickWayBlockersAfterSnakeEscape();
                    result.SolutionSnakeIds.Add(snake.Id);
                    result.EscapedCount++;
                    moved = true;
                    break;
                }

                guard--;
            }
            while (moved && guard > 0);

            for (int i = 0; i < board.Snakes.Count; i++)
            {
                SnakeModel snake = board.Snakes[i];
                if (snake != null && !snake.HasEscaped)
                {
                    result.StuckSnakeIds.Add(snake.Id);
                }
            }

            result.IsSolvable = result.StuckSnakeIds.Count == 0 && result.EscapedCount == result.TotalSnakeCount;
            if (!result.IsSolvable && string.IsNullOrEmpty(result.Error))
            {
                result.Error = "No legal move sequence cleared every snake.";
            }

            return result;
        }
    }
}
