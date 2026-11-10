namespace ArrowMaze.Common
{
    public enum GridDirection
    {
        None = 0,
        Up = 1,
        Down = 2,
        Left = 3,
        Right = 4
    }

    public static class GridDirectionExtensions
    {
        public static GridCoord ToDelta(this GridDirection direction)
        {
            switch (direction)
            {
                case GridDirection.Up:
                    return new GridCoord(0, 1);
                case GridDirection.Down:
                    return new GridCoord(0, -1);
                case GridDirection.Left:
                    return new GridCoord(-1, 0);
                case GridDirection.Right:
                    return new GridCoord(1, 0);
                default:
                    return new GridCoord(0, 0);
            }
        }

        public static GridDirection FromDelta(int dx, int dy)
        {
            if (dx == 1 && dy == 0) return GridDirection.Right;
            if (dx == -1 && dy == 0) return GridDirection.Left;
            if (dx == 0 && dy == 1) return GridDirection.Up;
            if (dx == 0 && dy == -1) return GridDirection.Down;
            return GridDirection.None;
        }
    }
}
