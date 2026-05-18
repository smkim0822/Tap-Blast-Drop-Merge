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

    private static Sprite defaultUiSprite;

    private bool isAnimating = false;
    private bool isEditPopupClosing = false;
    private RectTransform editPopupRect;
    private Coroutine moveCoroutine;
    private Coroutine editAnimCoroutine;
    private Coroutine syncRoutine;
    private RankingBoardView boardView;
    private RankingSyncFlow rankingSyncFlow;
    private bool isInitialized = false;

    private void Awake()
    {
        EnsureInitialized();
    }

    private void Start()
    {
        EnsureInitialized();
    }

    private void EnsureInitialized()
    {
        if (isInitialized) return;

        boardView = GetComponent<RankingBoardView>();
        if (boardView == null)
        {
            boardView = gameObject.AddComponent<RankingBoardView>();
        }
        rankingSyncFlow = new RankingSyncFlow(this, boardView);

        ResolveEditNameButton();
        BindEditNameButton();

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
            ConfigureEditPopupLayout();
            editPopupRoot.SetActive(false);
        }

        isInitialized = true;
    }

    private void ConfigureEditPopupLayout()
    {
        RectTransform popupRootRect = editPopupRoot.GetComponent<RectTransform>();
        if (popupRootRect == null)
        {
            return;
        }

        if (editPopupRoot.transform.parent != transform)
        {
            editPopupRoot.transform.SetParent(transform, false);
        }

        StretchToParent(popupRootRect);

        Transform dimTransform = editPopupRoot.transform.Find("DimPanel");
        if (dimTransform != null && dimTransform.TryGetComponent(out Image dimImage) && dimImage.sprite == null)
        {
            dimImage.sprite = GetDefaultUiSprite();
        }
    }

    private static void StretchToParent(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.localScale = Vector3.one;
    }

    private static Sprite GetDefaultUiSprite()
    {
        if (defaultUiSprite == null)
        {
            defaultUiSprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/UISprite.psd");
        }

        return defaultUiSprite;
    }

    private void ResolveEditNameButton()
    {
        if (editNameButton != null)
        {
            return;
        }

        Button[] buttons = GetComponentsInChildren<Button>(true);
        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttons[i].gameObject.name == "EditNameButton")
            {
                editNameButton = buttons[i];
                return;
            }
        }
    }

    private void BindEditNameButton()
    {
        if (editNameButton == null)
        {
            return;
        }

        editNameButton.onClick.RemoveListener(OpenEditNamePopup);
        editNameButton.onClick.AddListener(OpenEditNamePopup);
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

        EnsureInitialized();
        BindEditNameButton();
        gameObject.SetActive(true);

        if (RankingManager.Instance != null)
        {
            StartCoroutine(RankingManager.Instance.CheckServerStatus());
        }
        
        if (boardView != null)
        {
            boardView.RefreshLocalMyRank();
            boardView.ResetScrollToTop();
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

    /// <summary>
    /// 랭킹 데이터를 서버에서 다시 불러와 UI 갱신.
    /// 네트워크가 끊겨있다면 오프라인 모드로 자동 전환.
    /// </summary>
    public void RefreshRanking()
    {
        EnsureInitialized();
        if (syncRoutine != null)
        {
            StopCoroutine(syncRoutine);
        }

        if (rankingSyncFlow == null)
        {
            return;
        }

        syncRoutine = rankingSyncFlow.RefreshRanking();
    }

    // --- 닉네임 수정 로직 ---

    public void OpenEditNamePopup()
    {
        if (editPopupRoot == null || editPopupContent == null)
        {
            return;
        }

        if (editPopupRoot.activeSelf && !isEditPopupClosing)
        {
            return;
        }

        EnsureInitialized();
        ConfigureEditPopupLayout();
        editPopupRoot.transform.SetAsLastSibling();
        editPopupRoot.SetActive(true);
        isEditPopupClosing = false;

        if (editPopupRect != null)
        {
            editPopupRect.anchoredPosition = Vector2.zero;
            editPopupRect.localScale = Vector3.one;
        }

        if (nameInput != null)
        {
            nameInput.text = NicknameManager.Instance != null
                ? NicknameManager.Instance.GetNickname()
                : string.Empty;
        }

        if (editAnimCoroutine != null)
        {
            StopCoroutine(editAnimCoroutine);
        }

        RectTransform contentRect = editPopupContent as RectTransform ?? editPopupContent.GetComponent<RectTransform>();
        if (contentRect == null)
        {
            return;
        }

        contentRect.localScale = Vector3.zero;
        editAnimCoroutine = StartCoroutine(AnimateEditPopupScale(contentRect, Vector3.zero, Vector3.one, editPopupAnimDuration));
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

        RectTransform contentRect = editPopupContent as RectTransform ?? editPopupContent.GetComponent<RectTransform>();
        if (contentRect == null)
        {
            editPopupRoot.SetActive(false);
            isEditPopupClosing = false;
            return;
        }

        editAnimCoroutine = StartCoroutine(AnimateEditPopupScale(contentRect, contentRect.localScale, Vector3.zero, editPopupAnimDuration, () =>
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
            if (boardView != null)
            {
                boardView.RefreshLocalMyRank();
            }

            CloseEditNamePopup();
            if (rankingSyncFlow != null)
            {
                rankingSyncFlow.SyncNicknameAndRefresh(newName);
            }
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
    private IEnumerator AnimateEditPopupScale(RectTransform target, Vector3 start, Vector3 end, float duration, Action onComplete = null)
    {
        float elapsed = 0f;
        target.localScale = start;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;
            float easeT = 1f - Mathf.Pow(1f - t, 3f);
            target.localScale = Vector3.LerpUnclamped(start, end, easeT);
            yield return null;
        }

        target.localScale = end;
        onComplete?.Invoke();
    }

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