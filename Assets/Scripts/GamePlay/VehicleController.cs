using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class VehicleController : MonoBehaviour
{
    public VehicleState vehicleData { get; private set; }
    public Transform CurrentSlot { get; private set; }

    private Coroutine checkRoutine;


    private bool isSelected = false;
    public bool IsLoading { get; private set; }
    public bool IsArrived { get; private set; }

    static bool WaitSelected = true;

    private static float lastSelectTime = -999f;
    public static float selectCooldown = 2f;

    float timeDelay = 0;
    float timeDelayMax = 2;

    private static VehicleController vehicleController;

    public List<Transform> SeatedPassengers { get; private set; } = new List<Transform>();

    public void Setup(VehicleState data)
    {
        this.vehicleData = data;
        isSelected = false;
    }

    public void SetParkingSlot(Transform slot)
    {
        CurrentSlot = slot;
        IsArrived = true;

    }

    private void OnMouseUp()
    {
        Debug.Log("hien thi ra ten " + gameObject.name);

        if (Time.time - lastSelectTime < selectCooldown) return;
        if (vehicleController != null && vehicleController != this)
            return;

        GameMechanics.ProcessVehicalClick(this); // tthực hiên  khi click vào object
    }

    public void SetSelected(bool isSelected)
    {
        this.isSelected = isSelected;
        if (this.isSelected)
        {
            lastSelectTime = Time.time;
        }
        WaitSelected = !isSelected;
    }

    private void Update()
    {
        if (!IsArrived) return;
        if (IsLoading) return;

        //checkTimer += Time.deltaTime;
        //if (checkTimer > checkInterval)
        //{
        //    checkTimer = 0;
        //    CheckColorPerson();

        //}
        //if (vehicleData.IsFull)
        //{
        //    this.transform.DOMove(new Vector2(100, this.transform.position.y), 0.5f);
        //}


        //if (!WaitSelected)
        //{
        //    timeDelay += Time.deltaTime;
        //    if (timeDelay > timeDelayMax)
        //    {
        //        WaitSelected = true;
        //        timeDelay = 0;
        //    }
        //}

    }

    public bool CheckColorPerson()
    {
        if (IsLoading) return false;
        if (checkRoutine != null) return false;
        if (vehicleData.IsFull) return false;
        if (GameManager.Instance.LinePperson.Count == 0) return false;

        //if (GameManager.Instance.CurrentLoadingVehicle != null)
        //{
        //    // Nếu không phải chính mình thì không được đón
        //    if (GameManager.Instance.CurrentLoadingVehicle != this)
        //        return false;
        //}

        if (vehicleController != null && vehicleController != this)
        {
            return false;
        }
        PersonVisual person = GameManager.Instance.LinePperson[0].GetComponentInChildren<PersonVisual>();

        if (person.ColorPerson != vehicleData.color) return false;

        IsLoading = true;
        // GameManager.Instance.CurrentLoadingVehicle = this;
        vehicleController = this;
        checkRoutine = StartCoroutine(CheckColorPeronRoutine()); // đưa người lên xe
        return true;
    }

    //public void CheckColorPeron(Transform parkingSlotPos)
    //{
    //   StartCoroutine(CheckColorPeronRoutine(parkingSlotPos));
    //}
    private IEnumerator CheckColorPeronRoutine()
    {
        Debug.Log($"hien thi ra {vehicleData.capacity} vaf {vehicleData.currentOccupied}");
        while (GameManager.Instance.LinePperson.Count > 0 && vehicleData.capacity > vehicleData.currentOccupied)
        {
            // GameObject firstPersonObj = GameManager.Instance.LinePperson[0];
            PersonVisual person = GameManager.Instance.LinePperson[0].GetComponentInChildren<PersonVisual>();
            if (vehicleData.color == person.ColorPerson)
            {
                Debug.Log("có thể  di chueyern được người");
                GameManager.Instance.LinePperson.RemoveAt(0);

                GameManager.Instance.UpdateLinePerson();

                vehicleData.GotOnBus();// thuc hien tnawg nguoi

                person.transform.DOKill();
                person.transform.DOMove(CurrentSlot.position, 0.5f)
                    .SetEase(Ease.InQuad); // di chuyen nguoi len xe
                                           // Debug.Log("hien thi ra persont tra" + person.transform.parent);
                person.transform.parent.SetParent(this.transform);
                SeatedPassengers.Add(person.transform.parent);

                // Debug.Log("hien thi ra persont tra" + person.transform.parent);

                yield return new WaitForSeconds(0.1f);

            }
            else if (vehicleData.color != person.ColorPerson)
            {

                //yield return null; // khong thực hiện nếu không cung màu
                //continue;
                break;
            }
        }

        IsLoading = false;
        checkRoutine = null;

        if (vehicleData.IsFull)
        {
            yield return new WaitForSeconds(0.2f);
            this.transform.DOKill();

            Sequence leaveSeq = DOTween.Sequence();
            // 📌 BƯỚC 1: Di chuyển xe xuống dưới một khoảng (ví dụ: lùi xuống 1.5 đơn vị)
            leaveSeq.Append(transform.DOMoveY(transform.position.y - 1.5f, 1).SetEase(Ease.OutQuad));

            // 📌 BƯỚC 2: Xoay xe sang phải (Góc 0 độ trên mặt phẳng 2D XY)
            // Dùng RotateMode.FastBeyond3D hoặc Quaternion để xoay góc mượt mà
            leaveSeq.Append(transform.DORotate(new Vector3(0, 0, 0), 1, RotateMode.FastBeyond360).SetEase(Ease.InOutQuad));

            // 📌 BƯỚC 3: Chạy thẳng ra ngoài bên phải (tăng thời gian lên 2.0s để chạy chậm rãi)
            leaveSeq.Append(transform.DOMoveX(100f, 2.0f).SetEase(Ease.InQuad));

            GameManager.Instance.ClearSlot(this.gameObject);

            CurrentSlot = null;
            IsArrived = false;
            //  GameManager.Instance.CurrentLoadingVehicle = null;
            vehicleController = null;
        }
        else
        {
            vehicleController = null;
        }
    }

    public void ReuturnPassengersToLine()
    {
        if (SeatedPassengers.Count == 0) return;
        IsArrived = false; // Tắt flag để Update() không gọi CheckColorPerson() nữa
        IsLoading = false;

        if (checkRoutine != null)
        {
            StopCoroutine(checkRoutine);
            checkRoutine = null;
        }

        for (int i = SeatedPassengers.Count - 1; i >= 0; i--)
        {
            Transform personObj = SeatedPassengers[i]; // lấy từng person để gắn rồi  đỏi cha

            if (personObj == null) continue;


            personObj.SetParent(GameManager.Instance.gridManager.HolderPerson);

            Debug.Log("nguoi hien tại là" + personObj.name + " " + personObj.parent.name + " " +
                GameManager.Instance.gridManager.HolderPerson.name);

            GameManager.Instance.LinePperson.Insert(0, personObj.gameObject);

            vehicleData.currentOccupied--;

            //Vector3 targetPos = GameManager.Instance.GetLineSlotPosition(0);
            //// ví dụ: GameManager.Instance.LineSlots[0].position;

            //personObj.DOKill();
            //personObj.DOMove(targetPos, 0.5f)
            //    .SetEase(Ease.OutQuad);

            PersonVisual personVisual = personObj.GetComponentInChildren<PersonVisual>();
            if (personVisual != null)
            {
                personVisual.transform.DOKill();
                personVisual.transform.DOLocalMove(Vector3.zero, 0.3f);
            }

        }

        SeatedPassengers.Clear(); // xóa danh sách người trong xe
        GameManager.Instance.UpdateLinePerson(); // cập nhật lại số người
    }

}
