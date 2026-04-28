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

    [Header("Guide Line (표시선 설정)")]
    [SerializeField] LineRenderer guideLine;
    [SerializeField] float groundY = -4.5f;
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

        Vector3 endPos = new Vector3(currentPos.x, groundY, 0f);
        guideLine.SetPosition(0, currentPos);
        guideLine.SetPosition(1, endPos);

        float distance = Vector3.Distance(currentPos, endPos);
        guideLine.material.mainTextureScale = new Vector2(distance * lineTextureScale, 1f);

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
            targetUI.sprite = nextElements[i].sprite;

            bool isNextBomb = (i == nextElements.Count - 1);
            Color uiColor = Color.white;
            uiColor.a = isNextBomb ? 1.0f : 0.5f;
            targetUI.color = uiColor;

            float targetScale = 0.8f;
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
