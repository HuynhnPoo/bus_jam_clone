using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : SingletonBase<UIManager>
{
    [SerializeField] public GameObject PausePanelGO { get; private set; }
    [SerializeField] public GameObject StatusPanelGO { get; private set; }

    public enum SceneType 
    {
        MainMenu,
        GamePlay
    }
    public void Setup(GameObject pausePanelGO, GameObject statusPanelGO)
    {
        this.PausePanelGO = pausePanelGO;
        this.StatusPanelGO = statusPanelGO;
    }


    public void ChangeScene(SceneType sceneType)
    {
        SceneManager.LoadScene(sceneType.ToString());
    }
  
}
