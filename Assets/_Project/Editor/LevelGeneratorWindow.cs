using UnityEngine;
using UnityEditor;
using static LevelData;

public class LevelGeneratorWindow : EditorWindow
{
    int gridWidth = 6;
    int gridHeight = 6;
    int vehicleCount = 8;
    int minCapacity = 10;
    int maxCapacity = 15;

    public int levelIndex = 1;
    public int time = 60;
    public DifficultyType difficulty = DifficultyType.Easy;

    string savePath = "Assets/_Project/SO/Levels";

    LevelData previewData;
    bool isSolvable;

    // Window: capacity xe được chọn tay (không auto theo length) -> truyền null
    VehicleGridPanel panel;

    [MenuItem("Tools/CarJam/Level Generator")]
    public static void ShowWindow()
    {
        GetWindow<LevelGeneratorWindow>("Car Jam Level Generator");
    }

    private void OnEnable()
    {
        panel = new VehicleGridPanel(autoCapacityFromLength: null);
        panel.OnChanged += HandleChanged;
    }

    private void HandleChanged()
    {
        if (previewData == null) return;
        isSolvable = LevelSolver.Verify(previewData);
        Repaint();
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
        GUILayout.Label("Thông tin file", EditorStyles.boldLabel);
        levelIndex = EditorGUILayout.IntField("Level Index", levelIndex);
        time = EditorGUILayout.IntField("Time (Giây)", time);
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

            EditorGUILayout.Space(10);
            panel.DrawGridPreview(previewData, visualMultiplier: 10f);
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
        panel.selectedVehicleId = -1;
        panel.pendingNewVehicleCell = null;

        if (!solvable)
            Debug.LogWarning($"Không tạo được level giải được sau {attempts} lần thử. Thử giảm vehicleCount hoặc tăng grid size.");
    }

    void SavePreview()
    {
        if (previewData == null) return;

        EnsureFolderExists(savePath);
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

            EnsureFolderExists(savePath);
            string assetPath = AssetDatabase.GenerateUniqueAssetPath($"{savePath}/Level_{levelIndex:000}.asset");
            AssetDatabase.CreateAsset(data, assetPath);
            savedCount++;
            levelIndex++;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"✅ Đã tạo hàng loạt {savedCount}/{count} level thành công.");
    }

    private static void EnsureFolderExists(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;
        string parent = System.IO.Path.GetDirectoryName(path);
        string folderName = System.IO.Path.GetFileName(path);
        AssetDatabase.CreateFolder(parent, folderName);
    }
}