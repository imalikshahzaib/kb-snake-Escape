using System.Collections.Generic;
using ArrowMaze.Common;

namespace ArrowMaze.LevelContent
{
    public static class LevelValidator
    {
        public static LevelValidationResult Validate(LevelDefinition level, string expectedName = null)
        {
            var result = new LevelValidationResult();

            if (level == null)
            {
                result.AddError("level_null", "Level definition is null.");
                return result;
            }

            if (string.IsNullOrWhiteSpace(level.Name))
            {
                result.AddError("missing_name", "Level name is missing.");
            }
            else if (!string.IsNullOrWhiteSpace(expectedName) && level.Name != expectedName)
            {
                result.AddError("name_mismatch", "File name expects '" + expectedName + "' but level contains '" + level.Name + "'.");
            }

            if (level.Width <= 0 || level.Height <= 0)
            {
                result.AddError("invalid_size", "Board size must be greater than zero.");
                return result;
            }

            if (level.Width > 60 || level.Height > 60)
            {
                result.AddWarning("large_board", "Board is larger than the planned mobile validation range.");
            }

            if (level.Snakes == null || level.Snakes.Count == 0)
            {
                result.AddError("missing_snakes", "Level must contain at least one snake/arrow.");
                return result;
            }

            var occupancy = new Dictionary<GridCoord, int>();
            var objectOccupancy = new Dictionary<GridCoord, BoardObjectDefinition>();

            for (int snakeIndex = 0; snakeIndex < level.Snakes.Count; snakeIndex++)
            {
                SnakeDefinition snake = level.Snakes[snakeIndex];
                ValidateSnake(level, snake, snakeIndex, occupancy, result);
            }

            if (level.Objects != null)
            {
                for (int objectIndex = 0; objectIndex < level.Objects.Count; objectIndex++)
                {
                    BoardObjectDefinition boardObject = level.Objects[objectIndex];
                    ValidateBoardObject(level, boardObject, objectIndex, occupancy, objectOccupancy, result);
                }
            }

            if (!HasAnyLegalFirstMove(level, occupancy, objectOccupancy))
            {
                result.AddError("no_legal_first_move", "No snake can escape at the initial board state.");
            }

            return result;
        }

        private static void ValidateSnake(
            LevelDefinition level,
            SnakeDefinition snake,
            int snakeIndex,
            Dictionary<GridCoord, int> occupancy,
            LevelValidationResult result)
        {
            string prefix = "snake_" + snakeIndex;

            if (snake == null)
            {
                result.AddError(prefix + "_null", "Snake definition is null.");
                return;
            }

            if (snake.Direction == GridDirection.None)
            {
                result.AddError(prefix + "_direction_invalid", "Snake direction must be one of up, down, left, or right.");
            }

            if (snake.Cells == null || snake.Cells.Count == 0)
            {
                result.AddError(prefix + "_missing_cells", "Snake has no path cells.");
                return;
            }

            if (snake.HasDeclaredHead && !snake.DeclaredHead.Equals(snake.Head))
            {
                result.AddError(prefix + "_head_mismatch", "Declared X/Y head does not match the first path cell.");
            }

            var ownCells = new HashSet<GridCoord>();

            for (int i = 0; i < snake.Cells.Count; i++)
            {
                GridCoord cell = snake.Cells[i];

                if (!IsInside(level, cell))
                {
                    result.AddError(prefix + "_cell_out_of_bounds", "Cell " + cell + " is outside the board.");
                }

                if (!ownCells.Add(cell))
                {
                    result.AddError(prefix + "_self_overlap", "Snake contains duplicate cell " + cell + ".");
                }

                if (i > 0)
                {
                    GridCoord previous = snake.Cells[i - 1];
                    if (cell.ManhattanDistance(previous) != 1)
                    {
                        result.AddError(prefix + "_path_not_adjacent", "Snake path has non-adjacent cells at index " + (i - 1) + " and " + i + ".");
                    }
                }

                if (occupancy.TryGetValue(cell, out int otherSnakeIndex) && otherSnakeIndex != snakeIndex)
                {
                    result.AddError("overlap", "Cell " + cell + " is occupied by snake " + otherSnakeIndex + " and snake " + snakeIndex + ".");
                }
                else
                {
                    occupancy[cell] = snakeIndex;
                }
            }
        }

        private static void ValidateBoardObject(
            LevelDefinition level,
            BoardObjectDefinition boardObject,
            int objectIndex,
            Dictionary<GridCoord, int> snakeOccupancy,
            Dictionary<GridCoord, BoardObjectDefinition> objectOccupancy,
            LevelValidationResult result)
        {
            string prefix = "object_" + objectIndex;

            if (boardObject == null)
            {
                result.AddError(prefix + "_null", "Board object definition is null.");
                return;
            }

            if (string.IsNullOrWhiteSpace(boardObject.Id))
            {
                result.AddWarning(prefix + "_missing_id", "Board object has no stable id.");
            }

            if (string.IsNullOrWhiteSpace(boardObject.ObjectType))
            {
                result.AddError(prefix + "_type_missing", "Board object type is missing.");
            }
            else if (!BoardObjectTypes.IsSupported(boardObject.ObjectType))
            {
                result.AddWarning(prefix + "_type_unknown", "Board object type '" + boardObject.ObjectType + "' is not implemented yet.");
            }
            else if (BoardObjectTypes.IsCountLocked(boardObject.ObjectType) && BoardObjectTypes.GetEffectiveLockCount(boardObject) <= 0)
            {
                result.AddError(prefix + "_lock_count_invalid", "WayBlocker must have a positive lock count.");
            }

            if (boardObject.Cells == null || boardObject.Cells.Count == 0)
            {
                result.AddError(prefix + "_missing_cells", "Board object has no cells.");
                return;
            }

            var ownCells = new HashSet<GridCoord>();
            for (int i = 0; i < boardObject.Cells.Count; i++)
            {
                GridCoord cell = boardObject.Cells[i];

                if (!IsInside(level, cell))
                {
                    result.AddError(prefix + "_cell_out_of_bounds", "Cell " + cell + " is outside the board.");
                }

                if (!ownCells.Add(cell))
                {
                    result.AddError(prefix + "_self_overlap", "Board object contains duplicate cell " + cell + ".");
                }

                if (snakeOccupancy.TryGetValue(cell, out int snakeIndex))
                {
                    result.AddError(prefix + "_snake_overlap", "Cell " + cell + " overlaps snake " + snakeIndex + ".");
                }

                if (objectOccupancy.TryGetValue(cell, out BoardObjectDefinition otherObject))
                {
                    string otherObjectId = otherObject != null ? otherObject.Id : string.Empty;
                    result.AddError(prefix + "_object_overlap", "Cell " + cell + " overlaps object '" + otherObjectId + "'.");
                }
                else
                {
                    objectOccupancy[cell] = boardObject;
                }
            }
        }

        private static bool HasAnyLegalFirstMove(
            LevelDefinition level,
            Dictionary<GridCoord, int> occupancy,
            Dictionary<GridCoord, BoardObjectDefinition> objectOccupancy)
        {
            for (int i = 0; i < level.Snakes.Count; i++)
            {
                if (CanEscape(level, level.Snakes[i], i, occupancy, objectOccupancy))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool CanEscape(
            LevelDefinition level,
            SnakeDefinition snake,
            int snakeIndex,
            Dictionary<GridCoord, int> occupancy,
            Dictionary<GridCoord, BoardObjectDefinition> objectOccupancy)
        {
            if (snake == null || snake.Cells == null || snake.Cells.Count == 0 || snake.Direction == GridDirection.None)
            {
                return false;
            }

            GridCoord delta = snake.Direction.ToDelta();
            GridCoord check = snake.Head + delta;

            while (IsInside(level, check))
            {
                if (occupancy.TryGetValue(check, out int ownerIndex) && ownerIndex != snakeIndex)
                {
                    return false;
                }

                BoardObjectDefinition boardObject;
                if (objectOccupancy != null &&
                    objectOccupancy.TryGetValue(check, out boardObject) &&
                    boardObject != null &&
                    BoardObjectTypes.BlocksMovement(boardObject))
                {
                    return false;
                }

                check += delta;
            }

            return true;
        }

        private static bool IsInside(LevelDefinition level, GridCoord cell)
        {
            return cell.X >= 0 && cell.X < level.Width && cell.Y >= 0 && cell.Y < level.Height;
        }
    }
}
