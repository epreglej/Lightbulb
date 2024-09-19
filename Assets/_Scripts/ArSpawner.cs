using Fusion;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Digiphy
{
    public class ArSpawner : Singleton<SpawnManager>
    {
        private enum RoomType
        {
            ChessTestPosition = 1,
            ChessOfficePosition = 2,
            ChessLabPosition = 3
        }

        [SerializeField] private GameObject _chessPrefab;
        [SerializeField] private GameObject _synchronizationLocationPrefab;
        [SerializeField] private RoomType _roomType;
        private NetworkRunner _runner;

        public void PlayerJoined(NetworkRunner runner, PlayerRef player)
        {
            if (player != runner.LocalPlayer) return;

            _runner = runner;
            LoadAnchors();
        }

        private async void LoadAnchors()
        {
            string uuid = PlayerPrefs.GetString(_roomType.ToString());
            Guid guid = new Guid(uuid);

            var options = new OVRSpatialAnchor.LoadOptions
            {
                Timeout = 0,
                StorageLocation = OVRSpace.StorageLocation.Local,
                Uuids = new Guid[] { guid }
            };

            OVRResult<List<OVRSpatialAnchor.UnboundAnchor>, OVRAnchor.FetchResult> oVRResult = await OVRSpatialAnchor.LoadUnboundAnchorsAsync(new List<Guid> { guid },
                            new List<OVRSpatialAnchor.UnboundAnchor>());
            foreach (OVRSpatialAnchor.UnboundAnchor anchor in oVRResult.Value)
            {
                if (anchor.Localized) OnLocalized(anchor);
                else
                {
                    var result = await anchor.LocalizeAsync();
                    if (result) OnLocalized(anchor);
                }
            };
        }

        private void OnLocalized(OVRSpatialAnchor.UnboundAnchor unboundAnchor)
        {
            Pose pose;
            unboundAnchor.TryGetPose(out pose);
            Vector3 oldRotation = pose.rotation.eulerAngles;
            Quaternion newRotation = Quaternion.Euler(
                new Vector3(0, oldRotation.y + 90, 0));
            NetworkObject synchronizationLocation = _runner.Spawn(_synchronizationLocationPrefab, pose.position, newRotation);
            NetworkObject chess = _runner.Spawn(_chessPrefab, pose.position, newRotation);
        }
    }
}
