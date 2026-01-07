using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbyManager : MonoBehaviour
{
    public void LoadPendulum()
    {
        SceneManager.LoadScene("Game_PendulumControl");
    }

    public void LoadSlider()
    {
        SceneManager.LoadScene("Game_SliderControl");
    }

    public void LoadDrag()
    {
        SceneManager.LoadScene("Game_DragControl");
    }
}