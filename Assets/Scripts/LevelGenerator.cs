using UnityEngine;
using System.Collections.Generic;

public class LevelGenerator
{
    private int width, height, minCapacity, maxCapacity;

    private List<VehicleData> vehicles;
    private List<PersonGroupData> personGroups;
    // Với mỗi xe, lưu ô "chờ người" ngay ngoài mép thoát của xe đó
    private Dictionary<int, Vector3Int> vehicleWaitSlots;

    public void Setup(int gridWidth, int gridHeight, int vCount, int minCap, int maxCap)
    {
        width = gridWidth;
        height = gridHeight;
        minCapacity = minCap;
        maxCapacity = maxCap;
        // vCount không còn ảnh hưởng vì mỗi dòng được lấp kín tự động,
        // giữ tham số để không phải sửa chỗ gọi Setup().
    }

    public LevelData GenerateLevelData(int levelIndex)
    {
        vehicles = new List<VehicleData>();
        personGroups = new List<PersonGroupData>();
        vehicleWaitSlots = new Dictionary<int, Vector3Int>();

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
       // data.difficultyScore = EstimateDifficulty();
        data.cellSize = 28f;
        data.spacing = 1;
        return data;
    }

    // Xếp kín 1 dòng ngang bằng nhiều xe nối đuôi nhau, từ mép thoát vào trong
    private void PackLaneFull(int row, MoveDirection dir, ref int idCounter)
    {
        // Nếu thoát bên phải -> xe đầu tiên đặt ở cột phải nhất (đi ra trước)
        // Nếu thoát bên trái -> xe đầu tiên đặt ở cột trái nhất
        int startX = dir == MoveDirection.Right ? width - 1 : 0;
        int step = dir == MoveDirection.Right ? -1 : 1;

        int x = startX;
        int cellsLeft = width;

        while (cellsLeft > 0)
        {
            int maxLen = Mathf.Min(3, cellsLeft);
            int length = Random.Range(1, maxLen + 1);
            int capacity = Random.Range(minCapacity, maxCapacity + 1);

            Vector3Int head = new Vector3Int(x, row, 0);

            var vehicle = new VehicleData
            {
                vehicleId = idCounter,
                capacity = capacity,
                length = length,
                gridPostion = head,
                direction = dir
            };
            vehicles.Add(vehicle);

            vehicleWaitSlots[idCounter] = GameMechanics.GetWaitSlot(head, dir, width);

            idCounter++;
            x += step * length;
            cellsLeft -= length;
        }
    }

    private void GenerateBalancedPeople()
    {
        int personIdCounter = 0;

        foreach (var v in vehicles)
        {
            if (!vehicleWaitSlots.TryGetValue(v.vehicleId, out var waitSlot))
                continue;

            // 1 group chứa toàn bộ capacity của xe
            personGroups.Add(new PersonGroupData
            {
                groupPersonId = personIdCounter++,
                Count = v.capacity,
                slotPosition = waitSlot,
                ownerVehicleId = v.vehicleId
            });
        }
    }

    private int EstimateDifficulty()
    {
        int score = vehicles.Count * 10;
        score += personGroups.Count * 2;
        return score;
    }
}