using Fusion;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace Digiphy
{
    public class NetworkXRGrabInteractable : XRGrabInteractable
    {
        [SerializeField] private NetworkObject _networkObject;

        protected override void OnSelectEntered(SelectEnterEventArgs args)
        {
            _networkObject.RequestStateAuthority();
            base.OnSelectEntered(args);
        }

        protected override void OnSelectExited(SelectExitEventArgs args)
        {
            _networkObject.ReleaseStateAuthority();
            base.OnSelectExited(args);
        }
    }
}
