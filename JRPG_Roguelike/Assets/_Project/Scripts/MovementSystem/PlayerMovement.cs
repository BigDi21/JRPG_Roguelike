using System.Collections;
using UnityEngine;

/// <summary>
/// Управляет перемещением игрока по сетке и его поворотами.
/// </summary>
[DefaultExecutionOrder(0)]
public class PlayerMovement : MonoBehaviour
{
    private const float RotationStep = 90f;
    private const int DirectionCount = 4;

    private static readonly Directions[] DirectionOrder =
    {
        Directions.North,
        Directions.East,
        Directions.South,
        Directions.West
    };

    [Header("Ссылки")]
    [SerializeField] private GridManager _gridManager;

    [Header("Настройки анимации")]
    [SerializeField] private float _moveDuration = 0.2f;
    [SerializeField] private float _rotateDuration = 0.15f;

    /// <summary>Идёт ли сейчас анимация перемещения.</summary>
    public bool IsMoving { get; private set; }

    private Vector2Int _currentGridPos;
    private Directions _facingDirection = Directions.North;
    private Cell _currentCell;
    private bool _isAnimating;

    /// <summary>
    /// Ленивая инициализация GridManager.
    /// </summary>
    private GridManager Grid => _gridManager != null ? _gridManager : (_gridManager = GridManager.Instance);

    private void Start()
    {
        if (Grid == null)
        {
            Debug.LogError("[PlayerMovement] GridManager не найден!");
            enabled = false;
            return;
        }

        InitializeAtStartPosition();
    }

    private void Update()
    {
        if (_isAnimating) return;

        HandleInput();

        if (Input.GetKeyDown(KeyCode.J) && MapManager.Instance != null)
            MapManager.Instance.ToggleFog();
    }

    // ======== ИНИЦИАЛИЗАЦИЯ ========

    private void InitializeAtStartPosition()
    {
        _currentGridPos = Grid.StartPosition;
        _currentCell = Grid.GetCell(_currentGridPos);

        if (_currentCell != null)
        {
            _currentCell.SetOccupant(gameObject);
            transform.position = Grid.GetWorldPosition(_currentGridPos);
        }

        if (MapManager.Instance != null)
            MapManager.Instance.UpdateMap(_currentGridPos, _facingDirection);
    }

    // ======== ВВОД ========

    private void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.Q)) Rotate(-RotationStep);
        else if (Input.GetKeyDown(KeyCode.E)) Rotate(RotationStep);

        if (Input.GetKeyDown(KeyCode.W)) TryMove(Directions.North);
        else if (Input.GetKeyDown(KeyCode.S)) TryMove(Directions.South);
        else if (Input.GetKeyDown(KeyCode.A)) TryMove(Directions.West);
        else if (Input.GetKeyDown(KeyCode.D)) TryMove(Directions.East);
    }

    // ======== ПОВОРОТ ========

    private void Rotate(float angle)
    {
        if (_isAnimating) return;

        int currentIndex = System.Array.IndexOf(DirectionOrder, _facingDirection);
        int newIndex = (currentIndex + Mathf.RoundToInt(angle / RotationStep) + DirectionCount) % DirectionCount;
        _facingDirection = DirectionOrder[newIndex];

        float targetAngle = newIndex * RotationStep;
        StartCoroutine(RotateSmoothly(targetAngle));

        if (MapManager.Instance != null)
            MapManager.Instance.UpdateMap(_currentGridPos, _facingDirection);
    }

    private IEnumerator RotateSmoothly(float targetAngle)
    {
        _isAnimating = true;

        Quaternion startRot = transform.rotation;
        Quaternion endRot = Quaternion.Euler(0, targetAngle, 0);
        float elapsed = 0f;

        while (elapsed < _rotateDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / _rotateDuration;
            transform.rotation = Quaternion.Slerp(startRot, endRot, t);
            yield return null;
        }

        transform.rotation = endRot;
        _isAnimating = false;
    }

    // ======== ПЕРЕМЕЩЕНИЕ ========

    private void TryMove(Directions relativeDir)
    {
        Directions absoluteDir = RelativeToAbsolute(relativeDir);
        if (absoluteDir == Directions.None) return;

        if (_currentCell == null || !_currentCell.CanMove(absoluteDir))
        {
            Debug.Log("[PlayerMovement] Стена!");
            return;
        }

        Vector2Int targetPos = _currentGridPos + GetOffset(absoluteDir);
        Cell targetCell = Grid.GetCell(targetPos);

        if (targetCell == null)
        {
            Debug.Log("[PlayerMovement] За пределами сетки!");
            return;
        }

        if (targetCell.IsOccupied)
        {
            Debug.Log("[PlayerMovement] Ячейка занята!");
            return;
        }

        MoveToCell(targetCell);
    }

    private void MoveToCell(Cell targetCell)
    {
        _currentCell.ClearOccupant();
        _currentCell = targetCell;
        _currentGridPos = targetCell.Position;
        _currentCell.SetOccupant(gameObject);

        Vector3 startPos = transform.position;
        Vector3 endPos = Grid.GetWorldPosition(_currentGridPos);
        StartCoroutine(MoveSmoothly(startPos, endPos));

        if (MapManager.Instance != null)
            MapManager.Instance.UpdateMap(_currentGridPos, _facingDirection);
    }

    private IEnumerator MoveSmoothly(Vector3 startPos, Vector3 endPos)
    {
        _isAnimating = true;
        IsMoving = true;

        float elapsed = 0f;

        while (elapsed < _moveDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / _moveDuration;
            transform.position = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }

        transform.position = endPos;
        _isAnimating = false;
        IsMoving = false;
    }

    // ======== НАПРАВЛЕНИЯ ========

    private Directions RelativeToAbsolute(Directions relative)
    {
        switch (relative)
        {
            case Directions.North: return _facingDirection;
            case Directions.South: return Opposite(_facingDirection);
            case Directions.West: return RotateDirection(_facingDirection, -1);
            case Directions.East: return RotateDirection(_facingDirection, 1);
            default: return Directions.None;
        }
    }

    private Directions Opposite(Directions dir)
    {
        switch (dir)
        {
            case Directions.North: return Directions.South;
            case Directions.South: return Directions.North;
            case Directions.East: return Directions.West;
            case Directions.West: return Directions.East;
            default: return Directions.None;
        }
    }

    private Directions RotateDirection(Directions dir, int steps)
    {
        int idx = System.Array.IndexOf(DirectionOrder, dir);
        int newIdx = (idx + steps + DirectionCount) % DirectionCount;
        return DirectionOrder[newIdx];
    }

    private Vector2Int GetOffset(Directions dir)
    {
        switch (dir)
        {
            case Directions.North: return Vector2Int.up;
            case Directions.South: return Vector2Int.down;
            case Directions.East: return Vector2Int.right;
            case Directions.West: return Vector2Int.left;
            default: return Vector2Int.zero;
        }
    }
}