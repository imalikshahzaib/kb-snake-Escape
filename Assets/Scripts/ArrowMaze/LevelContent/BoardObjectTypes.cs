namespace ArrowMaze.LevelContent
{
    public static class BoardObjectTypes
    {
        public const string Snake = "Snake";
        public const string StaticWall = "StaticWall";
        public const string Barrier = "Barrier";
        public const string WayBlocker = "WayBlocker";

        public static bool BlocksMovement(string objectType)
        {
            return objectType == StaticWall || objectType == Barrier || objectType == WayBlocker;
        }

        public static bool BlocksMovement(BoardObjectDefinition boardObject)
        {
            if (boardObject == null) return false;
            if (boardObject.ObjectType == WayBlocker)
            {
                return GetEffectiveLockCount(boardObject) > 0;
            }

            return BlocksMovement(boardObject.ObjectType);
        }

        public static bool IsSupported(string objectType)
        {
            return objectType == StaticWall || objectType == Barrier || objectType == WayBlocker;
        }

        public static bool IsCountLocked(string objectType)
        {
            return objectType == WayBlocker;
        }

        public static int GetEffectiveLockCount(BoardObjectDefinition boardObject)
        {
            if (boardObject == null) return 0;
            return boardObject.LockCount > 0 ? boardObject.LockCount : boardObject.LockTime;
        }

        public static void EnsureRuntimeLockCount(BoardObjectDefinition boardObject)
        {
            if (boardObject == null) return;
            if (boardObject.ObjectType != WayBlocker) return;
            if (boardObject.LockCount <= 0 && boardObject.LockTime > 0)
            {
                boardObject.LockCount = boardObject.LockTime;
            }
        }
    }
}
