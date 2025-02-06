using Fusion;
using UnityEngine;

namespace Digiphy
{
    public class TestingSetupManager : Singleton<TestingSetupManager>
    {
        private NetworkRunner _runner;

        public void Init(NetworkRunner runer)
        {
            _runner = runer;
        }

        public void CreateChess(GameObject chessPrefab, Transform transform)
        {
            NetworkObject chess = _runner.Spawn(chessPrefab, transform.position, transform.rotation);
            chess.transform.localScale = transform.localScale;
        }
    }
}
