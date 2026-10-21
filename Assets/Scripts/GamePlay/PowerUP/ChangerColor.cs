using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;

public class ChangerColor : PowerUpBase
{
    private int amout = 1;
    private PersonVisual _personVisual;

    protected override void Awake()
    {
        base.Awake();
        BoostType = BoostType.ColorChange;
    }

    //public void SetPersonVisual(PersonVisual personVisual)
    //{
    //    this._personVisual = personVisual;
    //}

    protected override void ExecutePowerUp()
    {
        Debug.Log("ham any thưc hien dược");
        List<GameObject> firstGroup = GetFirstPersonGroup();
        if (firstGroup.Count == 0) return;

        VehicleState targetVehicle = GetVehiclesStateForColorChange();

        Color newColor =targetVehicle.color;
        int newGroupId = targetVehicle.id;

        Debug.Log($"mau {newColor} {newGroupId}");
        
        foreach (GameObject group in firstGroup) 
        {
            //Debug.Log($"{group.name}");
            if (group == null) continue;
            PersonVisual personVisual = group.GetComponentInChildren<PersonVisual>();

            if (personVisual == null) return;
           //  Debug.Log("danh sach nguowi ddoior teen "+ personVisual.name);
            personVisual.Setup(newColor, newGroupId);
        }
    }
    private VehicleState GetVehiclesStateForColorChange()
    {
        if (GameManager.Instance == null || GameManager.Instance.gridManager == null) return null;

        GridManager gridManager = GameManager.Instance.gridManager;
        if (gridManager.Board == null || gridManager.Board.vehicles == null) return null;

        List<VehicleState> availableVehicles = gridManager.Board.vehicles;
        if (availableVehicles.Count == 0) return null;

        // 1. Lấy ID nhóm người hiện tại ở đầu hàng
        List<GameObject> firstGroup = GetFirstPersonGroup();
        if (firstGroup.Count == 0) return null;

        PersonVisual firstPersonVisual = firstGroup[0].GetComponentInChildren<PersonVisual>();
        if (firstPersonVisual == null) return null;

        int currentGroupId = firstPersonVisual.GroupPesonId;

        // 2. Lọc ra các xe KHÁC màu với nhóm người hiện tại
        // Đồng thời đảm bảo sau khi đổi, vẫn còn xe (hoặc nhóm xe) mang màu đó trên bàn để người dân lên xe
        foreach (VehicleState vehicle in availableVehicles)
        {
            if (vehicle == null) continue;

            // Bỏ qua nếu xe này trùng màu với nhóm người hiện tại
            if (vehicle.id == currentGroupId) continue;

            // Kiểm tra xem trên board có thực sự tồn tại xe này hoặc còn xe để đi không
            // (Bạn có thể thêm điều kiện đếm số lượng xe cùng loại nếu cần thiết)
            // Ở đây chúng ta ưu tiên chọn chiếc xe khác màu đầu tiên tìm thấy
            return vehicle;
        }

        // Nếu không tìm thấy xe nào khác màu hợp lệ, fallback về xe đầu tiên
        return availableVehicles.FirstOrDefault();
    }

    public List<GameObject> GetFirstPersonGroup()
    {
        List<GameObject> group = new List<GameObject>();
        List<GameObject> line = GameManager.Instance.LinePperson;

        PersonVisual firstPersonVisual = line[0].GetComponentInChildren<PersonVisual>();
        if (firstPersonVisual == null) return group;

        int targetGroupId = firstPersonVisual.GroupPesonId;
        foreach (GameObject person in line)
        {
            if (person == null) break;

           PersonVisual personVisual = person.GetComponentInChildren<PersonVisual>();
            if (personVisual != null && personVisual.GroupPesonId == targetGroupId)
            {
                group.Add(person);
            }
            else
            {
                break;
            }
        }
        return group;
    }

}
