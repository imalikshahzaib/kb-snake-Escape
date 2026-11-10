using ArrowMaze.Common;

namespace ArrowMaze.Gameplay.Board
{
    public sealed class BoardCell
    {
        public GridCoord Coord { get; private set; }
        public string OccupantId { get; private set; }

        public bool IsOccupied
        {
            get { return !string.IsNullOrEmpty(OccupantId); }
        }

        public BoardCell(GridCoord coord)
        {
            Coord = coord;
            OccupantId = string.Empty;
        }

        public void SetOccupant(string occupantId)
        {
            OccupantId = occupantId ?? string.Empty;
        }

        public void Clear()
        {
            OccupantId = string.Empty;
        }
    }
}
