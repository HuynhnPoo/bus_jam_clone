using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RestartBtn : ButtonBase
{
    protected override void OnClick()
    {
     
        UIManager.Instance.ChangeScene(UIManager.SceneType.GamePlay);
    }

  
}
