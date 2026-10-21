using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameMechanics
{
    // click di chuyển xe
    private static HashSet<VehicleController> movingVehicles = new HashSet<VehicleController>();
    static float timeVehicleTween = 1;
    static float roadVehicle;
    public static void ProcessVehicalClick(VehicleController vehicalController)
    {
       // Debug.Log("thực hien di chuyển A");

        if (!GameManager.Instance.gridManager.Board.CheckIfPathIsClear(vehicalController.vehicleData))
        {


           // Debug.Log("xe bị cản không thể di chuyển được " + vehicalController.vehicleData.id);
            return;

        }

        //vehicalController.SetSelected(true); // set có thể click

        //GameManager.Instance.gridManager.Board.RemoveVehicleFromBoard(vehicalController.vehicleData.id);

        //MoveVehicleTween(vehicalController);
        //Debug.Log("thực hien di chuyển b");

        ICommand icommand = new RedoCommand(vehicalController);
        GameManager.Instance.gridManager.RedoBoost.ExecuteCommand(icommand); // them lich su di chuyển của xe

    }

    // thưc hien xe đến nơi 
    public static void MoveVehicleTween(VehicleController vehicle, System.Action<Transform> OnArrived = null)
    {
        if (vehicle == null && !movingVehicles.Add(vehicle)) return;

        Transform targetSlot = GetNextAvailableSlot(vehicle.gameObject, GameManager.Instance.gridManager.WhiteSlots, GameManager.Instance.gridManager.slotOccupants);

        // 1. Chặn ngay từ đầu nếu hết ô đỗ
        if (targetSlot == null)
        {
            movingVehicles.Remove(vehicle);
            Debug.Log("Slot đã đầy - Bạn thua!");
            return;
        }

        roadVehicle = targetSlot.position.y - 2f;
        Board board = GameManager.Instance.gridManager.Board;
        VehicleState vehicleState = vehicle.vehicleData;

        Vector3 moveDir = ToDelta(vehicleState.direction);
        float exitDistance = Mathf.Max(board.width, board.height) * (board.spacing + board.cellSize);
        Vector3 exitPos = vehicle.transform.position + moveDir * exitDistance;

        Sequence vehicleSeq = DOTween.Sequence();

        // 2. Xử lý logic di chuyển kèm xoay đầu xe theo từng hướng
        if (vehicleState.direction == MoveDirection.Up)
        {
            vehicleSeq.Append(CreateMoveAndRotateTween(vehicle, new Vector3(vehicle.transform.position.x, roadVehicle, 0), timeVehicleTween, Ease.InSine))
                      .Append(CreateMoveAndRotateTween(vehicle, new Vector3(targetSlot.position.x, roadVehicle, 0), timeVehicleTween, Ease.OutSine))
                      .Append(CreateMoveAndRotateTween(vehicle, targetSlot.position, timeVehicleTween, Ease.OutSine));
        }
        else if (vehicleState.direction == MoveDirection.Down)
        {
            vehicleSeq.Append(CreateMoveAndRotateTween(vehicle, exitPos, 1f, Ease.OutSine));

            int side = GetHorizontalSide(vehicle.transform);
            if (side != 0)
            {
                float shiftX = vehicle.transform.position.x + (side > 0 ? 100f : -100f);

                vehicleSeq.Append(CreateMoveAndRotateTween(vehicle, new Vector3(shiftX, vehicle.transform.position.y, 0), timeVehicleTween, Ease.OutSine))
                          .Append(CreateMoveAndRotateTween(vehicle, new Vector3(shiftX, roadVehicle, 0), timeVehicleTween, Ease.OutSine))
                          .Append(CreateMoveAndRotateTween(vehicle, new Vector3(targetSlot.position.x, roadVehicle, 0), timeVehicleTween, Ease.OutSine))
                          .Append(CreateMoveAndRotateTween(vehicle, targetSlot.position, timeVehicleTween, Ease.OutSine));
            }
        }
        else if (vehicleState.direction == MoveDirection.Left || vehicleState.direction == MoveDirection.Right)
        {
            vehicleSeq.Append(CreateMoveAndRotateTween(vehicle, exitPos, 1f, Ease.InQuad))
                      .Append(CreateMoveAndRotateTween(vehicle, new Vector3(vehicle.transform.position.x, roadVehicle, 0), 1f, Ease.OutQuad))
                      .Append(CreateMoveAndRotateTween(vehicle, new Vector3(targetSlot.position.x, roadVehicle, 0), 1f, Ease.OutQuad))
                      .Append(CreateMoveAndRotateTween(vehicle, targetSlot.position, 1f, Ease.OutQuad));
        }

        // 3. Callback hoàn tất chung
        vehicleSeq.OnComplete(() =>
        {
            vehicle.transform.position = targetSlot.position + new Vector3(0, 1, 0);
            vehicle.transform.eulerAngles = new Vector3(0, 0, 90);

            vehicle.SetParkingSlot(targetSlot);
            GameManager.Instance.NotifyVehicles();
            OnArrived?.Invoke(targetSlot);
           movingVehicles.Remove(vehicle);
        });

        vehicleSeq.OnKill(() =>
        {
            movingVehicles.Remove(vehicle);
        });
    }

    // -------------------------------------------------------------
    // Hàm Helper: Tạo Tween vừa di chuyển tới target vừa tự động xoay đầu xe
    // -------------------------------------------------------------
    private static Tween CreateMoveAndRotateTween(VehicleController vehicle, Vector3 targetPos, float duration, Ease easeType)
    {
        return vehicle.transform.DOMove(targetPos, duration)
            .SetEase(easeType)
            .OnUpdate(() =>
            {
                Vector3 dir = targetPos - vehicle.transform.position;
                dir.z = 0;

                if (dir.sqrMagnitude > 0.001f)
                {
                    float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                    vehicle.transform.rotation = Quaternion.Euler(0, 0, angle);
                }
            });
    }

    private static int GetHorizontalSide(Transform vehiclePos, float tolerance = 0.05f)
    {
        if (Camera.main == null) return 0;

        Vector3 viewPortPos = Camera.main.WorldToViewportPoint(vehiclePos.position);
        if (Mathf.Abs(viewPortPos.x - 0.5f) <= tolerance) return 0; // giua
        return viewPortPos.x < 0.5 ? -1 : 1;
    }

    private static Transform GetNextAvailableSlot(GameObject vehicleObj, Transform[] whiteSlots, GameObject[] slotOccupants)
    {
            int count = Mathf.Min(whiteSlots.Length, slotOccupants.Length);
        for (int i = 0; i < count; i++)
        {
            Debug.Log(whiteSlots[i].name + "vi tri" + whiteSlots[i].transform.position);
            if (slotOccupants[i] != null && whiteSlots[i] == null) continue;
            
                slotOccupants[i] = vehicleObj;
                Debug.Log("slot  là "+ slotOccupants[i]);
                return whiteSlots[i];
            
        }
        return null;
    }

    public static Vector3 CalculatePathPosition(int index, Vector3 holderPersonPos)
    {
        Vector3 startPos = holderPersonPos;
        // Debug.Log(GetRightEdge());
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

    public static Vector3Int ToDelta(MoveDirection d) => d switch
    {
        MoveDirection.Up => Vector3Int.up,
        MoveDirection.Down => Vector3Int.down,
        MoveDirection.Left => Vector3Int.left,
        MoveDirection.Right => Vector3Int.right,
        _ => Vector3Int.zero
    };
    // Xác định góc quay của xe trên trục Y
    public static Quaternion GetRotationWorld(MoveDirection dir) => dir switch
    {
        MoveDirection.Up => Quaternion.Euler(0f, 0f, 90f),
        MoveDirection.Down => Quaternion.Euler(0f, 0f, -90f),
        MoveDirection.Left => Quaternion.Euler(0f, 0f, 180f),
        MoveDirection.Right => Quaternion.Euler(0f, 0f, 0f),
        _ => Quaternion.identity
    };

    public static string ToSymbol(MoveDirection d) => d switch
    {
        MoveDirection.Up => "↑",
        MoveDirection.Down => "↓",
        MoveDirection.Left => "←",
        MoveDirection.Right => "→",
        _ => "?"
    };

    // Ô "chờ" ngay ngoài mép lưới theo hướng thoát của xe
    public static Vector3Int GetWaitSlot(Vector3Int headPos, MoveDirection dir, int gridWidth)
    {
        return dir == MoveDirection.Right
            ? new Vector3Int(gridWidth, headPos.y, 0)
            : new Vector3Int(-1, headPos.y, 0);
    }

    public static void Add(int value)
    {
        GameManager.Instance.curentCoin += value;
    }
}
