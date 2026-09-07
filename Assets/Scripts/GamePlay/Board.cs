using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class Board
{
    public int width, height;
    public float cellSize, spacing;

    public CellState[,] cells;
    public List<VehicleState> vehicles;

    private Dictionary<Vector3Int, List<PersonGroupData>> personMap;

    public enum CellState { Empty, Vehicle }

    private static readonly Color[] AvailableColors =
    {
            new Color(1f, 0.2f, 0.2f),     // Red
            new Color(0.2f, 0.5f, 1f),     // Blue
            new Color(0.2f, 0.8f, 0.3f),   // Green
            new Color(1f, 0.8f, 0.1f),     // Yellow
            new Color(0.7f, 0.3f, 1f),     // Purple
            new Color(1f, 0.5f, 0.1f)      // Orange
        };

    public Color GetRandomColor() => AvailableColors[Random.Range(0, AvailableColors.Length)];

    public static Board FromLevelData(LevelData data)
    {
        Board b = new Board
        {
            width = data.width,
            height = data.height,
            cellSize = data.cellSize,
            spacing = data.spacing,
            cells = new CellState[data.width, data.height],
            vehicles = new List<VehicleState>()
        };

        List<Color> randomColors = GetRandomColors(data.vehicles.Count);

        // Đánh màu theo THỨ TỰ DUYỆT, không theo vehicleId
        // (vehicleId có thể không liên tục nếu xe bị xóa/thêm trong Editor)
        for (int i = 0; i < data.vehicles.Count; i++)
        {
            var v = data.vehicles[i];
            var state = new VehicleState
            {
                id = v.vehicleId,
                color = randomColors[i],
                capacity = v.capacity,
                currentOccupied = 0,
                length = v.length,
                position = v.gridPostion,
                direction = v.direction,
                isActive = true
            };
            b.vehicles.Add(state);

            foreach (var c in b.GetOccupiedCells(state))
                if (b.InBounds(c)) b.cells[c.x, c.y] = CellState.Vehicle;
        }

        // personMap KHÔNG check InBounds vì waitSlot cố ý nằm NGOÀI lưới (x=-1 hoặc x=width)
        b.personMap = new Dictionary<Vector3Int, List<PersonGroupData>>();
        foreach (var p in data.personGroups)
        {
            if (!b.personMap.TryGetValue(p.slotPosition, out var list))
            {
                list = new List<PersonGroupData>();
                b.personMap[p.slotPosition] = list;
            }

            list.Add(new PersonGroupData
            {
                groupPersonId = p.groupPersonId,
                Count = p.Count,
                slotPosition = p.slotPosition,
                ownerVehicleId = p.ownerVehicleId,
            });
        }

        return b;
    }

    public static List<Color> GetRandomColors(int count)
    {
        List<Color> colors = new List<Color>(AvailableColors);
        Shuffle(colors);

        List<Color> result = new List<Color>();
        for (int i = 0; i < count; i++)
            result.Add(colors[i % colors.Count]);

        return result;
    }

    private static void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    public Vector3 GetPositionWorld(int x, int y)
    {
        float step = cellSize + spacing;
        float offsetX = (width - 1) * step / 2;
        float offsetY = (height - 1) * step / 2;
        int flippedY = (height - 1) - y;

        return new Vector3(x * step - offsetX, flippedY * step - offsetY);
    }



    public bool CheckIfPathIsClear(VehicleState v)
    {
        Vector3Int dirDelta = GameMechanics.ToDelta(v.direction);
        int checkX = v.position.x + dirDelta.x;
        int checkY = v.position.y + dirDelta.y;

        while (checkX >= 0 && checkX < width && checkY >= 0 && checkY < height)
        {
            if (cells[checkX, checkY] == CellState.Vehicle) return false;
            checkX += dirDelta.x;
            checkY += dirDelta.y;
        }
        return true;
    }

    public void RemoveVehicleFromBoard(int vehicleId)
    {
        var v = vehicles.FirstOrDefault(x => x.id == vehicleId);
        if (v == null) return;

        foreach (var c in GetOccupiedCells(v))
            if (InBounds(c)) cells[c.x, c.y] = CellState.Empty;

        v.isActive = false;
    }

    private List<Vector3Int> GetOccupiedCells(VehicleState v)
    {
        var result = new List<Vector3Int>();
        Vector3Int backDelta = -GameMechanics.ToDelta(v.direction);
        for (int i = 0; i < v.length; i++)
            result.Add(v.position + backDelta * i);
        return result;
    }

    public bool TryMoveVehicle(int vehicleId, out int cellsMoved)
    {
        cellsMoved = 0;
        var v = vehicles.FirstOrDefault(x => x.id == vehicleId && x.isActive);
        if (v == null) return false;

        Vector3Int delta = GameMechanics.ToDelta(v.direction);

        foreach (var c in GetOccupiedCells(v))
            if (InBounds(c)) cells[c.x, c.y] = CellState.Empty;

        Vector3Int cur = v.position;
        bool moved = false;

        while (true)
        {
            Vector3Int next = cur + delta;

            if (!InBounds(next))
            {
                // Đón khách đang chờ ngay ngoài mép (waitSlot) TRƯỚC khi quyết định thoát
                TryBoardPassengers(v, next);

                if (v.currentOccupied >= v.capacity)
                {
                    v.isActive = false;
                    moved = true;
                }
                break;
            }

            if (cells[next.x, next.y] == CellState.Vehicle)
                break;

            cur = next;
            moved = true;
            cellsMoved++;

            TryBoardPassengers(v, cur);
        }

        v.position = cur;

        if (v.isActive)
        {
            foreach (var c in GetOccupiedCells(v))
                if (InBounds(c)) cells[c.x, c.y] = CellState.Vehicle;
        }

        return moved;
    }

    private void TryBoardPassengers(VehicleState v, Vector3Int cell)
    {
        if (!personMap.TryGetValue(cell, out var groups)) return;

        for (int i = groups.Count - 1; i >= 0; i--)
        {
            if (v.currentOccupied >= v.capacity) break;

            int canTake = Mathf.Min(groups[i].Count, v.capacity - v.currentOccupied);
            v.currentOccupied += canTake;
            groups[i].Count -= canTake;
            if (groups[i].Count <= 0) groups.RemoveAt(i);
        }
    }

    public bool IsCleared() => vehicles.All(v => !v.isActive);

    private bool InBounds(Vector3Int p) => p.x >= 0 && p.x < width && p.y >= 0 && p.y < height;
}
