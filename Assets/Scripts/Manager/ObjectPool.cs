using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    // Dictionary lưu prefab mẫu theo key để tự sinh thêm khi pool hết object
    private Dictionary<string, GameObject> prefabLookup = new Dictionary<string, GameObject>();

    // Dictionary lưu danh sách object nhàn rỗi theo key
    private Dictionary<string, Queue<GameObject>> pool = new Dictionary<string, Queue<GameObject>>();


    // Khởi tạo Pool
    public void CreatePool(string key, GameObject prefab, int amount = 20, Transform holder = null)
    {
        if (pool.ContainsKey(key))
        {
            Debug.LogWarning($"Pool với key '{key}' đã tồn tại!");
            return;
        }

        prefabLookup[key] = prefab;
        Queue<GameObject> objectQueue = new Queue<GameObject>();

        for (int i = 0; i < amount; i++)
        {
            GameObject obj = CreateNewObject(prefab);
            obj.transform.SetParent(holder, false);
            objectQueue.Enqueue(obj);
        }

        pool.Add(key, objectQueue);
    }

    // Helper method tạo mới object và đặt về trạng thái disable
    private GameObject CreateNewObject(GameObject prefab)
    {
        GameObject obj = Instantiate(prefab, transform); // Đặt parent là ObjectPool cho gọn Hierarchy
        obj.SetActive(false);
        return obj;
    }

    // Lấy Object từ Pool
    public GameObject Get(string key)
    {
        if (!pool.ContainsKey(key))
        {
            Debug.LogError($"Key '{key}' không tồn tại trong ObjectPool!");
            return null;
        }

        // Nếu hết object trong pool, tự động Instantiate thêm
        if (pool[key].Count == 0)
        {
            GameObject newObj = CreateNewObject(prefabLookup[key]);
            pool[key].Enqueue(newObj);
        }

        GameObject obj = pool[key].Dequeue();
        obj.SetActive(true);
        return obj;
    }

    // Trả Object về Pool
    public void ReturnToPool(string key, GameObject obj)
    {
        if (!pool.ContainsKey(key))
        {
            Debug.LogWarning($"Key '{key}' không tồn tại trong ObjectPool để trả về!");
            Destroy(obj);
            return;
        }

        obj.SetActive(false);
        obj.transform.SetParent(transform); // Reset về làm con của ObjectPool
        pool[key].Enqueue(obj);
    }

    // Lấy số lượng object khả dụng trong Pool
    public int GetAvailableCount(string key)
    {
        if (!pool.ContainsKey(key)) return 0;
        return pool[key].Count;
    }
}