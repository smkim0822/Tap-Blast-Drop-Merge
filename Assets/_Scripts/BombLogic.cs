using UnityEngine;

public class BombLogic : MonoBehaviour
{
    private bool isSettled = false;

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (isSettled)  return;

        // "Stack" 태그가 붙은 것(바닥 or 이미 쌓인 폭탄)과 닿았을 때
        if (collision.gameObject.CompareTag("Stack"))
        {
            SpriteRenderer myRenderer = GetComponent<SpriteRenderer>();
            SpriteRenderer otherRenderer = collision.gameObject.GetComponent<SpriteRenderer>();

            // 1. 색상 비교 (상대방도 색이 있고, 색이 같으면)
            if (otherRenderer != null && IsColorMatch(myRenderer.color, otherRenderer.color))
            {
                // 바닥(Floor)은 흰색(기본)이라 색이 달라도 안 터지게 예외 처리 필요할 수 있음
                // 일단 바닥이 흰색이면 무조건 쌓이게 됨 (OK)

                Debug.Log("펑! 터짐");
                Destroy(collision.gameObject);  // 아래 것 파괴
                Destroy(this.gameObject);       // 나 자신 파괴
            }
            // 2. 색이 다르면 쌓임
            else
            {
                StackBomb(collision);
            }
        }
    }

    bool IsColorMatch(Color c1, Color c2)
    {
        // 미세한 오차 무시하고 비교
        return c1 == c2;
    }

    void StackBomb(Collision2D collision)
    {
        isSettled = true;
        gameObject.tag = "Stack";   // 나도 이제 "Stack"이 됨

        // 물리 고정 (안 움직이게)
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;

        // 부모 설정 (Hierarchy 정리용)
        transform.SetParent(collision.transform.parent);
    }
}
