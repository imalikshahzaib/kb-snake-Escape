namespace ArrowMaze.LevelContent
{
    public sealed class DifficultyEstimate
    {
        public int Score;
        public LevelDifficulty Difficulty;
        public int SnakeCount;
        public int ObjectCount;
        public int TotalPathNodes;
        public int BoardArea;
    }

    public static class DifficultyEstimator
    {
        public static DifficultyEstimate Estimate(LevelDefinition level)
        {
            var estimate = new DifficultyEstimate();
            if (level == null)
            {
                estimate.Difficulty = LevelDifficulty.Normal;
                return estimate;
            }

            estimate.BoardArea = level.Width * level.Height;
            estimate.SnakeCount = level.Snakes != null ? level.Snakes.Count : 0;
            estimate.ObjectCount = level.Objects != null ? level.Objects.Count : 0;

            if (level.Snakes != null)
            {
                for (int i = 0; i < level.Snakes.Count; i++)
                {
                    SnakeDefinition snake = level.Snakes[i];
                    if (snake != null && snake.Cells != null)
                    {
                        estimate.TotalPathNodes += snake.Cells.Count;
                    }
                }
            }

            estimate.Score =
                estimate.SnakeCount * 3 +
                estimate.ObjectCount * 4 +
                estimate.TotalPathNodes +
                estimate.BoardArea / 12;

            if (estimate.Score >= 165)
            {
                estimate.Difficulty = LevelDifficulty.SuperHard;
            }
            else if (estimate.Score >= 95)
            {
                estimate.Difficulty = LevelDifficulty.Hard;
            }
            else
            {
                estimate.Difficulty = LevelDifficulty.Normal;
            }

            return estimate;
        }
    }
}
