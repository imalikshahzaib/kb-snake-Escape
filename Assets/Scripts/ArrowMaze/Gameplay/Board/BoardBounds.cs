using ArrowMaze.Common;

namespace ArrowMaze.Gameplay.Board
{
    public struct BoardBounds
    {
        public int Width { get; private set; }
        public int Height { get; private set; }

        public BoardBounds(int width, int height)
        {
            Width = width;
            Height = height;
        }

        public bool Contains(GridCoord coord)
        {
            return coord.X >= 0 && coord.X < Width && coord.Y >= 0 && coord.Y < Height;
        }
    }
}
