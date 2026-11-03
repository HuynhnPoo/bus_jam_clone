using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuLevelBtn : ButtonBase
{
    protected override void OnClick()
    {
        UIManager.Instance.ChangeScene(UIManager.SceneType.LevelMenu);
    }
}
