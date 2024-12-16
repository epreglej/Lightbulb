using Fusion;
using System.Collections.Generic;
using Unity.XRTools.Rendering;
using UnityEngine;

[RequireComponent(typeof(XRLineRenderer))]
public class NetworkedLine : NetworkBehaviour
{
    [Networked, Capacity(500)]
    [OnChangedRender(nameof(PointAdded))]
    private NetworkLinkedList<Vector3> _points => default;

    private Mesh mesh = new();
    private List<GameObject> cylinders = new();
    private Face previousFace = new Face();
    private Face currentFace = new Face();

    private const int CylinderResolution = 8;
    
    struct Face
    {
        public Vector3 normal;
        public Vector3 up;
        public int[] verticesIndex;
    }

    // Start is called before the first frame update
    public override void Spawned()
    {
        GetComponent<MeshFilter>().mesh = mesh;
        previousFace.normal = Vector3.right;
        previousFace.up = Vector3.up;

        Vector3[] vertices = new Vector3[(CylinderResolution + 1) * 2];



        //_XRLineRenderer = GetComponent<XRLineRenderer>();
        //_XRLineRenderer.SetVertexCount(0);
        PointAdded();
    }

    private Face getFace(int index, Face previous, Vector3[] vertices)
    {
        Face face = new Face();
        face.verticesIndex = new int[CylinderResolution + 1];
        for(int i = 0; i < CylinderResolution + 1; i++)
        {
            face.verticesIndex[i] = index * (CylinderResolution + 1) + i;
        }
        face.normal = (_points[index] - _points[index - 1]).normalized;
        face.up = Vector3.ProjectOnPlane(previous.up, face.normal);
        if(face.up == Vector3.zero)
        {
            face.up = Vector3.Cross(face.normal, Vector3.up);
            if(face.up == Vector3.zero)
                face.up = Vector3.Cross(face.normal, Vector3.right);
        }

        face.up = face.up.normalized;

        return face;
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


        //var currentVertexCount = _XRLineRenderer.GetVertexCount();
        //if (currentVertexCount != 0)
        //{
        //    _XRLineRenderer.SetPosition(currentVertexCount - 1, _points[currentVertexCount - 1]);
        //}
        //while (currentVertexCount < _points.Count)
        //{
        //    currentVertexCount++;
        //    _XRLineRenderer.SetVertexCount(currentVertexCount);
        //    _XRLineRenderer.SetPosition(currentVertexCount - 1, _points[currentVertexCount - 1]);
        //}
    }
}
