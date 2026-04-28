using UnityEngine;

/// <summary>
/// 풀링 시스템 접근을 캡슐화하는 파사드입니다.
/// PoolManager가 없는 상황에서도 최소한 동작하도록 안전한 폴백을 제공합니다.
/// </summary>
public static class PoolService
{
    public static GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        if (prefab == null) return null;

        if (PoolManager.Instance != null)
        {
            return PoolManager.Instance.Spawn(prefab, position, rotation);
        }

        return Object.Instantiate(prefab, position, rotation);
    }

    public static void Despawn(GameObject obj, float delay = 0f)
    {
        if (obj == null) return;

        if (PoolManager.Instance != null)
        {
            PoolManager.Instance.Despawn(obj, delay);
            return;
        }

        // 폴백: 풀 매니저가 없으면 일반 Destroy 처리
        if (delay > 0f)
        {
            Object.Destroy(obj, delay);
        }
        else
        {
            Object.Destroy(obj);
        }
    }
}

