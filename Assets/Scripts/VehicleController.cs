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
    private static bool isArrived = false;
    public bool IsArrived { set => isArrived = value; get => isArrived; }
    private static bool isLeave = false;
    public bool IsLeave { set => isLeave = value; get => isLeave; }


    public void Setup(VehicleState data)
    {
        this.vehicleData = data;
        isSelected = false;
        isArrived = false;
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
        //if (vehicleData.IsFull)
        //{
        //    this.transform.DOMove(new Vector2(100, this.transform.position.y), 0.5f);
        //}
        
    }

    public void CheckColorPeron(Transform parkingSlotPos)
    {
        StartCoroutine(CheckColorPeronRoutine(parkingSlotPos));
    }
    private IEnumerator CheckColorPeronRoutine(Transform parkingSlotPos)
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

                vehicleData.GotOnBus();// thuc hien tnawg nguoi

                person.transform.DOKill();
                person.transform.DOMove(parkingSlotPos.position, 0.5f)
                    .SetEase(Ease.InQuad); // di chuyen nguoi len xe
                Debug.Log("hien thi ra persont tra"+person.transform.parent );
                person.transform.parent.SetParent(this.transform);
              
                Debug.Log("hien thi ra persont tra"+person.transform.parent );

                yield return new WaitForSeconds(0.1f);

            }
            else if (vehicleData.color != person.ColorPerson)
            {

                yield return null; // khong thực hiện nếu không cung màu
                continue;
            }
        }
        if (vehicleData.IsFull)
        {
            yield return new WaitForSeconds(0.2f);
            this.transform.DOKill();

            Sequence leaveSeq = DOTween.Sequence();
            // 📌 BƯỚC 1: Di chuyển xe xuống dưới một khoảng (ví dụ: lùi xuống 1.5 đơn vị)
            leaveSeq.Append(transform.DOMoveY(transform.position.y - 1.5f, 4).SetEase(Ease.OutQuad));

            // 📌 BƯỚC 2: Xoay xe sang phải (Góc 0 độ trên mặt phẳng 2D XY)
            // Dùng RotateMode.FastBeyond3D hoặc Quaternion để xoay góc mượt mà
            leaveSeq.Append(transform.DORotate(new Vector3(0, 0, 0), 2, RotateMode.FastBeyond360).SetEase(Ease.InOutQuad));

            // 📌 BƯỚC 3: Chạy thẳng ra ngoài bên phải (tăng thời gian lên 2.0s để chạy chậm rãi)
            leaveSeq.Append(transform.DOMoveX(100f, 2.0f).SetEase(Ease.InQuad));

            GameManager.Instance.ClearSlot(this.gameObject);
        }
    }


}
