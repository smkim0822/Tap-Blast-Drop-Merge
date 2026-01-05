using UnityEngine;

public class GrainLogic : MonoBehaviour
{
    // 출신 번호 (BombLogic에서 할당)
    public int batchId = -1;
    // 오디오 파일 변수
    public AudioClip popSound;
    // 파티클 프리팹 변수
    public ParticleSystem explosionEffect;

    private SpriteRenderer myRenderer;

    void Start(){
        myRenderer = GetComponent<SpriteRenderer>();
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // 다른 알갱이와 닿았을 때
        GrainLogic otherGrain = collision.gameObject.GetComponent<GrainLogic>();

        if (otherGrain != null)
        {
            // 색상 비교
            if (IsColorMatch(myRenderer.color, otherGrain.GetComponent<SpriteRenderer>().color))
            {
                // 출신 비교
                if (this.batchId != otherGrain.batchId)
                {
                    // 둘 다 사라짐
                    DestroyGrain();
                    otherGrain.DestroyGrain();
                }
            }
        }
    }

    public void DestroyGrain(){
        // 사운드 재생
        if (popSound != null)
        {
            AudioSource.PlayClipAtPoint(popSound, transform.position, 1.0f);
        }

        // 진동
        Handheld.Vibrate();
        
        // 파티클 생성 및 색상 적용
        if (explosionEffect != null)
        {
            ParticleSystem ps = Instantiate(explosionEffect, transform.position, Quaternion.identity);

            var main = ps.main;

            main.startColor = myRenderer.color;
            Destroy(ps.gameObject, main.startLifetime.constantMax + 0.1f);
        }
        Destroy(gameObject);
    }

    bool IsColorMatch(Color c1, Color c2)
    {
        return (Mathf.Abs(c1.r - c2.r) + Mathf.Abs(c1.g - c2.g) + Mathf.Abs(c1.b - c2.b)) < 0.1f;
    }
}