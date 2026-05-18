using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 스포너 본체 스프라이트, 다음 폭탄 UI, 가이드라인, 스케일 애니메이션을 담당합니다.
/// </summary>
[DisallowMultipleComponent]
public class SpawnerVisuals : MonoBehaviour
{
    [Header("UI References (UI 연결)")]
    [SerializeField] Image[] nextBombUIs;
    
    [Header("Next Bomb Preview (다음 폭탄 예고)")]
    [Tooltip("다음에 나올 폭탄(1번)을 강조할 때의 알파")]
    [SerializeField, Range(0f, 1f)] float nextBombFocusAlpha = 1.0f;
    [Tooltip("다다음/다다다음 폭탄(2~N번)을 표시할 때의 알파")]
    [SerializeField, Range(0f, 1f)] float nextBombDimAlpha = 0.25f;
    [Tooltip("다음에 나올 폭탄(1번) 스케일")]
    [SerializeField] float nextBombFocusScale = 0.75f;
    [Tooltip("다다음/다다다음 폭탄(2~N번) 스케일")]
    [SerializeField] float nextBombDimScale = 0.75f;

    [Header("Guide Line (표시선 설정)")]
    [SerializeField] LineRenderer guideLine;
    [Tooltip("표시선 끝점이 닿을 바닥 Y값(레이캐스트가 실패했을 때만 사용)")]
    [SerializeField] float groundY = -4.5f;
    [Tooltip("표시선 끝점을 고정값 대신 아래 방향 레이캐스트로 계산합니다.")]
    [SerializeField] bool useRaycastForGround = true;
    [Tooltip("바닥/벽 콜라이더가 포함된 레이어 마스크")]
    [SerializeField] LayerMask groundRaycastMask = ~0;
    [Tooltip("아래 방향 레이캐스트 최대 거리")]
    [SerializeField] float groundRaycastDistance = 50f;
    [Tooltip("바닥 접점에서 위/아래로 보정할 오프셋")]
    [SerializeField] float groundRaycastOffsetY = 0f;
    [Tooltip("레이캐스트가 실패하더라도 라인이 바닥을 '뚫지' 않도록 기준이 되는 바닥 콜라이더(권장: Bottom Wall)")]
    [SerializeField] Collider2D groundColliderOverride;
    [SerializeField] float lineTextureScale = 1f;

    [Header("Animation Settings (애니메이션 곡선 및 시간)")]
    [SerializeField] AnimationCurve spawnCurve;
    [SerializeField] float spawnDuration = 0.3f;
    [SerializeField] AnimationCurve spawnerLoadCurve;
    [SerializeField] float spawnerLoadDuration = 0.3f;
    [SerializeField] AnimationCurve uiPunchCurve;
    [SerializeField] float uiAnimDuration = 0.2f;

    SpriteRenderer spawnerRenderer;
    Vector3 originalScale;

    static readonly RaycastHit2D[] RaycastHitsBuffer = new RaycastHit2D[16];
    static MaterialPropertyBlock guideLinePropertyBlock;

    void Awake()
    {
        spawnerRenderer = GetComponent<SpriteRenderer>();
        if (guideLine == null)
        {
            guideLine = GetComponent<LineRenderer>();
        }

        originalScale = transform.localScale == Vector3.zero ? Vector3.one : transform.localScale;
        InitializeGuideLine();
    }

    public int PreviewCount => nextBombUIs != null ? nextBombUIs.Length : 0;

    public void SetGuideEnabled(bool enabled)
    {
        if (guideLine != null)
        {
            guideLine.enabled = enabled;
        }
    }

    public bool IsGuideEnabled()
    {
        return guideLine != null && guideLine.enabled;
    }

    public void DrawGuideLine(Vector3 currentPos, Color guideColor)
    {
        if (guideLine == null) return;

        float endY = groundY;
        // 바닥 콜라이더가 지정되어 있으면, 바닥 상단 Y를 하한으로 사용
        // (레이캐스트가 실패하더라도 바닥 아래로 내려가지 않게)
        float minGroundY = float.NegativeInfinity;
        if (groundColliderOverride != null)
        {
            minGroundY = groundColliderOverride.bounds.max.y;
        }
        if (useRaycastForGround)
        {
            var filter = new ContactFilter2D
            {
                useLayerMask = true,
                layerMask = groundRaycastMask,
                useTriggers = false,
            };

            // 여러 콜라이더(공/폭탄/이펙트) 위를 지나칠 수 있으니,
            // Dynamic Rigidbody2D는 스킵하고 "벽/바닥(Static 또는 Rigidbody 없음)"을 첫 히트로 사용.
            int hitCount = Physics2D.Raycast(
                currentPos,
                Vector2.down,
                filter,
                RaycastHitsBuffer,
                groundRaycastDistance);
            for (int i = 0; i < hitCount; i++)
            {
                Collider2D col = RaycastHitsBuffer[i].collider;
                if (col == null) continue;

                Rigidbody2D rb = col.attachedRigidbody;
                if (rb != null && rb.bodyType == RigidbodyType2D.Dynamic)
                {
                    continue; // 공/폭탄 등 동적 오브젝트 무시
                }

                endY = RaycastHitsBuffer[i].point.y + groundRaycastOffsetY;
                break;
            }
        }

        if (minGroundY > float.NegativeInfinity)
        {
            endY = Mathf.Max(endY, minGroundY + groundRaycastOffsetY);
        }

        Vector3 endPos = new Vector3(currentPos.x, endY, 0f);
        guideLine.SetPosition(0, currentPos);
        guideLine.SetPosition(1, endPos);

        float distance = Vector3.Distance(currentPos, endPos);
        if (guideLinePropertyBlock == null)
        {
            guideLinePropertyBlock = new MaterialPropertyBlock();
        }

        guideLine.GetPropertyBlock(guideLinePropertyBlock);
        guideLinePropertyBlock.SetVector("_MainTex_ST", new Vector4(distance * lineTextureScale, 1f, 0f, 0f));
        guideLine.SetPropertyBlock(guideLinePropertyBlock);

        guideLine.startColor = guideColor;
        guideColor.a = 0.3f;
        guideLine.endColor = guideColor;
    }

    public void PlaySpawnAnimation(Transform spawnedBomb)
    {
        if (spawnedBomb == null) return;

        Vector3 realScale = spawnedBomb.localScale;
        spawnedBomb.localScale = Vector3.zero;
        StartCoroutine(AnimateScale(spawnedBomb, spawnCurve, spawnDuration, realScale));
    }

    public void RenderQueue(Spawner.ElementData currentElement, System.Collections.Generic.IReadOnlyList<Spawner.ElementData> nextElements, bool playAnim)
    {
        if (spawnerRenderer != null)
        {
            spawnerRenderer.sprite = currentElement.sprite;
            spawnerRenderer.color = Color.white;

            if (playAnim)
            {
                StartCoroutine(AnimateScale(transform, spawnerLoadCurve, spawnerLoadDuration, originalScale));
            }
            else
            {
                transform.localScale = originalScale;
            }
        }

        if (nextBombUIs == null) return;

        for (int i = 0; i < nextBombUIs.Length; i++)
        {
            if (i >= nextElements.Count) continue;

            Image targetUI = nextBombUIs[i];
            // SpawnerQueue는 "다음에 나올 원소"를 리스트의 마지막에 두도록 운용됩니다.
            // 따라서 UI의 1번 슬롯(가장 왼쪽)이 '다음 폭탄'이 되도록 역순으로 매핑합니다.
            int srcIndex = nextElements.Count - 1 - i;
            if (srcIndex < 0 || srcIndex >= nextElements.Count) continue;
            targetUI.sprite = nextElements[srcIndex].sprite;

            // UI 배열의 0번(가장 왼쪽/첫 슬롯)을 "다음 폭탄"으로 취급합니다.
            bool isNextBomb = (i == 0);
            Color uiColor = Color.white;
            uiColor.a = isNextBomb ? nextBombFocusAlpha : nextBombDimAlpha;
            targetUI.color = uiColor;

            float targetScale = isNextBomb ? nextBombFocusScale : nextBombDimScale;
            if (playAnim)
            {
                StartCoroutine(AnimateScale(targetUI.rectTransform, uiPunchCurve, uiAnimDuration, Vector3.one * targetScale));
            }
            else
            {
                targetUI.rectTransform.localScale = Vector3.one * targetScale;
            }
        }
    }

    public void SetSpawnerVisible(bool visible)
    {
        if (spawnerRenderer != null)
        {
            spawnerRenderer.enabled = visible;
        }
    }

    public void ResetVisualState()
    {
        StopAllCoroutines();
        transform.localScale = originalScale;
        SetSpawnerVisible(true);
        SetGuideEnabled(false);
    }

    void InitializeGuideLine()
    {
        if (guideLine == null) return;

        guideLine.positionCount = 2;
        guideLine.enabled = false;
        guideLine.textureMode = LineTextureMode.Tile;
        guideLine.sortingOrder = 5;
    }

    IEnumerator AnimateScale(Transform targetTransform, AnimationCurve curve, float duration, Vector3 finalScale)
    {
        if (targetTransform == null) yield break;

        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            float progress = timer / duration;

            if (targetTransform != null)
            {
                targetTransform.localScale = finalScale * curve.Evaluate(progress);
            }
            yield return null;
        }

        if (targetTransform != null)
        {
            targetTransform.localScale = finalScale;
        }
    }
}
