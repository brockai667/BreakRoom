using UnityEngine;
using UnityEngine.SceneManagement;

/// Tlačidlá hlavného menu: spustenie hry, obchod, zbierka, ukončenie.
/// Volané priamo z UI (OnClick), nastavuje GameSession pred prechodom do Hubu.
public class MainMenu : MonoBehaviour
{
    /// Spustí normálny herný mód a otvorí Hub na záložke "Play".
    public void HratHru()
    {
        GameSession.Mode = GameSession.GameMode.Normal;
        GameSession.InitialHubTab = "Play";
        SceneManager.LoadScene("Hub");
    }

    /// Otvorí Hub rovno na záložke "Shop".
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
