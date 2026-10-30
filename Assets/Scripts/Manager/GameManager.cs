using DG.Tweening;
using DG.Tweening.Core.Easing;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : SingletonBase<GameManager>
{
    public GridManager gridManager { get; private set; }
    public static bool CanPause { get; set; } = false;
    public static bool NewGame { get; set; } = true;

    public static bool isGameOver { get; set; } = false;
    public static bool isWon { get; set; } = false;

    // public static bool NewGame { get; set; } = true;
    public int LevelIndex { set; get; } = 0;

    public int ReachedCoin { set; get; } = 0;
    public int curentCoin { set; get; } = 0;
    public int totalCoin { set; get; } = 0;
    public int curentLevel { set; get; } = 0;

    //  public int totalLevel { set; get; } =

    public string GameResutsStr = "";



    [Header("system")]
    public VehicleController CurrentLoadingVehicle;
    private static List<GameObject> linePperson = new List<GameObject>();

    [Header("Setting Shake Camera")]
    [Range(10, 20)]
    [SerializeField] private int vibrato = 15; // tàn suất
    private int randomness = 90; //độ

    private Vector3 originalCameraPos;

    public List<GameObject> LinePperson { get => linePperson; set => linePperson = value; }
    // public int OnLoadScene { get; private set; }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoad;

        totalCoin = PlayerPrefs.GetInt(Constans.coinSaveKey,0);
        Debug.Log("toatl coin dduwojc luuw laf "+ totalCoin);

        EventManager.Subscribe(EventGameType.GameOver, HanderResultStr);
        EventManager.Subscribe(EventGameType.GameWon, HanderResultStr);

    }

    private void OnSceneLoad(Scene scene, LoadSceneMode sceneMode)
    {

        if (scene.name == UIManager.SceneType.GamePlay.ToString())
        {
            Time.timeScale = 1;

            CanPause = false;
            gridManager = FindFirstObjectByType<GridManager>();
           
        }

        originalCameraPos = Camera.main.transform.localPosition;

        //if () { }
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoad;

        EventManager.UnSubscribe(EventGameType.GameOver, HanderResultStr);
        EventManager.UnSubscribe(EventGameType.GameWon, HanderResultStr);
    }


    public void UpdateLinePerson()
    {
        for (int i = 0; i < linePperson.Count; i++)
        {
            Transform perTransform = linePperson[i].transform;
            perTransform.DOKill();
            Vector3 newPos = GameMechanics.CalculatePathPosition(i, gridManager.posFirtsPerson);
            linePperson[i].transform.DOMove(newPos, 0.5f).SetEase(Ease.OutQuad);
        }
    }

    public void NotifyVehicles()
    {
        foreach (GameObject slot in gridManager.slotOccupants) // kiem tra cac xe đã đến 
        {
            if (slot == null) continue;

            VehicleController controller = slot.GetComponent<VehicleController>();

            if (controller == null) continue;
            if (!controller.IsArrived) continue;
            // controller.CheckColorPerson();
            if (controller.CheckColorPerson()) break;
        }
    }

    public void ClearSlot(GameObject vehicle) // clear slot cho ô đỗ trông khi xe ra
    {
        for (int i = 0; i < GameManager.Instance.gridManager.slotOccupants.Length; i++)
        {
            if (GameManager.Instance.gridManager.slotOccupants[i] == vehicle)
            {
                GameManager.Instance.gridManager.slotOccupants[i] = null;
                Debug.Log("hien thi ra slot " + i);
                break;
            }
        }
    }

    public void CameraShaking(float duration, float strenght)
    {
        Camera.main.transform.DOComplete();
        Camera.main.transform.DOShakePosition(duration, strenght, vibrato, randomness)
            .OnComplete(() => Camera.main.transform.localPosition = originalCameraPos);

#if UNITY_ANDROID || UNITY_IOS
         Handheld.Vibrate();
#endif
    }
    public void TogglePause()
    {
        CanPause = !CanPause;
        Debug.Log("canpuae là" + CanPause);

        Time.timeScale = CanPause ? 0 : 1;
        if (CanPause) EventManager.Notify(EventGameType.Pause);
        else EventManager.Notify(EventGameType.Resume);
    }
    void HanderResultStr()
    {
        if (isWon && !isGameOver)
        {
            GameResutsStr = Constans.gameWonKey;
            ReachedCoin = 50;
        }
        else
        {
            GameResutsStr = Constans.gameOverKey;
            ReachedCoin = 20;
        }
    }
}
