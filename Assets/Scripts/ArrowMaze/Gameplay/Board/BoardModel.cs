using System.Collections.Generic;
using ArrowMaze.Gameplay.Snakes;
using ArrowMaze.LevelContent;

namespace ArrowMaze.Gameplay.Board
{
    public sealed class BoardModel
    {
        private readonly List<SnakeModel> snakes = new List<SnakeModel>();
        private readonly List<BoardObjectDefinition> objects = new List<BoardObjectDefinition>();

        public BoardBounds Bounds { get; private set; }
        public BoardOccupancy Occupancy { get; private set; }

        public IReadOnlyList<SnakeModel> Snakes
        {
            get { return snakes; }
        }

        public BoardModel(BoardBounds bounds, IEnumerable<SnakeModel> sourceSnakes)
            : this(bounds, sourceSnakes, null)
        {
        }

        public IReadOnlyList<BoardObjectDefinition> Objects
        {
            get { return objects; }
        }

        public BoardModel(BoardBounds bounds, IEnumerable<SnakeModel> sourceSnakes, IEnumerable<BoardObjectDefinition> sourceObjects)
        {
            Bounds = bounds;
            Occupancy = new BoardOccupancy();

            if (sourceObjects != null)
            {
                foreach (BoardObjectDefinition boardObject in sourceObjects)
                {
                    if (boardObject == null) continue;
                    BoardObjectTypes.EnsureRuntimeLockCount(boardObject);
                    objects.Add(boardObject);
                    if (BoardObjectTypes.BlocksMovement(boardObject))
                    {
                        Occupancy.RegisterObject(boardObject);
                    }
                }
            }

            if (sourceSnakes == null) return;

            foreach (SnakeModel snake in sourceSnakes)
            {
                if (snake == null) continue;
                snakes.Add(snake);
                Occupancy.RegisterSnake(snake);
            }
        }

        public SnakeModel FindSnake(string snakeId)
        {
            if (string.IsNullOrEmpty(snakeId)) return null;

            for (int i = 0; i < snakes.Count; i++)
            {
                if (snakes[i] != null && snakes[i].Id == snakeId)
                {
                    return snakes[i];
                }
            }

            return null;
        }

        public void MarkEscaped(SnakeModel snake)
        {
            if (snake == null || snake.HasEscaped) return;
            Occupancy.RemoveSnake(snake);
            snake.MarkEscaped();
        }

        public int RemainingSnakeCount()
        {
            int count = 0;
            for (int i = 0; i < snakes.Count; i++)
            {
                if (snakes[i] != null && !snakes[i].HasEscaped)
                {
                    count++;
                }
            }

            return count;
        }

        public List<BoardObjectDefinition> TickWayBlockersAfterSnakeEscape()
        {
            var changed = new List<BoardObjectDefinition>();

            for (int i = objects.Count - 1; i >= 0; i--)
            {
                BoardObjectDefinition boardObject = objects[i];
                if (boardObject == null || boardObject.ObjectType != BoardObjectTypes.WayBlocker)
                {
                    continue;
                }

                BoardObjectTypes.EnsureRuntimeLockCount(boardObject);
                if (boardObject.LockCount <= 0)
                {
                    continue;
                }

                boardObject.LockCount--;
                changed.Add(boardObject);

                if (boardObject.LockCount <= 0)
                {
                    Occupancy.RemoveObject(boardObject);
                    objects.RemoveAt(i);
                }
            }

            return changed;
        }
    }
}
