using System.Collections.Generic;
using ArrowMaze.Common;
using ArrowMaze.Gameplay.Snakes;
using ArrowMaze.LevelContent;

namespace ArrowMaze.Gameplay.Board
{
    public static class BoardFactory
    {
        public static Result<BoardModel> Create(LevelDefinition level)
        {
            LevelValidationResult validation = LevelValidator.Validate(level);
            if (!validation.IsValid)
            {
                return Result<BoardModel>.Failure("Level validation failed with " + validation.ErrorCount + " error(s).");
            }

            var snakes = new List<SnakeModel>();
            for (int i = 0; i < level.Snakes.Count; i++)
            {
                snakes.Add(SnakeModel.FromDefinition(level.Snakes[i]));
            }

            var board = new BoardModel(new BoardBounds(level.Width, level.Height), snakes, level.Objects);
            return Result<BoardModel>.Success(board);
        }
    }
}
