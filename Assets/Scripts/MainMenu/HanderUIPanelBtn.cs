using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HanderUIPanelBtn : ButtonBase
{
    public enum PanelType { Setting, Shop }
    public PanelType panel;
    private GameObject panelGO;

    protected override void OnClick()
    {
        switch (panel)
        {
            case PanelType.Shop:
                StatusButton(UIManager.Instance.Second_UIGO);
                break;
            case PanelType.Setting:
                StatusButton(UIManager.Instance.ThirdUI_GO);
                break;
        }
    }

    void StatusButton(GameObject GO)
    {
        panelGO = GO;
        if (panelGO.activeSelf)
        {
            panelGO.SetActive(false);
        }
        else
        {
            panelGO.SetActive(true);
        }
    }

}
