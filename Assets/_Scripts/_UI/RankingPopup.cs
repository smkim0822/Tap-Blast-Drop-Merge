using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System;

public class RankingPopup : MonoBehaviour
{
    [Header("UI References")]
    public Transform contentParent;
    public RankingItem itemPrefab;
    public GameObject offlineMessageObject;

    [Header("My Rank References")]
    public TextMeshProUGUI myNameText;
    public TextMeshProUGUI myRankText;
    public TextMeshProUGUI myScoreText;

    [Header("Nickname Edit UI")]
    public Button editNameButton;       // 연필 모양 혹은 수정 버튼
    public GameObject editPopupRoot;    // 수정할 때 뜨는 작은 창(패널)
    public Transform editPopupContent;  // 실제 창 내용물 (애니메이션 적용 대상)
    public TMP_InputField nameInput;    // 새로운 이름을 입력할 곳
    public Button saveNameButton;       // 저장 버튼
    public Button cancelNameButton;     // 취소 버튼
    public float editPopupAnimDuration = 0.2f;  // 에딧 팝업 애니메이션 속도

    [Header("Animation Settings")]
    [SerializeField] private RectTransform popupWindow;
    [SerializeField] private AnimationCurve openCurve;
    [SerializeField] private float animDuration = 0.2f;

    [Header("Keyboard UX Settings")]
    public float keyboardOffsetY = 400f;    // 키보드가 올라올 때 위로 이동할 Y값
    public float moveAnimDuration = 0.2f;   // 이동 애니메이션 속도

    private bool isAnimating = false;
    private bool isEditPopupClosing = false;
    private RectTransform editPopupRect;
    private Coroutine moveCoroutine;
    private Coroutine editAnimCoroutine;
    private Coroutine syncRoutine;

    private void Start()
    {
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
            editPopupRoot.SetActive(false);
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

    public void ClosePopup()
    {
        if (isAnimating)
        {
            return;
        }
        StartCoroutine(AnimateScale(popupWindow, popupWindow.localScale, Vector3.zero, animDuration, null, () => gameObject.SetActive(false)));
    }

    // 오프라인 강제 전환 함수
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
            syncRoutine = StartCoroutine(SyncAndLoadRankingRoutine());
        }
    }

    // 오프라인 데이터를 서버에 동기화한 뒤 랭킹을 로드하는 코루틴
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

        UpdateMyRankUI(serverMyData, localName, localHighScore);

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
        if (saveNameButton != null)
        {
            yield break;
        }
        saveNameButton.interactable = false;    // 즉시 비활성화
        yield return new WaitForSecondsRealtime(3f);    // 3초 대기
        saveNameButton.interactable = true;    // 복구
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

    // 애니메이션 로직
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
    private void OnInputSelect(string text)
    {
        // 입력창 터치 시 위로 이동
        MovePopupY(keyboardOffsetY);
        StartCoroutine(MoveCursorToEndRoutine());
    }

    private void OnInputDeselect(string text)
    {
        // 입력 종료 시 원래 자리로 복구
        MovePopupY(0f);
    }

    private IEnumerator MoveCursorToEndRoutine()
    {
        yield return new WaitForEndOfFrame();
        nameInput.MoveTextEnd(false);
    }

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