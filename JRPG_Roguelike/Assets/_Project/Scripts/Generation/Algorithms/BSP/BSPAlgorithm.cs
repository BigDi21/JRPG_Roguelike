using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Алгоритм генерации уровня через рекурсивное бинарное деление пространства (BSP).
/// Создаёт комнаты, соединённые Г-образными коридорами.
/// Минимальный размер области деления привязан к максимальному размеру комнаты.
/// </summary>
public class BSPAlgorithm : ILevelAlgorithm
{
    private const int _roomPadding = 1;
    private const int _minAreaSizeLimit = 2;

    private BSPNode _root;
    private readonly List<BSPRoom> _rooms = new();
    private int _leafCount;
    private int _minAreaSize;

    /// <inheritdoc />
    public LevelGrid Generate(int width, int height, int seed, AlgorithmParams parameters)
    {
        var rng = new System.Random(seed);

        var grid = new LevelGrid
        {
            Width = width,
            Height = height,
            Cells = new Cell[width, height],
            Exits = new List<Vector2Int>(),
            Rooms = new List<RectInt>()
        };

        // 1. Вычисляем минимальный размер области
        // MinAreaSize = MaxRoomSize * 2 (гарантия, что комната поместится)
        _minAreaSize = Mathf.Max(
            parameters.MaxRoomSize + 1,
            _minAreaSizeLimit);

        Debug.Log($"[BSP] MinAreaSize = {_minAreaSize} (MaxRoomSize = {parameters.MaxRoomSize})");

        // 2. Пустые клетки
        CreateEmptyCells(grid);

        // 3. Дерево BSP
        _root = new BSPNode(new RectInt(0, 0, width, height));
        _leafCount = 1;
        _rooms.Clear();

        var minRooms = parameters.MinRooms > 0 ? parameters.MinRooms : 10;
        var maxRooms = parameters.MaxRooms > 0 ? parameters.MaxRooms : 15;

        SplitRecursive(_root, rng, minRooms, maxRooms);

        // 4. Создаём комнаты
        CreateRooms(_root, rng, parameters);

        // 5. Прорубаем все комнаты на сетке
        foreach (BSPRoom room in _rooms)
        {
            CarveRoom(grid, room);
        }

        // 6. Соединяем комнаты коридорами
        ConnectTree(_root, grid, rng);

        // 7. Петли
        if (parameters.LoopChance > 0)
        {
            AddLoops(grid, parameters.LoopChance, rng);
        }

        // 8. Спавн
        grid.SpawnPoint = _rooms.Count > 0
            ? _rooms[0].Center
            : new Vector2Int(1, 1);

        // 9. Выходы
        AddDefaultExits(grid);

        // Сохраняем границы комнат для механики «открыть комнату при входе»
        foreach (BSPRoom room in _rooms)
        {
            grid.Rooms.Add(room.Bounds);
        }

        Debug.Log($"[BSP] Сгенерировано комнат: {_rooms.Count}");

        return grid;
    }

    // ======== ПОДГОТОВКА ========

    private void CreateEmptyCells(LevelGrid grid)
    {
        for (var x = 0; x < grid.Width; x++)
        {
            for (var y = 0; y < grid.Height; y++)
            {
                grid.Cells[x, y] = new Cell(new Vector2Int(x, y));
            }
        }
    }

    // ======== ДЕЛЕНИЕ BSP ========

    private void SplitRecursive(BSPNode node, System.Random rng, int minRooms, int maxRooms)
    {
        if (node == null)
        {
            return;
        }

        if (!CanSplit(node))
        {
            return;
        }

        if (_leafCount >= maxRooms)
        {
            return;
        }

        var splitSucceeded = SplitNode(node, rng);
        if (!splitSucceeded)
        {
            return;
        }

        _leafCount++;

        SplitRecursive(node.Left, rng, minRooms, maxRooms);
        SplitRecursive(node.Right, rng, minRooms, maxRooms);
    }

    private bool CanSplit(BSPNode node)
    {
        if (node == null)
        {
            return false;
        }

        return node.Area.width >= _minAreaSize * 2
            || node.Area.height >= _minAreaSize * 2;
    }

    private bool SplitNode(BSPNode node, System.Random rng)
    {
        var splitHorizontal = ChooseSplitDirection(node.Area, rng);

        if (splitHorizontal)
        {
            var maxSplit = node.Area.width - _minAreaSize;
            if (maxSplit <= _minAreaSize)
            {
                return false;
            }

            var splitPos = rng.Next(_minAreaSize, maxSplit);

            node.Left = new BSPNode(new RectInt(node.Area.x, node.Area.y, splitPos, node.Area.height));
            node.Right = new BSPNode(new RectInt(node.Area.x + splitPos, node.Area.y, node.Area.width - splitPos, node.Area.height));
        }
        else
        {
            var maxSplit = node.Area.height - _minAreaSize;
            if (maxSplit <= _minAreaSize)
            {
                return false;
            }

            var splitPos = rng.Next(_minAreaSize, maxSplit);

            node.Left = new BSPNode(new RectInt(node.Area.x, node.Area.y, node.Area.width, splitPos));
            node.Right = new BSPNode(new RectInt(node.Area.x, node.Area.y + splitPos, node.Area.width, node.Area.height - splitPos));
        }

        return true;
    }

    private bool ChooseSplitDirection(RectInt area, System.Random rng)
    {
        if (area.width > area.height * 1.25f)
        {
            return true;
        }

        if (area.height > area.width * 1.25f)
        {
            return false;
        }

        return rng.Next(2) == 0;
    }

    // ======== СОЗДАНИЕ КОМНАТ ========

    private void CreateRooms(BSPNode node, System.Random rng, AlgorithmParams parameters)
    {
        if (node == null)
        {
            return;
        }

        if (!node.IsLeaf)
        {
            CreateRooms(node.Left, rng, parameters);
            CreateRooms(node.Right, rng, parameters);
            return;
        }

        var minRoom = parameters.MinRoomSize > 0 ? parameters.MinRoomSize : 4;
        var maxRoom = parameters.MaxRoomSize > 0 ? parameters.MaxRoomSize : 8;

        var maxW = Mathf.Min(maxRoom, node.Area.width - _roomPadding * 2);
        var maxH = Mathf.Min(maxRoom, node.Area.height - _roomPadding * 2);

        if (maxW < minRoom || maxH < minRoom)
        {
            Debug.LogWarning($"[BSP] Область {node.Area} слишком мала для комнаты {minRoom}×{minRoom}");
            return;
        }

        var roomW = rng.Next(minRoom, maxW + 1);
        var roomH = rng.Next(minRoom, maxH + 1);

        // Центрируем комнату в листе с небольшим случайным смещением
        var maxOffsetX = Mathf.Max(0, (node.Area.width - roomW) / 2 - _roomPadding);
        var maxOffsetY = Mathf.Max(0, (node.Area.height - roomH) / 2 - _roomPadding);

        var offsetX = maxOffsetX > 0 ? rng.Next(-Mathf.Min(1, maxOffsetX), Mathf.Min(1, maxOffsetX) + 1) : 0;
        var offsetY = maxOffsetY > 0 ? rng.Next(-Mathf.Min(1, maxOffsetY), Mathf.Min(1, maxOffsetY) + 1) : 0;

        var roomX = node.Area.x + (node.Area.width - roomW) / 2 + offsetX;
        var roomY = node.Area.y + (node.Area.height - roomH) / 2 + offsetY;

        var room = new BSPRoom(new RectInt(roomX, roomY, roomW, roomH));
        node.Room = room;
        _rooms.Add(room);
    }

    // ======== СОЕДИНЕНИЕ КОМНАТ КОРИДОРАМИ ========

    private void ConnectTree(BSPNode node, LevelGrid grid, System.Random rng)
    {
        if (node == null || node.IsLeaf)
        {
            return;
        }

        ConnectTree(node.Left, grid, rng);
        ConnectTree(node.Right, grid, rng);

        var leftRooms = new List<BSPRoom>();
        var rightRooms = new List<BSPRoom>();
        CollectRooms(node.Left, leftRooms);
        CollectRooms(node.Right, rightRooms);

        if (leftRooms.Count != 0 && rightRooms.Count != 0)
        {
            (BSPRoom a, BSPRoom b) = FindClosestPair(leftRooms, rightRooms);
            CarveCorridor(grid, a.Center, b.Center, rng);
        }
    }

    private (BSPRoom, BSPRoom) FindClosestPair(List<BSPRoom> left, List<BSPRoom> right)
    {
        BSPRoom bestA = left[0];
        BSPRoom bestB = right[0];
        var bestDist = float.MaxValue;

        foreach (BSPRoom a in left)
        {
            foreach (BSPRoom b in right)
            {
                var d = Vector2Int.Distance(a.Center, b.Center);
                if (d < bestDist)
                {
                    bestDist = d;
                    bestA = a;
                    bestB = b;
                }
            }
        }

        return (bestA, bestB);
    }

    private void CollectRooms(BSPNode node, List<BSPRoom> rooms)
    {
        if (node == null)
        {
            return;
        }

        if (node.IsLeaf && node.Room.HasValue)
        {
            rooms.Add(node.Room.Value);
            return;
        }

        CollectRooms(node.Left, rooms);
        CollectRooms(node.Right, rooms);
    }

    // ======== ПРОРУБАНИЕ ========

    private void CarveRoom(LevelGrid grid, BSPRoom room)
    {
        RectInt b = room.Bounds;

        for (var x = b.x; x < b.x + b.width; x++)
        {
            for (var y = b.y; y < b.y + b.height; y++)
            {
                if (!IsValid(grid, x, y))
                {
                    continue;
                }

                var current = new Vector2Int(x, y);

                if (x > b.x)
                {
                    ConnectCells(grid, current, new Vector2Int(x - 1, y));
                }

                if (x < b.x + b.width - 1)
                {
                    ConnectCells(grid, current, new Vector2Int(x + 1, y));
                }

                if (y > b.y)
                {
                    ConnectCells(grid, current, new Vector2Int(x, y - 1));
                }

                if (y < b.y + b.height - 1)
                {
                    ConnectCells(grid, current, new Vector2Int(x, y + 1));
                }
            }
        }
    }

    private void CarveCorridor(LevelGrid grid, Vector2Int from, Vector2Int to, System.Random rng)
    {
        var horizontalFirst = rng.Next(2) == 0;

        if (horizontalFirst)
        {
            CarveHorizontal(grid, from.x, to.x, from.y);
            CarveVertical(grid, from.y, to.y, to.x);
        }
        else
        {
            CarveVertical(grid, from.y, to.y, from.x);
            CarveHorizontal(grid, from.x, to.x, to.y);
        }
    }

    private void CarveHorizontal(LevelGrid grid, int x1, int x2, int y)
    {
        var start = Mathf.Min(x1, x2);
        var end = Mathf.Max(x1, x2);

        for (var x = start; x < end; x++)
        {
            if (IsValid(grid, x, y) && IsValid(grid, x + 1, y))
            {
                ConnectCells(grid, new Vector2Int(x, y), new Vector2Int(x + 1, y));
            }
        }
    }

    private void CarveVertical(LevelGrid grid, int y1, int y2, int x)
    {
        var start = Mathf.Min(y1, y2);
        var end = Mathf.Max(y1, y2);

        for (var y = start; y < end; y++)
        {
            if (IsValid(grid, x, y) && IsValid(grid, x, y + 1))
            {
                ConnectCells(grid, new Vector2Int(x, y), new Vector2Int(x, y + 1));
            }
        }
    }

    private void ConnectCells(LevelGrid grid, Vector2Int a, Vector2Int b)
    {
        Cell cellA = grid.Cells[a.x, a.y];
        Cell cellB = grid.Cells[b.x, b.y];
        Vector2Int diff = b - a;

        if (diff == Vector2Int.up)
        {
            cellA.AddConnection(Directions.North);
            cellB.AddConnection(Directions.South);
        }
        else if (diff == Vector2Int.down)
        {
            cellA.AddConnection(Directions.South);
            cellB.AddConnection(Directions.North);
        }
        else if (diff == Vector2Int.right)
        {
            cellA.AddConnection(Directions.East);
            cellB.AddConnection(Directions.West);
        }
        else if (diff == Vector2Int.left)
        {
            cellA.AddConnection(Directions.West);
            cellB.AddConnection(Directions.East);
        }
    }

    // ======== ПЕТЛИ ========

    private void AddLoops(LevelGrid grid, int loopChance, System.Random rng)
    {
        if (_rooms.Count < 2)
        {
            return;
        }

        var loopsToAdd = Mathf.RoundToInt(_rooms.Count * loopChance / 100f);
        var loopsAdded = 0;
        var attempts = 0;
        var maxAttempts = loopsToAdd * 20; // защита от бесконечного цикла

        while (loopsAdded < loopsToAdd && attempts < maxAttempts)
        {
            attempts++;

            BSPRoom a = _rooms[rng.Next(_rooms.Count)];
            BSPRoom b = _rooms[rng.Next(_rooms.Count)];

            if (a.Center == b.Center)
            {
                continue;
            }

            // Проверяем: комнаты уже соединены коротким путём?
            if (AreRoomsDirectlyConnected(grid, a.Center, b.Center))
            {
                continue;
            }

            CarveCorridor(grid, a.Center, b.Center, rng);
            loopsAdded++;
        }

        Debug.Log($"[BSP] Добавлено петель: {loopsAdded} (попыток: {attempts})");
    }

    /// <summary>
    /// Проверяет, соединены ли две точки коротким путём (≤ манхэттен + 2 клетки).
    /// Если да — новая петля между ними будет дубликатом.
    /// </summary>
    private bool AreRoomsDirectlyConnected(LevelGrid grid, Vector2Int from, Vector2Int to)
    {
        var manhattanDist = Mathf.Abs(from.x - to.x) + Mathf.Abs(from.y - to.y);
        var maxAcceptablePath = manhattanDist + 2;

        var queue = new Queue<(Vector2Int pos, int dist)>();
        var visited = new HashSet<Vector2Int>();

        queue.Enqueue((from, 0));
        visited.Add(from);

        Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        while (queue.Count > 0)
        {
            (Vector2Int current, var dist) = queue.Dequeue();

            if (current == to)
            {
                return dist <= maxAcceptablePath;
            }

            if (dist >= maxAcceptablePath)
            {
                continue;
            }

            foreach (Vector2Int dir in dirs)
            {
                Vector2Int next = current + dir;

                if (next.x < 0 || next.x >= grid.Width)
                {
                    continue;
                }

                if (next.y < 0 || next.y >= grid.Height)
                {
                    continue;
                }

                if (visited.Contains(next))
                {
                    continue;
                }

                Cell currentCell = grid.Cells[current.x, current.y];
                if (!currentCell.CanMove(GetDirectionFromOffset(dir)))
                {
                    continue;
                }

                visited.Add(next);
                queue.Enqueue((next, dist + 1));
            }
        }

        return false;
    }

    /// <summary>
    /// Преобразует смещение в направление.
    /// </summary>
    private Directions GetDirectionFromOffset(Vector2Int offset)
    {
        if (offset == Vector2Int.up)
        {
            return Directions.North;
        }

        if (offset == Vector2Int.down)
        {
            return Directions.South;
        }

        if (offset == Vector2Int.right)
        {
            return Directions.East;
        }

        if (offset == Vector2Int.left)
        {
            return Directions.West;
        }

        return Directions.None;
    }

    // ======== ВЫХОДЫ ========

    private void AddDefaultExits(LevelGrid grid)
    {
        grid.Exits.Add(new Vector2Int(0, grid.Height / 2));
        grid.Exits.Add(new Vector2Int(0, grid.Height / 2 + 1));
        grid.Exits.Add(new Vector2Int(grid.Width / 2, grid.Height - 1));
        grid.Exits.Add(new Vector2Int(grid.Width / 2 + 1, grid.Height - 1));
    }

    // ======== УТИЛИТЫ ========

    private bool IsValid(LevelGrid grid, int x, int y) => x >= 0 && x < grid.Width && y >= 0 && y < grid.Height;
}
