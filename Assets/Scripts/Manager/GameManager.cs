using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : SingletonBase<GameManager>
{
    public GridManager gridManager { get; private set; }

    private static List<GameObject> linePperson =new List<GameObject>();
    public List<GameObject> LinePperson { get => linePperson; set => linePperson = value; }

    private void OnEnable()
    {
        gridManager = FindFirstObjectByType<GridManager>();
    }

    public void UpdateLinePerson()
    {
        for (int i = 0; i < linePperson.Count; i++)
        {
            Vector3 newPos = GameMechanics.CalculatePathPosition(i,gridManager.posFirtsPerson);
            linePperson[i].transform.DOMove(newPos,0.5f).SetEase(Ease.OutQuad);
        }
    }
}
