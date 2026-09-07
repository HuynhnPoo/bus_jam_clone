using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(LevelData))]
public class LevelDataEditor : Editor
{
    private static int CalculateCapacity(int length) => (length * 2) + 2;
    private VehicleGridPanel panel;
    private bool isSolvable;
    private bool checkedOnce = false;

    private void OnEnable()
    {
        panel = new VehicleGridPanel(CalculateCapacity);
        panel.OnChanged += HandleChanged;
    }

    private void HandleChanged()
    {
        isSolvable = LevelSolver.Verify((LevelData)target);
        checkedOnce = true;
        Repaint();
    }

    public override void OnInspectorGUI()
    {
        LevelData data = (LevelData)target;

        EditorGUILayout.LabelField("Level Info", EditorStyles.boldLabel);

        // --- Input field cho Level Index ---
        EditorGUI.BeginChangeCheck();
        int newLevelIndex = EditorGUILayout.DelayedIntField("Level Index", data.levelIndex);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(data, "Change Level Index");
            data.levelIndex = newLevelIndex;
            EditorUtility.SetDirty(data);
        }

        // --- Input field cho Width / Height ---
        EditorGUI.BeginChangeCheck();
        int newWidth = EditorGUILayout.DelayedIntField("Width", data.width);
        int newHeight = EditorGUILayout.DelayedIntField("Height", data.height);
        if (EditorGUI.EndChangeCheck())
        {
            newWidth = Mathf.Max(1, newWidth);
            newHeight = Mathf.Max(1, newHeight);

            if (newWidth != data.width || newHeight != data.height)
            {
                Undo.RecordObject(data, "Resize Level Grid");
                ResizeGrid(data, data.width, data.height, newWidth, newHeight);
                data.width = newWidth;
                data.height = newHeight;
                EditorUtility.SetDirty(data);
            }
        }

        EditorGUILayout.LabelField($"Số xe: {data.vehicles.Count}");
        EditorGUILayout.LabelField($"Số nhóm người: {data.personGroups.Count}");
        EditorGUILayout.Space(8);

        if (GUILayout.Button("✅ Kiểm tra Solvable"))
            HandleChanged();

        if (checkedOnce)
        {
            EditorGUILayout.HelpBox(
                isSolvable ? "✅ Level giải được" : "❌ Level KHÔNG giải được",
                isSolvable ? MessageType.Info : MessageType.Error
            );
        }

        EditorGUILayout.Space(10);
        panel.DrawGridPreview(data, visualMultiplier: 10f);
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
            panel.RebuildAllPersonGroups(data);
            HandleChanged();
        }
    }

    // Resize grid: giữ nguyên dữ liệu ô cũ, phần mở rộng để trống
    // Bạn cần chỉnh theo đúng cấu trúc thật lưu ô lưới trong LevelData
    private void ResizeGrid(LevelData data, int oldW, int oldH, int newW, int newH)
    {
        /*
        var oldCells = data.cells; // ví dụ CellType[] theo row-major
        var newCells = new CellType[newW * newH];

        for (int y = 0; y < Mathf.Min(oldH, newH); y++)
        {
            for (int x = 0; x < Mathf.Min(oldW, newW); x++)
            {
                int oldIndex = y * oldW + x;
                int newIndex = y * newW + x;
                if (oldCells != null && oldIndex < oldCells.Length)
                    newCells[newIndex] = oldCells[oldIndex];
            }
        }
        data.cells = newCells;
        */
    }
}