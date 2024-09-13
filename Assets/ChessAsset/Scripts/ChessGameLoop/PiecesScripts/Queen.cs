namespace ChessMainLoop
{
    public class Queen : Piece
    {    
        public override void CreatePath()
        {
            PathCalculator.DiagonalPath(this);
            PathCalculator.VerticalPath(this);
        }

        public override bool IsAttackingKing(int _xPosition, int _yPosition)
        {
            return CheckStateCalculator.SearchForKingVertical(_xPosition, _yPosition, PieceColor) || CheckStateCalculator.SearchForKingDiagonal(_xPosition, _yPosition, PieceColor);
        }

        public override bool CanMove(int _xPosition, int _yPosition)
        {
            return GameEndCalculator.CanMoveDiagonal(_xPosition, _yPosition, PieceColor) || GameEndCalculator.CanMoveVertical(_xPosition, _yPosition, PieceColor);
        }
    }
}