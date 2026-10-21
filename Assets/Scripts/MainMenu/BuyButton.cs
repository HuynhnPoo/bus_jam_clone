using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuyButton : ButtonBase
{
    //private PowerUpBase powerUp;
    [SerializeField] private PowerupItemSO powerupItemSO;

    [SerializeField] private ShopManager shopManager;

    protected override void OnClick()
    {
        // powerUp.AddQuantity();
        shopManager.Buy(powerupItemSO);
    }


}
