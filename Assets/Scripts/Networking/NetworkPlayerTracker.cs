using UnityEngine;
using Photon.Pun;
using Unity.XR.CoreUtils;

public class NetworkPlayerTracker : MonoBehaviour
{
    public Transform networkedHead;
    public Transform networkedLeftController;
    public Transform networkedRightController;
    public Transform networkedLeftHand;
    public Transform networkedRightHand;

    private PhotonView photonView;
    private Transform rigHead;
    private Transform rigLeftController;
    private Transform rigRightController;
    private Transform rigLeftHand;
    private Transform rigRightHand;

    private void Start()
    {
        photonView = GetComponent<PhotonView>();

        XROrigin xrOrigin = FindObjectOfType<XROrigin>();
        rigHead = xrOrigin.transform.Find("Camera Offset/Main Camera");
        rigLeftController = xrOrigin.transform.Find("Camera Offset/Left Controller");
        rigRightController = xrOrigin.transform.Find("Camera Offset/Right Controller");
        rigLeftHand = xrOrigin.transform.Find("Camera Offset/Left Hand");
        rigRightHand = xrOrigin.transform.Find("Camera Offset/Right Hand");

        if (photonView.IsMine)
        {
            foreach (Renderer networkedItemRenderer in GetComponentsInChildren<Renderer>())
            {
                networkedItemRenderer.enabled = false;
            }
        }
    }

    private void Update()
    {
        if (photonView.IsMine)
        {
            MapPosition(networkedHead, rigHead);
            MapPosition(networkedLeftController, rigLeftController);
            MapPosition(networkedRightController, rigRightController);
            MapPosition(networkedLeftHand, rigLeftHand);
            MapPosition(networkedRightHand, rigRightHand);

            if (rigLeftController.gameObject.activeSelf)
            {
                networkedLeftController.gameObject.SetActive(true);
                networkedLeftHand.gameObject.SetActive(false);
            } 
            
            if (rigLeftHand.gameObject.activeSelf)
            {
                networkedLeftController.gameObject.SetActive(false);
                networkedLeftHand.gameObject.SetActive(true);
            }

            if (rigRightController.gameObject.activeSelf)
            {
                networkedRightController.gameObject.SetActive(true);
                networkedRightHand.gameObject.SetActive(false);
            }

            if (rigRightHand.gameObject.activeSelf)
            {
                networkedRightController.gameObject.SetActive(false);
                networkedRightHand.gameObject.SetActive(true);
            }
        }
    }

    private void MapPosition(Transform target, Transform rigTransform)
    {
        target.position = new Vector3(rigTransform.position.x, rigTransform.position.y, rigTransform.position.z);
        target.rotation = new Quaternion(rigTransform.rotation.x, rigTransform.rotation.y, rigTransform.rotation.z, rigTransform.rotation.w);
    }
}
