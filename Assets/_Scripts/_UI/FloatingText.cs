using UnityEngine;
using TMPro;

/// <summary>
/// 점수 획득 시 나타나는 플로팅 텍스트 스크립트.
/// 위로 떠오르며 커졌다가 서서히 투명해지는 연출을 수행하고, 완료 시 오브젝트 풀로 반환.
/// </summary>
public class FloatingText : MonoBehaviour
{
    [Header("Movement Settings (이동 설정)")]
    [Tooltip("텍스트가 위로 떠오르는 속도")]
    public float moveSpeed = 2f;
    [Tooltip("텍스트가 화면에 머무르다가 사라지는 총 시간 (초)")]
    public float destroyTime = 1f;
    [Tooltip("텍스트가 생성될 때 타겟의 중심지에서 얼마나 떨어져서 시작할지 보정하는 값")]
    public Vector3 offset = new Vector3(0, 0.5f, 0);

    [Header("Pop Animation Settings (팝업 연출 설정)")]
    [Tooltip("텍스트가 나타날 때 순간적으로 팽창할 최대 크기 비율")]
    public float popScale = 1.5f;
    [Tooltip("텍스트가 팽창했다가 원래 크기로 돌아오는 연출의 속도")]
    public float popSpeed = 5f;

    private TextMeshPro textMesh;
    private Color textColor;
    private Vector3 initialScale;
    private float timer = 0f;

    void Awake()
    {
        textMesh = GetComponent<TextMeshPro>();
        if (textMesh == null)
        {
            textMesh = GetComponentInChildren<TextMeshPro>();
        }
        
        if (textMesh != null)
        {
            textColor = textMesh.color;
        }
        initialScale = transform.localScale;
    }

    void OnEnable()
    {
        timer = 0;

        // 시작 위치 보정 (공 바로 위)
        transform.localPosition += offset;

        // 초기 크기를 0으로 시작
        transform.localScale = Vector3.zero;

        if (textMesh != null)
        {
            textColor = textMesh.color;
            textColor.a = 1f;
            textMesh.color = textColor;
        }
    }

    void Update()
    {
        timer += Time.deltaTime;

        // 위로 이동
        transform.Translate(Vector3.up * moveSpeed * Time.deltaTime);

        // 팝업 애니메이션
        float scaleProgress = timer * popSpeed;
        if (scaleProgress <= 1f)
        {
            transform.localScale = Vector3.Lerp(Vector3.zero, initialScale * popScale, scaleProgress);
        }
        else if (scaleProgress <= 2f)
        {
            transform.localScale = Vector3.Lerp(initialScale * popScale, initialScale, scaleProgress - 1f);
        }

        // 서서히 투명해짐
        if (timer > destroyTime * 0.5f)
        {
            float alpha = 1f - ((timer - destroyTime * 0.5f) / (destroyTime * 0.5f));
            textColor.a = alpha;
            if (textMesh != null) textMesh.color = textColor;
        }

        if (timer >= destroyTime)
        {
            PoolService.Despawn(gameObject);
        }        
    }

    /// <summary>
    /// 플로팅 텍스트를 스폰할 때, 표시할 내용과 색상을 외부에서 주입하는 메서드.
    /// </summary>
    /// <param name="text">화면에 띄울 문자열</param>
    /// <param name="color">텍스트의 색상</param>
    public void SetText(string text, Color color)
    {
        if (textMesh != null)
        {
            textMesh.text = text;
            textMesh.color = color;
            textColor = color;
        }
    }
}