using System.Linq;
using NUnit.Framework;

/// Čisté testy dátového zoznamu zbraní - žiadna scéna, žiadny PlayerPrefs.
public class WeaponDataTests
{
    [Test]
    public void All_MaNajmenejJednuZbran()
    {
        Assert.IsNotNull(WeaponData.All);
        Assert.Greater(WeaponData.All.Length, 0);
    }

    [Test]
    public void All_ZiadneDuplicitneId()
    {
        var ids = WeaponData.All.Select(w => w.id).ToList();
        var unikatne = ids.Distinct().ToList();
        Assert.AreEqual(ids.Count, unikatne.Count, "WeaponData.All obsahuje duplicitné id: "
            + string.Join(", ", ids.GroupBy(x => x).Where(g => g.Count() > 1).Select(g => g.Key)));
    }

    [Test]
    public void All_KazdaZbranMaVyplneneZakladnePolia()
    {
        foreach (var w in WeaponData.All)
        {
            Assert.IsFalse(string.IsNullOrWhiteSpace(w.id), "zbraň s prázdnym id");
            Assert.IsFalse(string.IsNullOrWhiteSpace(w.displayName), $"'{w.id}' nemá displayName");
            Assert.GreaterOrEqual(w.price, 0, $"'{w.id}' má zápornú cenu");
            Assert.Greater(w.damage, 0, $"'{w.id}' má damage <= 0");
            Assert.GreaterOrEqual(w.splashRadius, 0f, $"'{w.id}' má záporný splashRadius");
            Assert.Greater(w.hitDistance, 0f, $"'{w.id}' má hitDistance <= 0");
            Assert.Greater(w.swingSpeed, 0f, $"'{w.id}' má swingSpeed <= 0");
        }
    }

    [Test]
    public void All_ObsahujeFistsAkoBezplatnyZaklad()
    {
        var fists = WeaponData.All.FirstOrDefault(w => w.id == "fists");
        Assert.IsNotNull(fists, "chýba základná zbraň 'fists'");
        Assert.AreEqual(0, fists.price, "'fists' by mala byť zadarmo");
    }

    [Test]
    public void Get_ExistujuceId_VratiSpravnuZbran()
    {
        var bat = WeaponData.Get("bat");
        Assert.AreEqual("bat", bat.id);
        Assert.AreEqual("Baseball Bat", bat.displayName);
    }

    [Test]
    public void Get_NeexistujuceId_VratiFallbackFists()
    {
        var w = WeaponData.Get("neexistuje-xyz-123");
        Assert.AreEqual(WeaponData.All[0].id, w.id, "fallback pri neznámom id má byť All[0] (fists)");
    }

    [Test]
    public void UnlockLevel_FistsABat_SuOdomkuteOdLevelu1()
    {
        Assert.AreEqual(1, WeaponData.UnlockLevel("fists"));
        Assert.AreEqual(1, WeaponData.UnlockLevel("bat"));
    }

    [Test]
    public void UnlockLevel_NeexistujuceId_VratiDefault1()
    {
        Assert.AreEqual(1, WeaponData.UnlockLevel("neexistuje-xyz-123"));
    }

    [Test]
    public void UnlockLevel_JeMonotonneRastuciSCenou_PreDrahsieZbraneNeklesa()
    {
        // Nie je to prisny 1:1 vztah (dizajnove rozhodnutie), ale extremne draha zbran
        // by nemala mat nizsi unlock level ako lacna - hruba kontrola konzistencie.
        var flamethrower = WeaponData.Get("flamethrower");
        var fists = WeaponData.Get("fists");
        Assert.Greater(WeaponData.UnlockLevel(flamethrower.id), WeaponData.UnlockLevel(fists.id));
    }
}
