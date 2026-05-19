using UnityEngine;

/// <summary>
/// Android 뒤로가기·Escape 입력을 UI 계층에 맞게 처리합니다.
/// 팝업 닫기 → 게임오버 시 메인 → 인게임 시 설정 → 메인에서 두 번 누르면 종료.
/// </summary>
[DisallowMultipleComponent]
public class BackNavigationHandler : MonoBehaviour
{
    [SerializeField] GameUIRoots uiRoots;
    [Tooltip("메인 화면에서 종료 확인으로 인정할 시간(초)")]
    [SerializeField] float exitConfirmSeconds = 2f;

    float _lastBackPressTime = -10f;

    void Awake()
    {
        if (uiRoots == null)
        {
            uiRoots = GetComponent<GameUIRoots>();
        }
    }

    void Update()
    {
        if (!Input.GetKeyDown(KeyCode.Escape))
        {
            return;
        }

        if (uiRoots != null && uiRoots.TryHandleBackNavigation())
        {
            _lastBackPressTime = -10f;
            return;
        }

        if (uiRoots == null || !uiRoots.IsMainMenuVisible())
        {
            return;
        }

        float now = Time.unscaledTime;
        if (now - _lastBackPressTime <= exitConfirmSeconds)
        {
            QuitApplication();
            return;
        }

        _lastBackPressTime = now;
        Debug.Log("[Back] 한 번 더 누르면 앱이 종료됩니다.");
    }

    static void QuitApplication()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
