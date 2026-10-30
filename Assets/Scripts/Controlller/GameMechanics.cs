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

            
             Debug.LogWarning("xe bị cản không thể di chuyển được " + vehicalController.vehicleData.id);
            GameManager.Instance.CameraShaking(1,1);
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
        if (vehicle == null || !movingVehicles.Add(vehicle)) return;

        Transform targetSlot = GetNextAvailableSlot(vehicle.gameObject, GameManager.Instance.gridManager.WhiteSlots, GameManager.Instance.gridManager.slotOccupants);

        if (targetSlot == null)
        {
            movingVehicles.Remove(vehicle);
            Debug.Log("Slot đã đầy - Bạn thua!");
            GameManager.isGameOver = true;
            return;
        }

        roadVehicle = targetSlot.position.y - 2f;
        Board board = GameManager.Instance.gridManager.Board;
        VehicleState vehicleState = vehicle.vehicleData;

        // Lưu vị trí ban đầu trước khi tạo Tween
        Vector3 startPos = vehicle.transform.position;
        float startX = startPos.x;
        float startY = startPos.y;

        Vector3 moveDir = ToDelta(vehicleState.direction);
        float exitDistance = Mathf.Max(board.width, board.height) * (board.spacing + board.cellSize);
        Vector3 exitPos = startPos + moveDir * exitDistance;

        Sequence vehicleSeq = DOTween.Sequence();

        if (vehicleState.direction == MoveDirection.Up)
        {
            float horizontalAngle = targetSlot.position.x >= startX ? -90f : 90f;

            vehicleSeq.Append(CreateStep(vehicle, new Vector3(startX, roadVehicle, startPos.z), vehicle.transform.eulerAngles.x, 1f, Ease.InQuad))
                      .Append(CreateStep(vehicle, new Vector3(targetSlot.position.x, roadVehicle, startPos.z), horizontalAngle, 1f, Ease.OutQuad))
                      .Append(CreateStep(vehicle, targetSlot.position, 0f, 1f, Ease.OutQuad));
        }
        // 2. Hướng đi: DOWN (Xuống)
        else if (vehicleState.direction == MoveDirection.Down)
        {
            // Tính toán vị trí mép dưới ngoài màn hình
            float bottomEdgeY = (Camera.main != null) ? Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0f, Mathf.Abs(Camera.main.transform.position.z))).y - 3f : startPos.y - 15f;
            Vector3 outOfScreenPos = new Vector3(startX, bottomEdgeY, startPos.z);

            // Xác định xem targetSlot nằm bên trái hay bên phải để dịch sang lề tương ứng ngoài màn hình
            int side = targetSlot.position.x >= startX ? 1 : -1;
            float shiftX = startX + (side > 0 ? 15f : -15f);
            float horizontalAngle = targetSlot.position.x >= shiftX ? -90f : 90f;

            vehicleSeq.Append(CreateMoveTween(vehicle, outOfScreenPos, 1f, Ease.InQuad))
                      .Append(CreateMoveTween(vehicle, new Vector3(shiftX, bottomEdgeY, startPos.z), 1f, Ease.OutQuad))
                      .Append(CreateStep(vehicle, new Vector3(shiftX, roadVehicle, startPos.z), 0f, 1f, Ease.OutQuad))
                      .Append(CreateStep(vehicle, new Vector3(targetSlot.position.x, roadVehicle, startPos.z), horizontalAngle, 1f, Ease.OutQuad))
                      .Append(CreateStep(vehicle, targetSlot.position, 0f, 1f, Ease.OutQuad));
        }
        // 3. Hướng đi: LEFT hoặc RIGHT (Trái / Phải)
        else if (vehicleState.direction == MoveDirection.Left || vehicleState.direction == MoveDirection.Right)
        {

            float verticalAngle = roadVehicle >= startY ? 0f : 180f;
            float horizontalAngle = targetSlot.position.x >= exitPos.x ? -90f : 90f;

            vehicleSeq.Append(CreateStep(vehicle, new Vector3(exitPos.x, startY, startPos.z), vehicle.transform.eulerAngles.x, 1f, Ease.InQuad))
                .Append(CreateStep(vehicle, new Vector3(exitPos.x, roadVehicle, startPos.z), verticalAngle, 1f, Ease.OutQuad))
                .Append(CreateStep(vehicle, new Vector3(targetSlot.position.x, roadVehicle, startPos.z), horizontalAngle, 1f, Ease.OutQuad))
                .Append(CreateStep(vehicle, targetSlot.position, 0f, 1f, Ease.OutQuad));

        }

        // Callback hoàn tất chung khi xe đến nơi
        vehicleSeq.OnComplete(() =>
        {
            vehicle.transform.position = targetSlot.position + new Vector3(0, 1,0);
            vehicle.transform.eulerAngles = new Vector3(0, -90, 90);

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

    private static Sequence CreateStep(VehicleController vehicle, Vector3 targetPos, float targetRotationX, float duration, Ease easeType)
    {
        Sequence stepSeq = DOTween.Sequence();

        // 1. Xoay đầu xe tại chỗ mượt mà trước (0.2s)
        stepSeq.Append(vehicle.transform.DORotate(new Vector3(targetRotationX, -90f, 90f), 0.2f, RotateMode.Fast).SetEase(Ease.OutQuad));

        // 2. Di chuyển đến vị trí đích trong thời gian còn lại
        float moveDuration = Mathf.Max(0.01f, duration - 0.2f);
        stepSeq.Append(vehicle.transform.DOMove(targetPos, moveDuration).SetEase(easeType));

        return stepSeq;
    }

    // Hàm Helper: Chỉ di chuyển vị trí, tuyệt đối không đổi rotation
    private static Tween CreateMoveTween(VehicleController vehicle, Vector3 targetPos, float duration, Ease easeType)
    {
        return vehicle.transform.DOMove(targetPos, duration).SetEase(easeType);
    }

    // Hàm Helper: Chỉ xoay xe tại chỗ, không làm thay đổi vị trí
    private static Tween CreateRotateTween(VehicleController vehicle, float targetX, float duration)
    {
        return vehicle.transform.DORotate(new Vector3(targetX, -90f, 90f), duration, RotateMode.Fast)
            .SetEase(Ease.OutQuad);
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
            if (slotOccupants[i] != null || whiteSlots[i] == null) continue;

            slotOccupants[i] = vehicleObj;
            Debug.Log("slot  là " + slotOccupants[i]);
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
        MoveDirection.Up => Vector3Int.down,
        MoveDirection.Down => Vector3Int.up,
        MoveDirection.Left => Vector3Int.left,
        MoveDirection.Right => Vector3Int.right,
        _ => Vector3Int.zero
    };
    // Xác định góc quay của xe trên trục Y
    public static Quaternion GetRotationWorld(MoveDirection dir) => dir switch
    {
        MoveDirection.Up => Quaternion.Euler(0f, -90f, 90),
        MoveDirection.Down => Quaternion.Euler(180f, -90f, 90),
        MoveDirection.Left => Quaternion.Euler(90f, -90f, 90),
        MoveDirection.Right => Quaternion.Euler(-90f, -90f, 90f),
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
    // Ô "chờ" ngay ngoài mép lưới, đúng ô mà xe bước ra khi thoát
    public static Vector3Int GetWaitSlot(Vector3Int headPos, MoveDirection dir, int gridWidth, int gridHeight)
    {
        switch (dir)
        {
            case MoveDirection.Right: return new Vector3Int(gridWidth, headPos.y, 0);
            case MoveDirection.Left: return new Vector3Int(-1, headPos.y, 0);
            case MoveDirection.Up: return new Vector3Int(headPos.x, -1, 0);        // Up = y giảm
            case MoveDirection.Down: return new Vector3Int(headPos.x, gridHeight, 0); // Down = y tăng
            default: return new Vector3Int(-1, headPos.y, 0);
        }
    }

    public static void Add(int value)
    {
        GameManager.Instance.curentCoin += value;
    }
}
