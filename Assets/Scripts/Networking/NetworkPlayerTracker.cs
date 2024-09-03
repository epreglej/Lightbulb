using UnityEngine;
using Unity.XR.CoreUtils;
using Fusion;

public class NetworkPlayerTracker : NetworkBehaviour
{
    public Transform networkedHead;
    public Transform networkedLeftController;
    public Transform networkedRightController;
    public Transform networkedLeftHand;
    public Transform networkedRightHand;

    private Transform rigHead;
    private Transform rigLeftController;
    private Transform rigRightController;
    private Transform rigLeftHand;
    private Transform rigRightHand;

    public override void Spawned()
    {
        if (!HasStateAuthority)
        { 
            enabled = false;
            return;
        }

        XROrigin xrOrigin = FindObjectOfType<XROrigin>();
        rigHead = xrOrigin.transform.Find("Camera Offset/Main Camera");
        rigLeftController = xrOrigin.transform.Find("Camera Offset/Left Controller");
        rigRightController = xrOrigin.transform.Find("Camera Offset/Right Controller");
        rigLeftHand = xrOrigin.transform.Find("Camera Offset/Left Hand");
        rigRightHand = xrOrigin.transform.Find("Camera Offset/Right Hand");

        foreach (Renderer networkedItemRenderer in GetComponentsInChildren<Renderer>())
        {
            networkedItemRenderer.enabled = false;
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority) return;

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

    private void MapPosition(Transform target, Transform rigTransform)
    {
        target.position = new Vector3(rigTransform.position.x, rigTransform.position.y, rigTransform.position.z);
        target.rotation = new Quaternion(rigTransform.rotation.x, rigTransform.rotation.y, rigTransform.rotation.z, rigTransform.rotation.w);
    }
}
