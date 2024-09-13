using System.Collections;
using UnityEngine;

namespace ChessMainLoop
{
    public class AnimationManager : MonoBehaviour
    {
        #region Private animator prefabs
        [SerializeField]
        private RuntimeAnimatorController _whiteAnimator;
        [SerializeField]
        private RuntimeAnimatorController _blackAnimator;
        [SerializeField]
        private RuntimeAnimatorController _whiteKnightAnimator;
        [SerializeField]
        private RuntimeAnimatorController _blackKnightAnimator;
        [SerializeField]
        private RuntimeAnimatorController _whiteBishopAnimator;
        [SerializeField]
        private RuntimeAnimatorController _blackBishopAnimator;
        #endregion

        [SerializeField]
        private float _moveSpeed = 20f;
        [SerializeField]
        private AudioSource _moveSound;
        private bool _isActive = false;
        public bool IsActive { get => _isActive; }

        private static AnimationManager _instance;
        public static AnimationManager Instance { get => _instance; }

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

        /// <summary>
        /// Assigns animator controller to piece based on piece type.
        /// </summary>
        /// <returns>Animator controler for piece parameter</returns>
        public RuntimeAnimatorController Assign(Piece _piece)
        {
            if (_piece.PieceColor == SideColor.White)
            {
                if(_piece is Knight || _piece is King)
                {
                    return _whiteKnightAnimator;
                }

                if(_piece is Bishop)
                {
                    return _whiteBishopAnimator;
                }

                return _whiteAnimator;
            }
            else
            {
                if (_piece is Knight || _piece is King)
                {
                    return _blackKnightAnimator;
                }

                if (_piece is Bishop)
                {
                    return _blackBishopAnimator;
                }

                return _blackAnimator;
            }
        }

        public void MovePiece(Piece _piece, Vector3 _target, Piece _killTarget)
        {
            _isActive = true;
            StartCoroutine(MoveAnimation(_piece, _target, _killTarget));
        }

        /// <summary>
        /// Moves the piece to target location with root motion animations and translation.
        /// </summary>
        private IEnumerator MoveAnimation(Piece _piece, Vector3 _target, Piece _killTarget)
        {
            Animator _pieceAnimator = _piece.GetComponent<Animator>();

            //Performs animation to raise the piece and tilt it
            _pieceAnimator.SetInteger("State", 1);
            while (_pieceAnimator.GetCurrentAnimatorStateInfo(0).IsName("Travel") == false)
            {
                yield return new WaitForSeconds(0.001f);
            }

            //performs translation to target position
            _target.y = _piece.transform.localPosition.y;
            while (_piece.transform.localPosition != _target)
            {
                _piece.transform.localPosition = Vector3.MoveTowards(_piece.transform.localPosition, _target, _moveSpeed * (Time.deltaTime));
                yield return new WaitForSeconds(0.001f);
            }

            _moveSound.Play();

            //Perfoms root motion animation that puts piece back down
            _pieceAnimator.SetInteger("State", 2);
            if (_killTarget != null)
            {
                _killTarget.Die();
            }

            while (_pieceAnimator.GetCurrentAnimatorStateInfo(0).IsName("StartState") == false)
            {
                yield return new WaitForSeconds(0.001f);
            }

            _target.y = _piece.transform.localPosition.y;
            _piece.transform.localPosition = _target;
            _isActive = false;
        }
    }
}
