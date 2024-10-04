namespace ChessMainLoop
{
    public class Queen : Piece
    {    
        public override void CreatePath()
        {
            PathManager.CreateDiagonalPath(this);
            PathManager.CreateVerticalPath(this);
        }

        public override bool IsAttackingKing(int _xPosition, int _yPosition)
        {
            return CheckStateCalculator.IsAttackingKingVertical(_xPosition, _yPosition, PieceColor) || CheckStateCalculator.IsAttackingKingDiagonal(_xPosition, _yPosition, PieceColor);
        }

        public override bool CanMove(int _xPosition, int _yPosition)
        {
            return GameEndCalculator.CanMoveDiagonal(_xPosition, _yPosition, PieceColor) || GameEndCalculator.CanMoveVertical(_xPosition, _yPosition, PieceColor);
        }
    }
}