namespace ChessMainLoop
{
    public static class CheckStateCalculator
    {
        public static SideColor CalculateCheck(Piece[,] grid)
        {
            bool _whiteCheck = false;
            bool _blackCheck = false;

            for(int i = 0; i < grid.GetLength(0); i++)
            {
                for(int j = 0; j < grid.GetLength(1); j++)
                {
                    if (grid[i, j] == null)
                    {
                        continue;
                    }

                    if (grid[i, j].IsAttackingKing(i, j))
                    {
                        if (grid[i, j].PieceColor == SideColor.Black)
                        {
                            _whiteCheck = true;
                        }
                        else
                        {
                            _blackCheck = true;
                        }
                    }
                }            
            }

            return _whiteCheck ? _blackCheck ? SideColor.Both : SideColor.White : _blackCheck ? SideColor.Black : SideColor.None;
        }

        private static readonly int[,] DiagonalLookup =
        {
           { 1, 1 },
           { 1, -1 },
           { -1, 1 },
           { -1, -1 }
        };

        private static readonly int[,] VerticalLookup =
        {
           { 1, 0 },
           { -1, 0 },
           { 0, 1 },
           { 0, -1 }
        };

        public static bool SearchForKingDiagonal(int _xPosition, int _yPosition, SideColor _attackerColor)
        {
            return SearchForKing(_xPosition,_yPosition, DiagonalLookup, _attackerColor);
        }

        public static bool SearchForKingVertical(int _xPosition, int _yPosition, SideColor _attackerColor)
        {
            return SearchForKing(_xPosition, _yPosition, VerticalLookup, _attackerColor);
        }

        private static bool SearchForKing(int _xPosition, int _yPosition, int[,] _lookupTable, SideColor _attackerColor)
        {
            Piece _piece;

            for (int j = 0; j < DiagonalLookup.GetLength(0); j++)
            {
                for (int i = 1; BoardState.Instance.IsInBorders(_xPosition + i * _lookupTable[j, 0], _yPosition + i * _lookupTable[j, 1]); i++)
                {
                    _piece = BoardState.Instance.GetField(_xPosition + i * _lookupTable[j, 0], _yPosition + i * _lookupTable[j, 1]);

                    if (_piece != null)
                    {
                        if(_piece is King && _piece.PieceColor != _attackerColor)
                        {
                            return true;
                        }
                        else
                        {
                            break; 
                        }
                    }
                }
            }

            return false;
        }

        public static bool KingAtLocation(int _xPosition, int _yPosition, int _xDirection, int _yDirection, SideColor _attackerColor)
        {

            if (BoardState.Instance.IsInBorders(_xPosition + _xDirection, _yPosition + _yDirection))
            {
                Piece _piece = BoardState.Instance.GetField(_xPosition + _xDirection, _yPosition + _yDirection);
                if (_piece != null)
                {
                    if (_piece is King && _piece.PieceColor != _attackerColor)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            return false;
        }
    }
}