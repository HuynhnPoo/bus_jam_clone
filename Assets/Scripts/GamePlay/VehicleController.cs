using DG.Tweening;
using DG.Tweening.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SocialPlatforms.Impl;
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

    private void Start()
    {
        // Debug.Log("hien thi ra "+ vehicleData.isActive +"ID"+ vehicleData.id+ " "+vehicleData.IsFull);
        //Debug.Log("da được chon" + isSelected + IsLoading + WaitSelected);
    }
    public List<Transform> SeatedPassengers { get; private set; } = new List<Transform>();

    public void Setup(VehicleState data)
    {
        this.vehicleData = data;
        isSelected = false;
    }

    public void SetParkingSlot(Transform slot)
    {
        //  Debug.Log("slot"+ slot.name);
        CurrentSlot = slot;
        IsArrived = slot != null;

    }

    private void OnMouseUp()
    {
        //  Debug.Log("hien thi ra ten " + gameObject.name);

        // if (Time.time - lastSelectTime < selectCooldown) return;
        Debug.Log("thông số của xe" + vehicleData.length + "so ngưởi" + vehicleData.capacity);
        if (vehicleController != null && vehicleController != this)
            return;

        GameMechanics.ProcessVehicalClick(this); // tthực hiên  khi click vào object
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
        //    checkTimer = 0;
        //    CheckColorPerson();

        //}
        //if (vehicleData.IsFull)
        //{
        //    this.transform.DOMove(new Vector2(100, this.transform.position.y), 0.5f);
        //}


        if (!WaitSelected)
        {
            timeDelay += Time.deltaTime;
            if (timeDelay > timeDelayMax)
            {
                WaitSelected = true;
                timeDelay = 0;
            }
        }



    }


    public bool CheckColorPerson()
    {
        // Debug.Log($"[CheckColor] Xe {vehicleData.id}: IsLoading={IsLoading}, checkRoutine={(checkRoutine != null)}, IsFull={vehicleData.IsFull}, LinePerson.Count={GameManager.Instance.LinePperson.Count}");
        if (!IsArrived || CurrentSlot == null) return false;


        if (IsLoading) return false;
        if (checkRoutine != null) return false;
        if (vehicleData.IsFull) return false;
        if (GameManager.Instance.LinePperson.Count == 0) return false;

        if (GameManager.Instance.CurrentLoadingVehicle != null
            && GameManager.Instance.CurrentLoadingVehicle != this)
        {
            // Nếu không phải chính mình thì không được đón

            return false;
        }

        if (vehicleController != null && vehicleController != this)
        {
            return false;
        }
        PersonVisual person = GameManager.Instance.LinePperson[0].GetComponentInChildren<PersonVisual>();

        if (person.ColorPerson != vehicleData.color) return false;

        IsLoading = true;
        GameManager.Instance.CurrentLoadingVehicle = this;
        vehicleController = this;
        checkRoutine = StartCoroutine(CheckColorPeronRoutine()); // đưa người lên xe
        return true;
    }


    private IEnumerator CheckColorPeronRoutine()
    {
        while (GameManager.Instance.LinePperson.Count > 0 && !vehicleData.IsFull)
        {
            if (CurrentSlot == null) break;

            GameObject personObject = GameManager.Instance.LinePperson[0];

            if (personObject == null)
            {
                GameManager.Instance.LinePperson.RemoveAt(0);
                GameManager.Instance.UpdateLinePerson();
                continue;
            }

            PersonVisual person = personObject.GetComponentInChildren<PersonVisual>();

            if (person == null || person.ColorPerson != vehicleData.color)
                break;

            //Color? oldPersonColor = null;
            //if (GameManager.Instance.LinePperson.Count > 0)
            //{
            //    PersonVisual nextFrontPerson = GameManager.Instance.LinePperson[0].GetComponentInChildren<PersonVisual>();
            //    oldPersonColor = nextFrontPerson?.ColorPerson;
            //}

            Transform personRoot = personObject.transform;

            GameManager.Instance.LinePperson.RemoveAt(0);
            GameManager.Instance.UpdateLinePerson();

            //Color? newPersonColor = null;
            //if(GameManager.Instance.LinePperson.Count > 0)
            //{
            //    PersonVisual nextFrontPerson = GameManager.Instance.LinePperson[0].GetComponentInChildren<PersonVisual>();
            //    newPersonColor = nextFrontPerson?.ColorPerson;
            //}

            //if (oldPersonColor != newPersonColor)
            //{
            //    GameManager.Instance.NotifyVehicles();
            //}

            person.transform.DOKill();
            personRoot.DOKill();

            // Di chuyển object gốc của người lên vị trí xe.
            Tween boardingTween = personRoot.DOMove(CurrentSlot.position, 0.1f)
                .SetEase(Ease.InQuad);

            yield return boardingTween.WaitForCompletion();

            // Nếu object bị hủy trong lúc tween, dừng xử lý người này.
            if (personObject == null || this == null)
                break;

            // Đổi parent sau khi người đã đến xe.
            personRoot.SetParent(transform, true);

            SeatedPassengers.Add(personRoot);

            // Chỉ tăng số người sau khi đã lên xe hoàn tất.
            vehicleData.GotOnBus();

            yield return new WaitForSeconds(0.1f);
        }

        // Kiểm tra xe đã đủ người chưa
        if (vehicleData.IsFull)
        {
            yield return new WaitForSeconds(0.2f);

            if (this == null) yield break;

            transform.DOKill();

            Sequence leaveSeq = DOTween.Sequence();

            leaveSeq.Append(transform.DOMoveY(
                transform.position.y - 1.5f, 1f)
                .SetEase(Ease.OutQuad));

            leaveSeq.Append(transform.DORotate(
                new Vector3(-90f, -90f, 90f), 1f)
                .SetEase(Ease.InOutQuad));

            leaveSeq.Append(transform.DOMoveX(100f, 2f)
                .SetEase(Ease.InQuad));

            leaveSeq.OnComplete(
                () =>
                {
                    GameManager.Instance.CurrentVehicle.Remove(this.gameObject);
                    if (GameManager.Instance.CurrentVehicle.Count == 0 && GameManager.Instance.LinePperson.Count == 0)
                        GameManager.isWon = true;
                });
            // Giải phóng ô đỗ
            GameManager.Instance.ClearSlot(gameObject);

            CurrentSlot = null;
            IsArrived = false;
        }

        // Giải phóng trạng thái đang đón người
        IsLoading = false;
        checkRoutine = null;

        if (GameManager.Instance.CurrentLoadingVehicle == this)
        {
            GameManager.Instance.CurrentLoadingVehicle = null;
        }

        if (vehicleController == this)
        {
            vehicleController = null;
        }

        // QUAN TRỌNG:
        // Kiểm tra lại tất cả xe đã đỗ sau khi nhóm người hiện tại kết thúc.
        GameManager.Instance.NotifyVehicles();
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
            Transform personObj = SeatedPassengers[i]; // lấy từng person để gắn rồi  đỏi cha

            if (personObj == null) continue;


            personObj.SetParent(GameManager.Instance.gridManager.HolderPerson);

            Debug.Log("nguoi hien tại là" + personObj.name + " " + personObj.parent.name + " " +
              GameManager.Instance.gridManager.HolderPerson.name);

            GameManager.Instance.LinePperson.Insert(0, personObj.gameObject);

            vehicleData.currentOccupied--;

            //Vector3 targetPos = GameManager.Instance.GetLineSlotPosition(0);
            // ví dụ: GameManager.Instance.LineSlots[0].position;

            //personObj.DOKill();
            //personObj.DOMove(targetPos, 0.5f)
            //    .SetEase(Ease.OutQuad);

            PersonVisual personVisual = personObj.GetComponentInChildren<PersonVisual>();
            if (personVisual != null)
            {
                personVisual.transform.DOKill();
                personVisual.transform.DOLocalMove(Vector3.zero, 0.1f);
            }

        }

        SeatedPassengers.Clear(); // xóa danh sách người trong xe
        GameManager.Instance.UpdateLinePerson(); // cập nhật lại số người

        //   GameManager.Instance.NotifyVehicles();
    }

}