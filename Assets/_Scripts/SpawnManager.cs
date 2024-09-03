using Fusion.Addons.ConnectionManagerAddon;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Digiphy
{
    public class SpawnManager : Singleton<SpawnManager>
    {
        [SerializeField] private GameObject _gamesPrefab;

        public void Init()
        {
            base.Init();
            ConnectionManager.Instance.runner.Spawn(_gamesPrefab, transform.position, transform.rotation);
        }
    }
}
