using System;
using System.Collections.Generic;
using ArrowMaze.Common;

namespace ArrowMaze.LevelContent
{
    [Serializable]
    public sealed class SnakeDefinition
    {
        public string Id;
        public int SourceIndex;
        public GridDirection Direction;
        public List<GridCoord> Cells = new List<GridCoord>();
        public bool HasDeclaredHead;
        public GridCoord DeclaredHead;

        public GridCoord Head
        {
            get { return Cells != null && Cells.Count > 0 ? Cells[0] : new GridCoord(0, 0); }
        }
    }
}
