using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : SingletonBase<UIManager>
{
    [SerializeField] private GameObject pausePanelGO;
    [SerializeField] private GameObject statusPanelGO;
    [SerializeField] public GameObject PausePanelGO => pausePanelGO;
    [SerializeField] public GameObject StatusPanelGO => statusPanelGO;

    public enum SceneType
    {
        MainMenu,
        GamePlay
    }
    private void OnEnable()
    {
        EventManager.Subscribe(EventGameType.Pause, Pause); // dang ki event pause
        EventManager.Subscribe(EventGameType.Resume, Resume); // dang ki event reumse
    }
    private void OnDisable()
    {
        EventManager.UnSubscribe(EventGameType.Pause, Pause); // huy dang ki event pause
        EventManager.UnSubscribe(EventGameType.Resume, Resume); //  huy dang ki event reumse
    }
    public void Setup(GameObject pausePanelGO, GameObject statusPanelGO)
    {
        this.pausePanelGO = pausePanelGO;
        this.statusPanelGO = statusPanelGO;
    }

    public void ChangeScene(SceneType sceneType)
    {
        SceneManager.LoadScene(sceneType.ToString());
    }

    public void Pause()
    {     
        PausePanelGO.SetActive(true);
    }

    public void Resume()
    {
        PausePanelGO.SetActive(false);
    }
}
