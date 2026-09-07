using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RefManager : MonoBehaviour
{
    [SerializeField] private GameObject pausePanelGO;
    [SerializeField] private GameObject statusPanelGO;
    [SerializeField] private GameObject topPanelGO;

    private void Awake()
    {
        UIManager.Instance.Setup(pausePanelGO,statusPanelGO);
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
