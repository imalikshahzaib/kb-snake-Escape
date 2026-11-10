using System.Collections.Generic;
using ArrowMaze.Common;
using ArrowMaze.Gameplay.Board;
using ArrowMaze.Gameplay.Snakes;
using ArrowMaze.LevelContent;
using UnityEditor;
using UnityEngine;

namespace ArrowMaze.Editor
{
    public static class MoveValidatorTestMenu
    {
        [MenuItem("ArrowMaze/Run MoveValidator Smoke Tests")]
        public static void RunMoveValidatorSmokeTests()
        {
            int failures = 0;

            if (!ClearRouteCanEscape()) failures++;
            if (!BlockedRouteFailsWithBlocker()) failures++;
            if (!StaticWallBlocksRoute()) failures++;
            if (!WayBlockerUnlocksAfterEscapeCount()) failures++;
            if (!EscapedSnakeIsRemovedFromOccupancy()) failures++;

            if (failures == 0)
            {
                Debug.Log("MoveValidator smoke tests passed: clear route, snake blocker, static wall, way blocker unlock, and occupancy removal.");
            }
            else
            {
                Debug.LogError("MoveValidator smoke tests failed: " + failures + " failure(s).");
            }
        }

        private static bool ClearRouteCanEscape()
        {
            SnakeModel snake = new SnakeModel(
                "moving_clear",
                0,
                GridDirection.Right,
                new List<GridCoord>
                {
                    new GridCoord(1, 2),
                    new GridCoord(1, 1)
                });

            BoardModel board = new BoardModel(new BoardBounds(5, 5), new[] { snake });
            SnakeMoveResult result = MoveValidator.Validate(board, snake);

            return Expect(result.Status == SnakeMoveStatus.CanEscape, "Clear route should be escapable.");
        }

        private static bool BlockedRouteFailsWithBlocker()
        {
            SnakeModel moving = new SnakeModel(
                "moving_blocked",
                0,
                GridDirection.Right,
                new List<GridCoord>
                {
                    new GridCoord(1, 2),
                    new GridCoord(1, 1)
                });

            SnakeModel blocker = new SnakeModel(
                "blocker",
                1,
                GridDirection.Up,
                new List<GridCoord>
                {
                    new GridCoord(2, 2),
                    new GridCoord(2, 3)
                });

            BoardModel board = new BoardModel(new BoardBounds(5, 5), new[] { moving, blocker });
            SnakeMoveResult result = MoveValidator.Validate(board, moving);

            bool statusOk = Expect(result.Status == SnakeMoveStatus.Blocked, "Blocked route should return Blocked.");
            bool blockerOk = Expect(result.Blocker != null && result.Blocker.Id == "blocker", "Blocked route should identify blocker.");
            bool cellOk = Expect(result.BlockedCell.Equals(new GridCoord(2, 2)), "Blocked route should report blocked cell.");
            return statusOk && blockerOk && cellOk;
        }

        private static bool StaticWallBlocksRoute()
        {
            SnakeModel moving = new SnakeModel(
                "moving_wall_blocked",
                0,
                GridDirection.Right,
                new List<GridCoord>
                {
                    new GridCoord(1, 2),
                    new GridCoord(1, 1)
                });

            var wall = new BoardObjectDefinition
            {
                Id = "wall_test",
                ObjectType = BoardObjectTypes.StaticWall,
                Cells = new List<GridCoord> { new GridCoord(2, 2) }
            };

            BoardModel board = new BoardModel(new BoardBounds(5, 5), new[] { moving }, new[] { wall });
            SnakeMoveResult result = MoveValidator.Validate(board, moving);

            bool statusOk = Expect(result.Status == SnakeMoveStatus.Blocked, "Static wall route should return Blocked.");
            bool blockerOk = Expect(result.Blocker == null && result.BlockerId == "wall_test" && result.BlockerType == BoardObjectTypes.StaticWall, "Static wall route should identify wall blocker.");
            bool cellOk = Expect(result.BlockedCell.Equals(new GridCoord(2, 2)), "Static wall route should report blocked cell.");
            return statusOk && blockerOk && cellOk;
        }

        private static bool WayBlockerUnlocksAfterEscapeCount()
        {
            SnakeModel blocked = new SnakeModel(
                "blocked_by_way",
                0,
                GridDirection.Right,
                new List<GridCoord>
                {
                    new GridCoord(1, 2),
                    new GridCoord(1, 1)
                });

            SnakeModel escaping = new SnakeModel(
                "escape_counter",
                1,
                GridDirection.Right,
                new List<GridCoord>
                {
                    new GridCoord(4, 4),
                    new GridCoord(3, 4)
                });

            var blocker = new BoardObjectDefinition
            {
                Id = "way_blocker_test",
                ObjectType = BoardObjectTypes.WayBlocker,
                LockCount = 1,
                LockTime = 1,
                Cells = new List<GridCoord> { new GridCoord(2, 2) }
            };

            BoardModel board = new BoardModel(new BoardBounds(5, 5), new[] { blocked, escaping }, new[] { blocker });

            SnakeMoveResult blockedResult = MoveValidator.Validate(board, blocked);
            bool initiallyBlocked = Expect(blockedResult.Status == SnakeMoveStatus.Blocked && blockedResult.BlockerType == BoardObjectTypes.WayBlocker, "WayBlocker should block before enough snakes escape.");

            SnakeEscapeService.TryEscape(board, escaping);
            List<BoardObjectDefinition> changed = board.TickWayBlockersAfterSnakeEscape();

            BoardObjectDefinition occupant;
            SnakeMoveResult unlockedResult = MoveValidator.Validate(board, blocked);
            bool changedOk = Expect(changed.Count == 1 && changed[0].LockCount == 0, "WayBlocker should reach zero after one escape.");
            bool removedOk = Expect(!board.Occupancy.TryGetObjectAt(new GridCoord(2, 2), out occupant), "Unlocked WayBlocker should be removed from occupancy.");
            bool canEscapeOk = Expect(unlockedResult.Status == SnakeMoveStatus.CanEscape, "Route should become escapable after WayBlocker unlocks.");

            return initiallyBlocked && changedOk && removedOk && canEscapeOk;
        }

        private static bool EscapedSnakeIsRemovedFromOccupancy()
        {
            SnakeModel snake = new SnakeModel(
                "moving_escape",
                0,
                GridDirection.Right,
                new List<GridCoord>
                {
                    new GridCoord(3, 2),
                    new GridCoord(2, 2)
                });

            BoardModel board = new BoardModel(new BoardBounds(5, 5), new[] { snake });
            SnakeMoveResult result = SnakeEscapeService.TryEscape(board, snake);

            SnakeModel occupant;
            bool escapedOk = Expect(result.Status == SnakeMoveStatus.Escaped, "Clear route should execute escape.");
            bool removedOk = Expect(!board.Occupancy.TryGetSnakeAt(new GridCoord(3, 2), out occupant), "Escaped snake head should be removed from occupancy.");
            bool remainingOk = Expect(board.RemainingSnakeCount() == 0, "Escaped snake should not count as remaining.");
            return escapedOk && removedOk && remainingOk;
        }

        private static bool Expect(bool condition, string message)
        {
            if (condition) return true;
            Debug.LogError("MoveValidator smoke test failed: " + message);
            return false;
        }
    }
}
