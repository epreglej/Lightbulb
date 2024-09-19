using Fusion;

namespace Digiphy
{
    public class SynchronizationLocationProvider : NetworkBehaviour
    {
        public override void Spawned()
        {
            if (VrRoomSynchronizer.Instance == null) return;

            VrRoomSynchronizer.Instance.SynchronizeRoomWithAr(transform);
        }
    }
}
