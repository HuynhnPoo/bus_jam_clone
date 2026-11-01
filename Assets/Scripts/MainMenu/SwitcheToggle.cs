using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class SwitcheToggle : MonoBehaviour
{
    private Toggle toggle;

    [SerializeField] private RectTransform posCheckMark;
    [SerializeField] private Image backgoundImg;
    //[SerializeField] private Toggle toggle;
    [SerializeField] private Sprite offSprite; // Hình nền khi tắt
    [SerializeField] private Sprite onSprite;  // Hình nền khi bật

    [SerializeField] private float offsetPositionX = 25f; // Tọa độ X khi tắt (bên trái)
   // [SerializeField] private float onPositionX = 25f;


    // Start is called before the first frame update
    void Start()
    {
        toggle = GetComponent<Toggle>();
        toggle.onValueChanged.AddListener(OnToggleChanged);

    }

    public void OnToggleChanged(bool isOn)
    {
        float targetX = isOn ? offsetPositionX : -offsetPositionX;
        Vector2 newPos = posCheckMark.anchoredPosition;
        newPos.x = targetX;
        posCheckMark.anchoredPosition = newPos;
    }
    // Update is called once per frame
    void Update()
    {

    }
}
