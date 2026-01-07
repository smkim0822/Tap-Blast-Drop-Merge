using UnityEngine;
using UnityEngine.SceneManagement;

public class ReturnToLobby : MonoBehaviour
{
    public void GoLobby()
    {
        SceneManager.LoadScene("Lobby");
    }

    void Update()
    {
        // 안드로이드 뒤로가기 버튼 or PC ESC 버튼
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            GoLobby();
        }
    }
}
