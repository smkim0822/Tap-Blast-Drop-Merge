using UnityEngine;
using UnityEngine.EventSystems;

public class AutoSliderSpawner : MonoBehaviour
{
    public GameObject bombPrefab;   // 폭탄 프리팹
    public Color[] grainColors;     // 폭탄 색상
    public float moveSpeed = 3f;    // 이동 속도
    public float xRange = 2.5f;     // 좌우 이동 범위 (중앙 기준)

    private SpriteRenderer myRenderer;
    private Color nextColor;

    void Start()
    {
        myRenderer = GetComponent<SpriteRenderer>();
        PrepareNextBomb();
    }

    void Update()
    {
        // 좌우 왕복 이동
        float xPos = Mathf.PingPong(Time.time * moveSpeed, xRange * 2) - xRange;
        transform.position = new Vector3(xPos, transform.position.y, 0);

        // 탭하면 떨구기
        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current.IsPointerOverGameObject()) return;
            
            DropBomb();
        }
    }

    void DropBomb()
    {
        GameObject newBomb = Instantiate(bombPrefab, transform.position, Quaternion.identity);

        SpriteRenderer sr = newBomb.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = nextColor;
        }

        PrepareNextBomb();
    }

    void PrepareNextBomb()
    {
        if (grainColors.Length > 0)
        {
            nextColor = grainColors[Random.Range(0, grainColors.Length)];

            if (myRenderer != null)
            {
                myRenderer.color = nextColor;
            }
        }
    }
}
