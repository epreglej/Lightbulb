namespace ChessMainLoop
{
    public class Bishop : Piece
    {
        public override void CreatePath()
        {
            PathCalculator.DiagonalPath(this);
        }

        public override bool IsAttackingKing(int _xPosition, int _yPosition)
        {
            return CheckStateCalculator.SearchForKingDiagonal(_xPosition, _yPosition, PieceColor);
        }

        public override bool CanMove(int _xPosition, int _yPosition)
        {
            return GameEndCalculator.CanMoveDiagonal(_xPosition, _yPosition, PieceColor);
        }
    }
}