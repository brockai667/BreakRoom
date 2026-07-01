using NUnit.Framework;
using UnityEngine;

/// Testy PlayerInventory (kúpa/equip/upgrade, peniaze, PlayerPrefs kľúče, OnChanged).
/// "bat" (cena 150) sa používa ako testovacia zbraň - nie je to "fists", takže sa dá
/// otestovať aj vlastníctvo od nuly.
public class PlayerInventoryTests
{
    const string TEST_ID = "bat";   // WeaponData: price=150

    PlayerInventory inv;
    PlayerPrefsSnapshot prefs;

    [SetUp]
    public void SetUp()
    {
        prefs = new PlayerPrefsSnapshot();
        prefs.SetInt("Money", 0);
        prefs.SetString("Equipped", null);              // -> Load() pouzije default "fists"
        prefs.Delete("Own_" + TEST_ID);
        prefs.Delete("Up_" + TEST_ID);

        inv = new GameObject("TestPlayerInventory").AddComponent<PlayerInventory>();
    }

    [TearDown]
    public void TearDown()
    {
        if (inv != null) Object.DestroyImmediate(inv.gameObject);
        prefs.RestoreAsString("Equipped");
        prefs.RestoreAsInt("Money");
        prefs.RestoreAsInt("Own_" + TEST_ID);
        prefs.RestoreAsInt("Up_" + TEST_ID);
    }

    [Test]
    public void CerstvaInstancia_MaDefaultneHodnoty()
    {
        Assert.AreEqual(0, inv.Money);
        Assert.AreEqual("fists", inv.EquippedId);
        Assert.IsTrue(inv.Owns("fists"), "fists musia byt vzdy vlastnene");
        Assert.IsFalse(inv.Owns(TEST_ID));
    }

    [Test]
    public void AddMoney_PripocitaAUlozi()
    {
        inv.AddMoney(50);
        Assert.AreEqual(50, inv.Money);
        Assert.AreEqual(50, PlayerPrefs.GetInt("Money"));
    }

    [Test]
    public void AddMoney_NikdyNejdePodNulu()
    {
        inv.AddMoney(50);
        inv.AddMoney(-1000);
        Assert.AreEqual(0, inv.Money);
        Assert.AreEqual(0, PlayerPrefs.GetInt("Money"));
    }

    [Test]
    public void AddMoney_VyvolaOnChanged()
    {
        bool changed = false;
        inv.OnChanged += () => changed = true;
        inv.AddMoney(10);
        Assert.IsTrue(changed);
    }

    [Test]
    public void TryBuy_NedostatokPenazi_Zlyha()
    {
        bool ok = inv.TryBuy(TEST_ID);
        Assert.IsFalse(ok);
        Assert.AreEqual(0, inv.Money);
        Assert.IsFalse(inv.Owns(TEST_ID));
    }

    [Test]
    public void TryBuy_DostatokPenazi_Uspeje()
    {
        inv.AddMoney(200);
        bool ok = inv.TryBuy(TEST_ID);

        Assert.IsTrue(ok);
        Assert.AreEqual(50, inv.Money);   // 200 - 150
        Assert.IsTrue(inv.Owns(TEST_ID));
        Assert.AreEqual(1, PlayerPrefs.GetInt("Own_" + TEST_ID));
        Assert.AreEqual(50, PlayerPrefs.GetInt("Money"));
    }

    [Test]
    public void TryBuy_UzVlastnenuZbran_Zlyha()
    {
        inv.AddMoney(200);
        Assert.IsTrue(inv.TryBuy(TEST_ID));

        bool secondBuy = inv.TryBuy(TEST_ID);

        Assert.IsFalse(secondBuy);
        Assert.AreEqual(50, inv.Money, "druhy pokus o kupu uz vlastnenej zbrane nesmie strhnut peniaze");
    }

    [Test]
    public void Equip_NevlastnenuZbran_Ignoruje()
    {
        inv.Equip(TEST_ID);
        Assert.AreEqual("fists", inv.EquippedId);
    }

    [Test]
    public void Equip_VlastnenuZbran_NastaviAUlozi()
    {
        inv.AddMoney(200);
        inv.TryBuy(TEST_ID);

        inv.Equip(TEST_ID);

        Assert.AreEqual(TEST_ID, inv.EquippedId);
        Assert.AreEqual(TEST_ID, PlayerPrefs.GetString("Equipped"));
    }

    [Test]
    public void CanUpgrade_NevlastnenuZbran_JeFalse()
    {
        Assert.IsFalse(inv.CanUpgrade(TEST_ID));
    }

    [Test]
    public void UpgradeCost_PreCerstvoVlastnenuZbran_JeMaxOsemdesiatAleboTretinaCeny()
    {
        inv.AddMoney(200);
        inv.TryBuy(TEST_ID);

        // bat: price=150 -> max(80, 150/3=50) * (level0 + 1) = 80
        Assert.AreEqual(80, inv.UpgradeCost(TEST_ID));
    }

    [Test]
    public void TryUpgrade_NedostatokPenazi_Zlyha()
    {
        inv.AddMoney(200);
        inv.TryBuy(TEST_ID);   // Money = 50, cost upgradu = 80

        bool ok = inv.TryUpgrade(TEST_ID);

        Assert.IsFalse(ok);
        Assert.AreEqual(0, inv.UpgradeLevel(TEST_ID));
        Assert.AreEqual(50, inv.Money);
    }

    [Test]
    public void TryUpgrade_DostatokPenazi_ZvysiLevelAOdpocitaCenu()
    {
        inv.AddMoney(200);
        inv.TryBuy(TEST_ID);      // Money = 50
        inv.AddMoney(80);        // Money = 130, cost = 80

        bool ok = inv.TryUpgrade(TEST_ID);

        Assert.IsTrue(ok);
        Assert.AreEqual(50, inv.Money);   // 130 - 80
        Assert.AreEqual(1, inv.UpgradeLevel(TEST_ID));
        Assert.AreEqual(1, PlayerPrefs.GetInt("Up_" + TEST_ID));
    }

    [Test]
    public void TryUpgrade_NedaSaIstNadMaxUpgrade()
    {
        inv.AddMoney(100000);
        inv.TryBuy(TEST_ID);

        for (int i = 0; i < PlayerInventory.MAX_UPGRADE; i++)
            Assert.IsTrue(inv.TryUpgrade(TEST_ID), $"upgrade #{i} mal uspiet");

        Assert.IsFalse(inv.CanUpgrade(TEST_ID));
        Assert.IsFalse(inv.TryUpgrade(TEST_ID), "upgrade nad MAX_UPGRADE nesmie prejst");
        Assert.AreEqual(PlayerInventory.MAX_UPGRADE, inv.UpgradeLevel(TEST_ID));
    }
}
