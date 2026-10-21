using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BoostType
{
    None,
    Hint,
    ColorChange,
    Undo
}

public abstract class PowerUpBase : MonoBehaviour
{
    protected int quantity;
    protected int max;
    public BoostType BoostType;


    public  int Quantity => quantity;

    protected virtual void Awake()
    {
        quantity = PlayerPrefs.GetInt(BoostType.ToString(),0);
    }
   
    public virtual void AddQuantity(int amount=1) 
    {
        quantity += amount;// thưc hiện tang so luong

        PlayerPrefs.SetInt(BoostType.ToString(), quantity);
        PlayerPrefs.Save();
    }

    protected bool CanUse() => quantity > 0; // nếu mà có boost mới có thể sử dụng

    public virtual void Use()
    {
        Debug.Log("thuc hien su dụng");
        if (CanUse())
        {
            Debug.Log("số lượng là "+ quantity +"bạn có thể sử dụng được poweerup" + CanUse());
            quantity--; // giảm số lượng

            PlayerPrefs.SetInt(BoostType.ToString(), quantity);
            PlayerPrefs.Save();
           
            ExecutePowerUp(); // sử dụng powerup nếu điều khiện đúng
        }
    }

    protected abstract void ExecutePowerUp(); // ham thuc hien


}
