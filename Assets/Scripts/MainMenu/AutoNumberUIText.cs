using UnityEngine;
using TMPro; // Nếu dùng TextMeshPro
using UnityEngine.UI; // Nếu dùng Text UI truyền thống

public class AutoNumberUIText : MonoBehaviour
{
    [ContextMenu("Update Numbers")]
    public void UpdateChildNumbers()
    {
        int childCount = transform.childCount;

        for (int i = 0; i < childCount; i++)
        {
            Transform child = transform.GetChild(i);
            int numberToShow = i + 1; // Đánh số từ 1 đến childCount

            // 1. Thử tìm và cập nhật TextMeshPro (TMP)
            TextMeshProUGUI tmpText = child.GetComponentInChildren<TextMeshProUGUI>();
            if (tmpText != null)
            {
                tmpText.text = numberToShow.ToString();
                continue;
            }

            // 2. Nếu không có TMP, tìm UI Text thường
            Text legacyText = child.GetComponentInChildren<Text>();
            if (legacyText != null)
            {
                legacyText.text = numberToShow.ToString();
            }
        }
    }

    private void Awake()
    {
        // Tự động chạy khi game bắt đầu
        UpdateChildNumbers();
    }
}