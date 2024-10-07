using Fusion;
using TMPro;
using UnityEngine;

namespace ChessMainLoop
{
    public delegate void PieceMoved();

    public class GameManager : NetworkBehaviour
    {
        private int _turnCount = 0;
        public int TurnCount { get => _turnCount; }
        private SideColor _turnPlayer;
        public SideColor TurnPlayer { get => _turnPlayer; set => _turnPlayer = value; }
        private SideColor _localPlayer;
        public SideColor LocalPlayer { get => _localPlayer; set => _localPlayer = value; }
        public bool IsPlayerTurn => _localPlayer == _turnPlayer;
        private SideColor _checkedSide;
        public SideColor CheckedSide { get => _checkedSide; set => _checkedSide = Check(value); }
        private Pawn _passantable = null;
        public Pawn Passantable { get => _passantable; set => _passantable = value; }
        private Pawn _promotingPawn = null;
        [SerializeField]
        private AudioSource _checkSound;
        [SerializeField] private TextMeshPro _winnerText;
        private bool _isPieceMoving = false;
        public bool IsPieceMoving { get => _isPieceMoving; set => _isPieceMoving = value; }

        private static GameManager _instance;
        public static GameManager Instance { get => _instance; }

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

        public override void Spawned()
        {
            _checkedSide = SideColor.None;
            _turnPlayer = SideColor.White;
            if (Runner.IsSharedModeMasterClient)
            {
                _localPlayer = SideColor.White;
            }
            else
            {
                _localPlayer = SideColor.Black;
            }
        }

        /// <summary>
        /// Returns color of checked player and if there is a check plays check sound.
        /// </summary>
        /// <param name="_checkSide"></param>
        /// <returns>Color of player that is checked</returns>
        private SideColor Check(SideColor _checkSide)
        {
            if(_checkedSide == SideColor.None && _checkSide != SideColor.None)
            {
                _checkSound.Play();
            }
            return _checkSide == SideColor.Both ? _turnPlayer == SideColor.White ? SideColor.Black : SideColor.White : _checkSide;
        }

        public void ChangeTurn()
        {
            if (_turnPlayer == SideColor.White)
            {
                _turnPlayer = SideColor.Black;
            }
            else if (_turnPlayer == SideColor.Black)
            {
                _turnPlayer = SideColor.White;
            }
            SideColor _winner = BoardState.Instance.CheckIfGameOver();
            if (_winner != SideColor.None)
            {
                GameEnd(_winner);
            }
            _turnCount++;
        }

        public void GameEnd(SideColor _winner)
        {
            _winnerText.gameObject.SetActive(true);
            _winnerText.text = $"{_winner.ToString()} is the winner!!!";
            _turnPlayer = SideColor.None;
        }

        /// <summary>
        /// Resets state variables and starts a new round.
        /// </summary>
        public void Restart()
        {
            ObjectPool.Instance.ResetPieces();
            BoardState.Instance.ResetPieces();
            MoveTracker.Instance.ResetMoves();
            _turnCount = 0;
            _turnPlayer = SideColor.White;
            _checkedSide = SideColor.None;
            _passantable = null;
        }

        public void PawnPromoting(Pawn _pawn)
        {
            _promotingPawn = _pawn;
            UIManagerGameLoop.Instance.PawnPromotionMenu(_pawn.PieceColor);
        }

        /// <summary>
        /// Replaces pawn that is getting promoted with selected piece, then checks for checkmate.
        /// </summary>
        public void SelectedPromotion(Piece _piece, int _pieceIndex)
        {
            _piece.transform.parent = _promotingPawn.transform.parent;            
            _piece.transform.localScale = _promotingPawn.transform.localScale;
            BoardState.Instance.PromotePawn(_promotingPawn, _piece, _pieceIndex);

            SideColor _winner = BoardState.Instance.CheckIfGameOver();
            if (_winner != SideColor.None)
            {
                if (_turnPlayer == SideColor.White)
                {
                    _winner = SideColor.Black;
                }
                else if (_turnPlayer == SideColor.Black)
                {
                    _winner = SideColor.White;
                }

                GameEnd(_winner);
            }

            _promotingPawn = null;
        }
    }
}