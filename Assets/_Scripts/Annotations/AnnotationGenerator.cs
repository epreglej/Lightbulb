using Fusion;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Digiphy;
using Unity.XRTools.Rendering;

public class AnnotationGenerator : MonoBehaviour
{
    [SerializeField] private InputActionReference _drawButton;
    [SerializeField] private InputActionReference _eraseButton;
    [SerializeField] private Transform _pointer;
    [SerializeField] private GameObject _linePrefab;
    [SerializeField] private bool _isArPlayer;

    private bool _drawing = false;
    private bool _erasing = false;
    private NetworkedLine _currentLine;
    private List<NetworkedLine> _lines = new();
    private Vector3 _lastPosition;
    private NetworkRunner _runner = null;

    public static bool IsArPlayer;

    private void Start()
    {
        _drawButton.action.actionMap.Enable();
        IsArPlayer = _isArPlayer;
    }

    public void Init(NetworkRunner runner, PlayerRef player)
    {
        if (player != runner.LocalPlayer) return;

        _runner = runner;
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
            for(int i = _lines.Count - 1; i >= 0; i--)
            {
                var line = _lines[i];
                var points = line.Points;
                foreach(var point in points)
                {
                    float distance = Vector3.Distance(point, _pointer.position);
                    if (distance < 0.1 )
                    {
                        _lines.RemoveAt(i);
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

            _lines.Add(_currentLine.GetComponent<NetworkedLine>());
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
