using Fusion;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Digiphy;

public class AnnotationGenerator : Singleton<AnnotationGenerator>
{
    [SerializeField] private InputActionReference _drawButton;
    [SerializeField] private InputActionReference _eraseButton;
    [SerializeField] private Transform _pointer;
    [SerializeField] private GameObject _linePrefab;

    private bool _drawing = false;
    private bool _erasing = false;
    private NetworkedLine _currentLine;
    private List<LineRenderer> _lines = new();
    private Vector3 _lastPosition;
    private NetworkRunner _runner = null;

    public void Init(NetworkRunner runner)
    {
        _runner = runner;
        _drawButton.action.actionMap.Enable();
    }

    private void Update()
    {
        if (_drawing)
        {
            _currentLine.UpdatePoint(_pointer.position);
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
                        var networkedObject = line.GetComponent<NetworkObject>();
                        _runner.Despawn(networkedObject);
                        break;
                    }
                }
            }
        }
    }

    void ExtendLine(Vector3 newPosition)
    {
        _lastPosition = newPosition;
        _currentLine.AddPoint(_lastPosition);
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
        if(!_erasing && _runner)
        {
            _drawing = true;
            _lastPosition = _pointer.position;

            var prefab = _runner.Spawn(_linePrefab);
            _currentLine = prefab.GetComponent<NetworkedLine>();
            _currentLine.AddPoint(_lastPosition);
            _currentLine.AddPoint(_lastPosition);

            _lines.Add(_currentLine.GetComponent<LineRenderer>());
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
