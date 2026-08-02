using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class VehicleController : MonoBehaviour
{
    public VehicleState vehicleData { get; private set; }
    public int Speed { set; get; } = 2;
    private bool isSelected = false;


    public void Setup(VehicleState data)
    {
        this.vehicleData = data;
        isSelected = false;
    }

    private void OnMouseUp()
    {
        Debug.Log("hien thi ra ten " + gameObject.name);
        if (isSelected) return;
        GameMechanics.ProcessVehicalClick(this); // tthực hiên  khi click vào object
    }

    public void SetSelected(bool isSelected)
    {
        this.isSelected = isSelected;
    }

    private void Update()
    {
        if (vehicleData.IsFull)
        {
            this.transform.DOMove(new Vector2(100,this.transform.position.y),0.5f);
        }
    }

    public void CheckColorPeron(Transform parkingSlotPos)
    {
        StartCoroutine(CheckColorPeronRoutine(parkingSlotPos)) ;
    }
    private  IEnumerator CheckColorPeronRoutine(Transform parkingSlotPos)
    {
        Debug.Log($"hien thi ra {vehicleData.capacity} vaf {vehicleData.currentOccupied}");
        while (GameManager.Instance.LinePperson.Count > 0 && vehicleData.capacity > vehicleData.currentOccupied)
        {
            GameObject firstPersonObj = GameManager.Instance.LinePperson[0];
            PersonVisual person = firstPersonObj.GetComponentInChildren<PersonVisual>();
            if (vehicleData.color == person.ColorPerson)
            {
                Debug.Log("có thể  di chueyern được người");
                GameManager.Instance.LinePperson.RemoveAt(0);

               GameManager.Instance.UpdateLinePerson();

                person.transform.DOKill();
                person.transform.DOMove(parkingSlotPos.position, 0.5f)
                    .SetEase(Ease.InQuad); // di chuyen nguoi len xe

                vehicleData.GotOnBus();// thuc hien tnawg nguoi
                yield return new WaitForSeconds(0.1f);
            }
            else if(vehicleData.color != person.ColorPerson)
                break; // khong thực hiện nếu không cung màu

        }
    }

  
}
