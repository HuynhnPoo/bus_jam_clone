using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class LevelGenerator
{
    private int width, height, minCapacity, maxCapacity;
    private List<VehicleData> vehicles;
    private List<PersonGroupData> personGroups;

    private const int MaxVehicleLength = 3;

    private class Seg
    {
        public List<Vector3Int> cells = new List<Vector3Int>();
        public bool horizontal;
        public bool removed;
    }

    private List<Seg> segs;
    private int[,] owner; // Trạng thái chiếm ô trên lưới (-1 là trống)

    public void Setup(int gridWidth, int gridHeight, int vCount, int minCap, int maxCap)
    {
        width = Mathf.Max(2, gridWidth);
        height = Mathf.Max(2, gridHeight);
    }

    public LevelData GenerateLevelData(int levelIndex)
    {
        vehicles = new List<VehicleData>();
        personGroups = new List<PersonGroupData>();

        TileGrid();
        PeelAndAssignDirections();
        GenerateBalancedPeople();

        var data = ScriptableObject.CreateInstance<LevelData>();
        data.width = width;
        data.height = height;
        data.levelIndex = levelIndex;
        data.vehicles = vehicles;
        data.personGroups = personGroups;
        data.cellSize = 28f;
        data.spacing = 1;
        return data;
    }

    // ============ BƯỚC 1: LÁT KÍN LƯỚI ============
    private void TileGrid()
    {
        segs = new List<Seg>();
        owner = new int[width, height];
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                owner[x, y] = -1;

        var allCells = Enumerable.Range(0, width)
            .SelectMany(x => Enumerable.Range(0, height).Select(y => new Vector3Int(x, y, 0)))
            .OrderBy(_ => Random.value)
            .ToList();

        foreach (var start in allCells)
        {
            if (owner[start.x, start.y] != -1) continue;

            bool horizontal = Random.value < 0.5f;
            int wantLen = Random.Range(1, MaxVehicleLength + 1);
            Vector3Int step = horizontal ? Vector3Int.right : Vector3Int.up;

            var s = new Seg { horizontal = horizontal };
            int idx = segs.Count;

            for (int i = 0; i < wantLen; i++)
            {
                Vector3Int c = start + step * i;
                if (!InBounds(c) || owner[c.x, c.y] != -1) break;
                s.cells.Add(c);
                owner[c.x, c.y] = idx;
            }
            segs.Add(s);
        }
    }

    private void PeelAndAssignDirections()
    {
        int idCounter = 0;

        while (segs.Any(x => !x.removed))
        {
            var validOptions = (from i in Enumerable.Range(0, segs.Count)
                                let seg = segs[i]
                                where !seg.removed
                                from dir in GetAllowedDirs(seg)
                                where CanExit(i, dir)
                                select new { segIndex = i, dir }).ToList();

            if (validOptions.Count == 0)
            {
                if (!SplitRandomLongSegment()) break;
                continue;
            }

            var pick = validOptions[Random.Range(0, validOptions.Count)];
            Seg s = segs[pick.segIndex];
            Vector3Int head = GetHead(s, pick.dir);

            int len = s.cells.Count;

            int capacity = VehicleCapacityRules.GetCapacity(len);

            if (capacity <= 0)
                continue;

            vehicles.Add(new VehicleData
            {
                vehicleId = idCounter,
                capacity = capacity,
                length = len,
                gridPostion = head,
                direction = pick.dir
            });

            foreach (var c in s.cells) owner[c.x, c.y] = -1;
            s.removed = true;
            idCounter++;
        }
    }

    private List<MoveDirection> GetAllowedDirs(Seg s)
    {
        if (s.cells.Count == 1)
            return new List<MoveDirection> { MoveDirection.Left, MoveDirection.Right, MoveDirection.Up, MoveDirection.Down };

        return s.horizontal
            ? new List<MoveDirection> { MoveDirection.Left, MoveDirection.Right }
            : new List<MoveDirection> { MoveDirection.Up, MoveDirection.Down };
    }

    private Vector3Int GetHead(Seg s, MoveDirection dir)
    {
        Vector3Int d = GameMechanics.ToDelta(dir);
        return s.cells.OrderByDescending(c => c.x * d.x + c.y * d.y).First();
    }

    private bool CanExit(int segIndex, MoveDirection dir)
    {
        Seg s = segs[segIndex];
        Vector3Int d = GameMechanics.ToDelta(dir);
        Vector3Int c = GetHead(s, dir) + d;

        while (InBounds(c))
        {
            int o = owner[c.x, c.y];
            if (o != -1 && o != segIndex) return false;
            c += d;
        }
        return true;
    }
    private bool SplitRandomLongSegment()
    {
        var longOnes = segs.Select((s, i) => new { s, i })
                           .Where(x => !x.s.removed && x.s.cells.Count > 1)
                           .ToList();

        if (longOnes.Count == 0) return false;

        var pick = longOnes[Random.Range(0, longOnes.Count)];
        Seg old = pick.s;

        old.removed = true;
        foreach (var c in old.cells) owner[c.x, c.y] = -1;

        foreach (var c in old.cells)
        {
            var single = new Seg { horizontal = old.horizontal };
            single.cells.Add(c);
            owner[c.x, c.y] = segs.Count;
            segs.Add(single);
        }
        return true;
    }
    private void GenerateBalancedPeople()
    {
        personGroups = new List<PersonGroupData>();
        int groupPersonIdCounter = 0;

        foreach (var v in vehicles)
        {
            // QUAN TRỌNG: Tổng số người thuộc xe này PHẢI BẰNG CHÍNH XÁC v.capacity 
            // để xe có thể chở hết người và rời đi, không bị kẹt hay thừa người.
            int remainingCapacity = v.capacity;
            Vector3Int waitSlot = GameMechanics.GetWaitSlot(v.gridPostion, v.direction, width, height);

            // Chia capacity thành các nhóm người nhỏ hơn (ví dụ: nhóm 4, 6, 8)
            while (remainingCapacity > 0)
            {
                int groupCount;

                if (remainingCapacity >= 8)
                    groupCount = 8;
                else if (remainingCapacity >= 6)
                    groupCount = 6;
                else if (remainingCapacity >= 4)
                    groupCount = 4;
                else
                    groupCount = remainingCapacity;

                // Đảm bảo luôn là số chẵn
                if (groupCount % 2 != 0)
                    groupCount = Mathf.Max(2, groupCount - 1);

                // Nhóm cuối cùng gom nốt phần còn lại để khớp hoàn toàn capacity của xe
                if (remainingCapacity - groupCount < 4 && remainingCapacity - groupCount > 0)
                {
                    groupCount = remainingCapacity;
                }

                personGroups.Add(new PersonGroupData
                {
                    groupPersonId = groupPersonIdCounter++,
                    Count = groupCount,
                    slotPosition = waitSlot,
                    ownerVehicleId = v.vehicleId
                });

                remainingCapacity -= groupCount;
            }
        }
    }

    private bool InBounds(Vector3Int c) => c.x >= 0 && c.x < width && c.y >= 0 && c.y < height;
}