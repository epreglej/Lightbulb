using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Digiphy
{
    public class AnchorPLacement : MonoBehaviour
    {
        [SerializeField] private OVRSpatialAnchor _anchorPrefab;
        [SerializeField] private InputActionReference _placeCommand;
        [SerializeField] private Transform _rightController;

        private void Update()
        {
            if (_placeCommand.action.WasPressedThisFrame())
            {
                CreateSpatialAnchor();
            }
            else if(OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch))
            {
                CreateSpatialAnchor();
            }
        }

        private void CreateSpatialAnchor()
        {
            OVRSpatialAnchor anchor = Instantiate(_anchorPrefab, 
                _rightController.position,
                _rightController.rotation);

            Canvas canvas = anchor.gameObject.GetComponentInChildren<Canvas>();
            TextMeshProUGUI uuid = canvas.gameObject.transform.GetChild(0).GetComponent<TextMeshProUGUI>();            
            TextMeshProUGUI savedStatusText = canvas.gameObject.transform.GetChild(1).GetComponent<TextMeshProUGUI>();

            StartCoroutine(CreateAnchor(anchor, uuid, savedStatusText));
        }

        private IEnumerator CreateAnchor(OVRSpatialAnchor anchor, TextMeshProUGUI uuid, TextMeshProUGUI savedStatus)
        {
            while(!anchor.Created && !anchor.Localized)
            {
                yield return new WaitForEndOfFrame();
            }

            uuid.text = anchor.Uuid.ToString();
        }
    }
}
