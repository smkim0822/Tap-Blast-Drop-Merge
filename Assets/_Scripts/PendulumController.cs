using UnityEngine;
using UnityEngine.EventSystems;

public class PendulumController : MonoBehaviour
{
    public GameObject bombPrefab;   // 폭탄 프리팹
    public Transform spawnPoint;    // 폭탄이 매달릴 위치
    public float speed = 2.0f;      // 흔들리는 속도
    public float angle = 60.0f;     // 흔들리는 각도

    private GameObject currentVisualBomb;   // 매달려 있는 가짜 폭탄
    private Color nextColor;

    // 색상 목록 (빨, 파, 초)
    private Color[] colors = {Color.red, Color.blue, Color.green};

    void Start()
    {
        SpawnVisualBomb();
    }

    void Update()
    {
        // 1. 시계추 운동
        float zRotation = Mathf.Sin(Time.time * speed) * angle;
        transform.rotation = Quaternion.Euler(0, 0, zRotation);

        // 2. 탭하면 발사
        if (Input.GetMouseButtonDown(0)){
            // UI가 켜져있으면 무시
            if (EventSystem.current.IsPointerOverGameObject()) return;
            
            DropBomb();
        }
    }

    void SpawnVisualBomb(){
        // 이미 있으면 삭제 (안전장치)
        if (currentVisualBomb != null)  Destroy(currentVisualBomb);

        // 시각용 폭탄 생성 (Pivot의 자식이 되어 같이 움직임)
        currentVisualBomb = Instantiate(bombPrefab, spawnPoint.position, Quaternion.identity, spawnPoint);

        // 물리 기능 제거 (매달려 있을 때는 떨어지면 안 되니까)
        Destroy(currentVisualBomb.GetComponent<Rigidbody2D>());
        Destroy(currentVisualBomb.GetComponent<CircleCollider2D>());
        Destroy(currentVisualBomb.GetComponent<BombLogic>());  // 로직도 제거

        // 랜덤 색상 지정
        nextColor = colors[Random.Range(0, colors.Length)];
        currentVisualBomb.GetComponent<SpriteRenderer>().color = nextColor;
    }

    void DropBomb(){
        if (currentVisualBomb == null)  return;

        // 진짜 폭탄 생성 (물리 작용됨)
        GameObject droppedBomb = Instantiate(bombPrefab, spawnPoint.position, Quaternion.identity);
        droppedBomb.GetComponent<SpriteRenderer>().color = nextColor;

        // 매달린 가짜 삭제
        Destroy(currentVisualBomb);
        currentVisualBomb = null;

        // 잠시 후 새 폭탄 리로드 (0.5초 딜레이)
        Invoke("SpawnVisualBomb", 0.5f);
    }
}
