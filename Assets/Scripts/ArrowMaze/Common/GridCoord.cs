using System;

namespace ArrowMaze.Common
{
    [Serializable]
    public struct GridCoord : IEquatable<GridCoord>
    {
        public int X;
        public int Y;

        public GridCoord(int x, int y)
        {
            X = x;
            Y = y;
        }

        public static GridCoord FromIndex(int index, int width)
        {
            if (width <= 0) return new GridCoord(index, 0);
            return new GridCoord(index % width, index / width);
        }

        public int ToIndex(int width)
        {
            return Y * width + X;
        }

        public int ManhattanDistance(GridCoord other)
        {
            return Math.Abs(X - other.X) + Math.Abs(Y - other.Y);
        }

        public bool Equals(GridCoord other)
        {
            return X == other.X && Y == other.Y;
        }

        public override bool Equals(object obj)
        {
            return obj is GridCoord other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (X * 397) ^ Y;
            }
        }

        public override string ToString()
        {
            return X + "," + Y;
        }

        public static GridCoord operator +(GridCoord left, GridCoord right)
        {
            return new GridCoord(left.X + right.X, left.Y + right.Y);
        }

        public static GridCoord operator *(GridCoord coord, int amount)
        {
            return new GridCoord(coord.X * amount, coord.Y * amount);
        }
    }
}
