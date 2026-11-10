using System;
using System.Collections.Generic;
using ArrowMaze.Common;
using ArrowMaze.LevelContent;

namespace ArrowMaze.Tools.LevelGeneration
{
    public static class CleanRoomLevelGenerator
    {
        private static readonly GridDirection[] Directions =
        {
            GridDirection.Up,
            GridDirection.Right,
            GridDirection.Down,
            GridDirection.Left
        };

        public static Result<LevelDefinition> Generate(GeneratedLevelSettings settings)
        {
            if (settings == null)
            {
                return Result<LevelDefinition>.Failure("Generation settings are null.");
            }

            Normalize(settings);

            var level = new LevelDefinition
            {
                Name = settings.Name,
                Width = settings.Width,
                Height = settings.Height,
                TimeLimitSeconds = settings.TimeLimitSeconds,
                Mode = LevelMode.MainSaga,
                SchemaVersion = 1
            };

            var occupied = new HashSet<GridCoord>();
            var random = new Random(settings.Seed);

            for (int snakeIndex = 0; snakeIndex < settings.SnakeCount; snakeIndex++)
            {
                SnakeDefinition snake = TryCreateSnake(level, settings, occupied, snakeIndex, random);
                if (snake == null)
                {
                    return Result<LevelDefinition>.Failure("Could not place snake " + snakeIndex + " for " + settings.Name + ".");
                }

                level.Snakes.Add(snake);
                for (int i = 0; i < snake.Cells.Count; i++)
                {
                    occupied.Add(snake.Cells[i]);
                }
            }

            TryPlaceStaticWalls(level, settings, occupied, random);
            TryPlaceWayBlockers(level, settings, occupied, random);

            DifficultyEstimate difficulty = DifficultyEstimator.Estimate(level);
            level.Difficulty = difficulty.Difficulty;

            LevelValidationResult validation = LevelValidator.Validate(level, level.Name);
            if (!validation.IsValid)
            {
                return Result<LevelDefinition>.Failure("Generated level failed validation with " + validation.ErrorCount + " error(s).");
            }

            LevelSolvabilityResult solvability = LevelSolvabilityChecker.Check(level);
            if (!solvability.IsSolvable)
            {
                return Result<LevelDefinition>.Failure("Generated level failed solvability: " + solvability.Error);
            }

            return Result<LevelDefinition>.Success(level);
        }

        public static GeneratedLevelSettings CreateSettingsForLevelNumber(int levelNumber, int seed)
        {
            int safeNumber = Math.Max(1, levelNumber);
            return new GeneratedLevelSettings
            {
                Name = "Level_" + safeNumber,
                Width = Clamp(8 + safeNumber / 8, 8, 18),
                Height = Clamp(10 + safeNumber / 7, 10, 22),
                SnakeCount = Clamp(5 + safeNumber, 8, 42),
                MinSnakeLength = Clamp(2 + safeNumber / 45, 2, 4),
                MaxSnakeLength = Clamp(3 + safeNumber / 12, 4, 8),
                StaticWallCount = safeNumber >= 12 ? Clamp((safeNumber - 8) / 8, 1, 8) : 0,
                MaxStaticWallLength = 1,
                WayBlockerCount = safeNumber >= 18 ? Clamp((safeNumber - 14) / 12, 1, 4) : 0,
                MinWayBlockerLockCount = 3,
                MaxWayBlockerLockCount = Clamp(6 + safeNumber / 8, 8, 28),
                TimeLimitSeconds = 180,
                Seed = seed,
                MaxAttemptsPerSnake = 900
            };
        }

        private static SnakeDefinition TryCreateSnake(
            LevelDefinition level,
            GeneratedLevelSettings settings,
            HashSet<GridCoord> occupied,
            int snakeIndex,
            Random random)
        {
            for (int attempt = 0; attempt < settings.MaxAttemptsPerSnake; attempt++)
            {
                GridDirection direction = Directions[random.Next(Directions.Length)];
                GridCoord head = new GridCoord(random.Next(level.Width), random.Next(level.Height));
                if (occupied.Contains(head)) continue;
                if (!IsEscapeRouteClear(level, head, direction, occupied)) continue;

                int targetLength = random.Next(settings.MinSnakeLength, settings.MaxSnakeLength + 1);
                List<GridCoord> cells = BuildBodyCells(level, head, direction, targetLength, occupied, random);
                if (cells == null || cells.Count < settings.MinSnakeLength) continue;

                return new SnakeDefinition
                {
                    Id = "snake_" + snakeIndex.ToString("000"),
                    SourceIndex = snakeIndex,
                    Direction = direction,
                    HasDeclaredHead = true,
                    DeclaredHead = head,
                    Cells = cells
                };
            }

            return null;
        }

        private static void TryPlaceStaticWalls(
            LevelDefinition level,
            GeneratedLevelSettings settings,
            HashSet<GridCoord> occupied,
            Random random)
        {
            if (settings.StaticWallCount <= 0) return;

            int placed = 0;
            int attempts = Math.Max(100, settings.StaticWallCount * 80);
            while (placed < settings.StaticWallCount && attempts > 0)
            {
                attempts--;
                GridCoord cell = new GridCoord(random.Next(level.Width), random.Next(level.Height));
                if (occupied.Contains(cell)) continue;
                if (WouldBlockAllCurrentFirstMoves(level, cell)) continue;

                var wall = new BoardObjectDefinition
                {
                    Id = "wall_" + placed.ToString("000"),
                    ObjectType = BoardObjectTypes.StaticWall
                };
                wall.Cells.Add(cell);
                level.Objects.Add(wall);
                occupied.Add(cell);
                placed++;
            }
        }

        private static void TryPlaceWayBlockers(
            LevelDefinition level,
            GeneratedLevelSettings settings,
            HashSet<GridCoord> occupied,
            Random random)
        {
            if (settings.WayBlockerCount <= 0) return;

            int placed = 0;
            int attempts = Math.Max(100, settings.WayBlockerCount * 100);
            while (placed < settings.WayBlockerCount && attempts > 0)
            {
                attempts--;
                GridCoord cell = new GridCoord(random.Next(level.Width), random.Next(level.Height));
                if (occupied.Contains(cell)) continue;
                if (WouldBlockAllCurrentFirstMoves(level, cell)) continue;

                int maxLock = Math.Max(settings.MinWayBlockerLockCount, settings.MaxWayBlockerLockCount);
                int lockCount = random.Next(settings.MinWayBlockerLockCount, maxLock + 1);
                lockCount = Math.Min(lockCount, Math.Max(1, settings.SnakeCount - 1));

                var blocker = new BoardObjectDefinition
                {
                    Id = "way_blocker_" + placed.ToString("000"),
                    ObjectType = BoardObjectTypes.WayBlocker,
                    LockCount = lockCount,
                    LockTime = lockCount
                };

                blocker.Cells.Add(cell);
                level.Objects.Add(blocker);
                occupied.Add(cell);
                placed++;
            }
        }

        private static bool WouldBlockAllCurrentFirstMoves(LevelDefinition level, GridCoord candidateWall)
        {
            for (int i = 0; i < level.Snakes.Count; i++)
            {
                SnakeDefinition snake = level.Snakes[i];
                if (snake == null || snake.Direction == GridDirection.None || snake.Cells == null || snake.Cells.Count == 0)
                {
                    continue;
                }

                GridCoord check = snake.Head + snake.Direction.ToDelta();
                bool blockedByCandidate = false;
                while (IsInside(level, check))
                {
                    if (check.Equals(candidateWall))
                    {
                        blockedByCandidate = true;
                        break;
                    }

                    check += snake.Direction.ToDelta();
                }

                if (!blockedByCandidate)
                {
                    return false;
                }
            }

            return true;
        }

        private static List<GridCoord> BuildBodyCells(
            LevelDefinition level,
            GridCoord head,
            GridDirection escapeDirection,
            int targetLength,
            HashSet<GridCoord> occupied,
            Random random)
        {
            var cells = new List<GridCoord> { head };
            var used = new HashSet<GridCoord> { head };
            GridDirection previousDirection = Opposite(escapeDirection);

            while (cells.Count < targetLength)
            {
                List<GridDirection> candidates = CreateBodyDirectionOrder(previousDirection, escapeDirection, random);
                bool placed = false;

                for (int i = 0; i < candidates.Count; i++)
                {
                    GridCoord next = cells[cells.Count - 1] + candidates[i].ToDelta();
                    if (!IsInside(level, next)) continue;
                    if (occupied.Contains(next)) continue;
                    if (used.Contains(next)) continue;

                    cells.Add(next);
                    used.Add(next);
                    previousDirection = candidates[i];
                    placed = true;
                    break;
                }

                if (!placed)
                {
                    break;
                }
            }

            return cells.Count >= 2 ? cells : null;
        }

        private static List<GridDirection> CreateBodyDirectionOrder(GridDirection preferred, GridDirection escapeDirection, Random random)
        {
            var directions = new List<GridDirection>();
            if (preferred != GridDirection.None && preferred != escapeDirection)
            {
                directions.Add(preferred);
            }

            for (int i = 0; i < Directions.Length; i++)
            {
                GridDirection direction = Directions[i];
                if (direction == escapeDirection) continue;
                if (!directions.Contains(direction))
                {
                    directions.Add(direction);
                }
            }

            for (int i = 0; i < directions.Count; i++)
            {
                int swapIndex = random.Next(i, directions.Count);
                GridDirection temp = directions[i];
                directions[i] = directions[swapIndex];
                directions[swapIndex] = temp;
            }

            return directions;
        }

        private static bool IsEscapeRouteClear(LevelDefinition level, GridCoord head, GridDirection direction, HashSet<GridCoord> occupied)
        {
            GridCoord check = head + direction.ToDelta();
            while (IsInside(level, check))
            {
                if (occupied.Contains(check))
                {
                    return false;
                }

                check += direction.ToDelta();
            }

            return true;
        }

        private static bool IsInside(LevelDefinition level, GridCoord cell)
        {
            return cell.X >= 0 && cell.X < level.Width && cell.Y >= 0 && cell.Y < level.Height;
        }

        private static GridDirection Opposite(GridDirection direction)
        {
            switch (direction)
            {
                case GridDirection.Up:
                    return GridDirection.Down;
                case GridDirection.Down:
                    return GridDirection.Up;
                case GridDirection.Left:
                    return GridDirection.Right;
                case GridDirection.Right:
                    return GridDirection.Left;
                default:
                    return GridDirection.None;
            }
        }

        private static void Normalize(GeneratedLevelSettings settings)
        {
            settings.Width = Clamp(settings.Width, 4, 60);
            settings.Height = Clamp(settings.Height, 4, 60);
            settings.SnakeCount = Clamp(settings.SnakeCount, 1, settings.Width * settings.Height / 2);
            settings.MinSnakeLength = Clamp(settings.MinSnakeLength, 2, 12);
            settings.MaxSnakeLength = Clamp(settings.MaxSnakeLength, settings.MinSnakeLength, 14);
            settings.StaticWallCount = Clamp(settings.StaticWallCount, 0, settings.Width * settings.Height / 5);
            settings.MaxStaticWallLength = Clamp(settings.MaxStaticWallLength, 1, 4);
            settings.WayBlockerCount = Clamp(settings.WayBlockerCount, 0, settings.Width * settings.Height / 12);
            settings.MinWayBlockerLockCount = Math.Max(1, settings.MinWayBlockerLockCount);
            settings.MaxWayBlockerLockCount = Math.Max(settings.MinWayBlockerLockCount, settings.MaxWayBlockerLockCount);
            settings.TimeLimitSeconds = Math.Max(15, settings.TimeLimitSeconds);
            settings.MaxAttemptsPerSnake = Math.Max(50, settings.MaxAttemptsPerSnake);
        }

        private static int Clamp(int value, int min, int max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }
    }
}
