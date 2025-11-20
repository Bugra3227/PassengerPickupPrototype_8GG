using System;
using System.Collections.Generic;
using UnityEngine;

public class BusMovement : MonoBehaviour
{
    public bool IsDragging => _dragging;
    public bool IsBlockMove { get => _isBlockMove; set => _isBlockMove = value; }

    [Header("Refs")] 
    [SerializeField] private Transform headTransform;
    [SerializeField] private Transform segmentRoot;
    [SerializeField] private BusSeats busSeats;
    [SerializeField] private BusConfig busConfig;

    [Header("Movement")] 
    [SerializeField] private float dragLerp = 10f;
    [SerializeField] private float stepInterval = 0.06f;

    [Header("Collision")] 
    [SerializeField] private LayerMask blockMask;
    [SerializeField] private float blockCheckRadius = 0.3f;

    public readonly List<Vector2Int> _cells = new();

    private GridManager _gridManager;
    private Camera _cam;
    private Plane _dragPlane;
    private bool _isBlockMove;
    private bool _dragging;

    private readonly List<Transform> _segments = new();

    private Vector2Int _pointerTargetCell;
    private float _stepTimer;

    private enum ControlMode { None, Head, Tail }
    private ControlMode _controlMode = ControlMode.None;

    private Vector2 _lastPointerPos;
    private bool _pointerMoved;

    private void OnDisable()
    {
        if (_gridManager != null)
            _gridManager.UnregisterBus(this);
    }
// Initializes the bus's position, segments, and color based on the level data.
    public void InitializeFromData(LevelData.BusData data)
    {
        _cam = Camera.main;
        _gridManager = GridManager.Instance;

        _gridManager.RegisterBus(this);

        _cells.Clear();
        _segments.Clear();

        Vector3 worldHeadPos = headTransform.position;
        Vector2Int headCell = _gridManager.WorldToGrid(worldHeadPos);

        _cells.Add(headCell);
        _segments.Add(headTransform);

        busConfig.InitializeBusBusPassageColorEnums(data.busColor);
        ApplyColorToRenderer(headTransform);

        CreateTailCells(data, headCell);

        busSeats.AddSeatPointToList();

        SnapWorldPositions();
        UpdateRotation();

        _pointerTargetCell = headCell;
    }
// Creates the tail segments of the bus according to the level data configuration.
    private void CreateTailCells(LevelData.BusData data, Vector2Int headCell)
    {
        for (int i = 0; i < data.cells.Count; i++)
        {
            if (data.cells[i] == Vector2Int.zero)
                continue;

            Vector2Int cell = headCell + data.cells[i];
            _cells.Add(cell);

            Vector3 pos = _gridManager.GridToWorld(cell.x, cell.y);
            BusSegment seg = Instantiate(busConfig.SegmentPrefab, pos, Quaternion.identity, segmentRoot);

            _segments.Add(seg.transform);
            seg.InitializeBusMovement(this);
            seg.InitializeBusSeat(busSeats);
            ApplyColorToRenderer(seg.transform);
        }
    }
// Applies the bus's configured color to the Renderer material of the segment.
    private void ApplyColorToRenderer(Transform t)
    {
        var r = t.GetComponentInChildren<Renderer>();
        if (r == null) return;

        Material m = new Material(r.material);
        m.color = busConfig.BusColor;
        r.material = m;
    }
    // Main update loop: handles drag input, checks for move blocking, and performs grid movement steps.
    private void Update()
    {
        if (_isBlockMove)
            return;

        Vector2 pointer;

        if (GetPointerDown(out pointer))
            TryStartDrag(pointer);

        if (_dragging)
        {
            if (GetPointerMoved(out pointer))
            {
                _pointerMoved = true;
                UpdatePointerTarget(pointer);
            }

            if (GetPointerUp())
                EndDrag();
        }

        if (_dragging)
        {
            _stepTimer += Time.deltaTime;
            if (_stepTimer >= stepInterval)
            {
                _stepTimer = 0;

                if (_controlMode == ControlMode.Head)
                {
                    StepFromHead();
                    VibrateLight();
                }
                else if (_controlMode == ControlMode.Tail)
                {
                    StepFromTail();
                    VibrateLight();
                }
            }
        }

        UpdateWorldPositions();
    }

    

    private bool GetPointerDown(out Vector2 pos)
    {
        if (Input.touchCount > 0)
        {
            Touch t = Input.GetTouch(0);
            if (t.phase == TouchPhase.Began)
            {
                pos = t.position;
                _lastPointerPos = pos;
                return true;
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            pos = Input.mousePosition;
            _lastPointerPos = pos;
            return true;
        }

        pos = default;
        return false;
    }

    private bool GetPointerMoved(out Vector2 pos)
    {
        if (Input.touchCount > 0)
        {
            Touch t = Input.GetTouch(0);

            if (t.phase == TouchPhase.Moved)
            {
                pos = t.position;
                return true;
            }
        }

        if (Input.GetMouseButton(0))
        {
            pos = Input.mousePosition;
            if ((pos - _lastPointerPos).sqrMagnitude > 4f)
            {
                _lastPointerPos = pos;
                return true;
            }
        }

        pos = default;
        return false;
    }

    private bool GetPointerUp()
    {
        if (Input.touchCount > 0)
        {
            Touch t = Input.GetTouch(0);
            if (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled)
                return true;
        }

        return Input.GetMouseButtonUp(0);
    }

    

    private bool TryGetPointerWorld(Vector2 screenPos, out Vector3 world)
    {
        Ray ray = _cam.ScreenPointToRay(screenPos);
        if (_dragPlane.Raycast(ray, out float d))
        {
            world = ray.GetPoint(d);
            return true;
        }

        world = Vector3.zero;
        return false;
    }

    private void TryStartDrag(Vector2 pos)
    {
        Ray ray = _cam.ScreenPointToRay(pos);
        if (!Physics.Raycast(ray, out RaycastHit hit, 100f))
            return;

        Transform tail = _segments[_segments.Count - 1];

        if (hit.transform == headTransform || hit.transform.IsChildOf(headTransform))
            _controlMode = ControlMode.Head;
        else if (hit.transform == tail || hit.transform.IsChildOf(tail))
            _controlMode = ControlMode.Tail;
        else
            return;

        _dragging = true;
        _dragPlane = new Plane(Vector3.up, hit.point);

        if (TryGetPointerWorld(pos, out Vector3 wp))
            _pointerTargetCell = _gridManager.WorldToGrid(wp);

        _pointerMoved = false;
        _stepTimer = 0;
    }

    private void UpdatePointerTarget(Vector2 screenPos)
    {
        if (!TryGetPointerWorld(screenPos, out Vector3 world))
            return;

        _pointerTargetCell = _gridManager.WorldToGrid(world);
    }

    private void StepFromHead()
    {
        Vector2Int delta = _pointerTargetCell - _cells[0];
        if (delta == Vector2Int.zero) return;

        Vector2Int dir = (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
            ? (delta.x > 0 ? Vector2Int.right : Vector2Int.left)
            : (delta.y > 0 ? Vector2Int.up : Vector2Int.down);

        if (_cells.Count > 1 && dir == (_cells[1] - _cells[0]))
            return;

        Vector2Int target = _cells[0] + dir;
        target.x = Mathf.Clamp(target.x, 0, _gridManager.Width - 1);
        target.y = Mathf.Clamp(target.y, 0, _gridManager.Height - 1);

        if (_cells.Contains(target) || IsBlockedCell(target))
            return;

        for (int i = _cells.Count - 1; i > 0; i--)
            _cells[i] = _cells[i - 1];

        _cells[0] = target;
        UpdateRotation();
    }

    private void StepFromTail()
    {
        int tailIndex = _cells.Count - 1;
        Vector2Int tailCell = _cells[tailIndex];

        Vector2Int delta = _pointerTargetCell - tailCell;
        if (delta == Vector2Int.zero) return;

        Vector2Int dir;

        if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
        {
           
            if (delta.x > 0)
                dir = Vector2Int.right;
            else
                dir = Vector2Int.left;
        }
        else
        {
           
            if (delta.y > 0)
                dir = Vector2Int.up;
            else
                dir = Vector2Int.down;
        }


        if (_cells.Count > 1 && dir == (_cells[tailIndex - 1] - tailCell))
            return;

        Vector2Int target = tailCell + dir;
        target.x = Mathf.Clamp(target.x, 0, _gridManager.Width - 1);
        target.y = Mathf.Clamp(target.y, 0, _gridManager.Height - 1);

        if (_cells.Contains(target) || IsBlockedCell(target))
            return;

        for (int i = 0; i < _cells.Count - 1; i++)
            _cells[i] = _cells[i + 1];

        _cells[tailIndex] = target;

        UpdateRotation();
    }

    private bool IsBlockedCell(Vector2Int cell)
    {
        if (_gridManager.IsCellBlocked(cell))
            return true;

        if (_gridManager.IsCellOccupiedByBus(cell, this))
            return true;

        if (blockMask == 0)
            return false;

        Vector3 world = _gridManager.GridToWorld(cell.x, cell.y) + Vector3.up * 0.1f;
        return Physics.CheckSphere(world, blockCheckRadius, blockMask);
    }

    private void EndDrag()
    {
        _dragging = false;
        _pointerMoved = false;
    }

    private void UpdateWorldPositions()
    {
        for (int i = 0; i < _cells.Count; i++)
        {
            Vector3 target = _gridManager.GridToWorld(_cells[i].x, _cells[i].y);

            _segments[i].position = Vector3.MoveTowards(
                _segments[i].position,
                target,
                Time.deltaTime * dragLerp
            );
        }
    }
// Snaps all bus segments instantly to their current target grid world positions.
    private void SnapWorldPositions()
    {
        for (int i = 0; i < _cells.Count; i++)
        {
            Vector3 pos = _gridManager.GridToWorld(_cells[i].x, _cells[i].y);
            _segments[i].position = pos;
        }
    }
// Smoothly moves all bus segments towards their current target grid world positions.
    private void UpdateRotation()
    {
        if (_cells.Count < 2) return;

        Vector2Int headDir = _cells[0] - _cells[1];
        _segments[0].rotation = Quaternion.Euler(0f, DirectionToAngle(headDir), 0f);

        for (int i = 1; i < _cells.Count; i++)
        {
            Vector2Int d = (i == _cells.Count - 1)
                ? _cells[i - 1] - _cells[i]
                : _cells[i] - _cells[i + 1];

            float a = DirectionToAngle(d);
            _segments[i].rotation = Quaternion.Euler(0f, a, 0f);
        }
    }

    private float DirectionToAngle(Vector2Int dir)
    {
        if (dir == Vector2Int.up) return 0f;
        if (dir == Vector2Int.right) return 90f;
        if (dir == Vector2Int.down) return 180f;
        if (dir == Vector2Int.left) return 270f;
        return 0f;
    }

    public static void VibrateLight() {}
}
