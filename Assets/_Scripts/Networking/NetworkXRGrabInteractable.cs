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
            base.OnSelectEntered(args);
            _networkObject.RequestStateAuthority();
        }

        protected override void OnSelectExited(SelectExitEventArgs args)
        {
            base.OnSelectExited(args);
            _networkObject.ReleaseStateAuthority();
        }
    }
}
