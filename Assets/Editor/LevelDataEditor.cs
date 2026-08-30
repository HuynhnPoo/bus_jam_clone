using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;

[CustomEditor(typeof(LevelData))]
public class LevelDataEditor : Editor
{
    int selectedVehicleId = -1;
    Vector3Int? pendingNewVehicleCell = null;

    Color newVehicleColor = Color.red;
    int newVehicleCapacity = 4; // sô lương người trên xe
    int newVehicleLength = 2;
    MoveDirection newVehicleDir = MoveDirection.Right;

    bool isSolvable;
    bool checkedOnce = false;

    public override void OnInspectorGUI()
    {
        LevelData data = (LevelData)target;

        EditorGUILayout.LabelField("Level Info", EditorStyles.boldLabel);
        EditorGUILayout.LabelField($"Grid: {data.width} x {data.height}");
        EditorGUILayout.LabelField($"Số xe: {data.vehicles.Count}");
        EditorGUILayout.LabelField($"Số nhóm người: {data.personGroups.Count}");

        EditorGUILayout.Space(8);

        if (GUILayout.Button("✅ Kiểm tra Solvable"))
        {
            isSolvable = LevelSolver.Verify(data);
            checkedOnce = true;
        }

        if (checkedOnce)
        {
            EditorGUILayout.HelpBox(
                isSolvable ? "✅ Level giải được" : "❌ Level KHÔNG giải được",
                isSolvable ? MessageType.Info : MessageType.Error
            );
        }

        EditorGUILayout.Space(10);
        DrawGridPreview(data);

        EditorGUILayout.Space(10);
        if (GUILayout.Button("💾 Lưu thay đổi vào asset", GUILayout.Height(28)))
        {
            EditorUtility.SetDirty(data);
            AssetDatabase.SaveAssets();
            Debug.Log($"✅ Đã lưu thay đổi cho {data.name}");
        }

        EditorGUILayout.Space(6);
        if (GUILayout.Button("🛠 Rebuild toàn bộ người (fix data cũ)", GUILayout.Height(28)))
        {
            RebuildAllPersonGroups(data);
            isSolvable = LevelSolver.Verify(data);
            checkedOnce = true;
            EditorUtility.SetDirty(data);
            Repaint();
        }
    }

    void DrawGridPreview(LevelData data)
    {
        GUILayout.Label("Preview Grid (click ô để chọn/thêm xe)", EditorStyles.boldLabel);

        EditorGUI.BeginChangeCheck();
        // Giữ nguyên slider của bạn từ 2 đến 10
        data.cellSize = EditorGUILayout.Slider("Cell Size", data.cellSize, 0, 2);
        data.spacing = EditorGUILayout.Slider("Spacing", data.spacing, 0f, 4f);
        EditorUtility.SetDirty(data);

        // Bảo vệ dữ liệu đầu vào
        float safeCellSize = data.cellSize > 0 ? data.cellSize : 4f;
        float safeSpacing = data.spacing >= 0 ? data.spacing : 1f;

        // 🆕 GIẢI PHÁP: Tạo hệ số phóng to riêng cho Editor giao diện trực quan
        // Nhân kích thước gốc với 8 hoặc 10 lần để ô màu to ra rõ ràng trên UI
        float visualMultiplier = 10f;

        float editorCellSize = safeCellSize * visualMultiplier;
        float editorSpacing = safeSpacing * visualMultiplier;

        EditorGUILayout.Space(4);

        // Tính toán kích thước tổng của bảng Preview dựa trên kích thước Editor mới
        Rect gridRect = GUILayoutUtility.GetRect(
            data.width * (editorCellSize + editorSpacing) + 20,
            data.height * (editorCellSize + editorSpacing) + 20
        );
        Event e = Event.current;

        for (int x = 0; x < data.width; x++)
        {
            for (int y = 0; y < data.height; y++)
            {
                // 💡 Các ô con đồng loạt to lên theo tỷ lệ chuẩn, không bị lệch hay thưa thớt
                Rect cellRect = new Rect(
                    gridRect.x + x * (editorCellSize + editorSpacing) + 10,
                    gridRect.y + y * (editorCellSize + editorSpacing) + 10,
                    editorCellSize, // Kích thước chiều rộng của ô màu nay đã to ra!
                    editorCellSize  // Kích thước chiều cao của ô màu
                );

                var vehicleHere = data.vehicles.FirstOrDefault(v => IsVehicleAtCell(v, new Vector3Int(x, y, 0)));
                if (vehicleHere != null)
                {
                    bool selected = vehicleHere.vehicleId == selectedVehicleId;

                    DrawVehicleOutline(
                        cellRect,
                        selected ? Color.white : Color.gray,
                        selected ? 3f : 2f
                    );

                    if (vehicleHere.gridPostion == new Vector3Int(x, y, 0))
                    {
                        GUI.Label(
                            cellRect,
                            $"#{vehicleHere.vehicleId}\n{GetDirectionSymbol(vehicleHere.direction)}",
                            new GUIStyle(EditorStyles.boldLabel)
                            {
                                alignment = TextAnchor.MiddleCenter
                            });
                    }
                }

                // Vẽ viền trắng xung quanh ô được chọn
                if (vehicleHere != null && vehicleHere.vehicleId == selectedVehicleId)
                    EditorGUI.DrawRect(new Rect(cellRect.x - 2, cellRect.y - 2, cellRect.width + 4, cellRect.height + 4), Color.white);

                // Vẽ ô màu thực tế

                // Xử lý sự kiện click chuột
                if (e.type == EventType.MouseDown && cellRect.Contains(e.mousePosition))
                {
                    if (vehicleHere != null)
                    {
                        selectedVehicleId = vehicleHere.vehicleId;
                        pendingNewVehicleCell = null;
                    }
                    else
                    {
                        pendingNewVehicleCell = new Vector3Int(x, y, 0);
                        selectedVehicleId = -1;
                    }
                    e.Use(); // Đánh dấu đã xử lý để tránh click xuyên qua UI khác
                    Repaint();
                }
            }
        }

        if (selectedVehicleId >= 0)
            DrawEditVehiclePanel(data);
        else if (pendingNewVehicleCell.HasValue)
            DrawAddVehiclePanel(data);
    }

    void DrawVehicleOutline(Rect rect, Color color, float thickness)
    {
        EditorGUI.DrawRect(
            new Rect(rect.x, rect.y, rect.width, thickness),
            color);

        EditorGUI.DrawRect(
            new Rect(rect.x, rect.yMax - thickness, rect.width, thickness),
            color);

        EditorGUI.DrawRect(
            new Rect(rect.x, rect.y, thickness, rect.height),
            color);

        EditorGUI.DrawRect(
            new Rect(rect.xMax - thickness, rect.y, thickness, rect.height),
            color);
    }
    void DrawAddVehiclePanel(LevelData data)
    {
        var cell = pendingNewVehicleCell.Value;
        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField($"Thêm xe tại ô ({cell.x},{cell.y})", EditorStyles.boldLabel);

        newVehicleColor = EditorGUILayout.ColorField("Màu", newVehicleColor);
        newVehicleLength = EditorGUILayout.IntSlider("Độ dài xe", newVehicleLength, 1, 3);

        newVehicleCapacity = CalculateCapacity(newVehicleLength);
        EditorGUILayout.LabelField($"sức chưa :{newVehicleCapacity}");
        newVehicleDir = (MoveDirection)EditorGUILayout.EnumPopup("Hướng", newVehicleDir);

        bool wantAdd = false;
        bool wantCancel = false;

        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("➕ Thêm xe")) wantAdd = true;
            if (GUILayout.Button("Hủy")) wantCancel = true;
        }

        if (wantAdd)
        {
            int newId = data.vehicles.Count > 0 ? data.vehicles.Max(v => v.vehicleId) + 1 : 0;

            var newVehicle = new VehicleData
            {
                vehicleId = newId,
                capacity = newVehicleCapacity,
                length = newVehicleLength,
                gridPostion = cell,
                direction = newVehicleDir
       
            };

            data.vehicles.Add(newVehicle);
            RegeneratePersonGroupForVehicle(data, newVehicle); // sinh người khớp capacity

            pendingNewVehicleCell = null;
            isSolvable = LevelSolver.Verify(data);
            checkedOnce = true;
            EditorUtility.SetDirty(data);
            Repaint();
        }
        else if (wantCancel)
        {
            pendingNewVehicleCell = null;
        }
    }

    void DrawEditVehiclePanel(LevelData data)
    {
        var v = data.vehicles.FirstOrDefault(x => x.vehicleId == selectedVehicleId);
        if (v == null) { selectedVehicleId = -1; return; }

        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField($"Đang sửa xe #{v.vehicleId}", EditorStyles.boldLabel);

        v.length = EditorGUILayout.IntSlider("Độ dài xe", v.length, 1, 3);
        v.direction = (MoveDirection)EditorGUILayout.EnumPopup("Hướng", v.direction);

        v.capacity = CalculateCapacity(v.length);
        EditorGUILayout.LabelField($"Sức chứa :{v.capacity}");
        Vector2Int pos2D = new Vector2Int(v.gridPostion.x, v.gridPostion.y);
        pos2D = EditorGUILayout.Vector2IntField("Vị trí đầu xe", pos2D);
        v.gridPostion = new Vector3Int(pos2D.x, pos2D.y, 0);

        EditorGUILayout.Space(6);

        bool wantDelete = false;
        bool wantRecheck = false;
        bool wantSyncPeople = false;

        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("🗑 Xóa xe này")) wantDelete = true;
            if (GUILayout.Button("✅ Re-check Solvable")) wantRecheck = true;
        }

        if (GUILayout.Button("🔄 Đồng bộ lại số người theo Capacity"))
            wantSyncPeople = true;

        if (GUILayout.Button("Đóng panel"))
            selectedVehicleId = -1;

        if (wantDelete)
        {
            Vector3Int slot = GetWaitSlot(v, data.width);
            RemovePersonGroupsAtSlot(data, v.vehicleId); // xóa luôn người của xe này

            data.vehicles.Remove(v);
            selectedVehicleId = -1;
            isSolvable = LevelSolver.Verify(data);
            checkedOnce = true;
            EditorUtility.SetDirty(data);
            Repaint();
        }
        else if (wantRecheck)
        {
            isSolvable = LevelSolver.Verify(data);
            checkedOnce = true;
        }
        else if (wantSyncPeople)
        {
            RegeneratePersonGroupForVehicle(data, v);
            isSolvable = LevelSolver.Verify(data);
            checkedOnce = true;
            EditorUtility.SetDirty(data);
            Repaint();
        }
    }

    bool IsVehicleAtCell(VehicleData v, Vector3Int cell)
    {
        Vector3Int backDelta = -DirToDelta(v.direction);
        for (int i = 0; i < v.length; i++)
            if (v.gridPostion + backDelta * i == cell) return true;
        return false;
    }

    Vector3Int DirToDelta(MoveDirection d) => d switch
    {
        MoveDirection.Up => Vector3Int.up,
        MoveDirection.Down => Vector3Int.down,
        MoveDirection.Left => Vector3Int.left,
        MoveDirection.Right => Vector3Int.right,
        _ => Vector3Int.zero
    };

    Vector3Int GetWaitSlot(VehicleData v, int gridWidth)
    {
        return v.direction == MoveDirection.Right
            ? new Vector3Int(gridWidth, v.gridPostion.y, 0)
            : new Vector3Int(-1, v.gridPostion.y, 0);
    }

    void RemovePersonGroupsAtSlot(LevelData data, int vehicleID)
    {
        data.personGroups.RemoveAll(p => p.ownerVehicleId == vehicleID);
    }

    void RegeneratePersonGroupForVehicle(LevelData data, VehicleData v)
    {
        Vector3Int slot = GetWaitSlot(v, data.width);
        RemovePersonGroupsAtSlot(data, v.vehicleId);
        v.capacity = CalculateCapacity(v.length);

        int nextPersonId = v.vehicleId;
        int remaining = v.capacity;
        while (remaining > 0)
        {
            int groupSize = Mathf.Min(Random.Range(4, 7), remaining);
            data.personGroups.Add(new PersonGroupData
            {
                 groupPersonId = v.vehicleId,
                Count = groupSize,
                slotPosition = slot,
                ownerVehicleId = v.vehicleId,
            });
            remaining -= groupSize;
        }
    }
    void RebuildAllPersonGroups(LevelData data)
    {
        // Xóa toàn bộ person cũ
        data.personGroups.Clear();

       // int currentPersonId = 0;

        foreach (var v in data.vehicles)
        {
            // Capacity phụ thuộc vào length
            v.capacity = CalculateCapacity(v.length);

            // Vị trí người chờ của xe
            Vector3Int waitSlot = GetWaitSlot(v, data.width);

            int remainingPeople = v.capacity;

            while (remainingPeople > 0)
            {
                int groupSize = Mathf.Min( Random.Range(4, 7),remainingPeople
                );

                data.personGroups.Add(new PersonGroupData
                {
                    groupPersonId = v.vehicleId,
                    Count = groupSize,
                    slotPosition = waitSlot,

                    // Quan trọng:
                    // Người này thuộc về xe nào
                    ownerVehicleId = v.vehicleId
                });

                remainingPeople -= groupSize;
            }
        }

        EditorUtility.SetDirty(data);

        int totalSeats = data.vehicles.Sum(v => v.capacity);
        int totalPeople = data.personGroups.Sum(p => p.Count);

        Debug.Log(
            $"✅ Rebuild thành công!\n" +
            $"Xe: {data.vehicles.Count}\n" +
            $"Tổng chỗ: {totalSeats}\n" +
            $"Tổng người: {totalPeople}\n" +
            $"Số nhóm người: {data.personGroups.Count}"
        );
    }
    int CalculateCapacity(int length)
    {
        // 1 ô -> 4 chỗ | 2 ô -> 6 chỗ | 3 ô -> 8 chỗ
        return (length * 2) + 2;
    }

    string GetDirectionSymbol(MoveDirection direction)
    {
        return direction switch
        {
            MoveDirection.Up => "↑",
            MoveDirection.Down => "↓",
            MoveDirection.Left => "←",
            MoveDirection.Right => "→",
            _ => "?"
        };
    }
}