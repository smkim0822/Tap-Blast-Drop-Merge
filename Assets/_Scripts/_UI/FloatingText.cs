using UnityEngine;
using TMPro;

public class FloatingText : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 2f;                        // 위로 올라가는 속도
    public float destroyTime = 1f;                      // 사라지는 시간
    public Vector3 offset = new Vector3(0, 0.5f, 0);    // 공보다 약간 위에서 시작

    [Header("Pop Animation Settings")]
    public float popScale = 1.5f;                       // 순간적으로 커질 최대 크기
    public float popSpeed = 5f;                         // 커지는 속도

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
        textColor = textMesh.color;
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
            PoolManager.Instance.Despawn(gameObject);
        }        
    }

    // 외부에서 텍스트 내용과 색상을 설정하는 함수
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