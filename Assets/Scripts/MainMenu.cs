using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void HratHru()
    {
        SceneManager.LoadScene("SampleScene");
    }

    public void OpenShop()
    {
        SceneManager.LoadScene("Shop");
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
