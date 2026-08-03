using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using JetBrains.Annotations;

public class VehicleState
{
    public int id;
    public Color color;
    public int capacity; // số lượng xe cso thể chứa
    public int currentOccupied; // sô người đã lên xe
    public int length;
    public Vector3Int position;
    public MoveDirection direction;
    public bool isActive;

    public bool IsFull => currentOccupied >= capacity;
    public int AvailabeSeats => capacity - currentOccupied; // conf trong

    public void GotOnBus()
    {
        if(currentOccupied< capacity)
        {
            currentOccupied++;
        }
    }
}

public class Board
{
    public int width, height;
    public float cellSize, spacing;

    public CellState[,] cells;
    public List<VehicleState> vehicles;

    Dictionary<Vector3Int, List<PersonGroupData>> personMap;

    public enum CellState { Empty, Vehicle }



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

        foreach (var v in data.vehicles)
        {
            var state = new VehicleState
            {
                id = v.vehicleId,
                color = v.vehicleColor,
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

        // personMap KHÔNG check InBounds ở đây vì waitSlot cố ý nằm NGOÀI lưới (x=-1 hoặc x=width)
        b.personMap = new Dictionary<Vector3Int, List<PersonGroupData>>();
        foreach (var p in data.personGroups)
        {
            if (!b.personMap.ContainsKey(p.slotPosition))
                b.personMap[p.slotPosition] = new List<PersonGroupData>();

            b.personMap[p.slotPosition].Add(new PersonGroupData
            {
                colorPerson = p.colorPerson,
                Count = p.Count,
                slotPosition = p.slotPosition
            });
        }

        return b;
    }

    public Vector3 GetPostionWorld(int x, int y)
    {
        float step = cellSize + spacing;

        float offsetX = (width - 1) * step / 2;
        float offsetY = (height - 1) * step / 2;

        int flippedY = (height - 1) - y;

        return new Vector3(x * step - offsetX, flippedY * step - offsetY);
    }
    public Quaternion GetRotationWorld(MoveDirection dir) => dir switch
    {
        MoveDirection.Up => Quaternion.LookRotation(Vector3.right),
        MoveDirection.Down => Quaternion.LookRotation(Vector3.left),
        MoveDirection.Left => Quaternion.LookRotation(Vector3.back),
        MoveDirection.Right => Quaternion.LookRotation(Vector3.forward),
        _ => Quaternion.identity

    };

    public bool CheckIfPathIsClear(VehicleState vehicleData)
    {
        Vector3Int currentPos = vehicleData.position;

        Vector3Int dirDelta = DirToDelta(vehicleData.direction);

        int checkX = currentPos.x + dirDelta.x;
        int checkY = currentPos.y + dirDelta.y;

        while (checkX>=0 && checkX <width && checkY>=0 && checkY <height )
        {
            if (cells[checkX,checkY]==CellState.Vehicle)
            {
                return false;
            }

            checkX += dirDelta.x;
            checkY += dirDelta.y;
        }
        
        return true;
    }

    public void RemoveVehicleFromBoard(int vehicalID)
    {
        var v = vehicles.FirstOrDefault(x => x.id == vehicalID);
        if (v == null) return;

        foreach (var c in GetOccupiedCells(v)) 
        {
            if (InBounds(c))
            {
                cells[c.x, c.y] = CellState.Empty;
            }
        }
        v.isActive = false;
    }


    // cho scipts table object

    List<Vector3Int> GetOccupiedCells(VehicleState v)
    {
        var cells = new List<Vector3Int>();
        Vector3Int backDelta = -DirToDelta(v.direction);
        for (int i = 0; i < v.length; i++)
            cells.Add(v.position + backDelta * i);
        return cells;
    }

    public bool TryMoveVehicle(int vehicleId, out int cellsMoved)
    {
        cellsMoved = 0;
        var v = vehicles.FirstOrDefault(x => x.id == vehicleId && x.isActive);
        if (v == null) return false;

        Vector3Int delta = DirToDelta(v.direction);

        foreach (var c in GetOccupiedCells(v))
            if (InBounds(c)) cells[c.x, c.y] = CellState.Empty;

        Vector3Int cur = v.position;
        bool moved = false;

        while (true)
        {
            Vector3Int next = cur + delta;

            if (!InBounds(next))
            {
                // ✅ FIX: Đón khách đang chờ ngay ngoài mép (waitSlot) TRƯỚC khi quyết định thoát
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

    void TryBoardPassengers(VehicleState v, Vector3Int cell)
    {
        if (!personMap.TryGetValue(cell, out var groups)) return;

        for (int i = groups.Count - 1; i >= 0; i--)
        {
            if (groups[i].colorPerson == v.color && v.currentOccupied < v.capacity)
            {
                int canTake = Mathf.Min(groups[i].Count, v.capacity - v.currentOccupied);
                v.currentOccupied += canTake;
                groups[i].Count -= canTake;
                if (groups[i].Count <= 0) groups.RemoveAt(i);
            }
        }
    }

    public bool IsCleared() => vehicles.All(v => !v.isActive);

    bool InBounds(Vector3Int p) => p.x >= 0 && p.x < width && p.y >= 0 && p.y < height;

   public Vector3Int DirToDelta(MoveDirection d) => d switch
    {
        MoveDirection.Up => Vector3Int.up,
        MoveDirection.Down => Vector3Int.down,
        MoveDirection.Left => Vector3Int.left,
        MoveDirection.Right => Vector3Int.right,
        _ => Vector3Int.zero
    };

}