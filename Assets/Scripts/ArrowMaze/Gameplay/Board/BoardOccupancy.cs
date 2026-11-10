using System.Collections.Generic;
using ArrowMaze.Common;
using ArrowMaze.Gameplay.Snakes;
using ArrowMaze.LevelContent;

namespace ArrowMaze.Gameplay.Board
{
    public sealed class BoardOccupancy
    {
        private readonly Dictionary<GridCoord, SnakeModel> snakeByCell = new Dictionary<GridCoord, SnakeModel>();
        private readonly Dictionary<GridCoord, BoardObjectDefinition> objectByCell = new Dictionary<GridCoord, BoardObjectDefinition>();

        public int OccupiedCellCount
        {
            get { return snakeByCell.Count + objectByCell.Count; }
        }

        public int SnakeOccupiedCellCount
        {
            get { return snakeByCell.Count; }
        }

        public int ObjectOccupiedCellCount
        {
            get { return objectByCell.Count; }
        }

        public bool TryGetSnakeAt(GridCoord coord, out SnakeModel snake)
        {
            return snakeByCell.TryGetValue(coord, out snake);
        }

        public bool TryGetObjectAt(GridCoord coord, out BoardObjectDefinition boardObject)
        {
            return objectByCell.TryGetValue(coord, out boardObject);
        }

        public bool IsOccupiedByOther(GridCoord coord, SnakeModel movingSnake)
        {
            SnakeModel occupant;
            if (!snakeByCell.TryGetValue(coord, out occupant))
            {
                return false;
            }

            return occupant != null && movingSnake != null && occupant.Id != movingSnake.Id;
        }

        public bool IsBlockedByObject(GridCoord coord)
        {
            BoardObjectDefinition boardObject;
            return objectByCell.TryGetValue(coord, out boardObject) &&
                   boardObject != null &&
                   BoardObjectTypes.BlocksMovement(boardObject);
        }

        public void RegisterSnake(SnakeModel snake)
        {
            if (snake == null || snake.Cells == null) return;

            foreach (GridCoord cell in snake.Cells)
            {
                snakeByCell[cell] = snake;
            }
        }

        public void RegisterObject(BoardObjectDefinition boardObject)
        {
            if (boardObject == null || boardObject.Cells == null) return;

            foreach (GridCoord cell in boardObject.Cells)
            {
                objectByCell[cell] = boardObject;
            }
        }

        public void RemoveObject(BoardObjectDefinition boardObject)
        {
            if (boardObject == null) return;

            var keysToRemove = new List<GridCoord>();
            foreach (KeyValuePair<GridCoord, BoardObjectDefinition> entry in objectByCell)
            {
                if (entry.Value != null && entry.Value.Id == boardObject.Id)
                {
                    keysToRemove.Add(entry.Key);
                }
            }

            foreach (GridCoord key in keysToRemove)
            {
                objectByCell.Remove(key);
            }
        }

        public void RemoveSnake(SnakeModel snake)
        {
            if (snake == null) return;

            var keysToRemove = new List<GridCoord>();
            foreach (KeyValuePair<GridCoord, SnakeModel> entry in snakeByCell)
            {
                if (entry.Value != null && entry.Value.Id == snake.Id)
                {
                    keysToRemove.Add(entry.Key);
                }
            }

            foreach (GridCoord key in keysToRemove)
            {
                snakeByCell.Remove(key);
            }
        }

        public void Clear()
        {
            snakeByCell.Clear();
            objectByCell.Clear();
        }

        public Dictionary<GridCoord, string> Snapshot()
        {
            var snapshot = new Dictionary<GridCoord, string>();
            foreach (KeyValuePair<GridCoord, SnakeModel> entry in snakeByCell)
            {
                snapshot[entry.Key] = entry.Value != null ? entry.Value.Id : string.Empty;
            }

            foreach (KeyValuePair<GridCoord, BoardObjectDefinition> entry in objectByCell)
            {
                snapshot[entry.Key] = entry.Value != null ? entry.Value.Id : string.Empty;
            }

            return snapshot;
        }
    }
}
