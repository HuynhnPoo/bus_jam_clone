using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

public abstract class TextBase : MonoBehaviour
{
    [SerializeField] protected TextMeshProUGUI text;

    protected StringBuilder _sbText =new StringBuilder(10);

    private void Awake()
    {
        if (this.text != null) return;
        this.text = GetComponent<TextMeshProUGUI>();
    }
   

    // Update is called once per frame
    protected virtual void Update()
    {
        this.PrintText(_sbText);
        text.SetText(_sbText);
    }

    protected abstract void PrintText(StringBuilder sbText);
}
