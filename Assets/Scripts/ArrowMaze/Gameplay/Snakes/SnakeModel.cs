using System.Collections.Generic;
using ArrowMaze.Common;
using ArrowMaze.LevelContent;

namespace ArrowMaze.Gameplay.Snakes
{
    public sealed class SnakeModel
    {
        private readonly List<GridCoord> cells = new List<GridCoord>();

        public string Id { get; private set; }
        public int SourceIndex { get; private set; }
        public GridDirection Direction { get; private set; }
        public bool HasEscaped { get; private set; }

        public IReadOnlyList<GridCoord> Cells
        {
            get { return cells; }
        }

        public GridCoord Head
        {
            get { return cells.Count > 0 ? cells[0] : new GridCoord(0, 0); }
        }

        public SnakeModel(string id, int sourceIndex, GridDirection direction, IEnumerable<GridCoord> sourceCells)
        {
            Id = string.IsNullOrEmpty(id) ? "snake_" + sourceIndex.ToString("000") : id;
            SourceIndex = sourceIndex;
            Direction = direction;
            HasEscaped = false;

            if (sourceCells == null) return;
            cells.AddRange(sourceCells);
        }

        public static SnakeModel FromDefinition(SnakeDefinition definition)
        {
            if (definition == null)
            {
                return new SnakeModel("snake_invalid", -1, GridDirection.None, new List<GridCoord>());
            }

            return new SnakeModel(definition.Id, definition.SourceIndex, definition.Direction, definition.Cells);
        }

        public void MarkEscaped()
        {
            HasEscaped = true;
        }
    }
}
