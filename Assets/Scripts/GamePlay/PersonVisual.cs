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
    [SerializeField] private MeshRenderer m_Renderer;
    
    public int GroupPesonId { get; private set; }

    private Color colorPerson;
    public Color ColorPerson => colorPerson;

    public void Setup(Color color, int GroupPesonId)
    { 
        colorPerson = color; // gắn mau cho tưng người

        this.GroupPesonId = GroupPesonId;

        MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
        propertyBlock.SetColor("_Color",color);
        m_Renderer.SetPropertyBlock(propertyBlock); // set màu cho material

    }
    private void Awake()
    {
        m_Renderer = GetComponent<MeshRenderer>(); 
    }
}
