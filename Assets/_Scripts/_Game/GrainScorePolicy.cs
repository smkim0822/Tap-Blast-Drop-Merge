using UnityEngine;

/// <summary>
/// 알갱이 파괴 시 점수 계산 규칙을 캡슐화합니다.
/// </summary>
public static class GrainScorePolicy
{
    public static int Calculate(int batchId, float scaleX, float scoreMultiplier, float scoreGrowthRate)
    {
        float baseScore = scaleX * scoreMultiplier;
        float growthFactor = 1.0f + (Mathf.Max(0, batchId) * scoreGrowthRate);
        return Mathf.Max(1, Mathf.RoundToInt(baseScore * growthFactor));
    }
}
