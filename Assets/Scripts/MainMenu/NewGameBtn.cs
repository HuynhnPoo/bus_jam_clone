using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewGameBtn : ButtonBase
{
    private void OnEnable()
    {
        GameManager.NewGame = PlayerPrefs.GetInt(Constans.newGameKey, 0) == 1;
        Debug.Log("hien thi ra game moiw" + GameManager.NewGame);
     //s   Debug.Log(UIManager.Instance.PausePanelGO.name + "la tên game đọc bật lên");
        if (GameManager.NewGame)
        {
            UIManager.Instance.PausePanelGO.SetActive(false);
        }
        else
        {
            UIManager.Instance.PausePanelGO.SetActive(true);
        }
    }
    protected override void OnClick()
    {
       
        if (GameManager.NewGame)
        {
            GameManager.NewGame = false;
            int value = GameManager.NewGame ? 1 : 0;
            PlayerPrefs.SetInt(Constans.newGameKey, value);

            //UIManager.Instance.PausePanelGO.SetActive(true);

            UIManager.Instance.ChangeScene(UIManager.SceneType.GamePlay);
        }
        else
        {
            GameManager.NewGame = true;
           // UIManager.Instance.PausePanelGO.SetActive(true);
            PlayerPrefs.SetInt(Constans.newGameKey, 1);
            UIManager.Instance.ChangeScene(UIManager.SceneType.GamePlay);
        }

    }


}
