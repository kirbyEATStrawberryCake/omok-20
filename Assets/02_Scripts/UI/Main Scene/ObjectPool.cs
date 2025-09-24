using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    [SerializeField] private GameObject prefab;
    [SerializeField] private int poolSize;
    [SerializeField] private RectTransform parent;

    private Queue<GameObject> pool;

    private static ObjectPool instance;
    public static ObjectPool Instance => instance;

    private void Awake()
    {
        instance = this;
        pool = new Queue<GameObject>();

        for (int i = 0; i < poolSize; i++)
        {
            CreateNewObject();
        }
    }

    // Object Pool에 새로운 오브젝트를 생성하는 메서드
    private void CreateNewObject()
    {
        GameObject newObject = Instantiate(prefab, parent);
        newObject.SetActive(false);
        pool.Enqueue(newObject);
    }

    // Object Pool에 있는 오브젝트를 전달하는 메서드(꺼내서 씀)
    public GameObject GetObject()
    {
        if (pool.Count == 0) CreateNewObject();

        GameObject dequeueObject = pool.Dequeue();
        dequeueObject.SetActive(true);

        return dequeueObject;
    }

    // 다 사용한 오브젝트를 Object Pool로 반환하는 메서드(다시 넣음)
    public void ReturnObject(GameObject obj)
    {
        obj.SetActive(false);
        pool.Enqueue(obj);
    }
}