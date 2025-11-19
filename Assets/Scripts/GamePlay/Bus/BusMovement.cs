using System;
using System.Collections.Generic;
using UnityEngine;

public class BusMovement : MonoBehaviour
{
    public bool IsDragging => _dragging;

    public bool IsBlockMove
    {
        get => _isBlockMove;
        set => _isBlockMove = value;
    }

    [Header("Refs")] [SerializeField] private Transform headTransform;
    [SerializeField] private Transform segmentRoot;
    [SerializeField] private BusSeats busSeats;
    [SerializeField] private BusConfig busConfig;

    [Header("Movement")] [SerializeField] private float dragLerp = 15f;
    [SerializeField] private float stepInterval = 0.06f;

    [Header("Collision")] [SerializeField] private LayerMask blockMask;
    [SerializeField] private float blockCheckRadius = 0.3f;
    public readonly List<Vector2Int> _cells = new(); // world cell positions (head -> tail)

    private GridManager _gridManager;
    private Camera _cam;
    private Plane _dragPlane;
    private bool _isBlockMove;
    private bool _dragging;

    private readonly List<Transform> _segments = new(); // head + tails

    private Vector2Int _pointerTargetCell;
    private float _stepTimer;

    private enum ControlMode
    {
        None,
        Head,
        Tail
    }

    private ControlMode _controlMode = ControlMode.None;

    private void OnDisable()
    {
        _gridManager.UnregisterBus(this);
    }

    // Initializes the bus's position, segments, and color based on the level data.
    public void InitializeFromData(LevelData.BusData data)
    {
        _cam = Camera.main;
        _gridManager = GridManager.Instance;

        _gridManager.RegisterBus(this);
        Vector3 currentHeadWorld = headTransform.position;
        Vector2Int headCell = _gridManager.WorldToGrid(currentHeadWorld);

        _cells.Clear();
        _segments.Clear();


        _cells.Add(headCell);
        _segments.Add(headTransform);
        InitializeBusConfigColor(data);
        ApplyColorToRenderer(headTransform);

        CreateTailCells(data, headCell);

        busSeats.AddSeatPointToList();

        SnapWorldPositions();
        UpdateRotation();

        _pointerTargetCell = headCell;
    }

// Sets the color configuration for the bus based on the provided level data.
    private void InitializeBusConfigColor(LevelData.BusData data)
    {
        busConfig.InitializeBusBusPassageColorEnums(data.busColor);
    }

    // Creates the tail segments of the bus according to the level data configuration.
    private void CreateTailCells(LevelData.BusData data, Vector2Int headCell)
    {
        for (int i = 0; i < data.cells.Count; i++)
        {
            Vector2Int offset = data.cells[i];
            if (offset == Vector2Int.zero)
                continue;

            Vector2Int cell = headCell + offset;
            _cells.Add(cell);

            Vector3 segPos = _gridManager.GridToWorld(cell.x, cell.y);
            BusSegment seg = Instantiate(busConfig.SegmentPrefab, segPos, Quaternion.identity, segmentRoot);
            _segments.Add(seg.transform);
            seg.InitializeBusMovement(this);
            seg.InitializeBusSeat(busSeats);

            ApplyColorToRenderer(seg.transform);
        }
    }

// Applies the bus's configured color to the Renderer material of the segment.
    private void ApplyColorToRenderer(Transform vehicleMeshes)
    {
        var renderer = vehicleMeshes.GetComponentInChildren<Renderer>();
        if (renderer == null) return;

        var mat = new Material(renderer.material);
        mat.color = busConfig.BusColor;
        renderer.material = mat;
    }

    // Main update loop: handles drag input, checks for move blocking, and performs grid movement steps.
    private void Update()
    {
        if (_isBlockMove)
            return;

        Vector2 screenPos;

        if (GetPointerDown(out screenPos))
            TryStartDrag(screenPos);

        if (_dragging && GetPointerHeld(out screenPos))
            UpdatePointerTarget(screenPos);

        if (_dragging && GetPointerUp())
            EndDrag();

        if (_dragging)
        {
            _stepTimer += Time.deltaTime;
            if (_stepTimer >= stepInterval)
            {
                _stepTimer = 0f;
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

        UpdateWorldPositionsLerped();
    }
    private void StepFromHead()
    {
        Vector2Int deltaCell = _pointerTargetCell - _cells[0];
        if (deltaCell == Vector2Int.zero)
            return;

        Vector2Int moveDir;

        if (Mathf.Abs(deltaCell.x) > Mathf.Abs(deltaCell.y))
            moveDir = deltaCell.x > 0 ? Vector2Int.right : Vector2Int.left;
        else
            moveDir = deltaCell.y > 0 ? Vector2Int.up : Vector2Int.down;

        if (_cells.Count > 1)
        {
            Vector2Int backwardDir = _cells[1] - _cells[0];
            if (moveDir == backwardDir)
                return;
        }

        Vector2Int target = _cells[0] + moveDir;
        target.x = Mathf.Clamp(target.x, 0, _gridManager.Width - 1);
        target.y = Mathf.Clamp(target.y, 0, _gridManager.Height - 1);

        if (_cells.Contains(target))
            return;

        if (IsBlockedCell(target))
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

        Vector2Int deltaCell = _pointerTargetCell - tailCell;
        if (deltaCell == Vector2Int.zero)
            return;

        Vector2Int moveDir;

        if (Mathf.Abs(deltaCell.x) > Mathf.Abs(deltaCell.y))
            moveDir = deltaCell.x > 0 ? Vector2Int.right : Vector2Int.left;
        else
            moveDir = deltaCell.y > 0 ? Vector2Int.up : Vector2Int.down;

       
        if (_cells.Count > 1)
        {
            Vector2Int backwardDir = _cells[tailIndex - 1] - tailCell;
            if (moveDir == backwardDir)
                return;
        }

        Vector2Int target = tailCell + moveDir;
        target.x = Mathf.Clamp(target.x, 0, _gridManager.Width - 1);
        target.y = Mathf.Clamp(target.y, 0, _gridManager.Height - 1);

        if (_cells.Contains(target))
            return;

        if (IsBlockedCell(target))
            return;

        
        for (int i = 0; i < _cells.Count - 1; i++)
            _cells[i] = _cells[i + 1];

        _cells[tailIndex] = target;

        UpdateRotation();
    }


    private bool GetPointerDown(out Vector2 pos)
    {
        if (Input.touchCount > 0)
        {
            var t = Input.GetTouch(0);
            if (t.phase == TouchPhase.Began)
            {
                pos = t.position;
                return true;
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            pos = Input.mousePosition;
            return true;
        }

        pos = default;
        return false;
    }

    private bool GetPointerHeld(out Vector2 pos)
    {
        if (Input.touchCount > 0)
        {
            var t = Input.GetTouch(0);
            if (t.phase == TouchPhase.Moved || t.phase == TouchPhase.Stationary)
            {
                pos = t.position;
                return true;
            }
        }

        if (Input.GetMouseButton(0))
        {
            pos = Input.mousePosition;
            return true;
        }

        pos = default;
        return false;
    }

    private bool GetPointerUp()
    {
        if (Input.touchCount > 0)
        {
            var t = Input.GetTouch(0);
            if (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled)
                return true;
        }

        return Input.GetMouseButtonUp(0);
    }

    private bool TryGetPointerWorld(Vector2 screenPos, out Vector3 world)
    {
        Ray ray = _cam.ScreenPointToRay(screenPos);

        if (_dragPlane.Raycast(ray, out float dist))
        {
            world = ray.GetPoint(dist);
            return true;
        }

        world = Vector3.zero;
        return false;
    }

// Attempts to start dragging the bus if the raycast hits the bus head.
    private void TryStartDrag(Vector2 screenPos)
    {
        Ray ray = _cam.ScreenPointToRay(screenPos);

        if (Physics.Raycast(ray, out RaycastHit hit, 100f))
        {
            Transform tail = _segments[_segments.Count - 1];

            if (hit.transform == headTransform || hit.transform.IsChildOf(headTransform))
            {
                _controlMode = ControlMode.Head;
            }
            else if (hit.transform == tail || hit.transform.IsChildOf(tail))
            {
                _controlMode = ControlMode.Tail;
            }
            else
            {
                return;
            }

            _dragging = true;
            _dragPlane = new Plane(Vector3.up, hit.point);

            if (TryGetPointerWorld(screenPos, out Vector3 worldPos))
                _pointerTargetCell = _gridManager.WorldToGrid(worldPos);

            _stepTimer = 0f;
        }
    }


    private void UpdatePointerTarget(Vector2 screenPos)
    {
        if (!TryGetPointerWorld(screenPos, out Vector3 worldPos))
            return;

        _pointerTargetCell = _gridManager.WorldToGrid(worldPos);
    }

// Attempts to move the bus one grid unit closer to the target cell if conditions are met.
    private void StepTowardsPointer()
    {
        Vector2Int deltaCell = _pointerTargetCell - _cells[0];
        if (deltaCell == Vector2Int.zero)
            return;

        Vector2Int moveDir;

        if (Mathf.Abs(deltaCell.x) > Mathf.Abs(deltaCell.y))
            moveDir = deltaCell.x > 0 ? Vector2Int.right : Vector2Int.left;
        else
            moveDir = deltaCell.y > 0 ? Vector2Int.up : Vector2Int.down;
        //Block backward
        if (_cells.Count > 1)
        {
            Vector2Int backwardDir = _cells[1] - _cells[0];
            if (moveDir == backwardDir)
                return;
        }

        Vector2Int targetHeadCell = _cells[0] + moveDir;
        targetHeadCell.x = Mathf.Clamp(targetHeadCell.x, 0, _gridManager.Width - 1);
        targetHeadCell.y = Mathf.Clamp(targetHeadCell.y, 0, _gridManager.Height - 1);

        if (targetHeadCell == _cells[0])
            return;

        if (_cells.Contains(targetHeadCell))
            return;

        if (IsBlockedCell(targetHeadCell))
            return;

        for (int i = _cells.Count - 1; i > 0; i--)
            _cells[i] = _cells[i - 1];

        _cells[0] = targetHeadCell;
        UpdateRotation();
    }

    private bool IsBlockedCell(Vector2Int cell)
    {
        if (_gridManager != null)
        {
            if (_gridManager.IsCellBlocked(cell))
                return true;

            if (_gridManager.IsCellOccupiedByBus(cell, this))
                return true;
        }

        if (blockMask == 0)
            return false;

        Vector3 pos = _gridManager.GridToWorld(cell.x, cell.y) + Vector3.up * 0.1f;
        return Physics.CheckSphere(pos, blockCheckRadius, blockMask);
    }


    private void EndDrag()
    {
        _dragging = false;
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

// Smoothly moves all bus segments towards their current target grid world positions using Lerp.
    private void UpdateWorldPositionsLerped()
    {
        for (int i = 0; i < _cells.Count; i++)
        {
            Vector3 targetPos = _gridManager.GridToWorld(_cells[i].x, _cells[i].y);
            _segments[i].position = Vector3.Lerp(
                _segments[i].position,
                targetPos,
                Time.deltaTime * dragLerp
            );
        }
    }

    private void UpdateRotation()
    {
        if (_cells.Count < 2)
            return;

        Vector2Int headDir = _cells[0] - _cells[1];
        _segments[0].rotation = Quaternion.Euler(0f, DirectionToAngle(headDir), 0f);

        for (int i = 1; i < _cells.Count; i++)
        {
            Vector2Int dir;
            if (i == _cells.Count - 1)
                dir = _cells[i - 1] - _cells[i];
            else
                dir = _cells[i] - _cells[i + 1];

            float angle = DirectionToAngle(dir);
            _segments[i].rotation = Quaternion.Euler(0f, angle, 0f);
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
    //Haptic
    public static void VibrateLight()
    {
#if UNITY_ANDROID
        Handheld.Vibrate(); // Android'de bu zaten çok hafif
#endif
    }
}