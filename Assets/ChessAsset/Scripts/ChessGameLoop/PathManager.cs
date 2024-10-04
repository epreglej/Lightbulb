using UnityEngine;

namespace ChessMainLoop
{
    /// <summary>
    /// Contains methods for calculating viable positions piece can move to, and placing path fields on them with appropriate color
    /// </summary>
    public static class PathManager
    {
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

        public static void CreateDiagonalPath(Piece caller)
        {
            CreatePathOnDirection(caller, DiagonalLookup);
        }

        public static void CreateVerticalPath(Piece caller)
        {
            CreatePathOnDirection(caller, VerticalLookup);
        }

        /// <summary>
        /// Checks for available spots for directions specified in lookup table and sets path field on them. Stops at first enemy or unavailable field in each direction.
        /// </summary>
        private static void CreatePathOnDirection(Piece caller, int[,] lookupTable)
        {
            GameObject path;
            Vector3 position = new Vector3();
            Piece piece;

            int startRow = caller.Location.Row;
            int startColumn = caller.Location.Column;

            for (int j = 0; j < lookupTable.Length; j++)
            {
                /*Check field by field in current direction from lookup table until it reaches the edge of the board or enemy field.
                 * On empty fields creates walk path indexed by name HighlightPathYellow and on enemy fields creates enemy path 
                 * indexed by name HighlightPathRed
                 */
                for (int i = 1; BoardState.Instance.IsInBorders(startRow + i * lookupTable[j, 0], startColumn + i * lookupTable[j, 1]); i++)
                {
                    SideColor checkSide = BoardState.Instance.SimulateCheckState(startRow, startColumn, startRow + i * lookupTable[j, 0], startColumn + i * lookupTable[j, 1]);
                    piece = BoardState.Instance.GetField(startRow + i * lookupTable[j, 0], startColumn + i * lookupTable[j, 1]);

                    if (checkSide == caller.PieceColor || checkSide == SideColor.Both)
                    {
                        continue;
                    }

                    if (piece == null)
                    {
                        if (checkSide == caller.PieceColor || checkSide == SideColor.Both)
                        {
                            continue;
                        }

                        path = ObjectPool.Instance.GetHighlightPath("HighlightPathYellow");
                        position.x = caller.transform.localPosition.x + i * BoardState.Offset * lookupTable[j, 0];
                        position.z = caller.transform.localPosition.z + i * BoardState.Offset * lookupTable[j, 1];
                        position.y = path.transform.localPosition.y;

                        path.transform.localPosition = position;
                    }
                    else if (piece.PieceColor != caller.PieceColor)
                    {
                        if (checkSide == caller.PieceColor || checkSide == SideColor.Both)
                        {
                            break;
                        }

                        path = ObjectPool.Instance.GetHighlightPath("HighlightPathRed");
                        path.GetComponent<PathPiece>().AssignPiece(piece);
                        position.x = caller.transform.localPosition.x + i * BoardState.Offset * lookupTable[j, 0];
                        position.z = caller.transform.localPosition.z + i * BoardState.Offset * lookupTable[j, 1];
                        position.y = path.transform.localPosition.y;

                        path.transform.localPosition = position;
                        break;
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Checks if the field located at callers position translated by direction parameters is free to move. 
        /// If it is empty creates walk path or if it contains an enemy creates enemy path.
        /// </summary>
        public static void PathOneSpot(Piece _caller, int _xDirection, int _yDirection)
        {
            GameObject _path;
            Vector3 _position = new Vector3();

            int _xSource = (int)(_caller.transform.localPosition.x / BoardState.Offset);
            int _ySource = (int)(_caller.transform.localPosition.z / BoardState.Offset);

            if (BoardState.Instance.IsInBorders(_xSource + _xDirection, _ySource + _yDirection))
            {
                SideColor _checkSide = BoardState.Instance.SimulateCheckState(_xSource, _ySource, _xSource + _xDirection, _ySource + _yDirection);
                Piece _piece = BoardState.Instance.GetField(_xSource + _xDirection, _ySource + _yDirection);

                if (_piece == null)
                {
                    if (_checkSide == _caller.PieceColor || _checkSide == SideColor.Both)
                    {
                        return;
                    }
                    _path = ObjectPool.Instance.GetHighlightPath("HighlightPathYellow");
                    _position.x = _caller.transform.localPosition.x + _xDirection * BoardState.Offset;
                    _position.z = _caller.transform.localPosition.z + _yDirection * BoardState.Offset;
                    _position.y = _path.transform.localPosition.y;

                    _path.transform.localPosition = _position;
                }
                else if (_piece.PieceColor != _caller.PieceColor)
                {
                    if (_checkSide == _caller.PieceColor || _checkSide == SideColor.Both)
                    {
                        return;
                    }
                    _path = ObjectPool.Instance.GetHighlightPath("HighlightPathRed");
                    _path.GetComponent<PathPiece>().AssignPiece(_piece);
                    _position.x = _caller.transform.localPosition.x + _xDirection * BoardState.Offset;
                    _position.z = _caller.transform.localPosition.z + _yDirection * BoardState.Offset;
                    _position.y = _path.transform.localPosition.y;

                    _path.transform.localPosition = _position;

                }
            }
        }

        public static void PassantSpot(Piece _target, int _xPosition, int _yPosition)
        {
            GameObject _path;
            Vector3 _position = new Vector3();

            _path = ObjectPool.Instance.GetHighlightPath("HighlightPathRed");
            _path.GetComponent<PathPiece>().AssignPiece(_target);
            _position.x = _xPosition * BoardState.Offset; 
            _position.z = _yPosition * BoardState.Offset; 
            _position.y = _path.transform.localPosition.y;

            _path.transform.localPosition = _position;
        }

        /// <summary>
        /// Checks if there is a piece that can be castled with at target location and if that castle action would result in check for turn player.
        /// </summary>
        public static void CastleSpot(Piece _caller, Piece _target)
        {
            if (GameManager.Instance.CheckedSide == _caller.PieceColor)
            {
                return;
            }

            int _xCaller = (int)(_caller.transform.localPosition.x / BoardState.Offset);
            int _yCaller = (int)(_caller.transform.localPosition.z / BoardState.Offset);
            int _xTarget = (int)(_target.transform.localPosition.x / BoardState.Offset);
            int _yTarget = (int)(_target.transform.localPosition.z / BoardState.Offset);

            _yCaller+= _yTarget > _yCaller ? 1 : -1;
            while (_yCaller != _yTarget)
            {
                if (BoardState.Instance.GetField(_xCaller, _yCaller) != null)
                {
                    return;
                }
                _yCaller += _yTarget > _yCaller ? 1 : -1;
            }

            _yCaller = (int)(_caller.transform.localPosition.z / BoardState.Offset);
            int _yMedian = (int)Mathf.Ceil((_yCaller + _yTarget) / 2f);

            if(BoardState.Instance.SimulateCheckState(_xCaller, _yCaller, _xCaller, _yMedian) == _caller.PieceColor && _caller is King)
            {
                return;
            }
            else if (BoardState.Instance.SimulateCheckState(_xTarget, _yTarget, _xTarget, _yMedian) == _caller.PieceColor && _target is King)
            {
                return;
            }

            Vector3 _position = new Vector3();
            PathPiece _path = ObjectPool.Instance.GetHighlightPath("HighlightPathYellow").GetComponent<PathPiece>();
            _path.AssignCastle(_target);
            _position.x = _xTarget * BoardState.Offset;
            _position.z = _yTarget * BoardState.Offset;
            _position.y = _path.transform.localPosition.y;

            _path.transform.localPosition = _position;
        }

    }
}
