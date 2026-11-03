using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "PowerupData", menuName = "Powerup")]
public class PowerupItemSO : ScriptableObject
{
    public string namePowerup;
    public Sprite imgPowerup;
    public BoostType BoostType;
    public int price;
    public int amount = 1;
}
