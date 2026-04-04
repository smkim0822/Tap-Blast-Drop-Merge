using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System;

/// <summary>
/// 랭킹 팝업 창을 제어하는 메인 스크립트.
/// 서버 동기화, 오프라인 모드 대응, 닉네임 변경 및 가상 키보드 대응 UX 로직 포함.
/// </summary>
public class RankingPopup : MonoBehaviour
{
    [Header("UI References (기본 UI 연결)")]
    [Tooltip("랭킹 아이템들이 생성되어 배치될 부모 객체")]
    public Transform contentParent;
    [Tooltip("한 줄에 표시될 랭킹 아이템 프리팹")]
    public RankingItem itemPrefab;
    [Tooltip("인터넷 연결이 없을 때 띄워줄 오프라인 안내 메시지 객체")]
    public GameObject offlineMessageObject;

    [Header("My Rank References (내 순위 UI)")]
    [Tooltip("하단에 고정될 내 닉네임 텍스트")]
    public TextMeshProUGUI myNameText;
    [Tooltip("하단에 고정될 내 등수 텍스트")]
    public TextMeshProUGUI myRankText;
    [Tooltip("하단에 고정될 내 점수 텍스트")]
    public TextMeshProUGUI myScoreText;

    [Header("Nickname Edit UI (닉네임 수정 창 UI)")]
    [Tooltip("닉네임 수정 버튼")]
    public Button editNameButton;
    [Tooltip("닉네임 수정 팝업의 최상위 부모 객체")]
    public GameObject editPopupRoot;
    [Tooltip("스케일 애니메이션이 적용될 실제 수정 팝업창 객체")]
    public Transform editPopupContent;
    [Tooltip("닉네임을 입력받을 Input Field")]
    public TMP_InputField nameInput;
    [Tooltip("닉네임 변경을 확정하는 저장 버튼")]
    public Button saveNameButton;
    [Tooltip("닉네임 변경을 취소하고 창을 닫는 버튼")]
    public Button cancelNameButton;
    [Tooltip("닉네임 수정 팝업이 열리고 닫힐 때의 애니메이션 속도")]
    public float editPopupAnimDuration = 0.2f;

    [Header("Animation Settings (랭킹 팝업 애니메이션)")]
    [Tooltip("애니메이션이 적용될 랭킹 팝업의 전체 창 RectTransform")]
    [SerializeField] private RectTransform popupWindow;
    [Tooltip("팝업이 열릴 때의 스케일 변화 그래프")]
    [SerializeField] private AnimationCurve openCurve;
    [Tooltip("랭킹 팝업 애니메이션의 진행 시간")]
    [SerializeField] private float animDuration = 0.2f;

    [Header("Keyboard UX Settings (모바일 키보드 대응)")]
    [Tooltip("모바일 가상 키보드가 켜졌을 때 입력창이 가려지지 않도록 위로 올려줄 Y축 이동량")]
    public float keyboardOffsetY = 400f;
    [Tooltip("키보드 대응으로 팝업이 위아래로 이동할 때의 애니메이션 속도")]
    public float moveAnimDuration = 0.2f;

    private bool isAnimating = false;
    private bool isEditPopupClosing = false;
    private RectTransform editPopupRect;
    private Coroutine moveCoroutine;
    private Coroutine editAnimCoroutine;
    private Coroutine syncRoutine;

    private void Start()
    {
        // 버튼 및 입력창 이벤트 리스너 연결
        if (editNameButton != null)
        {
            editNameButton.onClick.AddListener(OpenEditNamePopup);
        }

        if (saveNameButton != null)
        {
            saveNameButton.onClick.AddListener(SaveNewNickname);
        }

        if (cancelNameButton != null)
        {
            cancelNameButton.onClick.AddListener(CloseEditNamePopup);
        }

        if (editPopupContent != null)
        {
            editPopupRect = editPopupContent.GetComponent<RectTransform>();
        }

        if (nameInput != null)
        {
            nameInput.onFocusSelectAll = false;
            nameInput.onSelect.AddListener(OnInputSelect);
            nameInput.onDeselect.AddListener(OnInputDeselect);
        }

        // 수정 팝업은 처음에 꺼둠
        if (editPopupRoot != null)
        {
            editPopupRoot.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        if (editNameButton != null)
        {
            editNameButton.onClick.RemoveListener(OpenEditNamePopup);
        }
        if (saveNameButton != null)
        {
            saveNameButton.onClick.RemoveListener(SaveNewNickname);
        }
        if (cancelNameButton != null)
        {
            cancelNameButton.onClick.RemoveListener(CloseEditNamePopup);
        }

        if (nameInput != null)
        {
            nameInput.onSelect.RemoveListener(OnInputSelect);
            nameInput.onDeselect.RemoveListener(OnInputDeselect);
        }
    }

    private void OnEnable()
    {
        // UI 강제 초기화
        if (saveNameButton != null)
        {
            saveNameButton.interactable = true;
        }

        if (editPopupRoot != null)
        {
            editPopupRoot.SetActive(false);
        }
        isEditPopupClosing = false;
    }

    /// <summary>
    /// 랭킹 팝업을 열 때 호출.
    /// 서버 상태 확인, 팝업 애니메이션 실행.
    /// </summary>
    public void OpenPopup()
    {
        if (isAnimating) return;

        gameObject.SetActive(true);

        if (RankingManager.Instance != null)
        {
            StartCoroutine(RankingManager.Instance.CheckServerStatus());
        }
        
        RefreshRanking();
        StartCoroutine(AnimateScale(popupWindow, Vector3.zero, Vector3.one, animDuration, openCurve, () => isAnimating = false));
    }

    /// <summary>
    /// 랭킹 팝업을 닫을 때 호출.
    /// 팝업 애니메이션 실행, 객체 비활성화.
    /// </summary>
    public void ClosePopup()
    {
        if (isAnimating) return;

        StartCoroutine(AnimateScale(popupWindow, popupWindow.localScale, Vector3.zero, animDuration, null, () => gameObject.SetActive(false)));
    }

    // 오프라인 상태일 때 호출되어 로컬 데이터만으로 UI를 구성하는 함수
    private void TriggerOfflineFallback()
    {
        ClearRankingItems();
        if (offlineMessageObject != null)
        {
            offlineMessageObject.SetActive(true);
        }
        
        // 로컬 데이터를 UI에 강제 렌더링
        string currentName = NicknameManager.Instance?.GetNickname() ?? PlayerPrefs.GetString("PlayerName", "Guest");
        int localHighScore = PlayerPrefs.GetInt("BestScore", 0);
        UpdateMyRankUI(null, $"{currentName} <size=70%><color=#FF7B7B>(Offline)</color></size>", localHighScore);
    }

    /// <summary>
    /// 랭킹 데이터를 서버에서 다시 불러와 UI 갱신.
    /// 네트워크가 끊겨있다면 오프라인 모드로 자동 전환.
    /// </summary>
    public void RefreshRanking()
    {
        if (syncRoutine != null)
        {
            StopCoroutine(syncRoutine);
        }

        if (RankingManager.Instance == null || !RankingManager.Instance.IsNetworkReached())
        {
            TriggerOfflineFallback();
            return;
        }

        if (offlineMessageObject != null)
        {
            offlineMessageObject.SetActive(false);
        }
        syncRoutine = StartCoroutine(SyncAndLoadRankingRoutine());
    }

    // 로컬 데이터를 서버에 동기화한 뒤 랭킹을 로드하는 코루틴
    private IEnumerator SyncAndLoadRankingRoutine()
    {
        MyRankData serverMyData = null;
        bool isMyRankLoaded = false;
        bool isNetworkError = false;

        // 서버에서 데이터 가져오기
        StartCoroutine(RankingManager.Instance.GetMyRank((success, data) =>
        {
            if (!success)
            {
                isNetworkError = true;
            }
            serverMyData = data;
            isMyRankLoaded = true;
        }));

        yield return new WaitUntil(() => isMyRankLoaded);

        if (isNetworkError)
        {
            TriggerOfflineFallback();
            yield break;
        }

        // 로컬에 저장된 정보 가져오기
        string localName = NicknameManager.Instance?.GetNickname() ?? PlayerPrefs.GetString("PlayerName", "Guest");
        int localHighScore = PlayerPrefs.GetInt("BestScore", 0);

        bool needsNicknameSync = serverMyData == null || localName != serverMyData.name;
        bool needsScoreSync = serverMyData == null ? (localHighScore > 0) : (localHighScore > serverMyData.score);

        if (needsNicknameSync || needsScoreSync)
        {
            if (needsNicknameSync)
            {
                yield return StartCoroutine(RankingManager.Instance.UpdateNickname(localName));
            }
            if (needsScoreSync)
            {
                yield return StartCoroutine(RankingManager.Instance.PostScore(localHighScore));
            }

            isMyRankLoaded = false;
            StartCoroutine(RankingManager.Instance.GetMyRank((success, data) =>
            {
                serverMyData = data;
                isMyRankLoaded = true;
            }));
            yield return new WaitUntil(() => isMyRankLoaded);
        }

        // 내 순위 UI 업데이트
        UpdateMyRankUI(serverMyData, localName, localHighScore);

        // 전체 랭킹 리스트 로드
        StartCoroutine(RankingManager.Instance.GetRanks((success, ranks) =>
        {
            if (success)
            {
                OnDataReceived(ranks);
            }
            else
            {
                TriggerOfflineFallback();
            }
        }));
    }

    private void UpdateMyRankUI(MyRankData myData, string displayName, int localBest = -1)
    {
        if (localBest == -1)
        {
            localBest = PlayerPrefs.GetInt("BestScore", 0);
        }

        if (myNameText != null)
        {
            string finalName = (myData != null && !string.IsNullOrEmpty(myData.name)) ? myData.name : displayName;
            myNameText.text = finalName;
        }

        if (myRankText != null)
        {
            myRankText.text = (myData != null && myData.rank > 0) ? myData.rank.ToString() : "-";
        }

        int displayScore = myData != null ? Mathf.Max(myData.score, localBest) : localBest;
        if (myScoreText != null)
        {
            myScoreText.text = displayScore.ToString("N0");
        }
    
    }

    private void ClearRankingItems()
    {
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }
    }

    void OnDataReceived(ScoreData[] ranks)
    {
        ClearRankingItems();
        
        if (ranks == null || ranks.Length == 0)
        {
            return;
        }

        for (int i = 0; i < ranks.Length; i++)
        {
            RankingItem newItem = Instantiate(itemPrefab, contentParent);
            newItem.SetData(i + 1, ranks[i].name, ranks[i].score);
        }
    }

    // --- 닉네임 수정 로직 ---

    private void OpenEditNamePopup()
    {
        if (editPopupRoot == null)
        {
            return;
        }
        editPopupRoot.SetActive(true);
        isEditPopupClosing = false;

        if (editPopupRect != null)
        {
            editPopupRect.anchoredPosition = Vector2.zero;
        }
        if (nameInput != null)
        {
            nameInput.text = NicknameManager.Instance?.GetNickname();
        }

        if (editAnimCoroutine != null)
        {
            StopCoroutine(editAnimCoroutine);
        }

        Vector3 startScale = editPopupContent.localScale;
        if (startScale == Vector3.one)
        {
            startScale = Vector3.zero;
        }

        editAnimCoroutine = StartCoroutine(AnimateScale(editPopupContent.GetComponent<RectTransform>(), Vector3.zero, Vector3.one, editPopupAnimDuration));
    }

    public void CloseEditNamePopup()
    {
        // 이미 꺼져있으면 무시
        if (editPopupRoot == null || !editPopupRoot.activeSelf || isEditPopupClosing)
        {
            return;
        }

        isEditPopupClosing = true;
        // 애니메이션
        if (editAnimCoroutine != null)
        {
            StopCoroutine(editAnimCoroutine);
        }

        editAnimCoroutine = StartCoroutine(AnimateScale(editPopupContent.GetComponent<RectTransform>(), editPopupContent.localScale, Vector3.zero, editPopupAnimDuration, null, () =>
        {
            editPopupRoot.SetActive(false);
            isEditPopupClosing = false;
        }));     
    }

    private void SaveNewNickname()
    {
        if (nameInput == null || (saveNameButton != null && !saveNameButton.interactable)) 
        {
            return;
        }

        string newName = nameInput.text;

        // 유효성 검사 (글자 수 제한)
        if (string.IsNullOrWhiteSpace(newName) || newName.Length > 10)
        {
            return;
        }

        // 버튼 쿨타임 코루틴 시작
        StartCoroutine(ButtonCooldownRoutine());

        // 매니저를 통해 저장 (매니저 내부에서 유효성 검사 수행함)
        if (NicknameManager.Instance != null && NicknameManager.Instance.SetNickname(newName))
        {
            CloseEditNamePopup();
            StartCoroutine(SyncNicknameAndRefresh(newName));
        }
    }

    // 닉네임 변경 쿨타임 코루틴
    private IEnumerator ButtonCooldownRoutine()
    {
        if (saveNameButton == null)
        {
            yield break;
        }
        saveNameButton.interactable = false;            // 즉시 비활성화
        yield return new WaitForSecondsRealtime(3f);    // 3초 대기
        saveNameButton.interactable = true;             // 복구
    }

    // 서버 통신이 끝날 때까지 기다렸다가 UI를 새로고침하는 코루틴
    private IEnumerator SyncNicknameAndRefresh(string newName)
    {
        // 오프라인 시 닉네임 동기화 대기 취소 및 알림
        if (RankingManager.Instance != null && !RankingManager.Instance.IsNetworkReached())
        {
            TriggerOfflineFallback();
            yield break;
        }

        // 서버와 닉네임 동기화를 시작하고, 끝날 때까지 대기
        if (RankingManager.Instance != null)
        {
            yield return StartCoroutine(RankingManager.Instance.UpdateNickname(newName));
        }

        // 최신 랭킹 리스트와 내 정보를 서버에서 다시 가져옴
        RefreshRanking();
    }

    // --- 애니메이션 범용 함수 ---

    /// <summary>
    /// UI 객체의 크기를 지정한 시간 동안 부드럽게 변경하는 공용 애니메이션 코루틴
    /// </summary>
    /// <param name="target">크기를 변경할 대상 RectTransform</param>
    /// <param name="start">시작 크기</param>
    /// <param name="end">목표 크기</param>
    /// <param name="duration">애니메이션 진행 시간 (초)</param>
    /// <param name="curve">크기 변화 속도를 조절할 커브</param>
    /// <param name="onComplete">애니메이션이 완전히 끝난 후 실행할 콜백 함수</param>
    /// <returns></returns>
    private IEnumerator AnimateScale(RectTransform target, Vector3 start, Vector3 end, float duration, AnimationCurve curve = null, Action onComplete = null)
    {
        isAnimating = true;
        float elapsed = 0f;
        target.localScale = start;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;

            float evalT = curve != null ? curve.Evaluate(t) : (1f - Mathf.Pow(1f - t, 3f));
            target.localScale = Vector3.LerpUnclamped(start, end, evalT);
            yield return null;
        }

        target.localScale = end;
        isAnimating = false;

        onComplete?.Invoke();
    }

    // --- 키보드 UX 및 커서 이동 로직 ---

    /// <summary>
    /// 닉네임 입력창이 선택되었을 때 호출.
    /// 모바일 가상 키보드가 화면을 가리지 않도록 팝업을 위로 올리고, 커서를 맨 끝으로 이동.
    /// </summary>
    /// <param name="text"></param>
    private void OnInputSelect(string text)
    {
        // 입력창 터치 시 위로 이동
        MovePopupY(keyboardOffsetY);
        StartCoroutine(MoveCursorToEndRoutine());
    }

    /// <summary>
    /// 닉네임 입력이 끝났을 때 호출.
    /// 위로 올라갔던 팝업을 원래 자리로 복구.
    /// </summary>
    /// <param name="text"></param>
    private void OnInputDeselect(string text)
    {
        // 입력 종료 시 원래 자리로 복구
        MovePopupY(0f);
    }

    /// <summary>
    /// 입력창 포커스 시, 기존 텍스트의 맨 끝으로 커서를 자연스럽게 이동시키는 코루틴.
    /// </summary>
    /// <returns></returns>
    private IEnumerator MoveCursorToEndRoutine()
    {
        yield return new WaitForEndOfFrame();
        nameInput.MoveTextEnd(false);
    }

    /// <summary>
    /// 팝업 창의 Y 좌표를 목표 위치로 이동시키도록 코루틴 실행.
    /// </summary>
    /// <param name="targetY">이동할 목표 Y 좌표</param>
    private void MovePopupY(float targetY)
    {
        if (editPopupRect == null || !editPopupRoot.activeSelf)
        {
            return;
        }

        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
        }
        moveCoroutine = StartCoroutine(CoMovePopupY(targetY));
    }

    /// <summary>
    /// 팝업 창을 목표 Y 좌표까지 부드럽게 이동시키는 코루틴.
    /// </summary>
    /// <param name="targetY"></param>
    /// <returns></returns>
    private IEnumerator CoMovePopupY(float targetY)
    {
        float elapsed = 0f;
        Vector2 startPos = editPopupRect.anchoredPosition;
        Vector2 endPos = new Vector2(startPos.x, targetY);

        while (elapsed < moveAnimDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / moveAnimDuration;

            // 스케일 애니메이션과 동일한 자연스러운 감속(Ease Out Cubic) 적용
            float easeT = 1f - Mathf.Pow(1f - t, 3f);

            editPopupRect.anchoredPosition = Vector2.Lerp(startPos, endPos, easeT);
            yield return null;
        }

        editPopupRect.anchoredPosition = endPos;
    }
}