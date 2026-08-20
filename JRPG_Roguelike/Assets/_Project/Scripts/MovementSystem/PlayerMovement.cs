using System.Collections;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Ссылки")]
    public GridManager gridManager;
    public Vector2Int startGridPosition = new Vector2Int(0, 0);

    [Header("Настройки анимации")]
    public float moveDuration = 0.2f;    // время перемещения между ячейками
    public float rotateDuration = 0.15f; // время поворота на 90°
    public bool IsMoving { get; private set; }

    private Vector2Int _currentGridPos;
    private Directions _facingDirection = Directions.North;
    private Cell _currentCell;
    private Cell _targetCell;

    private bool _isAnimating = false;   // блокировка ввода во время анимации

    void Start()
    {
        if (gridManager == null)
            gridManager = GridManager.Instance;

        if (gridManager == null)
        {
            Debug.LogError("GridManager не найден!");
            return;
        }

        _currentGridPos = startGridPosition;
        _currentCell = gridManager.GetCell(_currentGridPos);
        if (_currentCell != null)
        {
            _currentCell.SetOccupant(gameObject);
            transform.position = gridManager.GetWorldPosition(_currentGridPos);
        }
        else
        {
            Debug.LogError($"Ячейка {startGridPosition} не найдена!");
        }
    }

    void Update()
    {
        if (_isAnimating) return; // блокируем ввод во время анимации

        HandleInput();
    }

    private void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.Q))
            Rotate(-90);
        else if (Input.GetKeyDown(KeyCode.E))
            Rotate(90);

        if (Input.GetKeyDown(KeyCode.W))
            TryMove(Directions.North);
        else if (Input.GetKeyDown(KeyCode.S))
            TryMove(Directions.South);
        else if (Input.GetKeyDown(KeyCode.A))
            TryMove(Directions.West);
        else if (Input.GetKeyDown(KeyCode.D))
            TryMove(Directions.East);
    }

    private void Rotate(int angle)
    {
        if (_isAnimating) return;

        Directions[] dirOrder = { Directions.North, Directions.East, Directions.South, Directions.West };
        int currentIndex = System.Array.IndexOf(dirOrder, _facingDirection);
        int newIndex = (currentIndex + (angle / 90) + 4) % 4;
        _facingDirection = dirOrder[newIndex];

        float targetAngle = newIndex * 90f;
        StartCoroutine(RotateSmoothly(targetAngle));
    }

    private IEnumerator RotateSmoothly(float targetAngle)
    {
        _isAnimating = true;
        Quaternion startRot = transform.rotation;
        Quaternion endRot = Quaternion.Euler(0, targetAngle, 0);
        float elapsed = 0f;

        while (elapsed < rotateDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / rotateDuration;
            transform.rotation = Quaternion.Slerp(startRot, endRot, t);
            yield return null;
        }
        transform.rotation = endRot;
        _isAnimating = false;
    }

    private void TryMove(Directions relativeDir)
    {
        Directions absoluteDir = RelativeToAbsolute(relativeDir);
        if (absoluteDir == Directions.None) return;

        if (!_currentCell.CanMove(absoluteDir))
        {
            Debug.Log("Стена!");
            return;
        }

        Vector2Int targetPos = _currentGridPos + GetOffset(absoluteDir);
        _targetCell = gridManager.GetCell(targetPos);
        if (_targetCell == null)
        {
            Debug.Log("За пределами сетки!");
            return;
        }

        if (_targetCell.IsOccupied)
        {
            Debug.Log("Ячейка занята!");
            return;
        }

        MoveToCell(_targetCell);
    }

    private void MoveToCell(Cell targetCell)
    {
        _currentCell.ClearOccupant();
        _currentCell = targetCell;
        _currentGridPos = targetCell.Position;
        _currentCell.SetOccupant(gameObject);

        Vector3 startPos = transform.position;
        Vector3 endPos = gridManager.GetWorldPosition(_currentGridPos);
        StartCoroutine(MoveSmoothly(startPos, endPos));
    }

    private IEnumerator MoveSmoothly(Vector3 startPos, Vector3 endPos)
    {
        _isAnimating = true;
        IsMoving = true; // движение началось
        float elapsed = 0f;

        while (elapsed < moveDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / moveDuration;
            transform.position = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }
        transform.position = endPos;
        _isAnimating = false;
        IsMoving = false; // движение закончилось
    }

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
        Directions[] order = { Directions.North, Directions.East, Directions.South, Directions.West };
        int idx = System.Array.IndexOf(order, dir);
        int newIdx = (idx + steps + 4) % 4;
        return order[newIdx];
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