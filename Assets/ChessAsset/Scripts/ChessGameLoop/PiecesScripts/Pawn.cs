using UnityEngine;

namespace ChessMainLoop
{
    public class Pawn : Piece
    {
        public override void CreatePath()
        {
            int _xSource = (int)(transform.localPosition.x / BoardState.Offset);
            int _ySource = (int)(transform.localPosition.z / BoardState.Offset);

            int _direction = PieceColor == SideColor.Black ? 1 : -1;

            //Following two sections check if there are enemy pieces diagonally in looking direction of the pawn, and if there are creates attack path under them
            if (BoardState.Instance.IsInBorders(_xSource + _direction, _ySource + 1) == true)
            {
                if (BoardState.Instance.GetField(_xSource + _direction, _ySource + 1) != null ? BoardState.Instance.GetField(_xSource + _direction, _ySource + 1).PieceColor != PieceColor : false)
                {
                    PathCalculator.PathOneSpot(this, _direction, 1);
                }
            }

            if (BoardState.Instance.IsInBorders(_xSource + _direction, _ySource - 1) == true)
            {
                if (BoardState.Instance.GetField(_xSource + _direction, _ySource - 1) != null ? BoardState.Instance.GetField(_xSource + _direction, _ySource - 1).PieceColor != PieceColor : false)
                {
                    PathCalculator.PathOneSpot(this, _direction, -1);
                }
            }

            //Following two sections check left and right of the pawn for enemy pawns, and if they are passantable creates attack field behind them
            if (BoardState.Instance.IsInBorders(_xSource, _ySource + 1) == true)
            {
                if (BoardState.Instance.GetField(_xSource, _ySource + 1) != null ? BoardState.Instance.GetField(_xSource, _ySource + 1) == GameManager.Instance.Passantable : false)
                {
                    PathCalculator.PassantSpot(BoardState.Instance.GetField(_xSource, _ySource + 1), _xSource + _direction, _ySource + 1);
                }
            }

            if (BoardState.Instance.IsInBorders(_xSource, _ySource - 1) == true)
            {
                if (BoardState.Instance.GetField(_xSource, _ySource - 1) != null ? BoardState.Instance.GetField(_xSource, _ySource - 1) == GameManager.Instance.Passantable : false)
                {
                    if (BoardState.Instance.GetField(_xSource, _ySource - 1) is Pawn ? GameManager.Instance.Passantable : false)
                        PathCalculator.PassantSpot(BoardState.Instance.GetField(_xSource, _ySource - 1), _xSource + _direction, _ySource - 1);
                }
            }

            //Following sections check if path forward in the direction pawn is facing is empty for one and two spaces, and if they are creates walk path them.
            if (BoardState.Instance.IsInBorders(_xSource + _direction, _ySource)== false)
            {
                return;
            }

            if (BoardState.Instance.GetField(_xSource + _direction, _ySource) != null) 
            {
                return;
            }

            PathCalculator.PathOneSpot(this, _direction, 0);

            if (BoardState.Instance.IsInBorders(_xSource + _direction * 2, _ySource) == false)
            {
                return;
            }

            if (HasMoved == false && BoardState.Instance.GetField(_xSource + _direction * 2, _ySource) == null)
            {
                PathCalculator.PathOneSpot(this, _direction * 2, 0);
            }
       
        }

        /// <summary>
        /// Adds checks for making the piece passantable if it moved for two sapces and promoting the pawn if it reached the end of the board to Move method of base class.
        /// </summary>
        protected override void Move(int newRow, int newColumn)
        {
            int row = _row;
            base.Move(newRow, newColumn);


            if (Mathf.Abs(row - newRow) == 2)
            {
                GameManager.Instance.Passantable = this;
            }

            if (newRow == 0 || newRow == BoardState.Instance.BoardSize - 1)
            {
                GameManager.Instance.PawnPromoting(this);
            }
    
        }

        public override bool IsAttackingKing(int _xPosition, int _yPosition)
        {
            int _direction = PieceColor == SideColor.Black ? 1 : -1;

            if (CheckStateCalculator.KingAtLocation(_xPosition, _yPosition, _direction, 1, PieceColor))
            {
                return true;            
            }

            if (CheckStateCalculator.KingAtLocation(_xPosition, _yPosition, _direction, -1, PieceColor))
            {
                return true;
            }        

            return false;
        }

        public override bool CanMove(int _xPosition, int _yPosition)
        {
            int _direction = PieceColor == SideColor.Black ? 1 : -1;

            //Following two sections perform checks if there are attackable units diagonally in looking direction of the pawn, and if moving to them would not resolve in a check for turn player
            if (BoardState.Instance.IsInBorders(_xPosition + _direction, _yPosition + 1))
            {
                if (BoardState.Instance.GetField(_xPosition + _direction, _yPosition + 1) != null ?
                    BoardState.Instance.GetField(_xPosition + _direction, _yPosition + 1).PieceColor != PieceColor : false)
                {
                    if (GameEndCalculator.CanMoveToSpot(_xPosition, _yPosition, _direction, 1, PieceColor))
                    {
                        return true;
                    }
                }
            }

            if (BoardState.Instance.IsInBorders(_xPosition + _direction, _yPosition - 1))
            {
                if (BoardState.Instance.GetField(_xPosition + _direction, _yPosition - 1) != null ?
                BoardState.Instance.GetField(_xPosition + _direction, _yPosition - 1).PieceColor != PieceColor : false)
                {
                    if (GameEndCalculator.CanMoveToSpot(_xPosition, _yPosition, _direction, -1, PieceColor))
                    {
                        return true;
                    }
                }
            }

            //Following sections check if one and two spaces in looking direction of the pawn are awailable for moving to
            if (BoardState.Instance.IsInBorders(_xPosition + _direction, _yPosition) == false)
            {
                return false;
            }

            if (BoardState.Instance.GetField(_xPosition + _direction, _yPosition) != null)
            {
                return false;
            }

            if(GameEndCalculator.CanMoveToSpot(_xPosition, _yPosition, _direction, 0, PieceColor))
            {
                return true;
            }

            if (BoardState.Instance.IsInBorders(_xPosition + _direction, _yPosition) == false)
            {
                return false;
            }

            if (BoardState.Instance.GetField(_xPosition + _direction * 2, _yPosition) != null)
            {
                return false;
            }

            if (GameEndCalculator.CanMoveToSpot(_xPosition, _yPosition, _direction * 2, 0, PieceColor))
            {
                return true;
            }

            return false;
        }
    }
}
