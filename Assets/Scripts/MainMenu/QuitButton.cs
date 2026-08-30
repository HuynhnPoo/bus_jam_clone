using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class QuitButton : ButtonBase
{
    [SerializeField] private bool isQuitApp;

    protected override void OnClick()
    {
        if (isQuitApp)
        {
            QuitApp();
        }
        else
        {
            Time.timeScale = 1;
            UIManager.Instance.ChangeScene(UIManager.SceneType.MainMenu);
        }

    }
   
    void QuitApp()
    {
#if UNITY_EDITOR
        EditorApplication.ExitPlaymode();
#else

            Application.Quit();
#endif
    }
}
