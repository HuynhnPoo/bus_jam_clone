using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class QuantityPowerUpTxt : TextBase
{
    [SerializeField] PowerUpBase powerUp;
    protected override void Awake()
    {
        base.Awake();
        powerUp = GetComponentInParent<PowerUpBase>();
    }

    protected override void PrintText(StringBuilder sbText)
    {
        sbText.Clear().Append(powerUp.Quantity).ToString();
    }


}
