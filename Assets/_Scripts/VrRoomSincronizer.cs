using System.Collections.Generic;
using UnityEngine;

namespace Digiphy
{
    public class VrRoomSyncronizer : Singleton<VrRoomSyncronizer>
    {
        [SerializeField] private Transform _labModel;
        [SerializeField] private Transform _player;
        [SerializeField] private Transform _playerAnchor;
        [SerializeField] private Transform _chessExpectedLocation;

        public void SyncronizeRoomWithChess(Transform chess)
        {
            //Vector3 displacement = chess.position - _chessExpectedLocation.position;

            _labModel.parent = _chessExpectedLocation;

            _chessExpectedLocation.transform.rotation = chess.rotation;
            _chessExpectedLocation.transform.Rotate(0, 180, 0);
            _chessExpectedLocation.transform.position = chess.position;
            _player.position = new Vector3(_playerAnchor.position.x, _player.position.y, _playerAnchor.position.z);
            _player.Rotate(chess.transform.rotation.eulerAngles);

            _labModel.parent = null;
        }
    }
}
