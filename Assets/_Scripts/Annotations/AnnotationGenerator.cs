using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class AnnotationGenerator : MonoBehaviour
{
    [SerializeField] private InputActionReference _drawButton;
    [SerializeField] private InputActionReference _eraseButton;
    [SerializeField] private Transform _pointer;
    [SerializeField] private GameObject _linePrefab;

    private bool _drawing = false;
    private bool _erasing = false;
    private LineRenderer _lineRenderer;
    private List<LineRenderer> _lines = new();
    private Vector3 _lastPosition;

    // Start is called before the first frame update
    void Start()
    {
        _drawButton.action.actionMap.Enable();
    }

    private void Update()
    {
        if (_drawing)
        {
            _lineRenderer.SetPosition(_lineRenderer.positionCount - 1, _pointer.position);
            if(Vector3.Distance(_lastPosition, _pointer.position) > 0.02 )
                ExtendLine(_pointer.position);
        }

        if(_erasing)
        {
            foreach(var line in _lines)
            {
                Vector3[] points = new Vector3[line.positionCount];
                line.GetPositions(points);
                foreach(var point in points)
                {
                    if(Vector3.Distance(point, _pointer.position) < 0.5 )
                    {
                        _lines.Remove(line);
                        Destroy(line.gameObject);
                        break;
                    }
                }
            }
        }
    }

    void ExtendLine(Vector3 newPosition)
    {
        _lastPosition = newPosition;
        _lineRenderer.positionCount += 1;
        _lineRenderer.SetPosition(_lineRenderer.positionCount - 1, _lastPosition);
    }

    private void OnEnable()
    {
        _drawButton.action.performed += StartDrawing;
        _drawButton.action.canceled += StopDrawing;

        _eraseButton.action.performed += StartErasing;
        _eraseButton.action.canceled += StopErasing;
    }

    private void OnDisable()
    {
        _drawButton.action.performed -= StartDrawing;
        _drawButton.action.canceled -= StopDrawing;

        _eraseButton.action.performed -= StartErasing;
        _eraseButton.action.canceled -= StopErasing;
    }

    private void StartDrawing(InputAction.CallbackContext obj)
    {
        if(!_erasing)
        {
            _drawing = true;
            _lastPosition = _pointer.position;

            var prefab = Instantiate(_linePrefab, _lastPosition, Quaternion.identity);
            _lineRenderer = prefab.GetComponent<LineRenderer>();
            _lineRenderer.SetPosition(0, _lastPosition);
            _lineRenderer.SetPosition(1, _lastPosition);

            _lines.Add(_lineRenderer);
        }
    }

    private void StopDrawing(InputAction.CallbackContext obj)
    {
        _drawing = false;
    }

    private void StartErasing(InputAction.CallbackContext context)
    {
        if (!_drawing) _erasing = true;
    }

    private void StopErasing(InputAction.CallbackContext context)
    {
        _erasing = false;
    }
}
