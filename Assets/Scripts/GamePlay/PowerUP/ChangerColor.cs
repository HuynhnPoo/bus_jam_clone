using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ChangerColor : PowerUpBase
{
    protected override void Awake()
    {
        base.Awake();
        BoostType = BoostType.ColorChange;
    }

    protected override void ExecutePowerUp()
    {
        Debug.Log("Thực hiện đổi sang màu khác cho các nhóm trong hàng chờ.");

        if (GameManager.Instance == null || GameManager.Instance.gridManager == null) return;

        GridManager gridManager = GameManager.Instance.gridManager;
        if (gridManager.Board == null || gridManager.Board.vehicles == null) return;

        List<VehicleState> availableVehicles = gridManager.Board.vehicles;
        if (availableVehicles.Count == 0) return;

        // 1. Lấy tất cả các nhóm người đang xếp hàng theo từng block liên tiếp
        List<List<GameObject>> allGroups = GetAllPersonGroups();
        if (allGroups.Count == 0) return;

        // 2. Duyệt qua từng nhóm và đổi sang màu KHÁC với màu hiện tại của chúng (dùng offset dịch chuyển)
        for (int i = 0; i < allGroups.Count; i++)
        {
            if (allGroups[i].Count == 0) continue;

            // Lấy ID màu hiện tại của nhóm này
            PersonVisual firstPersonInGroup = allGroups[i][0].GetComponentInChildren<PersonVisual>();
            int currentGroupId = firstPersonInGroup != null ? firstPersonInGroup.GroupPesonId : -1;

            // Dùng phép dịch chuyển (i + 1) để đảm bảo nhóm không bị giữ nguyên màu cũ (nếu có từ 2 xe trở lên)
            int candidateIndex = (i + 1) % availableVehicles.Count;
            VehicleState targetVehicle = availableVehicles[candidateIndex];

            // Nếu xe được chọn vẫn trùng màu cũ, dịch tiếp sang xe kế tiếp
            if (availableVehicles.Count > 1 && targetVehicle.id == currentGroupId)
            {
                candidateIndex = (candidateIndex + 1) % availableVehicles.Count;
                targetVehicle = availableVehicles[candidateIndex];
            }

            if (targetVehicle == null) targetVehicle = availableVehicles[0];

            Color newColor = targetVehicle.color;
            int newGroupId = targetVehicle.id;

            // Cập nhật màu và ID mới cho toàn bộ người trong nhóm này
            foreach (GameObject person in allGroups[i])
            {
                if (person == null) continue;
                PersonVisual personVisual = person.GetComponentInChildren<PersonVisual>();
                if (personVisual != null)
                {
                    personVisual.Setup(newColor, newGroupId);
                }
            }
        }

        Debug.Log("Đã đổi màu các nhóm thành công!");
    }

    /// <summary>
    /// Phân tách toàn bộ hàng chờ LinePperson thành các danh sách con (các nhóm người đứng liên tiếp có cùng GroupPesonId)
    /// </summary>
    public List<List<GameObject>> GetAllPersonGroups()
    {
        List<List<GameObject>> allGroups = new List<List<GameObject>>();
        List<GameObject> line = GameManager.Instance.LinePperson;

        if (line == null || line.Count == 0) return allGroups;

        List<GameObject> currentGroup = new List<GameObject>();
        int currentId = -1;

        foreach (GameObject person in line)
        {
            if (person == null) continue;

            PersonVisual personVisual = person.GetComponentInChildren<PersonVisual>();
            if (personVisual == null) continue;

            if (currentGroup.Count == 0)
            {
                currentId = personVisual.GroupPesonId;
                currentGroup.Add(person);
            }
            else
            {
                // Nếu cùng ID nhóm thì gộp chung vào 1 nhóm
                if (personVisual.GroupPesonId == currentId)
                {
                    currentGroup.Add(person);
                }
                else
                {
                    // Khác ID thì lưu nhóm cũ lại và bắt đầu nhóm mới
                    allGroups.Add(new List<GameObject>(currentGroup));
                    currentGroup.Clear();
                    currentId = personVisual.GroupPesonId;
                    currentGroup.Add(person);
                }
            }
        }

        // Thêm nhóm cuối cùng vào danh sách
        if (currentGroup.Count > 0)
        {
            allGroups.Add(currentGroup);
        }

        return allGroups;
    }
}