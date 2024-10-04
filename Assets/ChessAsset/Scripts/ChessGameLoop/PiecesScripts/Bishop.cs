namespace ChessMainLoop
{
    public class Bishop : Piece
    {
        public override void CreatePath()
        {
            PathManager.CreateDiagonalPath(this);
        }

        public override bool IsAttackingKing(int _xPosition, int _yPosition)
        {
            return CheckStateCalculator.IsAttackingKingDiagonal(_xPosition, _yPosition, PieceColor);
        }

        public override bool CanMove(int _xPosition, int _yPosition)
        {
            return GameEndCalculator.CanMoveDiagonal(_xPosition, _yPosition, PieceColor);
        }
    }
}