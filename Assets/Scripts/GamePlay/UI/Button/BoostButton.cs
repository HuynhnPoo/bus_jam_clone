using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoostButton : ButtonBase
{
    [SerializeField] private PowerUpBase powerUp;

    private void OnEnable()
    {
        powerUp = GetComponent<PowerUpBase>();
    }
    protected override void OnClick() => powerUp.Use();
}
