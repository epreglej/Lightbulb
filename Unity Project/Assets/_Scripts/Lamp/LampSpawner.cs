using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static FindSpawnPositions;

public class LampSpawner : MonoBehaviour
{
    [SerializeField] GameObject lampPrefab;
    [SerializeField] Transform location;

    public void PlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (player == runner.LocalPlayer)
        {
            var obj = runner.Spawn(lampPrefab, location.position, location.rotation);
            obj.transform.parent = location;
        }
    }
}
