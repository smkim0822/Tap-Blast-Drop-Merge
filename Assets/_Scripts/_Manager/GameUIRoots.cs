using UnityEngine;

/// <summary>
/// 메인/인게임/게임오버에 해당하는 씬 UI 루트와 플레이어 시작 배치를 담당합니다.
/// GameManager가 직접 오브젝트 참조를 들고 있지 않도록 씬 연결 지점을 분리합니다.
/// </summary>
[DisallowMultipleComponent]
public class GameUIRoots : MonoBehaviour
{
    [Header("UI Controllers (UI 컨트롤러)")]
    [SerializeField] MainPanel _mainPanel;
    [SerializeField] GameOverPanel _gameOverPanel;

    [Header("Game Objects (게임 오브젝트)")]
    [SerializeField] GameObject _gamePlayGroup;
    [SerializeField] GameObject _gamePlayUI;

    [Header("Player (플레이어)")]
    [SerializeField] GameObject _player;
    [SerializeField] Transform _playerStartPos;

    public void ShowMainMenuLayout()
    {
        if (_mainPanel != null) _mainPanel.gameObject.SetActive(true);
        if (_gameOverPanel != null) _gameOverPanel.gameObject.SetActive(false);
        if (_gamePlayGroup != null) _gamePlayGroup.SetActive(false);
        if (_gamePlayUI != null) _gamePlayUI.SetActive(false);
    }

    public void ShowGameplayLayout()
    {
        if (_mainPanel != null) _mainPanel.gameObject.SetActive(false);
        if (_gameOverPanel != null) _gameOverPanel.gameObject.SetActive(false);
        if (_gamePlayGroup != null) _gamePlayGroup.SetActive(true);
        if (_gamePlayUI != null) _gamePlayUI.SetActive(true);
    }

    public void HideGameplayUI()
    {
        if (_gamePlayUI != null) _gamePlayUI.SetActive(false);
    }

    public void ShowGameOverLayout(int currentScore, int bestScore, bool isNewRecord)
    {
        HideGameplayUI();
        if (_gameOverPanel != null)
        {
            _gameOverPanel.gameObject.SetActive(true);
            _gameOverPanel.Setup(currentScore, bestScore, isNewRecord);
        }
    }

    public void PlacePlayerForNewRun()
    {
        if (_player == null) return;

        _player.SetActive(true);
        if (_playerStartPos != null) _player.transform.position = _playerStartPos.position;

        Spawner spawner = _player.GetComponent<Spawner>();
        if (spawner != null)
        {
            spawner.ResetSpawner();
        }

        SessionEvents.OnGameStart?.Invoke();
    }
}
