using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopManager : MonoBehaviour
{

    [SerializeField] public GameObject MyPanel;
    [SerializeField] public GameObject OtherPanel;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void ChangePanel(bool isItemPanel)
    {
        MyPanel.SetActive(isItemPanel);
        OtherPanel.SetActive(!isItemPanel);
    }

   public void Buy(PowerupItemSO item)
    {
        int currentQuantity = PlayerPrefs.GetInt(item.BoostType.ToString(), 0);
        PlayerPrefs.SetInt(item.BoostType.ToString(), currentQuantity+item.amount);
        PlayerPrefs.Save();

        Debug.Log("mua vat pham thanh cong");
    }

    void Watch()
    {

    }
}
