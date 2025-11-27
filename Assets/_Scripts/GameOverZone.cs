using UnityEngine;

public class GameOverZone : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        // "Stack"이 닿으면 게임 오버
        // 떨어지는 중인 폭탄은 무시
        if (other.CompareTag("Stack"))
        {
            Debug.Log("Game Over!");
            Time.timeScale = 0; // 시간 정지 (게임 멈춤)
        }
    }
}