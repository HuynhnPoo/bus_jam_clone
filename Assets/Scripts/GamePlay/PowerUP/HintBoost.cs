using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HintBoost : PowerUpBase
{
    private float blinkDuration = 0.3f;
    private int blinkLoops = 5;
    [SerializeField] private Color hintColor = Color.grey;

    private void Awake()
    {
        BoostType = BoostType.Hint;
    }
    private VehicleController FindMovableVehicle()
    {
        VehicleController[] allVehicles = FindObjectsOfType<VehicleController>();
        List<VehicleController> movableVehicle = new List<VehicleController>();
        foreach (VehicleController vehicle in allVehicles)
        {
            if (vehicle == null || vehicle.IsArrived || vehicle.IsLoading || vehicle.vehicleData == null) continue;

            //kiem tra di chuyen có the di chuyen được không
            bool canMove = GameManager.Instance.gridManager.Board.CheckIfPathIsClear(vehicle.vehicleData);

            if (canMove) movableVehicle.Add(vehicle);

        }
        if (movableVehicle.Count == 0) return null;
        return movableVehicle[Random.Range(0, movableVehicle.Count)];
    }

void BlinkVehicle(VehicleController vehicle)
{
    MeshRenderer[] renderers = vehicle.GetComponentsInChildren<MeshRenderer>();
    if (renderers.Length == 0) return;

    // thuc hien phong to thu nho voi xe
    Vector3 origanScale=vehicle.transform.localScale;
        vehicle.transform.DOKill();
        vehicle.transform.DOScale(origanScale * 1.15f, blinkDuration)
            .SetLoops(blinkLoops*2,LoopType.Yoyo)
            .OnComplete(() =>
            {
                if(vehicle !=null) vehicle.transform.localScale = origanScale;
            });

    foreach (var meshRenderer in renderers)
    {
        Material material = meshRenderer.material;
        
        // Bật Keyword Emission cho Shader nếu chưa bật
        material.EnableKeyword("_EMISSION");

        // Nhấp nháy màu phát sáng (Emission)
        material.DOKill();
        material.DOColor(hintColor, "_EmissionColor", blinkDuration)
                .SetLoops(blinkLoops * 2, LoopType.Yoyo)
                .OnComplete(() =>
                {
                    if (material != null) 
                        material.SetColor("_EmissionColor", Color.black);
                });
    }
}

    protected override void ExecutePowerUp()
    {
        VehicleController vehicle = FindMovableVehicle();

        Debug.Log("hien thi" + vehicle);
        if (vehicle == null)
        {
            Debug.Log("Không tìm thấy xe nào có thể di chuyển!");
            AddQuantity(); // Hoàn lại lượt nếu không có xe nào đi được
            return;
        }

        BlinkVehicle(vehicle);
    }
}
