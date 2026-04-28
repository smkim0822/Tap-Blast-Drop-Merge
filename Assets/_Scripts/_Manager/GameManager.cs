using UnityEngine;

/// <summary>
/// 게임의 진입점 싱글톤. 씬 참조는 <see cref="GameUIRoots"/>로 분리하고,
/// 실제 흐름은 <see cref="GameSessionFlow"/>에 위임합니다.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    GameUIRoots _uiRoots;
    GameSessionFlow _session;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        _uiRoots = GetComponent<GameUIRoots>();
        if (_uiRoots == null)
        {
            _uiRoots = gameObject.AddComponent<GameUIRoots>();
        }
        _session = new GameSessionFlow(this, _uiRoots);
    }

    void OnEnable()
    {
        if (_session != null)
        {
            _session.SubscribeToGameOverEvent();
            _session.SubscribeToUiRequests();
        }
    }

    void OnDisable()
    {
        if (_session != null)
        {
            _session.UnsubscribeFromGameOverEvent();
            _session.UnsubscribeFromUiRequests();
        }
    }

    void Start()
    {
        if (_session != null)
            _session.Bootstrap();
    }

    public void EnterMainState() => _session.EnterMainState();
    public void EnterPlayState() => _session.EnterPlayState();
    public void ResetData() => _session.ResetData();
    public void RetryGame() => _session.RetryGame();
    public void GoToMain() => _session.GoToMain();
}
