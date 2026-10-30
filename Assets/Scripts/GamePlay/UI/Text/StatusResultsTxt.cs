using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class StatusResultsTxt : TextBase
{
    public bool isTotalCoin;
    protected override void PrintText(StringBuilder sbText)
    {
        if (isTotalCoin)
        {

            sbText.Clear().Append(GameManager.Instance.totalCoin);
        }
        else
        {
            sbText.Clear().Append(GameManager.Instance.GameResutsStr);
        }
    }
}
