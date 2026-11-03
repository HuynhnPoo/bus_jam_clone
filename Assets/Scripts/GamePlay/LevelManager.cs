using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelManager : MonoBehaviour
{
    [SerializeField] private LevelDatabase levelDatas;

    void LoadLevel(int index)
    {
        if (levelDatas != null && levelDatas.TotalLevels >= 0)
        {

            LevelData currentLevel = levelDatas.GetLevel(index); // load level

            if (currentLevel == null) return;

            //  Debug.Log(GameManager.Instance.gridManager == null ? "có" : "không");

            GameManager.Instance.gridManager.Spawn(currentLevel);
            Debug.Log(currentLevel.name);
        }
    }

    public void NextLevel()
    {
        GameManager.Instance.curentLevel++;  // nên chuyển vào khi won game
        LoadLevel(GameManager.Instance.curentLevel);
    }

    // Start is called before the first frame update
    void Start()
    {
        // Debug.Log("hien thi"+GameManager.Instance.LevelIndex);
        LoadLevel(1);
    }
}
