using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

namespace Digiphy
{
    public class ArSpawner : Singleton<SpawnManager>
    {
        [SerializeField] private ARRaycastManager _raycastManager;
        [SerializeField] private GameObject _spawnPrefab;
        [SerializeField] private Camera _camera;
        private List<ARRaycastHit> _hits = new();
        private GameObject _spawnedObject;

        private void Update()
        {
            if (Input.touchCount == 0) return;

            RaycastHit hit;
            Ray ray = _camera.ScreenPointToRay(Input.GetTouch(0).position);

            if(_raycastManager.Raycast(Input.GetTouch(0).position, _hits))
            {
                if(Input.GetTouch(0).phase == TouchPhase.Began && _spawnedObject == null)
                {
                    if(Physics.Raycast(ray, out hit))
                    {
                        Instantiate(_spawnPrefab, _hits[0].pose.position, Quaternion.identity);
                    }
                }
            }
        }
    }
}
