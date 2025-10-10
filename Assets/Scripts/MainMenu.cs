using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void PlayGame()
    {
        SceneManager.LoadScene("SampleScene"); // o el nombre de tu escena principal
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
