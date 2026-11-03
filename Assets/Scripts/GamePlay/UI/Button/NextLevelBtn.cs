using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NextLevelBtn : ButtonBase
{
    protected override void OnClick()
    {

        GameManager.Instance.gridManager.levelManager.NextLevel();
    }

   
}
