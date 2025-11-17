using System.Collections.Generic;
using UnityEngine;

public class BusMovementController : MonoBehaviour
{
    [Header("Movement")] [SerializeField] private float dragLerp = 15f;

    [Header("Tail Settings")] [SerializeField]
    private GameObject tailPrefab;

    [SerializeField] private Transform tailRoot;

    [Header("Head")] [SerializeField] private Transform headTransform;

    private GridManager _gridManager;
    private Camera _camera;
    private Plane _dragPlane;

    private bool _dragging;

    // head -> tail
    private readonly List<Vector2Int> _cells = new List<Vector2Int>();
    private readonly List<Transform> _tailTransforms = new List<Transform>();

    private Vector2Int _anchorStartCell;
    private Vector2Int _pointerStartCell;

    private void Start()
    {
        _camera = Camera.main;
        _gridManager = GridManager.Instance;

        
        _cells.Clear();
        _cells.Add(new Vector2Int(2, 2)); // head
        _cells.Add(new Vector2Int(1, 2)); // tail 1
        _cells.Add(new Vector2Int(0, 2)); // tail 2

        BuildTailMeshes();
        SnapWorldPositionsImmediate();
        UpdateRotation();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
            TryStartDrag();

        if (_dragging && Input.GetMouseButton(0))
            DragMove();

        if (_dragging && Input.GetMouseButtonUp(0))
            EndDrag();

        if (_cells.Count > 0)
            UpdateWorldPositionsLerped();
    }


    private void TryStartDrag()
    {
        Ray ray = _camera.ScreenPointToRay(Input.mousePosition);


        if (Physics.Raycast(ray, out RaycastHit hit, 100f))
        {
            if (!hit.transform.IsChildOf(transform) && hit.transform != transform)
                return;

            _dragging = true;


            _dragPlane = new Plane(Vector3.up, headTransform.position);

            if (TryGetPointerPos(out Vector3 worldPos))
            {
                _pointerStartCell = _gridManager.WorldToGrid(worldPos);
                _anchorStartCell = _cells[0];
            }
        }
    }

    private void DragMove()
    {
        if (!TryGetPointerPos(out Vector3 worldPos))
            return;

        Vector2Int pointerCell = _gridManager.WorldToGrid(worldPos);
        Vector2Int delta = pointerCell - _pointerStartCell;


        Vector2Int moveDir = Vector2Int.zero;
        int steps = 0;


        if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
        {
            moveDir = delta.x > 0 ? Vector2Int.right : Vector2Int.left;
            steps = Mathf.Abs(delta.x);
        }
        else if (Mathf.Abs(delta.y) > 0)
        {
            moveDir = delta.y > 0 ? Vector2Int.up : Vector2Int.down;
            steps = Mathf.Abs(delta.y);
        }

        if (moveDir == Vector2Int.zero || steps == 0)
            return;


        if (_cells.Count > 1)
        {
            Vector2Int backwardDir = _cells[1] - _cells[0];
            if (moveDir == -backwardDir)
                return;
        }


        Vector2Int targetHeadCell = _anchorStartCell + moveDir * steps;


        targetHeadCell.x = Mathf.Clamp(targetHeadCell.x, 0, _gridManager.Width - 1);
        targetHeadCell.y = Mathf.Clamp(targetHeadCell.y, 0, _gridManager.Height - 1);


        if (targetHeadCell != _cells[0])
        {
            for (int i = _cells.Count - 1; i > 0; i--)
                _cells[i] = _cells[i - 1];

            _cells[0] = targetHeadCell;
            UpdateRotation();


            _anchorStartCell = _cells[0];
            _pointerStartCell = pointerCell;
        }
    }

    private void EndDrag()
    {
        _dragging = false;
    }


    private bool TryGetPointerPos(out Vector3 world)
    {
        Ray ray = _camera.ScreenPointToRay(Input.mousePosition);

        if (_dragPlane.Raycast(ray, out float dist))
        {
            world = ray.GetPoint(dist);
            return true;
        }

        world = Vector3.zero;
        return false;
    }


    private void BuildTailMeshes()
    {
        _tailTransforms.Clear();

        for (int i = 1; i < _cells.Count; i++)
        {
            Vector3 worldPos = _gridManager.GridToWorld(_cells[i].x, _cells[i].y);


            GameObject inst = Instantiate(tailPrefab, worldPos, Quaternion.identity, tailRoot);
            _tailTransforms.Add(inst.transform);
        }
    }

    private void SnapWorldPositionsImmediate()
    {
        headTransform.position = _gridManager.GridToWorld(_cells[0].x, _cells[0].y);

        for (int i = 1; i < _cells.Count; i++)
        {
            Vector3 pos = _gridManager.GridToWorld(_cells[i].x, _cells[i].y);
            _tailTransforms[i - 1].position = pos;
        }
    }

    private void UpdateWorldPositionsLerped()
    {
        Vector3 targetHeadPos = _gridManager.GridToWorld(_cells[0].x, _cells[0].y);
        headTransform.position = Vector3.Lerp(
            headTransform.position,
            targetHeadPos,
            Time.deltaTime * dragLerp
        );


        for (int i = 1; i < _cells.Count; i++)
        {
            Vector3 targetPos = _gridManager.GridToWorld(_cells[i].x, _cells[i].y);


            _tailTransforms[i - 1].position = Vector3.Lerp(
                _tailTransforms[i - 1].position,
                targetPos,
                Time.deltaTime * dragLerp
            );
        }
    }

    private void UpdateRotation()
    {
        if (_cells.Count < 2) return;

        Vector2Int dir = _cells[0] - _cells[1];

        float angle = 0f;
        if (dir == Vector2Int.up) angle = 0f;
        else if (dir == Vector2Int.right) angle = 90f;
        else if (dir == Vector2Int.down) angle = 180f;
        else if (dir == Vector2Int.left) angle = 270f;

        headTransform.rotation = Quaternion.Euler(0f, angle, 0f);
    }
}