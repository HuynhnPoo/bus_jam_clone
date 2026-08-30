using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RedoCommand : ICommand
{

    private readonly VehicleController vehicleController;

    private readonly Vector3 _startPos;
    private readonly Quaternion _startRot;

    private readonly Transform _targetSlot;

    private Transform arrivedSlot;

    private readonly List<GameObject> _boardedPassengers;

    public RedoCommand(VehicleController vehicleController)
    {
        this.vehicleController = vehicleController;
        _startPos = vehicleController.transform.position;
        _startRot = vehicleController.transform.rotation;
    }

    public void Execute()
    {
        vehicleController.SetSelected(true);
        GameManager.Instance.gridManager.Board.RemoveVehicleFromBoard(vehicleController.vehicleData.id);
        GameMechanics.MoveVehicleTween(vehicleController,OnVehicleArrived);
    }

    // Được gọi khi tween di chuyển xe hoàn tất và đã biết chính xác slot
    private void OnVehicleArrived(Transform slot)
    {
        arrivedSlot = slot;
        GameManager.Instance.RedoBoost.AddQuantity();
        // Lưu lại người hiện đang ở trong xe tại thời điểm đỗ (nếu có)
        // vehicleController = new List<GameObject>(vehicleController.GetPassengers());
    }


    public void Undo()
    {
        vehicleController.DOKill();

        vehicleController.ReuturnPassengersToLine(); // trả người về hàng

        int slotIndex = GetSlotIndex(); // nhận ô xe vừa di chuyển ra để rồi clear nó

        if (slotIndex != -1) GameManager.Instance.gridManager.slotOccupants[slotIndex] = null;
    
        vehicleController.SetParkingSlot(null); // xóa thông tin xe

        // di chuyển xe
        Sequence sequence = DOTween.Sequence();
        sequence.Append(vehicleController.transform.DOMove(_startPos, 0.8f).SetEase(Ease.OutCubic));
        sequence.Join(vehicleController.transform.DORotateQuaternion(_startRot, 0.8f));
        sequence.OnComplete(() =>
        {
            vehicleController.SetSelected(false);
            GameManager.Instance.NotifyVehicles();

        });
    }

    private int GetSlotIndex()
    {
        if (arrivedSlot == null) return -1;
        Transform[] slots = GameManager.Instance.gridManager.WhiteSlots;
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] == arrivedSlot) // gắn slot xe đã đến
            {
                return i;
            }
        }
        return -1;
    }
}

