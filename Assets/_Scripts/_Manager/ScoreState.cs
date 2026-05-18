/// <summary>
/// 현재 점수와 최고 점수 상태를 관리하는 순수 모델입니다.
/// UI와 저장소는 외부에서 연결합니다.
/// </summary>
public class ScoreState
{
    public int CurrentScore { get; private set; }
    public int BestScore { get; private set; }

    public ScoreState(int bestScore)
    {
        BestScore = bestScore;
    }

    public bool AddScore(int amount)
    {
        CurrentScore += amount;

        if (CurrentScore <= BestScore)
        {
            return false;
        }

        BestScore = CurrentScore;
        return true;
    }

    public void ResetCurrentScore()
    {
        CurrentScore = 0;
    }

    public bool SyncBestScoreIfHigher(int bestScore)
    {
        if (bestScore <= BestScore)
        {
            return false;
        }

        BestScore = bestScore;
        return true;
    }
}
