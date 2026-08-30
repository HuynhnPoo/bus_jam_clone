using DG.Tweening;
using DG.Tweening.Core.Easing;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : SingletonBase<GameManager>
{

    [Header("setting")]
    private static bool canPause=true;

    public RedoBoost RedoBoost;
    public GridManager gridManager { get; private set; }

    [Header("aa")]
    public VehicleController CurrentLoadingVehicle;

    private static List<GameObject> linePperson = new List<GameObject>();
    public List<GameObject> LinePperson { get => linePperson; set => linePperson = value; }

    public bool CanPause { get => canPause; set => canPause = value; }
    private void OnEnable()
    {
        gridManager = FindFirstObjectByType<GridManager>();
    }

    public void UpdateLinePerson()
    {
        for (int i = 0; i < linePperson.Count; i++)
        {
            Transform perTransform = linePperson[i].transform;
            perTransform.DOKill();
            Vector3 newPos = GameMechanics.CalculatePathPosition(i, gridManager.posFirtsPerson);
            linePperson[i].transform.DOMove(newPos, 0.5f).SetEase(Ease.OutQuad);
        }
    }

    public void NotifyVehicles()
    {
        foreach (GameObject slot in gridManager.slotOccupants) // kiem tra cac xe đã đến 
        {
            if (slot == null) continue; 

            VehicleController controller = slot.GetComponent<VehicleController>();

            if (controller == null) continue;
          // controller.CheckColorPerson();
            if (controller.CheckColorPerson()) break;
        }
    }

    public void ClearSlot(GameObject vehicle) // clear slot cho ô đỗ trông khi xe ra
    {
        for (int i =0; i<GameManager.Instance.gridManager.slotOccupants.Length; i++)
        {
            if (GameManager.Instance.gridManager.slotOccupants[i] == vehicle)
            {
                GameManager.Instance.gridManager.slotOccupants[i] = null;
                Debug.Log("hien thi ra slot " + i);
                break;
            }
        }
    }
    
   
    public void Pause(bool canPause)
    {
        if (canPause)
        {
            Time.timeScale = 0;
            UIManager.Instance.PausePanelGO.SetActive(true);
            canPause = false;
        }
        else
        {
            Time.timeScale = 1;
            UIManager.Instance.PausePanelGO.SetActive(false);
            canPause =true;
        }
    }
}
