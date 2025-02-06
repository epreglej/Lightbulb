using Digiphy;
using Fusion;
using System.Collections;
using UnityEngine;

namespace ChessMainLoop
{
    public class PieceController : SingletonNetworked<PieceController>
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

            int oldRow = _activePiece.Location.Row;
            int oldColumn = _activePiece.Location.Column;
            int newRow = _path.Location.Row;
            int newColumn = _path.Location.Column;

            if (_assignedCastle)
            {
                RPC_CastleMove(oldRow, oldColumn, newRow, newColumn);
            }
            else
            {
                int enemyRow = _assignedEnemy ? _assignedEnemy.Location.Row : -1;
                int enemyColumn = _assignedEnemy ? _assignedEnemy.Location.Column : -1;
                RPC_RegularMove(oldRow, oldColumn, newRow, newColumn, enemyRow, enemyColumn);
            }

            _activePiece.IsActive = false;
        }

        [Rpc(sources: RpcSources.All, targets: RpcTargets.All, HostMode = RpcHostMode.SourceIsServer)]
        public void RPC_RegularMove(int oldRow, int oldColumn, int newRow, int newColumn, int enemyRow, int enemyColumn)
        {
            StartCoroutine(PieceRegularMover(oldRow, oldColumn, newRow, newColumn, enemyRow, enemyColumn));
        }

        [Rpc(sources: RpcSources.All, targets: RpcTargets.All, HostMode = RpcHostMode.SourceIsServer)]
        public void RPC_CastleMove(int oldRow, int oldColumn, int castleRow, int castleColumn)
        {
            StartCoroutine(PieceCastleMover(oldRow, oldColumn, castleRow, castleColumn));
        }

        /// <summary>
        /// Moves the selected piece to target path position. Has special cases for castling.
        /// </summary>
        private IEnumerator PieceRegularMover(int oldRow, int oldColumn, int newRow, int newColumn, int enemyRow, int enemyColumn)
        {
            Vector3 targetPosition = new Vector3();

            SideColor checkSide = BoardState.Instance.SimulateCheckState(oldRow, oldColumn, newRow, newColumn);
            GameManager.Instance.CheckedSide = checkSide;


            Piece movingPiece = BoardState.Instance.GetField(oldRow, oldColumn); 
            targetPosition.x = newRow * BoardState.Offset;
            targetPosition.y = movingPiece.transform.localPosition.y;
            targetPosition.z = newColumn * BoardState.Offset;
            Piece enemy = null;
            if(BoardState.Instance.IsInBorders(enemyRow, enemyColumn)) enemy = BoardState.Instance.GetField(enemyRow, enemyColumn);

            movingPiece.Move(newRow, newColumn);
            AnimationManager.Instance.MovePiece(movingPiece, targetPosition, enemy);            
            while (AnimationManager.Instance.IsActive == true)
            {
                yield return null;
            }

            _activePiece = null;
            GameManager.Instance.IsPieceMoving = false;
            GameManager.Instance.ChangeTurn();
        }

        private IEnumerator PieceCastleMover(int callerRow, int callerColumn, int castleRow, int castleColumn)
        {
            Vector3 targetPositionKing = new Vector3();
            Vector3 targetPositionRook = new Vector3();

            Piece firstPiece = BoardState.Instance.GetField(callerRow, callerColumn);
            Piece secondPiece = BoardState.Instance.GetField(castleRow, castleColumn);
            Piece king = firstPiece is King ? firstPiece : secondPiece;
            Piece rook = firstPiece is Rook ? firstPiece : secondPiece;

            //If target is a castling position performs special castling action. Position calculations are done differently if the target is a King or a Rook          
            int columnMedian = (int)Mathf.Ceil((king.Location.Column + rook.Location.Column) / 2f);
            int rookNewColumn = columnMedian > king.Location.Column ? columnMedian - 1 : columnMedian + 1;
            SideColor checkedSide;

            targetPositionKing.x = callerRow * BoardState.Offset;
            targetPositionKing.y = 0;
            targetPositionKing.z = columnMedian * BoardState.Offset;

            targetPositionRook.x = callerRow * BoardState.Offset;
            targetPositionRook.y = 0;
            targetPositionRook.z = rookNewColumn * BoardState.Offset;

            king.Move(callerRow, columnMedian);
            AnimationManager.Instance.MovePiece(king, targetPositionKing, null);
            while (AnimationManager.Instance.IsActive == true)
            {
                yield return null;
            }

            checkedSide = BoardState.Instance.SimulateCheckState(callerRow, rook.Location.Column, callerRow, rookNewColumn);

            rook.Move(callerRow, rookNewColumn);
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