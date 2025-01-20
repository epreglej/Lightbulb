using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static FindSpawnPositions;

public class LampSpawner : NetworkBehaviour
{
    [SerializeField] GameObject lampPrefab;
    [SerializeField] Transform location;

    [Networked] public bool isLampSpawned { get; set; } = false;

    public void PlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (!isLampSpawned) {
            runner.Spawn(lampPrefab, location.position, location.rotation);
            isLampSpawned = true;
        }
        
    }
}
