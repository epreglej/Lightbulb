namespace ChessMainLoop
{
    public class Queen : Piece
    {    
        public override void CreatePath()
        {
            PathManager.CreateDiagonalPath(this);
            PathManager.CreateVerticalPath(this);
        }

        public override bool IsAttackingKing(int row, int column)
        {
            return CheckStateCalculator.IsAttackingKingVertical(row, column, PieceColor) || CheckStateCalculator.IsAttackingKingDiagonal(row, column, PieceColor);
        }

        public override bool CanMove(int _xPosition, int _yPosition)
        {
            return GameEndCalculator.CanMoveDiagonal(_xPosition, _yPosition, PieceColor) || GameEndCalculator.CanMoveVertical(_xPosition, _yPosition, PieceColor);
        }
    }
}