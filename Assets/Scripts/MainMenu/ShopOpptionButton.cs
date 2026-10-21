using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class ShopOpptionButton : ButtonBase
{
    [SerializeField] private ShopManager shopManager;
    public bool isItemOpption;
   
    protected override void OnClick()
    {
        shopManager.ChangePanel(isItemOpption);

    }
}
