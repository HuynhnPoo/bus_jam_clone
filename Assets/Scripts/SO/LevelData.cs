using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "newLevel", menuName = "level/LevelData")]
public class LevelData : ScriptableObject
{
    public enum DifficultyType
    {
        Easy,
        Nomarl,
        Hard
    }

    [Header("grid config")]
    public int width = 4;
    public int height = 4;

    public float cellSize = 4;
    public float spacing = 1;

    [Header("level config")]
    public int levelIndex=1;
    public int time=60;
    public DifficultyType difficulty= DifficultyType.Easy;

    [Header("vehicle data")]
    public List<VehicleData> vehicles = new List<VehicleData>();

    [Header("person data")]
    public List<PersonGroupData> personGroups= new List<PersonGroupData>();

}
