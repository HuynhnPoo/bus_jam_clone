using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Collections.Generic;
using Random = UnityEngine.Random;

// Gom toàn bộ phần "vẽ grid preview + thêm/sửa/xóa xe + sinh người theo capacity"
// dùng chung cho cả LevelDataEditor (Inspector) và LevelGeneratorWindow (EditorWindow).
public class VehicleGridPanel
{
    public int selectedVehicleId = -1;
    public Vector3Int? pendingNewVehicleCell = null;

    public Color newVehicleColor = Color.red;
    public int newVehicleLength = 2;
    public MoveDirection newVehicleDir = MoveDirection.Right;
    public int newVehicleCapacity = 4;

    // Nếu khác null: capacity tự tính từ length (kiểu LevelDataEditor).
    // Nếu null: cho phép người dùng tự chọn capacity bằng slider (kiểu LevelGeneratorWindow).
    private readonly Func<int, int> autoCapacityFromLength;

    // Gọi mỗi khi dữ liệu thay đổi để caller tự Repaint / SetDirty / verify solvable.
    public event Action OnChanged;

    public VehicleGridPanel(Func<int, int> autoCapacityFromLength = null)
    {
        this.autoCapacityFromLength = autoCapacityFromLength;
        if (autoCapacityFromLength != null)
            newVehicleCapacity = autoCapacityFromLength(newVehicleLength);
    }
    public void DrawGridPreview(LevelData data, float visualMultiplier = 4f)
    {
        GUILayout.Label("Preview Grid (click ô để chọn/thêm xe)", EditorStyles.boldLabel);

        data.cellSize = EditorGUILayout.Slider("Cell Size", data.cellSize, 0f, 2f);
        data.spacing = EditorGUILayout.Slider("Spacing", data.spacing, 0f, 4f);
        EditorUtility.SetDirty(data);

        float safeCellSize = data.cellSize > 0 ? data.cellSize : 4f;
        float safeSpacing = data.spacing >= 0 ? data.spacing : 1f;

        float editorCellSize = safeCellSize * visualMultiplier;
        float editorSpacing = safeSpacing * visualMultiplier;

        EditorGUILayout.Space(4);

        Rect gridRect = GUILayoutUtility.GetRect(
            data.width * (editorCellSize + editorSpacing) + 20,
            data.height * (editorCellSize + editorSpacing) + 20
        );
        Event e = Event.current;

        for (int x = 0; x < data.width; x++)
        {
            for (int y = 0; y < data.height; y++)
            {
                Rect cellRect = new Rect(
                    gridRect.x + x * (editorCellSize + editorSpacing) + 10,
                    gridRect.y + y * (editorCellSize + editorSpacing) + 10,
                    editorCellSize, editorCellSize
                );

                var vehicleHere = data.vehicles.FirstOrDefault(v => IsVehicleAtCell(v, new Vector3Int(x, y, 0)));

                if (vehicleHere == null)
                {
                    EditorGUI.DrawRect(cellRect, new Color(1f, 1f, 1f, 0.06f));
                    DrawVehicleOutline(cellRect, new Color(1f, 1f, 1f, 0.2f), 1f);
                }
                else
                {
                    bool selected = vehicleHere.vehicleId == selectedVehicleId;
                    EditorGUI.DrawRect(cellRect, new Color(0.15f, 0.15f, 0.15f, 0.9f));
                    Color outlineColor = selected ? new Color(1f, 0.85f, 0f) : Color.white;
                    DrawVehicleOutline(cellRect, outlineColor, selected ? 4f : 2.5f);

                    if (vehicleHere.gridPostion == new Vector3Int(x, y, 0))
                    {
                        var idStyle = new GUIStyle(EditorStyles.miniBoldLabel) { normal = { textColor = Color.white } };
                        GUI.Label(new Rect(cellRect.x + 2, cellRect.y + 1, cellRect.width, 14), $"#{vehicleHere.vehicleId}", idStyle);

                        DrawOutlinedArrow(
                            cellRect,
                            GameMechanics.ToSymbol(vehicleHere.direction),
                            Mathf.Max(14, (int)(cellRect.height * 0.6f)),
                            selected ? new Color(1f, 0.85f, 0f) : Color.white
                        );
                    }
                }

                // --- Xử lý click chuột vào ô ---
                if (e.type == EventType.MouseDown && e.button == 0 && cellRect.Contains(e.mousePosition))
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

                    e.Use();
                    OnChanged?.Invoke();
                }
            }
        }

        if (selectedVehicleId >= 0)
            DrawEditVehiclePanel(data);
        else if (pendingNewVehicleCell.HasValue)
            DrawAddVehiclePanel(data);
    }

    private void DrawOutlinedArrow(Rect rect, string symbol, int fontSize, Color textColor)
    {
        var baseStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = fontSize,
            fontStyle = FontStyle.Bold
        };

        // 4 lớp đen lệch nhẹ để tạo viền outline quanh chữ, giúp mũi tên nổi trên mọi nền
        var outlineStyle = new GUIStyle(baseStyle) { normal = { textColor = Color.black } };
        Vector2[] offsets = { new Vector2(-1, -1), new Vector2(1, -1), new Vector2(-1, 1), new Vector2(1, 1) };
        foreach (var off in offsets)
        {
            Rect r = new Rect(rect.x + off.x, rect.y + off.y, rect.width, rect.height);
            GUI.Label(r, symbol, outlineStyle);
        }

        // Lớp màu chính đè lên trên cùng
        var fillStyle = new GUIStyle(baseStyle) { normal = { textColor = textColor } };
        GUI.Label(rect, symbol, fillStyle);
    }
    //private void DrawOutlinedArrow(Rect rect, string symbol, int fontSize, Color textColor)
    //{
    //    var baseStyle = new GUIStyle(EditorStyles.boldLabel)
    //    {
    //        alignment = TextAnchor.MiddleCenter,
    //        fontSize = fontSize,
    //        fontStyle = FontStyle.Bold
    //    };

    //    // Vẽ 4 lớp đen lệch nhẹ quanh vị trí gốc để tạo viền (outline) cho chữ
    //    var outlineStyle = new GUIStyle(baseStyle) { normal = { textColor = Color.black } };
    //    Vector2[] offsets = { new Vector2(-1, -1), new Vector2(1, -1), new Vector2(-1, 1), new Vector2(1, 1) };
    //    foreach (var off in offsets)
    //    {
    //        Rect r = new Rect(rect.x + off.x, rect.y + off.y, rect.width, rect.height);
    //        GUI.Label(r, symbol, outlineStyle);
    //    }

    //    // Vẽ lớp màu chính đè lên trên cùng
    //    var fillStyle = new GUIStyle(baseStyle) { normal = { textColor = textColor } };
    //    GUI.Label(rect, symbol, fillStyle);
    //}

    private void DrawVehicleOutline(Rect rect, Color color, float thickness)
    {
        EditorGUI.DrawRect(new Rect(rect.x, rect.y, rect.width, thickness), color);
        EditorGUI.DrawRect(new Rect(rect.x, rect.yMax - thickness, rect.width, thickness), color);
        EditorGUI.DrawRect(new Rect(rect.x, rect.y, thickness, rect.height), color);
        EditorGUI.DrawRect(new Rect(rect.xMax - thickness, rect.y, thickness, rect.height), color);
    }

    private void DrawAddVehiclePanel(LevelData data)
    {
        var cell = pendingNewVehicleCell.Value;
        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField($"Thêm xe tại ô ({cell.x},{cell.y})", EditorStyles.boldLabel);

        newVehicleColor = EditorGUILayout.ColorField("Màu", newVehicleColor);
        newVehicleLength = EditorGUILayout.IntSlider("Độ dài xe", newVehicleLength, 1, 3);

        if (autoCapacityFromLength != null)
        {
            newVehicleCapacity = autoCapacityFromLength(newVehicleLength);
            EditorGUILayout.LabelField($"Sức chứa: {newVehicleCapacity}");
        }
        else
        {
            newVehicleCapacity = EditorGUILayout.IntSlider("Sức chứa", newVehicleCapacity, 5, 20);
        }

        newVehicleDir = (MoveDirection)EditorGUILayout.EnumPopup("Hướng", newVehicleDir);

        bool wantAdd = false, wantCancel = false;
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
            RegeneratePersonGroupForVehicle(data, newVehicle);

            pendingNewVehicleCell = null;
            EditorUtility.SetDirty(data);
            OnChanged?.Invoke();
        }
        else if (wantCancel)
        {
            pendingNewVehicleCell = null;
        }
    }

    private void DrawEditVehiclePanel(LevelData data)
    {
        var v = data.vehicles.FirstOrDefault(x => x.vehicleId == selectedVehicleId);
        if (v == null) { selectedVehicleId = -1; return; }

        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField($"Đang sửa xe #{v.vehicleId}", EditorStyles.boldLabel);

        v.length = EditorGUILayout.IntSlider("Độ dài xe", v.length, 1, 3);
        v.direction = (MoveDirection)EditorGUILayout.EnumPopup("Hướng", v.direction);

        if (autoCapacityFromLength != null)
        {
            v.capacity = autoCapacityFromLength(v.length);
            EditorGUILayout.LabelField($"Sức chứa: {v.capacity}");
        }
        else
        {
            v.capacity = EditorGUILayout.IntSlider("Sức chứa", v.capacity, 5, 20);
        }

        Vector2Int pos2D = new Vector2Int(v.gridPostion.x, v.gridPostion.y);
        pos2D = EditorGUILayout.Vector2IntField("Vị trí đầu xe", pos2D);
        v.gridPostion = new Vector3Int(pos2D.x, pos2D.y, 0);

        EditorGUILayout.Space(6);

        bool wantDelete = false, wantRecheck = false, wantSyncPeople = false;

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
            RemovePersonGroupsAtSlot(data, v.vehicleId);
            data.vehicles.Remove(v);
            selectedVehicleId = -1;
            EditorUtility.SetDirty(data);
            OnChanged?.Invoke();
        }
        else if (wantRecheck)
        {
            OnChanged?.Invoke();
        }
        else if (wantSyncPeople)
        {
            RegeneratePersonGroupForVehicle(data, v);
            EditorUtility.SetDirty(data);
            OnChanged?.Invoke();
        }
    }

    private bool IsVehicleAtCell(VehicleData v, Vector3Int cell)
    {
        Vector3Int backDelta = -GameMechanics.ToDelta(v.direction);
        for (int i = 0; i < v.length; i++)
            if (v.gridPostion + backDelta * i == cell) return true;
        return false;
    }

    private void RemovePersonGroupsAtSlot(LevelData data, int vehicleId)
    {
        data.personGroups.RemoveAll(p => p.ownerVehicleId == vehicleId);
    }

    // Xóa hết person group cũ của xe và sinh lại theo capacity hiện tại,
    // chia thành nhiều group nhỏ (4-6 người/group) cho tự nhiên.
    private void RegeneratePersonGroupForVehicle(LevelData data, VehicleData v)
    {
        Vector3Int slot = GameMechanics.GetWaitSlot(v.gridPostion, v.direction, data.width);
        RemovePersonGroupsAtSlot(data, v.vehicleId);

        if (autoCapacityFromLength != null)
            v.capacity = autoCapacityFromLength(v.length);

        int nextPersonId = data.personGroups.Count > 0 ? data.personGroups.Max(p => p.groupPersonId) + 1 : 0;
        int remaining = v.capacity;

        while (remaining > 0)
        {
            int groupSize = Mathf.Min(Random.Range(4, 7), remaining);
            data.personGroups.Add(new PersonGroupData
            {
                groupPersonId = nextPersonId++,
                Count = groupSize,
                slotPosition = slot,
                ownerVehicleId = v.vehicleId
            });
            remaining -= groupSize;
        }
    }

    // Dùng cho nút "Rebuild toàn bộ" — xóa sạch person group và sinh lại cho mọi xe.
    public void RebuildAllPersonGroups(LevelData data)
    {
        data.personGroups.Clear();

        foreach (var v in data.vehicles)
            RegeneratePersonGroupForVehicle(data, v);

        EditorUtility.SetDirty(data);

        int totalSeats = data.vehicles.Sum(v => v.capacity);
        int totalPeople = data.personGroups.Sum(p => p.Count);
        Debug.Log(
            $"✅ Rebuild thành công!\nXe: {data.vehicles.Count}\nTổng chỗ: {totalSeats}\n" +
            $"Tổng người: {totalPeople}\nSố nhóm người: {data.personGroups.Count}");
    }
}