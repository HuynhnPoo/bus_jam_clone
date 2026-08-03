using DG.Tweening;
using DG.Tweening.Core.Easing;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : SingletonBase<GameManager>
{
    public GridManager gridManager { get; private set; }

    private static List<GameObject> linePperson = new List<GameObject>();
    public List<GameObject> LinePperson { get => linePperson; set => linePperson = value; }


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


    public void ClearSlot(GameObject vehicle)
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
}
