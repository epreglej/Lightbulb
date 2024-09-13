namespace ChessMainLoop
{
    /// <summary>
    /// Contains methods for checking if state on the board resembles check.
    /// </summary>
    public static class GameEndCalculator 
    {
        /// <summary>
        /// Checks if turn player has any available turns left.
        /// </summary>
        /// <param name="grid"></param>
        /// <returns>Color of winner side</returns>
        public static SideColor CheckIfGameEnd(Piece[,] grid)
        {
            SideColor _turnPlayer = GameManager.Instance.TurnPlayer;

            /* Checks each field on the board, and if the field contains a piece with the same color as turn player 
             * checks if the piece can move. For the first such piece that can move returns indication that there isn't a checkmate, 
             * and if no such piece can be found returns color of winner player.
             */
            for (int i = 0; i < grid.GetLength(0); i++)
            {
                for (int j = 0; j < grid.GetLength(1); j++)
                {
                    if (grid[i, j] == null)
                    {
                        continue;
                    }

                    if (grid[i, j].PieceColor == _turnPlayer)
                    {
                        if (grid[i, j].CanMove(i, j) == true)
                        {
                            return SideColor.None;
                        }
                    }
                }
            }

            if (GameManager.Instance.CheckedSide == _turnPlayer)
            {
                return _turnPlayer == SideColor.Black ? SideColor.White : SideColor.Black;
            }

            return SideColor.Both;
        }

        #region Lookup tables for movement directions
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
        #endregion

        public static bool CanMoveDiagonal(int _xPosition, int _yPosition, SideColor _attackerColor)
        {
            return CanMove(_xPosition, _yPosition, DiagonalLookup, _attackerColor);
        }

        public static bool CanMoveVertical(int _xPosition, int _yPosition, SideColor _attackerColor)
        {
            return CanMove(_xPosition, _yPosition, VerticalLookup, _attackerColor);
        }

        private static bool CanMove(int _xPosition, int _yPosition, int[,] _lookupTable, SideColor _attackerColor)
        {
            Piece _piece;

            //Checks in each direction from lookup table if there is an available spot to move which doesnt result in check for piece player side
            for (int j = 0; j < DiagonalLookup.GetLength(0); j++)
            {
                for (int i = 1; BoardState.Instance.IsInBorders(_xPosition + i * _lookupTable[j, 0], _yPosition + i * _lookupTable[j, 1]); i++)
                {
                    SideColor _checkSide = BoardState.Instance.CalculateCheckState(_xPosition, _yPosition, _xPosition + i * _lookupTable[j, 0], _yPosition + i * _lookupTable[j, 1]);
                    _piece = BoardState.Instance.GetField(_xPosition + i * _lookupTable[j, 0], _yPosition + i * _lookupTable[j, 1]);

                    if (_piece == null)
                    {
                        if (_checkSide != _attackerColor && _checkSide != SideColor.Both)
                        {
                            return true;
                        }
                    }
                    else if (_piece.PieceColor != _attackerColor)
                    {
                        if (_checkSide == _attackerColor || _checkSide == SideColor.Both)
                        {
                            break; 
                        }
                        else
                        {
                            return true;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }

            return false;
        }

        public static bool CanMoveToSpot(int _xPosition, int _yPosition, int _xDirection, int _yDirection, SideColor _attackerColor)
        {
            Piece _piece;

            //Checks if the target location is available for moving and doesnt result in a check
            if (BoardState.Instance.IsInBorders(_xPosition + _xDirection, _yPosition + _yDirection))
            {
                SideColor _checkSide = BoardState.Instance.CalculateCheckState(_xPosition, _yPosition, _xPosition + _xDirection, _yPosition + _yDirection);
                _piece = BoardState.Instance.GetField(_xPosition + _xDirection, _yPosition + _yDirection);

                if (_piece == null)
                {
                    if (_checkSide != _attackerColor && _checkSide != SideColor.Both)
                    {
                        return true;
                    }
                }
                else if (_piece.PieceColor != _attackerColor)
                {
                    if (_checkSide == _attackerColor || _checkSide == SideColor.Both)
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }

            }
            return false;
        }
    }
}