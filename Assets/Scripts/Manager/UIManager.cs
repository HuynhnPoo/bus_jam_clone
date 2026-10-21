using DG.Tweening.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : SingletonBase<UIManager>
{
    [SerializeField] private GameObject pausePanelGO;
    [SerializeField] private GameObject statusPanelGO;

    [SerializeField] private GameObject thirdUI_GO;
    public GameObject PausePanelGO => pausePanelGO;
    public GameObject StatusPanelGO => statusPanelGO;

    [SerializeField] private GameObject playGame;

    public enum SceneType
    {
        MainMenu,
        GamePlay
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoad;

        EventManager.Subscribe(EventGameType.Pause, Pause); // dang ki event pause
        EventManager.Subscribe(EventGameType.Resume, Resume); // dang ki event reumse
     
        EventManager.Subscribe(EventGameType.GameOver, ShowResultGame); // dang ki event pause
        EventManager.Subscribe(EventGameType.GameWon,ShowResultGame); // dang ki event reumse

        EventManager.Subscribe(EventGameType.GameWon,ActiveNextLevel); // dang ki event reumse
    }

    private void OnSceneLoad(Scene scene, LoadSceneMode mode)
    {
        //    Debug.Log("load sscen thanh cong");
    }
    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoad;
        EventManager.UnSubscribe(EventGameType.Pause, Pause); // huy dang ki event pause
        EventManager.UnSubscribe(EventGameType.Resume, Resume); //  huy dang ki event reumse

        EventManager.UnSubscribe(EventGameType.GameOver, ShowResultGame); // dang ki event pause
        EventManager.UnSubscribe(EventGameType.GameWon, ShowResultGame); // dang ki event reumse
     
        EventManager.UnSubscribe(EventGameType.GameWon, ActiveNextLevel); // dang ki event reumse
    }


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            SceneManager.LoadScene(SceneType.MainMenu.ToString());
        }
    }
    public void Setup(GameObject pausePanelGO = null, GameObject statusPanelGO = null,GameObject thirdUI_GO=null)
    {
        this.pausePanelGO = pausePanelGO;
        this.statusPanelGO = statusPanelGO;
        this.thirdUI_GO = thirdUI_GO;

    }

    public void ChangeScene(SceneType sceneType)
    {
        SceneManager.LoadScene(sceneType.ToString());
    }

    public void Pause() => PausePanelGO.SetActive(true);


    public void Resume() => PausePanelGO.SetActive(false);

    public void ShowResultGame()
    { statusPanelGO.SetActive(true); }

    public void ActiveNextLevel()
    { if (GameManager.isWon) this.thirdUI_GO.SetActive(true); }

}
