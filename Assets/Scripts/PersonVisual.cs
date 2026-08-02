using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PersonVisual : MonoBehaviour
{
    [SerializeField] private MeshRenderer m_Renderer;
    // Start is called before the first frame update
    private Color colorPerson;
    public Color ColorPerson => colorPerson;

    public void Setup(Color color)
    { 
        colorPerson = color; // gắn mau cho tưng người

        MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
        propertyBlock.SetColor("_Color",color);
        m_Renderer.SetPropertyBlock(propertyBlock); // set màu cho material

    }
    private void Awake()
    {
        m_Renderer = GetComponent<MeshRenderer>(); 
    }

   
  
}
