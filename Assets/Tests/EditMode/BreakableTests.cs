using NUnit.Framework;

/// Testy čistej časti Breakable.Configure() (Breakable.ComputeStatsForSize) - žiadny
/// GameObject/Collider/scéna potrebné, je to čistá funkcia (maxDim, jackpot) -> hodnoty.
public class BreakableTests
{
    [Test]
    public void MalyObjekt_NedeliSaNaChunky_ZakladneHodnoty()
    {
        Breakable.ComputeStatsForSize(0.4f, false,
            out int subdivideStages, out int childPieces, out int hp, out int reward, out int xpValue);

        Assert.AreEqual(0, subdivideStages);
        Assert.AreEqual(0, childPieces);
        Assert.AreEqual(1, hp);
        Assert.AreEqual(2, reward);
        Assert.AreEqual(10, xpValue);
    }

    [Test]
    public void StrednyObjekt_JedenStupenDelenia()
    {
        Breakable.ComputeStatsForSize(1.0f, false,
            out int subdivideStages, out int childPieces, out int hp, out int reward, out int xpValue);

        Assert.AreEqual(1, subdivideStages);
        Assert.AreEqual(3, childPieces);
        Assert.AreEqual(3, hp);
        Assert.AreEqual(3, reward);
        Assert.AreEqual(16, xpValue);
    }

    [Test]
    public void VelkyObjekt_DvaStupneDelenia()
    {
        Breakable.ComputeStatsForSize(2.0f, false,
            out int subdivideStages, out int childPieces, out int hp, out int reward, out int xpValue);

        Assert.AreEqual(2, subdivideStages);
        Assert.AreEqual(4, childPieces);
        Assert.AreEqual(6, hp);
        Assert.AreEqual(5, reward);
        Assert.AreEqual(26, xpValue);
    }

    [Test]
    public void HranicaPresne07_UzSpadaDoStredneejKategorie()
    {
        Breakable.ComputeStatsForSize(0.7f, false,
            out int subdivideStages, out int childPieces, out _, out _, out _);

        Assert.AreEqual(1, subdivideStages);
        Assert.AreEqual(3, childPieces);
    }

    [Test]
    public void HranicaTesnePod07_EsteMalaKategoria()
    {
        Breakable.ComputeStatsForSize(0.69f, false,
            out int subdivideStages, out int childPieces, out _, out _, out _);

        Assert.AreEqual(0, subdivideStages);
        Assert.AreEqual(0, childPieces);
    }

    [Test]
    public void HranicaPresne13_UzSpadaDoVelkejKategorie()
    {
        Breakable.ComputeStatsForSize(1.3f, false,
            out int subdivideStages, out int childPieces, out _, out _, out _);

        Assert.AreEqual(2, subdivideStages);
        Assert.AreEqual(4, childPieces);
    }

    [Test]
    public void HranicaTesnePod13_EsteStrednaKategoria()
    {
        Breakable.ComputeStatsForSize(1.29f, false,
            out int subdivideStages, out int childPieces, out _, out _, out _);

        Assert.AreEqual(1, subdivideStages);
        Assert.AreEqual(3, childPieces);
    }

    [Test]
    public void Jackpot_NaMalomObjekte_UplatniVsetkyFloory()
    {
        Breakable.ComputeStatsForSize(0.4f, true,
            out int subdivideStages, out int childPieces, out int hp, out int reward, out int xpValue);

        // bez jackpotu by bolo hp=1,reward=2,xpValue=10,subdivideStages=0,childPieces=0 (viz test vyssie)
        Assert.AreEqual(8, hp, "hp floor je max(8, hp*6)");
        Assert.AreEqual(40, reward, "reward floor je max(40, reward*8)");
        Assert.AreEqual(30, xpValue, "xpValue je vzdy *3, ziadny floor");
        Assert.AreEqual(1, subdivideStages, "jackpot vynuti aspon 1 stupen delenia");
        Assert.AreEqual(4, childPieces, "jackpot vynuti aspon 4 kusky");
    }

    [Test]
    public void Jackpot_NaStrednomObjekte_NasobiBezPotrebyFloora()
    {
        Breakable.ComputeStatsForSize(1.0f, true,
            out int subdivideStages, out int childPieces, out int hp, out int reward, out int xpValue);

        // bez jackpotu: hp=3,reward=3,xpValue=16,subdivideStages=1,childPieces=3
        Assert.AreEqual(18, hp);     // max(8, 3*6=18) -> uz nad floorom
        Assert.AreEqual(40, reward); // max(40, 3*8=24) -> floor stale plati
        Assert.AreEqual(48, xpValue);
        Assert.AreEqual(1, subdivideStages);
        Assert.AreEqual(4, childPieces);   // max(3,4) -> bump z 3 na 4
    }
}
