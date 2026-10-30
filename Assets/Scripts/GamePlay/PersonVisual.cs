using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*
public enum ColorType 
{
    None,
    Red,
    Blue,
    Green,
    Yellow,
    Purple,
    Orange
}

public struct ColorUtilityBusJam
{
    public static Color GetColor(ColorType color)
    {
        return color switch
        {
            ColorType.Red => Color.red,
            ColorType.Blue => Color.blue,
            ColorType.Green => Color.green,
            ColorType.Yellow => Color.yellow,
            ColorType.Purple => new Color(0.6f, 0.2f, 0.8f),
            ColorType.Orange => new Color(1.0f, 0.5f, 0.0f),
            _ => Color.white
        };
    }
}*/

public class PersonVisual : MonoBehaviour
{
    [SerializeField] private Renderer m_Renderer;

    public int GroupPesonId { get; private set; }
    private MaterialPropertyBlock propertyBlock;
    private Color colorPerson;
    public Color ColorPerson => colorPerson;

    private void Awake()
    {
        m_Renderer = GetComponent<Renderer>(); // khai báo rendeer tự nhân mesh ay skin renderer
        propertyBlock = new MaterialPropertyBlock();
    }

    public void Setup(Color color, int GroupPesonId)
    {
        colorPerson = color; // gắn mau cho tưng người
       // Debug.Log("");

        this.GroupPesonId = GroupPesonId;

      //  Debug.Log("color "+color +"......."+this.GroupPesonId);
        m_Renderer.GetPropertyBlock(propertyBlock);
        propertyBlock.SetColor(Constans.colorURPRenderer, color);
        m_Renderer.SetPropertyBlock(propertyBlock); // set màu cho material
    }
}
