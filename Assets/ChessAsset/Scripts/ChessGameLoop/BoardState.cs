using Digiphy;
using System.Collections.Generic;
using UnityEngine;

namespace ChessMainLoop
{
    public class BoardState : Singleton<BoardState>
    {
        private Piece[,] _gridState;
        [SerializeField]
        private int _boardSize;
        public int BoardSize { get => _boardSize; }
        [SerializeField]
        private List<Piece> _blackPieces;
        [SerializeField]
        private List<Piece> _whitePieces;
        [SerializeField]
        private Queue<Piece> _promotedPieces;
        public static float Offset = 1.5f;

        private void Start()
        {
            _gridState = new Piece[_boardSize, _boardSize];
            InitializeGrid();

        }

        public void InitializeGrid()
        {
            for(int i = 0; i < _boardSize; i++)
            {
                for(int j = 0; j < _boardSize; j++)
                {
                    _gridState[i, j] = null;
                }
            }

            for (int i = 0; i < _blackPieces.Count; i++) 
            {
                var location = _blackPieces[i].Location;
                _gridState[location.Row, location.Column] = _blackPieces[i];
            } 

            for(int i = 0; i < _whitePieces.Count; i++)
            {
                var location = _whitePieces[i].Location;
                _gridState[location.Row, location.Column] = _whitePieces[i];
            }
        }

        /// <summary>
        /// Retrieves current state of that fied on board
        /// </summary>
        /// <returns>Pice reference of the piece on the field, or null if not occupied</returns>
        public Piece GetField(int row, int column) => _gridState[row, column];

        public void SetField(Piece _piece, int newRow, int newColumn)
        {
            _gridState[_piece.Location.Row, _piece.Location.Column] = null;
            _gridState[newRow, newColumn] = _piece;
            _piece.Location = (newRow, newColumn);
        }

        public void ClearField(int row, int column)
        {
            _gridState[row, column] = null;
        }

        /// <summary>
        /// Checks if cooridantes are inside board borders
        /// </summary>
        public bool IsInBorders(int row, int column) => (row >= 0 && row < _boardSize && column >= 0 && column < _boardSize);

        /// <summary>
        /// Mocks the translation of the piece to the target position and check if it would result in check.
        /// </summary>
        /// <returns>Wether translation performed on the piece would result in a check state</returns>
        public SideColor SimulateCheckState(int rowOld, int columnOld, int rowNew, int columnNew)
        {
            Piece missplaced = _gridState[rowNew, columnNew];
            _gridState[rowNew, columnNew] = _gridState[rowOld, columnOld];
            _gridState[rowOld, columnOld] = null;

            SideColor checkSide = CheckStateCalculator.CalculateCheck(_gridState);

            _gridState[rowOld, columnOld] = _gridState[rowNew, columnNew];
            _gridState[rowNew, columnNew] = missplaced;

            return checkSide;
        }

        public SideColor CheckIfGameOver()
        {
            return GameEndCalculator.CheckIfGameEnd(_gridState);
        }

        public void ResetPieces()
        {
            foreach (Piece piece in _blackPieces)
            {
                piece.ResetPiece();
            }
            foreach (Piece piece in _whitePieces)
            {
                piece.ResetPiece();
            }

            while (_promotedPieces.Count > 0)
            {
                Destroy(_promotedPieces.Dequeue());
            }

            InitializeGrid();
        }

        /// <summary>
        /// Replaces pawn being promoted with the selected piece.
        /// </summary>
        public void PromotePawn(Pawn _promotingPawn, Piece _piece, int _pieceIndex)
        {
            _promotedPieces.Enqueue(_piece);
            //int _xPosition = (int)(_promotingPawn.transform.localPosition.x / Offset);
            //int _yPosition = (int)(_promotingPawn.transform.localPosition.z / Offset);
            MoveTracker.Instance.AddMove(_promotingPawn.Location.Row, _promotingPawn.Location.Column, _pieceIndex, _pieceIndex, GameManager.Instance.TurnCount - 1);
            _gridState[_promotingPawn.Location.Row, _promotingPawn.Location.Column] = _piece;
            _piece.WasPawn = _promotingPawn;
            _piece.Location = _promotingPawn.Location;
            _piece.transform.position = _promotingPawn.transform.position;
            _piece.transform.localPosition = new Vector3(_piece.transform.localPosition.x, 0, _piece.transform.localPosition.z);
            _promotingPawn.gameObject.SetActive(false);
        }
    }
}
