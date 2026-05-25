using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void HratHru()
    {
        SceneManager.LoadScene("SampleScene");
    }

    public void KoniecHry()
    {
        Application.Quit();
    }
}