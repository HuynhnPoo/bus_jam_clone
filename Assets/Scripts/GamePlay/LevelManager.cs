using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [SerializeField] private LevelDatabase levelDatas;

    void LoadLevel(int index)
    {
        if (levelDatas != null && levelDatas.TotalLevels > 0)
        {

            LevelData currentLevel = levelDatas.GetLevel(index); // load level
            
            if (currentLevel == null) return;

            //  Debug.Log(GameManager.Instance.gridManager == null ? "có" : "không");
          
            GameManager.Instance.gridManager.Spawn(currentLevel);
            Debug.Log(currentLevel.name);
        }
    }
    
    // Start is called before the first frame update
    void Start()
    {
       // Debug.Log("hien thi"+GameManager.Instance.LevelIndex);
        LoadLevel(1);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
