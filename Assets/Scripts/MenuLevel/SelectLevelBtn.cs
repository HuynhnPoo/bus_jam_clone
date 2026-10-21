using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectLevelBtn : ButtonBase
{
    public int index;
    protected override void OnClick()
    {
        GameManager.Instance.LevelIndex=index;
        UIManager.Instance.ChangeScene(UIManager.SceneType.GamePlay);
    }

    
}
