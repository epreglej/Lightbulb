using Photon.Pun;
using UnityEngine.XR.Interaction.Toolkit;

public class NetworkXRGrabInteractable : XRGrabInteractable
{
    private PhotonView photonView;

    private void Start()
    {
        photonView = GetComponent<PhotonView>();
    }

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args);
        photonView.RequestOwnership();
    }
}
