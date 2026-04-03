using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PoolManager : MonoBehaviour
{
    // 게임 내 모든 오브젝트들의 생성과 파괴를 관리하는 풀링 매니저
    public static PoolManager Instance;

    // 프리팹 이름을 키로 사용하여 대기열 관리
    private Dictionary<string, Queue<GameObject>> pool = new Dictionary<string, Queue<GameObject>>();
    
    void Awake()
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

    // 생성 메서드
    public GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        string key = prefab.name;
        if (!pool.ContainsKey(key))
        {
            pool[key] = new Queue<GameObject>();
        }

        GameObject obj;

        if (pool[key].Count > 0)
        {
            obj = pool[key].Dequeue();
            obj.transform.position = position;
            obj.transform.rotation = rotation;
            obj.SetActive(true);
        }
        else
        {
            obj = Instantiate(prefab, position, rotation);
            obj.name = prefab.name;
        }
        return obj;
    }

    // 반환 메서드
    public void Despawn(GameObject obj, float delay = 0f)
    {
        if (delay > 0)
        {
            StartCoroutine(DespawnRoutine(obj, delay));
        }
        else
        {
            DespawnImmediate(obj);
        }
    }

    private IEnumerator DespawnRoutine(GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);
        DespawnImmediate(obj);
    }

    private void DespawnImmediate(GameObject obj)
    {
        if (!obj.activeSelf)
        {
            return;
        }

        obj.SetActive(false);
        if (!pool.ContainsKey(obj.name))
        {
            pool[obj.name] = new Queue<GameObject>();
        }
        pool[obj.name].Enqueue(obj);
    }
}
