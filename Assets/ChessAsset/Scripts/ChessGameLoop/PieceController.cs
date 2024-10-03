using Digiphy;
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
            StartCoroutine(PieceMover(_path, _assignedEnemy, _assignedCastle));
            _activePiece.IsActive = false;
        }

        /// <summary>
        /// Moves the selected piece to target path position. Has special cases for castling.
        /// </summary>
        private IEnumerator PieceMover(PathPiece _path, Piece _assignedEnemy, Piece _assignedCastle)
        {
            int _xPiece = (int)(_activePiece.transform.localPosition.x / BoardState.Offset);
            int _yPiece = (int)(_activePiece.transform.localPosition.z / BoardState.Offset);
            int _xPath = (int)(_path.transform.localPosition.x / BoardState.Offset);
            int _yPath = (int)(_path.transform.localPosition.z / BoardState.Offset);

            Vector3 _targetPosition=new Vector3();

            //If its target isnt castling position does the regular piece moving.
            if (_assignedCastle == false)
            {
                SideColor _checked = BoardState.Instance.SimulateCheckState(_xPiece, _yPiece, _xPath, _yPath);
                GameManager.Instance.CheckedSide = _checked;

                _activePiece.Move(_xPath, _yPath);

                _targetPosition.x = _xPath * BoardState.Offset;
                _targetPosition.y = _activePiece.transform.localPosition.y;
                _targetPosition.z = _yPath * BoardState.Offset;
                AnimationManager.Instance.MovePiece(_activePiece, _targetPosition, _assignedEnemy);
                while (AnimationManager.Instance.IsActive == true)
                {
                    yield return new WaitForSeconds(0.01f);
                }
            }
            //If target is a castling position performs special castling action. Position calculations are done differently if the target is a King or a Rook
            else
            {
                int _yMedian = (int)Mathf.Ceil((_yPiece + _yPath) / 2f);
                SideColor _checked;

                if (_activePiece is King)
                {
                    _activePiece.Move(_xPath, _yMedian);
                    _targetPosition.x = _xPath * BoardState.Offset;
                    _targetPosition.y = _activePiece.transform.localPosition.y;
                    _targetPosition.z = _yMedian * BoardState.Offset;
                    AnimationManager.Instance.MovePiece(_activePiece, _targetPosition, null);
                    while (AnimationManager.Instance.IsActive == true)
                    {
                        yield return new WaitForSeconds(0.01f);
                    }

                    _checked = BoardState.Instance.SimulateCheckState(_xPath, _yPath, _xPath, _yMedian > _yPiece ? _yMedian - 1 : _yMedian + 1);

                    _assignedCastle.Move(_xPath, _yMedian > _yPiece ? _yMedian - 1 : _yMedian + 1);
                    _targetPosition.x = _xPath * BoardState.Offset;
                    _targetPosition.y = _activePiece.transform.localPosition.y;
                    _targetPosition.z = (_yMedian > _yPiece ? _yMedian - 1 : _yMedian + 1) * BoardState.Offset;
                    AnimationManager.Instance.MovePiece(_assignedCastle, _targetPosition, null);
                    while (AnimationManager.Instance.IsActive == true)
                    {
                        yield return new WaitForSeconds(0.01f);
                    }

                }
                else
                {
                    _activePiece.Move(_xPath, _yMedian > _yPiece ? _yMedian + 1 : _yMedian - 1);
                    _targetPosition.x = _xPath * BoardState.Offset;
                    _targetPosition.y = _activePiece.transform.localPosition.y;
                    _targetPosition.z = (_yMedian > _yPiece ? _yMedian + 1 : _yMedian - 1) * BoardState.Offset;
                    AnimationManager.Instance.MovePiece(_activePiece, _targetPosition, null);
                    while (AnimationManager.Instance.IsActive == true)
                    {
                        yield return new WaitForSeconds(0.01f);
                    }

                    _checked = BoardState.Instance.SimulateCheckState(_xPath, _yPath, _xPath, _yMedian);
                    _assignedCastle.Move(_xPath, _yMedian);

                    _targetPosition.x = _xPath * BoardState.Offset;
                    _targetPosition.y = _activePiece.transform.localPosition.y;
                    _targetPosition.z = _yMedian * BoardState.Offset;
                    AnimationManager.Instance.MovePiece(_assignedCastle, _targetPosition, null);
                    while (AnimationManager.Instance.IsActive == true)
                    {
                        yield return new WaitForSeconds(0.01f);
                    }
                }

                GameManager.Instance.CheckedSide = _checked;            
                GameManager.Instance.Passantable = null;
            }
            _activePiece = null;
            GameManager.Instance.IsPieceMoving = false;
            //GameManager.Instance.ChangeTurn();
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