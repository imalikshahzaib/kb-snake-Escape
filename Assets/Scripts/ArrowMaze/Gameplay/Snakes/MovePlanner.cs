using System.Collections.Generic;
using ArrowMaze.Common;
using ArrowMaze.Gameplay.Board;

namespace ArrowMaze.Gameplay.Snakes
{
    public static class MovePlanner
    {
        public static List<GridCoord> BuildRouteToExit(BoardModel board, SnakeModel snake)
        {
            var route = new List<GridCoord>();
            if (board == null || snake == null || snake.Direction == GridDirection.None)
            {
                return route;
            }

            GridCoord delta = snake.Direction.ToDelta();
            GridCoord check = snake.Head + delta;

            while (board.Bounds.Contains(check))
            {
                route.Add(check);
                check += delta;
            }

            route.Add(check);
            return route;
        }
    }
}
