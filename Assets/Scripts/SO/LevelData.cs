using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="newLevel", menuName ="Level data")]
public class LevelData : ScriptableObject
{
    [Header(" grid config")]
    public int width = 4;
    public int height = 4;

    public float cellSize = 4;
    public float spacing = 1;

    [Header("vehicle data")]
    public List<VehicleData> vehicles = new List<VehicleData>();

    [Header("person data")]
    public List<PersonGroupData> personGroups= new List<PersonGroupData>();

    public int levelIndex;
    public int time;
    public int difficultyScore;
}
