using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "database", menuName = "level/database")]
public class LevelDatabase : ScriptableObject
{
    [SerializeField] private LevelData[] levels;

    public LevelData GetLevel(int index)
    {
        if (levels == null || levels.Length == 0) return null;

        int arrayIndex = index - 1;

        if (arrayIndex >= 0 && arrayIndex <= levels.Length)
        {
            return levels[arrayIndex];
        }
        return levels[0];
    }

    public int TotalLevels
    {
        get
        {
            if (levels == null) return 0;
            return levels.Length;
        }
    }

}
