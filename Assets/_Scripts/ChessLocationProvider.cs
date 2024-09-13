using Fusion;

namespace Digiphy
{
    public class ChessLocationProvider : NetworkBehaviour
    {
        public override void Spawned()
        {
            if (VrRoomSyncronizer.Instance == null) return;

            VrRoomSyncronizer.Instance.SyncronizeRoomWithChess(transform);
        }
    }
}
