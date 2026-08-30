using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResumeBtn : ButtonBase
{
    protected override void OnClick()
    {
        Time.timeScale = 1;
        GameManager.Instance.Pause(GameManager.Instance.CanPause);
       
    }
}
