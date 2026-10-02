using System.Collections;
using UnityEngine;

/// <summary>
/// Управляет перемещением игрока по сетке и его поворотами.
/// </summary>
[DefaultExecutionOrder(0)]
public class PlayerMovement : MonoBehaviour
{
    private const float _rotationStep = 90f;
    private const int _directionCount = 4;

    private static readonly Directions[] _directionOrder =
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
            if (Grid == null)
            {
                // GridManager отсутствует — вероятно, это боевая или другая сцена,
                // где сеточное движение не используется. Отключаемся без шума.
                Debug.LogWarning("[PlayerMovement] GridManager не найден. Компонент отключён.");
                enabled = false;
                return;
            }

            InitializeAtStartPosition();
        }

        InitializeAtStartPosition();
    }

    private void Update()
    {
        if (_isAnimating)
        {
            return;
        }

        HandleInput();

        if (Input.GetKeyDown(KeyCode.J) && MapManager.Instance != null)
        {
            MapManager.Instance.ToggleFog();
        }
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
        {
            MapManager.Instance.UpdateMap(_currentGridPos, _facingDirection);
        }
    }

    // ======== ВВОД ========

    private void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            Rotate(-_rotationStep);
        }
        else if (Input.GetKeyDown(KeyCode.E))
        {
            Rotate(_rotationStep);
        }

        if (Input.GetKeyDown(KeyCode.W))
        {
            TryMove(Directions.North);
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            TryMove(Directions.South);
        }
        else if (Input.GetKeyDown(KeyCode.A))
        {
            TryMove(Directions.West);
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            TryMove(Directions.East);
        }
    }

    // ======== ПОВОРОТ ========

    private void Rotate(float angle)
    {
        if (_isAnimating)
        {
            return;
        }

        var currentIndex = System.Array.IndexOf(_directionOrder, _facingDirection);
        var newIndex = (currentIndex + Mathf.RoundToInt(angle / _rotationStep) + _directionCount) % _directionCount;
        _facingDirection = _directionOrder[newIndex];

        var targetAngle = newIndex * _rotationStep;
        StartCoroutine(RotateSmoothly(targetAngle));

        if (MapManager.Instance != null)
        {
            MapManager.Instance.UpdateMap(_currentGridPos, _facingDirection);
        }
    }

    private IEnumerator RotateSmoothly(float targetAngle)
    {
        _isAnimating = true;

        Quaternion startRot = transform.rotation;
        var endRot = Quaternion.Euler(0, targetAngle, 0);
        var elapsed = 0f;

        while (elapsed < _rotateDuration)
        {
            elapsed += Time.deltaTime;
            var t = elapsed / _rotateDuration;
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
        if (absoluteDir == Directions.None)
        {
            return;
        }

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
        {
            MapManager.Instance.UpdateMap(_currentGridPos, _facingDirection);
        }
    }

    private IEnumerator MoveSmoothly(Vector3 startPos, Vector3 endPos)
    {
        _isAnimating = true;
        IsMoving = true;

        var elapsed = 0f;

        while (elapsed < _moveDuration)
        {
            elapsed += Time.deltaTime;
            var t = elapsed / _moveDuration;
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
        return relative switch
        {
            Directions.North => _facingDirection,
            Directions.South => Opposite(_facingDirection),
            Directions.West => RotateDirection(_facingDirection, -1),
            Directions.East => RotateDirection(_facingDirection, 1),
            _ => Directions.None,
        };
    }

    private Directions Opposite(Directions dir)
    {
        return dir switch
        {
            Directions.North => Directions.South,
            Directions.South => Directions.North,
            Directions.East => Directions.West,
            Directions.West => Directions.East,
            _ => Directions.None,
        };
    }

    private Directions RotateDirection(Directions dir, int steps)
    {
        var idx = System.Array.IndexOf(_directionOrder, dir);
        var newIdx = (idx + steps + _directionCount) % _directionCount;
        return _directionOrder[newIdx];
    }

    private Vector2Int GetOffset(Directions dir)
    {
        return dir switch
        {
            Directions.North => Vector2Int.up,
            Directions.South => Vector2Int.down,
            Directions.East => Vector2Int.right,
            Directions.West => Vector2Int.left,
            _ => Vector2Int.zero,
        };
    }
}
