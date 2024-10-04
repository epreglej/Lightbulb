namespace ChessMainLoop
{
    public class Knight : Piece
    {
        /// <summary>
        /// Lookup table containing knight movement directions
        /// </summary>
        private static readonly int[,] LookupMoves =
        {
           { 1, -2 },
           { 2, -1 },
           { 2, 1 },
           { 1, 2 },
           { -1, 2 },
           { -2, 1 },
           { -2, -1 },
           { -1, -2 }
        };

        public override void CreatePath()
        {
            for (int i = 0; i < LookupMoves.GetLength(0); i++)
            {
                PathManager.PathOneSpot(this, LookupMoves[i, 0], LookupMoves[i, 1]);
            }
        }

        public override bool IsAttackingKing(int _xPosition, int _yPosition)
        {
            for (int i = 0; i < LookupMoves.GetLength(0); i++)
            {
                if (CheckStateCalculator.IsEnemyKingAtLocation(_xPosition, _yPosition, LookupMoves[i, 0], LookupMoves[i, 1], PieceColor))
                {
                    return true;
                }
            }

            return false;
        }

        public override bool CanMove(int _xPosition, int _yPosition)
        {
            for (int i = 0; i < LookupMoves.GetLength(0); i++)
            {
                if (GameEndCalculator.CanMoveToSpot(_xPosition, _yPosition, LookupMoves[i, 0], LookupMoves[i, 1], PieceColor))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
