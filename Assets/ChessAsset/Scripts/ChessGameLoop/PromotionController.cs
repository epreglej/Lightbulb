using Digiphy;
using Fusion;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ChessMainLoop
{
    public class PromotionController :  NetworkBehaviour
    {
        private static PromotionController _instance;
        public static PromotionController Instance { get => _instance; }

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

        [SerializeField] private GameObject _blackPieces;
        [SerializeField] private GameObject _whitePieces;

        [SerializeField]
        private Queen _whiteQueen;
        [SerializeField]
        private Queen _blackQueen;
        [SerializeField]
        private Bishop _whiteBishop;
        [SerializeField]
        private Bishop _blackBishop;
        [SerializeField]
        private Rook _whiteRook;
        [SerializeField]
        private Rook _blackRook;
        [SerializeField]
        private Knight _whiteKnight;
        [SerializeField]
        private Knight _blackKnight;

        public void PawnPromotionMenu(SideColor color)
        {
            if (color == SideColor.White) _whitePieces.SetActive(true);
            else _blackPieces.SetActive(true);
        }

        [Rpc(sources: RpcSources.All, targets: RpcTargets.All, HostMode = RpcHostMode.SourceIsServer)]
        public void RPC_PieceSelected(int pieceIndex, int Row, int Column)
        {
            (int, int) pawnLocation = (Row, Column);
            _whitePieces.SetActive(false);
            _blackPieces.SetActive(false);

            switch(pieceIndex)
            {
                case -2:
                    GameManager.Instance.SelectedPromotion(Instantiate(_blackQueen), -2, pawnLocation);
                    break;
                case -3:
                    GameManager.Instance.SelectedPromotion(Instantiate(_whiteQueen), -3, pawnLocation);
                    break;
                case -4:
                    GameManager.Instance.SelectedPromotion(Instantiate(_blackRook), -4, pawnLocation);
                    break;
                case -5:
                    GameManager.Instance.SelectedPromotion(Instantiate(_whiteRook), -5, pawnLocation);
                    break;
                case -6:
                    GameManager.Instance.SelectedPromotion(Instantiate(_blackBishop), -6, pawnLocation);
                    break;
                case -7:
                    GameManager.Instance.SelectedPromotion(Instantiate(_whiteBishop), -7, pawnLocation);
                    break;
                case -8:
                    GameManager.Instance.SelectedPromotion(Instantiate(_blackKnight), -8, pawnLocation);
                    break;
                case -9:
                    GameManager.Instance.SelectedPromotion(Instantiate(_whiteKnight), -9, pawnLocation);
                    break;
            }
        }


    }
}