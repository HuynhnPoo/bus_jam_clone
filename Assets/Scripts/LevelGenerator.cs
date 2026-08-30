using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class LevelGenerator
{
    private int width, height, vehicleCount, minCapacity, maxCapacity;
  
    private enum CellOccupancy { Empty, VehicleBody, PathReserved }
    private CellOccupancy[,] occupancy;

    private List<VehicleData> vehicles;
    private List<PersonGroupData> personGroups;
    private Dictionary<int, List<Vector3Int>> vehicleExitPaths;

    public void Setup(int gridWidth, int gridHeight, int vCount, int minCap, int maxCap)
    {
        width = gridWidth;
        height = gridHeight;
        vehicleCount = vCount;
        minCapacity = minCap;
        maxCapacity = maxCap;
    }
    public LevelData GenerateLevelData(int levelIndex)
    {
        vehicles = new List<VehicleData>();
        personGroups = new List<PersonGroupData>();
        vehicleExitPaths = new Dictionary<int, List<Vector3Int>>();

        int idCounter = 0;

        // Đóng gói kín từng dòng (lane) — đảm bảo 100% ô có xe và luôn giải được
        for (int y = 0; y < height; y++)
        {
            MoveDirection dir = Random.value < 0.5f ? MoveDirection.Left : MoveDirection.Right;
            PackLaneFull(y, dir, ref idCounter);
        }

        GenerateBalancedPeople();

        LevelData data = ScriptableObject.CreateInstance<LevelData>();
        data.width = width;
        data.height = height;
        data.levelIndex = levelIndex;
        data.vehicles = vehicles;
        data.personGroups = personGroups;
        data.difficultyScore = EstimateDifficulty();
        data.cellSize = 28f;
        data.spacing = 1;
        return data;
    }

    // Xếp kín 1 dòng ngang bằng nhiều xe nối đuôi nhau, từ mép thoát vào trong
    private void PackLaneFull(int row, MoveDirection dir, ref int idCounter)
    {
        // Thứ tự duyệt: nếu thoát bên phải -> xe đầu tiên đặt ở cột phải nhất (đi ra trước)
        // nếu thoát bên trái -> xe đầu tiên đặt ở cột trái nhất
        int startX = dir == MoveDirection.Right ? width - 1 : 0;
        int step = dir == MoveDirection.Right ? -1 : 1;

        int x = startX;
        int cellsLeft = width;

        while (cellsLeft > 0)
        {
            int maxLen = Mathf.Min(3, cellsLeft);
            int length = Random.Range(1, maxLen + 1);
            int capacity = Random.Range(minCapacity, maxCapacity + 1);

            // headPos = ô ngoài cùng của xe theo hướng thoát (ô sẽ ra khỏi lưới trước)
            int headX = dir == MoveDirection.Right ? x : x + (length - 1) * -step * -1;
            // Tính lại cho đúng: head luôn là ô gần mép thoát nhất trong cụm length ô đang xét
            Vector3Int head = dir == MoveDirection.Right
                ? new Vector3Int(x, row, 0)
                : new Vector3Int(x, row, 0);

            var vehicle = new VehicleData
            {
                vehicleId = idCounter,
                capacity = capacity,
                length = length,
                gridPostion = head,
                direction = dir
            };

            vehicles.Add(vehicle);

            // Không có exit path trong lưới nữa -> lưu "waiting slot" ngoài mép làm nơi đặt người
            Vector3Int waitSlot = dir == MoveDirection.Right
                ? new Vector3Int(width, row, 0)   // ngay ngoài mép phải
                : new Vector3Int(-1, row, 0);     // ngay ngoài mép trái

            vehicleExitPaths[idCounter] = new List<Vector3Int> { waitSlot };

            idCounter++;
            x += step * length;
            cellsLeft -= length;
        }
    }
    // =========================================================
    private bool TryPlaceOneVehicle(int id)
    {
        const int maxLocalAttempts = 40;

        for (int attempt = 0; attempt < maxLocalAttempts; attempt++)
        {
            MoveDirection dir = RandomDirection();
            int length = Random.Range(2, 4);
            int capacity = Random.Range(minCapacity, maxCapacity + 1);

            Vector3Int headPos = RandomEmptyCell();
            if (headPos.x < 0) return false;

            List<Vector3Int> bodyCells = GetVehicleCells(headPos, dir, length);
            if (!AllCellsValidAndEmpty(bodyCells)) continue;

            List<Vector3Int> exitPath = GetExitPath(headPos, dir);
            if (!AllCellsValidAndEmpty(exitPath)) continue;

            MarkOccupied(bodyCells, CellOccupancy.VehicleBody);
            MarkOccupied(exitPath, CellOccupancy.PathReserved);

            var newVehicle = new VehicleData
            {
                vehicleId = id,
                capacity = capacity,
                length = length,
                gridPostion = headPos,
                direction = dir
            };

            vehicles.Add(newVehicle);
            vehicleExitPaths[id] = exitPath;
            return true;
        }

        return false;
    }

    // =========================================================
    private void FillRemainingEmptyCells(ref int placedCount)
    {
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                if (occupancy[x, y] != CellOccupancy.Empty) continue;

                Vector3Int cell = new Vector3Int(x, y, 0);

                MoveDirection bestDir = MoveDirection.Right;
                List<Vector3Int> bestExitPath = null;

                foreach (MoveDirection dir in System.Enum.GetValues(typeof(MoveDirection)))
                {
                    var exitPath = GetExitPath(cell, dir);
                    if (AllCellsValidAndEmpty(exitPath))
                    {
                        if (bestExitPath == null || exitPath.Count < bestExitPath.Count)
                        {
                            bestExitPath = exitPath;
                            bestDir = dir;
                        }
                    }
                }

                if (bestExitPath == null)
                    bestExitPath = new List<Vector3Int>();

                occupancy[x, y] = CellOccupancy.VehicleBody;
                if (bestExitPath.Count > 0)
                    MarkOccupied(bestExitPath, CellOccupancy.PathReserved);

                int capacity = Random.Range(minCapacity, maxCapacity + 1);
                int newId = placedCount++;

                var newVehicle = new VehicleData
                {
                    vehicleId = newId,
                    capacity = capacity,
                    length = 1,
                    gridPostion = cell,
                    direction = bestDir
                };

                vehicles.Add(newVehicle);
                vehicleExitPaths[newId] = bestExitPath;
            }
    }

    // =========================================================
    private void GenerateBalancedPeople()
    {
        personGroups = new List<PersonGroupData>();
        int personIdCounter = 0;

        foreach (var v in vehicles)
        {
            if (!vehicleExitPaths.TryGetValue(v.vehicleId, out var path) || path.Count == 0)
                continue;

            Vector3Int waitSlot = path[0];

            // Tạo đúng 1 group chứa toàn bộ capacity của xe
            personGroups.Add(new PersonGroupData
            {
                groupPersonId = personIdCounter++,
                Count = v.capacity,
                slotPosition = waitSlot,
                ownerVehicleId = v.vehicleId
            });
        }
    }

    // =========================================================
    // Helpers
    // =========================================================
    private List<Vector3Int> GetVehicleCells(Vector3Int head, MoveDirection dir, int length)
    {
        var cells = new List<Vector3Int>();
        Vector3Int backDelta = -DirToDelta(dir);
        for (int i = 0; i < length; i++)
            cells.Add(head + backDelta * i);
        return cells;
    }

    private List<Vector3Int> GetExitPath(Vector3Int head, MoveDirection dir)
    {
        var path = new List<Vector3Int>();
        Vector3Int delta = DirToDelta(dir);
        Vector3Int cur = head;

        int safety = 0;
        while (safety < Mathf.Max(width, height) + 2)
        {
            cur += delta;
            safety++;
            if (!InBounds(cur)) break;
            path.Add(cur);
        }
        return path;
    }

    private bool AllCellsValidAndEmpty(List<Vector3Int> cells)
    {
        foreach (var c in cells)
        {
            if (!InBounds(c)) return false;
            if (occupancy[c.x, c.y] != CellOccupancy.Empty) return false;
        }
        return true;
    }

    private void MarkOccupied(List<Vector3Int> cells, CellOccupancy state)
    {
        foreach (var c in cells)
            occupancy[c.x, c.y] = state;
    }

    private Vector3Int RandomEmptyCell()
    {
        var emptyCells = new List<Vector3Int>();
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                if (occupancy[x, y] == CellOccupancy.Empty)
                    emptyCells.Add(new Vector3Int(x, y, 0));

        if (emptyCells.Count == 0) return new Vector3Int(-1, -1, 0);
        return emptyCells[Random.Range(0, emptyCells.Count)];
    }

    private bool InBounds(Vector3Int p) => p.x >= 0 && p.x < width && p.y >= 0 && p.y < height;

    private MoveDirection RandomDirection() => (MoveDirection)Random.Range(0, 4);

    private Vector3Int DirToDelta(MoveDirection d) => d switch
    {
        MoveDirection.Up => Vector3Int.up,
        MoveDirection.Down => Vector3Int.down,
        MoveDirection.Left => Vector3Int.left,
        MoveDirection.Right => Vector3Int.right,
        _ => Vector3Int.zero
    };

    private void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    private int EstimateDifficulty()
    {
        int score = vehicles.Count * 10;
        score += personGroups.Count * 2;
        return score;
    }
}