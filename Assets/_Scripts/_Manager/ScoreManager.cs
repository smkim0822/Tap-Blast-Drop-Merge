using TMPro;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;

    private ScoreState scoreState;
    ScoreHudView scoreHudView;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        scoreState = new ScoreState(BestScoreStore.Load());
        scoreHudView = GetComponent<ScoreHudView>();
        if (scoreHudView == null)
        {
            scoreHudView = gameObject.AddComponent<ScoreHudView>();
        }
    }

    /// <summary>
    /// 오브젝트 활성화 시 점수 획득 이벤트 구독
    /// </summary>
    private void OnEnable()
    {
        ScoreEvents.OnScoreAdded += AddScore;
    }

    /// <summary>
    /// 오브젝트 비활성화 시 점수 획득 이벤트 해제
    /// </summary>
    private void OnDisable()
    {
        ScoreEvents.OnScoreAdded -= AddScore;
    }

    void Start()
    {
        RenderScore();
    }

    /// <summary>
    /// 점수를 획득하고 UI 및 애니메이션 업데이트
    /// 신기록 달성 시 최고 점수 UI도 함께 업데이트 및 애니메이션 처리
    /// </summary>
    /// <param name="amount"></param>
    public void AddScore(int amount)
    {
        bool isNewRecord = scoreState.AddScore(amount);
        if (isNewRecord)
        {
            BestScoreStore.Save(scoreState.BestScore);
        }

        RenderScore();
        if (scoreHudView != null)
        {
            scoreHudView.PlayScoreAdded(isNewRecord);
        }
    }

    /// <summary>
    /// 점수 초기화 (재시작 시 호출됨)
    /// </summary>
    public void ResetScore()
    {
        scoreState.ResetCurrentScore();

        if (scoreHudView != null)
        {
            scoreHudView.ResetVisualState();
        }

        RenderScore();
    }

    /// <summary>
    /// 서버 데이터 동기화 등으로 인해 외부에서 최고 점수를 다시 불러와야 할 때 호출
    /// </summary>
    public void ReloadBestScore()
    {
        SyncBestScore(BestScoreStore.Load());
    }

    public void SyncBestScore(int bestScore)
    {
        if (!scoreState.SyncBestScoreIfHigher(bestScore))
        {
            RenderScore();
            return;
        }

        BestScoreStore.Save(scoreState.BestScore);
        RenderScore();
    }

    /// <summary>
    /// 현재 점수와 최고 점수 UI의 텍스트 갱신
    /// </summary>
    void RenderScore()
    {
        if (scoreHudView != null)
        {
            scoreHudView.Render(scoreState.CurrentScore, scoreState.BestScore);
        }
    }

    /// <summary>
    /// 외부에서 현재 점수를 가져갈 때 사용
    /// </summary>
    /// <returns></returns>
    public int GetCurrentScore() => scoreState.CurrentScore;

    public int GetBestScore() => scoreState.BestScore;
}