using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BoostType
{
    None,
    Hint,
    ColorChangeBoost,
    Undo
}

public abstract class PowerUpBase : MonoBehaviour
{
    public BoostType BoostType;
    private int quantity;
    public int Quantily => quantity;

    protected bool CanUse() => quantity > 0; // nếu mà có boost mới có thể sử dụng

    public virtual void Use()
    {
        Debug.Log("thuc hien su dụng");
        if (CanUse())
        {
            quantity--; // giảm số lượng
            ExecutePowerUp(); // sử dụng powerup nếu điều khiện đúng
        }
    }

    protected abstract void ExecutePowerUp();

    public virtual void AddQuantity()
    {
        quantity++;
    }
}
