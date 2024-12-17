using Fusion;
using UnityEngine;
using Unity.XRTools.Rendering;


[RequireComponent(typeof(XRLineRenderer))]
public class NetworkedLine : NetworkBehaviour
{
    [Networked, Capacity(500)]
    [OnChangedRender(nameof(PointAdded))]
    private NetworkLinkedList<Vector3> _points => default;
    private XRLineRenderer _XRLineRenderer;

    // Start is called before the first frame update
    public override void Spawned()
    {
        _XRLineRenderer = GetComponent<XRLineRenderer>();
        _XRLineRenderer.SetVertexCount(0);
        PointAdded();
    }

    public void UpdatePoint(Vector3 point)
    {
        _points.Set(_points.Count - 1, point);
    }

    public void AddPoint(Vector3 point)
    {
        _points.Add(point);
    }

    private void PointAdded()
    {
        var currentVertexCount = _XRLineRenderer.GetVertexCount();
        if (currentVertexCount != 0)
        {
            _XRLineRenderer.SetPosition(currentVertexCount - 1, _points[currentVertexCount - 1]);
        }
        while (currentVertexCount < _points.Count)
        {
            currentVertexCount++;
            _XRLineRenderer.SetVertexCount(currentVertexCount);
            _XRLineRenderer.SetPosition(currentVertexCount - 1, _points[currentVertexCount - 1]);
        }
    }
}
