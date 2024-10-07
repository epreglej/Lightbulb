using Digiphy;
using Fusion;
using System.Collections;
using UnityEngine;

namespace ChessMainLoop
{
    public class PieceController : Singleton<PieceController>
    {
        private Piece _activePiece;
        public bool AnyActive { get => _activePiece != null; }

        public static event PieceMoved PieceMoved;

        void OnEnable()
        {
            _activePiece = null;
            Piece.Selected += PieceSelected;
            PathPiece.PathSelect += PathSelected;
        }

        void OnDisable()
        {
            Piece.Selected -= PieceSelected;
            PathPiece.PathSelect -= PathSelected;
        }

        /// <summary>
        /// Upon selecting path to move selected piece to starts the moving coroutine and clears all active paths
        /// </summary>
        private void PathSelected(PathPiece _path)
        {
            Piece _assignedEnemy = _path.AssignedPiece;
            Piece _assignedCastle = _path.AssignedCastle;
            GameManager.Instance.IsPieceMoving = true;
            if (_assignedCastle != null)
            {
                _path.AssignedCastle.AssignedAsCastle = null;
            }        
            PieceMoved?.Invoke();
            if (_assignedCastle) StartCoroutine(PieceCastleMover(_path, _assignedEnemy, _assignedCastle));
            else StartCoroutine(PieceRegularMover(_path, _assignedEnemy));
            _activePiece.IsActive = false;
        }

        /// <summary>
        /// Moves the selected piece to target path position. Has special cases for castling.
        /// </summary>
        private IEnumerator PieceRegularMover(PathPiece _path, Piece _assignedEnemy)
        {
            int oldRow = _activePiece.Location.Row;
            int oldColumn = _activePiece.Location.Column;
            int newRow = _path.Location.Row;
            int newColumn = _path.Location.Column;

            Vector3 _targetPosition = new Vector3();

            SideColor _checked = BoardState.Instance.SimulateCheckState(oldRow, oldColumn, newRow, newColumn);
            GameManager.Instance.CheckedSide = _checked;

            _activePiece.Move(newRow, newColumn);

            _targetPosition.x = newRow * BoardState.Offset;
            _targetPosition.y = _activePiece.transform.localPosition.y;
            _targetPosition.z = newColumn * BoardState.Offset;
            AnimationManager.Instance.MovePiece(_activePiece, _targetPosition, _assignedEnemy);            
            while (AnimationManager.Instance.IsActive == true)
            {
                yield return null;
            }
            _activePiece = null;
            GameManager.Instance.IsPieceMoving = false;
            GameManager.Instance.ChangeTurn();
        }

        private IEnumerator PieceCastleMover(PathPiece _path, Piece _assignedEnemy, Piece _assignedCastle)
        {
            int oldRow = _path.Location.Row;
            int oldColumn = _activePiece.Location.Column;
            int pathRow = _path.Location.Row;
            int pathColumn = _path.Location.Column;

            Vector3 targetPositionKing = new Vector3();
            Vector3 targetPositionRook = new Vector3();


            //If target is a castling position performs special castling action. Position calculations are done differently if the target is a King or a Rook          
            int columnMedian = (int)Mathf.Ceil((oldColumn + pathColumn) / 2f);
            int rookNewColumn = columnMedian > oldColumn ? columnMedian - 1 : columnMedian + 1;
            SideColor checkedSide;

            targetPositionKing.x = pathRow * BoardState.Offset;
            targetPositionKing.y = _activePiece.transform.localPosition.y;
            targetPositionKing.z = columnMedian * BoardState.Offset;

            targetPositionRook.x = pathRow * BoardState.Offset;
            targetPositionRook.y = _activePiece.transform.localPosition.y;
            targetPositionRook.z = rookNewColumn * BoardState.Offset;

            Piece king = _activePiece is King ? _activePiece : _assignedCastle;
            Piece rook = _activePiece is Rook ? _activePiece : _assignedCastle;

            king.Move(pathRow, columnMedian);
            AnimationManager.Instance.MovePiece(king, targetPositionKing, null);
            while (AnimationManager.Instance.IsActive == true)
            {
                yield return null;
            }

            checkedSide = BoardState.Instance.SimulateCheckState(oldRow, rook.Location.Column, pathRow, rookNewColumn);

            rook.Move(pathRow, rookNewColumn);
            AnimationManager.Instance.MovePiece(rook, targetPositionRook, null);
            while (AnimationManager.Instance.IsActive == true)
            {
                yield return null;
            }            

            GameManager.Instance.CheckedSide = checkedSide;
            GameManager.Instance.Passantable = null;
           
            _activePiece = null;
            GameManager.Instance.IsPieceMoving = false;
            GameManager.Instance.ChangeTurn();
        }

        /// <summary>
        /// Replaces status of selected piece with newly selected piece.
        /// </summary>
        private void PieceSelected(Piece _piece)
        {
            if (_activePiece)
            {
                _activePiece.IsActive = false;
                PieceMoved?.Invoke();
            }

            _activePiece = _piece;
        }
    }
}