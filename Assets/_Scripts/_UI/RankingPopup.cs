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

    private Coroutine moveCoroutine;
    private RectTransform editPopupRect;

    private bool isAnimating = false;
    private Coroutine editAnimCoroutine;

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

        // 모달 UI 생명주기 초기화
        // 랭킹 팝업이 활성화될 때 닉네임 팝업을 명시적으로 닫음
        CloseEditNamePopup();
        
        if (!isAnimating)
        {
            RefreshRanking();
        }
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
        StartCoroutine(AnimateOpen());
    }

    public void ClosePopup()
    {
        if (isAnimating) return;
        StartCoroutine(AnimateClose());
    }

    public void RefreshRanking()
    {
        if (RankingManager.Instance != null)
        {
            // 1. 전체 리스트 (Top 50) 호출
            StartCoroutine(RankingManager.Instance.GetRanks(OnDataReceived));
            
            // 2. 내 랭킹 단건 호출
            StartCoroutine(RankingManager.Instance.GetMyRank(OnMyRankReceived));
        }
    }

    void OnDataReceived(ScoreData[] ranks)
    {
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }
        
        if (ranks == null || ranks.Length == 0) return;

        for (int i = 0; i < ranks.Length; i++)
        {
            int rank = i + 1;
            string playerName = ranks[i].name;
            int score = ranks[i].score;

            RankingItem newItem = Instantiate(itemPrefab, contentParent);
            newItem.SetData(rank, playerName, score);
        }
    }

    void OnMyRankReceived(MyRankData myData)
    {
        if (myData == null)
        {
            // 기록이 아예 없는 유저
            string currentName = NicknameManager.Instance != null ? NicknameManager.Instance.GetNickname() : PlayerPrefs.GetString("PlayerName", "Guest");
            myNameText.text = currentName;
            myRankText.text = "-";
            myScoreText.text = "0";
        }
        else
        {
            // 정상적으로 받아온 유저
            myNameText.text = myData.name;
            myRankText.text = myData.rank.ToString();
            myScoreText.text = myData.score.ToString("N0");
        }
    }

    // --- 닉네임 수정 로직 ---

    private void OpenEditNamePopup()
    {
        if (editPopupRoot != null)
        {
            editPopupRoot.SetActive(true);

            // 위치 초기화
            if (editPopupRect != null)
            {
                editPopupRect.anchoredPosition = Vector2.zero;
            }

            // 입력창에 현재 닉네임을 미리 적어줌
            if (NicknameManager.Instance != null)
            {
                nameInput.text = NicknameManager.Instance.GetNickname();
            }

            // 애니메이션
            if (editAnimCoroutine != null)
            {
                StopCoroutine(editAnimCoroutine);
            }

            editAnimCoroutine = StartCoroutine(CoAnimateEditPopup(Vector3.zero, Vector3.one, null));
        }
    }

    public void CloseEditNamePopup()
    {
        // 이미 꺼져있으면 무시
        if (editPopupRoot == null || !editPopupRoot.activeSelf)
        {
            return;
        }

        // 애니메이션
        if (editAnimCoroutine != null)
        {
            StopCoroutine(editAnimCoroutine);
        }

        editAnimCoroutine = StartCoroutine(CoAnimateEditPopup(Vector3.one, Vector3.zero, () =>
        {
            editPopupRoot.SetActive(false);
        }));
    }

    private void SaveNewNickname()
    {
        if (nameInput == null || (saveNameButton != null && !saveNameButton.interactable)) return;

        string newName = nameInput.text;

        // 유효성 검사 (글자 수 제한)
        if (string.IsNullOrWhiteSpace(newName) || newName.Length > 10)
        {
            Debug.LogWarning($"⚠️ 닉네임 변경 실패: 길이가 유효하지 않습니다. (현재 길이: {newName.Length}자). API 호출을 차단합니다.");
            return;
        }

        // 버튼 쿨타임 코루틴 시작
        StartCoroutine(ButtonCooldownRoutine());

        // 매니저를 통해 저장 (매니저 내부에서 유효성 검사 수행함)
        if (NicknameManager.Instance != null)
        {
            NicknameManager.Instance.SetNickname(newName);
        }

        // UI 즉시 갱신 및 팝업 닫기
        CloseEditNamePopup();
        StartCoroutine(SyncNicknameAndRefresh(newName));
    }

    // 닉네임 변경 쿨타임 코루틴
    private IEnumerator ButtonCooldownRoutine()
    {
        if (saveNameButton != null)
        {
            saveNameButton.interactable = false;    // 즉시 비활성화
            yield return new WaitForSecondsRealtime(3f);    // 3초 대기
            saveNameButton.interactable = true;    // 복구
        }
    }

    // 서버 통신이 끝날 때까지 기다렸다가 UI를 새로고침하는 코루틴
    private IEnumerator SyncNicknameAndRefresh(string newName)
    {
        // 서버와 닉네임 동기화를 시작하고, 끝날 때까지 대기
        if (RankingManager.Instance != null)
        {
            yield return StartCoroutine(RankingManager.Instance.UpdateNickname(newName));
        }

        // 최신 랭킹 리스트와 내 정보를 서버에서 다시 가져옴
        RefreshRanking();
    }

    // --- 애니메이션 로직 ---

    // 닉네임 에딧 팝업 애니메이션 코루틴
    private IEnumerator CoAnimateEditPopup(Vector3 startScale, Vector3 endScale, Action onComplete)
    {
        if (editPopupContent == null)
        {
            yield break;
        }

        float elapsed = 0f;
        editPopupContent.localScale = startScale;

        while (elapsed < editPopupAnimDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / editPopupAnimDuration;

            // 자연스러운 감속(Ease Out Cubic)
            float easeT = 1f - Mathf.Pow(1f - t, 3f);

            editPopupContent.localScale = Vector3.Lerp(startScale, endScale, easeT);
            yield return null;
        }

        editPopupContent.localScale = endScale;
        onComplete?.Invoke();
    }

    IEnumerator AnimateOpen()
    {
        isAnimating = true;

        float timer = 0f;
        popupWindow.localScale = Vector3.zero;

        while (timer < animDuration)
        {
            timer += Time.unscaledDeltaTime;
            float progress = timer / animDuration;
            float curveValue = openCurve.Evaluate(progress);

            popupWindow.localScale = new Vector3(curveValue, curveValue, 1f);
            yield return null;
        }

        popupWindow.localScale = Vector3.one;
        isAnimating = false;
    }

    IEnumerator AnimateClose()
    {
        isAnimating = true;

        float timer = 0f;
        Vector3 startScale = popupWindow.localScale;

        while (timer < animDuration)
        {
            timer += Time.unscaledDeltaTime;
            float progress = timer / animDuration;

            popupWindow.localScale = Vector3.Lerp(startScale, Vector3.zero, progress);
            yield return null;
        }

        popupWindow.localScale = Vector3.zero;
        isAnimating = false;

        gameObject.SetActive(false);
    }

    // --- 키보드 위치 이동 로직 ---

    private void OnInputSelect(string text)
    {
        // 입력창 터치 시 위로 이동
        MovePopupY(keyboardOffsetY);
    }

    private void OnInputDeselect(string text)
    {
        // 입력 종료 시 원래 자리로 복구
        MovePopupY(0f);
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