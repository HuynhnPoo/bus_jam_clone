using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine;

public static class VehicleCapacityRules
{
    // Cố định theo length hoặc random ngẫu nhiên các số chẵn (4, 6, 8, 10)
    public static int GetCapacity(int length)
    {
        switch (length)
        {
            case 1: return 4;
            case 2: return 6;
            case 3: return 8;
            default: return 4;
        }
    }

    // Hoặc hàm random sức chứa chẵn từ 4 đến 10 dựa vào length để level đa dạng hơn
    public static int GetRandomCapacity(int length)
    {
        int[] possibleCaps = length switch
        {
            1 => new int[] { 4, 6 },
            2 => new int[] { 6, 8 },
            3 => new int[] { 8, 10 },
            _ => new int[] { 4, 6 }
        };
        return possibleCaps[Random.Range(0, possibleCaps.Length)];
    }
}
public static class LevelSolver
{
    public static bool Verify(LevelData data)
    {
        if (!ValidateCapacity(data))
            return false;

        Board board = Board.FromLevelData(data);

        int safety = 0;

        while (!board.IsCleared() && safety < 500)
        {
            safety++;

            bool progressed = false;

            foreach (var v in board.vehicles
                         .Where(x => x.isActive)
                         .ToList())
            {
                if (board.TryMoveVehicle(v.id, out int progress)
                    && progress > 0)
                {
                    progressed = true;
                }
            }

            if (!progressed)
                return false;
        }

        return board.IsCleared();
    }
    private static bool ValidateCapacity(LevelData data)
    {
        if (data == null ||
            data.vehicles == null ||
            data.personGroups == null ||
            data.width <= 0 ||
            data.height <= 0)
            return false;

        HashSet<int> vehicleIds = new HashSet<int>();

        Dictionary<int, VehicleData> vehicleMap =
            new Dictionary<int, VehicleData>();

        Dictionary<int, int> personCounts =
            new Dictionary<int, int>();

        bool[,] occupied = new bool[data.width, data.height];

        Vector3Int zero = Vector3Int.zero;

        // 1. Kiểm tra xe.
        foreach (var v in data.vehicles)
        {
            if (!vehicleIds.Add(v.vehicleId))
            {
                Debug.LogError($"Trùng Vehicle ID: {v.vehicleId}");
                return false;
            }

            if (v.length < 1 || v.length > 3)
            {
                Debug.LogError($"Xe {v.vehicleId} có length không hợp lệ.");
                return false;
            }

            int expectedCapacity =
                VehicleCapacityRules.GetCapacity(v.length);

            if (v.capacity != expectedCapacity)
            {
                Debug.LogError(
                    $"Xe {v.vehicleId}: Length={v.length}, " +
                    $"Capacity={v.capacity}, Expected={expectedCapacity}"
                );
                return false;
            }

            Vector3Int delta = GameMechanics.ToDelta(v.direction);

            if (delta == zero)
            {
                Debug.LogError($"Xe {v.vehicleId} có direction không hợp lệ.");
                return false;
            }

            Vector3Int backDelta = -delta;

            for (int i = 0; i < v.length; i++)
            {
                Vector3Int cell = v.gridPostion + backDelta * i;

                if (cell.x < 0 || cell.x >= data.width ||
                    cell.y < 0 || cell.y >= data.height)
                {
                    Debug.LogError(
                        $"Xe {v.vehicleId} nằm ngoài grid tại {cell}"
                    );
                    return false;
                }

                if (occupied[cell.x, cell.y])
                {
                    Debug.LogError(
                        $"Xe {v.vehicleId} bị overlap tại {cell}"
                    );
                    return false;
                }

                occupied[cell.x, cell.y] = true;
            }

            vehicleMap.Add(v.vehicleId, v);
            personCounts.Add(v.vehicleId, 0);
        }

        // 2. Kiểm tra các nhóm người.
        foreach (var p in data.personGroups)
        {
            if (p.Count <= 0)
            {
                Debug.LogError(
                    $"PersonGroup {p.groupPersonId} có Count không hợp lệ."
                );
                return false;
            }

            if (!vehicleMap.TryGetValue(
                    p.ownerVehicleId, out VehicleData owner))
            {
                Debug.LogError(
                    $"PersonGroup {p.groupPersonId} không tìm thấy xe sở hữu."
                );
                return false;
            }

            Vector3Int expectedSlot = GameMechanics.GetWaitSlot(
                owner.gridPostion,
                owner.direction,
                data.width,
                data.height
            );

            if (p.slotPosition != expectedSlot)
            {
                Debug.LogError(
                    $"PersonGroup {p.groupPersonId} sai vị trí chờ. " +
                    $"Expected={expectedSlot}, Actual={p.slotPosition}"
                );
                return false;
            }

            personCounts[p.ownerVehicleId] += p.Count;
        }

        // 3. Tổng số người phải bằng chính xác capacity của từng xe.
        foreach (var v in data.vehicles)
        {
            int totalPeople = personCounts[v.vehicleId];

            if (totalPeople != v.capacity)
            {
                Debug.LogError(
                    $"Xe {v.vehicleId}: Capacity={v.capacity}, " +
                    $"TotalPeople={totalPeople}"
                );
                return false;
            }
        }

        return true;
    }


    // Dùng hàm ValidateCapacity ở phần tiếp theo.
}