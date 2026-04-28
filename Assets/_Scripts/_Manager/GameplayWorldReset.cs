using UnityEngine;

/// <summary>
/// 재시작·메인 이동 시 풀에 올라간 엔티티(스택, 폭탄, 이펙트)와 점수 관련 전역을 초기화합니다.
/// </summary>
public static class GameplayWorldReset
{
    public static void ResetPooledContentAndScoring()
    {
        GameplayBatchSequence.Reset();

        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.ResetScore();
        }

        GameObject[] stacks = GameObject.FindGameObjectsWithTag("Stack");
        if (stacks != null)
        {
            foreach (GameObject obj in stacks)
            {
                if (obj.GetComponent<GrainLogic>() != null)
                {
                    PoolService.Despawn(obj);
                }
            }
        }

        BombLogic[] fallingBombs = Object.FindObjectsByType<BombLogic>(FindObjectsSortMode.None);
        foreach (BombLogic bomb in fallingBombs)
        {
            PoolService.Despawn(bomb.gameObject);
        }

        GameObject[] effects = GameObject.FindGameObjectsWithTag("Effect");
        if (effects != null)
        {
            foreach (GameObject effect in effects)
            {
                PoolService.Despawn(effect);
            }
        }
    }
}
