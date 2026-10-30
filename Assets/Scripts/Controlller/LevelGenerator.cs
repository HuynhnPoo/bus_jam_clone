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
        minCapacity = Mathf.Max(1, minCap);
        maxCapacity = Mathf.Max(minCapacity, maxCap);
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

            // TÍNH SỨC CHỨA: Đảm bảo luôn là số chẵn (2, 4, 6, 8) theo độ dài xe
            int capacity = len switch
            {
                1 => 4, // 2 hoặc 4 người cho xe 1 ô
                2 => 6, // 4 hoặc 6 người cho xe 2 ô
                _ => 8  // 6 hoặc 8 người cho xe 3 ô
            };
            capacity = Mathf.Clamp(capacity, minCapacity, maxCapacity);

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
        // 1. Tạo danh sách các nhóm người cơ bản dựa theo xe (giống như trước để đảm bảo đủ tổng số người)
        List<int> groupCounts = new List<int>();
        foreach (var v in vehicles)
        {
            groupCounts.Add(v.capacity);
        }

        int totalPersonsNeeded = groupCounts.Sum();

        // 2. Phân phối lại số lượng ngẫu nhiên (chẵn, từ 4 đến 10) nhưng vẫn giữ nguyên tổng số người
        List<int> randomizedCounts = new List<int>();
        int currentSum = 0;

        for (int i = 0; i < vehicles.Count; i++)
        {
            if (i == vehicles.Count - 1)
            {
                // Nhóm cuối cùng nhận phần số lượng còn thiếu để khớp tổng số người của màn chơi
                int remaining = totalPersonsNeeded - currentSum;
                // Đảm bảo số lượng cuối cùng vẫn là số chẵn và nằm trong khoảng [4, 10] (nếu có thể)
                remaining = Mathf.Clamp(remaining, 4, 10);
                if (remaining % 2 != 0) remaining = Mathf.Max(4, remaining - 1); // Bắt buộc chẵn

                // Nếu vì ép chẵn mà lệch tổng, ta chấp nhận điều chỉnh nhẹ ở xe cuối cùng
                randomizedCounts.Add(remaining);
            }
            else
            {
                // Random ngẫu nhiên các số chẵn từ 4 đến 10: 4, 6, 8, 10
                int randomCount = Random.Range(2, 6) * 2; // Tạo ra 4, 6, 8, hoặc 10

                // Đảm bảo không lấy quá nhiều khiến các xe sau không đủ người hoặc bị âm
                int maxAllowed = totalPersonsNeeded - currentSum - (vehicles.Count - 1 - i) * 4;
                int minAllowed = 4;

                if (maxAllowed < minAllowed) maxAllowed = minAllowed;
                randomCount = Mathf.Clamp(randomCount, minAllowed, Mathf.Min(10, maxAllowed));
                if (randomCount % 2 != 0) randomCount--;

                randomizedCounts.Add(randomCount);
                currentSum += randomCount;
            }
        }

        // Nếu tổng sau khi random bị lệch so với tổng sức chứa xe, ta bù trừ vào nhóm đầu tiên cho khớp hoàn toàn
        int diff = totalPersonsNeeded - randomizedCounts.Sum();
        if (diff != 0 && randomizedCounts.Count > 0)
        {
            randomizedCounts[0] += diff;
            // Đảm bảo nhóm đầu tiên sau khi bù vẫn là số chẵn và >= 4
            if (randomizedCounts[0] < 4) randomizedCounts[0] = 4;
            if (randomizedCounts[0] % 2 != 0) randomizedCounts[0] += (diff > 0 ? 1 : -1);
        }

        // 3. Tạo dữ liệu PersonGroupData dựa trên danh sách số lượng đã xáo trộn ngẫu nhiên
        for (int i = 0; i < vehicles.Count; i++)
        {
            var v = vehicles[i];
            Vector3Int waitSlot = GameMechanics.GetWaitSlot(v.gridPostion, v.direction, width, height);

            personGroups.Add(new PersonGroupData
            {
                groupPersonId = i,
                Count = randomizedCounts[i], // Sử dụng số lượng ngẫu nhiên (chẵn từ 4 đến 10)
                slotPosition = waitSlot,
                ownerVehicleId = v.vehicleId
            });
        }
    }

    private bool InBounds(Vector3Int c) => c.x >= 0 && c.x < width && c.y >= 0 && c.y < height;
}