using System;
using System.Collections.Generic;

namespace ArrowMaze.LevelContent
{
    [Serializable]
    public sealed class LevelDefinition
    {
        public string Name;
        public int Width;
        public int Height;
        public int TimeLimitSeconds;
        public int SchemaVersion = 1;
        public LevelMode Mode = LevelMode.MainSaga;
        public LevelDifficulty Difficulty = LevelDifficulty.Normal;
        public List<SnakeDefinition> Snakes = new List<SnakeDefinition>();
        public List<BoardObjectDefinition> Objects = new List<BoardObjectDefinition>();
    }
}
