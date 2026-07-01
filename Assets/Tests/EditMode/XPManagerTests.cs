using NUnit.Framework;
using UnityEngine;

/// Testy XP krivky a levelovania. XPManager.Awake() nevolá DontDestroyOnLoad
/// (na rozdiel od PlayerInventory), takže sa dá bezpečne vytvárať aj rušiť
/// v Edit Mode bez ďalších úprav.
public class XPManagerTests
{
    XPManager xpm;
    PlayerPrefsSnapshot prefs;

    [SetUp]
    public void SetUp()
    {
        prefs = new PlayerPrefsSnapshot();
        prefs.SetInt("Level", 1);
        prefs.SetInt("XP", 0);

        xpm = new GameObject("TestXPManager").AddComponent<XPManager>();
    }

    [TearDown]
    public void TearDown()
    {
        if (xpm != null) Object.DestroyImmediate(xpm.gameObject);
        prefs.RestoreAsInt("Level");
        prefs.RestoreAsInt("XP");
    }

    [Test]
    public void CerstvaInstancia_MaDefaultLevel1AXp0()
    {
        Assert.AreEqual(1, xpm.currentLevel);
        Assert.AreEqual(0, xpm.currentXP);
    }

    [TestCase(0, 0)]
    [TestCase(1, 0)]
    [TestCase(2, 80)]     // 2*2*20
    [TestCase(3, 260)]    // + 3*3*20 = 80+180
    public void XPForLevel_SucetKvadratickejKrivky(int level, int expectedTotal)
    {
        Assert.AreEqual(expectedTotal, xpm.XPForLevel(level));
    }

    [Test]
    public void AddXP_PodLevelUp_LenPripocitaXp()
    {
        xpm.AddXP(50);

        Assert.AreEqual(1, xpm.currentLevel);
        Assert.AreEqual(50, xpm.currentXP);
        Assert.AreEqual(50, PlayerPrefs.GetInt("XP"));
        Assert.AreEqual(1, PlayerPrefs.GetInt("Level"));
    }

    [Test]
    public void AddXP_PresneNaHranici_PosunieLevel()
    {
        xpm.AddXP(80);   // presne XPForLevel(2)

        Assert.AreEqual(2, xpm.currentLevel);
        Assert.AreEqual(80, xpm.currentXP);
        Assert.AreEqual(2, PlayerPrefs.GetInt("Level"));
    }

    [Test]
    public void AddXP_VelkyPrirastok_MozePreskocitViacLevelov()
    {
        xpm.AddXP(260);   // presne XPForLevel(3)

        Assert.AreEqual(3, xpm.currentLevel);
        Assert.AreEqual(260, xpm.currentXP);
    }

    [Test]
    public void XPProgress_VPolovicIntervalu_JePolovica()
    {
        xpm.AddXP(40);   // level 1: prev=0, next=80 -> 40/80 = 0.5
        Assert.AreEqual(0.5f, xpm.XPProgress(), 0.0001f);
    }

    [Test]
    public void XPProgress_NaMaxLeveli_JeVzdyJeden()
    {
        xpm.currentLevel = 100;
        Assert.AreEqual(1f, xpm.XPProgress(), 0.0001f);
    }

    [Test]
    public void SavedLevel_CitaPlayerPrefsPriamoAjBezInstancie()
    {
        prefs.SetInt("Level", 7);
        Assert.AreEqual(7, XPManager.SavedLevel);
    }
}
