using System.Collections.Generic;
using UnityEngine;

namespace ArrowMaze.Data
{
    [System.Serializable]
    public enum MoveDirection
    {
        Up,
        Down,
        Left,
        Right
    }

    [System.Serializable]
    public class ArrowData
    {
        public List<Vector2Int> bodyPositions;
        public MoveDirection direction;
    }

    [System.Serializable]
    public class BoardObjectData
    {
        public string id;
        public string objectType;
        public List<Vector2Int> cells;
        public string groupId;
        public int lockCount;
        public int lockTime;
    }

    [CreateAssetMenu(fileName = "NewLevel", menuName = "ArrowMaze/LevelData")]
    public class LevelData : ScriptableObject
    {
        public int width;
        public int height;
        public List<ArrowData> arrows;
        public List<BoardObjectData> objects;
    }
}
