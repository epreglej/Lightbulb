using Oculus.Interaction;
using UnityEngine;

namespace ChessMainLoop
{
    public delegate void Selected(Piece self);
    /// <summary>
    /// Represents side colors of players.
    /// </summary>
    public enum SideColor
    {
        White,
        Black,
        None,
        Both
    }

    public abstract class Piece : MonoBehaviour
    {
        #region Private fields and corresponding public properties
        [SerializeField] private SideColor _pieceColor;
        [SerializeField] private Renderer _renderer;
        [SerializeField] private Animator _animator;
        [SerializeField] private Grabbable _grabbable;
        [SerializeField] protected int _row;
        [SerializeField] protected int _column;
        private bool _isActive = false;
        private Color _startColor;
        private (int Row, int Column) _startLocation;
        private bool _hasMoved = false;
        private PathPiece _assignedAsCastle = null;
        private PathPiece _assignedAsEnemy = null;
        private Pawn _wasPawn = null;

        public SideColor PieceColor { get => _pieceColor; }
        public (int Row, int Column) Location => (_row, _column); 
        public Animator Animator => _animator;
        public bool IsActive { get => _isActive; set { _isActive = false; _renderer.material.color = _startColor; } }
        public bool HasMoved { get => _hasMoved; set => _hasMoved = value; }
        public PathPiece AssignedAsEnemy { get => _assignedAsEnemy; set => _assignedAsEnemy = value; }
        public PathPiece AssignedAsCastle { get => _assignedAsCastle; set { _assignedAsCastle = value; _renderer.material.color = _startColor; } }
        public Pawn WasPawn { get => _wasPawn; set => _wasPawn = value; }
        #endregion


        public static event Selected Selected;

        public abstract void CreatePath();
        public abstract bool IsAttackingKing(int row, int column);
        public abstract bool CanMove(int row, int column);

        private void Start()
        {
            try
            {
                _animator.runtimeAnimatorController = AnimationManager.Instance.Assign(this);
            }
            catch
            {
                Debug.LogError("Couldn't find animator for a piece.");
                Destroy(this);
            }
            _grabbable.WhenPointerEventRaised += ProcessPointerEvent;
            _startLocation = (_row, _column);
            _startColor = _renderer.material.color;
        }

        private void OnMouseEnter() => PieceHowered();

        private void OnMouseExit() => HoverEnd();

        private void OnMouseDown() => PieceSelected();

        public void ProcessPointerEvent(PointerEvent evt)
        {
            switch (evt.Type)
            {
                case PointerEventType.Hover:
                    PieceHowered();
                    break;
                case PointerEventType.Unhover:
                    HoverEnd();
                    break;
                case PointerEventType.Select:
                    PieceSelected();
                    break;
                case PointerEventType.Unselect:
                    break;
                case PointerEventType.Move:
                    break;
                case PointerEventType.Cancel:
                    break;
            }
        }        

        //If its turn players piece sets it as selected and sets path pieces for it. If the piece is target of enemy or castle calls select method of path object this piece is assing to.
        public void PieceSelected()
        {
            if (_isActive == false && GameManager.Instance.TurnPlayer == _pieceColor && _assignedAsCastle == false 
                && GameManager.Instance.IsPieceMoving == false && GameManager.Instance.IsPromotingPawn == false && GameManager.Instance.IsPlayerTurn) 
            {
                _isActive = true;
                Selected?.Invoke(this);
                CreatePath();
                _renderer.material.color = Color.yellow;
            }
            else if (_assignedAsEnemy)
            {
                _assignedAsEnemy.Selected();
            }
            else if (_assignedAsCastle)
            {
                _assignedAsCastle.Selected();
            }
        }

        public void PieceHowered()
        {
            if (GameManager.Instance.TurnPlayer == _pieceColor && PieceController.Instance.AnyActive == false && GameManager.Instance.IsPlayerTurn)
            {
                _renderer.material.color = Color.yellow;
            }
            else if (_assignedAsEnemy)
            {
                _renderer.material.color = Color.red;
            }
            else if (_assignedAsCastle)
            {
                _renderer.material.color = Color.yellow;
            }
        }

        public void HoverEnd()
        {
            if ((_isActive == false) || _assignedAsEnemy || _assignedAsCastle)
            {
                _renderer.material.color = _startColor;
            }

        }

        public void Die()
        {
            if (BoardState.Instance.GetField(_row, _column) == this)
            {
                BoardState.Instance.ClearField(_row, _column);
            }
            ObjectPool.Instance.AddPiece(this);
        }

        public void ResetPiece()
        {
            _row = _startLocation.Row; 
            _column = _startLocation.Column;
            _renderer.material.color = _startColor;
            _wasPawn = null;
            _hasMoved = false;
        }

        public virtual void Move(int newRow, int newColumn)
        {
            MoveTracker.Instance.AddMove(_row, _column, newRow, newColumn, GameManager.Instance.TurnCount);
            if (this is Pawn && GameManager.Instance.Passantable)
            {
                int _direction = PieceColor == SideColor.Black ? 1 : -1;

                if (_column == GameManager.Instance.Passantable.Location.Column)
                {
                    if (_row  == GameManager.Instance.Passantable.Location.Row && _row != newRow)
                    {
                        MoveTracker.Instance.AddMove(newRow - _direction, newColumn, -1, -1, GameManager.Instance.TurnCount);
                    }
                }
            }

            BoardState.Instance.SetField(this, newRow, newColumn);
            _row = newRow;
            _column = newColumn;

            _hasMoved = true;
            GameManager.Instance.Passantable = null;
        }

        public void PiecePromoted(Pawn promotingPawn)
        {
            WasPawn = promotingPawn;
            HasMoved = true;
            _row = promotingPawn.Location.Row;
            _column = promotingPawn.Location.Column;
            transform.localPosition = promotingPawn.transform.localPosition;
            transform.localPosition = new Vector3(transform.localPosition.x, 0, transform.localPosition.z);
        }
    }
}
