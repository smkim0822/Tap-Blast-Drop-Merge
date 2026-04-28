/// <summary>
/// 한 판 동안 폭탄 폭발 순서를 추적하는 시퀀스 카운터입니다.
/// BombLogic이 직접 정적 필드를 노출하지 않도록 분리합니다.
/// </summary>
public static class GameplayBatchSequence
{
    static int nextBatchId = 0;

    public static int Next()
    {
        return nextBatchId++;
    }

    public static void Reset()
    {
        nextBatchId = 0;
    }
}
