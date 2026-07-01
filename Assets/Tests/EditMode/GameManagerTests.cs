using NUnit.Framework;
using UnityEngine;

/// Testy čistej logiky GameManager (combo, hodnotenie, odmeny) - vytvára si vlastnú
/// izolovanú inštanciu cez AddComponent a po teste ju DestroyImmediate-uje, nech
/// singleton Instance nezostane "visieť" medzi testami (Unity berie zničený objekt ako null).
public class GameManagerTests
{
    GameManager gm;
    PlayerPrefsSnapshot prefs;

    [SetUp]
    public void SetUp()
    {
        prefs = new PlayerPrefsSnapshot();
        // AwardBreak cita Perks.MoneyMult()/ComboWindowBonus() - vynulujeme, aby vysledky
        // neboli zavisle od realneho progresu hraca ulozeneho v tomto Unity projekte.
        prefs.SetInt("Perk_money", 0);
        prefs.SetInt("Perk_combo", 0);

        gm = new GameObject("TestGameManager").AddComponent<GameManager>();
    }

    [TearDown]
    public void TearDown()
    {
        if (gm != null) Object.DestroyImmediate(gm.gameObject);
        prefs.RestoreAllAsInt();
    }

    // ---------- ComboMultiplier ----------
    [TestCase(0, 1f)]
    [TestCase(2, 1f)]
    [TestCase(3, 1.25f)]
    [TestCase(5, 1.25f)]
    [TestCase(6, 1.5f)]
    [TestCase(9, 1.5f)]
    [TestCase(10, 2f)]
    [TestCase(15, 2f)]
    [TestCase(16, 2.5f)]
    [TestCase(24, 2.5f)]
    [TestCase(25, 3f)]
    [TestCase(100, 3f)]
    public void ComboMultiplier_HranicePodlaComboCount(int combo, float expected)
    {
        gm.comboCount = combo;
        Assert.AreEqual(expected, gm.ComboMultiplier(), 0.0001f);
    }

    // ---------- CalculateBonus ----------
    [TestCase(0, 0)]
    [TestCase(19, 0)]
    [TestCase(20, 10)]
    [TestCase(44, 10)]
    [TestCase(45, 25)]
    [TestCase(79, 25)]
    [TestCase(80, 50)]
    [TestCase(119, 50)]
    [TestCase(120, 80)]
    [TestCase(500, 80)]
    public void CalculateBonus_HranicePodlaDestroyedCount(int destroyed, int expectedBonus)
    {
        gm.destroyedCount = destroyed;
        Assert.AreEqual(expectedBonus, gm.CalculateBonus());
    }

    // ---------- ComputeGrade (cleared) ----------
    [TestCase(50f, "S")]
    [TestCase(90f, "S")]
    [TestCase(91f, "A")]
    [TestCase(160f, "A")]
    [TestCase(161f, "B")]
    [TestCase(500f, "B")]
    public void ComputeGrade_Cleared_HraniceCasu(float elapsed, string expectedGrade)
    {
        gm.elapsedTime = elapsed;
        Assert.AreEqual(expectedGrade, gm.ComputeGrade(true));
    }

    // ---------- ComputeGrade (nevycistene, podla destroyedCount) ----------
    [TestCase(150, "S")]
    [TestCase(200, "S")]
    [TestCase(149, "A")]
    [TestCase(90, "A")]
    [TestCase(89, "B")]
    [TestCase(50, "B")]
    [TestCase(49, "C")]
    [TestCase(20, "C")]
    [TestCase(19, "D")]
    [TestCase(0, "D")]
    public void ComputeGrade_Nevycistene_HraniceDestroyedCount(int destroyed, string expectedGrade)
    {
        gm.destroyedCount = destroyed;
        Assert.AreEqual(expectedGrade, gm.ComputeGrade(false));
    }

    // ---------- AwardBreak ----------
    [Test]
    public void AwardBreak_PrveZasiahnutie_PripiseZakladnuOdmenu()
    {
        int pay = gm.AwardBreak(10, 5, false, Vector3.zero);

        Assert.AreEqual(10, pay);
        Assert.AreEqual(10, gm.roundMoney);
        Assert.AreEqual(1, gm.destroyedCount);
        Assert.AreEqual(1, gm.comboCount);
    }

    [Test]
    public void AwardBreak_KomboZvysujeMultiplikatorNapriecViacerymiZasahmi()
    {
        // baseReward=8 zvolene tak, aby vsetky medzivysledky boli cele cisla (ziadne .5 zaokruhlovacie tie).
        int totalPay = 0;
        for (int i = 0; i < 6; i++)
            totalPay += gm.AwardBreak(8, 0, false, Vector3.zero);

        // combo 1-2: x1 (8+8) | combo 3-5: x1.25 (10+10+10) | combo 6: x1.5 (12)
        Assert.AreEqual(8 + 8 + 10 + 10 + 10 + 12, totalPay);
        Assert.AreEqual(totalPay, gm.roundMoney);
        Assert.AreEqual(6, gm.destroyedCount);
        Assert.AreEqual(6, gm.comboCount);
    }

    [Test]
    public void AwardBreak_GoldenStrojnasobiOdmenu()
    {
        int pay = gm.AwardBreak(8, 0, true, Vector3.zero);
        Assert.AreEqual(24, pay);   // combo=1 -> mult 1x, potom *3 za golden
        Assert.AreEqual(24, gm.roundMoney);
    }

    [Test]
    public void AwardBreak_KedRoundNieJeAktivny_NicNepripise()
    {
        gm.roundActive = false;
        int pay = gm.AwardBreak(10, 5, false, Vector3.zero);

        Assert.AreEqual(0, pay);
        Assert.AreEqual(0, gm.roundMoney);
        Assert.AreEqual(0, gm.destroyedCount);
        Assert.AreEqual(0, gm.comboCount);
    }
}
