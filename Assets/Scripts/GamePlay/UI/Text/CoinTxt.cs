using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class CoinTxt : TextBase
{
    public bool isTotalCoin;
    protected override void PrintText(StringBuilder sbText)
    {
        if (!isTotalCoin) 
        {
            sbText.Clear().Append(GameManager.Instance.curentCoin.ToString("D2"));
        }
    }
}
