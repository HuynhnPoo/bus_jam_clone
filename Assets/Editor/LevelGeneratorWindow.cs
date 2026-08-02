using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

public class LevelGeneratorWindow : EditorWindow
{
    int gridWidth = 6;
    int gridHeight = 6;
    int vehicleCount = 8;
    int minCapacity = 10;
    int maxCapacity = 15;
    int levelIndex = 1;

    string savePath = "Assets/SO/Levels";

    LevelData previewData;
    bool isSolvable;

    int selectedVehicleId = -1;
    Vector3Int? pendingNewVehicleCell = null;

    Color newVehicleColor = Color.red;
    int newVehicleCapacity = 12;
    int newVehicleLength = 2;
    MoveDirection newVehicleDir = MoveDirection.Right;

    [MenuItem("Tools/CarJam/Level Generator")]
    public static void ShowWindow()
    {
        GetWindow<LevelGeneratorWindow>("Car Jam Level Generator");
    }

    void OnGUI()
    {
        GUILayout.Label("Cấu hình Level", EditorStyles.boldLabel);

        gridWidth = EditorGUILayout.IntField("Grid Width", gridWidth);
        gridHeight = EditorGUILayout.IntField("Grid Height", gridHeight);
        vehicleCount = EditorGUILayout.IntSlider("Vehicle Count", vehicleCount, 2, 30);
        minCapacity = EditorGUILayout.IntSlider("Min Capacity", minCapacity, 5, 20);
        maxCapacity = EditorGUILayout.IntSlider("Max Capacity", maxCapacity, minCapacity, 25);

        EditorGUILayout.Space(10);
        GUILayout.Label("Lưu file", EditorStyles.boldLabel);
        levelIndex = EditorGUILayout.IntField("Level Index", levelIndex);
        savePath = EditorGUILayout.TextField("Save Folder", savePath);

        EditorGUILayout.Space(15);

        if (GUILayout.Button("🎲 Generate Preview", GUILayout.Height(30)))
            GeneratePreview();

        EditorGUILayout.Space(5);

        if (previewData != null)
        {
            EditorGUILayout.HelpBox(
                isSolvable ? "✅ Level giải được (Solvable)" : "❌ Level KHÔNG giải được — hãy generate lại hoặc sửa tay",
                isSolvable ? MessageType.Info : MessageType.Error
            );

            EditorGUILayout.LabelField($"Số xe: {previewData.vehicles.Count}");
            EditorGUILayout.LabelField($"Số nhóm người: {previewData.personGroups.Count}");

            DrawGridPreview(previewData);

            EditorGUILayout.Space(10);

            GUI.enabled = isSolvable;
            if (GUILayout.Button("💾 Save Level Asset", GUILayout.Height(30)))
                SavePreview();
            GUI.enabled = true;

            if (GUILayout.Button("🔁 Generate Batch (10 levels)", GUILayout.Height(25)))
                GenerateBatch(10);
        }
    }

    void GeneratePreview()
    {
        var generator = new LevelGenerator();
        generator.Setup(gridWidth, gridHeight, vehicleCount, minCapacity, maxCapacity);

        int attempts = 0;
        LevelData data;
        bool solvable;

        do
        {
            data = generator.GenerateLevelData(levelIndex);
            solvable = LevelSolver.Verify(data);
            attempts++;
        }
        while (!solvable && attempts < 20);

        previewData = data;
        isSolvable = solvable;
        selectedVehicleId = -1;
        pendingNewVehicleCell = null;

        if (!solvable)
            Debug.LogWarning($"Không tạo được level giải được sau {attempts} lần thử. Thử giảm vehicleCount hoặc tăng grid size.");
    }

    void SavePreview()
    {
        if (previewData == null) return;

        if (!AssetDatabase.IsValidFolder(savePath))
        {
            string parent = System.IO.Path.GetDirectoryName(savePath);
            string folderName = System.IO.Path.GetFileName(savePath);
            AssetDatabase.CreateFolder(parent, folderName);
        }

        string assetPath = AssetDatabase.GenerateUniqueAssetPath($"{savePath}/Level_{levelIndex:000}.asset");

        AssetDatabase.CreateAsset(previewData, assetPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"✅ Đã lưu level tại: {assetPath}");
        EditorGUIUtility.PingObject(previewData);

        levelIndex++;
        previewData = null;
    }

    void GenerateBatch(int count)
    {
        var generator = new LevelGenerator();
        generator.Setup(gridWidth, gridHeight, vehicleCount, minCapacity, maxCapacity);

        int savedCount = 0;

        for (int i = 0; i < count; i++)
        {
            int attempts = 0;
            LevelData data;
            bool solvable;

            do
            {
                data = generator.GenerateLevelData(levelIndex);
                solvable = LevelSolver.Verify(data);
                attempts++;
            }
            while (!solvable && attempts < 20);

            if (!solvable)
            {
                Debug.LogWarning($"Bỏ qua level {levelIndex}, không tìm được lời giải sau {attempts} lần thử.");
                continue;
            }

            if (!AssetDatabase.IsValidFolder(savePath))
            {
                string parent = System.IO.Path.GetDirectoryName(savePath);
                string folderName = System.IO.Path.GetFileName(savePath);
                AssetDatabase.CreateFolder(parent, folderName);
            }

            string assetPath = AssetDatabase.GenerateUniqueAssetPath($"{savePath}/Level_{levelIndex:000}.asset");
            AssetDatabase.CreateAsset(data, assetPath);
            savedCount++;
            levelIndex++;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"✅ Đã tạo hàng loạt {savedCount}/{count} level thành công.");
    }
    void DrawGridPreview(LevelData data)
    {
        EditorGUILayout.Space(10);
        GUILayout.Label("Preview Grid (click ô để chọn/thêm xe)", EditorStyles.boldLabel);

        data.cellSize = EditorGUILayout.Slider("Cell Size", data.cellSize, 0f, 2f);
        data.spacing = EditorGUILayout.Slider("Spacing", data.spacing, 0f, 5f);

        EditorUtility.SetDirty(data);

        float safeCellSize = data.cellSize > 0 ? data.cellSize : 4f;
        float safeSpacing = data.spacing >= 0 ? data.spacing : 1f;

        float editorCellSize = safeCellSize * 4;
        float editorSpacing = safeSpacing * 4;

        EditorGUILayout.Space(4);

        Rect gridRect = GUILayoutUtility.GetRect(
            data.width * (editorCellSize + editorSpacing) + 20,
            data.height * (editorCellSize + editorSpacing) + 20
        );
        Event e = Event.current;

        for (int x = 0; x < data.width; x++)
            for (int y = 0; y < data.height; y++)
            {
                Rect cellRect = new Rect(
                    gridRect.x + x * (editorCellSize + editorSpacing)+10,
                    gridRect.y + y * (editorCellSize + editorSpacing)+10,
                    editorCellSize, editorCellSize
                );

                var vehicleHere = data.vehicles.FirstOrDefault(v => IsVehicleAtCell(v, new Vector3Int(x, y, 0)));
                Color drawColor = vehicleHere != null ? vehicleHere.vehicleColor : new Color(0.2f, 0.2f, 0.2f);

                if (vehicleHere != null && vehicleHere.vehicleId == selectedVehicleId)
                    EditorGUI.DrawRect(new Rect(cellRect.x - 2, cellRect.y - 2, cellRect.width + 4, cellRect.height + 4), Color.white);

                EditorGUI.DrawRect(cellRect, drawColor);

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
                    Repaint();
                }
            }

        if (selectedVehicleId >= 0)
            DrawEditVehiclePanel(data);
        else if (pendingNewVehicleCell.HasValue)
            DrawAddVehiclePanel(data);
    }
    void DrawAddVehiclePanel(LevelData data)
    {
        var cell = pendingNewVehicleCell.Value;
        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField($"Thêm xe tại ô ({cell.x},{cell.y})", EditorStyles.boldLabel);

        newVehicleColor = EditorGUILayout.ColorField("Màu", newVehicleColor);
        newVehicleCapacity = EditorGUILayout.IntSlider("Sức chứa", newVehicleCapacity, 5, 20);
        newVehicleLength = EditorGUILayout.IntSlider("Độ dài xe", newVehicleLength, 1, 3);
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
                vehicleColor = newVehicleColor,
                capacity = newVehicleCapacity,
                length = newVehicleLength,
                gridPostion = cell,
                direction = newVehicleDir
            };

            data.vehicles.Add(newVehicle);
            RegeneratePersonGroupForVehicle(data, newVehicle);

            pendingNewVehicleCell = null;
            isSolvable = LevelSolver.Verify(data);
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

        v.vehicleColor = EditorGUILayout.ColorField("Màu", v.vehicleColor);
        v.capacity = EditorGUILayout.IntSlider("Sức chứa", v.capacity, 5, 20);
        v.length = EditorGUILayout.IntSlider("Độ dài xe", v.length, 1, 4);
        v.direction = (MoveDirection)EditorGUILayout.EnumPopup("Hướng", v.direction);

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
            RemovePersonGroupsAtSlot(data, v.vehicleId);

            data.vehicles.Remove(v);
            selectedVehicleId = -1;
            isSolvable = LevelSolver.Verify(data);
            EditorUtility.SetDirty(data);
            Repaint();
        }
        else if (wantRecheck)
        {
            isSolvable = LevelSolver.Verify(data);
        }
        else if (wantSyncPeople)
        {
            RegeneratePersonGroupForVehicle(data, v);
            isSolvable = LevelSolver.Verify(data);
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

    void RemovePersonGroupsAtSlot(LevelData data, int vehicleId)
    {
        data.personGroups.RemoveAll(p => p.ownerVehicleId == vehicleId);
    }

    void RegeneratePersonGroupForVehicle(LevelData data, VehicleData v)
    {
        Vector3Int slot = GetWaitSlot(v, data.width);
        RemovePersonGroupsAtSlot(data, v.vehicleId);

        int remaining = v.capacity;
        while (remaining > 0)
        {
            int groupSize = Mathf.Min(Random.Range(3, 6), remaining);
            data.personGroups.Add(new PersonGroupData
            {
                colorPerson = v.vehicleColor,
                Count = groupSize,
                slotPosition = slot,
                ownerVehicleId = v.vehicleId
            });
            remaining -= groupSize;
        }
    }
}