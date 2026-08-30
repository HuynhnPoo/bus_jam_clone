using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class ButtonBase : MonoBehaviour
{
    [SerializeField] protected Button button;
    private void Awake()
    {
        if (this.button != null) return;
        this.button = GetComponent<Button>();
    }
    // Start is called before the first frame update
   protected virtual void Start()
    {
       this.AddListener();
    }

    protected virtual void AddListener()
    {
        this.button.onClick.AddListener(this.OnClick); 
    }
    protected abstract void OnClick();
}
