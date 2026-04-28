using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PoolManager : MonoBehaviour
{
    // 게임 내 모든 오브젝트들의 생성과 파괴를 관리하는 풀링 매니저
    public static PoolManager Instance;

    [Header("Pool Data (풀링 데이터)")]
    [Tooltip("프리팹 이름을 키로 사용하여 비활성화된 오브젝트 대기열을 관리")]
    private Dictionary<string, Queue<GameObject>> pool = new Dictionary<string, Queue<GameObject>>();

    [Header("Limits (안정성 설정)")]
    [Tooltip("프리팹별 풀 최대 보관 개수 (초과분은 파괴)")]
    [SerializeField] private int maxPooledPerPrefab = 128;
    
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

    /// <summary>
    /// 오브젝트 풀에서 꺼내 활성화
    /// 풀이 비어있다면 새로 생성하여 반환
    /// </summary>
    /// <param name="prefab"></param>
    /// <param name="position"></param>
    /// <param name="rotation"></param>
    /// <returns></returns>
    public GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        if (prefab == null) return null;

        string key = prefab.name;
        if (!pool.ContainsKey(key))
        {
            pool[key] = new Queue<GameObject>();
        }

        GameObject obj;

        // null 엔트리 정리
        while (pool[key].Count > 0 && pool[key].Peek() == null)
        {
            pool[key].Dequeue();
        }

        if (pool[key].Count > 0)
        {
            obj = pool[key].Dequeue();
            obj.transform.SetParent(null);
            obj.transform.position = position;
            obj.transform.rotation = rotation;
            obj.SetActive(true);
        }
        else
        {
            // 풀이 비어있을 경우 새로 생성
            obj = Instantiate(prefab, position, rotation);
            obj.name = prefab.name;
        }
        return obj;
    }

    /// <summary>
    /// 오브젝트를 비활성화하고 풀로 반환
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="delay"></param>
    public void Despawn(GameObject obj, float delay = 0f)
    {
        if (obj == null) return;

        if (delay > 0)
        {
            StartCoroutine(DespawnRoutine(obj, delay));
        }
        else
        {
            DespawnImmediate(obj);
        }
    }

    /// <summary>
    /// 지정된 지연 시간 후 오브젝트를 반환하는 코루틴
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="delay"></param>
    /// <returns></returns>
    private IEnumerator DespawnRoutine(GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);
        DespawnImmediate(obj);
    }

    /// <summary>
    /// 오브젝트를 즉시 비활성화하고 풀에 보관
    /// </summary>
    /// <param name="obj"></param>
    private void DespawnImmediate(GameObject obj)
    {
        if (obj == null || !obj.activeSelf)
        {
            return;
        }

        obj.SetActive(false);

        obj.transform.SetParent(transform);
        
        if (!pool.ContainsKey(obj.name))
        {
            pool[obj.name] = new Queue<GameObject>();
        }

        // 풀 과성장 방지
        if (maxPooledPerPrefab > 0 && pool[obj.name].Count >= maxPooledPerPrefab)
        {
            Destroy(obj);
            return;
        }

        pool[obj.name].Enqueue(obj);
    }

    /// <summary>
    /// 특정 프리팹을 지정 개수만큼 미리 풀에 채워 넣습니다.
    /// </summary>
    public void Prewarm(GameObject prefab, int count)
    {
        if (prefab == null || count <= 0) return;

        string key = prefab.name;
        if (!pool.ContainsKey(key))
        {
            pool[key] = new Queue<GameObject>();
        }

        int target = maxPooledPerPrefab > 0 ? Mathf.Min(count, maxPooledPerPrefab) : count;
        for (int i = 0; i < target; i++)
        {
            var obj = Instantiate(prefab, Vector3.zero, Quaternion.identity);
            obj.name = prefab.name;
            obj.SetActive(false);
            obj.transform.SetParent(transform);
            pool[key].Enqueue(obj);
        }
    }
}
