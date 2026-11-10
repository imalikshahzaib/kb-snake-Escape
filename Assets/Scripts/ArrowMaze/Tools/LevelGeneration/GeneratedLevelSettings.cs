namespace ArrowMaze.Tools.LevelGeneration
{
    public sealed class GeneratedLevelSettings
    {
        public string Name = "Generated_Level";
        public int Width = 10;
        public int Height = 12;
        public int SnakeCount = 12;
        public int MinSnakeLength = 2;
        public int MaxSnakeLength = 5;
        public int StaticWallCount = 0;
        public int MaxStaticWallLength = 1;
        public int WayBlockerCount = 0;
        public int MinWayBlockerLockCount = 3;
        public int MaxWayBlockerLockCount = 16;
        public int TimeLimitSeconds = 180;
        public int Seed = 12345;
        public int MaxAttemptsPerSnake = 500;
    }
}
