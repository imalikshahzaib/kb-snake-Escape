using System;
using System.Collections.Generic;
using ArrowMaze.Common;

namespace ArrowMaze.LevelContent
{
    [Serializable]
    public sealed class BoardObjectDefinition
    {
        public string Id;
        public string ObjectType;
        public List<GridCoord> Cells = new List<GridCoord>();
        public string GroupId;
        public int LockCount;
        public int LockTime;
    }
}
