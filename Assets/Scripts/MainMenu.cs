using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void HratHru()
    {
        GameSession.InitialHubTab = "Play";
        SceneManager.LoadScene("Hub");
    }

    public void OpenShop()
    {
        GameSession.InitialHubTab = "Shop";
        SceneManager.LoadScene("Hub");
    }

    public void OpenCollection()
    {
        SceneManager.LoadScene("Collection");
    }

    public void KoniecHry()
    {
        Application.Quit();
    }
}
