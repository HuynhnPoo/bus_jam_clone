using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class LevelTxt : TextBase
{
    protected override void PrintText(StringBuilder sbText)
    {
        sbText.Clear().Append(GameManager.Instance.curentLevel);
    }
}
