using System.Collections.Generic;
using UnityEngine;

public class ProjectilePool : MonoBehaviour
{
    public static ProjectilePool Instance { get; private set; }

    private Dictionary<string, Queue<GameObject>> poolDictionary = new Dictionary<string, Queue<GameObject>>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public GameObject SpawnFromPool(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        string poolKey = prefab.name;

        // Nếu loại đạn này chưa có Pool, tạo Pool mới
        if (!poolDictionary.ContainsKey(poolKey))
        {
            poolDictionary.Add(poolKey, new Queue<GameObject>());
        }

        // Nếu trong Pool có sẵn đạn rảnh rỗi
        if (poolDictionary[poolKey].Count > 0)
        {
            GameObject objectToSpawn = poolDictionary[poolKey].Dequeue();
            objectToSpawn.transform.position = position;
            objectToSpawn.transform.rotation = rotation;
            objectToSpawn.transform.SetParent(null);
            objectToSpawn.SetActive(true);
            return objectToSpawn;
        }
        else
        {
            // Nếu Pool trống, đẻ thêm đạn mới
            GameObject newObj = Instantiate(prefab, position, rotation);
            newObj.name = prefab.name; // Giữ nguyên tên để trả về đúng Pool
            return newObj;
        }
    }

    public void ReturnToPool(GameObject obj)
    {
        obj.SetActive(false);
        obj.transform.SetParent(this.transform); // Gom gọn về Pool cho sạch Hierarchy
        
        string poolKey = obj.name;

        if (!poolDictionary.ContainsKey(poolKey))
        {
            poolDictionary.Add(poolKey, new Queue<GameObject>());
        }

        poolDictionary[poolKey].Enqueue(obj);
    }
}
