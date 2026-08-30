using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameMechanics
{

    //static int offset = 10;
    //private static Board board;
    //private static GridManager gridManager;
    //private static Transform[] whiteSlots;
    //private static GameObject[] slotOccupants;

    //// Hàm khởi tạo (Constructor) để nhận dữ liệu từ MonoBehaviour bên ngoài truyền vào
    //public static GameMechanics(Board board, GridManager gridManager, Transform[] whiteSlots)
    //{
    //    this.board = board;
    //    this.gridManager = gridManager;
    //    this.whiteSlots = whiteSlots;
    //    this.slotOccupants = new GameObject[whiteSlots.Length];
    //}

    // click di chuyển xe
    public static void ProcessVehicalClick(VehicleController vehicalController)
    {
        Debug.Log("thực hien di chuyển A");

        //  Debug.LogWarning("hien thi ra vehicle data" + vehicalController.vehicleData);
        if (GameManager.Instance.gridManager.Board.CheckIfPathIsClear(vehicalController.vehicleData))
        {
            //vehicalController.SetSelected(true); // set có thể click

            //GameManager.Instance.gridManager.Board.RemoveVehicleFromBoard(vehicalController.vehicleData.id);

            //MoveVehicleTween(vehicalController);
            Debug.Log("thực hien di chuyển b");

            ICommand icommand = new RedoCommand(vehicalController);


            GameManager.Instance.RedoBoost.ExecuteCommand(icommand);

        }
        else
        {
            Debug.Log("xe bị cản không thể di chuyển được " + vehicalController.vehicleData.id);
        }
    }

    // thưc hien xe đến nơi 
    public static void MoveVehicleTween(VehicleController vehicle, System.Action<Transform> OnArrived = null)
    {
        Board board = GameManager.Instance.gridManager.Board;
        VehicleState vehicleState = vehicle.vehicleData;

        Vector3 moveDir = board.DirToDelta(vehicleState.direction); // trả về hướng hiên tại của xe
        float exitDistance = Mathf.Max(board.width, board.height) * (board.spacing + board.cellSize);
        Vector3 exitPos = vehicle.transform.position + moveDir * exitDistance;


        Transform targetSlot = GetNextAvailableSlot(vehicle.gameObject, GameManager.Instance.gridManager.WhiteSlots, GameManager.Instance.gridManager.slotOccupants);

        Sequence vehicleSeq = DOTween.Sequence();

        if (targetSlot != null)
        {
            vehicleSeq.Append(vehicle.transform.DOMove(exitPos, 1f).SetEase(Ease.InQuad));
            Tween moveToSlotTween = vehicle.transform.DOMove(targetSlot.position, 1)
                .SetEase(Ease.OutQuad)
                .OnUpdate(() =>
                {
                    Vector3 dirToSlot = targetSlot.position - vehicle.transform.position;
                    dirToSlot.z = 0;

                    if (dirToSlot != Vector3.zero)
                    {
                        // Tính góc xoay 2D chuẩn trên mặt phẳng XY
                        float angle = Mathf.Atan2(dirToSlot.y, dirToSlot.x) * Mathf.Rad2Deg;
                        vehicle.transform.rotation = Quaternion.Euler(0, 0, angle);
                    }
                });
            vehicleSeq.Append(moveToSlotTween);

            vehicleSeq.OnComplete(() =>
            {
                vehicle.transform.position = targetSlot.position + new Vector3(0, 1, 0);
                vehicle.transform.eulerAngles = new Vector3(0, 0, 90);

                //  vehicle.CheckColorPeron(targetSlot); // kiểm tra người cùng mau không khi xe đến nơi đỗ
                // vehicle.IsArrived = true;
                vehicle.SetParkingSlot(targetSlot); // đến slot trong
                GameManager.Instance.NotifyVehicles();

                OnArrived?.Invoke(targetSlot);
            });
        }
        else
        {
            vehicleSeq.OnComplete(() =>
            {
                Debug.Log("slot đã đầy bạn thua");
            });

        }

    }

    private static Transform GetNextAvailableSlot(GameObject vehicleObj, Transform[] whiteSlots, GameObject[] slotOccupants)
    {
        for (int i = 0; i < whiteSlots.Length; i++)
        {
            Debug.Log(whiteSlots[i].name + "vi tri" + whiteSlots[i].transform.position);
            if (slotOccupants[i] == null)
            {
                slotOccupants[i] = vehicleObj;
                return whiteSlots[i];
            }
        }
        return null;
    }

    public static Vector3 CalculatePathPosition(int index, Vector3 holderPersonPos)
    {
        Vector3 startPos = holderPersonPos;
        Debug.Log(GetRightEdge());
        float rightEdge = GetRightEdge() - 0.5f;
        float distance = index * 0.5f;
        float maxDistance = rightEdge - startPos.x;

        if (distance <= maxDistance)
        {
            return new Vector3(startPos.x + distance, startPos.y, startPos.z);
        }
        float remain = distance - maxDistance;
        return new Vector3(rightEdge, startPos.y + remain, startPos.z);
    }

    private static float GetRightEdge()
    {
        Camera camera = Camera.main;
        return camera.ViewportToWorldPoint(new Vector3(1, 0.5f, Mathf.Abs(camera.transform.position.z))).x;
    }


  
}
