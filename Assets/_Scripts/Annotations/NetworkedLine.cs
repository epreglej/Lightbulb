using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class NetworkedLine : NetworkBehaviour
{
    [Networked, Capacity(500)]
    [OnChangedRender(nameof(PointAdded))]
    private NetworkLinkedList<Vector3> _points => default;
    private LineRenderer _lineRenderer;

    // Start is called before the first frame update
    public override void Spawned()
    {
        _lineRenderer = GetComponent<LineRenderer>();
        _lineRenderer.positionCount = 0;
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
        if (_lineRenderer.positionCount != 0)
        {
            _lineRenderer.SetPosition(_lineRenderer.positionCount - 1, _points[_lineRenderer.positionCount - 1]);
        }
        while (_lineRenderer.positionCount < _points.Count)
        {
            _lineRenderer.positionCount++;
            _lineRenderer.SetPosition(_lineRenderer.positionCount - 1, _points[_lineRenderer.positionCount - 1]);
        }
    }
}
