namespace ArrowMaze.Gameplay.Snakes
{
    public sealed class SnakeMoveCommand
    {
        public string SnakeId { get; private set; }
        public bool ForceMove { get; private set; }

        public SnakeMoveCommand(string snakeId, bool forceMove = false)
        {
            SnakeId = snakeId;
            ForceMove = forceMove;
        }
    }
}
