using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

//[System.Serializable]
//struct ItemData
//{
//    public Sprite sprite;
//    public BoostType boardType;
//    public int quantity;
//    public int price;

//}

public class ItemDisplayInfo : MonoBehaviour
{
    public string path;

    [SerializeField] private PowerupItemSO itemSO;
    [SerializeField] private TextMeshProUGUI buyText;
    [SerializeField] private TextMeshProUGUI quantityText;
    [SerializeField] private Image image;

    private void Awake()
    {
        //foreach (var item in transform) 
        //{
        //    Debug.Log(item);
        //    ite
        //}
        itemSO = Resources.Load<PowerupItemSO>("SO/" + path);

        buyText = this.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>();
        quantityText = this.transform.GetChild(2).GetComponent<TextMeshProUGUI>();
        image = this.transform.GetChild(1).GetComponent<Image>();
    }

    private void OnEnable()
    {
        buyText.SetText(itemSO.price.ToString());
        quantityText.SetText(PlayerPrefs.GetInt(itemSO.BoostType.ToString(), 0).ToString());
        image.sprite = itemSO.imgPowerup;
    }
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
