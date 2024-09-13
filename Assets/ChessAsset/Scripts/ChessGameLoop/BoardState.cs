using System.Collections.Generic;
using UnityEngine;

namespace ChessMainLoop
{
    public class BoardState : MonoBehaviour
    {
        private Piece[,] _gridState;
        [SerializeField]
        private int _boardSize;
        public int BoardSize { get => _boardSize; }
        [SerializeField]
        private GameObject _blackPieces;
        [SerializeField]
        private GameObject _whitePieces;
        public static float Offset = 1.5f;

        private static BoardState _instance;
        public static BoardState Instance { get => _instance; }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(this.gameObject);
            }
            else
            {
                _instance = this;
            }
        }


        private void Start()
        {
            _gridState = new Piece[_boardSize, _boardSize];
            InitializeGrid();

        }

        public void InitializeGrid()
        {
            for(int i = 0; i < _gridState.GetLength(0); i++)
            {
                for(int j = 0; j < _gridState.GetLength(1); j++)
                {
                    _gridState[i, j] = null;
                }
            }

            foreach (Transform child in _blackPieces.transform)
            {
                int x = (int)(child.localPosition.x / Offset);
                int y = (int)(child.localPosition.z / Offset);
                _gridState[x, y] = child.GetComponent<Piece>();
            }
            foreach (Transform child in _whitePieces.transform)
            {
                int x = (int)(child.localPosition.x / Offset);
                int y = (int)(child.localPosition.z / Offset);
                _gridState[x, y] = child.GetComponent<Piece>();
            }
        }

        public Piece GetField(int _width, int _length)
        {
            try
            {
                return _gridState[_width, _length];
            }
            catch
            {
                Debug.Log("Grid index out of bounds");
                return null;
            }
        }

        public void SetField(Piece _piece, int _widthNew, int _lengthNew)
        {
            _gridState[(int)(_piece.transform.localPosition.x/ BoardState.Offset), (int)(_piece.transform.localPosition.z / BoardState.Offset)] = null;
            _gridState[_widthNew, _lengthNew] = _piece;
        }

        public void ClearField(int _width, int _length)
        {
            _gridState[_width, _length] = null;
        }

        public bool IsInBorders(int _x, int _y)
        {
            if(_x >=0 && _x < _boardSize && _y >= 0 && _y < _boardSize)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Mocks the translation of the piece to the target position and check if it would result in check.
        /// </summary>
        /// <returns>Weather translation performed on the piece would result in a check state</returns>
        public SideColor CalculateCheckState(int _xOld, int _yOld, int _xNew, int _yNew)
        {
            Piece _missplaced = _gridState[_xNew, _yNew];
            _gridState[_xNew, _yNew] = _gridState[_xOld, _yOld];
            _gridState[_xOld, _yOld] = null;

            SideColor _checkSide = CheckStateCalculator.CalculateCheck(_gridState);

            _gridState[_xOld, _yOld] = _gridState[_xNew, _yNew];
            _gridState[_xNew, _yNew] = _missplaced;

            return _checkSide;
        }

        public SideColor CheckIfGameOver()
        {
            return GameEndCalculator.CheckIfGameEnd(_gridState);
        }

        public void ResetPieces()
        {
            Piece _piece;
            Queue<Piece> _pieces = new Queue<Piece>();
            foreach (Transform child in _blackPieces.transform)
            {
                _piece = child.GetComponent<Piece>();
                if (_piece.WasPawn != null)
                {
                    _pieces.Enqueue(_piece.WasPawn);
                    _piece.WasPawn.gameObject.SetActive(true);
                }
                _pieces.Enqueue(_piece);
            }
            foreach (Transform child in _whitePieces.transform)
            {
                _piece = child.GetComponent<Piece>();
                if (_piece.WasPawn != null)
                {
                    _pieces.Enqueue(_piece.WasPawn);
                    _piece.WasPawn.gameObject.SetActive(true);
                }
                _pieces.Enqueue(_piece);
            }

            while (_pieces.Count > 0)
            {
                _piece = _pieces.Dequeue();
                if (_piece.WasPawn == null)
                {
                    _piece.ResetPosition();
                    _piece.HasMoved = false;
                }
                else
                {
                    Destroy(_piece.gameObject);
                }
            }

            InitializeGrid();
        }

        /// <summary>
        /// Replaces pawn being promoted with the selected piece.
        /// </summary>
        public void PromotePawn(Pawn _promotingPawn, Piece _piece, int _pieceIndex)
        {
            int _xPosition = (int)(_promotingPawn.transform.localPosition.x / Offset);
            int _yPosition = (int)(_promotingPawn.transform.localPosition.z / Offset);
            MoveTracker.Instance.AddMove(_xPosition, _yPosition, _pieceIndex, _pieceIndex, GameManager.Instance.TurnCount - 1);
            _gridState[_xPosition, _yPosition] = _piece;
            _piece.WasPawn = _promotingPawn;
            _piece.transform.position = _promotingPawn.transform.position;
            _piece.transform.localPosition = new Vector3(_piece.transform.localPosition.x, 0, _piece.transform.localPosition.z);
            _promotingPawn.gameObject.SetActive(false);
        }
    }
}
