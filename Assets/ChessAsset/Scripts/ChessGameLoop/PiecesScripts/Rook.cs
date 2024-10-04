using UnityEngine;

namespace ChessMainLoop
{
    public class Rook : Piece
    {
        [SerializeField]
        private King _king;

        public override void CreatePath()
        {
            PathManager.CreateVerticalPath(this);
            if (_king == null)
            {
                return;
            }
            if (_king.HasMoved == false && HasMoved == false)
            {
                PathManager.CastleSpot(this, _king);
            }
        }

        public override bool IsAttackingKing(int _xPosition, int _yPosition)
        {
            return CheckStateCalculator.IsAttackingKingVertical(_xPosition, _yPosition, PieceColor);
        }

        public override bool CanMove(int _xPosition, int _yPosition)
        {
            return GameEndCalculator.CanMoveVertical(_xPosition, _yPosition, PieceColor);
        }
    }
}