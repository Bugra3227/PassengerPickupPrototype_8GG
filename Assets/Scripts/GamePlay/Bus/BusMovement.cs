using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[System.Serializable]
public struct BusSegmentData
{
     public Vector2Int gridPosition;
}

public class BusMovement : MonoBehaviour
{
    [Header("Bus Prefabs")]
    [SerializeField] private Transform existingHeadTransform;
    [SerializeField] private GameObject segmentPrefab;
    [SerializeField] private Transform segmentRoot;

    [Header("Initial Setup")]
    [SerializeField] private List<BusSegmentData> initialSegments = new List<BusSegmentData>();

    [Header("Appearance")]
    [SerializeField] private Color busColor;

    [Header("Movement Settings")]
    [SerializeField] private float dragLerp = 15f;
    [SerializeField] private float stepInterval = 0.06f;

    [Header("Collision")]
    [SerializeField] private LayerMask blockMask;
    [SerializeField] private float blockCheckRadius = 0.3f;

    private GridManager _gridManager;
    private Camera _camera;
    private Plane _dragPlane;

    private bool _dragging;
    private readonly List<Vector2Int> _cells = new List<Vector2Int>();
    private readonly List<Transform> _segmentTransforms = new List<Transform>();

    private Vector2Int _pointerTargetCell;
    private float _stepTimer;

    private void Start()
    {
        _camera = Camera.main;
        _gridManager = GridManager.Instance;

        if (_gridManager == null || existingHeadTransform == null)
            return;

        Vector2Int initialHeadCell = SnapHeadToGrid();
        InitializeSegments(initialHeadCell);
        SnapWorldPositionsImmediate();
        UpdateRotation();
        _pointerTargetCell = initialHeadCell;
    }

    private void Update()
    {
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
                StepTowardsPointer();
            }
        }

        UpdateWorldPositionsLerped();
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
        Ray ray = _camera.ScreenPointToRay(screenPos);

        if (_dragPlane.Raycast(ray, out float dist))
        {
            world = ray.GetPoint(dist);
            return true;
        }

        world = Vector3.zero;
        return false;
    }

    private Vector2Int SnapHeadToGrid()
    {
        Vector2Int snappedCell = _gridManager.WorldToGrid(existingHeadTransform.position);
        existingHeadTransform.position = _gridManager.GridToWorld(snappedCell.x, snappedCell.y);
        return snappedCell;
    }

    private void InitializeSegments(Vector2Int initialHeadCell)
    {
        _cells.Clear();
        _segmentTransforms.Clear();

        Vector2Int currentCellPosition = initialHeadCell;

        for (int i = 0; i < initialSegments.Count; i++)
        {
            Vector2Int inputData = initialSegments[i].gridPosition;
            Transform currentTransform;

            if (i == 0)
            {
                currentCellPosition = initialHeadCell;
                currentTransform = existingHeadTransform;
            }
            else
            {
                currentCellPosition += inputData;
                Vector3 startPos = _gridManager.GridToWorld(currentCellPosition.x, currentCellPosition.y);
                GameObject inst = Instantiate(segmentPrefab, startPos, Quaternion.identity);
                if (segmentRoot != null)
                    inst.transform.SetParent(segmentRoot, true);
                currentTransform = inst.transform;
            }

            Renderer renderer = currentTransform.GetComponentInChildren<Renderer>();
            if (renderer != null)
                renderer.material.color = busColor;

            _cells.Add(currentCellPosition);
            _segmentTransforms.Add(currentTransform);
        }
    }

    private void TryStartDrag(Vector2 screenPos)
    {
        Ray ray = _camera.ScreenPointToRay(screenPos);

        if (Physics.Raycast(ray, out RaycastHit hit, 100f))
        {
            bool isHeadOrChild = hit.transform == existingHeadTransform || hit.transform.IsChildOf(existingHeadTransform);
            if (!isHeadOrChild)
                return;

            _dragging = true;
            _dragPlane = new Plane(Vector3.up, existingHeadTransform.position);

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
        if (blockMask == 0)
            return false;

        Vector3 pos = _gridManager.GridToWorld(cell.x, cell.y) + Vector3.up * 0.1f;
        return Physics.CheckSphere(pos, blockCheckRadius, blockMask);
    }

    private void EndDrag()
    {
        _dragging = false;
    }

    private void SnapWorldPositionsImmediate()
    {
        for (int i = 0; i < _cells.Count; i++)
        {
            Vector3 pos = _gridManager.GridToWorld(_cells[i].x, _cells[i].y);
            _segmentTransforms[i].position = pos;
        }
    }

    private void UpdateWorldPositionsLerped()
    {
        for (int i = 0; i < _cells.Count; i++)
        {
            Vector3 targetPos = _gridManager.GridToWorld(_cells[i].x, _cells[i].y);
            _segmentTransforms[i].position = Vector3.Lerp(
                _segmentTransforms[i].position,
                targetPos,
                Time.deltaTime * dragLerp
            );
        }
    }

    private void UpdateRotation()
    {
        if (_cells.Count < 2) 
            return;

        // HEAD
        Vector2Int headDir = _cells[0] - _cells[1];
        _segmentTransforms[0].rotation = Quaternion.Euler(0f, DirectionToAngle(headDir), 0f);

        // TAIL
        for (int i = 1; i < _cells.Count; i++)
        {
            Vector2Int dir;

            if (i == _cells.Count - 1)
                dir = _cells[i - 1] - _cells[i];       
            else
                dir = _cells[i] - _cells[i + 1];        

            float angle = DirectionToAngle(dir);
            _segmentTransforms[i].rotation = Quaternion.Euler(0f, angle, 0f);
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

}
