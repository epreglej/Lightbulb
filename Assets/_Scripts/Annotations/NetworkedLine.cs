using Fusion;
using UnityEngine;
using Unity.XRTools.Rendering;


[RequireComponent(typeof(XRLineRenderer))]
[RequireComponent(typeof(LineRenderer))]
public class NetworkedLine : NetworkBehaviour
{
    [Networked, Capacity(500)]
    [OnChangedRender(nameof(PointAdded))]
    public NetworkLinkedList<Vector3> Points => default;
    private XRLineRenderer _XRLineRenderer;
    private LineRenderer _lineRenderer;

    public bool usingArRenderer = false;

    // Start is called before the first frame update
    public override void Spawned()
    {
        _XRLineRenderer = GetComponent<XRLineRenderer>();
        _XRLineRenderer.SetVertexCount(0);
        _lineRenderer = GetComponent<LineRenderer>();
        _lineRenderer.positionCount = 0;
        PointAdded();
    }

    public void UpdatePoint(Vector3 point)
    {
        Points.Set(Points.Count - 1, point);
    }

    public void AddPoint(Vector3 point)
    {
        Points.Add(point);
    }

    private void PointAdded()
    {
        if(usingArRenderer)
        {
            var currentVertexCount = _XRLineRenderer.GetVertexCount();
            if (currentVertexCount != 0)
            {
                _XRLineRenderer.SetPosition(currentVertexCount - 1, Points[currentVertexCount - 1]);
            }
            while (currentVertexCount < Points.Count)
            {
                currentVertexCount++;
                _XRLineRenderer.SetVertexCount(currentVertexCount);
                _XRLineRenderer.SetPosition(currentVertexCount - 1, Points[currentVertexCount - 1]);
            }
        }
        else
        {
            if (_lineRenderer.positionCount != 0)
            {
                _lineRenderer.SetPosition(_lineRenderer.positionCount - 1, Points[_lineRenderer.positionCount - 1]);
            }
            while (_lineRenderer.positionCount < Points.Count)
            {
                _lineRenderer.positionCount++;
                _lineRenderer.SetPosition(_lineRenderer.positionCount - 1, Points[_lineRenderer.positionCount - 1]);
            }
        }

    }
}
